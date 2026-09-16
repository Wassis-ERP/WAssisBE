using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using WAssis.Infra.CrossCutting.Identity.Authentication;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Modules.Administration;
using WAssis.Application.Modules.Administration.Dtos;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Integration;

public sealed class PostgresAdministrationTests
{
    private const string TenantA = "11111111-1111-1111-1111-111111111111";
    private const string TenantB = "22222222-2222-2222-2222-222222222222";
    private const string BranchA = "33333333-3333-3333-3333-333333333333";
    private const string BranchB = "44444444-4444-4444-4444-444444444444";
    private const string MasterA = "55555555-5555-5555-5555-555555555555";
    private const string MasterB = "66666666-6666-6666-6666-666666666666";
    private const string UserA = "77777777-7777-7777-7777-777777777777";
    private const string OtherMasterA = "88888888-8888-8888-8888-888888888888";
    private const string UserB = "99999999-9999-9999-9999-999999999999";
    private const string RogueA = "cccccccc-cccc-cccc-cccc-cccccccccccc";
    private const string PermissionA = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    private const string PermissionB = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
    private const string Password = "local-integration-fixture-password";

    [Fact]
    [Trait("Category", "Postgres")]
    public async Task AdministrationHttp_RejectsCrossTenantAndUnsafeLifecycleChanges()
    {
        await using var postgres = await PostgresTestDatabase.StartAsync();
        await using (var migration = Context(postgres.GetConnectionString()))
        {
            await DatabaseMigrationGate.MigrateSingletonAsync(migration);
            await migration.Database.ExecuteSqlRawAsync($"""
                INSERT INTO erp.tenants (id, razao_social, ativo) VALUES
                  ('{TenantA}', 'Grupo A', true), ('{TenantB}', 'Grupo B', true);
                INSERT INTO erp.filiais (id, tenant_id, matriz_id, razao_social, ativo) VALUES
                  ('{BranchA}', '{TenantA}', NULL, 'Corretora A', true),
                  ('{BranchB}', '{TenantB}', NULL, 'Corretora B', true);
                INSERT INTO erp.perfis (id, tenant_id, nome, sistema, ativo) VALUES
                  ('{MasterA}', '{TenantA}', 'Master', true, true),
                  ('{MasterB}', '{TenantB}', 'Master', true, true);
                INSERT INTO erp.profiles (id, tenant_id, nome_completo, email, status, ativo, convite_status) VALUES
                  ('{UserA}', '{TenantA}', 'Administrador A', 'admin-a@example.invalid', 'ATIVO', true, 'ACEITO'),
                  ('{OtherMasterA}', '{TenantA}', 'Segundo Master', 'master-a@example.invalid', 'ATIVO', true, 'ACEITO'),
                  ('{UserB}', '{TenantB}', 'Administrador B', 'admin-b@example.invalid', 'ATIVO', true, 'ACEITO');
                INSERT INTO erp.profile_filiais (id, profile_id, filial_id, perfil_id, principal, ativo) VALUES
                  (gen_random_uuid(), '{UserA}', '{BranchA}', '{MasterA}', true, true),
                  (gen_random_uuid(), '{OtherMasterA}', '{BranchA}', '{MasterA}', true, true),
                  (gen_random_uuid(), '{UserB}', '{BranchB}', '{MasterB}', true, true);
                INSERT INTO erp.role_permissions
                  (id, perfil_id, modulo, escopo, can_read, can_create, can_update, can_delete, can_export, can_manage)
                VALUES
                  ('{PermissionA}', '{MasterA}', 'configuracoes', 'GRUPO', true, true, true, true, true, true),
                  ('{PermissionB}', '{MasterB}', 'configuracoes', 'GRUPO', true, true, true, true, true, true);
                INSERT INTO public.segurados
                  (id, tenant_id, filial_id, nome, tipo, status, lgpd_autorizado, created_at, updated_at)
                VALUES
                  (gen_random_uuid(), '{TenantA}', '{BranchA}', 'Pessoa A', 'PF', 'active', true, now(), now()),
                  (gen_random_uuid(), '{TenantB}', '{BranchB}', 'Pessoa B', 'PF', 'active', true, now(), now());
                INSERT INTO public.oportunidades
                  (id, tenant_id, filial_id, nome, responsavel_id, status, created_at, updated_at)
                VALUES
                  (gen_random_uuid(), '{TenantA}', '{BranchA}', 'Aberta A', '{UserA}', 'pending', now(), now()),
                  (gen_random_uuid(), '{TenantA}', '{BranchA}', 'Ganha A', '{UserA}', 'won', now(), now()),
                  (gen_random_uuid(), '{TenantB}', '{BranchB}', 'Perdida B', '{UserB}', 'lost', now(), now());
                """);
        }

        await using (var direct = Context(postgres.GetConnectionString()))
        {
            var administration = new AdministrationRepository(direct);
            Assert.True(await administration.HasAdministrationPermissionAsync(
                Guid.Parse(TenantA), Guid.Parse(UserA), false, default));
            Assert.Equal(2, (await administration.ListUsersAsync(Guid.Parse(TenantA), default)).Count);
            Assert.Empty(await administration.ListUserBranchAccessAsync(
                Guid.Parse(TenantA), Guid.Parse(UserB), default));

            // A rejected change rolls back its savepoint even if the caller catches it.
            // A successful repository operation must still obey the caller's rollback.
            Guid rolledBackProfile;
            await using (var outer = await direct.Database.BeginTransactionAsync())
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => administration.UpsertUserBranchAccessAsync(
                    Guid.Parse(TenantB), Guid.Parse(UserB), Guid.Parse(BranchB),
                    new UserBranchAccessUpdateDto(Guid.Parse(MasterB), false, false, null, null), default));
                var preserved = Assert.Single(await administration.ListUserBranchAccessAsync(Guid.Parse(TenantB), Guid.Parse(UserB), default));
                Assert.True(preserved.IsActive);
                Assert.True(preserved.IsPrimary);
                rolledBackProfile = (await administration.CreateAccessProfileAsync(Guid.Parse(TenantB),
                    new AccessProfileCreateDto("Rollback fixture", null, null), default)).Id;
                await outer.RollbackAsync();
            }
            Assert.DoesNotContain(await administration.ListAccessProfilesAsync(Guid.Parse(TenantB), default), p => p.Id == rolledBackProfile);
        }

        using var factory = new ApiFactory(await postgres.CreateApplicationConnectionAsync());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/administration/users")).StatusCode);
        await Login(client, "admin-a@example.invalid");

        var users = await client.GetFromJsonAsync<JsonElement[]>("/api/administration/users");
        Assert.Equal(2, users!.Length);
        Assert.DoesNotContain(users, user => user.GetProperty("id").GetGuid() == Guid.Parse(UserB));
        Assert.Empty((await client.GetFromJsonAsync<JsonElement[]>($"/api/administration/users/{UserB}/branches"))!);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync(
            $"/api/administration/users/{UserB}/branches/{BranchA}", Access(MasterA))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync(
            $"/api/administration/users/{UserA}/branches/{BranchB}", Access(MasterA))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync(
            $"/api/administration/users/{UserA}/branches/{BranchA}", Access(MasterB))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync(
            $"/api/administration/users/{UserA}/branches/{BranchA}", Access(MasterA, "2027-01-02", "2027-01-01"))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PatchAsJsonAsync(
            $"/api/administration/users/{UserA}/status", new { isActive = false })).StatusCode);

        var updatedAccess = await client.PutAsJsonAsync(
            $"/api/administration/users/{OtherMasterA}/branches/{BranchA}", Access(MasterA, "2000-01-01", "2099-12-31"));
        Assert.Equal(HttpStatusCode.OK, updatedAccess.StatusCode);
        using var accessBody = await updatedAccess.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal("2000-01-01", accessBody!.RootElement.GetProperty("startsOn").GetString());
        Assert.Equal("2099-12-31", accessBody.RootElement.GetProperty("endsOn").GetString());
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/administration/access-profiles",
            new { name = "Leitura futura", description = "Fixture", accessLevel = "OPERACIONAL" })).StatusCode);

        var created = await client.PostAsJsonAsync("/api/administration/users", new
        { name = "Novo usuário", email = "new@example.invalid" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var pending = await created.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.False(pending!.RootElement.GetProperty("isActive").GetBoolean());
        Assert.Equal("PENDENTE", pending.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, pending.RootElement.GetProperty("invitationSentAt").ValueKind);
        var newUserId = pending.RootElement.GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.Conflict, (await client.PatchAsJsonAsync(
            $"/api/administration/users/{newUserId}/status", new { isActive = true })).StatusCode);
        var duplicate = await client.PostAsJsonAsync("/api/administration/users", new
        { name = "Duplicado", email = "NEW@example.invalid" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var conflictBody = await duplicate.Content.ReadAsStringAsync();
        Assert.DoesNotContain("PostgresException", conflictBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stackTrace", conflictBody, StringComparison.OrdinalIgnoreCase);

        using var statistics = await client.GetFromJsonAsync<JsonDocument>("/api/administration/statistics");
        Assert.Equal(2, statistics!.RootElement.GetProperty("activeUsers").GetInt32());
        Assert.Equal(1, statistics.RootElement.GetProperty("pendingInvitations").GetInt32());
        Assert.Equal(0, statistics.RootElement.GetProperty("inactiveUsers").GetInt32());
        Assert.Equal(1, statistics.RootElement.GetProperty("insuredPeople").GetInt32());
        Assert.Equal(1, statistics.RootElement.GetProperty("openOpportunities").GetInt32());
        Assert.Equal(1, statistics.RootElement.GetProperty("wonOpportunities").GetInt32());
        Assert.Equal(0, statistics.RootElement.GetProperty("lostOpportunities").GetInt32());
        var permissions = await client.GetFromJsonAsync<JsonElement[]>("/api/administration/permissions");
        Assert.DoesNotContain(permissions!, permission => permission.GetProperty("id").GetGuid() == Guid.Parse(PermissionB));

        Assert.Equal(HttpStatusCode.Unauthorized, await Login(client, "rogue-a@example.invalid"));

        await Login(client, "admin-b@example.invalid");
        Assert.Single((await client.GetFromJsonAsync<JsonElement[]>("/api/administration/users"))!);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync(
            $"/api/administration/users/{UserA}/branches/{BranchA}", Access(MasterB))).StatusCode);

        await using var noAccess = Context(postgres.GetConnectionString());
        var repo = new AdministrationRepository(noAccess);
        var actor = Guid.NewGuid();
        var outcomes = await Task.WhenAll(
            Deactivate(postgres.GetConnectionString(), Guid.Parse(UserA), actor),
            Deactivate(postgres.GetConnectionString(), Guid.Parse(OtherMasterA), actor));
        Assert.Single(outcomes, result => result);
        Assert.Single(outcomes, result => !result);
        Assert.Equal(1, (await repo.GetStatisticsAsync(Guid.Parse(TenantA), default)).ActiveUsers);
        await using var audit = new NpgsqlConnection(postgres.GetConnectionString());
        await audit.OpenAsync();
        await using var command = new NpgsqlCommand(
            $"SELECT count(*) FROM operations.audit_entries WHERE \"TenantId\" = '{TenantA}' AND \"Module\" = 'administration'", audit);
        Assert.True((long)(await command.ExecuteScalarAsync())! >= 1);
    }

    private static object Access(string profileId, string? start = null, string? end = null) => new
    { accessProfileId = profileId, isPrimary = false, isActive = true, startsOn = start, endsOn = end };

    private static async Task<bool> Deactivate(string connection, Guid userId, Guid actor)
    {
        await using var context = Context(connection);
        try { return await new AdministrationRepository(context).SetUserStatusAsync(Guid.Parse(TenantA), userId, false, actor, default); }
        catch (InvalidOperationException) { return false; }
    }

    private static WAssisDbContext Context(string connection) => new(
        new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(connection,
            options => options.MigrationsHistoryTable("__EFMigrationsHistory", "quotes")).Options,
        new FakeCurrentUserContext());

    private static async Task<HttpStatusCode> Login(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/identity/login", new { username = email, password = Password });
        if (response.StatusCode != HttpStatusCode.OK) return response.StatusCode;
        using var csrf = await client.GetFromJsonAsync<JsonDocument>("/api/identity/csrf");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf!.RootElement.GetProperty("token").GetString());
        return response.StatusCode;
    }

    private sealed class ApiFactory(string connection) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Staging");
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connection,
                ["Database:AutoMigrate"] = "false",
                ["Identity:Jwt:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)),
                ["Identity:Jwt:RequireHttpsMetadata"] = "true",
                ["Identity:HomologationAuth:Enabled"] = "true",
                ["Serilog:MinimumLevel:Default"] = "Fatal"
            };
            var hash = new PasswordHasher<HomologationAuthUserOptions>().HashPassword(new(), Password);
            var users = new[] { ("admin-a@example.invalid", UserA, TenantA, BranchA),
                ("admin-b@example.invalid", UserB, TenantB, BranchB),
                ("rogue-a@example.invalid", RogueA, TenantA, BranchA) };
            for (var i = 0; i < users.Length; i++)
            {
                var prefix = $"Identity:HomologationAuth:Users:{i}:";
                settings[prefix + "Username"] = users[i].Item1;
                settings[prefix + "PasswordHash"] = hash;
                settings[prefix + "UserId"] = users[i].Item2;
                settings[prefix + "TenantId"] = users[i].Item3;
                settings[prefix + "BrokerageId"] = users[i].Item3;
                settings[prefix + "BranchId"] = users[i].Item4;
                settings[prefix + "BranchIds:0"] = users[i].Item4;
                settings[prefix + "UserType"] = "brokerage_staff";
                settings[prefix + "Roles:0"] = "brokerage_admin";
            }
            foreach (var setting in settings) builder.UseSetting(setting.Key, setting.Value);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(settings));
        }
    }
}
