using WAssis.Application.Modules.Billing.Commands;
using WAssis.Application.Modules.Billing.Interfaces;
using WAssis.Domain.Modules.Billing.Entities;
using WAssis.Domain.Modules.Billing.Enums;

namespace WAssis.Tests.Modules.Billing;

public sealed class IssueBillingInvoiceCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldIssueInvoice_AndAdvanceNextDueDate()
    {
        var subscription = BillingSubscription.Create(
            "tenant-billing",
            "corr-billing-2",
            "Corretora Beta",
            "erp-pro",
            "ERP Pro",
            BillingRecurrence.Monthly,
            399m,
            5,
            new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc));

        var repository = new InMemoryBillingRepository(subscription);
        var handler = new IssueBillingInvoiceCommandHandler(repository);

        var result = await handler.Handle(
            new IssueBillingInvoiceCommand(
                subscription.Id,
                "corr-invoice-1",
                "2026-04",
                null,
                "INV-ERP-2026-04",
                "primeira mensalidade"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(BillingInvoiceStatus.Pending, result.Value!.Status);
        Assert.Equal(new DateTime(2026, 5, 5, 0, 0, 0, DateTimeKind.Utc), subscription.NextInvoiceDueDateUtc);
    }

    private sealed class InMemoryBillingRepository(BillingSubscription subscription) : IBillingRepository
    {
        private readonly List<BillingSubscription> _subscriptions = [subscription];
        private readonly List<Domain.Modules.Billing.Entities.BillingInvoice> _invoices = [];

        public Task AddSubscriptionAsync(BillingSubscription newSubscription, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<BillingSubscription?> GetSubscriptionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_subscriptions.SingleOrDefault(x => x.Id == id));
        }

        public Task AddInvoiceAsync(Domain.Modules.Billing.Entities.BillingInvoice invoice, CancellationToken cancellationToken)
        {
            _invoices.Add(invoice);
            return Task.CompletedTask;
        }

        public Task<Domain.Modules.Billing.Entities.BillingInvoice?> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_invoices.SingleOrDefault(x => x.Id == id));
        }

        public Task<bool> InvoiceReferencePeriodExistsAsync(Guid subscriptionId, string referencePeriod, CancellationToken cancellationToken)
        {
            return Task.FromResult(_invoices.Any(x => x.BillingSubscriptionId == subscriptionId && x.ReferencePeriod == referencePeriod));
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
