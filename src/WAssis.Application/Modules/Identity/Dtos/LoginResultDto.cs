namespace WAssis.Application.Modules.Identity.Dtos;

public sealed record LoginResultDto(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string UserId,
    string? TenantId,
    string? BrokerageId,
    string? SellerId,
    string UserType,
    IReadOnlyCollection<string> Roles);
