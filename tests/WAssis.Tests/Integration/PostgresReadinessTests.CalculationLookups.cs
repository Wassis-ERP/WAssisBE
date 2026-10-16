using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Calculations;
using WAssis.Infra.Data.Context;

namespace WAssis.Tests.Integration;

public sealed partial class PostgresReadinessTests
{
    private static async Task VerifyCalculationLookupsHttp(WAssisDbContext db, HttpClient client, Guid opportunityId, Guid calculationId)
    {
        var calculation = (await client.GetFromJsonAsync<CalculationAggregate>($"/api/calculos/{calculationId}"))!;
        var line = calculation.Calculation.InsuranceBranchId;
        var foreignLine = Guid.NewGuid(); var inactiveInsurer = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.ramos (id,tenant_id,forma_calculo,ativo) VALUES ({foreignLine},{Guid.Parse(TenantB)},'AUTO',true);
            INSERT INTO erp.seguradoras (id,tenant_id,nome,ativo) VALUES ({inactiveInsurer},{Guid.Parse(TenantA)},'Inativa fixture',false);
            UPDATE erp.role_permissions SET can_read=false WHERE perfil_id={Guid.Parse(ProfileA)} AND modulo='configuracoes';
            """);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/core/catalogs/seguradoras")).StatusCode);
        var insurersPath = $"/api/oportunidades/{opportunityId}/calculos/lookups/seguradoras";
        using var response = await client.GetAsync(insurersPath + "?pageSize=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var insurers = (await response.Content.ReadFromJsonAsync<PagedResult<CalculationInsurerOption>>())!;
        Assert.Single(insurers.Items); Assert.True(insurers.HasMore); Assert.Equal(1, insurers.PageSize);
        Assert.All(insurers.Items, x => Assert.Equal(Guid.Parse(TenantA), x.TenantId));
        var raw = await client.GetStringAsync(insurersPath + "?pageSize=100");
        Assert.DoesNotContain("cnpj", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("credencial", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Inativa fixture", raw);
        var second = (await client.GetFromJsonAsync<PagedResult<CalculationInsurerOption>>(insurersPath + "?page=2&pageSize=1"))!;
        Assert.NotEqual(insurers.Items.Single().Id, second.Items.Single().Id);
        var search = (await client.GetFromJsonAsync<PagedResult<CalculationInsurerOption>>(insurersPath + "?search=Manual%20HTTP"))!;
        Assert.Single(search.Items); Assert.Equal("Manual HTTP", search.Items.Single().Name);

        var coversPath = $"/api/oportunidades/{opportunityId}/calculos/lookups/coberturas?ramoId={line}";
        var coverages = (await client.GetFromJsonAsync<PagedResult<CalculationCoverageOption>>(coversPath))!;
        Assert.Equal(2, coverages.TotalCount); Assert.All(coverages.Items, x => Assert.Equal(line, x.InsuranceBranchId));
        Assert.All(coverages.Items, x => Assert.True(x.Active));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync(coversPath.Replace(line.ToString(), foreignLine.ToString()))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync(insurersPath + "?search=" + new string('x', 101))).StatusCode);
        var contextPath = $"/api/oportunidades/{opportunityId}/calculos/contexto?ramoId={line}";
        var context = (await client.GetFromJsonAsync<CalculationEditorContext>(contextPath))!;
        Assert.Equal("America/Sao_Paulo", context.TimeZone); Assert.True(CalculationCalendar.OptionalDate(context.BranchDate));
        Assert.Equal(line, context.InsuranceBranchId); Assert.Equal(Guid.Parse(BranchA), context.BranchId);
        Assert.Equal("Origem HTTP", context.SourcePolicy!.Number); Assert.Equal("2028-02-29", context.SourcePolicy.CoverageEndsOn);
        Assert.Equal(calculation.Calculation.InsuredPersonId, context.InsuredPersonId);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.tenants SET timezone='Invalid/Fixture' WHERE id={Guid.Parse(TenantA)}");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync(contextPath)).StatusCode);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE erp.tenants SET timezone='America/Sao_Paulo' WHERE id={Guid.Parse(TenantA)};
            UPDATE erp.role_permissions SET can_read=true WHERE perfil_id={Guid.Parse(ProfileA)} AND modulo='configuracoes';
            """);
    }
}
