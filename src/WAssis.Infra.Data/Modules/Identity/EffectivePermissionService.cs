using Dapper;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Identity.Dtos;
using WAssis.Application.Modules.Identity.Interfaces;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Identity;

public sealed class EffectivePermissionService(WAssisDbContext dbContext) : IEffectivePermissionService
{
    private const string ActiveBindings = """
        FROM erp.profiles p
        JOIN erp.profile_filiais pf ON pf.profile_id = p.id
        JOIN erp.filiais f ON f.id = pf.filial_id AND f.tenant_id = p.tenant_id
        JOIN erp.perfis pe ON pe.id = pf.perfil_id AND pe.tenant_id = p.tenant_id
        JOIN erp.role_permissions rp ON rp.perfil_id = pe.id
        WHERE p.id = @UserId AND p.tenant_id = @TenantId
          AND p.ativo AND p.status = 'ATIVO'
          AND (p.convite_status IS NULL OR p.convite_status = 'ACEITO')
          AND pf.ativo AND f.ativo AND pe.ativo
          AND (pf.data_inicio IS NULL OR pf.data_inicio <= current_date)
          AND (pf.data_fim IS NULL OR pf.data_fim >= current_date)
        """;

    public async Task<bool> HasPermissionAsync(
        Guid tenantId,
        Guid userId,
        string module,
        string action,
        Guid? branchId,
        bool tenantWide,
        CancellationToken cancellationToken)
    {
        if (!AllowedActions.Contains(action) || string.IsNullOrWhiteSpace(module)) return false;

        var sql = """
            SELECT EXISTS (
              SELECT 1
            """ + ActiveBindings + """
                AND lower(rp.modulo) = lower(@Module)
                AND (
                  (@TenantWide AND rp.escopo = 'GRUPO')
                  OR (NOT @TenantWide AND @BranchId IS NOT NULL AND pf.filial_id = @BranchId)
                )
                AND CASE lower(@Action)
                  WHEN 'read' THEN rp.can_read
                  WHEN 'create' THEN rp.can_create
                  WHEN 'update' THEN rp.can_update
                  WHEN 'delete' THEN rp.can_delete
                  WHEN 'export' THEN rp.can_export
                  WHEN 'manage' THEN rp.can_manage
                  ELSE false
                END
            )
            """;

        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new
        {
            TenantId = tenantId,
            UserId = userId,
            Module = module.Trim(),
            Action = action,
            BranchId = branchId,
            TenantWide = tenantWide,
        }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<EffectivePermissionDto>> ListAsync(
        Guid tenantId,
        Guid userId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        var sql = """
            SELECT pf.filial_id AS BranchId, lower(rp.modulo) AS Module,
                   upper(COALESCE(rp.escopo, 'CORRETORA')) AS Scope,
                   COALESCE(rp.can_read, false) AS CanRead,
                   COALESCE(rp.can_create, false) AS CanCreate,
                   COALESCE(rp.can_update, false) AS CanUpdate,
                   COALESCE(rp.can_delete, false) AS CanDelete,
                   COALESCE(rp.can_export, false) AS CanExport,
                   COALESCE(rp.can_manage, false) AS CanManage
            """ + ActiveBindings + """
              AND (@BranchId IS NULL OR pf.filial_id = @BranchId)
            ORDER BY pf.filial_id, lower(rp.modulo)
            """;

        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);
        var rows = await connection.QueryAsync<EffectivePermissionDto>(new CommandDefinition(
            sql, new { TenantId = tenantId, UserId = userId, BranchId = branchId }, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    private static readonly HashSet<string> AllowedActions =
        new(["read", "create", "update", "delete", "export", "manage"], StringComparer.OrdinalIgnoreCase);
}
