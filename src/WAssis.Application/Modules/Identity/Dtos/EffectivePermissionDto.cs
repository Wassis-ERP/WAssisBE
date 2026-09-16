namespace WAssis.Application.Modules.Identity.Dtos;

public sealed record EffectivePermissionDto(
    Guid BranchId,
    string Module,
    string Scope,
    bool CanRead,
    bool CanCreate,
    bool CanUpdate,
    bool CanDelete,
    bool CanExport,
    bool CanManage);
