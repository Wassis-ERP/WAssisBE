using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using WAssis.Infra.CrossCutting.Identity.Authentication;

namespace WAssis.Infra.CrossCutting.IoC;

public static class DockerSecretConfiguration
{
    // Stable mount targets; Docker secret resource names remain environment/version-specific.
    public static void AddWAssisDockerSecrets(this ConfigurationManager configuration, string directory = "/run/secrets")
    {
        if (!Directory.Exists(directory)) return;
        var values = new Dictionary<string, string?>();
        foreach (var pair in new[]
        {
            (File: "wassis_db_connection", Key: "ConnectionStrings:DefaultConnection"),
            (File: "wassis_jwt_signing_key", Key: "Identity:Jwt:SigningKey")
        })
        {
            var content = ReadSecret(Path.Combine(directory, pair.File));
            if (content is not null) values[pair.Key] = content.Trim();
        }
        configuration.AddInMemoryCollection(values);
        var users = ReadSecret(Path.Combine(directory, "wassis_identity_users"));
        if (users is null) return;
        if (configuration.GetSection("Identity:HomologationAuth:Users").GetChildren().Any())
            throw new InvalidOperationException("Configure homologation users exclusively in the mounted secret; mixed sources are forbidden.");
        try
        {
            var identities = JsonSerializer.Deserialize<List<HomologationAuthUserOptions>>(users,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (identities is null || identities.Count == 0)
                throw new InvalidOperationException("The homologation identity secret must contain a nonempty user array.");
            // .NET 8 JSON configuration binds explicit null strings as empty strings.
            // Omit optional values so users without a linked producer retain SellerId=null.
            var wrapped = JsonSerializer.Serialize(new { Identity = new { HomologationAuth = new { Users = identities } } },
                new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(wrapped)));
        }
        catch (JsonException) { throw new InvalidOperationException("The homologation identity secret must contain a valid user array."); }
    }

    private static string? ReadSecret(string path)
    {
        if (!File.Exists(path)) return null;
        if (new FileInfo(path).Length > 65_536) throw new InvalidOperationException("A mounted configuration secret exceeds the size limit.");
        return File.ReadAllText(path);
    }
}
