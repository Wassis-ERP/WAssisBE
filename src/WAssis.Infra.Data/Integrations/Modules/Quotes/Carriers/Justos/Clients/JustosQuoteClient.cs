using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Models;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Clients;

public sealed class JustosQuoteClient(HttpClient httpClient)
{
    public async Task<JsonDocument?> CreateQuoteAsync(
        string accessToken,
        JustosQuoteRequest request,
        CancellationToken cancellationToken)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await httpClient.PostAsJsonAsync("/brokers/quote", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }
}
