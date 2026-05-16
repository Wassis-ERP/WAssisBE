namespace WAssis.Application.Modules.Quotes.Interfaces;

public interface IQuoteProviderActivationRepository
{
    Task<IReadOnlyDictionary<string, bool>> GetActivationMapAsync(CancellationToken cancellationToken);
}
