using WAssis.Domain.Modules.Opportunities.Entities;

namespace WAssis.Application.Modules.Opportunities.Interfaces;

public interface IOpportunityRepository
{
    Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken);
    Task<Opportunity?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
