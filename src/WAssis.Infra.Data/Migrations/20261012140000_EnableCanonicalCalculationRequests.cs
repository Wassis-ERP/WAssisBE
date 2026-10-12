using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Migrations;

[DbContext(typeof(WAssisDbContext))]
[Migration("20261012140000_EnableCanonicalCalculationRequests")]
public sealed class EnableCanonicalCalculationRequests : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE infrastructure.calculation_receipts (
          tenant_id uuid NOT NULL,
          branch_id uuid NOT NULL,
          opportunity_id uuid NOT NULL REFERENCES erp.oportunidades(id),
          idempotency_key varchar(64) NOT NULL,
          request_hash varchar(64) NOT NULL,
          calculation_id uuid NOT NULL UNIQUE REFERENCES erp.calculos(id),
          PRIMARY KEY (tenant_id,branch_id,opportunity_id,idempotency_key)
        );
        ALTER TABLE infrastructure.calculation_receipts ENABLE ROW LEVEL SECURITY;
        ALTER TABLE infrastructure.calculation_receipts FORCE ROW LEVEL SECURITY;
        CREATE POLICY tenant_branch_isolation ON infrastructure.calculation_receipts
          USING (tenant_id::text=security.current_tenant_id() AND branch_id::text=security.current_branch_id())
          WITH CHECK (tenant_id::text=security.current_tenant_id() AND branch_id::text=security.current_branch_id());
        ALTER TABLE erp.calculos ENABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.calculos FORCE ROW LEVEL SECURITY;
        CREATE POLICY opportunity_scope ON erp.calculos
          USING (EXISTS(SELECT 1 FROM erp.oportunidades o WHERE o.id=oportunidade_id))
          WITH CHECK (EXISTS(SELECT 1 FROM erp.oportunidades o WHERE o.id=oportunidade_id));
        ALTER TABLE erp.calc_auto ENABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.calc_auto FORCE ROW LEVEL SECURITY;
        CREATE POLICY calculation_scope ON erp.calc_auto
          USING (EXISTS(SELECT 1 FROM erp.calculos c WHERE c.id=calculo_id))
          WITH CHECK (EXISTS(SELECT 1 FROM erp.calculos c WHERE c.id=calculo_id));
        ALTER TABLE erp.calculo_coberturas ENABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.calculo_coberturas FORCE ROW LEVEL SECURITY;
        CREATE POLICY calculation_scope ON erp.calculo_coberturas
          USING (EXISTS(SELECT 1 FROM erp.calculos c WHERE c.id=calculo_id))
          WITH CHECK (EXISTS(SELECT 1 FROM erp.calculos c WHERE c.id=calculo_id));
        CREATE OR REPLACE FUNCTION erp.enforce_calculation_request_scope()
        RETURNS trigger LANGUAGE plpgsql AS $$
        DECLARE parent erp.oportunidades;
        BEGIN
          SELECT * INTO parent FROM erp.oportunidades WHERE id=NEW.oportunidade_id;
          IF parent.id IS NULL OR NOT EXISTS (
            SELECT 1 FROM erp.ramos r WHERE r.id=NEW.ramo_id AND r.tenant_id=parent.tenant_id
          ) THEN RAISE EXCEPTION 'Invalid calculation branch scope' USING ERRCODE='23514'; END IF;
          IF NEW.segurado_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.segurados s WHERE s.id=NEW.segurado_id AND s.tenant_id=parent.tenant_id AND s.filial_id=parent.filial_id
          ) THEN RAISE EXCEPTION 'Invalid quoted person scope' USING ERRCODE='23514'; END IF;
          IF NEW.seguradora_anterior_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.seguradoras s WHERE s.id=NEW.seguradora_anterior_id AND s.tenant_id=parent.tenant_id
          ) THEN RAISE EXCEPTION 'Invalid previous insurer scope' USING ERRCODE='23514'; END IF;
          RETURN NEW;
        END; $$;
        CREATE TRIGGER trg_calculos_request_scope BEFORE INSERT ON erp.calculos
          FOR EACH ROW EXECUTE FUNCTION erp.enforce_calculation_request_scope();
        CREATE OR REPLACE FUNCTION erp.enforce_calculation_coverage_scope()
        RETURNS trigger LANGUAGE plpgsql AS $$
        BEGIN
          IF NOT EXISTS (SELECT 1 FROM erp.calculos c JOIN erp.coberturas_catalogo k ON k.ramo_id=c.ramo_id
            WHERE c.id=NEW.calculo_id AND k.id=NEW.cobertura_id)
          THEN RAISE EXCEPTION 'Invalid requested coverage scope' USING ERRCODE='23514'; END IF;
          RETURN NEW;
        END; $$;
        CREATE TRIGGER trg_calculo_coberturas_scope BEFORE INSERT ON erp.calculo_coberturas
          FOR EACH ROW EXECUTE FUNCTION erp.enforce_calculation_coverage_scope();
        CREATE OR REPLACE FUNCTION erp.prevent_calculation_snapshot_changes()
        RETURNS trigger LANGUAGE plpgsql AS $$
        BEGIN RAISE EXCEPTION 'Calculation snapshots are immutable' USING ERRCODE='23514'; END; $$;
        CREATE TRIGGER trg_calculos_immutable BEFORE UPDATE OR DELETE ON erp.calculos
          FOR EACH ROW EXECUTE FUNCTION erp.prevent_calculation_snapshot_changes();
        CREATE TRIGGER trg_calc_auto_immutable BEFORE UPDATE OR DELETE ON erp.calc_auto
          FOR EACH ROW EXECUTE FUNCTION erp.prevent_calculation_snapshot_changes();
        CREATE TRIGGER trg_calculo_coberturas_immutable BEFORE UPDATE OR DELETE ON erp.calculo_coberturas
          FOR EACH ROW EXECUTE FUNCTION erp.prevent_calculation_snapshot_changes();
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP TABLE infrastructure.calculation_receipts;
        DROP TRIGGER trg_calculos_immutable ON erp.calculos;
        DROP TRIGGER trg_calc_auto_immutable ON erp.calc_auto;
        DROP TRIGGER trg_calculo_coberturas_immutable ON erp.calculo_coberturas;
        DROP TRIGGER trg_calculos_request_scope ON erp.calculos;
        DROP TRIGGER trg_calculo_coberturas_scope ON erp.calculo_coberturas;
        DROP FUNCTION erp.prevent_calculation_snapshot_changes();
        DROP FUNCTION erp.enforce_calculation_request_scope();
        DROP FUNCTION erp.enforce_calculation_coverage_scope();
        DROP POLICY opportunity_scope ON erp.calculos;
        DROP POLICY calculation_scope ON erp.calc_auto;
        DROP POLICY calculation_scope ON erp.calculo_coberturas;
        ALTER TABLE erp.calculos DISABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.calc_auto DISABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.calculo_coberturas DISABLE ROW LEVEL SECURITY;
        """);
}
