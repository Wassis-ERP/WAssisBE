namespace WAssis.Services.Api.Modules.Core.Contracts;

public sealed record CoreSchemaViewModel(
    string Version,
    string DatabaseSchema,
    string TenantId,
    string? ActiveBranchId,
    IReadOnlyCollection<string> AllowedBranchIds,
    bool HasAllBranchesAccess,
    IReadOnlyCollection<CoreSchemaModuleViewModel> Modules);

public sealed record CoreSchemaModuleViewModel(
    string Name,
    IReadOnlyCollection<string> Tables);

public sealed record CoreBranchViewModel(
    Guid Id,
    Guid? ParentBranchId,
    string? Name,
    string? DocumentNumber,
    bool IsActive);
