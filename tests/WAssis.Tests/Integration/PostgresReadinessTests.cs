using System.Net;
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
using WAssis.Infra.CrossCutting.Identity.Authentication;
using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Configuration;
using WAssis.Tests.TestDoubles;
using WAssis.Infra.Data.Modules.Quotes.Repositories;

namespace WAssis.Tests.Integration;

public sealed partial class PostgresReadinessTests
{
    private const string TenantA = "22222222-2222-2222-2222-222222222222";
    private const string TenantB = "22222222-2222-2222-2222-222222222223";
    private const string BranchA = "44444444-4444-4444-4444-444444444444";
    private const string BranchB = "44444444-4444-4444-4444-444444444445";
    private const string BranchTenantB = "44444444-4444-4444-4444-444444444446";
    private const string UserA = "77777777-7777-7777-7777-777777777770";
    private const string UserTenantB = "77777777-7777-7777-7777-777777777771";
    private const string UserBranchB = "77777777-7777-7777-7777-777777777772";
    private const string ProfileA = "88888888-8888-8888-8888-888888888880";
    private const string ProfileTenantB = "88888888-8888-8888-8888-888888888881";
    private const string Password = "local-integration-fixture-password";

    [Fact]
    [Trait("Category", "Postgres")]
    public async Task MigrationsAndHttpJourney_UseRealPostgresAndEnforceTenantAndBranch()
    {
        await using var postgres = await PostgresTestDatabase.StartAsync();
        await using var db = Context(postgres.GetConnectionString());
        await Assert.ThrowsAsync<InvalidOperationException>(() => DatabaseMigrationGate.EnsureCurrentAsync(db));
        await DatabaseMigrationGate.MigrateSingletonAsync(db);
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO erp.tenants (id) VALUES ('22222222-2222-2222-2222-222222222222'), ('22222222-2222-2222-2222-222222222223');
            INSERT INTO erp.filiais (id,tenant_id,matriz_id,ativo) VALUES
              ('44444444-4444-4444-4444-444444444444','22222222-2222-2222-2222-222222222222',null,true),
              ('44444444-4444-4444-4444-444444444445','22222222-2222-2222-2222-222222222222','44444444-4444-4444-4444-444444444444',true),
              ('44444444-4444-4444-4444-444444444446','22222222-2222-2222-2222-222222222223',null,true);
            INSERT INTO erp.perfis (id,tenant_id,nome,sistema,ativo) VALUES
              ('88888888-8888-8888-8888-888888888880','22222222-2222-2222-2222-222222222222','Master fixture A',false,true),
              ('88888888-8888-8888-8888-888888888881','22222222-2222-2222-2222-222222222223','Master fixture B',false,true);
            INSERT INTO erp.profiles (id,tenant_id,nome_completo,email,status,ativo,convite_status) VALUES
              ('77777777-7777-7777-7777-777777777770','22222222-2222-2222-2222-222222222222','User A','user0@example.invalid','ATIVO',true,'ACEITO'),
              ('77777777-7777-7777-7777-777777777771','22222222-2222-2222-2222-222222222223','User B','user1@example.invalid','ATIVO',true,'ACEITO'),
              ('77777777-7777-7777-7777-777777777772','22222222-2222-2222-2222-222222222222','User branch B','user2@example.invalid','ATIVO',true,'ACEITO');
            INSERT INTO erp.profile_filiais (id,profile_id,filial_id,perfil_id,principal,ativo) VALUES
              (gen_random_uuid(),'77777777-7777-7777-7777-777777777770','44444444-4444-4444-4444-444444444444','88888888-8888-8888-8888-888888888880',true,true),
              (gen_random_uuid(),'77777777-7777-7777-7777-777777777771','44444444-4444-4444-4444-444444444446','88888888-8888-8888-8888-888888888881',true,true),
              (gen_random_uuid(),'77777777-7777-7777-7777-777777777772','44444444-4444-4444-4444-444444444445','88888888-8888-8888-8888-888888888880',true,true);
            INSERT INTO erp.role_permissions
              (id,perfil_id,modulo,escopo,can_read,can_create,can_update,can_delete,can_export,can_manage)
            SELECT gen_random_uuid(), perfil_id, modulo, 'CORRETORA', true,true,true,true,true,true
            FROM (VALUES
              ('88888888-8888-8888-8888-888888888880'::uuid,'segurados'),
              ('88888888-8888-8888-8888-888888888880'::uuid,'comercial'),
              ('88888888-8888-8888-8888-888888888881'::uuid,'segurados'),
              ('88888888-8888-8888-8888-888888888881'::uuid,'comercial')
            ) permissions(perfil_id,modulo);
            INSERT INTO erp.pipelines (id,tenant_id,filial_id,nome,entidade_tipo,ativo) VALUES
              ('55555555-5555-5555-5555-555555555555','22222222-2222-2222-2222-222222222222','44444444-4444-4444-4444-444444444444','Comercial','oportunidade',true);
            INSERT INTO erp.pipeline_stages (id,pipeline_id,nome,tipo_stage,ativo) VALUES
              ('66666666-6666-6666-6666-666666666666','55555555-5555-5555-5555-555555555555','Em aberto','ABERTO',true),
              ('66666666-6666-6666-6666-666666666667','55555555-5555-5555-5555-555555555555','Ganha','GANHO',true),
              ('66666666-6666-6666-6666-666666666668','55555555-5555-5555-5555-555555555555','Perdida','PERDIDO',true);
            UPDATE erp.pipeline_stages SET finaliza_com_sucesso = true, finaliza_com_perda = true;
            """);

        using var factory = new ApiFactory(await postgres.CreateApplicationConnectionAsync());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/health/ready")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/segurados")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/identity/login", new { username = "user0@example.invalid", password = "wrong" })).StatusCode);
        using var anonymousCsrf = await client.GetFromJsonAsync<JsonDocument>("/api/identity/csrf");
        var sessionCookie = await Login(client, 0);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/core/branches")).StatusCode);
        await AssertIdentityBranches(client, BranchA);
        using (var staleCsrf = new HttpRequestMessage(HttpMethod.Post, "/api/identity/logout"))
        {
            staleCsrf.Headers.Add("X-CSRF-TOKEN", anonymousCsrf!.RootElement.GetProperty("token").GetString());
            Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(staleCsrf)).StatusCode);
        }
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/health/ready")).StatusCode);
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
        await VerifyRlsWithEffectiveApplicationRole(postgres.GetConnectionString(), id);
        await VerifyQuoteRlsWithEffectiveWorkerRole(postgres.GetConnectionString());
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
        await VerifyCanonicalCalculationReferences(db, opportunityId, id);
        Assert.Equal("55555555-5555-5555-5555-555555555555", opportunity["pipelineId"].GetString());
        opportunity["stageId"] = JsonSerializer.SerializeToElement("66666666-6666-6666-6666-666666666667");
        opportunity["status"] = JsonSerializer.SerializeToElement("won");
        opportunity["wonAtUtc"] = JsonSerializer.SerializeToElement(DateTime.UtcNow);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/oportunidades/{opportunityId}", opportunity)).StatusCode);
        using var opportunityRead = await client.GetFromJsonAsync<JsonDocument>($"/api/oportunidades/{opportunityId}");
        Assert.Equal("won", opportunityRead!.RootElement.GetProperty("status").GetString());
        var calculationId = await VerifyCanonicalCalculationHttp(db, client, opportunityId, id);

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

        await VerifyQuoteIdempotencyHttp(client);
        await VerifyOwnScopeAndLivePermissions(db, client, id, opportunityId);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/identity/logout", null)).StatusCode);
        using (var replay = factory.CreateClient(new WebApplicationFactoryClientOptions
        { BaseAddress = new Uri("https://localhost"), HandleCookies = false }))
        {
            replay.DefaultRequestHeaders.Add("Cookie", sessionCookie);
            Assert.Equal(HttpStatusCode.Unauthorized, (await replay.GetAsync("/api/identity/me")).StatusCode);
        }

        foreach (var otherUser in new[] { 1, 2 })
        {
            await Login(client, otherUser);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/segurados/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/segurados/{id}", insured)).StatusCode);
            using var list = await client.GetFromJsonAsync<JsonDocument>("/api/segurados");
            Assert.DoesNotContain(list!.RootElement.GetProperty("items").EnumerateArray(), item => item.GetProperty("id").GetGuid() == id);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/oportunidades/{opportunityId}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/calculos/{calculationId}")).StatusCode);
            Assert.Empty((await client.GetFromJsonAsync<JsonElement[]>("/api/oportunidades/stages"))!);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/oportunidades", new
            { officeBranchId = iBranch(otherUser), name = "Proibido", stageId = "66666666-6666-6666-6666-666666666666", insuredPersonId = id, status = "pending" })).StatusCode);
        }
        await Login(client, 0);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/segurados", new
        { officeBranchId = BranchB, name = "Proibido", status = "Prospecto", personType = "PF" })).StatusCode);

        await VerifyUpgrade();
    }

    private static async Task VerifyRlsWithEffectiveApplicationRole(string connectionString, Guid insuredId)
    {
        await using (var admin = new NpgsqlConnection(connectionString))
        {
            await admin.OpenAsync();
            await using var setup = new NpgsqlCommand("""
                CREATE ROLE wassis_test_app NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS;
                GRANT USAGE ON SCHEMA erp, security TO wassis_test_app;
                GRANT SELECT, INSERT, UPDATE, DELETE ON erp.segurados TO wassis_test_app;
                GRANT EXECUTE ON FUNCTION security.current_tenant_id(), security.current_branch_id() TO wassis_test_app;
                """, admin);
            await setup.ExecuteNonQueryAsync();
        }

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var scoped = await dataSource.OpenConnectionAsync())
        await using (var transaction = await scoped.BeginTransactionAsync())
        {
            await using var context = new NpgsqlCommand("""
                SET LOCAL ROLE wassis_test_app;
                SELECT set_config('app.tenant_id', @tenant, true), set_config('app.branch_id', @branch, true);
                """, scoped, transaction);
            context.Parameters.AddWithValue("tenant", TenantA);
            context.Parameters.AddWithValue("branch", BranchA);
            await context.ExecuteNonQueryAsync();
            await using var visible = new NpgsqlCommand("SELECT count(*) FROM erp.segurados WHERE id = @id", scoped, transaction);
            visible.Parameters.AddWithValue("id", insuredId);
            Assert.Equal(1L, await visible.ExecuteScalarAsync());
            await transaction.RollbackAsync();
        }

        foreach (var (tenant, branch, expected) in new[]
        {
            (TenantA, BranchA, 1L), (TenantA, BranchB, 0L),
            (TenantB, BranchTenantB, 0L), (string.Empty, string.Empty, 0L),
        })
        {
            await using var scoped = await dataSource.OpenConnectionAsync();
            await using var transaction = await scoped.BeginTransactionAsync();
            await using var setup = new NpgsqlCommand("""
                SET LOCAL ROLE wassis_test_app;
                SELECT set_config('app.tenant_id', @tenant, true), set_config('app.branch_id', @branch, true);
                """, scoped, transaction);
            setup.Parameters.AddWithValue("tenant", tenant);
            setup.Parameters.AddWithValue("branch", branch);
            await setup.ExecuteNonQueryAsync();
            await using var visible = new NpgsqlCommand("SELECT count(*) FROM erp.segurados WHERE id = @id", scoped, transaction);
            visible.Parameters.AddWithValue("id", insuredId);
            Assert.Equal(expected, await visible.ExecuteScalarAsync());
            await using var write = new NpgsqlCommand("UPDATE erp.segurados SET nome = nome WHERE id = @id", scoped, transaction);
            write.Parameters.AddWithValue("id", insuredId);
            Assert.Equal((int)expected, await write.ExecuteNonQueryAsync());
            // An INSERT outside the selected tenant/branch must fail WITH CHECK,
            // including when transaction-local context is absent.
            await using var forbidden = new NpgsqlCommand("""
                INSERT INTO erp.segurados (id,tenant_id,filial_id,nome,tipo,status,lgpd_autorizado,created_at,updated_at)
                VALUES (@id,CAST(@tenant AS uuid),CAST(@branch AS uuid),'RLS fixture','PF','Prospecto',false,now(),now())
                """, scoped, transaction);
            forbidden.Parameters.AddWithValue("id", Guid.NewGuid());
            forbidden.Parameters.AddWithValue("tenant", TenantA);
            forbidden.Parameters.AddWithValue("branch", branch == BranchA ? BranchB : BranchA);
            var denied = await Assert.ThrowsAsync<PostgresException>(() => forbidden.ExecuteNonQueryAsync());
            Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, denied.SqlState);
            await transaction.RollbackAsync();
        }

        // A pooled connection without transaction-local context must deny by default.
        await using (var reused = await dataSource.OpenConnectionAsync())
        await using (var transaction = await reused.BeginTransactionAsync())
        {
            await using var role = new NpgsqlCommand("SET LOCAL ROLE wassis_test_app", reused, transaction);
            await role.ExecuteNonQueryAsync();
            await using var hidden = new NpgsqlCommand("SELECT count(*) FROM erp.segurados WHERE id = @id", reused, transaction);
            hidden.Parameters.AddWithValue("id", insuredId);
            Assert.Equal(0L, await hidden.ExecuteScalarAsync());
            await transaction.RollbackAsync();
        }
    }

    private static async Task VerifyQuoteRlsWithEffectiveWorkerRole(string connectionString)
    {
        var workerRole = "wassis_test_worker_" + Guid.NewGuid().ToString("N");
        var quoteA = DurableDispatchTests.Quote($"rls-a-{Guid.NewGuid():N}", TenantA, BranchA);
        var quoteB = DurableDispatchTests.Quote($"rls-b-{Guid.NewGuid():N}", TenantA, BranchB);
        await using (var seed = new WAssisDbContext(
            new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(connectionString).Options,
            new FakeCurrentUserContext(),
            WAssis.Application.Abstractions.SystemDataScope.ForWorker("test-quote-rls-seed", _ => { })))
        {
            var repository = new QuoteRequestRepository(seed);
            await repository.AddAsync(quoteA, default);
            await repository.AddAsync(quoteB, default);
            await repository.SaveChangesAsync(default);
        }

        await using (var admin = new NpgsqlConnection(connectionString))
        {
            await admin.OpenAsync();
            // The identifier is generated locally as lowercase ASCII + GUID hex, never supplied externally.
            await using var setup = new NpgsqlCommand($"""
                CREATE ROLE {workerRole} NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS;
                GRANT USAGE ON SCHEMA security, quotes TO {workerRole};
                GRANT SELECT, UPDATE ON quotes.quote_requests TO {workerRole};
                GRANT SELECT, INSERT ON quotes.quote_options TO {workerRole};
                GRANT SELECT, INSERT ON quotes.quote_option_coverages, quotes.quote_option_installments, quotes.quote_option_messages TO {workerRole};
                GRANT EXECUTE ON FUNCTION security.current_tenant_id(), security.current_branch_id() TO {workerRole};
                """, admin);
            await setup.ExecuteNonQueryAsync();
        }

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        foreach (var (tenant, branch, expectedId) in new[]
        {
            (TenantA, BranchA, quoteA.Id),
            (TenantA, BranchB, quoteB.Id),
        })
        {
            await using var scoped = await dataSource.OpenConnectionAsync();
            await using var transaction = await scoped.BeginTransactionAsync();
            await using var setup = new NpgsqlCommand($"""
                SET LOCAL ROLE {workerRole};
                SELECT set_config('app.tenant_id', @tenant, true), set_config('app.branch_id', @branch, true);
                """, scoped, transaction);
            setup.Parameters.AddWithValue("tenant", tenant);
            setup.Parameters.AddWithValue("branch", branch);
            await setup.ExecuteNonQueryAsync();

            await using var visible = new NpgsqlCommand("SELECT array_agg(\"Id\" ORDER BY \"Id\") FROM quotes.quote_requests", scoped, transaction);
            Assert.Equal([expectedId], (Guid[])(await visible.ExecuteScalarAsync())!);

            var forbiddenId = expectedId == quoteA.Id ? quoteB.Id : quoteA.Id;
            await using var crossBranchUpdate = new NpgsqlCommand("UPDATE quotes.quote_requests SET \"UpdatedAtUtc\" = now() WHERE \"Id\" = @id", scoped, transaction);
            crossBranchUpdate.Parameters.AddWithValue("id", forbiddenId);
            Assert.Equal(0, await crossBranchUpdate.ExecuteNonQueryAsync());

            await using var crossBranchChild = new NpgsqlCommand("""
                INSERT INTO quotes.quote_options
                  ("Id","TenantId","QuoteRequestId","InsuranceCompanyCode","InsuranceCompanyName","ProductCode","ProductName","Status","ExternalReference")
                VALUES (@id,@tenant,@quote,'fixture','Fixture','fixture','Fixture',0,'fixture')
                """, scoped, transaction);
            crossBranchChild.Parameters.AddWithValue("id", Guid.NewGuid());
            crossBranchChild.Parameters.AddWithValue("tenant", TenantA);
            crossBranchChild.Parameters.AddWithValue("quote", forbiddenId);
            var denied = await Assert.ThrowsAsync<PostgresException>(() => crossBranchChild.ExecuteNonQueryAsync());
            Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, denied.SqlState);
            await transaction.RollbackAsync();
        }

        await using (var reused = await dataSource.OpenConnectionAsync())
        await using (var transaction = await reused.BeginTransactionAsync())
        {
            await using var role = new NpgsqlCommand($"SET LOCAL ROLE {workerRole}", reused, transaction);
            await role.ExecuteNonQueryAsync();
            await using var hidden = new NpgsqlCommand("SELECT count(*) FROM quotes.quote_requests", reused, transaction);
            Assert.Equal(0L, await hidden.ExecuteScalarAsync());
            await transaction.RollbackAsync();
        }
    }

    private static string iBranch(int user) => user == 2 ? BranchB : BranchTenantB;

    private static async Task VerifyUpgrade()
    {
        await using var upgradeDatabase = await PostgresTestDatabase.StartAsync();
        var upgradeConnection = upgradeDatabase.GetConnectionString();
        await using var upgrade = Context(upgradeConnection, TenantA, BranchA);
        await upgrade.GetService<IMigrator>().MigrateAsync("20260912120359_AlignFrontendV31ConnectedContracts");
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
        Assert.False(await upgrade.InsuredPeople.AnyAsync(x => x.Id == id)); // Authorized non-production reset.
        Assert.Equal(1, await upgrade.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM infrastructure.work_outbox WHERE state = 'Pending'").SingleAsync());
        Assert.Equal(1, await upgrade.Database.SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM infrastructure.work_outbox WHERE state = 'NeedsReview'").SingleAsync());
    }

    private static WAssisDbContext Context(string connection, string? tenant = null, string? branch = null) => new(
        new DbContextOptionsBuilder<WAssisDbContext>().UseNpgsql(connection, options => options.MigrationsHistoryTable("__EFMigrationsHistory", "quotes")).Options,
        new FakeCurrentUserContext { IsAuthenticated = tenant is not null, TenantId = tenant, BranchId = branch, HasAllBranchesAccess = false, BranchIds = branch is null ? [] : [branch] });

    private static async Task<string> Login(HttpClient client, int user)
    {
        var response = await client.PostAsJsonAsync("/api/identity/login", new { username = $"user{user}@example.invalid", password = Password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var csrf = await client.GetFromJsonAsync<JsonDocument>("/api/identity/csrf");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf!.RootElement.GetProperty("token").GetString());
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), value => value.StartsWith("wassis_access=", StringComparison.Ordinal));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        return cookie.Split(';')[0];
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
                ["Documents:PrivateStorage:RootPath"] = Path.Combine(Path.GetTempPath(), "wassis-test-documents", Guid.NewGuid().ToString("N")),
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
                settings[prefix + "UserId"] = i == 0 ? UserA : i == 1 ? UserTenantB : UserBranchB;
                settings[prefix + "TenantId"] = i == 1 ? TenantB : TenantA;
                settings[prefix + "BrokerageId"] = i == 1 ? TenantB : TenantA;
                settings[prefix + "BranchId"] = i == 2 ? BranchB : i == 1 ? BranchTenantB : BranchA;
                settings[prefix + "BranchIds:0"] = settings[prefix + "BranchId"];
                settings[prefix + "UserType"] = "brokerage_staff";
                settings[prefix + "Roles:0"] = "brokerage_admin";

            }
            foreach (var setting in settings) builder.UseSetting(setting.Key, setting.Value);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(settings));
        }
    }
}
