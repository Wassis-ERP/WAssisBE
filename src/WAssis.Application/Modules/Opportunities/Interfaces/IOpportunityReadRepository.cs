using WAssis.Domain.Modules.Opportunities.Entities;
using WAssis.Application.Abstractions;

namespace WAssis.Application.Modules.Opportunities.Interfaces;

public interface IOpportunityReadRepository
{
    Task<Opportunity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<Opportunity>> ListAsync(
        string? pipelineId,
        string? stageId,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
