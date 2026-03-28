using WAssis.Domain.Modules.Policies.Enums;

namespace WAssis.Application.Modules.Policies.Dtos;

public sealed record PolicyDraftDto(
    Guid Id,
    Guid ImportedDocumentId,
    string CorrelationId,
    string? InsuranceCompanyName,
    string? ProposalNumber,
    string? InsuredName,
    DateTime? CoverageStartDateUtc,
    DateTime? CoverageEndDateUtc,
    decimal? TotalPremiumAmount,
    decimal? CommissionAmount,
    PolicyDraftStatus Status,
    string? PolicyNumber,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime? ReadyForIssuanceAtUtc,
    DateTime? IssuedAtUtc);
