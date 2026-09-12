using WAssis.Domain.Modules.Customers.Entities;

namespace WAssis.Application.Modules.Customers.Interfaces;

public interface IInsuredPersonReadRepository
{
    Task<InsuredPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<InsuredPerson>> ListAsync(
        string? search,
        string? status,
        CancellationToken cancellationToken);
}
