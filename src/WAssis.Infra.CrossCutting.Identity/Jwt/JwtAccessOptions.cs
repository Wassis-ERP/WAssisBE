namespace WAssis.Infra.CrossCutting.Identity.Jwt;

public sealed class JwtAccessOptions
{
    public const string SectionName = "Identity:Jwt";

    public string Issuer { get; set; } = "WAssis";
    public string Audience { get; set; } = "WAssis.Clients";
    public string SigningKey { get; set; } = "change-this-signing-key-for-real-environments-12345";
    public bool RequireHttpsMetadata { get; set; }
}
