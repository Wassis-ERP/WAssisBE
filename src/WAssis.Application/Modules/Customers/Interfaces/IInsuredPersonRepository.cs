using WAssis.Domain.Modules.Customers.Entities;

namespace WAssis.Application.Modules.Customers.Interfaces;

public interface IInsuredPersonRepository
{
    Task AddAsync(InsuredPerson insuredPerson, CancellationToken cancellationToken);
    Task<InsuredPerson?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
