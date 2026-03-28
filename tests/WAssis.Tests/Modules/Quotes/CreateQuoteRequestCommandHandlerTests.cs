using WAssis.Application.Modules.Quotes.Commands;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Tests.Modules.Quotes;

public sealed class CreateQuoteRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreatePendingQuoteRequest()
    {
        var repository = new InMemoryQuoteRequestRepository();
        var handler = new CreateQuoteRequestCommandHandler(repository);

        var result = await handler.Handle(
            new CreateQuoteRequestCommand(
                "corr-123",
                "Cliente Teste",
                "DOC-123",
                "cliente@teste.local",
                "5511999999999",
                "ABC1D23",
                "Ford",
                "Ka",
                2022),
            CancellationToken.None);

        Assert.Equal("corr-123", result.CorrelationId);
        Assert.Equal(QuoteRequestStatus.Pending, result.Status);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Single(repository.Items);
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

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
