using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WAssis.Application.Modules.Documents.Interfaces;

namespace WAssis.Tests.Integration;

public sealed class DevelopmentStartupTests
{
    [Fact]
    public async Task ApiStartsWithDevelopmentServiceValidation_WithoutWorkerParser()
    {
        await using var factory = new DevelopmentFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"status\":\"ok\"}", await response.Content.ReadAsStringAsync());
        using var scope = factory.Services.CreateScope();
        Assert.Null(scope.ServiceProvider.GetService<IDocumentProcessingService>());
        Assert.Null(scope.ServiceProvider.GetService<IIsolatedDocumentProcessor>());
    }

    private sealed class DevelopmentFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=127.0.0.1;Database=startup_fixture;Username=unused",
                ["Database:AutoMigrate"] = "false",
                ["Documents:PrivateStorage:RootPath"] = Path.Combine(Path.GetTempPath(), "wassis-startup", Guid.NewGuid().ToString("N")),
                ["Identity:Jwt:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)),
                ["Identity:Auth0:Enabled"] = "false",
                ["Serilog:MinimumLevel:Default"] = "Fatal"
            };
            foreach (var setting in settings) builder.UseSetting(setting.Key, setting.Value);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(settings));
        }
    }
}
