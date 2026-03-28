using WAssis.Domain.Modules.Quotes.Entities;

namespace WAssis.Application.Modules.Quotes.Interfaces;

public interface IQuoteRequestRepository
{
    Task AddAsync(QuoteRequest quoteRequest, CancellationToken cancellationToken);
    Task<QuoteRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
