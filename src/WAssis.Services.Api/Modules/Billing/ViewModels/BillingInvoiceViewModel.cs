using WAssis.Domain.Modules.Billing.Enums;

namespace WAssis.Services.Api.Modules.Billing.ViewModels;

public sealed record BillingInvoiceViewModel(
    Guid Id,
    Guid BillingSubscriptionId,
    string CorrelationId,
    string ReferencePeriod,
    decimal Amount,
    string Currency,
    DateTime DueDateUtc,
    BillingInvoiceStatus Status,
    string? ExternalReference,
    string? PaymentMethod,
    string? Notes,
    DateTime IssuedAtUtc,
    DateTime? PaidAtUtc);
