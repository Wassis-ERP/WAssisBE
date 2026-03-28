using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Billing.Enums;

namespace WAssis.Domain.Modules.Billing.Entities;

public class BillingSubscription : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string CustomerDisplayName { get; private set; } = string.Empty;
    public string PlanCode { get; private set; } = string.Empty;
    public string PlanName { get; private set; } = string.Empty;
    public BillingRecurrence Recurrence { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public int BillingDayOfMonth { get; private set; }
    public BillingSubscriptionStatus Status { get; private set; }
    public DateTime StartsAtUtc { get; private set; }
    public DateTime NextInvoiceDueDateUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CanceledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }

    private BillingSubscription()
    {
    }

    private BillingSubscription(
        Guid id,
        string tenantId,
        string correlationId,
        string customerDisplayName,
        string planCode,
        string planName,
        BillingRecurrence recurrence,
        decimal amount,
        int billingDayOfMonth,
        DateTime startsAtUtc)
    {
        Id = id;
        TenantId = tenantId;
        CorrelationId = correlationId;
        CustomerDisplayName = customerDisplayName;
        PlanCode = planCode;
        PlanName = planName;
        Recurrence = recurrence;
        Amount = amount;
        BillingDayOfMonth = billingDayOfMonth;
        StartsAtUtc = startsAtUtc;
        NextInvoiceDueDateUtc = AlignDueDate(startsAtUtc, billingDayOfMonth);
        Status = BillingSubscriptionStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static BillingSubscription Create(
        string tenantId,
        string correlationId,
        string customerDisplayName,
        string planCode,
        string planName,
        BillingRecurrence recurrence,
        decimal amount,
        int billingDayOfMonth,
        DateTime startsAtUtc)
    {
        return new BillingSubscription(
            Guid.NewGuid(),
            tenantId,
            correlationId,
            customerDisplayName,
            planCode,
            planName,
            recurrence,
            amount,
            billingDayOfMonth,
            startsAtUtc);
    }

    public void RegisterInvoiceIssued(DateTime currentDueDateUtc)
    {
        if (Status != BillingSubscriptionStatus.Active)
        {
            return;
        }

        NextInvoiceDueDateUtc = Recurrence switch
        {
            BillingRecurrence.Monthly => currentDueDateUtc.AddMonths(1),
            BillingRecurrence.Quarterly => currentDueDateUtc.AddMonths(3),
            BillingRecurrence.Annual => currentDueDateUtc.AddYears(1),
            _ => currentDueDateUtc.AddMonths(1)
        };
    }

    public void Cancel(string? reason)
    {
        Status = BillingSubscriptionStatus.Canceled;
        CanceledAtUtc = DateTime.UtcNow;
        CancellationReason = reason;
    }

    private static DateTime AlignDueDate(DateTime startsAtUtc, int billingDayOfMonth)
    {
        var safeDay = Math.Clamp(billingDayOfMonth, 1, DateTime.DaysInMonth(startsAtUtc.Year, startsAtUtc.Month));
        return new DateTime(startsAtUtc.Year, startsAtUtc.Month, safeDay, 0, 0, 0, DateTimeKind.Utc);
    }
}
