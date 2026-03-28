namespace WAssis.Services.Api.Modules.Financial.ViewModels;

public sealed record CommissionStatementLineViewModel(
    int LineNumber,
    string RawText,
    string? InsuranceCompanyCode,
    string? ProposalNumber,
    decimal? Amount,
    DateTime? OccurredAtUtc,
    string? SuggestedReference,
    bool NeedsManualReview);

public sealed record CommissionStatementAnalysisViewModel(
    string SourceType,
    int ParsedLines,
    int LinesNeedingManualReview,
    IReadOnlyCollection<CommissionStatementLineViewModel> Lines);
