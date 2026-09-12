using WAssis.Application.Abstractions;
using WAssis.Application.Abstractions.Messaging;
using WAssis.Application.Behaviors;

namespace WAssis.Tests.Architecture;

public sealed class TransactionBehaviorTests
{
    [Fact]
    public async Task TransactionalCommand_ExecutesInsideApplicationTransaction()
    {
        var transaction = new RecordingTransaction();
        var behavior = new TransactionBehavior<TransactionalRequest, string>(transaction);

        var response = await behavior.Handle(
            new TransactionalRequest(),
            () => Task.FromResult("ok"),
            CancellationToken.None);

        Assert.Equal("ok", response);
        Assert.Equal(1, transaction.ExecutionCount);
    }

    [Fact]
    public async Task NonTransactionalRequest_BypassesApplicationTransaction()
    {
        var transaction = new RecordingTransaction();
        var behavior = new TransactionBehavior<ReadRequest, string>(transaction);

        var response = await behavior.Handle(
            new ReadRequest(),
            () => Task.FromResult("ok"),
            CancellationToken.None);

        Assert.Equal("ok", response);
        Assert.Equal(0, transaction.ExecutionCount);
    }

    private sealed record TransactionalRequest : ITransactionalCommand<string>;
    private sealed record ReadRequest : IQuery<string>;

    private sealed class RecordingTransaction : IApplicationTransaction
    {
        public int ExecutionCount { get; private set; }

        public Task<TResponse> ExecuteAsync<TResponse>(
            Func<Task<TResponse>> operation,
            CancellationToken cancellationToken)
        {
            ExecutionCount++;
            return operation();
        }
    }
}
