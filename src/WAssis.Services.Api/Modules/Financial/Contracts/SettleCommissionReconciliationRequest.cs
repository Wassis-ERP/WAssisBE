namespace WAssis.Services.Api.Modules.Financial.Contracts;

public sealed record SettleCommissionReconciliationRequest(
    string? MatchedReference,
    string? SettlementNotes);
