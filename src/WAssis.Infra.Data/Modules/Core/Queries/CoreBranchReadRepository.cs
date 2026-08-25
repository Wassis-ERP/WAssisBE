using Dapper;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Core.Dtos;
using WAssis.Application.Modules.Core.Interfaces;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Core.Queries;

public sealed class CoreBranchReadRepository(WAssisDbContext dbContext) : ICoreBranchReadRepository
{
    public async Task<IReadOnlyCollection<CoreBranchDto>> ListAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> allowedBranchIds,
        bool hasAllBranchesAccess,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                f.id AS Id,
                f.matriz_id AS ParentBranchId,
                COALESCE(f.razao_social, f.fantasia, f.nome) AS Name,
                COALESCE(f.cnpj_cpf, f.cnpj) AS DocumentNumber,
                f.ativo AS IsActive
            FROM erp.filiais f
            WHERE f.tenant_id = @TenantId
              AND (@HasAllBranchesAccess OR f.id = ANY(@AllowedBranchIds))
            ORDER BY COALESCE(f.razao_social, f.fantasia, f.nome) NULLS LAST, f.id
            """;

        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var command = new CommandDefinition(
            sql,
            new
            {
                TenantId = tenantId,
                AllowedBranchIds = allowedBranchIds.ToArray(),
                HasAllBranchesAccess = hasAllBranchesAccess,
            },
            cancellationToken: cancellationToken);

        var branches = await connection.QueryAsync<CoreBranchDto>(command);
        return branches.AsList();
    }
}
