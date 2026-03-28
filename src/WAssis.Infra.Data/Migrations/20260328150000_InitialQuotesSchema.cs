using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WAssis.Infra.Data.Migrations;

public partial class InitialQuotesSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "quotes");

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
                VehiclePlate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                VehicleBrand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                VehicleModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                VehicleModelYear = table.Column<int>(type: "integer", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                ShareToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_quote_requests", x => x.Id);
            });

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
        migrationBuilder.DropTable(name: "quote_option_coverages", schema: "quotes");
        migrationBuilder.DropTable(name: "quote_option_installments", schema: "quotes");
        migrationBuilder.DropTable(name: "quote_option_messages", schema: "quotes");
        migrationBuilder.DropTable(name: "quote_options", schema: "quotes");
        migrationBuilder.DropTable(name: "quote_requests", schema: "quotes");
    }
}
