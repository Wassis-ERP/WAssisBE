using WAssis.Application.Modules.Documents.Dtos;
using WAssis.Domain.Modules.Documents.Entities;

namespace WAssis.Application.Modules.Documents;

internal static class ImportedDocumentMappings
{
    public static ImportedDocumentDto ToDto(ImportedDocument document)
    {
        return new ImportedDocumentDto(
            document.Id,
            document.CorrelationId,
            document.FileName,
            document.ContentType,
            document.Source,
            document.StoragePath,
            document.Status,
            document.DocumentType,
            document.InsuranceCompanyName,
            document.ProposalNumber,
            document.InsuredName,
            document.CoverageStartDateUtc,
            document.CoverageEndDateUtc,
            document.TotalPremiumAmount,
            document.CommissionAmount,
            document.ParsingConfidence,
            document.RequiresHumanReview,
            document.ReviewedAtUtc,
            document.ReviewedByUserId,
            document.ParsingNotes,
            document.CreatedAtUtc,
            document.LastProcessedAtUtc);
    }
}
