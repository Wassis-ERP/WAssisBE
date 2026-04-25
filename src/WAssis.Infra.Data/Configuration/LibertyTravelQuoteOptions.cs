namespace WAssis.Infra.Data.Configuration;

public sealed class LibertyTravelQuoteOptions : LibertyQuoteOptionsBase
{
    public const string SectionName = "Quotes:Providers:Liberty:Travel";

    public LibertyTravelQuoteOptions()
    {
        ProductLine = "seguro_viagem";
    }
}
