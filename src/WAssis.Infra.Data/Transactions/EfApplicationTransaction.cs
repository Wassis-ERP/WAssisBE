using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Transactions;

public sealed class EfApplicationTransaction(WAssisDbContext dbContext) : IApplicationTransaction
{
    public async Task<TResponse> ExecuteAsync<TResponse>(
        Func<Task<TResponse>> operation,
        CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is not null)
        {
            return await operation();
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var response = await operation();
            await transaction.CommitAsync(cancellationToken);
            return response;
        });
    }
}
