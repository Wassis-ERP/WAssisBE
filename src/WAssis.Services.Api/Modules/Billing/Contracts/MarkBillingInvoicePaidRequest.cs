namespace WAssis.Services.Api.Modules.Billing.Contracts;

public sealed record MarkBillingInvoicePaidRequest(
    string? PaymentMethod,
    string? ExternalReference,
    string? Notes);
