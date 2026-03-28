using WAssis.Application.Modules.Billing.Commands;
using WAssis.Application.Modules.Billing.Interfaces;
using WAssis.Domain.Modules.Billing.Entities;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Modules.Billing;

public sealed class CreateBillingSubscriptionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateSubscription_ForCurrentTenant()
    {
        var repository = new InMemoryBillingRepository();
        var handler = new CreateBillingSubscriptionCommandHandler(
            repository,
            new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                TenantId = "tenant-billing"
            });

        var result = await handler.Handle(
            new CreateBillingSubscriptionCommand(
                "corr-billing-1",
                "Corretora Alpha",
                "erp-pro",
                "ERP Pro",
                Domain.Modules.Billing.Enums.BillingRecurrence.Monthly,
                299m,
                10,
                new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("tenant-billing", repository.Subscriptions.Single().TenantId);
        Assert.Equal("Corretora Alpha", repository.Subscriptions.Single().CustomerDisplayName);
    }

    private sealed class InMemoryBillingRepository : IBillingRepository
    {
        public List<BillingSubscription> Subscriptions { get; } = [];
        public List<Domain.Modules.Billing.Entities.BillingInvoice> Invoices { get; } = [];

        public Task AddSubscriptionAsync(BillingSubscription subscription, CancellationToken cancellationToken)
        {
            Subscriptions.Add(subscription);
            return Task.CompletedTask;
        }

        public Task<BillingSubscription?> GetSubscriptionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Subscriptions.SingleOrDefault(x => x.Id == id));
        }

        public Task AddInvoiceAsync(Domain.Modules.Billing.Entities.BillingInvoice invoice, CancellationToken cancellationToken)
        {
            Invoices.Add(invoice);
            return Task.CompletedTask;
        }

        public Task<Domain.Modules.Billing.Entities.BillingInvoice?> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Invoices.SingleOrDefault(x => x.Id == id));
        }

        public Task<bool> InvoiceReferencePeriodExistsAsync(Guid subscriptionId, string referencePeriod, CancellationToken cancellationToken)
        {
            return Task.FromResult(Invoices.Any(x => x.BillingSubscriptionId == subscriptionId && x.ReferencePeriod == referencePeriod));
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
