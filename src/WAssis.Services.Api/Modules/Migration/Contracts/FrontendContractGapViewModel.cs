namespace WAssis.Services.Api.Modules.Migration.Contracts;

public sealed record FrontendContractGapViewModel(
    string ContractKey,
    string LegacySurface,
    string OwnerIssue,
    string NextStep,
    string? ReplacementEndpoint);
