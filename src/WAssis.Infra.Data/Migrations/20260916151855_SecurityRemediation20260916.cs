using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WAssis.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class SecurityRemediation20260916 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE SCHEMA IF NOT EXISTS security;
                CREATE TABLE security.rate_limit_buckets (
                  bucket_key text PRIMARY KEY,
                  window_started_at timestamptz NOT NULL,
                  request_count integer NOT NULL CHECK (request_count > 0),
                  expires_at timestamptz NOT NULL
                );
                CREATE INDEX ix_rate_limit_buckets_expires_at
                  ON security.rate_limit_buckets (expires_at);
                CREATE TABLE security.auth_sessions (
                  jti text PRIMARY KEY,
                  tenant_id text NOT NULL,
                  user_id text NOT NULL,
                  expires_at timestamptz NOT NULL,
                  created_at timestamptz NOT NULL,
                  revoked_at timestamptz
                );
                CREATE INDEX ix_auth_sessions_active
                  ON security.auth_sessions (tenant_id, user_id, expires_at)
                  WHERE revoked_at IS NULL;
                """);
            migrationBuilder.AddColumn<string>(
                name: "office_branch_id",
                schema: "whatsapp_support",
                table: "whatsapp_conversations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "office_branch_id",
                schema: "policies",
                table: "policy_drafts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "office_branch_id",
                schema: "documents",
                table: "imported_documents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "office_branch_id",
                schema: "documents",
                table: "document_searches",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "office_branch_id",
                schema: "financial",
                table: "commission_reconciliations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "office_branch_id",
                schema: "financial",
                table: "commission_receipts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "office_branch_id",
                schema: "operations",
                table: "audit_entries",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_conversations_TenantId_office_branch_id",
                schema: "whatsapp_support",
                table: "whatsapp_conversations",
                columns: new[] { "TenantId", "office_branch_id" });

            migrationBuilder.CreateIndex(
                name: "IX_policy_drafts_TenantId_office_branch_id",
                schema: "policies",
                table: "policy_drafts",
                columns: new[] { "TenantId", "office_branch_id" });

            migrationBuilder.CreateIndex(
                name: "IX_imported_documents_TenantId_office_branch_id",
                schema: "documents",
                table: "imported_documents",
                columns: new[] { "TenantId", "office_branch_id" });

            migrationBuilder.CreateIndex(
                name: "IX_document_searches_TenantId_office_branch_id",
                schema: "documents",
                table: "document_searches",
                columns: new[] { "TenantId", "office_branch_id" });

            migrationBuilder.CreateIndex(
                name: "IX_commission_reconciliations_TenantId_office_branch_id",
                schema: "financial",
                table: "commission_reconciliations",
                columns: new[] { "TenantId", "office_branch_id" });

            migrationBuilder.CreateIndex(
                name: "IX_commission_receipts_TenantId_office_branch_id",
                schema: "financial",
                table: "commission_receipts",
                columns: new[] { "TenantId", "office_branch_id" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_TenantId_office_branch_id",
                schema: "operations",
                table: "audit_entries",
                columns: new[] { "TenantId", "office_branch_id" });

            migrationBuilder.Sql("""
                WITH modules(modulo) AS (
                  VALUES ('plataforma'), ('configuracoes'), ('segurados'),
                         ('comercial'), ('documentos'), ('financeiro'),
                         ('apolices'), ('whatsapp'), ('auditoria'),
                         ('faturamento'), ('compatibilidade')
                )
                INSERT INTO erp.role_permissions
                  (id, perfil_id, modulo, escopo, can_read, can_create, can_update,
                   can_delete, can_export, can_manage)
                SELECT gen_random_uuid(), p.id, m.modulo,
                       CASE WHEN p.sistema AND lower(p.nome) = 'master' THEN 'GRUPO' ELSE 'CORRETORA' END,
                       p.sistema AND lower(p.nome) = 'master',
                       p.sistema AND lower(p.nome) = 'master',
                       p.sistema AND lower(p.nome) = 'master',
                       p.sistema AND lower(p.nome) = 'master',
                       p.sistema AND lower(p.nome) = 'master',
                       p.sistema AND lower(p.nome) = 'master'
                FROM erp.perfis p CROSS JOIN modules m
                WHERE NOT EXISTS (
                  SELECT 1 FROM erp.role_permissions rp
                  WHERE rp.perfil_id = p.id AND lower(rp.modulo) = m.modulo
                );
                """);

            migrationBuilder.Sql("""
                CREATE SCHEMA IF NOT EXISTS security;

                CREATE OR REPLACE FUNCTION security.current_tenant_id()
                RETURNS text LANGUAGE sql STABLE PARALLEL SAFE AS $$
                  SELECT NULLIF(current_setting('app.tenant_id', true), '')
                $$;

                CREATE OR REPLACE FUNCTION security.current_branch_id()
                RETURNS text LANGUAGE sql STABLE PARALLEL SAFE AS $$
                  SELECT NULLIF(current_setting('app.branch_id', true), '')
                $$;

                """);

            var branchTables = new[]
            {
                ("documents", "document_searches", "TenantId", "office_branch_id"),
                ("documents", "imported_documents", "TenantId", "office_branch_id"),
                ("financial", "commission_receipts", "TenantId", "office_branch_id"),
                ("financial", "commission_reconciliations", "TenantId", "office_branch_id"),
                ("policies", "policy_drafts", "TenantId", "office_branch_id"),
                ("public", "segurados", "tenant_id", "filial_id"),
                ("public", "oportunidades", "tenant_id", "filial_id"),
                ("whatsapp_support", "whatsapp_conversations", "TenantId", "office_branch_id"),
                ("operations", "audit_entries", "TenantId", "office_branch_id"),
            };
            foreach (var (schema, table, tenantColumn, branchColumn) in branchTables)
            {
                migrationBuilder.Sql($$"""
                    ALTER TABLE {{schema}}.{{table}} ENABLE ROW LEVEL SECURITY;
                    ALTER TABLE {{schema}}.{{table}} FORCE ROW LEVEL SECURITY;
                    CREATE POLICY tenant_branch_isolation ON {{schema}}.{{table}}
                      USING ("{{tenantColumn}}" = security.current_tenant_id()
                             AND "{{branchColumn}}" IS NOT NULL
                             AND "{{branchColumn}}" = security.current_branch_id())
                      WITH CHECK ("{{tenantColumn}}" = security.current_tenant_id()
                                  AND "{{branchColumn}}" IS NOT NULL
                                  AND "{{branchColumn}}" = security.current_branch_id());
                    """);
            }

            var tenantTables = new[]
            {
                ("billing", "billing_subscriptions"),
                ("billing", "billing_invoices"),
            };
            foreach (var (schema, table) in tenantTables)
            {
                migrationBuilder.Sql($$"""
                    ALTER TABLE {{schema}}.{{table}} ENABLE ROW LEVEL SECURITY;
                    ALTER TABLE {{schema}}.{{table}} FORCE ROW LEVEL SECURITY;
                    CREATE POLICY tenant_isolation ON {{schema}}.{{table}}
                      USING ("TenantId" = security.current_tenant_id())
                      WITH CHECK ("TenantId" = security.current_tenant_id());
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS security.auth_sessions; DROP TABLE IF EXISTS security.rate_limit_buckets;");
            migrationBuilder.Sql("""
                DROP POLICY IF EXISTS tenant_isolation ON billing.billing_invoices;
                ALTER TABLE billing.billing_invoices DISABLE ROW LEVEL SECURITY;
                DROP POLICY IF EXISTS tenant_isolation ON billing.billing_subscriptions;
                ALTER TABLE billing.billing_subscriptions DISABLE ROW LEVEL SECURITY;
                """);
            foreach (var (schema, table) in new[]
            {
                ("documents", "document_searches"), ("documents", "imported_documents"),
                ("financial", "commission_receipts"), ("financial", "commission_reconciliations"),
                ("policies", "policy_drafts"), ("public", "segurados"),
                ("public", "oportunidades"), ("whatsapp_support", "whatsapp_conversations"),
                ("operations", "audit_entries"),
            })
            {
                migrationBuilder.Sql($$"""
                    DROP POLICY IF EXISTS tenant_branch_isolation ON {{schema}}.{{table}};
                    ALTER TABLE {{schema}}.{{table}} DISABLE ROW LEVEL SECURITY;
                    """);
            }
            migrationBuilder.DropIndex(
                name: "IX_whatsapp_conversations_TenantId_office_branch_id",
                schema: "whatsapp_support",
                table: "whatsapp_conversations");

            migrationBuilder.DropIndex(
                name: "IX_policy_drafts_TenantId_office_branch_id",
                schema: "policies",
                table: "policy_drafts");

            migrationBuilder.DropIndex(
                name: "IX_imported_documents_TenantId_office_branch_id",
                schema: "documents",
                table: "imported_documents");

            migrationBuilder.DropIndex(
                name: "IX_document_searches_TenantId_office_branch_id",
                schema: "documents",
                table: "document_searches");

            migrationBuilder.DropIndex(
                name: "IX_commission_reconciliations_TenantId_office_branch_id",
                schema: "financial",
                table: "commission_reconciliations");

            migrationBuilder.DropIndex(
                name: "IX_commission_receipts_TenantId_office_branch_id",
                schema: "financial",
                table: "commission_receipts");

            migrationBuilder.DropIndex(
                name: "IX_audit_entries_TenantId_office_branch_id",
                schema: "operations",
                table: "audit_entries");

            migrationBuilder.DropColumn(
                name: "office_branch_id",
                schema: "whatsapp_support",
                table: "whatsapp_conversations");

            migrationBuilder.DropColumn(
                name: "office_branch_id",
                schema: "policies",
                table: "policy_drafts");

            migrationBuilder.DropColumn(
                name: "office_branch_id",
                schema: "documents",
                table: "imported_documents");

            migrationBuilder.DropColumn(
                name: "office_branch_id",
                schema: "documents",
                table: "document_searches");

            migrationBuilder.DropColumn(
                name: "office_branch_id",
                schema: "financial",
                table: "commission_reconciliations");

            migrationBuilder.DropColumn(
                name: "office_branch_id",
                schema: "financial",
                table: "commission_receipts");

            migrationBuilder.DropColumn(
                name: "office_branch_id",
                schema: "operations",
                table: "audit_entries");
        }
    }
}
