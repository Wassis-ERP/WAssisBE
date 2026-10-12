using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Migrations;

[DbContext(typeof(WAssisDbContext))]
[Migration("20261012180000_EnableCanonicalPresentations")]
public sealed class EnableCanonicalPresentations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        ALTER TABLE erp.apresentacoes_comerciais ENABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.apresentacoes_comerciais FORCE ROW LEVEL SECURITY;
        CREATE POLICY opportunity_scope ON erp.apresentacoes_comerciais
          USING (EXISTS(SELECT 1 FROM erp.oportunidades o WHERE o.id=oportunidade_id))
          WITH CHECK (EXISTS(SELECT 1 FROM erp.oportunidades o WHERE o.id=oportunidade_id));
        ALTER TABLE erp.apresentacao_cotacoes ENABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.apresentacao_cotacoes FORCE ROW LEVEL SECURITY;
        CREATE POLICY presentation_scope ON erp.apresentacao_cotacoes
          USING (EXISTS(SELECT 1 FROM erp.apresentacoes_comerciais p WHERE p.id=apresentacao_id))
          WITH CHECK (EXISTS(SELECT 1 FROM erp.apresentacoes_comerciais p WHERE p.id=apresentacao_id));
        CREATE TABLE infrastructure.presentation_states (
          presentation_id uuid PRIMARY KEY REFERENCES erp.apresentacoes_comerciais(id),
          revision integer NOT NULL CHECK (revision>0)
        );
        INSERT INTO infrastructure.presentation_states SELECT id,1 FROM erp.apresentacoes_comerciais;
        ALTER TABLE infrastructure.presentation_states ENABLE ROW LEVEL SECURITY;
        ALTER TABLE infrastructure.presentation_states FORCE ROW LEVEL SECURITY;
        CREATE POLICY presentation_scope ON infrastructure.presentation_states
          USING (EXISTS(SELECT 1 FROM erp.apresentacoes_comerciais p WHERE p.id=presentation_id))
          WITH CHECK (EXISTS(SELECT 1 FROM erp.apresentacoes_comerciais p WHERE p.id=presentation_id));
        CREATE TABLE infrastructure.presentation_receipts (
          tenant_id uuid NOT NULL,
          branch_id uuid NOT NULL,
          operation varchar(16) NOT NULL CHECK (operation IN ('create','update')),
          resource_id uuid NOT NULL,
          idempotency_key varchar(64) NOT NULL,
          request_hash varchar(64) NOT NULL,
          presentation_id uuid NOT NULL REFERENCES erp.apresentacoes_comerciais(id),
          revision integer NOT NULL CHECK (revision>0),
          PRIMARY KEY (tenant_id,branch_id,operation,resource_id,idempotency_key)
        );
        ALTER TABLE infrastructure.presentation_receipts ENABLE ROW LEVEL SECURITY;
        ALTER TABLE infrastructure.presentation_receipts FORCE ROW LEVEL SECURITY;
        CREATE POLICY tenant_branch_isolation ON infrastructure.presentation_receipts
          USING (tenant_id::text=security.current_tenant_id() AND branch_id::text=security.current_branch_id())
          WITH CHECK (tenant_id::text=security.current_tenant_id() AND branch_id::text=security.current_branch_id());
        ALTER TABLE erp.apresentacoes_comerciais ADD CONSTRAINT ck_presentation_values CHECK (
          layout IN ('VERTICAL','HORIZONTAL') AND criterio_ordenacao IN ('RECOMENDACAO','PREMIO','FRANQUIA','MANUAL')
          AND status IN ('RASCUNHO','GERADA')
        );
        ALTER TABLE erp.apresentacao_cotacoes ADD CONSTRAINT ck_presentation_item_order CHECK (ordem>0);

        CREATE FUNCTION erp.enforce_presentation_scope() RETURNS trigger LANGUAGE plpgsql AS $$
        BEGIN
          IF TG_OP='UPDATE' AND ROW(NEW.id,NEW.oportunidade_id,NEW.criado_por_id,NEW.criado_em)
            IS DISTINCT FROM ROW(OLD.id,OLD.oportunidade_id,OLD.criado_por_id,OLD.criado_em)
          THEN RAISE EXCEPTION 'Presentation identity is immutable' USING ERRCODE='23514'; END IF;
          IF NOT EXISTS(SELECT 1 FROM erp.oportunidades WHERE id=NEW.oportunidade_id)
          THEN RAISE EXCEPTION 'Invalid presentation opportunity' USING ERRCODE='23514'; END IF;
          IF NEW.criado_por_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.profiles p JOIN erp.oportunidades o ON o.tenant_id=p.tenant_id
            WHERE p.id=NEW.criado_por_id AND o.id=NEW.oportunidade_id
          ) THEN RAISE EXCEPTION 'Invalid presentation author scope' USING ERRCODE='23514'; END IF;
          RETURN NEW;
        END; $$;
        CREATE TRIGGER trg_presentation_scope BEFORE INSERT OR UPDATE ON erp.apresentacoes_comerciais
          FOR EACH ROW EXECUTE FUNCTION erp.enforce_presentation_scope();
        CREATE FUNCTION erp.enforce_presentation_item_scope() RETURNS trigger LANGUAGE plpgsql AS $$
        DECLARE parent_opportunity uuid;
        BEGIN
          IF TG_OP='DELETE' THEN
            PERFORM 1 FROM erp.apresentacoes_comerciais WHERE id=OLD.apresentacao_id FOR UPDATE;
            RETURN OLD;
          END IF;
          IF TG_OP='UPDATE' AND NEW.apresentacao_id<>OLD.apresentacao_id
          THEN RAISE EXCEPTION 'Presentation item cannot move' USING ERRCODE='23514'; END IF;
          SELECT oportunidade_id INTO parent_opportunity FROM erp.apresentacoes_comerciais WHERE id=NEW.apresentacao_id FOR UPDATE;
          IF parent_opportunity IS NULL OR NOT EXISTS (
            SELECT 1 FROM erp.cotacoes q JOIN erp.calculo_execucoes e ON e.id=q.execucao_id JOIN erp.calculos c ON c.id=e.calculo_id
            WHERE q.id=NEW.cotacao_id AND c.oportunidade_id=parent_opportunity AND e.status='CONCLUIDA'
          ) THEN RAISE EXCEPTION 'Invalid presentation quote scope' USING ERRCODE='23514'; END IF;
          IF NEW.parcelamento_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.cotacao_parcelamentos p WHERE p.id=NEW.parcelamento_id AND p.cotacao_id=NEW.cotacao_id
          ) THEN RAISE EXCEPTION 'Invalid presentation installment' USING ERRCODE='23514'; END IF;
          RETURN NEW;
        END; $$;
        CREATE TRIGGER trg_presentation_item_scope BEFORE INSERT OR UPDATE OR DELETE ON erp.apresentacao_cotacoes
          FOR EACH ROW EXECUTE FUNCTION erp.enforce_presentation_item_scope();
        CREATE FUNCTION erp.validate_presentation_final_state() RETURNS trigger LANGUAGE plpgsql AS $$
        DECLARE target uuid; parent erp.apresentacoes_comerciais; item_count integer;
        BEGIN
          IF TG_TABLE_NAME='apresentacoes_comerciais' THEN target=NEW.id;
          ELSIF TG_OP='DELETE' THEN target=OLD.apresentacao_id;
          ELSE target=NEW.apresentacao_id; END IF;
          SELECT * INTO parent FROM erp.apresentacoes_comerciais WHERE id=target;
          IF parent.id IS NULL THEN RETURN NULL; END IF;
          SELECT count(*) INTO item_count FROM erp.apresentacao_cotacoes WHERE apresentacao_id=target;
          IF item_count>5 OR (parent.status='GERADA' AND item_count=0)
          THEN RAISE EXCEPTION 'Invalid presentation selection size' USING ERRCODE='23514'; END IF;
          IF parent.cotacao_escolhida_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.apresentacao_cotacoes WHERE apresentacao_id=target AND cotacao_id=parent.cotacao_escolhida_id
          ) THEN RAISE EXCEPTION 'Selected quote must belong to presentation' USING ERRCODE='23514'; END IF;
          RETURN NULL;
        END; $$;
        CREATE CONSTRAINT TRIGGER trg_presentation_final_state AFTER INSERT OR UPDATE ON erp.apresentacoes_comerciais
          DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION erp.validate_presentation_final_state();
        CREATE CONSTRAINT TRIGGER trg_presentation_items_final_state AFTER INSERT OR UPDATE OR DELETE ON erp.apresentacao_cotacoes
          DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION erp.validate_presentation_final_state();
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DROP TABLE infrastructure.presentation_receipts;
        DROP TABLE infrastructure.presentation_states;
        DROP TRIGGER trg_presentation_final_state ON erp.apresentacoes_comerciais;
        DROP TRIGGER trg_presentation_items_final_state ON erp.apresentacao_cotacoes;
        DROP TRIGGER trg_presentation_item_scope ON erp.apresentacao_cotacoes;
        DROP TRIGGER trg_presentation_scope ON erp.apresentacoes_comerciais;
        DROP FUNCTION erp.validate_presentation_final_state();
        DROP FUNCTION erp.enforce_presentation_item_scope();
        DROP FUNCTION erp.enforce_presentation_scope();
        ALTER TABLE erp.apresentacoes_comerciais DROP CONSTRAINT ck_presentation_values;
        ALTER TABLE erp.apresentacao_cotacoes DROP CONSTRAINT ck_presentation_item_order;
        DROP POLICY opportunity_scope ON erp.apresentacoes_comerciais;
        DROP POLICY presentation_scope ON erp.apresentacao_cotacoes;
        ALTER TABLE erp.apresentacoes_comerciais DISABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.apresentacao_cotacoes DISABLE ROW LEVEL SECURITY;
        """);
}
