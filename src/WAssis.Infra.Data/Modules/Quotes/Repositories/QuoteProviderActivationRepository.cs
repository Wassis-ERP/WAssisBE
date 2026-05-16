using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Quotes.Repositories;

public sealed class QuoteProviderActivationRepository(WAssisDbContext dbContext) : IQuoteProviderActivationRepository
{
    public async Task<IReadOnlyDictionary<string, bool>> GetActivationMapAsync(CancellationToken cancellationToken)
    {
        return await dbContext.QuoteProviderActivationSettings
            .AsNoTracking()
            .ToDictionaryAsync(
                x => x.ProviderCode,
                x => x.IsEnabled,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);
    }
}
