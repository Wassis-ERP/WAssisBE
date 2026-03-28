namespace WAssis.Infra.Data.Configuration;

public sealed class IcatuQuoteOptions
{
    public const string SectionName = "Quotes:Providers:IcatuSeguros";

    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "https://portal-api.icatuseguros.com.br";
    public string DocumentationUrl { get; set; } = "https://portal-api.icatuseguros.com.br/apis";
    public string? ApiCatalogKey { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? ProductLine { get; set; }
    public string? PartnerId { get; set; }
    public string? ApplicationId { get; set; }
    public string? CertificateId { get; set; }
}
