using WAssis.Domain.Modules.Billing.Enums;

namespace WAssis.Application.Modules.Billing.Dtos;

public sealed record BillingSubscriptionDto(
    Guid Id,
    string CorrelationId,
    string CustomerDisplayName,
    string PlanCode,
    string PlanName,
    BillingRecurrence Recurrence,
    decimal Amount,
    string Currency,
    int BillingDayOfMonth,
    BillingSubscriptionStatus Status,
    DateTime StartsAtUtc,
    DateTime NextInvoiceDueDateUtc,
    DateTime CreatedAtUtc,
    DateTime? CanceledAtUtc,
    string? CancellationReason);
