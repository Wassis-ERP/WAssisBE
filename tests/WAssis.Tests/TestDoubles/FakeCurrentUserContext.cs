using WAssis.Application.Abstractions;

namespace WAssis.Tests.TestDoubles;

public sealed class FakeCurrentUserContext : ICurrentUserContext
{
    public bool IsAuthenticated { get; init; }
    public string? UserId { get; init; }
    public string? TenantId { get; init; }
    public string? BrokerageId { get; init; }
    public string? BranchId { get; init; }
    public string? SellerId { get; init; }
    public string? UserType { get; init; }
    public bool HasAllBranchesAccess { get; init; } = true;
    public IReadOnlyCollection<string> BranchIds { get; init; } = [];
    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public bool IsInRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
