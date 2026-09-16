using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Domain.Modules.Documents.Enums;

namespace WAssis.Application.Modules.Documents.Services;

public sealed class DocumentProcessingService(
    IImportedDocumentRepository repository,
    IDurableWorkQueue workQueue,
    IWorkerDataScope workerDataScope,
    IIsolatedDocumentProcessor isolatedProcessor,
    IAuditTrailWriter auditTrail) : IDocumentProcessingService
{
    public async Task<int> ProcessPendingBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        await workQueue.QuarantineExpiredAsync(cancellationToken);
        var processed = 0;
        for (var index = 0; index < Math.Clamp(batchSize, 1, 25); index++)
        {
            var lease = await workQueue.ClaimAsync("documents.parse", cancellationToken);
            if (lease is null) break;
            var storageKey = await workerDataScope.ExecuteAsync(
                lease.TenantId,
                lease.BranchId,
                async scopedCancellationToken =>
                {
                    var document = await repository.GetByIdAsync(lease.AggregateId, scopedCancellationToken);
                    if (document is null || document.TenantId != lease.TenantId
                        || document.OfficeBranchId != lease.BranchId
                        || document.Status != ImportedDocumentStatus.Received
                        || string.IsNullOrWhiteSpace(document.StoragePath))
                        return null;
                    document.MarkAsProcessing();
                    await repository.SaveChangesAsync(scopedCancellationToken);
                    return document.StoragePath;
                }, cancellationToken);
            if (storageKey is null)
            {
                await workQueue.MarkNeedsReviewAsync(lease, cancellationToken);
                continue;
            }

            await workQueue.RenewAsync(lease, cancellationToken);
            var result = await isolatedProcessor.ProcessAsync(storageKey, cancellationToken);
            await workerDataScope.ExecuteAsync(
                lease.TenantId,
                lease.BranchId,
                async scopedCancellationToken =>
                {
                    var document = await repository.GetByIdAsync(lease.AggregateId, scopedCancellationToken)
                        ?? throw new InvalidOperationException("Leased document became inaccessible before completion.");
                    if (result.Succeeded)
                    {
                        var parsed = result.Parsed!;
                        var notes = string.IsNullOrWhiteSpace(parsed.ParsingNotes)
                            ? $"parser_origin:{result.ParsingOrigin}"
                            : $"{parsed.ParsingNotes} parser_origin:{result.ParsingOrigin}";
                        document.MarkAsParsed(
                            parsed.DocumentType, parsed.ExtractedText, parsed.InsuranceCompanyName,
                            parsed.ProposalNumber, parsed.InsuredName, parsed.CoverageStartDateUtc,
                            parsed.CoverageEndDateUtc, parsed.TotalPremiumAmount, parsed.CommissionAmount,
                            parsed.ParsingConfidence, parsed.RequiresHumanReview, notes);
                    }
                    else
                    {
                        document.MarkAsFailed(result.ErrorCode ?? "document_processing_failed");
                    }
                    await workQueue.CompleteAsync(lease, scopedCancellationToken);
                    await auditTrail.WriteAsync(
                        document.CorrelationId, "Documents", "Parsed", "ImportedDocument", document.Id.ToString(),
                        result.Succeeded ? "status=parsed" : $"status={result.ErrorCode}", scopedCancellationToken,
                        document.TenantId, document.OfficeBranchId);
                    return true;
                }, cancellationToken);
            processed++;
        }
        return processed;
    }
}
