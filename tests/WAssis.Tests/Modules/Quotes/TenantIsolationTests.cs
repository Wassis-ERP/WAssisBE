using Microsoft.EntityFrameworkCore;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Modules.Notifications.Queries;
using WAssis.Infra.Data.Modules.Quotes.Repositories;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Modules.Quotes;

public sealed class TenantIsolationTests
{
    [Fact]
    public async Task QuoteRequestRepository_ShouldOnlyReturnDataFromCurrentTenant()
    {
        var databaseName = $"tenant-quotes-{Guid.NewGuid():N}";

        await using (var seedContext = CreateContext(databaseName, new FakeCurrentUserContext()))
        {
            seedContext.QuoteRequests.AddRange(
                CreateQuoteRequest("tenant-a", "corr-a"),
                CreateQuoteRequest("tenant-b", "corr-b"));
            await seedContext.SaveChangesAsync();
        }

        await using var tenantAContext = CreateContext(
            databaseName,
            new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                TenantId = "tenant-a"
            });

        var repository = new QuoteRequestRepository(tenantAContext);

        var ownItem = await repository.GetByCorrelationIdAsync("corr-a", CancellationToken.None);
        var otherTenantItem = await repository.GetByCorrelationIdAsync("corr-b", CancellationToken.None);

        Assert.NotNull(ownItem);
        Assert.Equal("tenant-a", ownItem!.TenantId);
        Assert.Null(otherTenantItem);
    }

    [Fact]
    public async Task OperationsDashboardReadRepository_ShouldCountOnlyCurrentTenantData()
    {
        var databaseName = $"tenant-dashboard-{Guid.NewGuid():N}";

        await using (var seedContext = CreateContext(databaseName, new FakeCurrentUserContext()))
        {
            seedContext.QuoteRequests.AddRange(
                CreateQuoteRequest("tenant-a", "corr-a"),
                CreateQuoteRequest("tenant-b", "corr-b"));
            await seedContext.SaveChangesAsync();
        }

        await using var tenantAContext = CreateContext(
            databaseName,
            new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                TenantId = "tenant-a"
            });

        var repository = new OperationsDashboardReadRepository(tenantAContext);
        var overview = await repository.GetOverviewAsync(CancellationToken.None);

        Assert.Equal(1, overview.PendingQuotes);
    }

    [Fact]
    public async Task QuoteRequestRepository_ShouldOnlyReturnAuthorizedBranches()
    {
        var databaseName = $"branch-quotes-{Guid.NewGuid():N}";

        await using (var seedContext = CreateContext(databaseName, new FakeCurrentUserContext()))
        {
            seedContext.QuoteRequests.AddRange(
                CreateQuoteRequest("tenant-a", "corr-a", "branch-a"),
                CreateQuoteRequest("tenant-a", "corr-b", "branch-b"),
                CreateQuoteRequest("tenant-a", "corr-unscoped"));
            await seedContext.SaveChangesAsync();
        }

        await using var branchContext = CreateContext(
            databaseName,
            new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                TenantId = "tenant-a",
                BranchId = "branch-a",
                BranchIds = ["branch-a"],
                HasAllBranchesAccess = false,
            });

        var repository = new QuoteRequestRepository(branchContext);

        Assert.NotNull(await repository.GetByCorrelationIdAsync("corr-a", CancellationToken.None));
        Assert.Null(await repository.GetByCorrelationIdAsync("corr-b", CancellationToken.None));
        Assert.Null(await repository.GetByCorrelationIdAsync("corr-unscoped", CancellationToken.None));
    }

    private static WAssisDbContext CreateContext(string databaseName, FakeCurrentUserContext currentUserContext)
    {
        var options = new DbContextOptionsBuilder<WAssisDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new WAssisDbContext(options, currentUserContext, currentUserContext.IsAuthenticated ? null
            : WAssis.Application.Abstractions.SystemDataScope.ForWorker("test-fixture-seed", _ => { }));
    }

    private static QuoteRequest CreateQuoteRequest(string tenantId, string correlationId, string? officeBranchId = null)
    {
        return QuoteRequest.Create(
            tenantId,
            correlationId,
            "Cliente Teste",
            "12345678910",
            "cliente@teste.local",
            "5511999999999",
            "05516020",
            "Silva",
            "M",
            "1",
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            12,
            "12345678901",
            "0",
            "9BWZZZ377VT004251",
            "ABC1D23",
            "Ford",
            "Ka",
            "023108-8",
            2021,
            2022,
            false,
            false,
            true,
            false,
            false,
            "1",
            "05516020",
            false,
            false,
            false,
            "0",
            15,
            null,
            officeBranchId);
    }
}
