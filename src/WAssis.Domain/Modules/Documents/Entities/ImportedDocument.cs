using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Documents.Enums;

namespace WAssis.Domain.Modules.Documents.Entities;

public class ImportedDocument : AggregateRoot
{
    public string CorrelationId { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
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
    public string? ParsingNotes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ImportedDocument()
    {
    }

    private ImportedDocument(Guid id, string correlationId, string fileName, string contentType, string source)
    {
        Id = id;
        CorrelationId = correlationId;
        FileName = fileName;
        ContentType = contentType;
        Source = source;
        Status = ImportedDocumentStatus.Received;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static ImportedDocument Create(string correlationId, string fileName, string contentType, string source)
    {
        return new ImportedDocument(Guid.NewGuid(), correlationId, fileName, contentType, source);
    }

    public void MarkAsProcessing()
    {
        Status = ImportedDocumentStatus.Processing;
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
        ParsingNotes = parsingNotes;
    }

    public void MarkAsFailed(string? parsingNotes)
    {
        Status = ImportedDocumentStatus.Failed;
        ParsingNotes = parsingNotes;
    }
}
