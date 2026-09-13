using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Migrations;

[DbContext(typeof(WAssisDbContext))]
[Migration("20260913180000_AddDurableQuoteDispatch")]
public sealed class AddDurableQuoteDispatch : Migration
{
    // Technical SQL-owned tables, deliberately outside the business EF model snapshot.
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE SCHEMA IF NOT EXISTS infrastructure;
        CREATE TABLE infrastructure.work_outbox (
          id uuid PRIMARY KEY,
          tenant_id varchar(64) NOT NULL,
          kind varchar(64) NOT NULL,
          aggregate_id uuid NOT NULL,
          state varchar(16) NOT NULL DEFAULT 'Pending' CHECK (state IN ('Pending','Processing','Completed','NeedsReview')),
          lease_token uuid,
          lease_until timestamptz,
          created_at timestamptz NOT NULL DEFAULT now(),
          updated_at timestamptz NOT NULL DEFAULT now(),
          UNIQUE(tenant_id, kind, aggregate_id)
        );
        CREATE INDEX ix_work_outbox_dispatch ON infrastructure.work_outbox (kind, state, created_at);
        CREATE TABLE infrastructure.work_inbox (
          work_id uuid PRIMARY KEY REFERENCES infrastructure.work_outbox(id),
          tenant_id varchar(64) NOT NULL,
          completed_at timestamptz NOT NULL DEFAULT now()
        );
        INSERT INTO infrastructure.work_outbox (id,tenant_id,kind,aggregate_id,state,created_at)
          SELECT "Id", "TenantId", 'quotes.dispatch', "Id",
            CASE WHEN "Status" = 0 THEN 'Pending' ELSE 'NeedsReview' END, "CreatedAtUtc"
          FROM quotes.quote_requests WHERE "Status" IN (0,1);
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => throw new NotSupportedException(
        "Retain dispatch and receipt history. Roll back the application only after reviewing in-flight work; use a forward migration.");
}
