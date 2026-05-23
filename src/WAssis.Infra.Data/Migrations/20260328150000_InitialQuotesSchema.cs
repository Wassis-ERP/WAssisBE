using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Migrations;

[DbContext(typeof(WAssisDbContext))]
[Migration("20260328150000_InitialQuotesSchema")]
public partial class InitialQuotesSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "documents");
        migrationBuilder.EnsureSchema(name: "financial");
        migrationBuilder.EnsureSchema(name: "operations");
        migrationBuilder.EnsureSchema(name: "policies");
        migrationBuilder.EnsureSchema(name: "quotes");
        migrationBuilder.EnsureSchema(name: "whatsapp_support");

        migrationBuilder.CreateTable(
            name: "document_searches",
            schema: "documents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                InsuranceCompanyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                SearchType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_document_searches", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_document_searches_CorrelationId",
            schema: "documents",
            table: "document_searches",
            column: "CorrelationId",
            unique: true);

        migrationBuilder.CreateTable(
            name: "imported_documents",
            schema: "documents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                ExtractedText = table.Column<string>(type: "text", nullable: true),
                InsuranceCompanyName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                ProposalNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                InsuredName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                CoverageStartDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CoverageEndDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                TotalPremiumAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                CommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                ParsingNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_imported_documents", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "commission_receipts",
            schema: "financial",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                InsuranceCompanyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                ReceivedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                SourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                ImportedDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_commission_receipts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "commission_reconciliations",
            schema: "financial",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CommissionReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                ExpectedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                ReceivedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                DifferenceAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_commission_reconciliations", x => x.Id);
                table.ForeignKey(
                    name: "FK_commission_reconciliations_commission_receipts_CommissionReceiptId",
                    column: x => x.CommissionReceiptId,
                    principalSchema: "financial",
                    principalTable: "commission_receipts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_commission_reconciliations_CommissionReceiptId",
            schema: "financial",
            table: "commission_reconciliations",
            column: "CommissionReceiptId");

        migrationBuilder.CreateTable(
            name: "policy_drafts",
            schema: "policies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ImportedDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                InsuranceCompanyName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                ProposalNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                InsuredName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                CoverageStartDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CoverageEndDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                TotalPremiumAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                CommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                PolicyNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ReadyForIssuanceAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IssuedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_policy_drafts", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_policy_drafts_ImportedDocumentId",
            schema: "policies",
            table: "policy_drafts",
            column: "ImportedDocumentId");

        migrationBuilder.CreateIndex(
            name: "IX_policy_drafts_PolicyNumber",
            schema: "policies",
            table: "policy_drafts",
            column: "PolicyNumber",
            unique: true);

        migrationBuilder.CreateTable(
            name: "audit_entries",
            schema: "operations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Module = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                EntityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_audit_entries", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "whatsapp_conversations",
            schema: "whatsapp_support",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                CustomerIdentifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                LastMessagePreview = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_whatsapp_conversations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "quote_requests",
            schema: "quotes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                DocumentNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                PostalCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                CustomerSurname = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                CustomerGender = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                CustomerBirthDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                VehiclePlate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                VehicleBrand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                VehicleModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                VehicleFipeCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                VehicleModelYear = table.Column<int>(type: "integer", nullable: false),
                HasDriverUnder24 = table.Column<bool>(type: "boolean", nullable: false),
                IsCurrentlyInsured = table.Column<bool>(type: "boolean", nullable: false),
                PreviousBonus = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                BrokerCommissionPercentage = table.Column<int>(type: "integer", nullable: true),
                RenewalInsurerCode = table.Column<int>(type: "integer", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                ShareToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_quote_requests", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_quote_requests_CorrelationId",
            schema: "quotes",
            table: "quote_requests",
            column: "CorrelationId",
            unique: true);

        migrationBuilder.CreateTable(
            name: "quote_options",
            schema: "quotes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                QuoteRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                InsuranceCompanyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                InsuranceCompanyName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                ProductCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                ProductName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                PremiumAmount = table.Column<decimal>(type: "numeric", nullable: true),
                CommissionAmount = table.Column<decimal>(type: "numeric", nullable: true),
                ExternalReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_quote_options", x => x.Id);
                table.ForeignKey(
                    name: "FK_quote_options_quote_requests_QuoteRequestId",
                    column: x => x.QuoteRequestId,
                    principalSchema: "quotes",
                    principalTable: "quote_requests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "quote_option_coverages",
            schema: "quotes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                QuoteOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                InsuredAmount = table.Column<decimal>(type: "numeric", nullable: true),
                DeductibleAmount = table.Column<decimal>(type: "numeric", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_quote_option_coverages", x => x.Id);
                table.ForeignKey(
                    name: "FK_quote_option_coverages_quote_options_QuoteOptionId",
                    column: x => x.QuoteOptionId,
                    principalSchema: "quotes",
                    principalTable: "quote_options",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "quote_option_installments",
            schema: "quotes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                QuoteOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                Number = table.Column<int>(type: "integer", nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_quote_option_installments", x => x.Id);
                table.ForeignKey(
                    name: "FK_quote_option_installments_quote_options_QuoteOptionId",
                    column: x => x.QuoteOptionId,
                    principalSchema: "quotes",
                    principalTable: "quote_options",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "quote_option_messages",
            schema: "quotes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                QuoteOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_quote_option_messages", x => x.Id);
                table.ForeignKey(
                    name: "FK_quote_option_messages_quote_options_QuoteOptionId",
                    column: x => x.QuoteOptionId,
                    principalSchema: "quotes",
                    principalTable: "quote_options",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_quote_option_coverages_QuoteOptionId",
            schema: "quotes",
            table: "quote_option_coverages",
            column: "QuoteOptionId");

        migrationBuilder.CreateIndex(
            name: "IX_quote_option_installments_QuoteOptionId",
            schema: "quotes",
            table: "quote_option_installments",
            column: "QuoteOptionId");

        migrationBuilder.CreateIndex(
            name: "IX_quote_option_messages_QuoteOptionId",
            schema: "quotes",
            table: "quote_option_messages",
            column: "QuoteOptionId");

        migrationBuilder.CreateIndex(
            name: "IX_quote_options_QuoteRequestId",
            schema: "quotes",
            table: "quote_options",
            column: "QuoteRequestId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "document_searches", schema: "documents");
        migrationBuilder.DropTable(name: "imported_documents", schema: "documents");
        migrationBuilder.DropTable(name: "commission_reconciliations", schema: "financial");
        migrationBuilder.DropTable(name: "commission_receipts", schema: "financial");
        migrationBuilder.DropTable(name: "audit_entries", schema: "operations");
        migrationBuilder.DropTable(name: "policy_drafts", schema: "policies");
        migrationBuilder.DropTable(name: "quote_option_coverages", schema: "quotes");
        migrationBuilder.DropTable(name: "quote_option_installments", schema: "quotes");
        migrationBuilder.DropTable(name: "quote_option_messages", schema: "quotes");
        migrationBuilder.DropTable(name: "quote_options", schema: "quotes");
        migrationBuilder.DropTable(name: "quote_requests", schema: "quotes");
        migrationBuilder.DropTable(name: "whatsapp_conversations", schema: "whatsapp_support");
    }
}
