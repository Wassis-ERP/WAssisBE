-- Execute as an authorized PostgreSQL administrator after reviewing ownership.
-- No LOGIN/password is created here; bind environment-specific login roles
-- explicitly with GRANT wassis_app/worker/migrator TO <login_role>.

DO $roles$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'wassis_app') THEN
    CREATE ROLE wassis_app NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'wassis_worker') THEN
    CREATE ROLE wassis_worker NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'wassis_migrator') THEN
    CREATE ROLE wassis_migrator NOLOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
  END IF;
END
$roles$;

ALTER ROLE wassis_app NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
ALTER ROLE wassis_worker NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
ALTER ROLE wassis_migrator NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;

GRANT USAGE ON SCHEMA public, documents, financial, policies, quotes,
  whatsapp_support, operations, billing, erp, security TO wassis_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA documents,
  financial, policies, quotes, public, whatsapp_support, operations, billing TO wassis_app;
GRANT SELECT ON erp.profiles, erp.profile_filiais, erp.filiais, erp.perfis,
  erp.role_permissions, erp.produtores TO wassis_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON security.auth_sessions,
  security.rate_limit_buckets TO wassis_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA documents, financial, policies,
  quotes, public, whatsapp_support, operations, billing TO wassis_app;

GRANT USAGE ON SCHEMA infrastructure, security TO wassis_worker;
GRANT SELECT, INSERT, UPDATE ON infrastructure.work_outbox, infrastructure.work_inbox TO wassis_worker;
GRANT EXECUTE ON FUNCTION security.current_tenant_id() TO wassis_app, wassis_worker;
GRANT EXECUTE ON FUNCTION security.current_branch_id() TO wassis_app, wassis_worker;

-- The migration role owns schema changes only. Do not grant it to API/worker logins.
GRANT USAGE, CREATE ON SCHEMA public, documents, financial, policies, quotes,
  whatsapp_support, operations, billing, erp, infrastructure, security TO wassis_migrator;

ALTER DEFAULT PRIVILEGES FOR ROLE wassis_migrator IN SCHEMA documents, financial,
  policies, quotes, public, whatsapp_support, operations, billing
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO wassis_app;
ALTER DEFAULT PRIVILEGES FOR ROLE wassis_migrator IN SCHEMA documents, financial,
  policies, quotes, public, whatsapp_support, operations, billing
  GRANT USAGE, SELECT ON SEQUENCES TO wassis_app;
