using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Migrations;

[DbContext(typeof(WAssisDbContext))]
[Migration("20261012160000_EnableCanonicalManualExecutions")]
public sealed class EnableCanonicalManualExecutions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        -- v3.1 was additive: retire the old direct calculation/insurer result links.
        -- Existing unlinked legacy quotes must be reset in nonproduction before this migration.
        ALTER TABLE erp.cotacoes
          DROP COLUMN IF EXISTS calculo_id,
          DROP COLUMN IF EXISTS seguradora_id,
          DROP COLUMN IF EXISTS premio,
          DROP COLUMN IF EXISTS comissao,
          DROP COLUMN IF EXISTS referencia_externa,
          DROP COLUMN IF EXISTS produto_codigo,
          DROP COLUMN IF EXISTS produto_nome,
          DROP COLUMN IF EXISTS criado_em,
          DROP COLUMN IF EXISTS atualizado_em;
        ALTER TABLE erp.cotacoes ALTER COLUMN execucao_id SET NOT NULL;
        CREATE TABLE infrastructure.execution_receipts (
          tenant_id uuid NOT NULL,
          branch_id uuid NOT NULL,
          operation varchar(32) NOT NULL CHECK (operation IN ('create','retry','cancel','manual-quote')),
          resource_id uuid NOT NULL,
          idempotency_key varchar(64) NOT NULL,
          request_hash varchar(64) NOT NULL,
          execution_id uuid NOT NULL REFERENCES erp.calculo_execucoes(id),
          quote_id uuid REFERENCES erp.cotacoes(id),
          PRIMARY KEY (tenant_id,branch_id,operation,resource_id,idempotency_key)
        );
        ALTER TABLE infrastructure.execution_receipts ENABLE ROW LEVEL SECURITY;
        ALTER TABLE infrastructure.execution_receipts FORCE ROW LEVEL SECURITY;
        CREATE POLICY tenant_branch_isolation ON infrastructure.execution_receipts
          USING (tenant_id::text=security.current_tenant_id() AND branch_id::text=security.current_branch_id())
          WITH CHECK (tenant_id::text=security.current_tenant_id() AND branch_id::text=security.current_branch_id());
        ALTER TABLE erp.calculo_execucoes ENABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.calculo_execucoes FORCE ROW LEVEL SECURITY;
        CREATE POLICY calculation_scope ON erp.calculo_execucoes
          USING (EXISTS(SELECT 1 FROM erp.calculos c WHERE c.id=calculo_id))
          WITH CHECK (EXISTS(SELECT 1 FROM erp.calculos c WHERE c.id=calculo_id));
        ALTER TABLE erp.cotacoes ENABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.cotacoes FORCE ROW LEVEL SECURITY;
        CREATE POLICY execution_scope ON erp.cotacoes
          USING (EXISTS(SELECT 1 FROM erp.calculo_execucoes e WHERE e.id=execucao_id))
          WITH CHECK (EXISTS(SELECT 1 FROM erp.calculo_execucoes e WHERE e.id=execucao_id));
        ALTER TABLE erp.cotacao_coberturas ENABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.cotacao_coberturas FORCE ROW LEVEL SECURITY;
        CREATE POLICY quote_scope ON erp.cotacao_coberturas
          USING (EXISTS(SELECT 1 FROM erp.cotacoes q WHERE q.id=cotacao_id))
          WITH CHECK (EXISTS(SELECT 1 FROM erp.cotacoes q WHERE q.id=cotacao_id));
        ALTER TABLE erp.cotacao_parcelamentos ENABLE ROW LEVEL SECURITY;
        ALTER TABLE erp.cotacao_parcelamentos FORCE ROW LEVEL SECURITY;
        CREATE POLICY quote_scope ON erp.cotacao_parcelamentos
          USING (EXISTS(SELECT 1 FROM erp.cotacoes q WHERE q.id=cotacao_id))
          WITH CHECK (EXISTS(SELECT 1 FROM erp.cotacoes q WHERE q.id=cotacao_id));

        ALTER TABLE erp.calculo_execucoes ADD CONSTRAINT ck_execution_values CHECK (
          tentativa > 0 AND motor IN ('PROPRIO','AGGER','MANUAL')
          AND comissao_origem IN ('PADRAO_CALCULO','SOBRESCRITA')
          AND (comissao_pct_aplicada IS NULL OR comissao_pct_aplicada BETWEEN 0 AND 100)
          AND (comissao_origem <> 'SOBRESCRITA' OR comissao_pct_aplicada IS NOT NULL)
          AND status IN ('AGUARDANDO','EM_EXECUCAO','CONCLUIDA','PENDENTE_DADOS','INDISPONIVEL','ERRO','CANCELADA')
          AND (concluida_em IS NULL OR concluida_em >= criada_em)
          AND (iniciada_em IS NULL OR iniciada_em >= criada_em)
        );
        CREATE UNIQUE INDEX ux_execution_open ON erp.calculo_execucoes (calculo_id,seguradora_id)
          WHERE status IN ('AGUARDANDO','EM_EXECUCAO');
        ALTER TABLE erp.cotacoes ADD CONSTRAINT ck_quote_values CHECK (
          (premio_total IS NULL OR premio_total >= 0) AND (premio_liquido IS NULL OR premio_liquido >= 0)
          AND (iof IS NULL OR iof >= 0) AND (adicional_fracionamento IS NULL OR adicional_fracionamento >= 0)
          AND (comissao_valor IS NULL OR comissao_valor >= 0)
          AND (status IS NULL OR status IN ('APRESENTADA','APROVADA','DESCARTADA','EXPIRADA','RECUSADA'))
        );
        ALTER TABLE erp.cotacao_coberturas ADD CONSTRAINT ck_returned_coverage_values CHECK (
          length(btrim(chave_resultado)) BETWEEN 1 AND 100
          AND (limite_aceito IS NULL OR limite_aceito >= 0) AND (percentual_fipe_aceito IS NULL OR percentual_fipe_aceito BETWEEN 0 AND 200)
          AND (franquia_valor IS NULL OR franquia_valor >= 0) AND (premio IS NULL OR premio >= 0)
          AND (carencia_dias IS NULL OR carencia_dias >= 0)
          AND (participacao_obrigatoria_pct IS NULL OR participacao_obrigatoria_pct BETWEEN 0 AND 100)
          AND (ordem IS NULL OR ordem >= 0)
        );
        ALTER TABLE erp.cotacao_parcelamentos ADD CONSTRAINT ck_installment_option_values CHECK (
          length(btrim(codigo_opcao)) BETWEEN 1 AND 100 AND length(btrim(forma_pagamento)) BETWEEN 1 AND 100
          AND quantidade_parcelas BETWEEN 1 AND 360
          AND (valor_entrada IS NULL OR valor_entrada >= 0) AND (valor_parcela IS NULL OR valor_parcela >= 0)
          AND (valor_total IS NULL OR valor_total >= 0) AND (juros_pct IS NULL OR juros_pct BETWEEN 0 AND 100)
          AND (adicional_fracionamento IS NULL OR adicional_fracionamento >= 0) AND (ordem IS NULL OR ordem >= 0)
        );
        CREATE UNIQUE INDEX ux_quote_primary_installment ON erp.cotacao_parcelamentos (cotacao_id) WHERE principal IS TRUE;

        CREATE FUNCTION erp.enforce_execution_lifecycle() RETURNS trigger LANGUAGE plpgsql AS $$
        DECLARE parent erp.calculos; previous erp.calculo_execucoes; next_attempt integer;
        BEGIN
          IF TG_OP='DELETE' THEN RAISE EXCEPTION 'Execution history cannot be deleted' USING ERRCODE='23514'; END IF;
          IF TG_OP='INSERT' THEN
            SELECT * INTO parent FROM erp.calculos WHERE id=NEW.calculo_id FOR UPDATE;
            IF parent.id IS NULL OR NOT EXISTS (
              SELECT 1 FROM erp.seguradoras s JOIN erp.oportunidades o ON o.tenant_id=s.tenant_id
              WHERE o.id=parent.oportunidade_id AND s.id=NEW.seguradora_id AND s.ativo
            ) THEN RAISE EXCEPTION 'Invalid execution insurer scope' USING ERRCODE='23514'; END IF;
            SELECT COALESCE(max(tentativa),0)+1 INTO next_attempt FROM erp.calculo_execucoes
              WHERE calculo_id=NEW.calculo_id AND seguradora_id=NEW.seguradora_id;
            IF NEW.tentativa <> next_attempt OR NEW.status <> 'AGUARDANDO'
              OR (next_attempt=1 AND NEW.reexecucao_de_id IS NOT NULL)
              OR (next_attempt>1 AND NEW.reexecucao_de_id IS NULL)
            THEN RAISE EXCEPTION 'Invalid execution attempt' USING ERRCODE='23514'; END IF;
            IF NEW.reexecucao_de_id IS NOT NULL THEN
              SELECT * INTO previous FROM erp.calculo_execucoes WHERE id=NEW.reexecucao_de_id;
              IF previous.id IS NULL OR previous.calculo_id<>NEW.calculo_id OR previous.seguradora_id<>NEW.seguradora_id
                OR previous.status IN ('AGUARDANDO','EM_EXECUCAO') OR previous.motor<>NEW.motor
              THEN RAISE EXCEPTION 'Invalid execution predecessor' USING ERRCODE='23514'; END IF;
            END IF;
            IF NEW.comissao_origem='PADRAO_CALCULO' AND NEW.comissao_pct_aplicada IS DISTINCT FROM parent.comissao_sugerida_pct
            THEN RAISE EXCEPTION 'Invalid default commission' USING ERRCODE='23514'; END IF;
          ELSE
            IF ROW(NEW.id,NEW.calculo_id,NEW.seguradora_id,NEW.reexecucao_de_id,NEW.tentativa,NEW.motor,NEW.comissao_pct_aplicada,NEW.comissao_origem,NEW.criada_em)
              IS DISTINCT FROM ROW(OLD.id,OLD.calculo_id,OLD.seguradora_id,OLD.reexecucao_de_id,OLD.tentativa,OLD.motor,OLD.comissao_pct_aplicada,OLD.comissao_origem,OLD.criada_em)
              OR OLD.status NOT IN ('AGUARDANDO','EM_EXECUCAO')
            THEN RAISE EXCEPTION 'Execution history is immutable' USING ERRCODE='23514'; END IF;
            IF NEW.status='CONCLUIDA' AND NOT EXISTS(SELECT 1 FROM erp.cotacoes q WHERE q.execucao_id=NEW.id)
            THEN RAISE EXCEPTION 'Completed execution requires result' USING ERRCODE='23514'; END IF;
            IF NEW.status NOT IN ('AGUARDANDO','EM_EXECUCAO') AND NEW.concluida_em IS NULL
            THEN RAISE EXCEPTION 'Terminal execution requires timestamp' USING ERRCODE='23514'; END IF;
          END IF;
          RETURN NEW;
        END; $$;
        CREATE TRIGGER trg_execution_lifecycle BEFORE INSERT OR UPDATE OR DELETE ON erp.calculo_execucoes
          FOR EACH ROW EXECUTE FUNCTION erp.enforce_execution_lifecycle();
        CREATE FUNCTION erp.enforce_quote_result_scope() RETURNS trigger LANGUAGE plpgsql AS $$
        DECLARE execution_status text;
        BEGIN
          SELECT status INTO execution_status FROM erp.calculo_execucoes WHERE id=NEW.execucao_id FOR UPDATE;
          IF execution_status IS NULL OR execution_status NOT IN ('AGUARDANDO','EM_EXECUCAO')
          THEN RAISE EXCEPTION 'Result requires open execution' USING ERRCODE='23514'; END IF;
          RETURN NEW;
        END; $$;
        CREATE TRIGGER trg_quote_result_scope BEFORE INSERT ON erp.cotacoes
          FOR EACH ROW EXECUTE FUNCTION erp.enforce_quote_result_scope();
        CREATE FUNCTION erp.enforce_returned_coverage_scope() RETURNS trigger LANGUAGE plpgsql AS $$
        BEGIN
          IF NOT EXISTS (SELECT 1 FROM erp.cotacoes q JOIN erp.calculo_execucoes e ON e.id=q.execucao_id
            WHERE q.id=NEW.cotacao_id AND e.status IN ('AGUARDANDO','EM_EXECUCAO'))
          THEN RAISE EXCEPTION 'Result details require open execution' USING ERRCODE='23514'; END IF;
          IF NEW.cobertura_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM erp.cotacoes q JOIN erp.calculo_execucoes e ON e.id=q.execucao_id
            JOIN erp.calculos c ON c.id=e.calculo_id JOIN erp.coberturas_catalogo k ON k.ramo_id=c.ramo_id
            WHERE q.id=NEW.cotacao_id AND k.id=NEW.cobertura_id
          ) THEN RAISE EXCEPTION 'Invalid returned coverage scope' USING ERRCODE='23514'; END IF;
          RETURN NEW;
        END; $$;
        CREATE TRIGGER trg_returned_coverage_scope BEFORE INSERT ON erp.cotacao_coberturas
          FOR EACH ROW EXECUTE FUNCTION erp.enforce_returned_coverage_scope();
        CREATE FUNCTION erp.enforce_installment_result_scope() RETURNS trigger LANGUAGE plpgsql AS $$
        BEGIN
          IF NOT EXISTS (SELECT 1 FROM erp.cotacoes q JOIN erp.calculo_execucoes e ON e.id=q.execucao_id
            WHERE q.id=NEW.cotacao_id AND e.status IN ('AGUARDANDO','EM_EXECUCAO'))
          THEN RAISE EXCEPTION 'Result details require open execution' USING ERRCODE='23514'; END IF;
          RETURN NEW;
        END; $$;
        CREATE TRIGGER trg_installment_result_scope BEFORE INSERT ON erp.cotacao_parcelamentos
          FOR EACH ROW EXECUTE FUNCTION erp.enforce_installment_result_scope();
        CREATE FUNCTION erp.enforce_quote_completion() RETURNS trigger LANGUAGE plpgsql AS $$
        BEGIN
          IF NOT EXISTS (SELECT 1 FROM erp.calculo_execucoes WHERE id=NEW.execucao_id AND status='CONCLUIDA')
          THEN RAISE EXCEPTION 'Result requires completed execution at commit' USING ERRCODE='23514'; END IF;
          RETURN NEW;
        END; $$;
        CREATE CONSTRAINT TRIGGER trg_quote_completion AFTER INSERT ON erp.cotacoes
          DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION erp.enforce_quote_completion();
        CREATE TRIGGER trg_quote_result_immutable BEFORE UPDATE OR DELETE ON erp.cotacoes
          FOR EACH ROW EXECUTE FUNCTION erp.prevent_calculation_snapshot_changes();
        CREATE TRIGGER trg_quote_coverages_immutable BEFORE UPDATE OR DELETE ON erp.cotacao_coberturas
          FOR EACH ROW EXECUTE FUNCTION erp.prevent_calculation_snapshot_changes();
        CREATE TRIGGER trg_quote_installments_immutable BEFORE UPDATE OR DELETE ON erp.cotacao_parcelamentos
          FOR EACH ROW EXECUTE FUNCTION erp.prevent_calculation_snapshot_changes();
        """);

    protected override void Down(MigrationBuilder migrationBuilder) => throw new NotSupportedException(
        "Canonical result cutover is forward-only. Recreate disposable nonproduction fixtures instead of restoring legacy quote columns.");
}