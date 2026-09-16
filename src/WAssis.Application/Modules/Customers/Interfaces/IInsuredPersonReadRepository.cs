using WAssis.Domain.Modules.Customers.Entities;
using WAssis.Application.Abstractions;

namespace WAssis.Application.Modules.Customers.Interfaces;

public interface IInsuredPersonReadRepository
{
    Task<InsuredPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<InsuredPerson>> ListAsync(
        string? search,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
