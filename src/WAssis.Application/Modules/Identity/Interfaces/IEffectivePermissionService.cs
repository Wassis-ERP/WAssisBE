using WAssis.Application.Modules.Identity.Dtos;

namespace WAssis.Application.Modules.Identity.Interfaces;

public interface IEffectivePermissionService
{
    Task<bool> HasPermissionAsync(
        Guid tenantId,
        Guid userId,
        string module,
        string action,
        Guid? branchId,
        bool tenantWide,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<EffectivePermissionDto>> ListAsync(
        Guid tenantId,
        Guid userId,
        Guid? branchId,
        CancellationToken cancellationToken);
}
