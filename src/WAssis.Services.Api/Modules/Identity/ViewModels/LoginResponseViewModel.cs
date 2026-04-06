namespace WAssis.Services.Api.Modules.Identity.ViewModels;

public sealed record LoginResponseViewModel(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string UserId,
    string? TenantId,
    string? BrokerageId,
    string? SellerId,
    string UserType,
    IReadOnlyCollection<string> Roles);
