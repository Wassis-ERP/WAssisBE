namespace WAssis.Application.Modules.Identity.Dtos;

public sealed record LoginResultDto(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string UserId,
    string? TenantId,
    string? BrokerageId,
    string? BranchId,
    IReadOnlyCollection<string> BranchIds,
    bool HasAllBranchesAccess,
    string? SellerId,
    string UserType,
    IReadOnlyCollection<string> Roles);
