using Microsoft.EntityFrameworkCore;
using WAssis.Domain.Modules.Quotes.Entities;

namespace WAssis.Infra.Data.Context;

public class WAssisDbContext(DbContextOptions<WAssisDbContext> options) : DbContext(options)
{
    public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();
    public DbSet<QuoteOption> QuoteOptions => Set<QuoteOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WAssisDbContext).Assembly);
    }
}
