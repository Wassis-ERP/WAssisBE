using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WAssis.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20260328221500_OperationalFlowsEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedToDisplayName",
                schema: "whatsapp_support",
                table: "whatsapp_conversations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserId",
                schema: "whatsapp_support",
                table: "whatsapp_conversations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAtUtc",
                schema: "whatsapp_support",
                table: "whatsapp_conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "whatsapp_support",
                table: "whatsapp_conversations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SlaDueAtUtc",
                schema: "whatsapp_support",
                table: "whatsapp_conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedHumanAtUtc",
                schema: "whatsapp_support",
                table: "whatsapp_conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                schema: "policies",
                table: "policy_drafts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByUserId",
                schema: "policies",
                table: "policy_drafts",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastProcessedAtUtc",
                schema: "documents",
                table: "imported_documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ParsingConfidence",
                schema: "documents",
                table: "imported_documents",
                type: "numeric(5,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresHumanReview",
                schema: "documents",
                table: "imported_documents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                schema: "documents",
                table: "imported_documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedByUserId",
                schema: "documents",
                table: "imported_documents",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                schema: "documents",
                table: "imported_documents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchedReference",
                schema: "financial",
                table: "commission_reconciliations",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledAtUtc",
                schema: "financial",
                table: "commission_reconciliations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementNotes",
                schema: "financial",
                table: "commission_reconciliations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedToDisplayName",
                schema: "whatsapp_support",
                table: "whatsapp_conversations");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                schema: "whatsapp_support",
                table: "whatsapp_conversations");

            migrationBuilder.DropColumn(
                name: "ClosedAtUtc",
                schema: "whatsapp_support",
                table: "whatsapp_conversations");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "whatsapp_support",
                table: "whatsapp_conversations");

            migrationBuilder.DropColumn(
                name: "SlaDueAtUtc",
                schema: "whatsapp_support",
                table: "whatsapp_conversations");

            migrationBuilder.DropColumn(
                name: "StartedHumanAtUtc",
                schema: "whatsapp_support",
                table: "whatsapp_conversations");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                schema: "policies",
                table: "policy_drafts");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                schema: "policies",
                table: "policy_drafts");

            migrationBuilder.DropColumn(
                name: "LastProcessedAtUtc",
                schema: "documents",
                table: "imported_documents");

            migrationBuilder.DropColumn(
                name: "ParsingConfidence",
                schema: "documents",
                table: "imported_documents");

            migrationBuilder.DropColumn(
                name: "RequiresHumanReview",
                schema: "documents",
                table: "imported_documents");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                schema: "documents",
                table: "imported_documents");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                schema: "documents",
                table: "imported_documents");

            migrationBuilder.DropColumn(
                name: "StoragePath",
                schema: "documents",
                table: "imported_documents");

            migrationBuilder.DropColumn(
                name: "MatchedReference",
                schema: "financial",
                table: "commission_reconciliations");

            migrationBuilder.DropColumn(
                name: "SettledAtUtc",
                schema: "financial",
                table: "commission_reconciliations");

            migrationBuilder.DropColumn(
                name: "SettlementNotes",
                schema: "financial",
                table: "commission_reconciliations");
        }
    }
}
