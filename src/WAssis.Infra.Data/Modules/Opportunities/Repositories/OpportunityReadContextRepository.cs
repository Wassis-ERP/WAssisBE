using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Opportunities.Interfaces;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Opportunities.Repositories;

public sealed class OpportunityReadContextRepository(WAssisDbContext db, ICurrentUserContext user) : IOpportunityReadContext
{
    public async Task<OpportunityReadContext?> GetAsync(Guid opportunityId, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated || !Guid.TryParse(user.TenantId, out var tenant) || !Guid.TryParse(user.BranchId, out var branch)) return null;
        // The live comercial/PROPRIO filter must authorize the root before reading any label.
        var opportunity = await db.Opportunities.AsNoTracking().SingleOrDefaultAsync(
            row => row.Id == opportunityId && row.OfficeBranchId == user.BranchId, cancellationToken);
        if (opportunity is null) return null;
        await db.Database.OpenConnectionAsync(cancellationToken);

        var line = await ReferenceAsync<OpportunityInsuranceBranchLabel>("""
            SELECT id AS Id,nome AS Name,risk_type AS RiskType,grupo_operacional AS OperationalGroup,forma_calculo AS CalculationForm
            FROM erp.ramos WHERE id=@Reference AND tenant_id=@Tenant
            """, opportunity.InsuranceLineId, tenant, branch, cancellationToken);
        var origin = await ReferenceAsync<OpportunityReferenceLabel>("""
            SELECT id AS Id,nome AS Name FROM erp.origens WHERE id=@Reference AND tenant_id=@Tenant
            """, opportunity.OriginId, tenant, branch, cancellationToken);
        var reason = await ReferenceAsync<OpportunityReferenceLabel>("""
            SELECT id AS Id,nome AS Name FROM erp.motivos_perda WHERE id=@Reference AND tenant_id=@Tenant
            """, opportunity.LossReasonId, tenant, branch, cancellationToken);
        var stage = await ReferenceAsync<OpportunityStageLabel>("""
            SELECT s.id AS Id,s.pipeline_id AS PipelineId,s.nome AS Name,s.codigo AS Code,s.tipo_stage AS Type,
              s.cor AS Color,s.ordem AS "Order",s.probabilidade AS Probability,s.sla_dias AS SlaDays,
              s.finaliza_com_sucesso AS CanWin,s.finaliza_com_perda AS CanLose,s.ativo AS Active
            FROM erp.pipeline_stages s JOIN erp.pipelines p ON p.id=s.pipeline_id
            WHERE s.id=@Reference AND p.tenant_id=@Tenant AND p.entidade_tipo='oportunidade'
              AND (p.filial_id IS NULL OR p.filial_id=@Branch)
            """, opportunity.StageId, tenant, branch, cancellationToken);
        return new(opportunityId, line, origin, reason, stage);
    }

    private Task<T?> ReferenceAsync<T>(string sql, string? reference, Guid tenant, Guid branch, CancellationToken cancellationToken) where T : class
    {
        if (!Guid.TryParse(reference, out var id)) return Task.FromResult<T?>(null);
        var parameters = new DynamicParameters();
        parameters.Add("Reference", id, DbType.Guid); parameters.Add("Tenant", tenant, DbType.Guid); parameters.Add("Branch", branch, DbType.Guid);
        return db.Database.GetDbConnection().QuerySingleOrDefaultAsync<T>(new CommandDefinition(sql, parameters,
            db.Database.CurrentTransaction?.GetDbTransaction(), commandTimeout: 30, cancellationToken: cancellationToken));
    }
}
