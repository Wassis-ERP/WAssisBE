namespace WAssis.Infra.Data.Configuration;

public abstract class LibertyQuoteOptionsBase
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string DocumentationUrl { get; set; } = "local://liberty-brazil-business-systems-marketplace.yaml";
    public string ApiVersion { get; set; } = "v1";
    public string ProductLine { get; set; } = string.Empty;
    public string? User { get; set; }
    public string? BrokerCode { get; set; }
    public string? BrokerBranchCode { get; set; }
    public string? CommercialProductCode { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? AccessToken { get; set; }
}
