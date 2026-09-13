using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;
using WAssis.Infra.CrossCutting.Identity.Authentication;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Configuration;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Integration;

public sealed class PostgresReadinessTests
{
    private const string TenantA = "22222222-2222-2222-2222-222222222222";
    private const string TenantB = "22222222-2222-2222-2222-222222222223";
    private const string BranchA = "44444444-4444-4444-4444-444444444444";
    private const string BranchB = "44444444-4444-4444-4444-444444444445";
    private const string Password = "local-integration-fixture-password";

    [Fact]
    [Trait("Category", "Postgres")]
    public async Task MigrationsAndHttpJourney_UseRealPostgresAndEnforceTenantAndBranch()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithPassword(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))).Build();
        await postgres.StartAsync();
        await using var db = Context(postgres.GetConnectionString());
        await Assert.ThrowsAsync<InvalidOperationException>(() => DatabaseMigrationGate.EnsureCurrentAsync(db));
        await DatabaseMigrationGate.MigrateSingletonAsync(db);
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO erp.tenants (id) VALUES ('22222222-2222-2222-2222-222222222222'), ('22222222-2222-2222-2222-222222222223');
            INSERT INTO erp.filiais (id,tenant_id,matriz_id,ativo) VALUES
              ('44444444-4444-4444-4444-444444444444','22222222-2222-2222-2222-222222222222',null,true),
              ('44444444-4444-4444-4444-444444444445','22222222-2222-2222-2222-222222222222','44444444-4444-4444-4444-444444444444',true);
            INSERT INTO erp.pipelines (id,tenant_id,filial_id,nome,entidade_tipo,ativo) VALUES
              ('55555555-5555-5555-5555-555555555555','22222222-2222-2222-2222-222222222222','44444444-4444-4444-4444-444444444444','Comercial','oportunidade',true);
            INSERT INTO erp.pipeline_stages (id,pipeline_id,nome,tipo_stage,ativo) VALUES
              ('66666666-6666-6666-6666-666666666666','55555555-5555-5555-5555-555555555555','Em aberto','ABERTO',true),
              ('66666666-6666-6666-6666-666666666667','55555555-5555-5555-5555-555555555555','Ganha','GANHO',true),
              ('66666666-6666-6666-6666-666666666668','55555555-5555-5555-5555-555555555555','Perdida','PERDIDO',true);
            UPDATE erp.pipeline_stages SET finaliza_com_sucesso = true, finaliza_com_perda = true;
            """);

        using var factory = new ApiFactory(postgres.GetConnectionString());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/ready")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/segurados")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/identity/login", new { username = "user0@example.invalid", password = "wrong" })).StatusCode);
        await Login(client, 0);
        var create = await client.PostAsJsonAsync("/api/segurados", new
        {
            officeBranchId = BranchA, name = "Segurado de integração", personType = "PF", status = "Prospecto",
            email = "fixture@example.invalid", lgpdAuthorized = false, socialName = "Preservar no PUT"
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var insured = await create.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        Assert.NotNull(insured);
        var id = insured["id"].GetGuid();
        insured["name"] = JsonSerializer.SerializeToElement("Segurado atualizado");
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/segurados/{id}", insured)).StatusCode);
        using var reread = await client.GetFromJsonAsync<JsonDocument>($"/api/segurados/{id}");
        Assert.Equal("Segurado atualizado", reread!.RootElement.GetProperty("name").GetString());
        Assert.Equal("Preservar no PUT", reread.RootElement.GetProperty("socialName").GetString());
        var stages = await client.GetFromJsonAsync<JsonElement[]>("/api/oportunidades/stages");
        Assert.Equal(3, stages!.Length);
        var opportunityResponse = await client.PostAsJsonAsync("/api/oportunidades", new
        {
            officeBranchId = BranchA, name = "Negócio real", insuredPersonId = id, status = "pending",
            stageId = "66666666-6666-6666-6666-666666666666", title = "Negócio real"
        });
        Assert.Equal(HttpStatusCode.Created, opportunityResponse.StatusCode);
        var opportunity = await opportunityResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var opportunityId = opportunity!["id"].GetGuid();
        Assert.Equal("55555555-5555-5555-5555-555555555555", opportunity["pipelineId"].GetString());
        opportunity["stageId"] = JsonSerializer.SerializeToElement("66666666-6666-6666-6666-666666666667");
        opportunity["status"] = JsonSerializer.SerializeToElement("won");
        opportunity["wonAtUtc"] = JsonSerializer.SerializeToElement(DateTime.UtcNow);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/oportunidades/{opportunityId}", opportunity)).StatusCode);
        using var opportunityRead = await client.GetFromJsonAsync<JsonDocument>($"/api/oportunidades/{opportunityId}");
        Assert.Equal("won", opportunityRead!.RootElement.GetProperty("status").GetString());

        // New context proves persistence, not an EF tracking/in-memory artifact.
        await using (var verification = Context(postgres.GetConnectionString(), TenantA, BranchA))
            Assert.Equal("Segurado atualizado", (await verification.InsuredPeople.SingleAsync(x => x.Id == id)).Name);
        await using (var anonymous = Context(postgres.GetConnectionString()))
            Assert.Empty(await anonymous.InsuredPeople.ToArrayAsync());
        await using (var owner = Context(postgres.GetConnectionString(), TenantA, BranchA))
        await using (var attacker = Context(postgres.GetConnectionString(), TenantB, BranchA))
        {
            var entity = await owner.InsuredPeople.AsNoTracking().SingleAsync(x => x.Id == id);
            attacker.Entry(entity).State = EntityState.Modified;
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => attacker.SaveChangesAsync());
        }
        await using (var owner = Context(postgres.GetConnectionString(), TenantA, BranchA))
        await using (var attacker = Context(postgres.GetConnectionString(), TenantB, BranchA))
        {
            var entity = await owner.InsuredPeople.AsNoTracking().SingleAsync(x => x.Id == id);
            attacker.Entry(entity).Property("TenantId").CurrentValue = TenantB;
            attacker.Entry(entity).State = EntityState.Modified;
            attacker.Entry(entity).Property("TenantId").OriginalValue = TenantB;
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => attacker.SaveChangesAsync());
        }
        await using (var lockConnection = new NpgsqlConnection(postgres.GetConnectionString()))
        {
            await lockConnection.OpenAsync();
            await using var lockCommand = new NpgsqlCommand("SELECT pg_advisory_lock(872391004201)", lockConnection);
            await lockCommand.ExecuteNonQueryAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => DatabaseMigrationGate.MigrateSingletonAsync(db));
            lockCommand.CommandText = "SELECT pg_advisory_unlock(872391004201)";
            await lockCommand.ExecuteNonQueryAsync();
        }
        string? auditPurpose = null;
        await using (var worker = new WAssisDbContext(
            new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(postgres.GetConnectionString()).Options,
            new FakeCurrentUserContext(), WAssis.Application.Abstractions.SystemDataScope.ForWorker("test-queue", purpose => auditPurpose = purpose)))
            Assert.Contains(await worker.InsuredPeople.Select(x => x.Id).ToArrayAsync(), item => item == id);
        Assert.Equal("test-queue", auditPurpose);

        foreach (var otherUser in new[] { 1, 2 })
        {
            await Login(client, otherUser);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/segurados/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/segurados/{id}", insured)).StatusCode);
            var list = await client.GetFromJsonAsync<JsonElement[]>("/api/segurados");
            Assert.DoesNotContain(list!, item => item.GetProperty("id").GetGuid() == id);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/oportunidades/{opportunityId}")).StatusCode);
            Assert.Empty((await client.GetFromJsonAsync<JsonElement[]>("/api/oportunidades/stages"))!);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/oportunidades", new
            { officeBranchId = iBranch(otherUser), name = "Proibido", stageId = "66666666-6666-6666-6666-666666666666", insuredPersonId = id, status = "pending" })).StatusCode);
        }
        await Login(client, 0);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/segurados", new
        { officeBranchId = BranchB, name = "Proibido", status = "Prospecto", personType = "PF" })).StatusCode);

        await VerifyUpgrade(postgres.GetConnectionString());
    }

    private static string iBranch(int user) => user == 2 ? BranchB : BranchA;

    private static async Task VerifyUpgrade(string connectionString)
    {
        await using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand("CREATE DATABASE readiness_upgrade", connection);
            await command.ExecuteNonQueryAsync();
        }
        var upgradeConnection = new NpgsqlConnectionStringBuilder(connectionString) { Database = "readiness_upgrade" }.ConnectionString;
        await using var upgrade = Context(upgradeConnection, TenantA, BranchA);
        var migrations = upgrade.Database.GetMigrations().ToArray();
        await upgrade.GetService<IMigrator>().MigrateAsync(migrations[^2]);
        var id = Guid.NewGuid();
        await upgrade.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO public.segurados (id, tenant_id, filial_id, nome, tipo, status, lgpd_autorizado, created_at, updated_at) VALUES ({id}, {TenantA}, {BranchA}, 'Antes do upgrade', 'PF', 'Prospecto', false, now(), now())");
        // Seed through the old schema: new queue tables do not exist yet.
        var pending = DurableDispatchTests.Quote("upgrade-pending");
        var processing = DurableDispatchTests.Quote("upgrade-processing");
        processing.MarkAsProcessing();
        await using (var legacyWorker = new WAssisDbContext(
            new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(upgradeConnection).Options,
            new FakeCurrentUserContext(), WAssis.Application.Abstractions.SystemDataScope.ForWorker("test-upgrade", _ => { })))
        {
            legacyWorker.QuoteRequests.AddRange(pending, processing);
            await legacyWorker.SaveChangesAsync();
        }
        await upgrade.Database.MigrateAsync();
        Assert.Empty(await upgrade.Database.GetPendingMigrationsAsync());
        Assert.Equal("Antes do upgrade", (await upgrade.InsuredPeople.SingleAsync(x => x.Id == id)).Name);
        Assert.Equal(1, await upgrade.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM infrastructure.work_outbox WHERE state = 'Pending'").SingleAsync());
        Assert.Equal(1, await upgrade.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM infrastructure.work_outbox WHERE state = 'NeedsReview'").SingleAsync());
    }

    private static WAssisDbContext Context(string connection, string? tenant = null, string? branch = null) => new(
        new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(connection, options => options.MigrationsHistoryTable("__EFMigrationsHistory", "quotes")).Options,
        new FakeCurrentUserContext { IsAuthenticated = tenant is not null, TenantId = tenant, BranchId = branch, HasAllBranchesAccess = false, BranchIds = branch is null ? [] : [branch] });

    private static async Task Login(HttpClient client, int user)
    {
        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.PostAsJsonAsync("/api/identity/login", new { username = $"user{user}@example.invalid", password = Password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var login = await response.Content.ReadFromJsonAsync<JsonDocument>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.RootElement.GetProperty("accessToken").GetString());
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
            for (var i = 0; i < 3; i++)
            {
                var prefix = $"Identity:HomologationAuth:Users:{i}:";
                settings[prefix + "Username"] = $"user{i}@example.invalid";
                settings[prefix + "PasswordHash"] = hash;
                settings[prefix + "UserId"] = Guid.NewGuid().ToString();
                settings[prefix + "TenantId"] = i == 1 ? TenantB : TenantA;
                settings[prefix + "BrokerageId"] = i == 1 ? TenantB : TenantA;
                settings[prefix + "BranchId"] = i == 2 ? BranchB : BranchA;
                settings[prefix + "BranchIds:0"] = settings[prefix + "BranchId"];
                settings[prefix + "UserType"] = "brokerage_staff";
                settings[prefix + "Roles:0"] = "brokerage_admin";
            }
            foreach (var setting in settings) builder.UseSetting(setting.Key, setting.Value);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(settings));
        }
    }
}
