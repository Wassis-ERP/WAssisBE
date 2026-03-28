using WAssis.Domain.Modules.Policies.Entities;

namespace WAssis.Application.Modules.Policies.Interfaces;

public interface IPolicyDraftRepository
{
    Task AddAsync(PolicyDraft draft, CancellationToken cancellationToken);
    Task<PolicyDraft?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
