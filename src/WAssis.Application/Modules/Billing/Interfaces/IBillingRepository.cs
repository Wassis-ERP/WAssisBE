using WAssis.Domain.Modules.Billing.Entities;

namespace WAssis.Application.Modules.Billing.Interfaces;

public interface IBillingRepository
{
    Task AddSubscriptionAsync(BillingSubscription subscription, CancellationToken cancellationToken);
    Task<BillingSubscription?> GetSubscriptionByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddInvoiceAsync(BillingInvoice invoice, CancellationToken cancellationToken);
    Task<BillingInvoice?> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> InvoiceReferencePeriodExistsAsync(Guid subscriptionId, string referencePeriod, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
