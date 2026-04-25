namespace WAssis.Infra.Data.Configuration;

public sealed class LibertyResidenceQuoteOptions : LibertyQuoteOptionsBase
{
    public const string SectionName = "Quotes:Providers:Liberty:Residence";

    public LibertyResidenceQuoteOptions()
    {
        ProductLine = "residencial";
    }
}
