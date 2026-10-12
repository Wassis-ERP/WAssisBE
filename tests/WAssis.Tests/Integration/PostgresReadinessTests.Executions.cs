using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Calculations;
using WAssis.Infra.Data.Context;

namespace WAssis.Tests.Integration;

public sealed partial class PostgresReadinessTests
{
    private static async Task<(Guid Execution, Guid Quote)> VerifyCanonicalManualExecutionHttp(WAssisDbContext db, HttpClient client, Guid opportunityId, Guid calculationId)
    {
        var insurer = Guid.NewGuid(); var foreignInsurer = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.seguradoras (id,tenant_id,nome,ativo) VALUES
            ({insurer},{Guid.Parse(TenantA)},'Manual HTTP',true),({foreignInsurer},{Guid.Parse(TenantB)},'Outro grupo',true);
            """);
        async Task<HttpResponseMessage> Post(string path, string key, object body)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) };
            request.Headers.Add("Idempotency-Key", key);
            return await client.SendAsync(request);
        }
        var executionPath = $"/api/calculos/{calculationId}/execucoes";
        var selection = new CreateCalculationExecution { InsurerId = insurer, CommissionOverride = 0 };
        using var unavailableEngine = await Post(executionPath, "no-engine", selection with { Engine = "AGGER" });
        Assert.Equal(HttpStatusCode.BadRequest, unavailableEngine.StatusCode);
        using var foreign = await Post(executionPath, "foreign-insurer", selection with { InsurerId = foreignInsurer });
        Assert.Equal(HttpStatusCode.BadRequest, foreign.StatusCode);
        using var created = await Post(executionPath, "manual-first", selection);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var first = (await created.Content.ReadFromJsonAsync<CalculationExecutionRow>())!;
        Assert.Equal(1, first.Attempt); Assert.Equal(0, first.AppliedCommission); Assert.Equal("SOBRESCRITA", first.CommissionOrigin);
        Assert.Equal("AGUARDANDO", first.Status); Assert.Null(first.RetryOfId);
        using var replay = await Post(executionPath, "manual-first", selection with { CommissionOverride = 0.00m });
        Assert.Equal(first.Id, (await replay.Content.ReadFromJsonAsync<CalculationExecutionRow>())!.Id);
        using var duplicate = await Post(executionPath, "manual-different", selection);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        using var keyConflict = await Post(executionPath, "manual-first", selection with { CommissionOverride = 15 });
        Assert.Equal(HttpStatusCode.Conflict, keyConflict.StatusCode);
        using var prematureRetry = await Post($"/api/execucoes/{first.Id}/reexecutar", "premature", new RetryCalculationExecution());
        Assert.Equal(HttpStatusCode.Conflict, prematureRetry.StatusCode);

        var quotePath = $"/api/execucoes/{first.Id}/cotacao-manual";
        var quote = new RecordManualQuote
        {
            TotalPremium = 1234.50m, NetPremium = null, Tax = 0, ValidUntil = "2028-02-29",
            Coverages = [new() { ResultKey = "external-casco", ReportedName = "Casco informado", AcceptedLimit = 0, DeductibleAmount = null },
                new() { ResultKey = "external-extra", Included = null }],
            Installments = [new() { OptionCode = "boleto", PaymentMethod = "BOLETO", InstallmentCount = 1, TotalAmount = 1234.50m, Primary = true },
                new() { OptionCode = "cartao", PaymentMethod = "CARTAO", InstallmentCount = 10, InstallmentAmount = null }]
        };
        using var invalid = await Post(quotePath, "invalid-quote", quote with { Coverages = [new() { ResultKey = "bad", CoverageId = Guid.NewGuid() }] });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(0, await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM erp.cotacoes WHERE execucao_id={first.Id}").SingleAsync());
        // Inject a late INSERT failure to prove quote + coverages + execution + receipt rollback together.
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp.cotacao_parcelamentos ADD CONSTRAINT ck_manual_rollback_fixture CHECK (codigo_opcao<>'rollback-fixture')");
        using var rolledBack = await Post(quotePath, "rollback-quote", quote with { Installments = [quote.Installments[0] with { OptionCode = "rollback-fixture" }] });
        Assert.Equal(HttpStatusCode.Conflict, rolledBack.StatusCode);
        Assert.Equal(0, await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM erp.cotacoes WHERE execucao_id={first.Id}").SingleAsync());
        Assert.Equal("AGUARDANDO", (await client.GetFromJsonAsync<CalculationExecutionRow>($"/api/execucoes/{first.Id}"))!.Status);
        Assert.Equal(0, await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM infrastructure.execution_receipts WHERE resource_id={first.Id} AND idempotency_key='rollback-quote'").SingleAsync());
        using var result = await Post(quotePath, "manual-result", quote);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        var stored = (await result.Content.ReadFromJsonAsync<QuoteResultAggregate>())!;
        Assert.Equal("CONCLUIDA", stored.Execution.Status); Assert.Equal("APRESENTADA", stored.Quote.Status);
        Assert.Null(stored.Quote.NetPremium); Assert.Equal(0, stored.Quote.Tax); Assert.Equal(2, stored.Coverages.Count);
        Assert.Equal(2, stored.Installments.Count); Assert.Null(stored.Coverages.Single(x => x.ResultKey == "external-casco").DeductibleAmount);
        Assert.Equal("2028-02-29", stored.Quote.ValidUntil);
        using var quoteReplay = await Post(quotePath, "manual-result", quote with { TotalPremium = 1234.500m, Coverages = quote.Coverages.Reverse().ToArray(), Installments = quote.Installments.Reverse().ToArray() });
        Assert.Equal(stored.Quote.Id, (await quoteReplay.Content.ReadFromJsonAsync<QuoteResultAggregate>())!.Quote.Id);
        using var overwritten = await Post(quotePath, "manual-new-result", quote);
        Assert.Equal(HttpStatusCode.Conflict, overwritten.StatusCode);
        using var cancelCompleted = await Post($"/api/execucoes/{first.Id}/cancelar", "bad-cancel", new { });
        Assert.Equal(HttpStatusCode.Conflict, cancelCompleted.StatusCode);
        using var retried = await Post($"/api/execucoes/{first.Id}/reexecutar", "manual-retry", new RetryCalculationExecution());
        var second = (await retried.Content.ReadFromJsonAsync<CalculationExecutionRow>())!;
        Assert.Equal(HttpStatusCode.Created, retried.StatusCode); Assert.Equal(2, second.Attempt); Assert.Equal(first.Id, second.RetryOfId);
        Assert.Equal("PADRAO_CALCULO", second.CommissionOrigin); Assert.Null(second.AppliedCommission);
        using var cancelled = await Post($"/api/execucoes/{second.Id}/cancelar", "manual-cancel", new { });
        Assert.Equal("CANCELADA", (await cancelled.Content.ReadFromJsonAsync<CalculationExecutionRow>())!.Status);
        using var cancelledReplay = await Post($"/api/execucoes/{second.Id}/cancelar", "manual-cancel", new { });
        Assert.Equal(HttpStatusCode.OK, cancelledReplay.StatusCode);
        Assert.Equal(2, (await client.GetFromJsonAsync<PagedResult<CalculationExecutionRow>>(executionPath))!.TotalCount);
        Assert.Equal(1, (await client.GetFromJsonAsync<PagedResult<QuoteResultAggregate>>($"/api/oportunidades/{opportunityId}/cotacoes"))!.TotalCount);
        Assert.Equal(stored.Quote.Id, (await client.GetFromJsonAsync<QuoteResultAggregate>($"/api/cotacoes/{stored.Quote.Id}"))!.Quote.Id);
        Assert.Equal(100000, (await client.GetFromJsonAsync<CalculationAggregate>($"/api/calculos/{calculationId}"))!.Coverages.Single(x => x.RequestedLimit.HasValue).RequestedLimit);
        return (first.Id, stored.Quote.Id);
    }
}
