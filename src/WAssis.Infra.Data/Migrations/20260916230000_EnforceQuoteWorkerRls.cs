using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Migrations;

[DbContext(typeof(WAssisDbContext))]
[Migration("20260916230000_EnforceQuoteWorkerRls")]
public sealed class EnforceQuoteWorkerRls : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        ALTER TABLE infrastructure.work_outbox
          ADD COLUMN IF NOT EXISTS branch_id varchar(64);

        UPDATE infrastructure.work_outbox w
        SET branch_id = q."OfficeBranchId"
        FROM quotes.quote_requests q
        WHERE w.kind = 'quotes.dispatch'
          AND w.aggregate_id = q."Id"
          AND w.tenant_id = q."TenantId"
          AND w.branch_id IS NULL
          AND q."OfficeBranchId" IS NOT NULL;

        UPDATE infrastructure.work_outbox
        SET state = 'NeedsReview', updated_at = now()
        WHERE kind = 'quotes.dispatch' AND state = 'Pending' AND branch_id IS NULL;

        CREATE INDEX IF NOT EXISTS ix_work_outbox_scoped_dispatch
          ON infrastructure.work_outbox (kind, state, branch_id, created_at);

        ALTER TABLE quotes.quote_requests ENABLE ROW LEVEL SECURITY;
        ALTER TABLE quotes.quote_requests FORCE ROW LEVEL SECURITY;
        DROP POLICY IF EXISTS tenant_branch_isolation ON quotes.quote_requests;
        CREATE POLICY tenant_branch_isolation ON quotes.quote_requests
          USING ("TenantId" = security.current_tenant_id()
                 AND "OfficeBranchId" IS NOT NULL
                 AND "OfficeBranchId" = security.current_branch_id())
          WITH CHECK ("TenantId" = security.current_tenant_id()
                      AND "OfficeBranchId" IS NOT NULL
                      AND "OfficeBranchId" = security.current_branch_id());

        ALTER TABLE quotes.quote_options ENABLE ROW LEVEL SECURITY;
        ALTER TABLE quotes.quote_options FORCE ROW LEVEL SECURITY;
        DROP POLICY IF EXISTS tenant_branch_isolation ON quotes.quote_options;
        CREATE POLICY tenant_branch_isolation ON quotes.quote_options
          USING ("TenantId" = security.current_tenant_id() AND EXISTS (
            SELECT 1 FROM quotes.quote_requests q
            WHERE q."Id" = "QuoteRequestId"
              AND q."TenantId" = security.current_tenant_id()
              AND q."OfficeBranchId" = security.current_branch_id()))
          WITH CHECK ("TenantId" = security.current_tenant_id() AND EXISTS (
            SELECT 1 FROM quotes.quote_requests q
            WHERE q."Id" = "QuoteRequestId"
              AND q."TenantId" = security.current_tenant_id()
              AND q."OfficeBranchId" = security.current_branch_id()));

        ALTER TABLE quotes.quote_option_coverages ENABLE ROW LEVEL SECURITY;
        ALTER TABLE quotes.quote_option_coverages FORCE ROW LEVEL SECURITY;
        DROP POLICY IF EXISTS tenant_branch_isolation ON quotes.quote_option_coverages;
        CREATE POLICY tenant_branch_isolation ON quotes.quote_option_coverages
          USING (EXISTS (
            SELECT 1 FROM quotes.quote_options o
            WHERE o."Id" = "QuoteOptionId"))
          WITH CHECK (EXISTS (
            SELECT 1 FROM quotes.quote_options o
            WHERE o."Id" = "QuoteOptionId"));

        ALTER TABLE quotes.quote_option_installments ENABLE ROW LEVEL SECURITY;
        ALTER TABLE quotes.quote_option_installments FORCE ROW LEVEL SECURITY;
        DROP POLICY IF EXISTS tenant_branch_isolation ON quotes.quote_option_installments;
        CREATE POLICY tenant_branch_isolation ON quotes.quote_option_installments
          USING (EXISTS (
            SELECT 1 FROM quotes.quote_options o
            WHERE o."Id" = "QuoteOptionId"))
          WITH CHECK (EXISTS (
            SELECT 1 FROM quotes.quote_options o
            WHERE o."Id" = "QuoteOptionId"));

        ALTER TABLE quotes.quote_option_messages ENABLE ROW LEVEL SECURITY;
        ALTER TABLE quotes.quote_option_messages FORCE ROW LEVEL SECURITY;
        DROP POLICY IF EXISTS tenant_branch_isolation ON quotes.quote_option_messages;
        CREATE POLICY tenant_branch_isolation ON quotes.quote_option_messages
          USING (EXISTS (
            SELECT 1 FROM quotes.quote_options o
            WHERE o."Id" = "QuoteOptionId"))
          WITH CHECK (EXISTS (
            SELECT 1 FROM quotes.quote_options o
            WHERE o."Id" = "QuoteOptionId"));
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP POLICY IF EXISTS tenant_branch_isolation ON quotes.quote_option_messages;
        ALTER TABLE quotes.quote_option_messages DISABLE ROW LEVEL SECURITY;
        DROP POLICY IF EXISTS tenant_branch_isolation ON quotes.quote_option_installments;
        ALTER TABLE quotes.quote_option_installments DISABLE ROW LEVEL SECURITY;
        DROP POLICY IF EXISTS tenant_branch_isolation ON quotes.quote_option_coverages;
        ALTER TABLE quotes.quote_option_coverages DISABLE ROW LEVEL SECURITY;
        DROP POLICY IF EXISTS tenant_branch_isolation ON quotes.quote_options;
        ALTER TABLE quotes.quote_options DISABLE ROW LEVEL SECURITY;
        DROP POLICY IF EXISTS tenant_branch_isolation ON quotes.quote_requests;
        ALTER TABLE quotes.quote_requests DISABLE ROW LEVEL SECURITY;
        DROP INDEX IF EXISTS infrastructure.ix_work_outbox_scoped_dispatch;
        ALTER TABLE infrastructure.work_outbox DROP COLUMN IF EXISTS branch_id;
        """);
}
