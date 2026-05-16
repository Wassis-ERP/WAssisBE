using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Application.Modules.Quotes.Services;

namespace WAssis.Tests.Modules.Quotes;

public sealed class QuoteProviderRegistryTests
{
    [Fact]
    public async Task GetEnabledProvidersAsync_ShouldExcludeProviderDisabledByDatabase()
    {
        var registry = new QuoteProviderRegistry(
            [new FakeQuoteProvider("aggilizador_auto", true), new FakeQuoteProvider("justos_auto", true)],
            new FakeActivationRepository(new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                ["aggilizador_auto"] = false
            }));

        var providers = await registry.GetEnabledProvidersAsync(CancellationToken.None);

        Assert.Single(providers);
        Assert.Equal("justos_auto", providers.Single().ProviderCode);
    }

    [Fact]
    public async Task DescribeProvidersAsync_ShouldExposeDisabledMessage_WhenDatabaseToggleIsOff()
    {
        var registry = new QuoteProviderRegistry(
            [new FakeQuoteProvider("aggilizador_auto", true)],
            new FakeActivationRepository(new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                ["aggilizador_auto"] = false
            }));

        var descriptors = await registry.DescribeProvidersAsync(CancellationToken.None);

        var descriptor = Assert.Single(descriptors);
        Assert.False(descriptor.IsEnabled);
        Assert.Contains(descriptor.Messages, static message => message.Code == "disabled_by_database_toggle");
    }

    private sealed class FakeActivationRepository(IReadOnlyDictionary<string, bool> activationMap) : IQuoteProviderActivationRepository
    {
        public Task<IReadOnlyDictionary<string, bool>> GetActivationMapAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(activationMap);
        }
    }

    private sealed class FakeQuoteProvider(string providerCode, bool enabled) : IQuoteProvider
    {
        public string ProviderCode => providerCode;
        public string ProviderName => providerCode;
        public bool IsEnabled => enabled;

        public QuoteProviderDescriptorDto Describe()
        {
            return new QuoteProviderDescriptorDto(
                ProviderCode,
                ProviderName,
                IsEnabled,
                true,
                "none",
                "auto",
                null,
                [],
                []);
        }

        public Task<IReadOnlyCollection<QuoteProviderResultDto>> StartQuoteAsync(StartQuoteProcessingDto request, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<QuoteProviderResultDto>>([]);
        }
    }
}
