using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Migrations;

[DbContext(typeof(WAssisDbContext))]
partial class WAssisDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasDefaultSchema("public")
            .HasAnnotation("ProductVersion", "8.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("WAssis.Domain.Modules.Quotes.Entities.QuoteOption", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
            b.Property<decimal?>("CommissionAmount").HasColumnType("numeric");
            b.Property<string>("ExternalReference").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<string>("InsuranceCompanyCode").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
            b.Property<string>("InsuranceCompanyName").IsRequired().HasMaxLength(120).HasColumnType("character varying(120)");
            b.Property<decimal?>("PremiumAmount").HasColumnType("numeric");
            b.Property<string>("ProductCode").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
            b.Property<string>("ProductName").IsRequired().HasMaxLength(120).HasColumnType("character varying(120)");
            b.Property<Guid>("QuoteRequestId").HasColumnType("uuid");
            b.Property<int>("Status").HasColumnType("integer");
            b.HasKey("Id");
            b.HasIndex("QuoteRequestId");
            b.ToTable("quote_options", "quotes");
        });

        modelBuilder.Entity("WAssis.Domain.Modules.Quotes.Entities.QuoteRequest", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
            b.Property<string>("CorrelationId").IsRequired().HasMaxLength(64).HasColumnType("character varying(64)");
            b.Property<DateTime>("CreatedAtUtc").HasColumnType("timestamp with time zone");
            b.Property<string>("CustomerName").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)");
            b.Property<string>("DocumentNumber").IsRequired().HasMaxLength(32).HasColumnType("character varying(32)");
            b.Property<string>("Email").HasMaxLength(200).HasColumnType("character varying(200)");
            b.Property<string>("PhoneNumber").HasMaxLength(32).HasColumnType("character varying(32)");
            b.Property<string>("ShareToken").HasMaxLength(64).HasColumnType("character varying(64)");
            b.Property<int>("Status").HasColumnType("integer");
            b.Property<DateTime?>("UpdatedAtUtc").HasColumnType("timestamp with time zone");
            b.Property<string>("VehicleBrand").HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<string>("VehicleModel").HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<int>("VehicleModelYear").HasColumnType("integer");
            b.Property<string>("VehiclePlate").HasMaxLength(16).HasColumnType("character varying(16)");
            b.HasKey("Id");
            b.ToTable("quote_requests", "quotes");
        });

        modelBuilder.Entity("WAssis.Domain.Modules.Quotes.Entities.QuoteOption", b =>
        {
            b.HasOne("WAssis.Domain.Modules.Quotes.Entities.QuoteRequest", null)
                .WithMany("Options")
                .HasForeignKey("QuoteRequestId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("WAssis.Domain.Modules.Quotes.Entities.QuoteOption", b =>
        {
            b.OwnsMany("WAssis.Domain.Modules.Quotes.ValueObjects.CoverageSnapshot", "_coverages", b1 =>
            {
                b1.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
                b1.Property<Guid>("QuoteOptionId").HasColumnType("uuid");
                b1.Property<string>("Code").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
                b1.Property<decimal?>("DeductibleAmount").HasColumnType("numeric");
                b1.Property<decimal?>("InsuredAmount").HasColumnType("numeric");
                b1.Property<string>("Name").IsRequired().HasMaxLength(120).HasColumnType("character varying(120)");
                b1.HasKey("Id");
                b1.HasIndex("QuoteOptionId");
                b1.ToTable("quote_option_coverages", "quotes");
                b1.WithOwner().HasForeignKey("QuoteOptionId");
            });

            b.OwnsMany("WAssis.Domain.Modules.Quotes.ValueObjects.InstallmentSnapshot", "_installments", b1 =>
            {
                b1.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
                b1.Property<Guid>("QuoteOptionId").HasColumnType("uuid");
                b1.Property<decimal>("Amount").HasColumnType("numeric(18,2)");
                b1.Property<int>("Number").HasColumnType("integer");
                b1.Property<decimal?>("TotalAmount").HasColumnType("numeric(18,2)");
                b1.HasKey("Id");
                b1.HasIndex("QuoteOptionId");
                b1.ToTable("quote_option_installments", "quotes");
                b1.WithOwner().HasForeignKey("QuoteOptionId");
            });

            b.OwnsMany("WAssis.Domain.Modules.Quotes.ValueObjects.QuoteStatusMessage", "_messages", b1 =>
            {
                b1.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
                b1.Property<Guid>("QuoteOptionId").HasColumnType("uuid");
                b1.Property<string>("Code").IsRequired().HasMaxLength(50).HasColumnType("character varying(50)");
                b1.Property<string>("Description").IsRequired().HasMaxLength(500).HasColumnType("character varying(500)");
                b1.HasKey("Id");
                b1.HasIndex("QuoteOptionId");
                b1.ToTable("quote_option_messages", "quotes");
                b1.WithOwner().HasForeignKey("QuoteOptionId");
            });

            b.Navigation("_coverages");
            b.Navigation("_installments");
            b.Navigation("_messages");
        });
#pragma warning restore 612, 618
    }
}
