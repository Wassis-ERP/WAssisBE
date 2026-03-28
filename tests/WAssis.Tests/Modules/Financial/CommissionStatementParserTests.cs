using WAssis.Infra.Data.Integrations.Parsers;

namespace WAssis.Tests.Modules.Financial;

public sealed class CommissionStatementParserTests
{
    [Fact]
    public void Analyze_ShouldExtractStructuredLines_WhenStatementContainsExpectedMarkers()
    {
        const string text = """
            10/04/2026 Seguradora: Allianz Proposta: PROP-123 Valor: 320,45
            11/04/2026 Seguradora: Porto Seguro Referencia: REF-987 Valor: 105,90
            """;

        var parser = new CommissionStatementParser();

        var result = parser.Analyze("pdf_statement", text);
        var lines = result.Lines.ToArray();

        Assert.Equal(2, result.ParsedLines);
        Assert.Equal(0, result.LinesNeedingManualReview);
        Assert.Equal("Allianz", lines[0].InsuranceCompanyCode);
        Assert.Equal("PROP-123", lines[0].ProposalNumber);
        Assert.Equal(320.45m, lines[0].Amount);
        Assert.Equal("REF-987", lines[1].SuggestedReference);
    }

    [Fact]
    public void Analyze_ShouldFlagLineForManualReview_WhenReferenceOrAmountIsMissing()
    {
        const string text = """
            Seguradora: Allianz sem valor e sem referencia
            """;

        var parser = new CommissionStatementParser();

        var result = parser.Analyze("pdf_statement", text);
        var line = result.Lines.Single();

        Assert.Equal(1, result.ParsedLines);
        Assert.Equal(1, result.LinesNeedingManualReview);
        Assert.True(line.NeedsManualReview);
    }
}
