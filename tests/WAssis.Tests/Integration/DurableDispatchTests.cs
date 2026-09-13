using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WAssis.Application.Abstractions;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Modules.Quotes.Repositories;
using WAssis.Infra.Data.Processing;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Integration;

public sealed class DurableDispatchTests
{
    private const string Tenant = "22222222-2222-2222-2222-222222222222";
    private static SystemDataScope Scope() => SystemDataScope.ForWorker("test-durable-dispatch", _ => { });
    private static WAssisDbContext Context(string connection) => new(
        new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(connection).Options, new FakeCurrentUserContext(), Scope());

    [Fact]
    [Trait("Category", "Postgres")]
    public async Task AtomicOutboxClaimReceiptAndExpiredLease_UseRealPostgres()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithPassword(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))).Build();
        await postgres.StartAsync();
        await using var db = Context(postgres.GetConnectionString());
        await DatabaseMigrationGate.MigrateSingletonAsync(db);
        var repository = new QuoteRequestRepository(db);
        var rolledBack = Quote("rollback");
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            await repository.AddAsync(rolledBack, default);
            await repository.SaveChangesAsync(default);
            await transaction.RollbackAsync();
        }
        db.ChangeTracker.Clear();
        Assert.False(await db.QuoteRequests.AnyAsync(x => x.Id == rolledBack.Id));
        Assert.Equal(0, await Count(db, "SELECT count(*)::int AS \"Value\" FROM infrastructure.work_outbox"));
        var quote = Quote("durable");
        await repository.AddAsync(quote, default);
        await repository.SaveChangesAsync(default);
        await using var second = Context(postgres.GetConnectionString());
        var queue = new DurableWorkQueue(db, Scope());
        var otherQueue = new DurableWorkQueue(second, Scope());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new DurableWorkQueue(db).ClaimAsync("quotes.dispatch", default));
        var claims = await Task.WhenAll(queue.ClaimAsync("quotes.dispatch", default), otherQueue.ClaimAsync("quotes.dispatch", default));
        var lease = Assert.Single(claims, item => item is not null)!;
        Assert.Equal(quote.Id, lease.AggregateId);
        Assert.Equal(Tenant, lease.TenantId);
        await Assert.ThrowsAsync<InvalidOperationException>(() => queue.CompleteAsync(lease with { Token = Guid.NewGuid() }, default));
        await queue.RenewAsync(lease, default);
        quote.MarkAsProcessing();
        quote.MarkAsCompleted();
        await queue.CompleteAsync(lease, default);
        await Assert.ThrowsAsync<InvalidOperationException>(() => otherQueue.CompleteAsync(lease, default));
        Assert.Null(await queue.ClaimAsync("quotes.dispatch", default));
        Assert.Equal(1, await Count(db, "SELECT count(*)::int AS \"Value\" FROM infrastructure.work_inbox"));
        Assert.Equal(4, (int)(await second.QuoteRequests.AsNoTracking().SingleAsync(x => x.Id == quote.Id)).Status);

        var expired = Quote("expired");
        await repository.AddAsync(expired, default);
        await repository.SaveChangesAsync(default);
        var stale = await queue.ClaimAsync("quotes.dispatch", default);
        Assert.NotNull(stale);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE infrastructure.work_outbox SET lease_until = now() - interval '1 second' WHERE id = {stale.Id}");
        Assert.Equal(1, await otherQueue.QuarantineExpiredAsync(default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => queue.RenewAsync(stale, default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => queue.CompleteAsync(stale, default));
        Assert.Null(await queue.ClaimAsync("quotes.dispatch", default));
        Assert.Equal(1, await Count(db, "SELECT count(*)::int AS \"Value\" FROM infrastructure.work_outbox WHERE state = 'NeedsReview'"));
    }

    private static Task<int> Count(WAssisDbContext db, string sql) => db.Database.SqlQueryRaw<int>(sql).SingleAsync();

    internal static QuoteRequest Quote(string correlation) => QuoteRequest.Create(
        tenantId: Tenant, correlationId: correlation, customerName: "Fixture", documentNumber: "00000000000",
        email: null, phoneNumber: null, postalCode: null, customerSurname: null, customerGender: null,
        customerMaritalStatusCode: null, customerBirthDateUtc: null, driverLicenseYears: null,
        driverLicenseNumber: null, insuredDriverRelationshipCode: null, vehicleChassisNumber: null,
        vehiclePlate: null, vehicleBrand: null, vehicleModel: null, vehicleFipeCode: null,
        vehicleManufactureYear: null, vehicleModelYear: 2026, vehicleIsZeroKm: null, vehicleHasTracker: null,
        vehicleHasAntiTheft: null, vehicleIsFinanced: null, vehicleIsArmored: null, vehicleFuelTypeCode: null,
        vehicleOvernightPostalCode: null, vehicleHasKitGas: null, hasDriverUnder24: false,
        isCurrentlyInsured: false, previousBonus: null, brokerCommissionPercentage: null, renewalInsurerCode: null);
}
