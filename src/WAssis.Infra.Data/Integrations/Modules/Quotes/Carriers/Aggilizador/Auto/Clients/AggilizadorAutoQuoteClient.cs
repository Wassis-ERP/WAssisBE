using System.Text;
using System.Text.Json;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Aggilizador.Auto.Clients;

public sealed class AggilizadorAutoQuoteClient(HttpClient httpClient)
{
    public async Task<JsonDocument?> CreateQuoteAsync(object payload, CancellationToken cancellationToken)
    {
        using var requestContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.PostAsync("Auto", requestContent, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return JsonDocument.Parse("{}");
        }

        return JsonDocument.Parse(responseContent);
    }
}
