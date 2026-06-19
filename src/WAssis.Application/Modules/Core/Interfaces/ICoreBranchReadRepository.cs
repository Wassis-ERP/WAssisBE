using WAssis.Application.Modules.Core.Dtos;

namespace WAssis.Application.Modules.Core.Interfaces;

public interface ICoreBranchReadRepository
{
    Task<IReadOnlyCollection<CoreBranchDto>> ListAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> allowedBranchIds,
        bool hasAllBranchesAccess,
        CancellationToken cancellationToken);
}
