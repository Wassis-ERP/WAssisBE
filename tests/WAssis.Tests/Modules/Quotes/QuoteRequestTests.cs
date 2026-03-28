using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Tests.Modules.Quotes;

public sealed class QuoteRequestTests
{
    [Fact]
    public void Create_ShouldInitializeShareTokenAndPendingStatus()
    {
        var quoteRequest = QuoteRequest.Create(
            "corr-domain",
            "Cliente Domain",
            "DOC-999",
            null,
            null,
            "BRA2E19",
            "Chevrolet",
            "Onix",
            2024);

        Assert.Equal(QuoteRequestStatus.Pending, quoteRequest.Status);
        Assert.False(string.IsNullOrWhiteSpace(quoteRequest.ShareToken));
        Assert.Equal("Cliente Domain", quoteRequest.CustomerName);
    }
}
