using Microsoft.EntityFrameworkCore;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Infra.Data.Context;
using WAssis.Tests.Integration;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Modules.Quotes;

public sealed class AutoQuotePersistenceTests
{
    [Fact]
    public async Task SnapshotRoundtrip_AndDefensiveCopy_PreserveVersions()
    {
        var input = AutoQuoteSubmissionTests.Submission();
        var first = DurableDispatchTests.Quote("first", submission: input);
        var second = DurableDispatchTests.Quote("second", submission: AutoQuoteSubmissionTests.Submission(200000));
        Assert.NotSame(input, first.AutoSubmission);
        Assert.NotSame(input.Coverages, first.AutoSubmission!.Coverages);
        var options = new DbContextOptionsBuilder<WAssisDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var user = new FakeCurrentUserContext { IsAuthenticated = true, TenantId = first.TenantId,
            BranchId = first.OfficeBranchId, BranchIds = [first.OfficeBranchId!] };
        await using (var db = new WAssisDbContext(options, user))
        {
            db.QuoteRequests.AddRange(first, second);
            await db.SaveChangesAsync();
        }
        await using var reloaded = new WAssisDbContext(options, user);
        var versions = await reloaded.QuoteRequests.OrderBy(x => x.CorrelationId).ToArrayAsync();
        Assert.Equal(2, versions.Length);
        Assert.Equal(100000, versions[0].AutoSubmission!.Coverages.Single(x => x.Code == "danos-materiais").RequestedLimit);
        Assert.Equal(200000, versions[1].AutoSubmission!.Coverages.Single(x => x.Code == "danos-materiais").RequestedLimit);
        Assert.Equal(new DateOnly(2028, 2, 29), versions[0].AutoSubmission!.CoverageStartsOn);
        Assert.Equal("Condutora da versão", versions[0].AutoSubmission!.DriverName);
    }

    [Theory]
    [InlineData(QuoteOptionStatus.Pending, QuoteRequestStatus.AwaitingResults)]
    [InlineData(QuoteOptionStatus.Restriction, QuoteRequestStatus.Failed)]
    [InlineData(QuoteOptionStatus.Failure, QuoteRequestStatus.Failed)]
    [InlineData(QuoteOptionStatus.Ok, QuoteRequestStatus.Completed)]
    public void AcknowledgementAndFailureAreNotCompletedQuotes(QuoteOptionStatus result, QuoteRequestStatus expected)
    {
        var quote = DurableDispatchTests.Quote("status");
        quote.AddOption(new QuoteOption(Guid.NewGuid(), quote.TenantId, quote.Id, "carrier", "Carrier",
            "auto", "Auto", result, null, null, "ref", [], [], [], "engine", "Engine"));
        quote.MarkAsCompleted();
        Assert.Equal(expected, quote.Status);
        Assert.Equal("engine", quote.Options.Single().ProviderCode);
        Assert.Equal("carrier", quote.Options.Single().InsuranceCompanyCode);
    }
}
