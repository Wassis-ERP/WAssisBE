namespace WAssis.Infra.Data.Configuration;

public sealed class LibertyAutoQuoteOptions : LibertyQuoteOptionsBase
{
    public const string SectionName = "Quotes:Providers:Liberty:Auto";

    public LibertyAutoQuoteOptions()
    {
        ProductLine = "auto";
    }
}
