using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Migrations;

// Authorized reset of non-production CRM roots. No historical business data is copied.
public sealed partial class UseCanonicalCrmRoots : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP TABLE public.oportunidades;
        DROP TABLE public.segurados;

        ALTER TABLE erp.segurados ADD COLUMN IF NOT EXISTS created_by uuid;
        ALTER TABLE erp.oportunidades ADD COLUMN IF NOT EXISTS created_at timestamptz NOT NULL DEFAULT now();
        ALTER TABLE erp.oportunidades ADD COLUMN IF NOT EXISTS updated_at timestamptz NOT NULL DEFAULT now();
        ALTER TABLE erp.oportunidades ADD CONSTRAINT ck_oportunidades_single_conclusion
          CHECK (ganha_em IS NULL OR perdida_em IS NULL);

        ALTER TABLE erp.segurados ENABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.segurados FORCE ROW LEVEL SECURITY;
        CREATE POLICY tenant_branch_isolation ON erp.segurados
          USING (tenant_id::text = security.current_tenant_id() AND filial_id::text = security.current_branch_id())
          WITH CHECK (tenant_id::text = security.current_tenant_id() AND filial_id::text = security.current_branch_id());
        ALTER TABLE erp.oportunidades ENABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.oportunidades FORCE ROW LEVEL SECURITY;
        CREATE POLICY tenant_branch_isolation ON erp.oportunidades
          USING (tenant_id::text = security.current_tenant_id() AND filial_id::text = security.current_branch_id())
          WITH CHECK (tenant_id::text = security.current_tenant_id() AND filial_id::text = security.current_branch_id());

        CREATE OR REPLACE FUNCTION erp.enforce_crm_opportunity_references()
        RETURNS trigger LANGUAGE plpgsql AS $$
        BEGIN
          IF NEW.segurado_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.segurados s WHERE s.id = NEW.segurado_id
              AND s.tenant_id = NEW.tenant_id AND s.filial_id = NEW.filial_id
          ) THEN RAISE EXCEPTION 'Invalid insured scope' USING ERRCODE = '23514'; END IF;
          IF NOT EXISTS (
            SELECT 1 FROM erp.pipeline_stages s JOIN erp.pipelines p ON p.id = s.pipeline_id
            WHERE s.id = NEW.stage_id AND p.tenant_id = NEW.tenant_id AND p.entidade_tipo = 'oportunidade'
              AND (p.filial_id IS NULL OR p.filial_id = NEW.filial_id)
          ) THEN RAISE EXCEPTION 'Invalid stage scope' USING ERRCODE = '23514'; END IF;
          IF NEW.responsavel_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.profiles p WHERE p.id = NEW.responsavel_id AND p.tenant_id = NEW.tenant_id
          ) THEN RAISE EXCEPTION 'Invalid responsible scope' USING ERRCODE = '23514'; END IF;
          IF NEW.ramo_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.ramos r WHERE r.id = NEW.ramo_id AND r.tenant_id = NEW.tenant_id
          ) THEN RAISE EXCEPTION 'Invalid branch of insurance scope' USING ERRCODE = '23514'; END IF;
          IF NEW.origem_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.origens r WHERE r.id = NEW.origem_id AND r.tenant_id = NEW.tenant_id
          ) THEN RAISE EXCEPTION 'Invalid origin scope' USING ERRCODE = '23514'; END IF;
          IF NEW.motivo_perda_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.motivos_perda r WHERE r.id = NEW.motivo_perda_id AND r.tenant_id = NEW.tenant_id
          ) THEN RAISE EXCEPTION 'Invalid loss reason scope' USING ERRCODE = '23514'; END IF;
          IF NEW.apolice_origem_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.apolices a JOIN erp.segurados s ON s.id = a.segurado_id
            WHERE a.id = NEW.apolice_origem_id AND s.tenant_id = NEW.tenant_id AND s.filial_id = NEW.filial_id
          ) THEN RAISE EXCEPTION 'Invalid source policy scope' USING ERRCODE = '23514'; END IF;
          RETURN NEW;
        END; $$;
        CREATE TRIGGER trg_oportunidades_crm_references
          BEFORE INSERT OR UPDATE ON erp.oportunidades FOR EACH ROW
          EXECUTE FUNCTION erp.enforce_crm_opportunity_references();
        """);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("Non-production root reset cannot restore discarded test data. Recreate a disposable database.");
}
