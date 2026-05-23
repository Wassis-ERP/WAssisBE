using WAssis.Domain.Modules.Customers.Entities;

namespace WAssis.Application.Modules.Customers.Interfaces;

public interface IInsuredPersonRepository
{
    Task AddAsync(InsuredPerson insuredPerson, CancellationToken cancellationToken);
    Task<InsuredPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<InsuredPerson>> ListAsync(string? search, string? status, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
