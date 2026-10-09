using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Modules.Quotes.Repositories;
using WAssis.Tests.Modules.Quotes;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Integration;

public sealed class AutoQuoteSubmissionPostgresTests
{
    [Fact]
    [Trait("Category", "Postgres")]
    public async Task SnapshotAndCoverageVersions_RoundtripWithAtomicOutbox()
    {
        await using var postgres = await PostgresTestDatabase.StartAsync();
        var options = new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(postgres.GetConnectionString()).Options;
        await using var db = new WAssisDbContext(options, new FakeCurrentUserContext(), SystemDataScope.ForWorker("test-auto-snapshot", _ => { }));
        await DatabaseMigrationGate.MigrateSingletonAsync(db);
        var repository = new QuoteRequestRepository(db);
        var quote = DurableDispatchTests.Quote("auto-pg", submission: AutoQuoteSubmissionTests.Submission());
        await repository.AddAsync(quote, default);
        await repository.SaveChangesAsync(default);
        db.ChangeTracker.Clear();
        var saved = await repository.GetByIdAsync(quote.Id, default);
        Assert.Equal("Condutora da versão", saved!.AutoSubmission!.DriverName);
        Assert.Equal(0, saved.AutoSubmission.BonusClass);
        Assert.Equal(new DateOnly(2028, 2, 29), saved.AutoSubmission.CoverageStartsOn);
        Assert.Equal(100000, saved.AutoSubmission.Coverages.Single(x => x.Code == "danos-materiais").RequestedLimit);
        Assert.Equal(1, await db.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM infrastructure.work_outbox").SingleAsync());
        var rolledBack = DurableDispatchTests.Quote("auto-rollback", submission: AutoQuoteSubmissionTests.Submission(200000));
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            await repository.AddAsync(rolledBack, default);
            await repository.SaveChangesAsync(default);
            await transaction.RollbackAsync();
        }
        db.ChangeTracker.Clear();
        Assert.Null(await repository.GetByIdAsync(rolledBack.Id, default));
        Assert.Equal(4, await db.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM quotes.quote_requested_coverages").SingleAsync());
    }
}
