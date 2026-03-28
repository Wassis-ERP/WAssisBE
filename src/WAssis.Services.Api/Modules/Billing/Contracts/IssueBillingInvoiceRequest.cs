namespace WAssis.Services.Api.Modules.Billing.Contracts;

public sealed record IssueBillingInvoiceRequest(
    string? CorrelationId,
    string ReferencePeriod,
    DateTime? DueDateUtc,
    string? ExternalReference,
    string? Notes);
