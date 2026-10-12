using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Calculations;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Modules.Calculations;
using WAssis.Infra.Data.Modules.Notifications.Repositories;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Integration;

public sealed class ExecutionIdempotencyPostgresTests
{
    [Fact]
    [Trait("Category", "Postgres")]
    public async Task ManualExecutionAndResult_AreAtomicIdempotentAndScoped_WithConcurrentNoBypassRole()
    {
        await using var postgres = await PostgresTestDatabase.StartAsync();
        await using var admin = new WAssisDbContext(new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(postgres.GetConnectionString()).Options,
            new FakeCurrentUserContext(), SystemDataScope.ForWorker("test-manual-executions", _ => { }));
        await DatabaseMigrationGate.MigrateSingletonAsync(admin);
        var tenant = Guid.NewGuid(); var branch = Guid.NewGuid(); var userId = Guid.NewGuid();
        var opportunity = Guid.NewGuid(); var line = Guid.NewGuid(); var insurer = Guid.NewGuid(); var coverage = Guid.NewGuid();
        var pipeline = Guid.NewGuid(); var stage = Guid.NewGuid();
        await admin.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.tenants (id,timezone) VALUES ({tenant},'America/Sao_Paulo');
            INSERT INTO erp.filiais (id,tenant_id,ativo) VALUES ({branch},{tenant},true);
            INSERT INTO erp.profiles (id,tenant_id,ativo) VALUES ({userId},{tenant},true);
            INSERT INTO erp.pipelines (id,tenant_id,filial_id,entidade_tipo,ativo) VALUES ({pipeline},{tenant},{branch},'oportunidade',true);
            INSERT INTO erp.pipeline_stages (id,pipeline_id,ativo) VALUES ({stage},{pipeline},true);
            INSERT INTO erp.oportunidades (id,tenant_id,filial_id,responsavel_id,stage_id,titulo) VALUES ({opportunity},{tenant},{branch},{userId},{stage},'Manual fixture');
            INSERT INTO erp.ramos (id,tenant_id,forma_calculo,ativo) VALUES ({line},{tenant},'AUTO',true);
            INSERT INTO erp.seguradoras (id,tenant_id,ativo) VALUES ({insurer},{tenant},true);
            INSERT INTO erp.coberturas_catalogo (id,ramo_id,nome,ativo) VALUES ({coverage},{line},'Casco',true);
            """);
        var connection = await postgres.CreateApplicationConnectionAsync();
        var user = new FakeCurrentUserContext { IsAuthenticated = true, TenantId = tenant.ToString(), BranchId = branch.ToString(), BranchIds = [branch.ToString()], UserId = userId.ToString() };
        WAssisDbContext Context(FakeCurrentUserContext current, RecordPermissionScope? scope = null) => new(
            new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(connection).Options, current, recordScope: scope);
        async Task SetScope(WAssisDbContext db, FakeCurrentUserContext current) =>
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT set_config('app.tenant_id',{current.TenantId},true),set_config('app.branch_id',{current.BranchId},true)");
        Guid calculationId;
        await using (var db = Context(user))
        {
            await using var transaction = await db.Database.BeginTransactionAsync(); await SetScope(db, user);
            var calculations = new CanonicalCalculationStore(db, user, TimeProvider.System, new AuditTrailWriter(db, user));
            calculationId = (await calculations.CreateAsync(opportunity, "snapshot", new() { InsuranceBranchId = line, SuggestedCommission = 15 }, default))!.Calculation.Id;
            await transaction.CommitAsync();
        }
        async Task<T> Run<T>(Func<CanonicalExecutionStore, Task<T>> action, FakeCurrentUserContext? current = null, RecordPermissionScope? scope = null)
        {
            current ??= user;
            await using var db = Context(current, scope);
            await using var transaction = await db.Database.BeginTransactionAsync(); await SetScope(db, current);
            var result = await action(new(db, current, TimeProvider.System, new AuditTrailWriter(db, current)));
            await transaction.CommitAsync(); return result;
        }
        var selection = new CreateCalculationExecution { InsurerId = insurer };
        await using (var lookupDb = Context(user))
        {
            await using var lookupTransaction = await lookupDb.Database.BeginTransactionAsync(); await SetScope(lookupDb, user);
            var lookups = new CanonicalCalculationLookupStore(lookupDb, user, new LookupClock());
            var calendar = (await lookups.GetContextAsync(opportunity, line, default))!;
            Assert.Equal("2026-10-11", calendar.BranchDate); // 01:30 UTC is still the previous day in the group timezone.
            Assert.Single((await lookups.ListInsurersAsync(opportunity, 1, 50, null, default))!.Items);
            var forbiddenScope = new RecordPermissionScope(); forbiddenScope.RestrictToOwner("comercial", Guid.NewGuid().ToString(), []);
            await using var hiddenDb = Context(user, forbiddenScope);
            await using var hiddenTransaction = await hiddenDb.Database.BeginTransactionAsync(); await SetScope(hiddenDb, user);
            Assert.Null(await new CanonicalCalculationLookupStore(hiddenDb, user, new LookupClock()).GetContextAsync(opportunity, line, default));
            await hiddenTransaction.CommitAsync(); await lookupTransaction.CommitAsync();
        }
        var executions = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Run(s => s.CreateAsync(calculationId, "same-execution", selection, default))));
        var execution = executions[0]!;
        Assert.Single(executions.Select(x => x!.Id).Distinct()); Assert.Equal(15, execution.AppliedCommission);
        await Assert.ThrowsAsync<CalculationConflictException>(() => Run(s => s.CreateAsync(calculationId, "same-execution", selection with { CommissionOverride = 0 }, default)));
        await Assert.ThrowsAsync<ExecutionStateConflictException>(() => Run(s => s.CreateAsync(calculationId, "other-execution", selection, default)));
        var quote = new RecordManualQuote
        {
            TotalPremium = 1234.50m, Tax = 0,
            Coverages = [new() { ResultKey = "casco", CoverageId = coverage, AcceptedFipePercentage = 100, DeductibleAmount = 0 }, new() { ResultKey = "unmapped", ReportedName = "Cobertura sem de/para", Included = null }],
            Installments = [new() { OptionCode = "a", PaymentMethod = "BOLETO", InstallmentCount = 1, TotalAmount = 1234.50m }, new() { OptionCode = "b", PaymentMethod = "CARTAO", InstallmentCount = 10, FirstDueDate = "2028-02-29", Primary = true }]
        };
        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Run(s => s.RecordManualQuoteAsync(execution.Id, "same-quote", quote, default))));
        var result = results[0]!;
        Assert.Single(results.Select(x => x!.Quote.Id).Distinct()); Assert.Equal("CONCLUIDA", result.Execution.Status);
        Assert.Null(result.Quote.NetPremium); Assert.Equal(0, result.Coverages.Single(x => x.ResultKey == "casco").DeductibleAmount);
        var reordered = quote with { TotalPremium = 1234.500m, Coverages = quote.Coverages.Reverse().ToArray(), Installments = quote.Installments.Reverse().ToArray() };
        Assert.Equal(result.Quote.Id, (await Run(s => s.RecordManualQuoteAsync(execution.Id, "same-quote", reordered, default)))!.Quote.Id);
        await Assert.ThrowsAsync<CalculationConflictException>(() => Run(s => s.RecordManualQuoteAsync(execution.Id, "same-quote", quote with { TotalPremium = 2222 }, default)));
        Assert.Equal(1, await admin.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM erp.cotacoes").SingleAsync());
        Assert.Equal(2, await admin.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM infrastructure.execution_receipts").SingleAsync());

        var retries = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Run(s => s.RetryAsync(execution.Id, "same-retry", new() { CommissionOverride = 0 }, default))));
        var retry = retries[0]!;
        Assert.Single(retries.Select(x => x!.Id).Distinct()); Assert.Equal(2, retry.Attempt); Assert.Equal(execution.Id, retry.RetryOfId);
        Assert.Equal(0, retry.AppliedCommission); Assert.Equal("SOBRESCRITA", retry.CommissionOrigin);
        // Two competing operations on an open attempt must produce one terminal outcome.
        async Task<bool> Compete(Func<CanonicalExecutionStore, Task<object?>> operation)
        {
            try { await Run(operation); return true; } catch (ExecutionStateConflictException) { return false; }
        }
        var outcomes = await Task.WhenAll(
            Compete(async s => await s.CancelAsync(retry.Id, "race-cancel", default)),
            Compete(async s => await s.RecordManualQuoteAsync(retry.Id, "race-result", quote, default)));
        Assert.Single(outcomes, x => x);
        var final = (await Run(s => s.GetAsync(retry.Id, default)))!;
        Assert.Contains(final.Status, new[] { "CANCELADA", "CONCLUIDA" });
        Assert.Equal(final.Status == "CONCLUIDA" ? 2 : 1, await admin.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM erp.cotacoes").SingleAsync());
        Assert.Equal(5, await admin.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM operations.audit_entries WHERE \"Module\"='Calculations'").SingleAsync()); // snapshot + create + result + retry + race winner

        async Task<CommercialPresentationAggregate?> SavePresentation(string key, SaveCommercialPresentation input, Guid resource, bool update, RecordPermissionScope? scope = null)
        {
            await using var db = Context(user, scope); await using var transaction = await db.Database.BeginTransactionAsync(); await SetScope(db, user);
            var audit = new AuditTrailWriter(db, user);
            var store = new CanonicalPresentationStore(db, user, TimeProvider.System, audit, new CanonicalExecutionStore(db, user, TimeProvider.System, audit));
            var saved = await store.SaveAsync(resource, update, key, input, default); await transaction.CommitAsync(); return saved;
        }
        var selectionInput = new SaveCommercialPresentation { Status = "GERADA", Items = [new() { QuoteId = result.Quote.Id }] };
        var presentations = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => SavePresentation("same-presentation", selectionInput, opportunity, false)));
        Assert.Single(presentations.Select(x => x!.Presentation.Id).Distinct());
        var presentation = presentations[0]!;
        async Task<bool> Edit(int index)
        {
            try { await SavePresentation("edit-" + index, selectionInput with { ExpectedRevision = 1, Title = "Edit " + index }, presentation.Presentation.Id, true); return true; }
            catch (PresentationConflictException) { return false; }
        }
        Assert.Single(await Task.WhenAll(Enumerable.Range(0, 8).Select(Edit)), x => x);
        Assert.Equal(2, await admin.Database.SqlQuery<int>($"SELECT revision AS \"Value\" FROM infrastructure.presentation_states WHERE presentation_id={presentation.Presentation.Id}").SingleAsync());
        Assert.Equal(2, await admin.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM infrastructure.presentation_receipts").SingleAsync());
        await Assert.ThrowsAsync<PresentationConflictException>(() => SavePresentation("same-presentation", selectionInput, opportunity, false));

        // Historical FK must not expose a catalog name if a privileged repair moves the catalog row to another group.
        var otherTenant = Guid.NewGuid();
        await admin.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.tenants (id) VALUES ({otherTenant});
            UPDATE erp.seguradoras SET tenant_id={otherTenant},nome='Nome privado de outro grupo' WHERE id={insurer};
            """);
        await using (var db = Context(user))
        {
            await using var transaction = await db.Database.BeginTransactionAsync(); await SetScope(db, user);
            var audit = new AuditTrailWriter(db, user);
            var store = new CanonicalPresentationStore(db, user, TimeProvider.System, audit, new CanonicalExecutionStore(db, user, TimeProvider.System, audit));
            var historical = (await store.GetAsync(presentation.Presentation.Id, default))!;
            Assert.All(historical.Results, x => Assert.Equal("Seguradora indisponível", x.InsurerName));
            await transaction.CommitAsync();
        }

        var own = new RecordPermissionScope(); own.RestrictToOwner("comercial", Guid.NewGuid().ToString(), []);
        Assert.Null(await SavePresentation("hidden-presentation", selectionInput, opportunity, false, own));
        Assert.Null(await Run(s => s.GetAsync(execution.Id, default), scope: own));
        Assert.Null(await Run(s => s.GetQuoteAsync(result.Quote.Id, default), scope: own));
        Assert.Null(await Run(s => s.RetryAsync(execution.Id, "hidden-owner", new(), default), scope: own));
        var otherBranch = new FakeCurrentUserContext { IsAuthenticated = true, TenantId = tenant.ToString(), BranchId = Guid.NewGuid().ToString(), UserId = userId.ToString() };
        Assert.Null(await Run(s => s.GetAsync(execution.Id, default), otherBranch));
        Assert.Null(await Run(s => s.GetQuoteAsync(result.Quote.Id, default), otherBranch));
        Assert.Null(await Run(s => s.CancelAsync(execution.Id, "hidden-branch", default), otherBranch));

        var immutable = await Assert.ThrowsAsync<PostgresException>(() => admin.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.calculo_execucoes SET comissao_pct_aplicada=25 WHERE id={execution.Id}"));
        Assert.Equal(PostgresErrorCodes.CheckViolation, immutable.SqlState);
        var resultImmutable = await Assert.ThrowsAsync<PostgresException>(() => admin.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.cotacoes SET premio_total=999 WHERE id={result.Quote.Id}"));
        Assert.Equal(PostgresErrorCodes.CheckViolation, resultImmutable.SqlState);
        await Assert.ThrowsAsync<ValidationException>(() => Run(s => s.RecordManualQuoteAsync(execution.Id, "invalid-array", quote with { Coverages = [null!] }, default)));
    }

    private sealed class LookupClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 10, 12, 1, 30, 0, TimeSpan.Zero);
    }
}
