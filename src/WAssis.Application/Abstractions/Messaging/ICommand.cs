using MediatR;

namespace WAssis.Application.Abstractions.Messaging;

public interface ICommand<out TResponse> : IRequest<TResponse>;

public interface ITransactionalCommand<out TResponse> : ICommand<TResponse>;
