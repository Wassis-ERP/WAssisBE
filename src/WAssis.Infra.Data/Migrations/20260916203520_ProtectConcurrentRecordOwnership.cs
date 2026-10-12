using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WAssis.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProtectConcurrentRecordOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Snapshot-only change: concurrency predicates use existing ownership columns.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No physical schema or data change to reverse.
        }
    }
}
