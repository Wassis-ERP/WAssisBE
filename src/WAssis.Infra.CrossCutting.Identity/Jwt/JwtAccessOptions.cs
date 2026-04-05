namespace WAssis.Infra.CrossCutting.Identity.Jwt;

public sealed class JwtAccessOptions
{
    public const string SectionName = "Identity:Jwt";

    public string Issuer { get; set; } = "WAssis";
    public string Audience { get; set; } = "WAssis.Clients";
    public string SigningKey { get; set; } = "__configure_in_user_secrets_or_env__";
    public bool RequireHttpsMetadata { get; set; }
}
