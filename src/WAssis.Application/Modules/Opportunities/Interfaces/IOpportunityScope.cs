namespace WAssis.Application.Modules.Opportunities.Interfaces;

public sealed record OpportunityStageOption(string Id, string PipelineId, string PipelineName, string Name, string Type, string? BranchId, bool CanWin, bool CanLose);

public interface IOpportunityScope
{
    Task<IReadOnlyCollection<OpportunityStageOption>> ListStagesAsync(CancellationToken cancellationToken);
    Task<OpportunityStageOption> ValidateAsync(string? branchId, string? stageId, Guid? insuredId, string? status, CancellationToken cancellationToken);
}
