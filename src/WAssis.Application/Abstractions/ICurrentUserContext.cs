namespace WAssis.Application.Abstractions;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? TenantId { get; }
    string? BrokerageId { get; }
    string? BranchId { get; }
    string? SellerId { get; }
    string? UserType { get; }
    bool HasAllBranchesAccess { get; }
    IReadOnlyCollection<string> BranchIds { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);
}
