using System.Text.Json;
using Microsoft.Extensions.Configuration;
using WAssis.Infra.CrossCutting.Identity.Authentication;
using WAssis.Infra.CrossCutting.IoC;

namespace WAssis.Tests.Security;

public sealed class DockerSecretConfigurationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("55555555-5555-5555-5555-555555555555")]
    public void MountedIdentity_PreservesOptionalSellerId(string? sellerId)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"wassis-secret-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var user = new HomologationAuthUserOptions
            {
                Username = "fixture@example.invalid",
                UserId = "11111111-1111-1111-1111-111111111111",
                BranchIds = ["44444444-4444-4444-4444-444444444444"],
                SellerId = sellerId
            };
            File.WriteAllText(Path.Combine(directory, "wassis_identity_users"), JsonSerializer.Serialize(new[] { user }));
            using var configuration = new ConfigurationManager();
            configuration.AddWAssisDockerSecrets(directory);

            var options = configuration.GetSection(HomologationAuthOptions.SectionName).Get<HomologationAuthOptions>();
            var bound = Assert.Single(Assert.IsType<HomologationAuthOptions>(options).Users);
            Assert.Equal(sellerId, bound.SellerId);
            Assert.Equal(user.UserId, bound.UserId);
            Assert.Equal(user.BranchIds, bound.BranchIds);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
