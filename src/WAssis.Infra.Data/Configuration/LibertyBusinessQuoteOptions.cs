namespace WAssis.Infra.Data.Configuration;

public sealed class LibertyBusinessQuoteOptions : LibertyQuoteOptionsBase
{
    public const string SectionName = "Quotes:Providers:Liberty:Business";

    public LibertyBusinessQuoteOptions()
    {
        ProductLine = "empresarial";
    }
}
