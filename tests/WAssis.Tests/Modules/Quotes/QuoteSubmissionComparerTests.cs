using WAssis.Domain.Modules.Quotes;
using WAssis.Domain.Modules.Quotes.ValueObjects;
using WAssis.Tests.Integration;

namespace WAssis.Tests.Modules.Quotes;

public sealed class QuoteSubmissionComparerTests
{
    [Fact]
    public void RetryIgnoresGeneratedStateAndCoverageOrdering()
    {
        var submission = AutoQuoteSubmissionTests.Submission();
        var original = DurableDispatchTests.Quote("same", submission: submission);
        original.MarkAsProcessing();
        var retry = DurableDispatchTests.Quote("same", submission: submission with { Coverages = submission.Coverages.Reverse().ToArray() });
        Assert.NotEqual(original.Id, retry.Id);
        Assert.True(QuoteSubmissionComparer.Matches(original, retry));
    }

    [Theory]
    [InlineData("driver")]
    [InlineData("dates")]
    [InlineData("bonus")]
    [InlineData("limit")]
    [InlineData("selection")]
    [InlineData("missing")]
    public void ChangedSubmissionCannotReplayAnotherVersion(string change)
    {
        var submission = AutoQuoteSubmissionTests.Submission();
        AutoQuoteSubmission? changed = change switch
        {
            "driver" => submission with { DriverDocument = "00000000002" },
            "dates" => submission with { CoverageStartsOn = submission.CoverageStartsOn.AddDays(1) },
            "bonus" => submission with { BonusClass = null },
            "limit" => AutoQuoteSubmissionTests.Submission(200000),
            "selection" => submission with { Coverages = submission.Coverages.Select(x => x with { Selected = !x.Selected }).ToArray() },
            _ => null
        };
        Assert.False(QuoteSubmissionComparer.Matches(
            DurableDispatchTests.Quote("same", submission: submission),
            DurableDispatchTests.Quote("same", submission: changed)));
    }

    [Fact]
    public void ScopeIsPartOfTheInput()
    {
        var original = DurableDispatchTests.Quote("same");
        Assert.False(QuoteSubmissionComparer.Matches(original, DurableDispatchTests.Quote("same", tenantId: "another")));
        Assert.False(QuoteSubmissionComparer.Matches(original, DurableDispatchTests.Quote("same", branchId: "another")));
    }
}
