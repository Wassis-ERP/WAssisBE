using WAssis.Domain.Modules.Billing.Enums;

namespace WAssis.Services.Api.Modules.Billing.ViewModels;

public sealed record BillingSubscriptionViewModel(
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
