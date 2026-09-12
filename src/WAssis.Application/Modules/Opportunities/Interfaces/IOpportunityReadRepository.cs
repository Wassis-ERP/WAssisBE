using WAssis.Domain.Modules.Opportunities.Entities;

namespace WAssis.Application.Modules.Opportunities.Interfaces;

public interface IOpportunityReadRepository
{
    Task<Opportunity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Opportunity>> ListAsync(
        string? pipelineId,
        string? stageId,
        string? status,
        CancellationToken cancellationToken);
}
