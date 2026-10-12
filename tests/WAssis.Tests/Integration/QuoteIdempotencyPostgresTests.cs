using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Domain.Modules.Quotes;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Modules.Quotes.Repositories;
using WAssis.Tests.Modules.Quotes;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Integration;

public sealed class QuoteIdempotencyPostgresTests
{
    private const string Tenant = "22222222-2222-2222-2222-222222222222";
    private const string Branch = "33333333-3333-3333-3333-333333333333";
    private const string OtherBranch = "33333333-3333-3333-3333-333333333334";

    [Fact]
    [Trait("Category", "Postgres")]
    public async Task ConcurrentRetries_RlsConflictCancellationAndOutbox_AreAtomic()
    {
        await using var postgres = await PostgresTestDatabase.StartAsync();
        await using var admin = new WAssisDbContext(
            new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(postgres.GetConnectionString()).Options,
            new FakeCurrentUserContext(), SystemDataScope.ForWorker("test-idempotency-migration", _ => { }));
        await DatabaseMigrationGate.MigrateSingletonAsync(admin);
        var connection = await postgres.CreateApplicationConnectionAsync();

        var retries = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Create(connection, "concurrent", 100000)));
        Assert.Single(retries.Distinct());
        Assert.Equal(1, await Count(admin, "quotes.quote_requests"));
        Assert.Equal(1, await Count(admin, "infrastructure.work_outbox"));
        await Assert.ThrowsAsync<QuoteRequestConflictException>(() => Create(connection, "concurrent", 200000));
        await Assert.ThrowsAsync<QuoteRequestConflictException>(() => Create(connection, "concurrent", 100000, branch: OtherBranch));
        Assert.Equal(retries[0], await Create(connection, "concurrent", 100000));

        // Different tenants can reuse a key; a hidden row from another branch cannot be returned.
        Assert.NotEqual(retries[0], await Create(connection, "concurrent", 100000, tenant: "22222222-2222-2222-2222-222222222223"));

        // Two different inputs racing for a new key produce exactly one winner and one conflict.
        async Task<bool> TryVersion(decimal limit)
        {
            try { await Create(connection, "racing-versions", limit); return true; }
            catch (QuoteRequestConflictException) { return false; }
        }
        var versions = await Task.WhenAll(TryVersion(100000), TryVersion(200000));
        Assert.Single(versions, won => won);
        Assert.Equal(3, await Count(admin, "quotes.quote_requests"));
        Assert.Equal(3, await Count(admin, "infrastructure.work_outbox"));

        // Cancellation while waiting for the same key must not enqueue a partial request.
        await using (var blocker = Context(connection, Tenant, Branch))
        await using (var transaction = await blocker.Database.BeginTransactionAsync())
        {
            var key = $"quotes.create:{Tenant.Length}:{Tenant}:cancelled";
            await blocker.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))");
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Create(connection, "cancelled", 100000, cancellationToken: cancellation.Token));
            await transaction.RollbackAsync();
        }
        Assert.Equal(3, await Count(admin, "infrastructure.work_outbox"));
        await Create(connection, "cancelled", 100000);
        Assert.Equal(4, await Count(admin, "infrastructure.work_outbox"));
    }

    private static WAssisDbContext Context(string connection, string tenant, string branch) => new(
        new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(connection).Options,
        new FakeCurrentUserContext { IsAuthenticated = true, TenantId = tenant, BranchId = branch, BranchIds = [branch] });

    private static async Task<Guid> Create(string connection, string key, decimal limit,
        string tenant = Tenant, string branch = Branch, CancellationToken cancellationToken = default)
    {
        await using var db = Context(connection, tenant, branch);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.tenant_id', {tenant}, true), set_config('app.branch_id', {branch}, true)", cancellationToken);
        var candidate = DurableDispatchTests.Quote(key, tenant, branch, AutoQuoteSubmissionTests.Submission(limit));
        var result = await new QuoteRequestRepository(db).CreateIdempotentAsync(candidate, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result.Id;
    }

    private static Task<int> Count(WAssisDbContext db, string table) => table switch
    {
        "quotes.quote_requests" => db.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM quotes.quote_requests").SingleAsync(),
        "infrastructure.work_outbox" => db.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM infrastructure.work_outbox").SingleAsync(),
        _ => throw new ArgumentOutOfRangeException(nameof(table))
    };
}
