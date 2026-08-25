namespace WAssis.Application.Modules.Core.Interfaces;

public interface ICoreCatalogReadRepository
{
    IReadOnlyCollection<string> SupportedResources { get; }

    Task<IReadOnlyCollection<string>> ListJsonAsync(
        string resource,
        Guid tenantId,
        IReadOnlyCollection<Guid> allowedBranchIds,
        bool hasAllBranchesAccess,
        bool activeOnly,
        string? search,
        CancellationToken cancellationToken);
}
