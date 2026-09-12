using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Abstractions.Messaging;

namespace WAssis.Application.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(IApplicationTransaction transaction)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ITransactionalCommand<TResponse>)
        {
            return next();
        }

        return transaction.ExecuteAsync<TResponse>(() => next(), cancellationToken);
    }
}
