using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Calculations;
using WAssis.Infra.Data.Context;

namespace WAssis.Tests.Integration;

public sealed partial class PostgresReadinessTests
{
    private static async Task<Guid> VerifyCanonicalCalculationHttp(WAssisDbContext db, HttpClient client, Guid opportunityId, Guid insuredId)
    {
        var line = Guid.NewGuid(); var coverageA = Guid.NewGuid(); var coverageB = Guid.NewGuid(); var insurer = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE erp.tenants SET timezone='America/Sao_Paulo' WHERE id={Guid.Parse(TenantA)};
            INSERT INTO erp.ramos (id,tenant_id,nome,forma_calculo,ativo) VALUES ({line},{Guid.Parse(TenantA)},'Auto HTTP','AUTO',true);
            INSERT INTO erp.seguradoras (id,tenant_id,nome,ativo) VALUES ({insurer},{Guid.Parse(TenantA)},'Seguradora HTTP',true);
            INSERT INTO erp.coberturas_catalogo (id,ramo_id,codigo,nome,ativo) VALUES
              ({coverageA},{line},'danos-materiais','Danos materiais',true),({coverageB},{line},'casco','Casco',true);
            """);
        var input = new CreateAutoCalculation
        {
            OpportunityId = opportunityId, InsuranceBranchId = line, InsuredPersonId = insuredId,
            InternalNote = "Nota interna não transmitida", Specialization = new() { Data = new() { DriverName = "Condutor A", DriverBirthDate = "1990-01-01", Model = "Ká", ChassisRemarked = null } },
            Coverages = [new() { CoverageId = coverageA, Selected = true, RequestedLimit = 100000 },
                new() { CoverageId = coverageB, Selected = true, RequestedFipePercentage = 100, RequestedDeductibleType = "normal" }]
        };
        async Task<HttpResponseMessage> Create(string key, CreateAutoCalculation body)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/oportunidades/{opportunityId}/calculos") { Content = JsonContent.Create(body) };
            request.Headers.Add("Idempotency-Key", key);
            return await client.SendAsync(request);
        }
        using var created = await Create("calc-first", input);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var first = (await created.Content.ReadFromJsonAsync<CalculationAggregate>())!;
        Assert.Equal(opportunityId, first.Calculation.OpportunityId);
        Assert.Equal(2, first.Coverages.Count);
        Assert.Equal("Condutor A", first.Specialization.Data.DriverName);
        Assert.Null(first.Specialization.Data.ChassisRemarked);
        Assert.Equal(CalculationCalendar.EndOfAnnualTerm(first.Calculation.CoverageStartsOn!), first.Calculation.CoverageEndsOn);
        using var retry = await Create("calc-first", input with { Coverages = input.Coverages.Reverse().ToArray() });
        Assert.Equal(HttpStatusCode.Created, retry.StatusCode);
        Assert.Equal(first.Calculation.Id, (await retry.Content.ReadFromJsonAsync<CalculationAggregate>())!.Calculation.Id);
        var changed = input with
        {
            Specialization = input.Specialization with { Data = input.Specialization.Data with { DriverName = "Condutor B" } },
            Coverages = [input.Coverages[0] with { RequestedLimit = 200000 }, input.Coverages[1]]
        };
        using var conflict = await Create("calc-first", changed);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        using var second = await Create("calc-second", changed);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.NotEqual(first.Calculation.Id, (await second.Content.ReadFromJsonAsync<CalculationAggregate>())!.Calculation.Id);
        var stored = (await client.GetFromJsonAsync<CalculationAggregate>($"/api/calculos/{first.Calculation.Id}"))!;
        Assert.Equal("Condutor A", stored.Specialization.Data.DriverName);
        Assert.Equal(100000, stored.Coverages.Single(x => x.CoverageId == coverageA).RequestedLimit);
        Assert.Equal(3, (await client.GetFromJsonAsync<WAssis.Application.Abstractions.PagedResult<CalculationAggregate>>($"/api/oportunidades/{opportunityId}/calculos"))!.TotalCount); // Includes the canonical FK fixture.
        using var unsupported = await Create("bad-form", input with { Specialization = input.Specialization with { Form = "VIDA" } });
        Assert.Equal(HttpStatusCode.BadRequest, unsupported.StatusCode);
        using var badCoverage = await Create("bad-coverage", input with { Coverages = [new() { CoverageId = Guid.NewGuid(), Selected = true }] });
        Assert.Equal(HttpStatusCode.BadRequest, badCoverage.StatusCode);
        Assert.Equal(3, await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM erp.calculos WHERE oportunidade_id={opportunityId}").SingleAsync());
        var renewal = input with
        {
            InsuranceType = "RENOVACAO_OUTRA", PreviousInsurerId = insurer, PreviousPolicyNumber = "Anterior HTTP",
            PreviousCoverageEndsOn = "2028-02-29", CoverageStartsOn = "2028-02-29", CoverageEndsOn = "2029-02-28", Bonus = 0
        };
        using var renewalResponse = await Create("calc-renewal", renewal);
        Assert.Equal(HttpStatusCode.Created, renewalResponse.StatusCode);
        var renewed = (await renewalResponse.Content.ReadFromJsonAsync<CalculationAggregate>())!;
        Assert.Equal("2029-02-28", renewed.Calculation.CoverageEndsOn);
        Assert.Equal(0, renewed.Calculation.Bonus);

        // Failure after header/risk insertion must roll back the entire aggregate and receipt.
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp.calculo_coberturas ADD CONSTRAINT ck_http_rollback_fixture CHECK (quantidade_solicitada IS DISTINCT FROM 123456789)");
        using var rolledBack = await Create("calc-rollback", input with { Coverages = [input.Coverages[0] with { RequestedQuantity = 123456789 }] });
        Assert.Equal(HttpStatusCode.Conflict, rolledBack.StatusCode);
        Assert.Equal(4, await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM erp.calculos WHERE oportunidade_id={opportunityId}").SingleAsync());
        Assert.Equal(0, await db.Database.SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM infrastructure.calculation_receipts WHERE opportunity_id={opportunityId} AND idempotency_key='calc-rollback'").SingleAsync());

        var sourcePolicy = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.apolices (id,segurado_id,seguradora_id,ramo_id,status,numero_apolice,vigencia_fim)
            VALUES ({sourcePolicy},{insuredId},{insurer},{line},'VIGENTE','Origem HTTP','2028-02-29');
            UPDATE erp.oportunidades SET apolice_origem_id={sourcePolicy} WHERE id={opportunityId};
            """);
        using var ownRenewal = await Create("calc-own-renewal", input with { InsuranceType = "RENOVACAO_PROPRIA", Bonus = 0 });
        Assert.Equal(HttpStatusCode.Created, ownRenewal.StatusCode);
        var own = (await ownRenewal.Content.ReadFromJsonAsync<CalculationAggregate>())!;
        Assert.Equal("2028-02-29", own.Calculation.CoverageStartsOn);
        Assert.Equal("2029-02-28", own.Calculation.CoverageEndsOn);
        Assert.Equal("Origem HTTP", own.Calculation.PreviousPolicyNumber);
        Assert.Equal(insurer, own.Calculation.PreviousInsurerId);
        using var spoofedPolicy = await Create("calc-own-spoof", input with { InsuranceType = "RENOVACAO_PROPRIA", PreviousPolicyNumber = "Outra apólice" });
        Assert.Equal(HttpStatusCode.BadRequest, spoofedPolicy.StatusCode);
        return first.Calculation.Id;
    }
}
