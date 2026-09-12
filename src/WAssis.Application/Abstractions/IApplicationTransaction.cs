namespace WAssis.Application.Abstractions;

public interface IApplicationTransaction
{
    Task<TResponse> ExecuteAsync<TResponse>(
        Func<Task<TResponse>> operation,
        CancellationToken cancellationToken);
}
