using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;

namespace WAssis.Application.Modules.Quotes.Services;

public sealed class QuoteProviderRegistry(
    IEnumerable<IQuoteProvider> providers,
    IQuoteProviderActivationRepository activationRepository)
    : IQuoteProviderRegistry
{
    public async Task<IReadOnlyCollection<IQuoteProvider>> GetEnabledProvidersAsync(CancellationToken cancellationToken)
    {
        var activationMap = await activationRepository.GetActivationMapAsync(cancellationToken);

        return providers
            .Where(provider => provider.IsEnabled && IsActivatedByDatabase(provider.ProviderCode, activationMap))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<QuoteProviderDescriptorDto>> DescribeProvidersAsync(CancellationToken cancellationToken)
    {
        var activationMap = await activationRepository.GetActivationMapAsync(cancellationToken);

        return providers
            .Select(provider => ApplyActivation(provider.Describe(), activationMap))
            .OrderBy(static provider => provider.ProviderName)
            .ToArray();
    }

    private static QuoteProviderDescriptorDto ApplyActivation(
        QuoteProviderDescriptorDto descriptor,
        IReadOnlyDictionary<string, bool> activationMap)
    {
        if (!activationMap.TryGetValue(descriptor.ProviderCode, out var isDatabaseEnabled))
        {
            return descriptor;
        }

        if (isDatabaseEnabled)
        {
            return descriptor;
        }

        return descriptor with
        {
            IsEnabled = false,
            Messages = descriptor.Messages
                .Append(new QuoteStatusMessageDto(
                    "disabled_by_database_toggle",
                    "Provider desativado pela configuracao persistida no banco de dados."))
                .ToArray()
        };
    }

    private static bool IsActivatedByDatabase(
        string providerCode,
        IReadOnlyDictionary<string, bool> activationMap)
    {
        return !activationMap.TryGetValue(providerCode, out var isDatabaseEnabled) || isDatabaseEnabled;
    }
}
