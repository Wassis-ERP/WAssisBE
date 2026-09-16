namespace WAssis.Application.Abstractions;

public sealed record OperationLimitPartition(string Key, int PermitLimit);

public static class OperationLimitPartitions
{
    // Defaults per operation/window: individual budget, shared tenant budget
    // (10 individuals), and shared IP budget (20 individuals behind a NAT).
    public static IReadOnlyList<OperationLimitPartition> Create(
        string operation, string? tenantId, string? userId, string? address, int permitLimit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentOutOfRangeException.ThrowIfLessThan(permitLimit, 1);
        var authenticated = !string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(userId);
        if (!authenticated)
            return [new($"{operation}|ip|{address ?? "unknown"}", permitLimit)];
        return [
            new($"{operation}|tenant|{tenantId}", checked(permitLimit * 10)),
            new($"{operation}|user|{tenantId}|{userId}", permitLimit),
            new($"{operation}|ip|{address ?? "unknown"}", checked(permitLimit * 20)),
        ];
    }
}
