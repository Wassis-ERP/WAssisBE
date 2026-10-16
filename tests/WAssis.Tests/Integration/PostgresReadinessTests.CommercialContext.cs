using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Opportunities.Interfaces;
using WAssis.Infra.Data.Context;

namespace WAssis.Tests.Integration;

public sealed partial class PostgresReadinessTests
{
    private static async Task VerifyOpportunityReadContextHttp(WAssisDbContext db, HttpClient client, Guid opportunityId, Guid line)
    {
        var path = $"/api/oportunidades/{opportunityId}/contexto-comercial";
        var unlinked = (await client.GetFromJsonAsync<OpportunityReadContext>(path))!;
        Assert.Null(unlinked.InsuranceBranch);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.oportunidades SET ramo_id={line} WHERE id={opportunityId}");
        var context = (await client.GetFromJsonAsync<OpportunityReadContext>(path))!;
        Assert.Equal(opportunityId, context.OpportunityId);
        Assert.Equal(line, context.InsuranceBranch!.Id);
        Assert.Equal("AUTO", context.InsuranceBranch.CalculationForm);
        Assert.Equal(Guid.Parse("66666666-6666-6666-6666-666666666667"), context.Stage!.Id);
        Assert.True(context.Stage.CanWin);
        Assert.Null(context.Origin); Assert.Null(context.LossReason);
        using (var json = await client.GetFromJsonAsync<JsonDocument>(path))
        {
            Assert.Equal(5, json!.RootElement.EnumerateObject().Count());
            Assert.Equal(5, json.RootElement.GetProperty("insuranceBranch").EnumerateObject().Count());
            Assert.Equal(12, json.RootElement.GetProperty("stage").EnumerateObject().Count());
            Assert.False(json.RootElement.TryGetProperty("insuredPerson", out _));
            Assert.False(json.RootElement.TryGetProperty("profiles", out _));
        }
        var origin = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.origens (id,tenant_id,nome,ativo) VALUES ({origin},{Guid.Parse(TenantA)},'Origem histórica fixture',false);
            UPDATE erp.oportunidades SET origem_id={origin} WHERE id={opportunityId};
            UPDATE erp.ramos SET ativo=false WHERE id={line};
            UPDATE erp.pipeline_stages SET ativo=false WHERE id={context.Stage.Id};
            """);
        var historical = (await client.GetFromJsonAsync<OpportunityReadContext>(path))!;
        Assert.Equal(line, historical.InsuranceBranch!.Id);
        Assert.Equal(origin, historical.Origin!.Id);
        Assert.Equal("Origem histórica fixture", historical.Origin.Name);
        Assert.False(historical.Stage!.Active);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/oportunidades/{Guid.NewGuid()}/contexto-comercial")).StatusCode);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE erp.ramos SET ativo=true WHERE id={line};
            UPDATE erp.pipeline_stages SET ativo=true WHERE id={context.Stage.Id};
            UPDATE erp.oportunidades SET origem_id=NULL,ramo_id=NULL WHERE id={opportunityId};
            """);
    }
}
