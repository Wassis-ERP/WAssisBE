using WAssis.Application.Modules.Billing.Dtos;
using WAssis.Domain.Modules.Billing.Entities;

namespace WAssis.Application.Modules.Billing;

public static class BillingMappings
{
    public static BillingSubscriptionDto ToDto(this BillingSubscription subscription)
    {
        return new BillingSubscriptionDto(
            subscription.Id,
            subscription.CorrelationId,
            subscription.CustomerDisplayName,
            subscription.PlanCode,
            subscription.PlanName,
            subscription.Recurrence,
            subscription.Amount,
            subscription.Currency,
            subscription.BillingDayOfMonth,
            subscription.Status,
            subscription.StartsAtUtc,
            subscription.NextInvoiceDueDateUtc,
            subscription.CreatedAtUtc,
            subscription.CanceledAtUtc,
            subscription.CancellationReason);
    }

    public static BillingInvoiceDto ToDto(this BillingInvoice invoice)
    {
        return new BillingInvoiceDto(
            invoice.Id,
            invoice.BillingSubscriptionId,
            invoice.CorrelationId,
            invoice.ReferencePeriod,
            invoice.Amount,
            invoice.Currency,
            invoice.DueDateUtc,
            invoice.Status,
            invoice.ExternalReference,
            invoice.PaymentMethod,
            invoice.Notes,
            invoice.IssuedAtUtc,
            invoice.PaidAtUtc);
    }
}
