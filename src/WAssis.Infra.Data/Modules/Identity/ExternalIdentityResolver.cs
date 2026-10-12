using Dapper;
using Npgsql;
using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Application.Modules.Identity.Interfaces;

namespace WAssis.Infra.Data.Modules.Identity;

public sealed class ExternalIdentityResolver(NpgsqlDataSource dataSource) : IExternalIdentityResolver
{
    public async Task<ExternalIdentityDto?> ResolveAsync(string issuer, string subject, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(subject)) return null;
        const string sql = """
            SELECT p.id::text AS "UserId", p.tenant_id::text AS "TenantId",
                   pf.filial_id::text AS "BranchId", COALESCE(pf.principal, false) AS "Principal",
                   producer.id::text AS "SellerId",
                   EXISTS (
                     SELECT 1
                     FROM erp.profile_filiais admin_pf
                     JOIN erp.perfis admin_pe ON admin_pe.id = admin_pf.perfil_id AND admin_pe.tenant_id = p.tenant_id
                     JOIN erp.role_permissions rp ON rp.perfil_id = admin_pe.id
                     WHERE admin_pf.profile_id = p.id AND admin_pf.ativo AND admin_pe.ativo
                       AND (admin_pf.data_inicio IS NULL OR admin_pf.data_inicio <= current_date)
                       AND (admin_pf.data_fim IS NULL OR admin_pf.data_fim >= current_date)
                       AND lower(rp.modulo) = 'configuracoes' AND rp.can_manage
                   ) AS "RequiresMfa"
            FROM (SELECT security.resolve_external_profile(@Issuer, @Subject) AS profile_id) external_identity
            JOIN erp.profiles p ON p.id = external_identity.profile_id
            JOIN erp.tenants tenant ON tenant.id = p.tenant_id
            JOIN erp.profile_filiais pf ON pf.profile_id = p.id
            JOIN erp.filiais f ON f.id = pf.filial_id AND f.tenant_id = p.tenant_id
            JOIN erp.perfis pe ON pe.id = pf.perfil_id AND pe.tenant_id = p.tenant_id
            LEFT JOIN LATERAL (
              SELECT producer_row.id FROM erp.produtores producer_row
              WHERE producer_row.profile_id = p.id AND producer_row.tenant_id = p.tenant_id AND producer_row.ativo
              ORDER BY producer_row.id LIMIT 1
            ) producer ON true
            WHERE tenant.ativo AND p.ativo AND p.status = 'ATIVO'
              AND (p.convite_status IS NULL OR p.convite_status = 'ACEITO')
              AND pf.ativo AND f.ativo AND pe.ativo
              AND (pf.data_inicio IS NULL OR pf.data_inicio <= current_date)
              AND (pf.data_fim IS NULL OR pf.data_fim >= current_date)
            ORDER BY COALESCE(pf.principal, false) DESC, pf.filial_id
            """;
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var rows = (await connection.QueryAsync<ExternalIdentityRow>(new CommandDefinition(
            sql, new { Issuer = NormalizeIssuer(issuer), Subject = subject.Trim() }, cancellationToken: cancellationToken))).ToArray();
        if (rows.Length == 0) return null;
        var first = rows[0];
        var roles = first.RequiresMfa ? new[] { "brokerage_admin" } : new[] { "brokerage_agent" };
        return new ExternalIdentityDto(
            first.UserId,
            first.TenantId,
            first.TenantId,
            first.BranchId,
            rows.Select(static row => row.BranchId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            first.SellerId,
            "brokerage_staff",
            roles,
            first.RequiresMfa);
    }

    internal static string NormalizeIssuer(string issuer) => issuer.Trim().TrimEnd('/');

    private sealed record ExternalIdentityRow(
        string UserId,
        string TenantId,
        string BranchId,
        bool Principal,
        string? SellerId,
        bool RequiresMfa);
}
