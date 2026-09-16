namespace WAssis.Application.Abstractions;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    long TotalCount)
{
    public bool HasMore => (long)Page * PageSize < TotalCount;
}
