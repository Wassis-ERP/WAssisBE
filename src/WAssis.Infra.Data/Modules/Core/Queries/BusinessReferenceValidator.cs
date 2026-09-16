using Dapper;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Core.Interfaces;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Core.Queries;

public sealed class BusinessReferenceValidator(WAssisDbContext db, ICurrentUserContext user)
    : IBusinessReferenceValidator
{
    public async Task ValidateProducerReferencesAsync(
        string branchId,
        string? producerId,
        string? managerId,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireScope(branchId);
        foreach (var (value, name) in new[] { (producerId, "Produtor"), (managerId, "Gerente") })
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            if (!Guid.TryParse(value, out var id)) throw new UnauthorizedAccessException($"{name} inválido.");
            const string sql = """
                SELECT EXISTS (
                  SELECT 1 FROM erp.produtores
                  WHERE id = @Id AND tenant_id = @TenantId AND ativo
                )
                """;
            if (!await ExecuteExistsAsync(sql, new { Id = id, TenantId = tenantId }, cancellationToken))
                throw new UnauthorizedAccessException($"{name} indisponível neste grupo.");
        }
    }

    public async Task ValidateResponsibleAsync(string branchId, string responsibleId, CancellationToken cancellationToken)
    {
        var tenantId = RequireScope(branchId);
        if (!Guid.TryParse(responsibleId, out var responsible) || !Guid.TryParse(branchId, out var branch))
            throw new UnauthorizedAccessException("Responsável inválido.");
        const string sql = """
            SELECT EXISTS (
              SELECT 1 FROM erp.profiles p
              JOIN erp.profile_filiais pf ON pf.profile_id = p.id
              JOIN erp.perfis pe ON pe.id = pf.perfil_id AND pe.tenant_id = p.tenant_id
              JOIN erp.filiais f ON f.id = pf.filial_id AND f.tenant_id = p.tenant_id
              WHERE p.id = @Responsible AND p.tenant_id = @TenantId
                AND pf.filial_id = @Branch AND p.ativo AND p.status = 'ATIVO'
                AND pf.ativo AND pe.ativo AND f.ativo
                AND (pf.data_inicio IS NULL OR pf.data_inicio <= current_date)
                AND (pf.data_fim IS NULL OR pf.data_fim >= current_date)
            )
            """;
        if (!await ExecuteExistsAsync(sql, new { Responsible = responsible, TenantId = tenantId, Branch = branch }, cancellationToken))
            throw new UnauthorizedAccessException("Responsável sem vínculo ativo com esta corretora.");
    }

    private Guid RequireScope(string branchId)
    {
        if (!Guid.TryParse(user.TenantId, out var tenant) || !user.CanAccessBranch(branchId))
            throw new UnauthorizedAccessException("Contexto de grupo e corretora inválido.");
        return tenant;
    }

    private async Task<bool> ExecuteExistsAsync(string sql, object parameters, CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }
}
