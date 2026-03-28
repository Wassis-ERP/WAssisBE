using WAssis.Domain.Modules.Billing.Enums;

namespace WAssis.Application.Modules.Billing.Dtos;

public sealed record BillingInvoiceDto(
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
