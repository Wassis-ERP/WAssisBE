namespace WAssis.Application.Modules.Identity.Dtos;

public sealed record ExternalIdentityDto(
    string UserId,
    string TenantId,
    string BrokerageId,
    string BranchId,
    IReadOnlyList<string> BranchIds,
    string? SellerId,
    string UserType,
    IReadOnlyList<string> Roles,
    bool RequiresMfa);
