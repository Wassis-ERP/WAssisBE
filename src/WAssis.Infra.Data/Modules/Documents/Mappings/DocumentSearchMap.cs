using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Documents.Entities;

namespace WAssis.Infra.Data.Modules.Documents.Mappings;

public sealed class DocumentSearchMap : IEntityTypeConfiguration<DocumentSearch>
{
    public void Configure(EntityTypeBuilder<DocumentSearch> builder)
    {
        builder.ToTable("document_searches", "documents");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.InsuranceCompanyCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SearchType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.CorrelationId).IsUnique();
    }
}
