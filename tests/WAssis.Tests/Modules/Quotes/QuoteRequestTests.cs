using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Tests.Modules.Quotes;

public sealed class QuoteRequestTests
{
    [Fact]
    public void Create_ShouldInitializeShareTokenAndPendingStatus()
    {
        var quoteRequest = QuoteRequest.Create(
            "tenant-domain",
            "corr-domain",
            "Cliente Domain",
            "DOC-999",
            null,
            null,
            "05516020",
            "Domain",
            "F",
            "2",
            new DateTime(1994, 6, 10, 0, 0, 0, DateTimeKind.Utc),
            9,
            "99887766",
            "0",
            "9BWZZZ377VT004251",
            "BRA2E19",
            "Chevrolet",
            "Onix",
            "004411-0",
            2023,
            2024,
            false,
            false,
            true,
            false,
            false,
            "1",
            "05516020",
            false,
            false,
            true,
            "5",
            15,
            null);

        Assert.Equal(QuoteRequestStatus.Pending, quoteRequest.Status);
        Assert.Equal("tenant-domain", quoteRequest.TenantId);
        Assert.False(string.IsNullOrWhiteSpace(quoteRequest.ShareToken));
        Assert.Equal("Cliente Domain", quoteRequest.CustomerName);
        Assert.Equal("05516020", quoteRequest.PostalCode);
    }
}
