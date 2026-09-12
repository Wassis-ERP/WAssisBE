using MediatR;

namespace WAssis.Application.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>;
