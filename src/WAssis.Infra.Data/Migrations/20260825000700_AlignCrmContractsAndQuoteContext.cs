using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WAssis.Infra.Data.Migrations;

/// <inheritdoc />
public partial class AlignCrmContractsAndQuoteContext : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "OfficeBranchId",
            schema: "quotes",
            table: "quote_requests",
            type: "character varying(64)",
            maxLength: 64,
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "OpportunityId",
            schema: "quotes",
            table: "quote_requests",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "InsuranceBranchId",
            schema: "quotes",
            table: "quote_requests",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "InsuredPersonId",
            schema: "quotes",
            table: "quote_requests",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CalculationType",
            schema: "quotes",
            table: "quote_requests",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "AUTO");

        migrationBuilder.AddColumn<string>(
            name: "CalculationOrigin",
            schema: "quotes",
            table: "quote_requests",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "PROPRIO");

        migrationBuilder.AddColumn<string>(
            name: "VersionLabel",
            schema: "quotes",
            table: "quote_requests",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_quote_requests_TenantId_OfficeBranchId_OpportunityId",
            schema: "quotes",
            table: "quote_requests",
            columns: new[] { "TenantId", "OfficeBranchId", "OpportunityId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_quote_requests_TenantId_OfficeBranchId_OpportunityId",
            schema: "quotes",
            table: "quote_requests");

        migrationBuilder.DropColumn(name: "OfficeBranchId", schema: "quotes", table: "quote_requests");
        migrationBuilder.DropColumn(name: "OpportunityId", schema: "quotes", table: "quote_requests");
        migrationBuilder.DropColumn(name: "InsuranceBranchId", schema: "quotes", table: "quote_requests");
        migrationBuilder.DropColumn(name: "InsuredPersonId", schema: "quotes", table: "quote_requests");
        migrationBuilder.DropColumn(name: "CalculationType", schema: "quotes", table: "quote_requests");
        migrationBuilder.DropColumn(name: "CalculationOrigin", schema: "quotes", table: "quote_requests");
        migrationBuilder.DropColumn(name: "VersionLabel", schema: "quotes", table: "quote_requests");
    }
}
