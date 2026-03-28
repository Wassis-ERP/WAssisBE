using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Documents.Enums;

namespace WAssis.Domain.Modules.Documents.Entities;

public class ImportedDocument : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public string? StoragePath { get; private set; }
    public ImportedDocumentStatus Status { get; private set; }
    public string? DocumentType { get; private set; }
    public string? ExtractedText { get; private set; }
    public string? InsuranceCompanyName { get; private set; }
    public string? ProposalNumber { get; private set; }
    public string? InsuredName { get; private set; }
    public DateTime? CoverageStartDateUtc { get; private set; }
    public DateTime? CoverageEndDateUtc { get; private set; }
    public decimal? TotalPremiumAmount { get; private set; }
    public decimal? CommissionAmount { get; private set; }
    public decimal ParsingConfidence { get; private set; }
    public bool RequiresHumanReview { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public string? ReviewedByUserId { get; private set; }
    public string? ParsingNotes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastProcessedAtUtc { get; private set; }

    private ImportedDocument()
    {
    }

    private ImportedDocument(Guid id, string tenantId, string correlationId, string fileName, string contentType, string source, string? storagePath)
    {
        Id = id;
        TenantId = tenantId;
        CorrelationId = correlationId;
        FileName = fileName;
        ContentType = contentType;
        Source = source;
        StoragePath = storagePath;
        Status = ImportedDocumentStatus.Received;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static ImportedDocument Create(
        string tenantId,
        string correlationId,
        string fileName,
        string contentType,
        string source,
        string? storagePath = null)
    {
        return new ImportedDocument(Guid.NewGuid(), tenantId, correlationId, fileName, contentType, source, storagePath);
    }

    public void MarkAsProcessing()
    {
        Status = ImportedDocumentStatus.Processing;
        LastProcessedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsParsed(
        string documentType,
        string extractedText,
        string? insuranceCompanyName,
        string? proposalNumber,
        string? insuredName,
        DateTime? coverageStartDateUtc,
        DateTime? coverageEndDateUtc,
        decimal? totalPremiumAmount,
        decimal? commissionAmount,
        decimal parsingConfidence,
        bool requiresHumanReview,
        string? parsingNotes)
    {
        Status = ImportedDocumentStatus.Parsed;
        DocumentType = documentType;
        ExtractedText = extractedText;
        InsuranceCompanyName = insuranceCompanyName;
        ProposalNumber = proposalNumber;
        InsuredName = insuredName;
        CoverageStartDateUtc = coverageStartDateUtc;
        CoverageEndDateUtc = coverageEndDateUtc;
        TotalPremiumAmount = totalPremiumAmount;
        CommissionAmount = commissionAmount;
        ParsingConfidence = parsingConfidence;
        RequiresHumanReview = requiresHumanReview;
        ParsingNotes = parsingNotes;
        LastProcessedAtUtc = DateTime.UtcNow;
    }

    public void MarkReviewed(string reviewedByUserId, string? parsingNotes = null)
    {
        RequiresHumanReview = false;
        ReviewedAtUtc = DateTime.UtcNow;
        ReviewedByUserId = reviewedByUserId;
        ParsingNotes = parsingNotes ?? ParsingNotes;
    }

    public void MarkAsFailed(string? parsingNotes)
    {
        Status = ImportedDocumentStatus.Failed;
        ParsingNotes = parsingNotes;
        LastProcessedAtUtc = DateTime.UtcNow;
    }
}
