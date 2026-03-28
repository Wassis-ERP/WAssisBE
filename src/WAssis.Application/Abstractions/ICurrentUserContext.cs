namespace WAssis.Application.Abstractions;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? TenantId { get; }
    string? BrokerageId { get; }
    string? SellerId { get; }
    string? UserType { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);
}
