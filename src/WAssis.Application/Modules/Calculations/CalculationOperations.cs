using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Abstractions.Messaging;

namespace WAssis.Application.Modules.Calculations;

public interface ICanonicalCalculationStore
{
    Task<CalculationAggregate?> CreateAsync(Guid opportunityId, string key, CreateAutoCalculation input, CancellationToken cancellationToken);
    Task<CalculationAggregate?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<CalculationAggregate>?> ListAsync(Guid opportunityId, int page, int pageSize, CancellationToken cancellationToken);
}

public sealed record CreateCalculationCommand(Guid OpportunityId, string IdempotencyKey, CreateAutoCalculation Input)
    : ITransactionalCommand<CalculationAggregate?>;
public sealed record GetCalculationQuery(Guid Id) : IQuery<CalculationAggregate?>;
public sealed record ListCalculationsQuery(Guid OpportunityId, int Page = 1, int PageSize = 50)
    : IQuery<PagedResult<CalculationAggregate>?>;

public sealed class CreateCalculationHandler(ICanonicalCalculationStore store) : IRequestHandler<CreateCalculationCommand, CalculationAggregate?>
{
    public Task<CalculationAggregate?> Handle(CreateCalculationCommand request, CancellationToken cancellationToken) =>
        store.CreateAsync(request.OpportunityId, request.IdempotencyKey, request.Input, cancellationToken);
}
public sealed class GetCalculationHandler(ICanonicalCalculationStore store) : IRequestHandler<GetCalculationQuery, CalculationAggregate?>
{
    public Task<CalculationAggregate?> Handle(GetCalculationQuery request, CancellationToken cancellationToken) => store.GetAsync(request.Id, cancellationToken);
}
public sealed class ListCalculationsHandler(ICanonicalCalculationStore store) : IRequestHandler<ListCalculationsQuery, PagedResult<CalculationAggregate>?>
{
    public Task<PagedResult<CalculationAggregate>?> Handle(ListCalculationsQuery request, CancellationToken cancellationToken) =>
        store.ListAsync(request.OpportunityId, Math.Max(1, request.Page), Math.Clamp(request.PageSize, 1, 100), cancellationToken);
}
