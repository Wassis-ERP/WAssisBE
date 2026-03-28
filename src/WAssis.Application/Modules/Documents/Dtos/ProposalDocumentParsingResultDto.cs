namespace WAssis.Application.Modules.Documents.Dtos;

public sealed record ProposalDocumentParsingResultDto(
    string DocumentType,
    string ExtractedText,
    string? InsuranceCompanyName,
    string? ProposalNumber,
    string? InsuredName,
    DateTime? CoverageStartDateUtc,
    DateTime? CoverageEndDateUtc,
    decimal? TotalPremiumAmount,
    decimal? CommissionAmount,
    decimal ParsingConfidence,
    bool RequiresHumanReview,
    string? ParsingNotes);
