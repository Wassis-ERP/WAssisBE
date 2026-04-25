namespace WAssis.Infra.Data.Configuration;

public sealed class LibertyLifeQuoteOptions : LibertyQuoteOptionsBase
{
    public const string SectionName = "Quotes:Providers:Liberty:Life";

    public LibertyLifeQuoteOptions()
    {
        ProductLine = "vida";
    }
}
