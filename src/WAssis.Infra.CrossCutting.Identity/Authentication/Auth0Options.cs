namespace WAssis.Infra.CrossCutting.Identity.Authentication;

public sealed class Auth0Options
{
    public const string SectionName = "Identity:Auth0";
    public bool Enabled { get; set; }
    public bool RequiredInProduction { get; set; }
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string FrontendCallbackUrl { get; set; } = string.Empty;
    public string FrontendLogoutUrl { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 10;
}
