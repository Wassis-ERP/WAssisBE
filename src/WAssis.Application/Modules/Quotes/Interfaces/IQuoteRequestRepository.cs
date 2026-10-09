using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Application.Abstractions;

namespace WAssis.Application.Modules.Quotes.Interfaces;

public interface IQuoteRequestRepository
{
    Task<QuoteRequest> CreateIdempotentAsync(QuoteRequest quoteRequest, CancellationToken cancellationToken);
    Task AddAsync(QuoteRequest quoteRequest, CancellationToken cancellationToken);
    Task<QuoteRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<QuoteRequest?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken);
    Task<PagedResult<QuoteRequest>> ListAsync(Guid? opportunityId, string? officeBranchId, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<QuotePendingDispatchDto>> GetPendingDispatchBatchAsync(int batchSize, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
