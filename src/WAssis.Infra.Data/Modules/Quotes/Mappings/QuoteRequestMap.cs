using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Quotes.Entities;

namespace WAssis.Infra.Data.Modules.Quotes.Mappings;

public sealed class QuoteRequestMap : IEntityTypeConfiguration<QuoteRequest>
{
    public void Configure(EntityTypeBuilder<QuoteRequest> builder)
    {
        builder.ToTable("quote_requests", "quotes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.OfficeBranchId).HasMaxLength(64);
        builder.Property(x => x.CalculationType).HasMaxLength(32).HasDefaultValue("AUTO").IsRequired();
        builder.Property(x => x.CalculationOrigin).HasMaxLength(32).HasDefaultValue("PROPRIO").IsRequired();
        builder.Property(x => x.VersionLabel).HasMaxLength(200);
        builder.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DocumentNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.PostalCode).HasMaxLength(16);
        builder.Property(x => x.CustomerSurname).HasMaxLength(120);
        builder.Property(x => x.CustomerGender).HasMaxLength(1);
        builder.Property(x => x.CustomerMaritalStatusCode).HasMaxLength(4);
        builder.Property(x => x.DriverLicenseNumber).HasMaxLength(32);
        builder.Property(x => x.InsuredDriverRelationshipCode).HasMaxLength(4);
        builder.Property(x => x.VehicleChassisNumber).HasMaxLength(32);
        builder.Property(x => x.VehiclePlate).HasMaxLength(16);
        builder.Property(x => x.VehicleBrand).HasMaxLength(100);
        builder.Property(x => x.VehicleModel).HasMaxLength(100);
        builder.Property(x => x.VehicleFipeCode).HasMaxLength(32);
        builder.Property(x => x.VehicleFuelTypeCode).HasMaxLength(4);
        builder.Property(x => x.VehicleOvernightPostalCode).HasMaxLength(16);
        builder.Property(x => x.PreviousBonus).HasMaxLength(4);
        builder.Property(x => x.ShareToken).HasMaxLength(64);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.CorrelationId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.OfficeBranchId, x.OpportunityId });

        builder.OwnsOne(x => x.AutoSubmission, submission =>
        {
            submission.ToTable("quote_auto_submissions", "quotes");
            submission.WithOwner().HasForeignKey("QuoteRequestId");
            submission.Property<Guid>("QuoteRequestId");
            submission.HasKey("QuoteRequestId");
            submission.Property(x => x.DriverName).HasMaxLength(200).IsRequired();
            submission.Property(x => x.DriverDocument).HasMaxLength(32).IsRequired();
            submission.Property(x => x.DriverGender).HasMaxLength(1).IsRequired();
            submission.Property(x => x.DriverMaritalStatus).HasMaxLength(64).IsRequired();
            submission.Property(x => x.InsuranceType).HasMaxLength(32).IsRequired();
            submission.Property(x => x.PreviousPolicyNumber).HasMaxLength(120);
            submission.Property(x => x.PreviousIdentificationCode).HasMaxLength(120);
            submission.OwnsMany(x => x.Coverages, coverage =>
            {
                coverage.ToTable("quote_requested_coverages", "quotes");
                coverage.WithOwner().HasForeignKey("QuoteRequestId");
                coverage.Property<Guid>("QuoteRequestId");
                coverage.Property(x => x.Code).HasMaxLength(80).ValueGeneratedNever();
                coverage.HasKey("QuoteRequestId", nameof(WAssis.Domain.Modules.Quotes.ValueObjects.RequestedQuoteCoverage.Code));
                coverage.Property(x => x.RequestedDeductibleType).HasMaxLength(80);
                coverage.Property(x => x.RequestedOption).HasMaxLength(80);
            });
        });

        builder.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(x => x.QuoteRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
