START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE whatsapp_support.whatsapp_conversations ADD office_branch_id character varying(64);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE policies.policy_drafts ADD office_branch_id character varying(64);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE documents.imported_documents ADD office_branch_id character varying(64);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE documents.document_searches ADD office_branch_id character varying(64);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE financial.commission_reconciliations ADD office_branch_id character varying(64);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE financial.commission_receipts ADD office_branch_id character varying(64);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE operations.audit_entries ADD office_branch_id character varying(64);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    CREATE INDEX "IX_whatsapp_conversations_TenantId_office_branch_id" ON whatsapp_support.whatsapp_conversations ("TenantId", office_branch_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    CREATE INDEX "IX_policy_drafts_TenantId_office_branch_id" ON policies.policy_drafts ("TenantId", office_branch_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    CREATE INDEX "IX_imported_documents_TenantId_office_branch_id" ON documents.imported_documents ("TenantId", office_branch_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    CREATE INDEX "IX_document_searches_TenantId_office_branch_id" ON documents.document_searches ("TenantId", office_branch_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    CREATE INDEX "IX_commission_reconciliations_TenantId_office_branch_id" ON financial.commission_reconciliations ("TenantId", office_branch_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    CREATE INDEX "IX_commission_receipts_TenantId_office_branch_id" ON financial.commission_receipts ("TenantId", office_branch_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    CREATE INDEX "IX_audit_entries_TenantId_office_branch_id" ON operations.audit_entries ("TenantId", office_branch_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    CREATE SCHEMA IF NOT EXISTS security;

    CREATE OR REPLACE FUNCTION security.current_tenant_id()
    RETURNS text LANGUAGE sql STABLE PARALLEL SAFE AS $$
      SELECT NULLIF(current_setting('app.tenant_id', true), '')
    $$;

    CREATE OR REPLACE FUNCTION security.current_branch_id()
    RETURNS text LANGUAGE sql STABLE PARALLEL SAFE AS $$
      SELECT NULLIF(current_setting('app.branch_id', true), '')
    $$;

    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE documents.document_searches ENABLE ROW LEVEL SECURITY;
    ALTER TABLE documents.document_searches FORCE ROW LEVEL SECURITY;
    CREATE POLICY tenant_branch_isolation ON documents.document_searches
      USING ("TenantId" = security.current_tenant_id()
             AND "office_branch_id" IS NOT NULL
             AND "office_branch_id" = security.current_branch_id())
      WITH CHECK ("TenantId" = security.current_tenant_id()
                  AND "office_branch_id" IS NOT NULL
                  AND "office_branch_id" = security.current_branch_id());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE documents.imported_documents ENABLE ROW LEVEL SECURITY;
    ALTER TABLE documents.imported_documents FORCE ROW LEVEL SECURITY;
    CREATE POLICY tenant_branch_isolation ON documents.imported_documents
      USING ("TenantId" = security.current_tenant_id()
             AND "office_branch_id" IS NOT NULL
             AND "office_branch_id" = security.current_branch_id())
      WITH CHECK ("TenantId" = security.current_tenant_id()
                  AND "office_branch_id" IS NOT NULL
                  AND "office_branch_id" = security.current_branch_id());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE financial.commission_receipts ENABLE ROW LEVEL SECURITY;
    ALTER TABLE financial.commission_receipts FORCE ROW LEVEL SECURITY;
    CREATE POLICY tenant_branch_isolation ON financial.commission_receipts
      USING ("TenantId" = security.current_tenant_id()
             AND "office_branch_id" IS NOT NULL
             AND "office_branch_id" = security.current_branch_id())
      WITH CHECK ("TenantId" = security.current_tenant_id()
                  AND "office_branch_id" IS NOT NULL
                  AND "office_branch_id" = security.current_branch_id());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE financial.commission_reconciliations ENABLE ROW LEVEL SECURITY;
    ALTER TABLE financial.commission_reconciliations FORCE ROW LEVEL SECURITY;
    CREATE POLICY tenant_branch_isolation ON financial.commission_reconciliations
      USING ("TenantId" = security.current_tenant_id()
             AND "office_branch_id" IS NOT NULL
             AND "office_branch_id" = security.current_branch_id())
      WITH CHECK ("TenantId" = security.current_tenant_id()
                  AND "office_branch_id" IS NOT NULL
                  AND "office_branch_id" = security.current_branch_id());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE policies.policy_drafts ENABLE ROW LEVEL SECURITY;
    ALTER TABLE policies.policy_drafts FORCE ROW LEVEL SECURITY;
    CREATE POLICY tenant_branch_isolation ON policies.policy_drafts
      USING ("TenantId" = security.current_tenant_id()
             AND "office_branch_id" IS NOT NULL
             AND "office_branch_id" = security.current_branch_id())
      WITH CHECK ("TenantId" = security.current_tenant_id()
                  AND "office_branch_id" IS NOT NULL
                  AND "office_branch_id" = security.current_branch_id());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE public.segurados ENABLE ROW LEVEL SECURITY;
    ALTER TABLE public.segurados FORCE ROW LEVEL SECURITY;
    CREATE POLICY tenant_branch_isolation ON public.segurados
      USING ("tenant_id" = security.current_tenant_id()
             AND "filial_id" IS NOT NULL
             AND "filial_id" = security.current_branch_id())
      WITH CHECK ("tenant_id" = security.current_tenant_id()
                  AND "filial_id" IS NOT NULL
                  AND "filial_id" = security.current_branch_id());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE public.oportunidades ENABLE ROW LEVEL SECURITY;
    ALTER TABLE public.oportunidades FORCE ROW LEVEL SECURITY;
    CREATE POLICY tenant_branch_isolation ON public.oportunidades
      USING ("tenant_id" = security.current_tenant_id()
             AND "filial_id" IS NOT NULL
             AND "filial_id" = security.current_branch_id())
      WITH CHECK ("tenant_id" = security.current_tenant_id()
                  AND "filial_id" IS NOT NULL
                  AND "filial_id" = security.current_branch_id());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE whatsapp_support.whatsapp_conversations ENABLE ROW LEVEL SECURITY;
    ALTER TABLE whatsapp_support.whatsapp_conversations FORCE ROW LEVEL SECURITY;
    CREATE POLICY tenant_branch_isolation ON whatsapp_support.whatsapp_conversations
      USING ("TenantId" = security.current_tenant_id()
             AND "office_branch_id" IS NOT NULL
             AND "office_branch_id" = security.current_branch_id())
      WITH CHECK ("TenantId" = security.current_tenant_id()
                  AND "office_branch_id" IS NOT NULL
                  AND "office_branch_id" = security.current_branch_id());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE operations.audit_entries ENABLE ROW LEVEL SECURITY;
    ALTER TABLE operations.audit_entries FORCE ROW LEVEL SECURITY;
    CREATE POLICY tenant_branch_isolation ON operations.audit_entries
      USING ("TenantId" = security.current_tenant_id()
             AND "office_branch_id" IS NOT NULL
             AND "office_branch_id" = security.current_branch_id())
      WITH CHECK ("TenantId" = security.current_tenant_id()
                  AND "office_branch_id" IS NOT NULL
                  AND "office_branch_id" = security.current_branch_id());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE billing.billing_subscriptions ENABLE ROW LEVEL SECURITY;
    ALTER TABLE billing.billing_subscriptions FORCE ROW LEVEL SECURITY;
    CREATE POLICY tenant_isolation ON billing.billing_subscriptions
      USING ("TenantId" = security.current_tenant_id())
      WITH CHECK ("TenantId" = security.current_tenant_id());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    ALTER TABLE billing.billing_invoices ENABLE ROW LEVEL SECURITY;
    ALTER TABLE billing.billing_invoices FORCE ROW LEVEL SECURITY;
    CREATE POLICY tenant_isolation ON billing.billing_invoices
      USING ("TenantId" = security.current_tenant_id())
      WITH CHECK ("TenantId" = security.current_tenant_id());
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM quotes."__EFMigrationsHistory" WHERE "MigrationId" = '20260916151855_SecurityRemediation20260916') THEN
    INSERT INTO quotes."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260916151855_SecurityRemediation20260916', '10.0.12');
    END IF;
END $EF$;
COMMIT;

