using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WAssis.Application.Abstractions;

namespace WAssis.Infra.Data.Context;

public sealed class WAssisDesignTimeDbContextFactory : IDesignTimeDbContextFactory<WAssisDbContext>
{
    public WAssisDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WAssisDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=wassis;Username=postgres;Password=postgres",
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "quotes"));

        return new WAssisDbContext(optionsBuilder.Options, new DesignTimeCurrentUserContext());
    }

    private sealed class DesignTimeCurrentUserContext : ICurrentUserContext
    {
        public bool IsAuthenticated => false;
        public string? UserId => null;
        public string? TenantId => null;
        public string? BrokerageId => null;
        public string? SellerId => null;
        public string? UserType => null;
        public IReadOnlyCollection<string> Roles => [];

        public bool IsInRole(string role)
        {
            return false;
        }
    }
}
