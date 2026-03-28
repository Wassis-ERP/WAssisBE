using WAssis.Domain.Modules.Billing.Enums;

namespace WAssis.Services.Api.Modules.Billing.Contracts;

public sealed record CreateBillingSubscriptionRequest(
    string? CorrelationId,
    string CustomerDisplayName,
    string PlanCode,
    string PlanName,
    BillingRecurrence Recurrence,
    decimal Amount,
    int BillingDayOfMonth,
    DateTime StartsAtUtc);
