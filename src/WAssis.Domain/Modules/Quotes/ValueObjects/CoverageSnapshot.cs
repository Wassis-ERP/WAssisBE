namespace WAssis.Domain.Modules.Quotes.ValueObjects;

public class CoverageSnapshot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal? InsuredAmount { get; private set; }
    public decimal? DeductibleAmount { get; private set; }

    private CoverageSnapshot()
    {
    }

    public CoverageSnapshot(string code, string name, decimal? insuredAmount, decimal? deductibleAmount)
    {
        Code = code;
        Name = name;
        InsuredAmount = insuredAmount;
        DeductibleAmount = deductibleAmount;
    }
}
