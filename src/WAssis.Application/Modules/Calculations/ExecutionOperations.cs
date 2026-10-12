using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Abstractions.Messaging;

namespace WAssis.Application.Modules.Calculations;

public interface ICanonicalExecutionStore
{
    Task<CalculationExecutionRow?> CreateAsync(Guid calculationId, string key, CreateCalculationExecution input, CancellationToken token);
    Task<CalculationExecutionRow?> RetryAsync(Guid executionId, string key, RetryCalculationExecution input, CancellationToken token);
    Task<CalculationExecutionRow?> CancelAsync(Guid executionId, string key, CancellationToken token);
    Task<QuoteResultAggregate?> RecordManualQuoteAsync(Guid executionId, string key, RecordManualQuote input, CancellationToken token);
    Task<CalculationExecutionRow?> GetAsync(Guid executionId, CancellationToken token);
    Task<QuoteResultAggregate?> GetQuoteAsync(Guid quoteId, CancellationToken token);
    Task<PagedResult<CalculationExecutionRow>?> ListAsync(Guid calculationId, int page, int pageSize, CancellationToken token);
    Task<PagedResult<QuoteResultAggregate>?> ListQuotesAsync(Guid opportunityId, int page, int pageSize, CancellationToken token);
}
public sealed record CreateExecutionCommand(Guid CalculationId, string IdempotencyKey, CreateCalculationExecution Input) : ITransactionalCommand<CalculationExecutionRow?>;
public sealed record RetryExecutionCommand(Guid ExecutionId, string IdempotencyKey, RetryCalculationExecution Input) : ITransactionalCommand<CalculationExecutionRow?>;
public sealed record CancelExecutionCommand(Guid ExecutionId, string IdempotencyKey) : ITransactionalCommand<CalculationExecutionRow?>;
public sealed record RecordManualQuoteCommand(Guid ExecutionId, string IdempotencyKey, RecordManualQuote Input) : ITransactionalCommand<QuoteResultAggregate?>;
public sealed record GetExecutionQuery(Guid Id) : IQuery<CalculationExecutionRow?>;
public sealed record GetQuoteResultQuery(Guid Id) : IQuery<QuoteResultAggregate?>;
public sealed record ListExecutionsQuery(Guid CalculationId, int Page = 1, int PageSize = 50) : IQuery<PagedResult<CalculationExecutionRow>?>;
public sealed record ListQuoteResultsQuery(Guid OpportunityId, int Page = 1, int PageSize = 50) : IQuery<PagedResult<QuoteResultAggregate>?>;

public sealed class ExecutionHandlers(ICanonicalExecutionStore store) :
    IRequestHandler<CreateExecutionCommand, CalculationExecutionRow?>,
    IRequestHandler<RetryExecutionCommand, CalculationExecutionRow?>,
    IRequestHandler<CancelExecutionCommand, CalculationExecutionRow?>,
    IRequestHandler<RecordManualQuoteCommand, QuoteResultAggregate?>,
    IRequestHandler<GetExecutionQuery, CalculationExecutionRow?>,
    IRequestHandler<GetQuoteResultQuery, QuoteResultAggregate?>,
    IRequestHandler<ListExecutionsQuery, PagedResult<CalculationExecutionRow>?>,
    IRequestHandler<ListQuoteResultsQuery, PagedResult<QuoteResultAggregate>?>
{
    public Task<CalculationExecutionRow?> Handle(CreateExecutionCommand r, CancellationToken t) => store.CreateAsync(r.CalculationId, r.IdempotencyKey, r.Input, t);
    public Task<CalculationExecutionRow?> Handle(RetryExecutionCommand r, CancellationToken t) => store.RetryAsync(r.ExecutionId, r.IdempotencyKey, r.Input, t);
    public Task<CalculationExecutionRow?> Handle(CancelExecutionCommand r, CancellationToken t) => store.CancelAsync(r.ExecutionId, r.IdempotencyKey, t);
    public Task<QuoteResultAggregate?> Handle(RecordManualQuoteCommand r, CancellationToken t) => store.RecordManualQuoteAsync(r.ExecutionId, r.IdempotencyKey, r.Input, t);
    public Task<CalculationExecutionRow?> Handle(GetExecutionQuery r, CancellationToken t) => store.GetAsync(r.Id, t);
    public Task<QuoteResultAggregate?> Handle(GetQuoteResultQuery r, CancellationToken t) => store.GetQuoteAsync(r.Id, t);
    public Task<PagedResult<CalculationExecutionRow>?> Handle(ListExecutionsQuery r, CancellationToken t) => store.ListAsync(r.CalculationId, Math.Max(1, r.Page), Math.Clamp(r.PageSize, 1, 100), t);
    public Task<PagedResult<QuoteResultAggregate>?> Handle(ListQuoteResultsQuery r, CancellationToken t) => store.ListQuotesAsync(r.OpportunityId, Math.Max(1, r.Page), Math.Clamp(r.PageSize, 1, 100), t);
}
