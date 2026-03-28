namespace WAssis.Services.Api.Modules.Financial.Contracts;

public sealed record AnalyzeCommissionStatementRequest(
    string SourceType,
    string RawText);
