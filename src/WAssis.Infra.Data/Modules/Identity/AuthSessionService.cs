using System.IdentityModel.Tokens.Jwt;
using Dapper;
using Npgsql;
using WAssis.Application.Modules.Identity.Interfaces;

namespace WAssis.Infra.Data.Modules.Identity;

public sealed class AuthSessionService(NpgsqlDataSource dataSource) : IAuthSessionService
{
    public async Task RegisterAsync(string token, CancellationToken cancellationToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var jti = jwt.Id;
        var userId = jwt.Subject;
        var tenantId = jwt.Claims.SingleOrDefault(x => x.Type == "tenant_id")?.Value;
        if (string.IsNullOrWhiteSpace(jti) || string.IsNullOrWhiteSpace(userId)
            || string.IsNullOrWhiteSpace(tenantId) || jwt.ValidTo <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Sessão de autenticação inválida.");
        const string sql = """
            INSERT INTO security.auth_sessions (jti, tenant_id, user_id, expires_at, created_at)
            VALUES (@Jti, @TenantId, @UserId, @ExpiresAt, now())
            ON CONFLICT (jti) DO NOTHING
            """;
        await ExecuteAsync(sql, new { Jti = jti, TenantId = tenantId, UserId = userId, ExpiresAt = jwt.ValidTo }, cancellationToken);
        if (!await ValidateAsync(jti, userId, tenantId, cancellationToken))
        {
            await RevokeAsync(jti, cancellationToken);
            throw new UnauthorizedAccessException("Usuário ou vínculo de corretora inativo.");
        }
    }

    public async Task<bool> ValidateAsync(string jti, string userId, string tenantId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS (
              SELECT 1 FROM security.auth_sessions s
              JOIN erp.profiles p ON p.id::text = s.user_id AND p.tenant_id::text = s.tenant_id
              WHERE s.jti = @Jti AND s.user_id = @UserId AND s.tenant_id = @TenantId
                AND s.revoked_at IS NULL AND s.expires_at > now()
                AND p.ativo AND p.status = 'ATIVO'
                AND (p.convite_status IS NULL OR p.convite_status = 'ACEITO')
                AND EXISTS (
                  SELECT 1 FROM erp.profile_filiais pf
                  JOIN erp.filiais f ON f.id = pf.filial_id AND f.tenant_id = p.tenant_id
                  JOIN erp.perfis pe ON pe.id = pf.perfil_id AND pe.tenant_id = p.tenant_id
                  WHERE pf.profile_id = p.id AND pf.ativo AND f.ativo AND pe.ativo
                    AND (pf.data_inicio IS NULL OR pf.data_inicio <= current_date)
                    AND (pf.data_fim IS NULL OR pf.data_fim >= current_date)
                )
            )
            """;
        return await ExecuteScalarAsync(sql, new { Jti = jti, UserId = userId, TenantId = tenantId }, cancellationToken);
    }

    public Task RevokeAsync(string jti, CancellationToken cancellationToken) => ExecuteAsync(
        "UPDATE security.auth_sessions SET revoked_at = now() WHERE jti = @Jti AND revoked_at IS NULL",
        new { Jti = jti }, cancellationToken);

    private async Task ExecuteAsync(string sql, object values, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, values, cancellationToken: cancellationToken));
    }

    private async Task<bool> ExecuteScalarAsync(string sql, object values, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(sql, values, cancellationToken: cancellationToken));
    }
}
