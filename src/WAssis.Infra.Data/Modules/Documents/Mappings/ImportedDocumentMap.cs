using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Documents.Entities;

namespace WAssis.Infra.Data.Modules.Documents.Mappings;

public sealed class ImportedDocumentMap : IEntityTypeConfiguration<ImportedDocument>
{
    public void Configure(EntityTypeBuilder<ImportedDocument> builder)
    {
        builder.ToTable("imported_documents", "documents");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.DocumentType).HasMaxLength(50);
        builder.Property(x => x.ExtractedText).HasColumnType("text");
        builder.Property(x => x.InsuranceCompanyName).HasMaxLength(120);
        builder.Property(x => x.ProposalNumber).HasMaxLength(100);
        builder.Property(x => x.InsuredName).HasMaxLength(200);
        builder.Property(x => x.TotalPremiumAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.CommissionAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.ParsingNotes).HasMaxLength(2000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
