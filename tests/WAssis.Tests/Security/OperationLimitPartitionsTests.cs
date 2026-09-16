using WAssis.Application.Abstractions;

namespace WAssis.Tests.Security;

public sealed class OperationLimitPartitionsTests
{
    [Fact]
    public void ChangingIpDoesNotResetUserOrTenantBudget()
    {
        var first = OperationLimitPartitions.Create("quotes.create", "tenant", "user", "192.0.2.1", 10);
        var moved = OperationLimitPartitions.Create("quotes.create", "tenant", "user", "192.0.2.2", 10);
        Assert.Equal(2, first.Select(p => p.Key).Intersect(moved.Select(p => p.Key)).Count());
        Assert.Contains(first, p => p.Key == "quotes.create|user|tenant|user" && p.PermitLimit == 10);
        Assert.Contains(first, p => p.Key == "quotes.create|tenant|tenant" && p.PermitLimit == 100);
    }

    [Fact]
    public void DifferentUsersShareTenantBudgetAndDifferentTenantsDoNot()
    {
        var first = OperationLimitPartitions.Create("quotes", "tenant-a", "user-a", "192.0.2.1", 5);
        var second = OperationLimitPartitions.Create("quotes", "tenant-a", "user-b", "192.0.2.2", 5);
        Assert.Single(first.Select(p => p.Key).Intersect(second.Select(p => p.Key)));
        var other = OperationLimitPartitions.Create("quotes", "tenant-b", "user-b", "192.0.2.3", 5);
        Assert.Empty(first.Select(p => p.Key).Intersect(other.Select(p => p.Key)));
    }

    [Fact]
    public void AnonymousLoginUsesSharedIpBudget()
    {
        var partition = Assert.Single(OperationLimitPartitions.Create("login", null, null, "192.0.2.1", 5));
        Assert.Equal("login|ip|192.0.2.1", partition.Key);
        Assert.Equal(5, partition.PermitLimit);
    }
}
