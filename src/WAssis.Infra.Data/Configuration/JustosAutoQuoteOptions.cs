namespace WAssis.Infra.Data.Configuration;

public sealed class JustosAutoQuoteOptions
{
    public const string SectionName = "Quotes:Providers:Justos:Auto";

    public bool Enabled { get; set; } = true;
    public string BaseUrl { get; set; } = "https://api.justos.com.br";
    public string DocumentationUrl { get; set; } = "https://justos.notion.site/Documenta-o-da-API-Justos-4f0a82f7b85547319fa5ca7cef7790ce";
    public string? BrokerId { get; set; }
    public string? Issuer { get; set; }
    public string? PrivateKeyPemPath { get; set; }
    public string DefaultCommissionPercentage { get; set; } = "15";
}
