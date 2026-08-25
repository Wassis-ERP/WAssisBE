-- Generated from docs/database/wassis_erp_esqueleto_v1_0.dbml.

-- Do not edit manually; update the DBML and rerun tools/generate-erp-core-schema.mjs.

CREATE SCHEMA IF NOT EXISTS erp;

COMMENT ON SCHEMA erp IS 'W.Assis ERP core schema contract v1.0';

CREATE TABLE erp."tenants" (
  "id" uuid PRIMARY KEY NOT NULL
);

CREATE TABLE erp."filiais" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "matriz_id" uuid
);

CREATE TABLE erp."profiles" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL
);

CREATE TABLE erp."profile_filiais" (
  "id" uuid PRIMARY KEY NOT NULL,
  "profile_id" uuid NOT NULL,
  "filial_id" uuid NOT NULL
);

CREATE TABLE erp."role_permissions" (
  "id" uuid PRIMARY KEY NOT NULL
);

CREATE TABLE erp."produtores" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "profile_id" uuid
);

CREATE TABLE erp."segurados" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid NOT NULL,
  "produtor_id" uuid,
  "gerente_id" uuid,
  "cpf_cnpj" text
);

CREATE TABLE erp."pessoa_contato" (
  "id" uuid PRIMARY KEY NOT NULL,
  "pj_id" uuid NOT NULL,
  "pf_id" uuid
);

CREATE TABLE erp."seguradoras" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL
);

CREATE TABLE erp."ramos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL
);

CREATE TABLE erp."origens" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL
);

CREATE TABLE erp."motivos_perda" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL
);

CREATE TABLE erp."coberturas_catalogo" (
  "id" uuid PRIMARY KEY NOT NULL,
  "ramo_id" uuid NOT NULL
);

CREATE TABLE erp."pipelines" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid
);

CREATE TABLE erp."pipeline_stages" (
  "id" uuid PRIMARY KEY NOT NULL,
  "pipeline_id" uuid NOT NULL
);

CREATE TABLE erp."oportunidades" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid NOT NULL,
  "segurado_id" uuid,
  "ramo_id" uuid,
  "origem_id" uuid,
  "apolice_origem_id" uuid,
  "responsavel_id" uuid,
  "stage_id" uuid NOT NULL,
  "motivo_perda_id" uuid
);

CREATE TABLE erp."calculos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "oportunidade_id" uuid NOT NULL,
  "ramo_id" uuid NOT NULL,
  "segurado_id" uuid,
  "seguradora_anterior_id" uuid
);

CREATE TABLE erp."calc_auto" (
  "calculo_id" uuid PRIMARY KEY NOT NULL
);

CREATE TABLE erp."calc_residencia" (
  "calculo_id" uuid PRIMARY KEY NOT NULL
);

CREATE TABLE erp."calc_condominio" (
  "calculo_id" uuid PRIMARY KEY NOT NULL
);

CREATE TABLE erp."calc_vida" (
  "calculo_id" uuid PRIMARY KEY NOT NULL
);

CREATE TABLE erp."calc_empresa" (
  "calculo_id" uuid PRIMARY KEY NOT NULL
);

CREATE TABLE erp."calc_diversos" (
  "calculo_id" uuid PRIMARY KEY NOT NULL
);

CREATE TABLE erp."calculo_coberturas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "calculo_id" uuid NOT NULL,
  "cobertura_id" uuid
);

CREATE TABLE erp."cotacoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "calculo_id" uuid NOT NULL,
  "seguradora_id" uuid
);

CREATE TABLE erp."apolices" (
  "id" uuid PRIMARY KEY NOT NULL,
  "segurado_id" uuid NOT NULL,
  "seguradora_id" uuid,
  "ramo_id" uuid,
  "status" text,
  "renovada_de_id" uuid,
  "produtor_id" uuid
);

CREATE TABLE erp."propostas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "apolice_id" uuid NOT NULL,
  "tipo" text,
  "cotacao_id" uuid UNIQUE,
  "stage_id" uuid NOT NULL,
  "responsavel_id" uuid,
  "recebimento_grade_id" uuid
);

CREATE TABLE erp."apolice_itens" (
  "id" uuid PRIMARY KEY NOT NULL,
  "apolice_id" uuid NOT NULL,
  "risk_type" text,
  "incluido_por_proposta_id" uuid,
  "excluido_por_proposta_id" uuid
);

CREATE TABLE erp."item_veiculo" (
  "apolice_item_id" uuid PRIMARY KEY NOT NULL
);

CREATE TABLE erp."item_imovel" (
  "apolice_item_id" uuid PRIMARY KEY NOT NULL
);

CREATE TABLE erp."item_empresa" (
  "apolice_item_id" uuid PRIMARY KEY NOT NULL
);

CREATE TABLE erp."item_vida" (
  "apolice_item_id" uuid PRIMARY KEY NOT NULL,
  "pessoa_id" uuid
);

CREATE TABLE erp."item_coberturas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "apolice_item_id" uuid NOT NULL,
  "cobertura_id" uuid,
  "incluido_por_proposta_id" uuid,
  "excluido_por_proposta_id" uuid
);

CREATE TABLE erp."sinistros" (
  "id" uuid PRIMARY KEY NOT NULL,
  "apolice_id" uuid NOT NULL,
  "stage_id" uuid NOT NULL,
  "responsavel_id" uuid
);

CREATE TABLE erp."sinistro_envolvidos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "sinistro_id" uuid NOT NULL,
  "apolice_item_id" uuid,
  "tipo" text
);

CREATE TABLE erp."pos_vendas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "apolice_id" uuid NOT NULL,
  "stage_id" uuid NOT NULL,
  "responsavel_id" uuid
);

CREATE TABLE erp."recebimento_grades" (
  "id" uuid PRIMARY KEY NOT NULL,
  "seguradora_id" uuid NOT NULL,
  "ramo_id" uuid NOT NULL
);

CREATE TABLE erp."recebimento_grade_parcelas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "grade_id" uuid NOT NULL
);

CREATE TABLE erp."repasse_regras" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid,
  "produtor_id" uuid,
  "ramo_id" uuid
);

CREATE TABLE erp."parcelas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "proposta_id" uuid NOT NULL
);

CREATE TABLE erp."financeiro_cobrancas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "parcela_id" uuid NOT NULL,
  "stage_id" uuid NOT NULL,
  "responsavel_id" uuid
);

CREATE TABLE erp."comissoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "proposta_id" uuid NOT NULL,
  "parcela_id" uuid
);

CREATE TABLE erp."repasses" (
  "id" uuid PRIMARY KEY NOT NULL,
  "proposta_id" uuid NOT NULL,
  "comissao_id" uuid,
  "beneficiario_id" uuid NOT NULL,
  "regra_id" uuid
);

CREATE TABLE erp."atividades" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid,
  "responsavel_id" uuid,
  "entidade_tipo" text,
  "entidade_id" uuid
);

CREATE TABLE erp."atividade_mencoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "atividade_id" uuid NOT NULL,
  "profile_id" uuid NOT NULL
);

CREATE TABLE erp."anexos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid,
  "entidade_tipo" text,
  "entidade_id" uuid
);

CREATE TABLE erp."audit_logs" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "user_id" uuid
);

CREATE TABLE erp."integracao_logs" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL
);

CREATE TABLE erp."campo_definicoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid,
  "entidade_tipo" text NOT NULL,
  "chave" text NOT NULL
);

CREATE TABLE erp."campo_opcoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "campo_definicao_id" uuid NOT NULL
);

CREATE TABLE erp."campo_valores" (
  "id" uuid PRIMARY KEY NOT NULL,
  "campo_definicao_id" uuid NOT NULL,
  "entidade_id" uuid NOT NULL,
  "valor_texto" text,
  "valor_numero" numeric,
  "valor_booleano" boolean,
  "valor_data" date,
  "valor_datahora" timestamp with time zone,
  "valor_opcao_id" uuid
);

CREATE TABLE erp."campo_valor_opcoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "campo_valor_id" uuid NOT NULL,
  "campo_opcao_id" uuid NOT NULL
);


ALTER TABLE erp.tenants
  ADD COLUMN nome text,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true;

ALTER TABLE erp.filiais
  ADD COLUMN nome text,
  ADD COLUMN cnpj text,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true;

ALTER TABLE erp.profiles
  ADD COLUMN nome text,
  ADD COLUMN email text,
  ADD COLUMN ativo boolean NOT NULL DEFAULT true;

ALTER TABLE erp.profile_filiais
  ADD COLUMN papel text NOT NULL DEFAULT 'operador',
  ADD COLUMN principal boolean NOT NULL DEFAULT false;

ALTER TABLE erp.role_permissions
  ADD COLUMN papel text NOT NULL,
  ADD COLUMN modulo text NOT NULL,
  ADD COLUMN pode_ler boolean NOT NULL DEFAULT false,
  ADD COLUMN pode_criar boolean NOT NULL DEFAULT false,
  ADD COLUMN pode_editar boolean NOT NULL DEFAULT false,
  ADD COLUMN pode_excluir boolean NOT NULL DEFAULT false;

CREATE UNIQUE INDEX ux_filiais_cnpj
ON erp.filiais (cnpj)
WHERE cnpj IS NOT NULL;

CREATE UNIQUE INDEX ux_profiles_tenant_email
ON erp.profiles (tenant_id, email)
WHERE email IS NOT NULL;

CREATE UNIQUE INDEX ux_role_permissions_papel_modulo
ON erp.role_permissions (papel, modulo);

ALTER TABLE erp."filiais" ADD CONSTRAINT "fk_filiais_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."filiais" ADD CONSTRAINT "fk_filiais_matriz_id" FOREIGN KEY ("matriz_id") REFERENCES erp."filiais" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_filiais_tenant_id" ON erp."filiais" ("tenant_id");

CREATE INDEX "ix_filiais_matriz_id" ON erp."filiais" ("matriz_id");

ALTER TABLE erp."profiles" ADD CONSTRAINT "fk_profiles_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_profiles_tenant_id" ON erp."profiles" ("tenant_id");

ALTER TABLE erp."profile_filiais" ADD CONSTRAINT "fk_profile_filiais_profile_id" FOREIGN KEY ("profile_id") REFERENCES erp."profiles" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."profile_filiais" ADD CONSTRAINT "fk_profile_filiais_filial_id" FOREIGN KEY ("filial_id") REFERENCES erp."filiais" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_profile_filiais_profile_id" ON erp."profile_filiais" ("profile_id");

CREATE INDEX "ix_profile_filiais_filial_id" ON erp."profile_filiais" ("filial_id");

ALTER TABLE erp."produtores" ADD CONSTRAINT "fk_produtores_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."produtores" ADD CONSTRAINT "fk_produtores_profile_id" FOREIGN KEY ("profile_id") REFERENCES erp."profiles" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_produtores_tenant_id" ON erp."produtores" ("tenant_id");

CREATE INDEX "ix_produtores_profile_id" ON erp."produtores" ("profile_id");

ALTER TABLE erp."segurados" ADD CONSTRAINT "fk_segurados_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."segurados" ADD CONSTRAINT "fk_segurados_filial_id" FOREIGN KEY ("filial_id") REFERENCES erp."filiais" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."segurados" ADD CONSTRAINT "fk_segurados_produtor_id" FOREIGN KEY ("produtor_id") REFERENCES erp."produtores" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."segurados" ADD CONSTRAINT "fk_segurados_gerente_id" FOREIGN KEY ("gerente_id") REFERENCES erp."produtores" ("id") ON DELETE RESTRICT;

CREATE UNIQUE INDEX "ux_segurados_filial_id_cpf_cnpj" ON erp."segurados" ("filial_id", "cpf_cnpj");

CREATE INDEX "ix_segurados_cpf_cnpj" ON erp."segurados" ("cpf_cnpj");

CREATE INDEX "ix_segurados_tenant_id" ON erp."segurados" ("tenant_id");

CREATE INDEX "ix_segurados_filial_id" ON erp."segurados" ("filial_id");

CREATE INDEX "ix_segurados_produtor_id" ON erp."segurados" ("produtor_id");

CREATE INDEX "ix_segurados_gerente_id" ON erp."segurados" ("gerente_id");

ALTER TABLE erp."pessoa_contato" ADD CONSTRAINT "fk_pessoa_contato_pj_id" FOREIGN KEY ("pj_id") REFERENCES erp."segurados" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."pessoa_contato" ADD CONSTRAINT "fk_pessoa_contato_pf_id" FOREIGN KEY ("pf_id") REFERENCES erp."segurados" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_pessoa_contato_pj_id" ON erp."pessoa_contato" ("pj_id");

CREATE INDEX "ix_pessoa_contato_pf_id" ON erp."pessoa_contato" ("pf_id");

ALTER TABLE erp."seguradoras" ADD CONSTRAINT "fk_seguradoras_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_seguradoras_tenant_id" ON erp."seguradoras" ("tenant_id");

ALTER TABLE erp."ramos" ADD CONSTRAINT "fk_ramos_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_ramos_tenant_id" ON erp."ramos" ("tenant_id");

ALTER TABLE erp."origens" ADD CONSTRAINT "fk_origens_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_origens_tenant_id" ON erp."origens" ("tenant_id");

ALTER TABLE erp."motivos_perda" ADD CONSTRAINT "fk_motivos_perda_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_motivos_perda_tenant_id" ON erp."motivos_perda" ("tenant_id");

ALTER TABLE erp."coberturas_catalogo" ADD CONSTRAINT "fk_coberturas_catalogo_ramo_id" FOREIGN KEY ("ramo_id") REFERENCES erp."ramos" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_coberturas_catalogo_ramo_id" ON erp."coberturas_catalogo" ("ramo_id");

ALTER TABLE erp."pipelines" ADD CONSTRAINT "fk_pipelines_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."pipelines" ADD CONSTRAINT "fk_pipelines_filial_id" FOREIGN KEY ("filial_id") REFERENCES erp."filiais" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_pipelines_tenant_id" ON erp."pipelines" ("tenant_id");

CREATE INDEX "ix_pipelines_filial_id" ON erp."pipelines" ("filial_id");

ALTER TABLE erp."pipeline_stages" ADD CONSTRAINT "fk_pipeline_stages_pipeline_id" FOREIGN KEY ("pipeline_id") REFERENCES erp."pipelines" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_pipeline_stages_pipeline_id" ON erp."pipeline_stages" ("pipeline_id");

ALTER TABLE erp."oportunidades" ADD CONSTRAINT "fk_oportunidades_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."oportunidades" ADD CONSTRAINT "fk_oportunidades_filial_id" FOREIGN KEY ("filial_id") REFERENCES erp."filiais" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."oportunidades" ADD CONSTRAINT "fk_oportunidades_segurado_id" FOREIGN KEY ("segurado_id") REFERENCES erp."segurados" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."oportunidades" ADD CONSTRAINT "fk_oportunidades_ramo_id" FOREIGN KEY ("ramo_id") REFERENCES erp."ramos" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."oportunidades" ADD CONSTRAINT "fk_oportunidades_origem_id" FOREIGN KEY ("origem_id") REFERENCES erp."origens" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."oportunidades" ADD CONSTRAINT "fk_oportunidades_apolice_origem_id" FOREIGN KEY ("apolice_origem_id") REFERENCES erp."apolices" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."oportunidades" ADD CONSTRAINT "fk_oportunidades_responsavel_id" FOREIGN KEY ("responsavel_id") REFERENCES erp."profiles" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."oportunidades" ADD CONSTRAINT "fk_oportunidades_stage_id" FOREIGN KEY ("stage_id") REFERENCES erp."pipeline_stages" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."oportunidades" ADD CONSTRAINT "fk_oportunidades_motivo_perda_id" FOREIGN KEY ("motivo_perda_id") REFERENCES erp."motivos_perda" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_oportunidades_tenant_id" ON erp."oportunidades" ("tenant_id");

CREATE INDEX "ix_oportunidades_filial_id" ON erp."oportunidades" ("filial_id");

CREATE INDEX "ix_oportunidades_segurado_id" ON erp."oportunidades" ("segurado_id");

CREATE INDEX "ix_oportunidades_ramo_id" ON erp."oportunidades" ("ramo_id");

CREATE INDEX "ix_oportunidades_origem_id" ON erp."oportunidades" ("origem_id");

CREATE INDEX "ix_oportunidades_apolice_origem_id" ON erp."oportunidades" ("apolice_origem_id");

CREATE INDEX "ix_oportunidades_responsavel_id" ON erp."oportunidades" ("responsavel_id");

CREATE INDEX "ix_oportunidades_stage_id" ON erp."oportunidades" ("stage_id");

CREATE INDEX "ix_oportunidades_motivo_perda_id" ON erp."oportunidades" ("motivo_perda_id");

ALTER TABLE erp."calculos" ADD CONSTRAINT "fk_calculos_oportunidade_id" FOREIGN KEY ("oportunidade_id") REFERENCES erp."oportunidades" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."calculos" ADD CONSTRAINT "fk_calculos_ramo_id" FOREIGN KEY ("ramo_id") REFERENCES erp."ramos" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."calculos" ADD CONSTRAINT "fk_calculos_segurado_id" FOREIGN KEY ("segurado_id") REFERENCES erp."segurados" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."calculos" ADD CONSTRAINT "fk_calculos_seguradora_anterior_id" FOREIGN KEY ("seguradora_anterior_id") REFERENCES erp."seguradoras" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_calculos_oportunidade_id" ON erp."calculos" ("oportunidade_id");

CREATE INDEX "ix_calculos_ramo_id" ON erp."calculos" ("ramo_id");

CREATE INDEX "ix_calculos_segurado_id" ON erp."calculos" ("segurado_id");

CREATE INDEX "ix_calculos_seguradora_anterior_id" ON erp."calculos" ("seguradora_anterior_id");

ALTER TABLE erp."calc_auto" ADD CONSTRAINT "fk_calc_auto_calculo_id" FOREIGN KEY ("calculo_id") REFERENCES erp."calculos" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."calc_residencia" ADD CONSTRAINT "fk_calc_residencia_calculo_id" FOREIGN KEY ("calculo_id") REFERENCES erp."calculos" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."calc_condominio" ADD CONSTRAINT "fk_calc_condominio_calculo_id" FOREIGN KEY ("calculo_id") REFERENCES erp."calculos" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."calc_vida" ADD CONSTRAINT "fk_calc_vida_calculo_id" FOREIGN KEY ("calculo_id") REFERENCES erp."calculos" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."calc_empresa" ADD CONSTRAINT "fk_calc_empresa_calculo_id" FOREIGN KEY ("calculo_id") REFERENCES erp."calculos" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."calc_diversos" ADD CONSTRAINT "fk_calc_diversos_calculo_id" FOREIGN KEY ("calculo_id") REFERENCES erp."calculos" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."calculo_coberturas" ADD CONSTRAINT "fk_calculo_coberturas_calculo_id" FOREIGN KEY ("calculo_id") REFERENCES erp."calculos" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."calculo_coberturas" ADD CONSTRAINT "fk_calculo_coberturas_cobertura_id" FOREIGN KEY ("cobertura_id") REFERENCES erp."coberturas_catalogo" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_calculo_coberturas_calculo_id" ON erp."calculo_coberturas" ("calculo_id");

CREATE INDEX "ix_calculo_coberturas_cobertura_id" ON erp."calculo_coberturas" ("cobertura_id");

ALTER TABLE erp."cotacoes" ADD CONSTRAINT "fk_cotacoes_calculo_id" FOREIGN KEY ("calculo_id") REFERENCES erp."calculos" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."cotacoes" ADD CONSTRAINT "fk_cotacoes_seguradora_id" FOREIGN KEY ("seguradora_id") REFERENCES erp."seguradoras" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_cotacoes_calculo_id" ON erp."cotacoes" ("calculo_id");

CREATE INDEX "ix_cotacoes_seguradora_id" ON erp."cotacoes" ("seguradora_id");

ALTER TABLE erp."apolices" ADD CONSTRAINT "fk_apolices_segurado_id" FOREIGN KEY ("segurado_id") REFERENCES erp."segurados" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."apolices" ADD CONSTRAINT "fk_apolices_seguradora_id" FOREIGN KEY ("seguradora_id") REFERENCES erp."seguradoras" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."apolices" ADD CONSTRAINT "fk_apolices_ramo_id" FOREIGN KEY ("ramo_id") REFERENCES erp."ramos" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."apolices" ADD CONSTRAINT "fk_apolices_renovada_de_id" FOREIGN KEY ("renovada_de_id") REFERENCES erp."apolices" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."apolices" ADD CONSTRAINT "fk_apolices_produtor_id" FOREIGN KEY ("produtor_id") REFERENCES erp."produtores" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_apolices_segurado_id" ON erp."apolices" ("segurado_id");

CREATE INDEX "ix_apolices_seguradora_id" ON erp."apolices" ("seguradora_id");

CREATE INDEX "ix_apolices_ramo_id" ON erp."apolices" ("ramo_id");

CREATE INDEX "ix_apolices_renovada_de_id" ON erp."apolices" ("renovada_de_id");

CREATE INDEX "ix_apolices_produtor_id" ON erp."apolices" ("produtor_id");

ALTER TABLE erp."propostas" ADD CONSTRAINT "fk_propostas_apolice_id" FOREIGN KEY ("apolice_id") REFERENCES erp."apolices" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."propostas" ADD CONSTRAINT "fk_propostas_cotacao_id" FOREIGN KEY ("cotacao_id") REFERENCES erp."cotacoes" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."propostas" ADD CONSTRAINT "fk_propostas_stage_id" FOREIGN KEY ("stage_id") REFERENCES erp."pipeline_stages" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."propostas" ADD CONSTRAINT "fk_propostas_responsavel_id" FOREIGN KEY ("responsavel_id") REFERENCES erp."profiles" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."propostas" ADD CONSTRAINT "fk_propostas_recebimento_grade_id" FOREIGN KEY ("recebimento_grade_id") REFERENCES erp."recebimento_grades" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_propostas_apolice_id" ON erp."propostas" ("apolice_id");

CREATE INDEX "ix_propostas_stage_id" ON erp."propostas" ("stage_id");

CREATE INDEX "ix_propostas_responsavel_id" ON erp."propostas" ("responsavel_id");

CREATE INDEX "ix_propostas_recebimento_grade_id" ON erp."propostas" ("recebimento_grade_id");

ALTER TABLE erp."apolice_itens" ADD CONSTRAINT "fk_apolice_itens_apolice_id" FOREIGN KEY ("apolice_id") REFERENCES erp."apolices" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."apolice_itens" ADD CONSTRAINT "fk_apolice_itens_incluido_por_proposta_id" FOREIGN KEY ("incluido_por_proposta_id") REFERENCES erp."propostas" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."apolice_itens" ADD CONSTRAINT "fk_apolice_itens_excluido_por_proposta_id" FOREIGN KEY ("excluido_por_proposta_id") REFERENCES erp."propostas" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_apolice_itens_apolice_id" ON erp."apolice_itens" ("apolice_id");

CREATE INDEX "ix_apolice_itens_incluido_por_proposta_id" ON erp."apolice_itens" ("incluido_por_proposta_id");

CREATE INDEX "ix_apolice_itens_excluido_por_proposta_id" ON erp."apolice_itens" ("excluido_por_proposta_id");

ALTER TABLE erp."item_veiculo" ADD CONSTRAINT "fk_item_veiculo_apolice_item_id" FOREIGN KEY ("apolice_item_id") REFERENCES erp."apolice_itens" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."item_imovel" ADD CONSTRAINT "fk_item_imovel_apolice_item_id" FOREIGN KEY ("apolice_item_id") REFERENCES erp."apolice_itens" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."item_empresa" ADD CONSTRAINT "fk_item_empresa_apolice_item_id" FOREIGN KEY ("apolice_item_id") REFERENCES erp."apolice_itens" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."item_vida" ADD CONSTRAINT "fk_item_vida_apolice_item_id" FOREIGN KEY ("apolice_item_id") REFERENCES erp."apolice_itens" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."item_vida" ADD CONSTRAINT "fk_item_vida_pessoa_id" FOREIGN KEY ("pessoa_id") REFERENCES erp."segurados" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_item_vida_pessoa_id" ON erp."item_vida" ("pessoa_id");

ALTER TABLE erp."item_coberturas" ADD CONSTRAINT "fk_item_coberturas_apolice_item_id" FOREIGN KEY ("apolice_item_id") REFERENCES erp."apolice_itens" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."item_coberturas" ADD CONSTRAINT "fk_item_coberturas_cobertura_id" FOREIGN KEY ("cobertura_id") REFERENCES erp."coberturas_catalogo" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."item_coberturas" ADD CONSTRAINT "fk_item_coberturas_incluido_por_proposta_id" FOREIGN KEY ("incluido_por_proposta_id") REFERENCES erp."propostas" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."item_coberturas" ADD CONSTRAINT "fk_item_coberturas_excluido_por_proposta_id" FOREIGN KEY ("excluido_por_proposta_id") REFERENCES erp."propostas" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_item_coberturas_apolice_item_id" ON erp."item_coberturas" ("apolice_item_id");

CREATE INDEX "ix_item_coberturas_cobertura_id" ON erp."item_coberturas" ("cobertura_id");

CREATE INDEX "ix_item_coberturas_incluido_por_proposta_id" ON erp."item_coberturas" ("incluido_por_proposta_id");

CREATE INDEX "ix_item_coberturas_excluido_por_proposta_id" ON erp."item_coberturas" ("excluido_por_proposta_id");

ALTER TABLE erp."sinistros" ADD CONSTRAINT "fk_sinistros_apolice_id" FOREIGN KEY ("apolice_id") REFERENCES erp."apolices" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."sinistros" ADD CONSTRAINT "fk_sinistros_stage_id" FOREIGN KEY ("stage_id") REFERENCES erp."pipeline_stages" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."sinistros" ADD CONSTRAINT "fk_sinistros_responsavel_id" FOREIGN KEY ("responsavel_id") REFERENCES erp."profiles" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_sinistros_apolice_id" ON erp."sinistros" ("apolice_id");

CREATE INDEX "ix_sinistros_stage_id" ON erp."sinistros" ("stage_id");

CREATE INDEX "ix_sinistros_responsavel_id" ON erp."sinistros" ("responsavel_id");

ALTER TABLE erp."sinistro_envolvidos" ADD CONSTRAINT "fk_sinistro_envolvidos_sinistro_id" FOREIGN KEY ("sinistro_id") REFERENCES erp."sinistros" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."sinistro_envolvidos" ADD CONSTRAINT "fk_sinistro_envolvidos_apolice_item_id" FOREIGN KEY ("apolice_item_id") REFERENCES erp."apolice_itens" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_sinistro_envolvidos_sinistro_id" ON erp."sinistro_envolvidos" ("sinistro_id");

CREATE INDEX "ix_sinistro_envolvidos_apolice_item_id" ON erp."sinistro_envolvidos" ("apolice_item_id");

ALTER TABLE erp."pos_vendas" ADD CONSTRAINT "fk_pos_vendas_apolice_id" FOREIGN KEY ("apolice_id") REFERENCES erp."apolices" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."pos_vendas" ADD CONSTRAINT "fk_pos_vendas_stage_id" FOREIGN KEY ("stage_id") REFERENCES erp."pipeline_stages" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."pos_vendas" ADD CONSTRAINT "fk_pos_vendas_responsavel_id" FOREIGN KEY ("responsavel_id") REFERENCES erp."profiles" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_pos_vendas_apolice_id" ON erp."pos_vendas" ("apolice_id");

CREATE INDEX "ix_pos_vendas_stage_id" ON erp."pos_vendas" ("stage_id");

CREATE INDEX "ix_pos_vendas_responsavel_id" ON erp."pos_vendas" ("responsavel_id");

ALTER TABLE erp."recebimento_grades" ADD CONSTRAINT "fk_recebimento_grades_seguradora_id" FOREIGN KEY ("seguradora_id") REFERENCES erp."seguradoras" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."recebimento_grades" ADD CONSTRAINT "fk_recebimento_grades_ramo_id" FOREIGN KEY ("ramo_id") REFERENCES erp."ramos" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_recebimento_grades_seguradora_id" ON erp."recebimento_grades" ("seguradora_id");

CREATE INDEX "ix_recebimento_grades_ramo_id" ON erp."recebimento_grades" ("ramo_id");

ALTER TABLE erp."recebimento_grade_parcelas" ADD CONSTRAINT "fk_recebimento_grade_parcelas_grade_id" FOREIGN KEY ("grade_id") REFERENCES erp."recebimento_grades" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_recebimento_grade_parcelas_grade_id" ON erp."recebimento_grade_parcelas" ("grade_id");

ALTER TABLE erp."repasse_regras" ADD CONSTRAINT "fk_repasse_regras_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."repasse_regras" ADD CONSTRAINT "fk_repasse_regras_filial_id" FOREIGN KEY ("filial_id") REFERENCES erp."filiais" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."repasse_regras" ADD CONSTRAINT "fk_repasse_regras_produtor_id" FOREIGN KEY ("produtor_id") REFERENCES erp."produtores" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."repasse_regras" ADD CONSTRAINT "fk_repasse_regras_ramo_id" FOREIGN KEY ("ramo_id") REFERENCES erp."ramos" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_repasse_regras_tenant_id" ON erp."repasse_regras" ("tenant_id");

CREATE INDEX "ix_repasse_regras_filial_id" ON erp."repasse_regras" ("filial_id");

CREATE INDEX "ix_repasse_regras_produtor_id" ON erp."repasse_regras" ("produtor_id");

CREATE INDEX "ix_repasse_regras_ramo_id" ON erp."repasse_regras" ("ramo_id");

ALTER TABLE erp."parcelas" ADD CONSTRAINT "fk_parcelas_proposta_id" FOREIGN KEY ("proposta_id") REFERENCES erp."propostas" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_parcelas_proposta_id" ON erp."parcelas" ("proposta_id");

ALTER TABLE erp."financeiro_cobrancas" ADD CONSTRAINT "fk_financeiro_cobrancas_parcela_id" FOREIGN KEY ("parcela_id") REFERENCES erp."parcelas" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."financeiro_cobrancas" ADD CONSTRAINT "fk_financeiro_cobrancas_stage_id" FOREIGN KEY ("stage_id") REFERENCES erp."pipeline_stages" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."financeiro_cobrancas" ADD CONSTRAINT "fk_financeiro_cobrancas_responsavel_id" FOREIGN KEY ("responsavel_id") REFERENCES erp."profiles" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_financeiro_cobrancas_parcela_id" ON erp."financeiro_cobrancas" ("parcela_id");

CREATE INDEX "ix_financeiro_cobrancas_stage_id" ON erp."financeiro_cobrancas" ("stage_id");

CREATE INDEX "ix_financeiro_cobrancas_responsavel_id" ON erp."financeiro_cobrancas" ("responsavel_id");

ALTER TABLE erp."comissoes" ADD CONSTRAINT "fk_comissoes_proposta_id" FOREIGN KEY ("proposta_id") REFERENCES erp."propostas" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."comissoes" ADD CONSTRAINT "fk_comissoes_parcela_id" FOREIGN KEY ("parcela_id") REFERENCES erp."parcelas" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_comissoes_proposta_id" ON erp."comissoes" ("proposta_id");

CREATE INDEX "ix_comissoes_parcela_id" ON erp."comissoes" ("parcela_id");

ALTER TABLE erp."repasses" ADD CONSTRAINT "fk_repasses_proposta_id" FOREIGN KEY ("proposta_id") REFERENCES erp."propostas" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."repasses" ADD CONSTRAINT "fk_repasses_comissao_id" FOREIGN KEY ("comissao_id") REFERENCES erp."comissoes" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."repasses" ADD CONSTRAINT "fk_repasses_beneficiario_id" FOREIGN KEY ("beneficiario_id") REFERENCES erp."produtores" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."repasses" ADD CONSTRAINT "fk_repasses_regra_id" FOREIGN KEY ("regra_id") REFERENCES erp."repasse_regras" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_repasses_proposta_id" ON erp."repasses" ("proposta_id");

CREATE INDEX "ix_repasses_comissao_id" ON erp."repasses" ("comissao_id");

CREATE INDEX "ix_repasses_beneficiario_id" ON erp."repasses" ("beneficiario_id");

CREATE INDEX "ix_repasses_regra_id" ON erp."repasses" ("regra_id");

ALTER TABLE erp."atividades" ADD CONSTRAINT "fk_atividades_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."atividades" ADD CONSTRAINT "fk_atividades_filial_id" FOREIGN KEY ("filial_id") REFERENCES erp."filiais" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."atividades" ADD CONSTRAINT "fk_atividades_responsavel_id" FOREIGN KEY ("responsavel_id") REFERENCES erp."profiles" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_atividades_tenant_id" ON erp."atividades" ("tenant_id");

CREATE INDEX "ix_atividades_filial_id" ON erp."atividades" ("filial_id");

CREATE INDEX "ix_atividades_responsavel_id" ON erp."atividades" ("responsavel_id");

ALTER TABLE erp."atividade_mencoes" ADD CONSTRAINT "fk_atividade_mencoes_atividade_id" FOREIGN KEY ("atividade_id") REFERENCES erp."atividades" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."atividade_mencoes" ADD CONSTRAINT "fk_atividade_mencoes_profile_id" FOREIGN KEY ("profile_id") REFERENCES erp."profiles" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_atividade_mencoes_atividade_id" ON erp."atividade_mencoes" ("atividade_id");

CREATE INDEX "ix_atividade_mencoes_profile_id" ON erp."atividade_mencoes" ("profile_id");

ALTER TABLE erp."anexos" ADD CONSTRAINT "fk_anexos_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."anexos" ADD CONSTRAINT "fk_anexos_filial_id" FOREIGN KEY ("filial_id") REFERENCES erp."filiais" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_anexos_tenant_id" ON erp."anexos" ("tenant_id");

CREATE INDEX "ix_anexos_filial_id" ON erp."anexos" ("filial_id");

ALTER TABLE erp."audit_logs" ADD CONSTRAINT "fk_audit_logs_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."audit_logs" ADD CONSTRAINT "fk_audit_logs_user_id" FOREIGN KEY ("user_id") REFERENCES erp."profiles" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_audit_logs_tenant_id" ON erp."audit_logs" ("tenant_id");

CREATE INDEX "ix_audit_logs_user_id" ON erp."audit_logs" ("user_id");

ALTER TABLE erp."integracao_logs" ADD CONSTRAINT "fk_integracao_logs_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_integracao_logs_tenant_id" ON erp."integracao_logs" ("tenant_id");

ALTER TABLE erp."campo_definicoes" ADD CONSTRAINT "fk_campo_definicoes_tenant_id" FOREIGN KEY ("tenant_id") REFERENCES erp."tenants" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."campo_definicoes" ADD CONSTRAINT "fk_campo_definicoes_filial_id" FOREIGN KEY ("filial_id") REFERENCES erp."filiais" ("id") ON DELETE RESTRICT;

CREATE UNIQUE INDEX "ux_campo_definicoes_tenant_id_entidade_tipo_chave" ON erp."campo_definicoes" ("tenant_id", "entidade_tipo", "chave");

CREATE INDEX "ix_campo_definicoes_tenant_id" ON erp."campo_definicoes" ("tenant_id");

CREATE INDEX "ix_campo_definicoes_filial_id" ON erp."campo_definicoes" ("filial_id");

ALTER TABLE erp."campo_opcoes" ADD CONSTRAINT "fk_campo_opcoes_campo_definicao_id" FOREIGN KEY ("campo_definicao_id") REFERENCES erp."campo_definicoes" ("id") ON DELETE RESTRICT;

CREATE INDEX "ix_campo_opcoes_campo_definicao_id" ON erp."campo_opcoes" ("campo_definicao_id");

ALTER TABLE erp."campo_valores" ADD CONSTRAINT "fk_campo_valores_campo_definicao_id" FOREIGN KEY ("campo_definicao_id") REFERENCES erp."campo_definicoes" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."campo_valores" ADD CONSTRAINT "fk_campo_valores_valor_opcao_id" FOREIGN KEY ("valor_opcao_id") REFERENCES erp."campo_opcoes" ("id") ON DELETE RESTRICT;

CREATE UNIQUE INDEX "ux_campo_valores_campo_definicao_id_entidade_id" ON erp."campo_valores" ("campo_definicao_id", "entidade_id");

CREATE INDEX "ix_campo_valores_entidade_id" ON erp."campo_valores" ("entidade_id");

CREATE INDEX "ix_campo_valores_campo_definicao_id" ON erp."campo_valores" ("campo_definicao_id");

CREATE INDEX "ix_campo_valores_valor_opcao_id" ON erp."campo_valores" ("valor_opcao_id");

ALTER TABLE erp."campo_valor_opcoes" ADD CONSTRAINT "fk_campo_valor_opcoes_campo_valor_id" FOREIGN KEY ("campo_valor_id") REFERENCES erp."campo_valores" ("id") ON DELETE RESTRICT;

ALTER TABLE erp."campo_valor_opcoes" ADD CONSTRAINT "fk_campo_valor_opcoes_campo_opcao_id" FOREIGN KEY ("campo_opcao_id") REFERENCES erp."campo_opcoes" ("id") ON DELETE RESTRICT;

CREATE UNIQUE INDEX "ux_campo_valor_opcoes_campo_valor_id_campo_opcao_id" ON erp."campo_valor_opcoes" ("campo_valor_id", "campo_opcao_id");

CREATE INDEX "ix_campo_valor_opcoes_campo_valor_id" ON erp."campo_valor_opcoes" ("campo_valor_id");

CREATE INDEX "ix_campo_valor_opcoes_campo_opcao_id" ON erp."campo_valor_opcoes" ("campo_opcao_id");


CREATE OR REPLACE FUNCTION erp.enforce_filial_tenant_match()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
  IF NEW.filial_id IS NOT NULL AND NOT EXISTS (
    SELECT 1 FROM erp.filiais f WHERE f.id = NEW.filial_id AND f.tenant_id = NEW.tenant_id
  ) THEN
    RAISE EXCEPTION 'filial % does not belong to tenant %', NEW.filial_id, NEW.tenant_id
      USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END;
$$;

CREATE TRIGGER "trg_segurados_filial_tenant" BEFORE INSERT OR UPDATE OF tenant_id, filial_id ON erp."segurados" FOR EACH ROW EXECUTE FUNCTION erp.enforce_filial_tenant_match();

CREATE TRIGGER "trg_pipelines_filial_tenant" BEFORE INSERT OR UPDATE OF tenant_id, filial_id ON erp."pipelines" FOR EACH ROW EXECUTE FUNCTION erp.enforce_filial_tenant_match();

CREATE TRIGGER "trg_oportunidades_filial_tenant" BEFORE INSERT OR UPDATE OF tenant_id, filial_id ON erp."oportunidades" FOR EACH ROW EXECUTE FUNCTION erp.enforce_filial_tenant_match();

CREATE TRIGGER "trg_repasse_regras_filial_tenant" BEFORE INSERT OR UPDATE OF tenant_id, filial_id ON erp."repasse_regras" FOR EACH ROW EXECUTE FUNCTION erp.enforce_filial_tenant_match();

CREATE TRIGGER "trg_atividades_filial_tenant" BEFORE INSERT OR UPDATE OF tenant_id, filial_id ON erp."atividades" FOR EACH ROW EXECUTE FUNCTION erp.enforce_filial_tenant_match();

CREATE TRIGGER "trg_anexos_filial_tenant" BEFORE INSERT OR UPDATE OF tenant_id, filial_id ON erp."anexos" FOR EACH ROW EXECUTE FUNCTION erp.enforce_filial_tenant_match();

CREATE TRIGGER "trg_campo_definicoes_filial_tenant" BEFORE INSERT OR UPDATE OF tenant_id, filial_id ON erp."campo_definicoes" FOR EACH ROW EXECUTE FUNCTION erp.enforce_filial_tenant_match();


CREATE OR REPLACE FUNCTION erp.enforce_profile_filial_tenant_match()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM erp.profiles p
    JOIN erp.filiais f ON f.id = NEW.filial_id AND f.tenant_id = p.tenant_id
    WHERE p.id = NEW.profile_id
  ) THEN
    RAISE EXCEPTION 'profile % and filial % must belong to the same tenant', NEW.profile_id, NEW.filial_id
      USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END;
$$;

CREATE TRIGGER trg_profile_filiais_tenant
BEFORE INSERT OR UPDATE OF profile_id, filial_id ON erp.profile_filiais
FOR EACH ROW EXECUTE FUNCTION erp.enforce_profile_filial_tenant_match();

CREATE UNIQUE INDEX ux_profile_filiais_profile_filial
ON erp.profile_filiais (profile_id, filial_id);

CREATE TABLE erp.schema_contract_versions (
  version text PRIMARY KEY,
  applied_at timestamp with time zone NOT NULL DEFAULT now(),
  source_file text NOT NULL
);

INSERT INTO erp.schema_contract_versions(version, source_file)
VALUES ('1.0.0', 'wassis_erp_esqueleto_v1_0.dbml');
