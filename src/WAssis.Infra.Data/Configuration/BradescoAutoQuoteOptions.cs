namespace WAssis.Infra.Data.Configuration;

public sealed class BradescoAutoQuoteOptions
{
    public const string SectionName = "Quotes:Providers:BradescoSeguros:Auto";

    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "https://apiportal.bradescoseguros.com.br";
    public string DocumentationUrl { get; set; } = "https://apiportal.bradescoseguros.com.br/pages/Portal_UI_Bundle/documentacaotecnica/credenciais.html";
    public string? ProductLine { get; set; } = "auto";
    public string? TokenUrl { get; set; } = "https://susc.hml.bradescoseguros.com.br:8443/V3/Auth";
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public bool RequiresMutualTls { get; set; } = true;
    public string? ClientCertificatePath { get; set; }
    public string? ClientCertificatePassword { get; set; }
    public string? ClientCertificateThumbprint { get; set; }
}
