using WAssis.Domain.Modules.Documents.Enums;

namespace WAssis.Application.Modules.Documents.Dtos;

public sealed record ImportedDocumentDto(
    Guid Id,
    string CorrelationId,
    string FileName,
    string ContentType,
    string Source,
    string? StoragePath,
    ImportedDocumentStatus Status,
    string? DocumentType,
    string? InsuranceCompanyName,
    string? ProposalNumber,
    string? InsuredName,
    DateTime? CoverageStartDateUtc,
    DateTime? CoverageEndDateUtc,
    decimal? TotalPremiumAmount,
    decimal? CommissionAmount,
    decimal ParsingConfidence,
    bool RequiresHumanReview,
    DateTime? ReviewedAtUtc,
    string? ReviewedByUserId,
    string? ParsingNotes,
    DateTime CreatedAtUtc,
    DateTime? LastProcessedAtUtc);
