using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WAssis.Infra.Data.Context;
using WAssis.Application.Abstractions;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Integration;

public sealed partial class PostgresReadinessTests
{
    private static async Task VerifyOwnScopeAndLivePermissions(WAssisDbContext db, HttpClient client, Guid insuredId, Guid opportunityId)
    {
        var otherOpportunity = Guid.NewGuid();
        var otherInsured = Guid.NewGuid();
        var producer = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.produtores (id,tenant_id,profile_id,ativo) VALUES ({producer},{Guid.Parse(TenantA)},{Guid.Parse(UserA)},true);
            UPDATE erp.segurados SET produtor_id = {producer} WHERE id = {insuredId};
            UPDATE erp.oportunidades SET responsavel_id = {Guid.Parse(UserA)} WHERE id = {opportunityId};
            INSERT INTO erp.segurados (id,tenant_id,filial_id,nome,tipo,status,lgpd_autorizado,created_at,updated_at)
            VALUES ({otherInsured},{Guid.Parse(TenantA)},{Guid.Parse(BranchA)},'Outro produtor','PF','Prospecto',false,now(),now());
            INSERT INTO erp.oportunidades (id,tenant_id,filial_id,titulo,responsavel_id,stage_id,created_at,updated_at)
            VALUES ({otherOpportunity},{Guid.Parse(TenantA)},{Guid.Parse(BranchA)},'Outro responsável',{Guid.Parse(UserBranchB)},'66666666-6666-6666-6666-666666666666',now(),now());
            UPDATE erp.role_permissions SET escopo = 'PROPRIO' WHERE perfil_id = {Guid.Parse(ProfileA)};
            """);

        // The existing cookie must immediately obey the changed database permissions.
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/segurados/{insuredId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/segurados/{otherInsured}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/oportunidades/{opportunityId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/oportunidades/{otherOpportunity}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/oportunidades/{opportunityId}/contexto-comercial")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/oportunidades/{otherOpportunity}/contexto-comercial")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/oportunidades/{opportunityId}/calculos/lookups/seguradoras")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/oportunidades/{otherOpportunity}/calculos/lookups/seguradoras")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/oportunidades/{otherOpportunity}/calculos/lookups/seguradoras?includeReferencedInactive=true")).StatusCode);
        using var opportunities = await client.GetFromJsonAsync<JsonDocument>("/api/oportunidades");
        Assert.Equal(opportunityId, Assert.Single(opportunities!.RootElement.GetProperty("items").EnumerateArray()).GetProperty("id").GetGuid());
        using var insuredPeople = await client.GetFromJsonAsync<JsonDocument>("/api/segurados");
        Assert.Equal(insuredId, Assert.Single(insuredPeople!.RootElement.GetProperty("items").EnumerateArray()).GetProperty("id").GetGuid());

        var owned = (await client.GetFromJsonAsync<Dictionary<string, JsonElement>>($"/api/oportunidades/{opportunityId}"))!;
        owned["responsibleId"] = JsonSerializer.SerializeToElement(UserBranchB);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsJsonAsync($"/api/oportunidades/{opportunityId}", owned)).StatusCode);
        using var stillOwned = await client.GetFromJsonAsync<JsonDocument>($"/api/oportunidades/{opportunityId}");
        Assert.Equal(UserA, stillOwned!.RootElement.GetProperty("responsibleId").GetString());

        await VerifyConcurrentOwnershipChange(db, insuredId, opportunityId, producer);

        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.role_permissions SET can_read = false WHERE perfil_id = {Guid.Parse(ProfileA)}");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/segurados/{insuredId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/oportunidades/{opportunityId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"/api/oportunidades/{opportunityId}/contexto-comercial")).StatusCode);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.role_permissions SET can_read = true, escopo = 'CORRETORA' WHERE perfil_id = {Guid.Parse(ProfileA)}");
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/oportunidades/{otherOpportunity}")).StatusCode);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.profile_filiais (id,profile_id,filial_id,perfil_id,principal,ativo)
            VALUES (gen_random_uuid(),{Guid.Parse(UserA)},{Guid.Parse(BranchB)},{Guid.Parse(ProfileA)},false,true)
            """);
        client.DefaultRequestHeaders.Add("X-Office-Branch-Id", BranchB);
        await AssertIdentityBranches(client, BranchA, BranchB);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/segurados")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/segurados/{insuredId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/oportunidades/{opportunityId}/contexto-comercial")).StatusCode);
        using var selected = await client.GetFromJsonAsync<JsonDocument>("/api/identity/me");
        Assert.Equal(BranchB, selected!.RootElement.GetProperty("branchId").GetString());
        await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM erp.profile_filiais WHERE profile_id = {Guid.Parse(UserA)} AND filial_id = {Guid.Parse(BranchB)}");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/segurados")).StatusCode);
        client.DefaultRequestHeaders.Remove("X-Office-Branch-Id");
        await AssertIdentityBranches(client, BranchA);

        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.profile_filiais SET ativo = false WHERE profile_id = {Guid.Parse(UserA)}");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/identity/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/identity/me/branches")).StatusCode);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.profile_filiais SET ativo = true WHERE profile_id = {Guid.Parse(UserA)}");
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/identity/me")).StatusCode);
    }

    private static async Task AssertIdentityBranches(HttpClient client, params string[] expected)
    {
        using var response = await client.GetAsync("/api/identity/me/branches");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("documentNumber", body);
        using var json = JsonDocument.Parse(body);
        var ids = json.RootElement.EnumerateArray().Select(row => row.GetProperty("id").GetString()).Order().ToArray();
        Assert.Equal(expected.Order().ToArray(), ids);
        Assert.All(json.RootElement.EnumerateArray(), row => Assert.True(row.GetProperty("isActive").GetBoolean()));
    }

    private static async Task VerifyConcurrentOwnershipChange(WAssisDbContext admin, Guid insuredId, Guid opportunityId, Guid producer)
    {
        var scope = new RecordPermissionScope();
        var user = new FakeCurrentUserContext
        { IsAuthenticated = true, UserId = UserA, TenantId = TenantA, BranchId = BranchA, BranchIds = [BranchA] };
        var options = new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(admin.Database.GetConnectionString()).Options;
        scope.RestrictToOwner("comercial", UserA, []);
        await using (var stale = new WAssisDbContext(options, user, recordScope: scope))
        {
            var opportunity = await stale.Opportunities.SingleAsync(x => x.Id == opportunityId);
            await admin.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.oportunidades SET responsavel_id = {Guid.Parse(UserBranchB)} WHERE id = {opportunityId}");
            stale.Entry(opportunity).Property(x => x.Title).CurrentValue = "Stale overwrite";
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => stale.SaveChangesAsync());
        }
        await admin.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.oportunidades SET responsavel_id = {Guid.Parse(UserA)} WHERE id = {opportunityId}");
        scope = new RecordPermissionScope();
        scope.RestrictToOwner("segurados", UserA, [producer.ToString()]);
        await using (var stale = new WAssisDbContext(options, user, recordScope: scope))
        {
            var insured = await stale.InsuredPeople.SingleAsync(x => x.Id == insuredId);
            await admin.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.segurados SET produtor_id = NULL WHERE id = {insuredId}");
            stale.Entry(insured).Property(x => x.Name).CurrentValue = "Stale overwrite";
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => stale.SaveChangesAsync());
        }
        await admin.Database.ExecuteSqlInterpolatedAsync($"UPDATE erp.segurados SET produtor_id = {producer} WHERE id = {insuredId}");
    }
}
