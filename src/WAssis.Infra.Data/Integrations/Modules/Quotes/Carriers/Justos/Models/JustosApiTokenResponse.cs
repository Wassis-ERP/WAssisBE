using System.Text.Json.Serialization;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Models;

public sealed record JustosApiTokenResponse([property: JsonPropertyName("token")] string Token);
