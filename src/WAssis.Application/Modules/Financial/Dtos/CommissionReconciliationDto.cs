using WAssis.Domain.Modules.Financial.Enums;

namespace WAssis.Application.Modules.Financial.Dtos;

public sealed record CommissionReconciliationDto(
    Guid Id,
    Guid CommissionReceiptId,
    decimal ExpectedAmount,
    decimal ReceivedAmount,
    decimal DifferenceAmount,
    CommissionReconciliationStatus Status,
    string? MatchedReference,
    string? SettlementNotes,
    DateTime CreatedAtUtc,
    DateTime? SettledAtUtc);

public sealed record CommissionStatementLineDto(
    int LineNumber,
    string RawText,
    string? InsuranceCompanyCode,
    string? ProposalNumber,
    decimal? Amount,
    DateTime? OccurredAtUtc,
    string? SuggestedReference,
    bool NeedsManualReview);

public sealed record CommissionStatementAnalysisDto(
    string SourceType,
    int ParsedLines,
    int LinesNeedingManualReview,
    IReadOnlyCollection<CommissionStatementLineDto> Lines);
