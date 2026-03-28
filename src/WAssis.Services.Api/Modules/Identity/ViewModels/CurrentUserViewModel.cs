namespace WAssis.Services.Api.Modules.Identity.ViewModels;

public sealed record CurrentUserViewModel(
    bool IsAuthenticated,
    string? UserId,
    string? TenantId,
    string? BrokerageId,
    string? SellerId,
    string? UserType,
    IReadOnlyCollection<string> Roles);
