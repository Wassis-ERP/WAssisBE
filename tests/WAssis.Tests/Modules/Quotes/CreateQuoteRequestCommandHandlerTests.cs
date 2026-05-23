using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Commands;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Modules.Quotes;

public sealed class CreateQuoteRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreatePendingQuoteRequest()
    {
        var repository = new InMemoryQuoteRequestRepository();
        var handler = new CreateQuoteRequestCommandHandler(
            repository,
            new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                TenantId = "tenant-alpha"
            });

        var result = await handler.Handle(
            new CreateQuoteRequestCommand(
                "corr-123",
                "Cliente Teste",
                "DOC-123",
                "cliente@teste.local",
                "5511999999999",
                "05516020",
                "Silva",
                "M",
                "1",
                new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                12,
                "12345678901",
                "0",
                "9BWZZZ377VT004251",
                "ABC1D23",
                "Ford",
                "Ka",
                "023108-8",
                2021,
                2022,
                false,
                false,
                true,
                false,
                false,
                "1",
                "05516020",
                false,
                true,
                false,
                "0",
                15,
                null),
            CancellationToken.None);

        Assert.Equal("corr-123", result.CorrelationId);
        Assert.Equal(QuoteRequestStatus.Pending, result.Status);
        Assert.Equal("05516020", result.PostalCode);
        Assert.Equal("023108-8", result.VehicleFipeCode);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Single(repository.Items);
        Assert.Equal("tenant-alpha", repository.Items.Single().TenantId);
    }

    private sealed class InMemoryQuoteRequestRepository : IQuoteRequestRepository
    {
        public List<QuoteRequest> Items { get; } = [];

        public Task AddAsync(QuoteRequest quoteRequest, CancellationToken cancellationToken)
        {
            Items.Add(quoteRequest);
            return Task.CompletedTask;
        }

        public Task<QuoteRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.SingleOrDefault(x => x.Id == id));
        }

        public Task<QuoteRequest?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Items.SingleOrDefault(x => x.CorrelationId == correlationId));
        }

        public Task<IReadOnlyCollection<QuotePendingDispatchDto>> GetPendingDispatchBatchAsync(int batchSize, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<QuotePendingDispatchDto> items = Items
                .Take(batchSize)
                .Select(x => new QuotePendingDispatchDto(
                    x.Id,
                    x.CorrelationId,
                    x.CreatedAtUtc,
                    x.CustomerName,
                    x.VehiclePlate))
                .ToArray();

            return Task.FromResult(items);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
