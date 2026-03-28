using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Application.Modules.Quotes.Services;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Tests.Modules.Quotes;

public sealed class QuoteProcessingServiceTests
{
    [Fact]
    public async Task ProcessPendingBatchAsync_ShouldPersistProviderOption_AndCompleteRequest()
    {
        var quoteRequest = QuoteRequest.Create(
            "corr-proc-1",
            "Cliente Teste",
            "12345678910",
            "cliente@teste.local",
            "5511999999999",
            "05516020",
            "Silva",
            "M",
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "ABC1D23",
            "Ford",
            "Ka",
            "023108-8",
            2022,
            false,
            false,
            "0",
            15,
            null);

        var repository = new InMemoryQuoteRequestRepository(quoteRequest);
        var service = new QuoteProcessingService(
            repository,
            new FakeQuoteProviderRegistry([new SuccessQuoteProvider()]),
            new FakeAuditTrailWriter());

        var processedCount = await service.ProcessPendingBatchAsync(25, CancellationToken.None);

        Assert.Equal(1, processedCount);
        Assert.Single(quoteRequest.Options);
        Assert.Equal(QuoteRequestStatus.Completed, quoteRequest.Status);
        Assert.Equal("justos", quoteRequest.Options.Single().InsuranceCompanyCode);
    }

    private sealed class InMemoryQuoteRequestRepository(QuoteRequest quoteRequest) : IQuoteRequestRepository
    {
        public Task AddAsync(QuoteRequest item, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<QuoteRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(id == quoteRequest.Id ? quoteRequest : null);
        }

        public Task<QuoteRequest?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(correlationId == quoteRequest.CorrelationId ? quoteRequest : null);
        }

        public Task<IReadOnlyCollection<QuotePendingDispatchDto>> GetPendingDispatchBatchAsync(int batchSize, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<QuotePendingDispatchDto> items =
            [
                new QuotePendingDispatchDto(
                    quoteRequest.Id,
                    quoteRequest.CorrelationId,
                    quoteRequest.CreatedAtUtc,
                    quoteRequest.CustomerName,
                    quoteRequest.VehiclePlate)
            ];

            return Task.FromResult(items);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class SuccessQuoteProvider : IQuoteProvider
    {
        public string ProviderCode => "justos";
        public string ProviderName => "Justos";
        public bool IsEnabled => true;

        public QuoteProviderDescriptorDto Describe()
        {
            return new QuoteProviderDescriptorDto(
                ProviderCode,
                ProviderName,
                true,
                true,
                "JWT ES256 + API Token",
                "Auto",
                "https://justos.notion.site/Documenta-o-da-API-Justos-4f0a82f7b85547319fa5ca7cef7790ce",
                [],
                []);
        }

        public Task<IReadOnlyCollection<QuoteProviderResultDto>> StartQuoteAsync(StartQuoteProcessingDto request, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<QuoteProviderResultDto> results =
            [
                new QuoteProviderResultDto(
                    ProviderCode,
                    ProviderName,
                    QuoteOptionStatus.Ok,
                    "quote-001",
                    1500m,
                    225m,
                    [],
                    [],
                    [new QuoteStatusMessageDto("ok", "Cotação processada.")])
            ];

            return Task.FromResult(results);
        }
    }

    private sealed class FakeQuoteProviderRegistry(IReadOnlyCollection<IQuoteProvider> providers) : IQuoteProviderRegistry
    {
        public IReadOnlyCollection<IQuoteProvider> GetEnabledProviders() => providers;

        public IReadOnlyCollection<QuoteProviderDescriptorDto> DescribeProviders()
        {
            return providers.Select(static provider => provider.Describe()).ToArray();
        }
    }

    private sealed class FakeAuditTrailWriter : IAuditTrailWriter
    {
        public Task WriteAsync(
            string correlationId,
            string module,
            string action,
            string entityType,
            string entityId,
            string? notes,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
