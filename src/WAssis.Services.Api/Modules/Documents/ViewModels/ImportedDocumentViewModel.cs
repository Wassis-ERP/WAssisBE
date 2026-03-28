using WAssis.Domain.Modules.Documents.Enums;

namespace WAssis.Services.Api.Modules.Documents.ViewModels;

public sealed record ImportedDocumentViewModel(
    Guid Id,
    string CorrelationId,
    string FileName,
    string ContentType,
    string Source,
    ImportedDocumentStatus Status,
    string? DocumentType,
    string? InsuranceCompanyName,
    string? ProposalNumber,
    string? InsuredName,
    DateTime? CoverageStartDateUtc,
    DateTime? CoverageEndDateUtc,
    decimal? TotalPremiumAmount,
    decimal? CommissionAmount,
    string? ParsingNotes,
    DateTime CreatedAtUtc);
