using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WAssis.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIsolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_quote_requests_CorrelationId",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropIndex(
                name: "IX_policy_drafts_PolicyNumber",
                schema: "policies",
                table: "policy_drafts");

            migrationBuilder.DropIndex(
                name: "IX_document_searches_CorrelationId",
                schema: "documents",
                table: "document_searches");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "whatsapp_support",
                table: "whatsapp_conversations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "quotes",
                table: "quote_requests",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "quotes",
                table: "quote_options",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "policies",
                table: "policy_drafts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "documents",
                table: "imported_documents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "documents",
                table: "document_searches",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "financial",
                table: "commission_reconciliations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "financial",
                table: "commission_receipts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                schema: "operations",
                table: "audit_entries",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_quote_requests_TenantId_CorrelationId",
                schema: "quotes",
                table: "quote_requests",
                columns: new[] { "TenantId", "CorrelationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policy_drafts_TenantId_PolicyNumber",
                schema: "policies",
                table: "policy_drafts",
                columns: new[] { "TenantId", "PolicyNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_searches_TenantId_CorrelationId",
                schema: "documents",
                table: "document_searches",
                columns: new[] { "TenantId", "CorrelationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_quote_requests_TenantId_CorrelationId",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropIndex(
                name: "IX_policy_drafts_TenantId_PolicyNumber",
                schema: "policies",
                table: "policy_drafts");

            migrationBuilder.DropIndex(
                name: "IX_document_searches_TenantId_CorrelationId",
                schema: "documents",
                table: "document_searches");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "whatsapp_support",
                table: "whatsapp_conversations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "quotes",
                table: "quote_options");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "policies",
                table: "policy_drafts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "documents",
                table: "imported_documents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "documents",
                table: "document_searches");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "financial",
                table: "commission_reconciliations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "financial",
                table: "commission_receipts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "operations",
                table: "audit_entries");

            migrationBuilder.CreateIndex(
                name: "IX_quote_requests_CorrelationId",
                schema: "quotes",
                table: "quote_requests",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policy_drafts_PolicyNumber",
                schema: "policies",
                table: "policy_drafts",
                column: "PolicyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_searches_CorrelationId",
                schema: "documents",
                table: "document_searches",
                column: "CorrelationId",
                unique: true);
        }
    }
}
