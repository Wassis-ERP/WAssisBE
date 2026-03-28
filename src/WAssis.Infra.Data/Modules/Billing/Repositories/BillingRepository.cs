using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Billing.Interfaces;
using WAssis.Domain.Modules.Billing.Entities;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Billing.Repositories;

public sealed class BillingRepository(WAssisDbContext dbContext) : IBillingRepository
{
    public async Task AddSubscriptionAsync(BillingSubscription subscription, CancellationToken cancellationToken)
    {
        await dbContext.BillingSubscriptions.AddAsync(subscription, cancellationToken);
    }

    public Task<BillingSubscription?> GetSubscriptionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.BillingSubscriptions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddInvoiceAsync(BillingInvoice invoice, CancellationToken cancellationToken)
    {
        await dbContext.BillingInvoices.AddAsync(invoice, cancellationToken);
    }

    public Task<BillingInvoice?> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.BillingInvoices.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> InvoiceReferencePeriodExistsAsync(Guid subscriptionId, string referencePeriod, CancellationToken cancellationToken)
    {
        return dbContext.BillingInvoices.AnyAsync(
            x => x.BillingSubscriptionId == subscriptionId && x.ReferencePeriod == referencePeriod,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
