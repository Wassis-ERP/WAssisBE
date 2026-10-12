using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace WAssis.Tests.Integration;

public sealed partial class PostgresReadinessTests
{
    private static async Task VerifyQuoteIdempotencyHttp(HttpClient client)
    {
        var input = new
        {
            correlationId = "http-idempotency", customerName = "Synthetic quote",
            documentNumber = "FIXTURE-DOCUMENT", vehicleModelYear = 2026, officeBranchId = BranchA
        };
        using var created = await client.PostAsJsonAsync("/api/quotes/requests", input);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var original = await created.Content.ReadFromJsonAsync<JsonDocument>();
        using var repeated = await client.PostAsJsonAsync("/api/quotes/requests", input);
        Assert.Equal(HttpStatusCode.Created, repeated.StatusCode);
        using var retry = await repeated.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.Equal(original!.RootElement.GetProperty("id").GetGuid(), retry!.RootElement.GetProperty("id").GetGuid());
        using var conflict = await client.PostAsJsonAsync("/api/quotes/requests", input with { vehicleModelYear = 2027 });
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        var problem = await conflict.Content.ReadAsStringAsync();
        Assert.DoesNotContain(input.documentNumber, problem);
        Assert.DoesNotContain(input.customerName, problem);
        Assert.DoesNotContain(input.correlationId, problem);
        using var stored = await client.GetFromJsonAsync<JsonDocument>($"/api/quotes/requests/{original.RootElement.GetProperty("id").GetGuid()}");
        Assert.Equal(2026, stored!.RootElement.GetProperty("vehicleModelYear").GetInt32());
    }
}
