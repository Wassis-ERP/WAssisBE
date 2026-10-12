using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Calculations;
using WAssis.Infra.Data.Context;

namespace WAssis.Tests.Integration;

public sealed partial class PostgresReadinessTests
{
    private static async Task<Guid> VerifyCanonicalPresentationHttp(WAssisDbContext db, HttpClient client, Guid opportunityId, Guid calculationId, Guid firstQuoteId)
    {
        async Task<HttpResponseMessage> Write(HttpMethod method, string path, string key, object body)
        {
            using var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) };
            request.Headers.Add("Idempotency-Key", key); return await client.SendAsync(request);
        }
        var secondCalculationId = await db.Database.SqlQuery<Guid>($"SELECT a.calculo_id AS \"Value\" FROM erp.calc_auto a JOIN erp.calculos c ON c.id=a.calculo_id WHERE c.oportunidade_id={opportunityId} AND a.condutor_nome='Condutor B'").SingleAsync();
        var quotes = new List<QuoteResultAggregate> { (await client.GetFromJsonAsync<QuoteResultAggregate>($"/api/cotacoes/{firstQuoteId}"))! };
        for (var i = 1; i <= 5; i++)
        {
            var insurer = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO erp.seguradoras (id,tenant_id,nome,ativo) VALUES ({insurer},{Guid.Parse(TenantA)},{"Comparativo fixture " + i},true)");
            var calculation = i == 1 ? secondCalculationId : calculationId;
            using var executionResponse = await Write(HttpMethod.Post, $"/api/calculos/{calculation}/execucoes", "comparison-exec-" + i, new CreateCalculationExecution { InsurerId = insurer });
            Assert.Equal(HttpStatusCode.Created, executionResponse.StatusCode);
            var execution = (await executionResponse.Content.ReadFromJsonAsync<CalculationExecutionRow>())!;
            using var resultResponse = await Write(HttpMethod.Post, $"/api/execucoes/{execution.Id}/cotacao-manual", "comparison-result-" + i, new RecordManualQuote
            {
                TotalPremium = (6 - i) * 10, Coverages = [new() { ResultKey = "casco", DeductibleAmount = i == 1 ? null : i * 100 }],
                Installments = [new() { OptionCode = "boleto", PaymentMethod = "BOLETO", InstallmentCount = 1, Primary = true }]
            });
            Assert.Equal(HttpStatusCode.Created, resultResponse.StatusCode);
            quotes.Add((await resultResponse.Content.ReadFromJsonAsync<QuoteResultAggregate>())!);
        }
        var path = $"/api/oportunidades/{opportunityId}/apresentacoes";
        var input = new SaveCommercialPresentation
        {
            Title = "Comparativo HTTP", Ordering = "PREMIO", Status = "GERADA", SelectedQuoteId = firstQuoteId,
            Items = [new() { QuoteId = firstQuoteId, InstallmentId = quotes[0].Installments.First().Id },
                new() { QuoteId = quotes[1].Quote.Id, Recommended = true, InstallmentId = quotes[1].Installments.Single().Id }]
        };
        using var created = await Write(HttpMethod.Post, path, "comparison-first", input);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var first = (await created.Content.ReadFromJsonAsync<CommercialPresentationAggregate>())!;
        Assert.Equal(1, first.Revision); Assert.Equal(2, first.Results.Count);
        Assert.Equal(quotes[1].Quote.Id, first.Items.First().QuoteId); Assert.Equal(new[] { 10, 20 }, first.Items.Select(x => x.Order));
        Assert.Equal(2, first.Results.Select(x => x.Calculation.Id).Distinct().Count());
        Assert.Equal(firstQuoteId, first.Presentation.SelectedQuoteId);
        Assert.Equal("APRESENTADA", (await client.GetFromJsonAsync<QuoteResultAggregate>($"/api/cotacoes/{firstQuoteId}"))!.Quote.Status);
        using var replay = await Write(HttpMethod.Post, path, "comparison-first", input);
        Assert.Equal(first.Presentation.Id, (await replay.Content.ReadFromJsonAsync<CommercialPresentationAggregate>())!.Presentation.Id);
        using var tooMany = await Write(HttpMethod.Post, path, "comparison-too-many", input with { Items = quotes.Select(x => new PresentationQuoteInput { QuoteId = x.Quote.Id }).ToArray() });
        Assert.Equal(HttpStatusCode.BadRequest, tooMany.StatusCode);
        using var wrongInstallment = await Write(HttpMethod.Post, path, "comparison-installment", input with { Items = [input.Items[0] with { InstallmentId = quotes[1].Installments.Single().Id }] });
        Assert.Equal(HttpStatusCode.BadRequest, wrongInstallment.StatusCode);
        using var five = await Write(HttpMethod.Post, path, "comparison-five", input with { Items = quotes.Take(5).Select(x => new PresentationQuoteInput { QuoteId = x.Quote.Id }).ToArray() });
        Assert.Equal(HttpStatusCode.Created, five.StatusCode);
        var fiveSaved = (await five.Content.ReadFromJsonAsync<CommercialPresentationAggregate>())!;
        Assert.Equal(5, fiveSaved.Items.Count);
        var limitViolation = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.apresentacao_cotacoes (id,apresentacao_id,cotacao_id,ordem)
            VALUES ({Guid.NewGuid()},{fiveSaved.Presentation.Id},{quotes[5].Quote.Id},60)
            """));
        Assert.Equal(PostgresErrorCodes.CheckViolation, limitViolation.SqlState);

        var otherOpportunity = Guid.NewGuid(); var otherCalculation = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.oportunidades (id,tenant_id,filial_id,responsavel_id,stage_id,titulo)
              SELECT {otherOpportunity},tenant_id,filial_id,{Guid.Parse(UserBranchB)},stage_id,'Outra oportunidade do mesmo grupo' FROM erp.oportunidades WHERE id={opportunityId};
            INSERT INTO erp.calculos (id,oportunidade_id,ramo_id,origem,criado_em)
              SELECT {otherCalculation},{otherOpportunity},ramo_id,'MANUAL',now() FROM erp.calculos WHERE id={calculationId};
            INSERT INTO erp.calc_auto (calculo_id) VALUES ({otherCalculation});
            """);
        using var otherExecutionResponse = await Write(HttpMethod.Post, $"/api/calculos/{otherCalculation}/execucoes", "other-opportunity-exec", new CreateCalculationExecution { InsurerId = quotes[0].Execution.InsurerId });
        var otherExecution = (await otherExecutionResponse.Content.ReadFromJsonAsync<CalculationExecutionRow>())!;
        using var otherQuoteResponse = await Write(HttpMethod.Post, $"/api/execucoes/{otherExecution.Id}/cotacao-manual", "other-opportunity-result", new RecordManualQuote { TotalPremium = 99 });
        var otherQuote = (await otherQuoteResponse.Content.ReadFromJsonAsync<QuoteResultAggregate>())!;
        using var wrongOpportunity = await Write(HttpMethod.Post, path, "comparison-wrong-opportunity", input with { SelectedQuoteId = null, Items = [new() { QuoteId = otherQuote.Quote.Id }] });
        Assert.Equal(HttpStatusCode.BadRequest, wrongOpportunity.StatusCode);
        var scopeViolation = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.apresentacao_cotacoes (id,apresentacao_id,cotacao_id,ordem)
            VALUES ({Guid.NewGuid()},{first.Presentation.Id},{otherQuote.Quote.Id},30)
            """));
        Assert.Equal(PostgresErrorCodes.CheckViolation, scopeViolation.SqlState);

        var updatePath = $"/api/apresentacoes/{first.Presentation.Id}";
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp.apresentacao_cotacoes ADD CONSTRAINT ck_presentation_rollback_fixture CHECK (vantagens_texto IS DISTINCT FROM 'rollback-fixture')");
        using var failedUpdate = await Write(HttpMethod.Put, updatePath, "comparison-rollback", input with { ExpectedRevision = 1, Title = "Falha não persistida", Items = [input.Items[0] with { Advantages = "rollback-fixture" }] });
        Assert.Equal(HttpStatusCode.Conflict, failedUpdate.StatusCode);
        var unchanged = (await client.GetFromJsonAsync<CommercialPresentationAggregate>(updatePath))!;
        Assert.Equal(1, unchanged.Revision); Assert.Equal(input.Title, unchanged.Presentation.Title); Assert.Equal(2, unchanged.Items.Count);
        var edit = input with { ExpectedRevision = 1, Layout = "HORIZONTAL", Ordering = "MANUAL", Title = "Editado", Items = input.Items.Reverse().ToArray() };
        using var updated = await Write(HttpMethod.Put, updatePath, "comparison-edit", edit);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var second = (await updated.Content.ReadFromJsonAsync<CommercialPresentationAggregate>())!;
        Assert.Equal(2, second.Revision); Assert.Equal("HORIZONTAL", second.Presentation.Layout);
        using var editReplay = await Write(HttpMethod.Put, updatePath, "comparison-edit", edit);
        Assert.Equal(2, (await editReplay.Content.ReadFromJsonAsync<CommercialPresentationAggregate>())!.Revision);
        using var stale = await Write(HttpMethod.Put, updatePath, "comparison-stale", input with { ExpectedRevision = 1 });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        using var oldCreateReplay = await Write(HttpMethod.Post, path, "comparison-first", input);
        Assert.Equal(HttpStatusCode.Conflict, oldCreateReplay.StatusCode);
        Assert.Equal(2, (await client.GetFromJsonAsync<PagedResult<CommercialPresentationAggregate>>(path))!.TotalCount);
        Assert.Equal(0, await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM infrastructure.presentation_receipts WHERE resource_id={first.Presentation.Id} AND idempotency_key='comparison-rollback'").SingleAsync());
        return first.Presentation.Id;
    }
}
