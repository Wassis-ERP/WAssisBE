using System.Collections.Frozen;
using Dapper;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Core.Interfaces;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Core.Queries;

public sealed class CoreCatalogReadRepository(WAssisDbContext dbContext) : ICoreCatalogReadRepository
{
    private const string TenantCatalog = """
        SELECT to_jsonb(x)::text
        FROM erp.{0} x
        WHERE x.tenant_id = @TenantId
          AND (NOT @ActiveOnly OR x.ativo)
          AND (@Search IS NULL OR COALESCE(x.nome, '') ILIKE ('%' || @Search || '%'))
        ORDER BY x.nome NULLS LAST, x.id
        """;

    private const string BranchCatalog = """
        SELECT to_jsonb(x)::text
        FROM erp.{0} x
        WHERE x.tenant_id = @TenantId
          AND (@HasAllBranchesAccess OR x.id = ANY(@AllowedBranchIds))
          AND (NOT @ActiveOnly OR x.ativo)
          AND (@Search IS NULL OR COALESCE(x.razao_social, x.nome, '') ILIKE ('%' || @Search || '%'))
        ORDER BY COALESCE(x.razao_social, x.nome) NULLS LAST, x.id
        """;

    private const string ScopedCatalog = """
        SELECT to_jsonb(x)::text
        FROM erp.{0} x
        WHERE x.tenant_id = @TenantId
          AND (x.filial_id IS NULL OR @HasAllBranchesAccess OR x.filial_id = ANY(@AllowedBranchIds))
          AND (NOT @ActiveOnly OR x.ativo)
          AND (@Search IS NULL OR COALESCE(x.nome, '') ILIKE ('%' || @Search || '%'))
        ORDER BY x.nome NULLS LAST, x.id
        """;

    private static readonly FrozenDictionary<string, string> Queries =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["filiais"] = string.Format(BranchCatalog, "filiais"),
            ["perfis"] = string.Format(TenantCatalog, "perfis"),
            ["produtores"] = string.Format(TenantCatalog, "produtores"),
            ["seguradoras"] = string.Format(TenantCatalog, "seguradoras"),
            ["ramos"] = string.Format(TenantCatalog, "ramos"),
            ["origens"] = string.Format(TenantCatalog, "origens"),
            ["motivos_perda"] = string.Format(TenantCatalog, "motivos_perda"),
            ["pipelines"] = string.Format(ScopedCatalog, "pipelines"),
            ["campo_definicoes"] = string.Format(ScopedCatalog, "campo_definicoes"),
            ["repasse_regras"] = """
                SELECT to_jsonb(x)::text
                FROM erp.repasse_regras x
                WHERE x.tenant_id = @TenantId
                  AND (x.filial_id IS NULL OR @HasAllBranchesAccess OR x.filial_id = ANY(@AllowedBranchIds))
                  AND (NOT @ActiveOnly OR x.ativo)
                  AND (@Search IS NULL OR COALESCE(x.observacoes, x.papel, '') ILIKE ('%' || @Search || '%'))
                ORDER BY x.prioridade DESC, x.id
                """,
            ["coberturas_catalogo"] = """
                SELECT to_jsonb(x)::text
                FROM erp.coberturas_catalogo x
                JOIN erp.ramos r ON r.id = x.ramo_id
                WHERE r.tenant_id = @TenantId
                  AND (NOT @ActiveOnly OR x.ativo)
                  AND (@Search IS NULL OR COALESCE(x.nome, x.codigo, '') ILIKE ('%' || @Search || '%'))
                ORDER BY x.ordem, x.nome NULLS LAST, x.id
                """,
            ["pipeline_stages"] = """
                SELECT to_jsonb(x)::text
                FROM erp.pipeline_stages x
                JOIN erp.pipelines p ON p.id = x.pipeline_id
                WHERE p.tenant_id = @TenantId
                  AND (p.filial_id IS NULL OR @HasAllBranchesAccess OR p.filial_id = ANY(@AllowedBranchIds))
                  AND (NOT @ActiveOnly OR x.ativo)
                  AND (@Search IS NULL OR COALESCE(x.nome, x.codigo, '') ILIKE ('%' || @Search || '%'))
                ORDER BY x.ordem, x.nome NULLS LAST, x.id
                """,
            ["recebimento_grades"] = """
                SELECT to_jsonb(x)::text
                FROM erp.recebimento_grades x
                JOIN erp.seguradoras s ON s.id = x.seguradora_id
                WHERE s.tenant_id = @TenantId
                  AND (NOT @ActiveOnly OR x.ativo)
                  AND (@Search IS NULL OR COALESCE(x.nome, '') ILIKE ('%' || @Search || '%'))
                ORDER BY x.nome NULLS LAST, x.id
                """,
            ["recebimento_grade_parcelas"] = """
                SELECT to_jsonb(x)::text
                FROM erp.recebimento_grade_parcelas x
                JOIN erp.recebimento_grades g ON g.id = x.grade_id
                JOIN erp.seguradoras s ON s.id = g.seguradora_id
                WHERE s.tenant_id = @TenantId
                  AND (NOT @ActiveOnly OR x.ativo)
                ORDER BY x.numero, x.id
                """,
            ["campo_opcoes"] = """
                SELECT to_jsonb(x)::text
                FROM erp.campo_opcoes x
                JOIN erp.campo_definicoes d ON d.id = x.campo_definicao_id
                WHERE d.tenant_id = @TenantId
                  AND (d.filial_id IS NULL OR @HasAllBranchesAccess OR d.filial_id = ANY(@AllowedBranchIds))
                  AND (NOT @ActiveOnly OR x.ativo)
                  AND (@Search IS NULL OR COALESCE(x.rotulo, x.valor, '') ILIKE ('%' || @Search || '%'))
                ORDER BY x.ordem, x.rotulo NULLS LAST, x.id
                """,
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> SupportedResources => Queries.Keys.Order().ToArray();

    public async Task<IReadOnlyCollection<string>> ListJsonAsync(
        string resource,
        Guid tenantId,
        IReadOnlyCollection<Guid> allowedBranchIds,
        bool hasAllBranchesAccess,
        bool activeOnly,
        string? search,
        CancellationToken cancellationToken)
    {
        if (!Queries.TryGetValue(resource, out var sql))
        {
            throw new ArgumentOutOfRangeException(nameof(resource), "Catalogo nao suportado.");
        }

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
                ActiveOnly = activeOnly,
                Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<string>(command);
        return rows.AsList();
    }
}
