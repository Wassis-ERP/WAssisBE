using Dapper;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Opportunities.Interfaces;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Opportunities.Repositories;

public sealed class OpportunityScope(WAssisDbContext db, ICurrentUserContext user) : IOpportunityScope
{
    public async Task<IReadOnlyCollection<OpportunityStageOption>> ListStagesAsync(CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || !Guid.TryParse(user.TenantId, out var tenant)) throw new UnauthorizedAccessException("Contexto de grupo obrigatório.");
        var branches = user.BranchIds.Append(user.BranchId).Where(x => Guid.TryParse(x, out _)).Select(x => Guid.Parse(x!)).Distinct().ToArray();
        const string sql = """
            SELECT s.id::text AS Id, p.id::text AS PipelineId, COALESCE(p.nome, '') AS PipelineName,
                   COALESCE(s.nome, '') AS Name, COALESCE(s.tipo_stage, '') AS Type, p.filial_id::text AS BranchId,
                   COALESCE(s.finaliza_com_sucesso, false) AS CanWin, COALESCE(s.finaliza_com_perda, false) AS CanLose
            FROM erp.pipeline_stages s JOIN erp.pipelines p ON p.id = s.pipeline_id
            WHERE p.tenant_id = @Tenant AND p.entidade_tipo = 'oportunidade' AND p.ativo AND s.ativo
              AND (p.filial_id IS NULL OR @AllBranches OR p.filial_id = ANY(@Branches))
            ORDER BY p.ordem, s.ordem, s.id
            """;
        var result = await db.Database.GetDbConnection().QueryAsync<OpportunityStageOption>(new CommandDefinition(sql,
            new { Tenant = tenant, AllBranches = user.HasAllBranchesAccess, Branches = branches }, cancellationToken: cancellationToken));
        return result.AsList();
    }

    public async Task<OpportunityStageOption> ValidateAsync(string? branchId, string? stageId, Guid? insuredId, string? status, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || !Guid.TryParse(user.TenantId, out var tenant) || !Guid.TryParse(branchId, out var branch)
            || !user.CanAccessBranch(branchId)) throw new UnauthorizedAccessException("Contexto de corretora obrigatório.");
        var validBranch = await db.Database.GetDbConnection().ExecuteScalarAsync<bool>(new CommandDefinition(
            "SELECT EXISTS (SELECT 1 FROM erp.filiais WHERE id = @Branch AND tenant_id = @Tenant AND ativo)",
            new { Branch = branch, Tenant = tenant }, cancellationToken: cancellationToken));
        var stage = (await ListStagesAsync(cancellationToken)).SingleOrDefault(x => x.Id == stageId && (x.BranchId is null || x.BranchId == branchId));
        if (!validBranch || stage is null) throw new UnauthorizedAccessException("Corretora ou etapa indisponível para esta conta.");
        if (insuredId.HasValue && !await db.InsuredPeople.AnyAsync(x => x.Id == insuredId && x.OfficeBranchId == branchId, cancellationToken))
            throw new UnauthorizedAccessException("Segurado indisponível nesta corretora.");
        // A conclusion is a fact; it does not move the card to another stage.
        if ((status == "won" && !stage.CanWin) || (status == "lost" && !stage.CanLose))
            throw new FluentValidation.ValidationException("Esta etapa não permite a conclusão solicitada.");
        return stage;
    }
}
