-- CRM contract expansion derived from the frontend hand-off consolidated through PR #40.
-- The DBML remains the structural source; these are the implementation-level business columns.

ALTER TABLE erp.filiais
  ADD COLUMN razao_social text,
  ADD COLUMN fantasia text,
  ADD COLUMN cnpj_cpf text,
  ADD COLUMN susep text,
  ADD COLUMN percentual_imposto numeric(9,4),
  ADD COLUMN lgpd_aceito boolean NOT NULL DEFAULT false,
  ADD COLUMN lgpd_aceito_em timestamp with time zone,
  ADD COLUMN gerente text,
  ADD COLUMN gerente_id uuid,
  ADD COLUMN contato text,
  ADD COLUMN home_page text,
  ADD COLUMN email text,
  ADD COLUMN telefone text,
  ADD COLUMN celular text,
  ADD COLUMN telefone2 text,
  ADD COLUMN cep text,
  ADD COLUMN endereco text,
  ADD COLUMN numero text,
  ADD COLUMN complemento text,
  ADD COLUMN bairro text,
  ADD COLUMN cidade text,
  ADD COLUMN uf text,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

UPDATE erp.filiais
SET razao_social = COALESCE(razao_social, nome),
    fantasia = COALESCE(fantasia, nome),
    cnpj_cpf = COALESCE(cnpj_cpf, cnpj);

CREATE UNIQUE INDEX ux_filiais_tenant_documento
ON erp.filiais (tenant_id, cnpj_cpf)
WHERE cnpj_cpf IS NOT NULL;

CREATE UNIQUE INDEX ux_filiais_matriz_por_tenant
ON erp.filiais (tenant_id)
WHERE matriz_id IS NULL AND ativo;

ALTER TABLE erp.profiles
  ADD COLUMN full_name text,
  ADD COLUMN avatar_url text,
  ADD COLUMN telefone text,
  ADD COLUMN ultimo_acesso_em timestamp with time zone,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

CREATE TABLE erp.perfis (
  id uuid PRIMARY KEY,
  tenant_id uuid NOT NULL REFERENCES erp.tenants(id) ON DELETE RESTRICT,
  nome text NOT NULL,
  sistema boolean NOT NULL DEFAULT false,
  ativo boolean NOT NULL DEFAULT true,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  updated_at timestamp with time zone NOT NULL DEFAULT now()
);

CREATE UNIQUE INDEX ux_perfis_tenant_nome
ON erp.perfis (tenant_id, lower(nome));

ALTER TABLE erp.profile_filiais
  ADD COLUMN perfil_id uuid REFERENCES erp.perfis(id) ON DELETE RESTRICT,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

CREATE UNIQUE INDEX ux_profile_filiais_principal
ON erp.profile_filiais (profile_id)
WHERE principal;

ALTER TABLE erp.role_permissions
  ALTER COLUMN papel DROP NOT NULL,
  ALTER COLUMN modulo DROP NOT NULL,
  ADD COLUMN perfil_id uuid REFERENCES erp.perfis(id) ON DELETE CASCADE,
  ADD COLUMN module text,
  ADD COLUMN can_read boolean NOT NULL DEFAULT false,
  ADD COLUMN can_create boolean NOT NULL DEFAULT false,
  ADD COLUMN can_update boolean NOT NULL DEFAULT false,
  ADD COLUMN can_delete boolean NOT NULL DEFAULT false,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

CREATE UNIQUE INDEX ux_role_permissions_perfil_module
ON erp.role_permissions (perfil_id, module)
WHERE perfil_id IS NOT NULL AND module IS NOT NULL;

ALTER TABLE erp.produtores
  ADD COLUMN nome text,
  ADD COLUMN cpf_cnpj text,
  ADD COLUMN email text,
  ADD COLUMN telefone text,
  ADD COLUMN celular text,
  ADD COLUMN banco text,
  ADD COLUMN agencia text,
  ADD COLUMN conta text,
  ADD COLUMN chave_pix text,
  ADD COLUMN percentual_repasse_padrao numeric(9,4),
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

CREATE UNIQUE INDEX ux_produtores_profile
ON erp.produtores (profile_id)
WHERE profile_id IS NOT NULL;

ALTER TABLE erp.filiais
  ADD CONSTRAINT fk_filiais_gerente
  FOREIGN KEY (gerente_id) REFERENCES erp.produtores(id) ON DELETE RESTRICT;

ALTER TABLE erp.segurados
  ADD COLUMN tipo text,
  ADD COLUMN nome text,
  ADD COLUMN nome_fantasia text,
  ADD COLUMN status text NOT NULL DEFAULT 'PROSPECTO',
  ADD COLUMN lgpd_autorizado boolean NOT NULL DEFAULT false,
  ADD COLUMN email text,
  ADD COLUMN telefone text,
  ADD COLUMN celular text,
  ADD COLUMN data_nascimento date,
  ADD COLUMN sexo text,
  ADD COLUMN estado_civil text,
  ADD COLUMN porte text,
  ADD COLUMN cnae text,
  ADD COLUMN site text,
  ADD COLUMN cep text,
  ADD COLUMN logradouro text,
  ADD COLUMN numero text,
  ADD COLUMN complemento text,
  ADD COLUMN bairro text,
  ADD COLUMN cidade text,
  ADD COLUMN uf text,
  ADD COLUMN observacoes text,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.pessoa_contato
  ADD COLUMN tenant_id uuid REFERENCES erp.tenants(id) ON DELETE RESTRICT,
  ADD COLUMN cargo text,
  ADD COLUMN principal boolean NOT NULL DEFAULT false,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

UPDATE erp.pessoa_contato pc
SET tenant_id = s.tenant_id
FROM erp.segurados s
WHERE s.id = pc.pj_id AND pc.tenant_id IS NULL;

ALTER TABLE erp.pessoa_contato
  ALTER COLUMN tenant_id SET NOT NULL;

CREATE UNIQUE INDEX ux_pessoa_contato_principal
ON erp.pessoa_contato (pj_id)
WHERE principal;

ALTER TABLE erp.seguradoras
  ADD COLUMN nome text,
  ADD COLUMN nome_curto text,
  ADD COLUMN cnpj text,
  ADD COLUMN codigo_susep text,
  ADD COLUMN codigo_interno text,
  ADD COLUMN site text,
  ADD COLUMN portal_url text,
  ADD COLUMN telefone_sac text,
  ADD COLUMN telefone_assistencia text,
  ADD COLUMN email text,
  ADD COLUMN aceita_importacao_pdf boolean NOT NULL DEFAULT false,
  ADD COLUMN aceita_busca_automatica boolean NOT NULL DEFAULT false,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN observacoes text,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

CREATE UNIQUE INDEX ux_seguradoras_tenant_cnpj
ON erp.seguradoras (tenant_id, cnpj)
WHERE cnpj IS NOT NULL;

ALTER TABLE erp.ramos
  ADD COLUMN nome text,
  ADD COLUMN codigo_susep text,
  ADD COLUMN risk_type text,
  ADD COLUMN grupo_operacional text,
  ADD COLUMN forma_calculo text,
  ADD COLUMN is_monthly boolean NOT NULL DEFAULT false,
  ADD COLUMN renovavel boolean NOT NULL DEFAULT true,
  ADD COLUMN permite_endosso boolean NOT NULL DEFAULT true,
  ADD COLUMN exige_item boolean NOT NULL DEFAULT true,
  ADD COLUMN exige_coberturas boolean NOT NULL DEFAULT true,
  ADD COLUMN ordem integer NOT NULL DEFAULT 0,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN observacoes text,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.origens
  ADD COLUMN nome text,
  ADD COLUMN tipo text,
  ADD COLUMN ordem integer NOT NULL DEFAULT 0,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.motivos_perda
  ADD COLUMN nome text,
  ADD COLUMN categoria text,
  ADD COLUMN ordem integer NOT NULL DEFAULT 0,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.coberturas_catalogo
  ADD COLUMN codigo text,
  ADD COLUMN codigo_susep text,
  ADD COLUMN nome text,
  ADD COLUMN descricao text,
  ADD COLUMN tipo_cobertura text,
  ADD COLUMN caracteristica text,
  ADD COLUMN tipo_risco text,
  ADD COLUMN modalidade text,
  ADD COLUMN capital_lmi_padrao numeric(18,2),
  ADD COLUMN franquia_padrao numeric(18,2),
  ADD COLUMN carencia_dias integer,
  ADD COLUMN obrigatoria boolean NOT NULL DEFAULT false,
  ADD COLUMN ordem integer NOT NULL DEFAULT 0,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.pipelines
  ADD COLUMN nome text,
  ADD COLUMN entidade_tipo text,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN ordem integer NOT NULL DEFAULT 0,
  ADD COLUMN descricao text,
  ADD COLUMN modelo_fabrica boolean NOT NULL DEFAULT false,
  ADD COLUMN permite_customizacao boolean NOT NULL DEFAULT true,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.pipeline_stages
  ADD COLUMN nome text,
  ADD COLUMN cor text,
  ADD COLUMN ordem integer NOT NULL DEFAULT 0,
  ADD COLUMN codigo text,
  ADD COLUMN tipo_stage text,
  ADD COLUMN probabilidade numeric(5,2),
  ADD COLUMN sla_dias integer,
  ADD COLUMN finaliza_com_sucesso boolean NOT NULL DEFAULT false,
  ADD COLUMN finaliza_com_perda boolean NOT NULL DEFAULT false,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.oportunidades
  ADD COLUMN nome text,
  ADD COLUMN status text NOT NULL DEFAULT 'ABERTA',
  ADD COLUMN responsavel_id uuid,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.calculos
  ADD COLUMN origem text NOT NULL DEFAULT 'PROPRIO',
  ADD COLUMN aggilizador_id text,
  ADD COLUMN comissao_sugerida_pct numeric(9,4),
  ADD COLUMN rotulo_versao text,
  ADD COLUMN tipo_seguro text,
  ADD COLUMN bonus text,
  ADD COLUMN banco text,
  ADD COLUMN sinistros integer,
  ADD COLUMN vigencia_inicio date,
  ADD COLUMN vigencia_fim date,
  ADD COLUMN vigencia_final_anterior date,
  ADD COLUMN numero_apolice_anterior text,
  ADD COLUMN codigo_identificacao text,
  ADD COLUMN observacoes text,
  ADD COLUMN status text NOT NULL DEFAULT 'RASCUNHO',
  ADD COLUMN criado_por_id uuid,
  ADD COLUMN criado_em timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN atualizado_em timestamp with time zone NOT NULL DEFAULT now();

CREATE INDEX ix_calculos_oportunidade_criado_em
ON erp.calculos (oportunidade_id, criado_em DESC);

ALTER TABLE erp.calc_auto
  ADD COLUMN condutor_nome text,
  ADD COLUMN condutor_documento text,
  ADD COLUMN condutor_nascimento date,
  ADD COLUMN condutor_sexo text,
  ADD COLUMN condutor_estado_civil text,
  ADD COLUMN tempo_habilitacao_anos integer,
  ADD COLUMN numero_cnh text,
  ADD COLUMN relacao_condutor_segurado text,
  ADD COLUMN veiculo_chassi text,
  ADD COLUMN veiculo_placa text,
  ADD COLUMN veiculo_marca text,
  ADD COLUMN veiculo_modelo text,
  ADD COLUMN veiculo_fipe text,
  ADD COLUMN ano_fabricacao integer,
  ADD COLUMN ano_modelo integer,
  ADD COLUMN zero_km boolean,
  ADD COLUMN possui_rastreador boolean,
  ADD COLUMN possui_antifurto boolean,
  ADD COLUMN financiado boolean,
  ADD COLUMN blindado boolean,
  ADD COLUMN combustivel_codigo text,
  ADD COLUMN cep_pernoite text,
  ADD COLUMN possui_gnv boolean,
  ADD COLUMN condutor_menor_24 boolean;

ALTER TABLE erp.calc_residencia
  ADD COLUMN cep text,
  ADD COLUMN endereco text,
  ADD COLUMN numero text,
  ADD COLUMN complemento text,
  ADD COLUMN tipo_imovel text,
  ADD COLUMN tipo_ocupacao text,
  ADD COLUMN area_m2 numeric(12,2),
  ADD COLUMN valor_construcao numeric(18,2),
  ADD COLUMN possui_alarme boolean,
  ADD COLUMN possui_portaria boolean;

ALTER TABLE erp.calc_condominio
  ADD COLUMN cep text,
  ADD COLUMN endereco text,
  ADD COLUMN numero text,
  ADD COLUMN tipo_condominio text,
  ADD COLUMN quantidade_unidades integer,
  ADD COLUMN quantidade_blocos integer,
  ADD COLUMN area_construida_m2 numeric(12,2),
  ADD COLUMN valor_reconstrucao numeric(18,2);

ALTER TABLE erp.calc_vida
  ADD COLUMN modalidade text,
  ADD COLUMN quantidade_vidas integer,
  ADD COLUMN capital_individual numeric(18,2),
  ADD COLUMN faixa_etaria_media numeric(5,2),
  ADD COLUMN atividade_principal text,
  ADD COLUMN percentual_afastados numeric(9,4);

ALTER TABLE erp.calc_empresa
  ADD COLUMN cnpj text,
  ADD COLUMN atividade text,
  ADD COLUMN cep_risco text,
  ADD COLUMN endereco_risco text,
  ADD COLUMN area_m2 numeric(12,2),
  ADD COLUMN valor_predio numeric(18,2),
  ADD COLUMN valor_conteudo numeric(18,2),
  ADD COLUMN faturamento_anual numeric(18,2);

ALTER TABLE erp.calc_diversos
  ADD COLUMN objeto_seguro text,
  ADD COLUMN local_risco text,
  ADD COLUMN valor_risco numeric(18,2),
  ADD COLUMN quantidade integer,
  ADD COLUMN observacoes text;

ALTER TABLE erp.cotacoes
  ADD COLUMN premio numeric(18,2),
  ADD COLUMN comissao numeric(18,2),
  ADD COLUMN status text NOT NULL DEFAULT 'APRESENTADA',
  ADD COLUMN link_proposta text,
  ADD COLUMN referencia_externa text,
  ADD COLUMN produto_codigo text,
  ADD COLUMN produto_nome text,
  ADD COLUMN criado_em timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN atualizado_em timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.recebimento_grades
  ADD COLUMN nome text,
  ADD COLUMN tipo text,
  ADD COLUMN qtd_parcelas integer,
  ADD COLUMN base_calculo text,
  ADD COLUMN percentual_default numeric(9,4),
  ADD COLUMN considera_iof boolean NOT NULL DEFAULT false,
  ADD COLUMN considera_adicional_fracionamento boolean NOT NULL DEFAULT false,
  ADD COLUMN vitalicio boolean NOT NULL DEFAULT false,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN observacoes text,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.recebimento_grade_parcelas
  ADD COLUMN numero integer,
  ADD COLUMN percentual numeric(9,4),
  ADD COLUMN percentual_sobre text,
  ADD COLUMN dias_apos_vencimento integer NOT NULL DEFAULT 0,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.repasse_regras
  ADD COLUMN papel text,
  ADD COLUMN tipo_documento text,
  ADD COLUMN base text,
  ADD COLUMN percentual numeric(9,4),
  ADD COLUMN valor_fixo numeric(18,2),
  ADD COLUMN gatilho text,
  ADD COLUMN qtd_parcelas integer,
  ADD COLUMN limite_parcelas integer,
  ADD COLUMN prioridade integer NOT NULL DEFAULT 0,
  ADD COLUMN inicio_vigencia date,
  ADD COLUMN fim_vigencia date,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN observacoes text,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.campo_definicoes
  ADD COLUMN nome text,
  ADD COLUMN tipo_dado text,
  ADD COLUMN formato text,
  ADD COLUMN obrigatorio boolean NOT NULL DEFAULT false,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN ordem integer NOT NULL DEFAULT 0,
  ADD COLUMN ajuda text,
  ADD COLUMN min_valor numeric,
  ADD COLUMN max_valor numeric,
  ADD COLUMN tamanho_max integer,
  ADD COLUMN mascara text,
  ADD COLUMN placeholder text,
  ADD COLUMN agrupamento text,
  ADD COLUMN visivel_em_listagem boolean NOT NULL DEFAULT false,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

ALTER TABLE erp.campo_opcoes
  ADD COLUMN rotulo text,
  ADD COLUMN valor text,
  ADD COLUMN ordem integer NOT NULL DEFAULT 0,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true,
  ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
  ADD COLUMN updated_at timestamp with time zone NOT NULL DEFAULT now();

CREATE OR REPLACE FUNCTION erp.enforce_profile_filial_perfil_tenant()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
  IF NEW.perfil_id IS NOT NULL AND NOT EXISTS (
    SELECT 1
    FROM erp.profiles p
    JOIN erp.perfis pe ON pe.id = NEW.perfil_id AND pe.tenant_id = p.tenant_id
    WHERE p.id = NEW.profile_id
  ) THEN
    RAISE EXCEPTION 'profile % and perfil % must belong to the same tenant', NEW.profile_id, NEW.perfil_id
      USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_profile_filiais_perfil_tenant
BEFORE INSERT OR UPDATE OF profile_id, perfil_id ON erp.profile_filiais
FOR EACH ROW EXECUTE FUNCTION erp.enforce_profile_filial_perfil_tenant();

CREATE OR REPLACE FUNCTION erp.enforce_produtor_profile_tenant()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
  IF NEW.profile_id IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM erp.profiles p
    WHERE p.id = NEW.profile_id AND p.tenant_id = NEW.tenant_id
  ) THEN
    RAISE EXCEPTION 'producer profile must belong to the producer tenant'
      USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_produtores_profile_tenant
BEFORE INSERT OR UPDATE OF tenant_id, profile_id ON erp.produtores
FOR EACH ROW EXECUTE FUNCTION erp.enforce_produtor_profile_tenant();

CREATE OR REPLACE FUNCTION erp.enforce_filial_gerente_tenant()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
  IF NEW.gerente_id IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM erp.produtores p
    WHERE p.id = NEW.gerente_id AND p.tenant_id = NEW.tenant_id
  ) THEN
    RAISE EXCEPTION 'branch manager must belong to the branch tenant'
      USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_filiais_gerente_tenant
BEFORE INSERT OR UPDATE OF tenant_id, gerente_id ON erp.filiais
FOR EACH ROW EXECUTE FUNCTION erp.enforce_filial_gerente_tenant();

CREATE OR REPLACE FUNCTION erp.enforce_pessoa_contato_scope()
RETURNS trigger
LANGUAGE plpgsql
AS $$
DECLARE
  derived_tenant uuid;
BEGIN
  SELECT pj.tenant_id INTO derived_tenant
  FROM erp.segurados pj
  WHERE pj.id = NEW.pj_id AND upper(pj.tipo) = 'PJ';

  IF derived_tenant IS NULL THEN
    RAISE EXCEPTION 'pj_id must reference a PJ insured record'
      USING ERRCODE = '23514';
  END IF;

  NEW.tenant_id := derived_tenant;

  IF NEW.pf_id IS NOT NULL AND NOT EXISTS (
    SELECT 1
    FROM erp.segurados pj
    JOIN erp.segurados pf
      ON pf.id = NEW.pf_id
     AND upper(pf.tipo) = 'PF'
     AND pf.tenant_id = pj.tenant_id
     AND pf.filial_id = pj.filial_id
    WHERE pj.id = NEW.pj_id
  ) THEN
    RAISE EXCEPTION 'PJ and PF contact records must belong to the same tenant and branch'
      USING ERRCODE = '23514';
  END IF;

  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_pessoa_contato_scope
BEFORE INSERT OR UPDATE OF pj_id, pf_id, tenant_id ON erp.pessoa_contato
FOR EACH ROW EXECUTE FUNCTION erp.enforce_pessoa_contato_scope();

CREATE OR REPLACE FUNCTION erp.enforce_recebimento_grade_tenant()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM erp.seguradoras s
    JOIN erp.ramos r ON r.id = NEW.ramo_id AND r.tenant_id = s.tenant_id
    WHERE s.id = NEW.seguradora_id
  ) THEN
    RAISE EXCEPTION 'insurer and insurance branch must belong to the same tenant'
      USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_recebimento_grades_tenant
BEFORE INSERT OR UPDATE OF seguradora_id, ramo_id ON erp.recebimento_grades
FOR EACH ROW EXECUTE FUNCTION erp.enforce_recebimento_grade_tenant();

CREATE OR REPLACE FUNCTION erp.enforce_calculo_scope()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM erp.oportunidades o
    JOIN erp.ramos r ON r.id = NEW.ramo_id AND r.tenant_id = o.tenant_id
    WHERE o.id = NEW.oportunidade_id
      AND (
        NEW.segurado_id IS NULL OR EXISTS (
          SELECT 1 FROM erp.segurados s
          WHERE s.id = NEW.segurado_id
            AND s.tenant_id = o.tenant_id
            AND s.filial_id = o.filial_id
        )
      )
      AND (
        NEW.seguradora_anterior_id IS NULL OR EXISTS (
          SELECT 1 FROM erp.seguradoras sa
          WHERE sa.id = NEW.seguradora_anterior_id AND sa.tenant_id = o.tenant_id
        )
      )
  ) THEN
    RAISE EXCEPTION 'calculation references must remain in the opportunity tenant and branch'
      USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_calculos_scope
BEFORE INSERT OR UPDATE OF oportunidade_id, ramo_id, segurado_id, seguradora_anterior_id ON erp.calculos
FOR EACH ROW EXECUTE FUNCTION erp.enforce_calculo_scope();

CREATE OR REPLACE FUNCTION erp.enforce_repasse_regra_scope()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
  IF (NEW.filial_id IS NOT NULL AND NOT EXISTS (
      SELECT 1 FROM erp.filiais f WHERE f.id = NEW.filial_id AND f.tenant_id = NEW.tenant_id
    )) OR (NEW.produtor_id IS NOT NULL AND NOT EXISTS (
      SELECT 1 FROM erp.produtores p WHERE p.id = NEW.produtor_id AND p.tenant_id = NEW.tenant_id
    )) OR (NEW.ramo_id IS NOT NULL AND NOT EXISTS (
      SELECT 1 FROM erp.ramos r WHERE r.id = NEW.ramo_id AND r.tenant_id = NEW.tenant_id
    )) THEN
    RAISE EXCEPTION 'transfer rule references must belong to the rule tenant'
      USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_repasse_regras_scope
BEFORE INSERT OR UPDATE OF tenant_id, filial_id, produtor_id, ramo_id ON erp.repasse_regras
FOR EACH ROW EXECUTE FUNCTION erp.enforce_repasse_regra_scope();

INSERT INTO erp.schema_contract_versions(version, source_file)
VALUES ('1.1.0', 'WassisCRM relatorio-endpoints-campos.md @ PR #40');
