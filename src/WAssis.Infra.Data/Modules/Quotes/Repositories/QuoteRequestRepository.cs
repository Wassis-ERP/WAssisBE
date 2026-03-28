using WAssis.Application.Modules.Quotes.Dtos;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.Quotes.Enums;
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

    public Task<QuoteRequest?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken)
    {
        return dbContext.QuoteRequests
            .Include(x => x.Options)
            .SingleOrDefaultAsync(x => x.CorrelationId == correlationId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<QuotePendingDispatchDto>> GetPendingDispatchBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        return await dbContext.QuoteRequests
            .AsNoTracking()
            .Where(x => x.Status == QuoteRequestStatus.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(batchSize)
            .Select(x => new QuotePendingDispatchDto(
                x.Id,
                x.CorrelationId,
                x.CreatedAtUtc,
                x.CustomerName,
                x.VehiclePlate))
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
