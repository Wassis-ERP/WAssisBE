using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;

using WAssis.Infra.Data.Context;
using WAssis.Infra.Data.Modules.Identity;
using WAssis.Infra.Data.Security;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Integration;

public sealed class SecurityServicesPostgresTests
{
    [Fact]
    [Trait("Category", "Postgres")]
    public async Task PermissionsSessionsAndDistributedBudgetsUseRealPostgres()
    {
        await using var postgres = await PostgresTestDatabase.StartAsync();
        await using var source = NpgsqlDataSource.Create(postgres.GetConnectionString());
        await using var db = new WAssisDbContext(new DbContextOptionsBuilder<WAssisDbContext>()
            .UseNpgsql(source).Options, new FakeCurrentUserContext());
        await db.Database.MigrateAsync();
        await using var connection = await source.OpenConnectionAsync();
        var tenant = Guid.NewGuid(); var branch = Guid.NewGuid(); var user = Guid.NewGuid(); var profile = Guid.NewGuid();
        await connection.ExecuteAsync("""
            INSERT INTO erp.tenants (id) VALUES (@Tenant);
            INSERT INTO erp.filiais (id,tenant_id,ativo) VALUES (@Branch,@Tenant,true);
            INSERT INTO erp.perfis (id,tenant_id,nome,ativo) VALUES (@Profile,@Tenant,'Fixture',true);
            INSERT INTO erp.profiles (id,tenant_id,status,ativo,convite_status) VALUES (@User,@Tenant,'ATIVO',true,'ACEITO');
            INSERT INTO erp.profile_filiais (id,profile_id,filial_id,perfil_id,ativo)
            VALUES (gen_random_uuid(),@User,@Branch,@Profile,true);
            INSERT INTO erp.role_permissions (id,perfil_id,modulo,escopo,can_read,can_update)
            VALUES (gen_random_uuid(),@Profile,'comercial','PROPRIO',true,false);
            """, new { Tenant = tenant, Branch = branch, User = user, Profile = profile });
        var permissions = new EffectivePermissionService(source);
        var own = await permissions.GetAccessAsync(tenant, user, "comercial", "read", branch, false, default);
        Assert.True(own.Allowed); Assert.True(own.OwnRecordsOnly);
        Assert.False((await permissions.GetAccessAsync(tenant, user, "comercial", "update", branch, false, default)).Allowed);
        Assert.False((await permissions.GetAccessAsync(tenant, user, "comercial", "read", Guid.NewGuid(), false, default)).Allowed);
        Assert.Single(await permissions.ListAsync(tenant, user, null, default));
        await connection.ExecuteAsync("UPDATE erp.role_permissions SET escopo = 'UNKNOWN' WHERE perfil_id = @Profile", new { Profile = profile });
        Assert.False((await permissions.GetAccessAsync(tenant, user, "comercial", "read", branch, false, default)).Allowed);

        var sessions = new AuthSessionService(source);
        var jti = Guid.NewGuid().ToString();
        var jwt = new JwtSecurityToken(claims: [new Claim("sub", user.ToString()), new Claim("tenant_id", tenant.ToString()), new Claim("jti", jti)], expires: DateTime.UtcNow.AddMinutes(5));
        await sessions.RegisterAsync(new JwtSecurityTokenHandler().WriteToken(jwt), default);
        Assert.Equal(branch.ToString(), Assert.Single(await sessions.GetAuthorizedBranchesAsync(jti, user.ToString(), tenant.ToString(), default)));
        await sessions.RevokeAsync(jti, default);
        Assert.Empty(await sessions.GetAuthorizedBranchesAsync(jti, user.ToString(), tenant.ToString(), default));

        // Two service instances and changing IPs still share one individual budget.
        var limiter = new PostgresOperationLimiter(source);
        var secondInstance = new PostgresOperationLimiter(source);
        var decisions = await Task.WhenAll(Enumerable.Range(0, 8).Select(i =>
            (i % 2 == 0 ? limiter : secondInstance).AcquireAsync("user-budget", "tenant", "user", $"192.0.2.{i}", 3, TimeSpan.FromMinutes(1), default)));
        Assert.Equal(3, decisions.Count(d => d.Allowed));
        // Eleven distinct users/IPs exceed the tenant budget of ten.
        for (var i = 0; i < 11; i++)
        {
            var decision = await limiter.AcquireAsync("tenant-budget", "tenant", $"user-{i}", $"192.0.2.{i}", 1, TimeSpan.FromMinutes(1), default);
            Assert.Equal(i < 10, decision.Allowed);
        }
        await using (var businessTransaction = await connection.BeginTransactionAsync())
        {
            Assert.True((await limiter.AcquireAsync("rollback", "tenant", "user", "192.0.2.1", 1, TimeSpan.FromMinutes(1), default)).Allowed);
            await businessTransaction.RollbackAsync();
        }
        Assert.False((await secondInstance.AcquireAsync("rollback", "tenant", "user", "192.0.2.2", 1, TimeSpan.FromMinutes(1), default)).Allowed);
    }
}
