using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Domain.Modules.Quotes.Entities;

namespace WAssis.Application.Modules.Quotes.Interfaces;

public interface IQuoteRequestRepository
{
    Task AddAsync(QuoteRequest quoteRequest, CancellationToken cancellationToken);
    Task<QuoteRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<QuoteRequest?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<QuoteRequest>> ListAsync(Guid? opportunityId, string? officeBranchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<QuotePendingDispatchDto>> GetPendingDispatchBatchAsync(int batchSize, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
