using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Quotes.Repositories;

public sealed class QuoteRequestRepository(WAssisDbContext dbContext) : IQuoteRequestRepository
{
    public async Task AddAsync(QuoteRequest quoteRequest, CancellationToken cancellationToken)
    {
        await dbContext.QuoteRequests.AddAsync(quoteRequest, cancellationToken);
    }

    public Task<QuoteRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.QuoteRequests
            .Include(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
