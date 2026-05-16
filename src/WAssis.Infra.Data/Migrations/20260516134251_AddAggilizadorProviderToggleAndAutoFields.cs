using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WAssis.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAggilizadorProviderToggleAndAutoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerMaritalStatusCode",
                schema: "quotes",
                table: "quote_requests",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverLicenseNumber",
                schema: "quotes",
                table: "quote_requests",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DriverLicenseYears",
                schema: "quotes",
                table: "quote_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuredDriverRelationshipCode",
                schema: "quotes",
                table: "quote_requests",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleChassisNumber",
                schema: "quotes",
                table: "quote_requests",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleFuelTypeCode",
                schema: "quotes",
                table: "quote_requests",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VehicleHasAntiTheft",
                schema: "quotes",
                table: "quote_requests",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VehicleHasKitGas",
                schema: "quotes",
                table: "quote_requests",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VehicleHasTracker",
                schema: "quotes",
                table: "quote_requests",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VehicleIsArmored",
                schema: "quotes",
                table: "quote_requests",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VehicleIsFinanced",
                schema: "quotes",
                table: "quote_requests",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VehicleIsZeroKm",
                schema: "quotes",
                table: "quote_requests",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleManufactureYear",
                schema: "quotes",
                table: "quote_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleOvernightPostalCode",
                schema: "quotes",
                table: "quote_requests",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "quote_provider_activation_settings",
                schema: "quotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quote_provider_activation_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quote_provider_activation_settings_ProviderCode",
                schema: "quotes",
                table: "quote_provider_activation_settings",
                column: "ProviderCode",
                unique: true);

            migrationBuilder.InsertData(
                schema: "quotes",
                table: "quote_provider_activation_settings",
                columns: ["Id", "ProviderCode", "IsEnabled", "UpdatedAtUtc"],
                values: [new Guid("9b39a312-4b97-4b62-91ce-4ee7050c0f4a"), "aggilizador_auto", false, new DateTime(2026, 5, 16, 0, 0, 0, DateTimeKind.Utc)]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quote_provider_activation_settings",
                schema: "quotes");

            migrationBuilder.DropColumn(
                name: "CustomerMaritalStatusCode",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "DriverLicenseNumber",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "DriverLicenseYears",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "InsuredDriverRelationshipCode",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "VehicleChassisNumber",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "VehicleFuelTypeCode",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "VehicleHasAntiTheft",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "VehicleHasKitGas",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "VehicleHasTracker",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "VehicleIsArmored",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "VehicleIsFinanced",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "VehicleIsZeroKm",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "VehicleManufactureYear",
                schema: "quotes",
                table: "quote_requests");

            migrationBuilder.DropColumn(
                name: "VehicleOvernightPostalCode",
                schema: "quotes",
                table: "quote_requests");
        }
    }
}
