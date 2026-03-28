using WAssis.Infra.Data.Integrations.Parsers;

namespace WAssis.Tests.Modules.Documents;

public sealed class ProposalDocumentParserTests
{
    [Fact]
    public void Parse_ShouldExtractCoreProposalFields()
    {
        const string text = """
            Seguradora: Allianz
            Número da proposta: PROP-2026-0001
            Segurado: Guilherme Ramos
            Início de vigência: 01/04/2026
            Fim de vigência: 01/04/2027
            Prêmio total: 1.250,55
            Comissão: 250,10
            """;

        var parser = new ProposalDocumentParser();

        var result = parser.Parse(text);

        Assert.Equal("proposal_pdf", result.DocumentType);
        Assert.Equal("Allianz", result.InsuranceCompanyName);
        Assert.Equal("PROP-2026-0001", result.ProposalNumber);
        Assert.Equal("Guilherme Ramos", result.InsuredName);
        Assert.Equal(1250.55m, result.TotalPremiumAmount);
        Assert.Equal(250.10m, result.CommissionAmount);
        Assert.NotNull(result.CoverageStartDateUtc);
        Assert.NotNull(result.CoverageEndDateUtc);
        Assert.True(result.ParsingConfidence >= 0.80m);
        Assert.False(result.RequiresHumanReview);
    }

    [Fact]
    public void Parse_ShouldApplyCarrierProfile_WhenCarrierHeaderIsPresent()
    {
        const string text = """
            PORTO SEGURO AUTO
            Proposta Porto: PS-7788
            Segurado(a): Maria Silva
            Das: 10/04/2026
            Até: 10/04/2027
            Valor total do prêmio: 2.345,67
            Comissão total: 320,45
            """;

        var parser = new ProposalDocumentParser();

        var result = parser.Parse(text);

        Assert.Equal("Porto Seguro", result.InsuranceCompanyName);
        Assert.Equal("PS-7788", result.ProposalNumber);
        Assert.Equal("Maria Silva", result.InsuredName);
        Assert.Contains("Porto Seguro", result.ParsingNotes);
        Assert.True(result.ParsingConfidence >= 0.80m);
    }

    [Fact]
    public void Parse_ShouldRequireHumanReview_WhenOnlySparseDataIsAvailable()
    {
        const string text = """
            Documento simples
            Segurado: Cliente Parcial
            """;

        var parser = new ProposalDocumentParser();

        var result = parser.Parse(text);

        Assert.Equal("Cliente Parcial", result.InsuredName);
        Assert.True(result.RequiresHumanReview);
        Assert.True(result.ParsingConfidence < 0.80m);
    }
}
