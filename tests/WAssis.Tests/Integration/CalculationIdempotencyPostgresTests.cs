using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Calculations;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Modules.Calculations;
using WAssis.Infra.Data.Modules.Notifications.Repositories;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Integration;

public sealed class CalculationIdempotencyPostgresTests
{
    private const string Tenant = "22222222-2222-2222-2222-222222222222";
    private const string Branch = "44444444-4444-4444-4444-444444444444";
    private const string User = "77777777-7777-7777-7777-777777777777";

    [Fact]
    [Trait("Category", "Postgres")]
    public async Task ConcurrentCanonicalRetries_CreateOneAggregateAndReceipt_WithNoRlsBypass()
    {
        await using var postgres = await PostgresTestDatabase.StartAsync();
        await using var admin = new WAssisDbContext(new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(postgres.GetConnectionString()).Options,
            new FakeCurrentUserContext(), SystemDataScope.ForWorker("test-calculation-idempotency", _ => { }));
        await DatabaseMigrationGate.MigrateSingletonAsync(admin);
        var pipeline = Guid.NewGuid(); var stage = Guid.NewGuid(); var opportunity = Guid.NewGuid(); var line = Guid.NewGuid(); var coverage = Guid.NewGuid();
        await admin.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.tenants (id,timezone) VALUES ({Guid.Parse(Tenant)},'America/Sao_Paulo');
            INSERT INTO erp.filiais (id,tenant_id,ativo) VALUES ({Guid.Parse(Branch)},{Guid.Parse(Tenant)},true);
            INSERT INTO erp.profiles (id,tenant_id,ativo) VALUES ({Guid.Parse(User)},{Guid.Parse(Tenant)},true);
            INSERT INTO erp.pipelines (id,tenant_id,filial_id,entidade_tipo,ativo) VALUES ({pipeline},{Guid.Parse(Tenant)},{Guid.Parse(Branch)},'oportunidade',true);
            INSERT INTO erp.pipeline_stages (id,pipeline_id,ativo) VALUES ({stage},{pipeline},true);
            INSERT INTO erp.oportunidades (id,tenant_id,filial_id,responsavel_id,stage_id,titulo) VALUES ({opportunity},{Guid.Parse(Tenant)},{Guid.Parse(Branch)},{Guid.Parse(User)},{stage},'Lead');
            INSERT INTO erp.ramos (id,tenant_id,forma_calculo,ativo) VALUES ({line},{Guid.Parse(Tenant)},'AUTO',true);
            INSERT INTO erp.coberturas_catalogo (id,ramo_id,nome,ativo) VALUES ({coverage},{line},'RC',true);
            """);
        var connection = await postgres.CreateApplicationConnectionAsync();
        async Task<Guid> Create(string key, decimal limit)
        {
            var user = new FakeCurrentUserContext { IsAuthenticated = true, TenantId = Tenant, BranchId = Branch, BranchIds = [Branch], UserId = User };
            await using var db = new WAssisDbContext(new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(connection).Options, user);
            await using var transaction = await db.Database.BeginTransactionAsync();
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT set_config('app.tenant_id',{Tenant},true),set_config('app.branch_id',{Branch},true)");
            var store = new CanonicalCalculationStore(db, user, TimeProvider.System, new AuditTrailWriter(db, user));
            var result = await store.CreateAsync(opportunity, key, new()
            {
                InsuranceBranchId = line, Specialization = new() { Data = new() { DriverName = "Condutor" } },
                Coverages = [new() { CoverageId = coverage, RequestedLimit = limit, Selected = true }]
            }, default);
            await transaction.CommitAsync();
            return result!.Calculation.Id;
        }
        var ids = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Create("same-key", 100000)));
        Assert.Single(ids.Distinct());
        Assert.Equal(1, await admin.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM erp.calculos").SingleAsync());
        Assert.Equal(1, await admin.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM infrastructure.calculation_receipts").SingleAsync());
        Assert.Equal(ids[0], await Create("same-key", 100000.00m));
        await Assert.ThrowsAsync<CalculationConflictException>(() => Create("same-key", 200000));
        var versions = await Task.WhenAll(Create("version-two", 200000), Create("version-three", 300000));
        Assert.Equal(2, versions.Distinct().Count());
        Assert.Equal(3, await admin.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM erp.calculos").SingleAsync());
        Assert.Equal(3, await admin.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM operations.audit_entries WHERE \"Module\"='Calculations'").SingleAsync());
    }
}
