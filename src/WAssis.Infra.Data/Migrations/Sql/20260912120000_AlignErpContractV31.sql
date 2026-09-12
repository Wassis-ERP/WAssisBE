-- Additive upgrade generated from docs/database/wassis_erp_esqueleto_v3_1.dbml.

-- Existing columns are preserved so this migration is safe for databases already using the v1/v1.1 transition schema.

-- New tables receive the DBML primary-key and not-null constraints; destructive renames and drops require a later data migration.

CREATE SCHEMA IF NOT EXISTS erp;

COMMENT ON SCHEMA erp IS 'W.Assis ERP canonical contract v3.1.0';

CREATE TABLE IF NOT EXISTS erp."tenants" (
  "id" uuid PRIMARY KEY NOT NULL,
  "razao_social" text,
  "nome_fantasia" text,
  "cnpj_cpf" text,
  "slug" text,
  "email" text,
  "telefone" text,
  "celular" text,
  "home_page" text,
  "cep" text,
  "endereco" text,
  "numero" text,
  "complemento" text,
  "bairro" text,
  "cidade" text,
  "uf" text,
  "pais" text,
  "timezone" text,
  "moeda_padrao" text,
  "status" text,
  "ativo" boolean,
  "criado_em" timestamp with time zone,
  "atualizado_em" timestamp with time zone
);

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "razao_social" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "nome_fantasia" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "cnpj_cpf" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "slug" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "email" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "telefone" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "celular" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "home_page" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "cep" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "endereco" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "numero" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "complemento" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "bairro" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "cidade" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "uf" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "pais" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "timezone" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "moeda_padrao" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "ativo" boolean;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "criado_em" timestamp with time zone;

ALTER TABLE erp."tenants" ADD COLUMN IF NOT EXISTS "atualizado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."filiais" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "matriz_id" uuid,
  "razao_social" text,
  "fantasia" text,
  "cnpj_cpf" text,
  "susep" text,
  "percentual_imposto" numeric,
  "lgpd_aceito" boolean,
  "lgpd_aceito_em" timestamp with time zone,
  "gerente" text,
  "gerente_id" uuid,
  "contato" text,
  "home_page" text,
  "email" text,
  "telefone" text,
  "celular" text,
  "telefone2" text,
  "inscricao_estadual" text,
  "inscricao_municipal" text,
  "regime_tributario" text,
  "percentual_iss" numeric,
  "codigo_corretora" text,
  "codigo_externo" text,
  "municipio_ibge" text,
  "pais" text,
  "horario_atendimento" text,
  "observacoes" text,
  "cep" text,
  "endereco" text,
  "numero" text,
  "complemento" text,
  "bairro" text,
  "cidade" text,
  "uf" text,
  "ativo" boolean
);

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "matriz_id" uuid;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "razao_social" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "fantasia" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "cnpj_cpf" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "susep" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "percentual_imposto" numeric;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "lgpd_aceito" boolean;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "lgpd_aceito_em" timestamp with time zone;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "gerente" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "gerente_id" uuid;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "contato" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "home_page" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "email" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "telefone" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "celular" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "telefone2" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "inscricao_estadual" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "inscricao_municipal" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "regime_tributario" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "percentual_iss" numeric;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "codigo_corretora" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "codigo_externo" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "municipio_ibge" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "pais" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "horario_atendimento" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "observacoes" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "cep" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "endereco" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "numero" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "complemento" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "bairro" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "cidade" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "uf" text;

ALTER TABLE erp."filiais" ADD COLUMN IF NOT EXISTS "ativo" boolean;

CREATE TABLE IF NOT EXISTS erp."profiles" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "nome_completo" text,
  "email" text,
  "telefone" text,
  "celular" text,
  "cargo" text,
  "departamento" text,
  "avatar_url" text,
  "status" text,
  "ativo" boolean,
  "ultimo_acesso_em" timestamp with time zone,
  "convite_status" text,
  "convite_enviado_em" timestamp with time zone
);

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "nome_completo" text;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "email" text;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "telefone" text;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "celular" text;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "cargo" text;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "departamento" text;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "avatar_url" text;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "ativo" boolean;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "ultimo_acesso_em" timestamp with time zone;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "convite_status" text;

ALTER TABLE erp."profiles" ADD COLUMN IF NOT EXISTS "convite_enviado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."perfis" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "nome" text,
  "descricao" text,
  "sistema" boolean,
  "nivel_acesso" text,
  "ordem" integer,
  "ativo" boolean
);

ALTER TABLE erp."perfis" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."perfis" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."perfis" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."perfis" ADD COLUMN IF NOT EXISTS "descricao" text;

ALTER TABLE erp."perfis" ADD COLUMN IF NOT EXISTS "sistema" boolean;

ALTER TABLE erp."perfis" ADD COLUMN IF NOT EXISTS "nivel_acesso" text;

ALTER TABLE erp."perfis" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."perfis" ADD COLUMN IF NOT EXISTS "ativo" boolean;

CREATE TABLE IF NOT EXISTS erp."profile_filiais" (
  "id" uuid PRIMARY KEY NOT NULL,
  "profile_id" uuid NOT NULL,
  "filial_id" uuid NOT NULL,
  "perfil_id" uuid NOT NULL,
  "principal" boolean,
  "ativo" boolean,
  "data_inicio" date,
  "data_fim" date
);

ALTER TABLE erp."profile_filiais" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."profile_filiais" ADD COLUMN IF NOT EXISTS "profile_id" uuid;

ALTER TABLE erp."profile_filiais" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."profile_filiais" ADD COLUMN IF NOT EXISTS "perfil_id" uuid;

ALTER TABLE erp."profile_filiais" ADD COLUMN IF NOT EXISTS "principal" boolean;

ALTER TABLE erp."profile_filiais" ADD COLUMN IF NOT EXISTS "ativo" boolean;

ALTER TABLE erp."profile_filiais" ADD COLUMN IF NOT EXISTS "data_inicio" date;

ALTER TABLE erp."profile_filiais" ADD COLUMN IF NOT EXISTS "data_fim" date;

CREATE TABLE IF NOT EXISTS erp."role_permissions" (
  "id" uuid PRIMARY KEY NOT NULL,
  "perfil_id" uuid NOT NULL,
  "modulo" text,
  "escopo" text,
  "can_read" boolean,
  "can_create" boolean,
  "can_update" boolean,
  "can_delete" boolean,
  "can_export" boolean,
  "can_manage" boolean
);

ALTER TABLE erp."role_permissions" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."role_permissions" ADD COLUMN IF NOT EXISTS "perfil_id" uuid;

ALTER TABLE erp."role_permissions" ADD COLUMN IF NOT EXISTS "modulo" text;

ALTER TABLE erp."role_permissions" ADD COLUMN IF NOT EXISTS "escopo" text;

ALTER TABLE erp."role_permissions" ADD COLUMN IF NOT EXISTS "can_read" boolean;

ALTER TABLE erp."role_permissions" ADD COLUMN IF NOT EXISTS "can_create" boolean;

ALTER TABLE erp."role_permissions" ADD COLUMN IF NOT EXISTS "can_update" boolean;

ALTER TABLE erp."role_permissions" ADD COLUMN IF NOT EXISTS "can_delete" boolean;

ALTER TABLE erp."role_permissions" ADD COLUMN IF NOT EXISTS "can_export" boolean;

ALTER TABLE erp."role_permissions" ADD COLUMN IF NOT EXISTS "can_manage" boolean;

CREATE TABLE IF NOT EXISTS erp."produtores" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "profile_id" uuid,
  "nome" text,
  "cpf_cnpj" text,
  "tipo_pessoa" text,
  "nome_fantasia" text,
  "rg_ie" text,
  "susep" text,
  "categoria_operacional" text,
  "data_nascimento" date,
  "email" text,
  "telefone" text,
  "celular" text,
  "telefone2" text,
  "cep" text,
  "endereco" text,
  "numero" text,
  "complemento" text,
  "bairro" text,
  "cidade" text,
  "uf" text,
  "pais" text,
  "banco" text,
  "agencia" text,
  "conta" text,
  "tipo_conta" text,
  "chave_pix" text,
  "favorecido_nome" text,
  "favorecido_cpf_cnpj" text,
  "descontar_imposto" boolean,
  "percentual_imposto" numeric,
  "percentual_repasse_padrao" numeric,
  "observacoes" text,
  "ativo" boolean
);

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "profile_id" uuid;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "cpf_cnpj" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "tipo_pessoa" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "nome_fantasia" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "rg_ie" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "susep" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "categoria_operacional" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "data_nascimento" date;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "email" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "telefone" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "celular" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "telefone2" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "cep" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "endereco" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "numero" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "complemento" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "bairro" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "cidade" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "uf" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "pais" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "banco" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "agencia" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "conta" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "tipo_conta" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "chave_pix" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "favorecido_nome" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "favorecido_cpf_cnpj" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "descontar_imposto" boolean;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "percentual_imposto" numeric;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "percentual_repasse_padrao" numeric;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "observacoes" text;

ALTER TABLE erp."produtores" ADD COLUMN IF NOT EXISTS "ativo" boolean;

CREATE TABLE IF NOT EXISTS erp."segurados" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid NOT NULL,
  "produtor_id" uuid,
  "gerente_id" uuid,
  "cpf_cnpj" text,
  "tipo" text,
  "nome" text,
  "nome_fantasia" text,
  "status" text,
  "lgpd_autorizado" boolean,
  "email" text,
  "telefone" text,
  "chatwoot_id" text,
  "cep" text,
  "logradouro" text,
  "endereco" text,
  "numero" text,
  "complemento" text,
  "bairro" text,
  "cidade" text,
  "estado" text,
  "data_nascimento" date,
  "sexo" text,
  "estado_civil" text,
  "cnae" text,
  "porte" text,
  "site" text,
  "observacoes" text,
  "created_at" timestamp with time zone,
  "updated_at" timestamp with time zone,
  "nome_social" text,
  "rg_ie" text,
  "inscricao_municipal" text,
  "atividade_economica" text,
  "profissao" text,
  "renda_mensal" numeric,
  "cnh_numero" text,
  "cnh_categoria" text,
  "cnh_vencimento" date,
  "celular" text,
  "telefone2" text,
  "whatsapp" text,
  "pais" text,
  "lgpd_autorizado_em" timestamp with time zone,
  "origem_importacao" text
);

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "produtor_id" uuid;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "gerente_id" uuid;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "cpf_cnpj" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "tipo" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "nome_fantasia" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "lgpd_autorizado" boolean;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "email" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "telefone" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "chatwoot_id" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "cep" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "logradouro" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "endereco" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "numero" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "complemento" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "bairro" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "cidade" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "estado" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "data_nascimento" date;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "sexo" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "estado_civil" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "cnae" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "porte" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "site" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "observacoes" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "created_at" timestamp with time zone;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "updated_at" timestamp with time zone;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "nome_social" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "rg_ie" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "inscricao_municipal" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "atividade_economica" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "profissao" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "renda_mensal" numeric;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "cnh_numero" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "cnh_categoria" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "cnh_vencimento" date;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "celular" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "telefone2" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "whatsapp" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "pais" text;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "lgpd_autorizado_em" timestamp with time zone;

ALTER TABLE erp."segurados" ADD COLUMN IF NOT EXISTS "origem_importacao" text;

CREATE TABLE IF NOT EXISTS erp."pessoa_contato" (
  "id" uuid PRIMARY KEY NOT NULL,
  "pj_id" uuid NOT NULL,
  "pf_id" uuid,
  "nome" text,
  "cargo" text,
  "departamento" text,
  "email" text,
  "telefone" text,
  "celular" text,
  "principal" boolean,
  "ativo" boolean,
  "observacoes" text
);

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "pj_id" uuid;

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "pf_id" uuid;

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "cargo" text;

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "departamento" text;

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "email" text;

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "telefone" text;

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "celular" text;

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "principal" boolean;

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "ativo" boolean;

ALTER TABLE erp."pessoa_contato" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."seguradoras" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "nome" text,
  "nome_curto" text,
  "cnpj" text,
  "codigo_susep" text,
  "codigo_interno" text,
  "site" text,
  "portal_url" text,
  "telefone_sac" text,
  "telefone_assistencia" text,
  "email" text,
  "aceita_importacao_pdf" boolean,
  "aceita_busca_automatica" boolean,
  "ativo" boolean,
  "observacoes" text
);

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "nome_curto" text;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "cnpj" text;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "codigo_susep" text;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "codigo_interno" text;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "site" text;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "portal_url" text;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "telefone_sac" text;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "telefone_assistencia" text;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "email" text;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "aceita_importacao_pdf" boolean;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "aceita_busca_automatica" boolean;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "ativo" boolean;

ALTER TABLE erp."seguradoras" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."ramos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "nome" text,
  "codigo_susep" text,
  "risk_type" text,
  "grupo_operacional" text,
  "forma_calculo" text,
  "is_monthly" boolean,
  "renovavel" boolean,
  "permite_endosso" boolean,
  "exige_item" boolean,
  "exige_coberturas" boolean,
  "ordem" integer,
  "ativo" boolean,
  "observacoes" text
);

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "codigo_susep" text;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "risk_type" text;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "grupo_operacional" text;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "forma_calculo" text;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "is_monthly" boolean;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "renovavel" boolean;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "permite_endosso" boolean;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "exige_item" boolean;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "exige_coberturas" boolean;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "ativo" boolean;

ALTER TABLE erp."ramos" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."endosso_subtipos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid,
  "ramo_id" uuid,
  "nome" text,
  "natureza_canonica" text,
  "ordem" integer,
  "ativo" boolean,
  "observacoes" text
);

ALTER TABLE erp."endosso_subtipos" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."endosso_subtipos" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."endosso_subtipos" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."endosso_subtipos" ADD COLUMN IF NOT EXISTS "ramo_id" uuid;

ALTER TABLE erp."endosso_subtipos" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."endosso_subtipos" ADD COLUMN IF NOT EXISTS "natureza_canonica" text;

ALTER TABLE erp."endosso_subtipos" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."endosso_subtipos" ADD COLUMN IF NOT EXISTS "ativo" boolean;

ALTER TABLE erp."endosso_subtipos" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."cancelamento_motivos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid,
  "ramo_id" uuid,
  "nome" text,
  "ordem" integer,
  "ativo" boolean,
  "observacoes" text
);

ALTER TABLE erp."cancelamento_motivos" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."cancelamento_motivos" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."cancelamento_motivos" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."cancelamento_motivos" ADD COLUMN IF NOT EXISTS "ramo_id" uuid;

ALTER TABLE erp."cancelamento_motivos" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."cancelamento_motivos" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."cancelamento_motivos" ADD COLUMN IF NOT EXISTS "ativo" boolean;

ALTER TABLE erp."cancelamento_motivos" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."origens" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "nome" text,
  "tipo" text,
  "ordem" integer,
  "ativo" boolean
);

ALTER TABLE erp."origens" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."origens" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."origens" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."origens" ADD COLUMN IF NOT EXISTS "tipo" text;

ALTER TABLE erp."origens" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."origens" ADD COLUMN IF NOT EXISTS "ativo" boolean;

CREATE TABLE IF NOT EXISTS erp."motivos_perda" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "nome" text,
  "categoria" text,
  "ordem" integer,
  "ativo" boolean
);

ALTER TABLE erp."motivos_perda" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."motivos_perda" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."motivos_perda" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."motivos_perda" ADD COLUMN IF NOT EXISTS "categoria" text;

ALTER TABLE erp."motivos_perda" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."motivos_perda" ADD COLUMN IF NOT EXISTS "ativo" boolean;

CREATE TABLE IF NOT EXISTS erp."coberturas_catalogo" (
  "id" uuid PRIMARY KEY NOT NULL,
  "ramo_id" uuid NOT NULL,
  "codigo" text,
  "codigo_susep" text,
  "nome" text,
  "descricao" text,
  "tipo_cobertura" text,
  "caracteristica" text,
  "tipo_risco" text,
  "modalidade" text,
  "capital_lmi_padrao" numeric,
  "franquia_padrao" numeric,
  "carencia_dias" integer,
  "obrigatoria" boolean,
  "ordem" integer,
  "ativo" boolean
);

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "ramo_id" uuid;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "codigo" text;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "codigo_susep" text;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "descricao" text;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "tipo_cobertura" text;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "caracteristica" text;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "tipo_risco" text;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "modalidade" text;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "capital_lmi_padrao" numeric;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "franquia_padrao" numeric;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "carencia_dias" integer;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "obrigatoria" boolean;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."coberturas_catalogo" ADD COLUMN IF NOT EXISTS "ativo" boolean;

CREATE TABLE IF NOT EXISTS erp."pipelines" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid,
  "nome" text,
  "entidade_tipo" text,
  "descricao" text,
  "modelo_fabrica" boolean,
  "permite_customizacao" boolean,
  "ordem" integer,
  "ativo" boolean
);

ALTER TABLE erp."pipelines" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."pipelines" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."pipelines" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."pipelines" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."pipelines" ADD COLUMN IF NOT EXISTS "entidade_tipo" text;

ALTER TABLE erp."pipelines" ADD COLUMN IF NOT EXISTS "descricao" text;

ALTER TABLE erp."pipelines" ADD COLUMN IF NOT EXISTS "modelo_fabrica" boolean;

ALTER TABLE erp."pipelines" ADD COLUMN IF NOT EXISTS "permite_customizacao" boolean;

ALTER TABLE erp."pipelines" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."pipelines" ADD COLUMN IF NOT EXISTS "ativo" boolean;

CREATE TABLE IF NOT EXISTS erp."pipeline_stages" (
  "id" uuid PRIMARY KEY NOT NULL,
  "pipeline_id" uuid NOT NULL,
  "nome" text,
  "codigo" text,
  "tipo_stage" text,
  "cor" text,
  "ordem" integer,
  "probabilidade" numeric,
  "sla_dias" integer,
  "finaliza_com_sucesso" boolean,
  "finaliza_com_perda" boolean,
  "ativo" boolean
);

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "pipeline_id" uuid;

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "codigo" text;

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "tipo_stage" text;

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "cor" text;

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "probabilidade" numeric;

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "sla_dias" integer;

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "finaliza_com_sucesso" boolean;

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "finaliza_com_perda" boolean;

ALTER TABLE erp."pipeline_stages" ADD COLUMN IF NOT EXISTS "ativo" boolean;

CREATE TABLE IF NOT EXISTS erp."oportunidades" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid NOT NULL,
  "segurado_id" uuid,
  "ramo_id" uuid,
  "origem_id" uuid,
  "apolice_origem_id" uuid,
  "responsavel_id" uuid,
  "stage_id" uuid NOT NULL,
  "motivo_perda_id" uuid,
  "lead_nome" text,
  "lead_documento" text,
  "lead_email" text,
  "lead_telefone" text,
  "titulo" text,
  "descricao" text,
  "prioridade" text,
  "valor_premio_estimado" numeric,
  "valor_comissao_estimada" numeric,
  "comissao_estimada_pct" numeric,
  "agenciamento_pct" numeric,
  "data_abertura" date,
  "data_fechamento_prevista" date,
  "ganha_em" timestamp with time zone,
  "perdida_em" timestamp with time zone,
  "motivo_perda_observacao" text,
  "campanha" text,
  "observacoes" text
);

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "segurado_id" uuid;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "ramo_id" uuid;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "origem_id" uuid;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "apolice_origem_id" uuid;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "responsavel_id" uuid;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "stage_id" uuid;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "motivo_perda_id" uuid;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "lead_nome" text;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "lead_documento" text;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "lead_email" text;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "lead_telefone" text;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "titulo" text;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "descricao" text;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "prioridade" text;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "valor_premio_estimado" numeric;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "valor_comissao_estimada" numeric;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "comissao_estimada_pct" numeric;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "agenciamento_pct" numeric;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "data_abertura" date;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "data_fechamento_prevista" date;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "ganha_em" timestamp with time zone;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "perdida_em" timestamp with time zone;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "motivo_perda_observacao" text;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "campanha" text;

ALTER TABLE erp."oportunidades" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."calculos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "oportunidade_id" uuid NOT NULL,
  "ramo_id" uuid NOT NULL,
  "segurado_id" uuid,
  "seguradora_anterior_id" uuid,
  "origem" text,
  "comissao_sugerida_pct" numeric,
  "rotulo_versao" text,
  "tipo_seguro" text,
  "bonus" integer,
  "qtd_sinistros" integer,
  "qtd_sinistros_perda_parcial" integer,
  "transferiu_titularidade" boolean,
  "vigencia_inicio" date,
  "vigencia_fim" date,
  "vigencia_fim_anterior" date,
  "numero_apolice_anterior" text,
  "codigo_identificacao_anterior" text,
  "status_apolice_anterior" text,
  "nota_interna" text,
  "criado_em" timestamp with time zone
);

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "oportunidade_id" uuid;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "ramo_id" uuid;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "segurado_id" uuid;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "seguradora_anterior_id" uuid;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "origem" text;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "comissao_sugerida_pct" numeric;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "rotulo_versao" text;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "tipo_seguro" text;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "bonus" integer;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "qtd_sinistros" integer;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "qtd_sinistros_perda_parcial" integer;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "transferiu_titularidade" boolean;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "vigencia_inicio" date;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "vigencia_fim" date;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "vigencia_fim_anterior" date;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "numero_apolice_anterior" text;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "codigo_identificacao_anterior" text;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "status_apolice_anterior" text;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "nota_interna" text;

ALTER TABLE erp."calculos" ADD COLUMN IF NOT EXISTS "criado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."calc_auto" (
  "calculo_id" uuid PRIMARY KEY NOT NULL,
  "codigo_fipe" text,
  "marca" text,
  "modelo" text,
  "versao" text,
  "ano_fabricacao" integer,
  "ano_modelo" integer,
  "placa" text,
  "chassi" text,
  "chassi_remarcado" boolean,
  "renavam" text,
  "zero_km" boolean,
  "combustivel" text,
  "cambio" text,
  "categoria" text,
  "tipo_veiculo" text,
  "uso" text,
  "cep_pernoite" text,
  "possui_garagem_residencia" boolean,
  "possui_garagem_trabalho" boolean,
  "possui_garagem_estudo" boolean,
  "km_mensal" integer,
  "blindado" boolean,
  "alienado" boolean,
  "rastreador" boolean,
  "antifurto" boolean,
  "kit_gas" boolean,
  "condutor_nome" text,
  "condutor_cpf" text,
  "condutor_data_nascimento" date,
  "condutor_sexo" text,
  "condutor_estado_civil" text,
  "condutor_profissao" text,
  "condutor_reside_com_segurado" boolean,
  "condutor_tempo_habilitacao" integer
);

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "calculo_id" uuid;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "codigo_fipe" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "marca" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "modelo" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "versao" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "ano_fabricacao" integer;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "ano_modelo" integer;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "placa" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "chassi" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "chassi_remarcado" boolean;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "renavam" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "zero_km" boolean;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "combustivel" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "cambio" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "categoria" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "tipo_veiculo" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "uso" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "cep_pernoite" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "possui_garagem_residencia" boolean;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "possui_garagem_trabalho" boolean;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "possui_garagem_estudo" boolean;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "km_mensal" integer;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "blindado" boolean;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "alienado" boolean;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "rastreador" boolean;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "antifurto" boolean;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "kit_gas" boolean;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "condutor_nome" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "condutor_cpf" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "condutor_data_nascimento" date;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "condutor_sexo" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "condutor_estado_civil" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "condutor_profissao" text;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "condutor_reside_com_segurado" boolean;

ALTER TABLE erp."calc_auto" ADD COLUMN IF NOT EXISTS "condutor_tempo_habilitacao" integer;

CREATE TABLE IF NOT EXISTS erp."calc_residencia" (
  "calculo_id" uuid PRIMARY KEY NOT NULL,
  "cep" text,
  "endereco" text,
  "numero" text,
  "complemento" text,
  "bairro" text,
  "cidade" text,
  "uf" text,
  "tipo_imovel" text,
  "tipo_ocupacao" text,
  "tipo_construcao" text,
  "area_m2" numeric,
  "valor_imovel" numeric,
  "proprietario" boolean,
  "desocupado" boolean,
  "condominio_fechado" boolean,
  "area_de_risco" boolean,
  "possui_alarme" boolean,
  "possui_monitoramento" boolean,
  "possui_portao_eletronico" boolean
);

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "calculo_id" uuid;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "cep" text;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "endereco" text;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "numero" text;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "complemento" text;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "bairro" text;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "cidade" text;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "uf" text;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "tipo_imovel" text;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "tipo_ocupacao" text;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "tipo_construcao" text;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "area_m2" numeric;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "valor_imovel" numeric;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "proprietario" boolean;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "desocupado" boolean;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "condominio_fechado" boolean;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "area_de_risco" boolean;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "possui_alarme" boolean;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "possui_monitoramento" boolean;

ALTER TABLE erp."calc_residencia" ADD COLUMN IF NOT EXISTS "possui_portao_eletronico" boolean;

CREATE TABLE IF NOT EXISTS erp."calc_condominio" (
  "calculo_id" uuid PRIMARY KEY NOT NULL,
  "nome_condominio" text,
  "cep" text,
  "endereco" text,
  "numero" text,
  "complemento" text,
  "bairro" text,
  "cidade" text,
  "uf" text,
  "tipo_condominio" text,
  "area_total_m2" numeric,
  "qtd_blocos" integer,
  "qtd_pavimentos" integer,
  "qtd_unidades" integer,
  "qtd_elevadores" integer,
  "possui_portaria_24h" boolean,
  "possui_sprinklers" boolean,
  "possui_extintores" boolean,
  "possui_para_raios" boolean,
  "possui_garagem" boolean
);

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "calculo_id" uuid;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "nome_condominio" text;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "cep" text;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "endereco" text;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "numero" text;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "complemento" text;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "bairro" text;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "cidade" text;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "uf" text;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "tipo_condominio" text;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "area_total_m2" numeric;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "qtd_blocos" integer;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "qtd_pavimentos" integer;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "qtd_unidades" integer;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "qtd_elevadores" integer;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "possui_portaria_24h" boolean;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "possui_sprinklers" boolean;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "possui_extintores" boolean;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "possui_para_raios" boolean;

ALTER TABLE erp."calc_condominio" ADD COLUMN IF NOT EXISTS "possui_garagem" boolean;

CREATE TABLE IF NOT EXISTS erp."calc_vida" (
  "calculo_id" uuid PRIMARY KEY NOT NULL,
  "sexo" text,
  "data_nascimento" date,
  "altura_cm" integer,
  "peso_kg" numeric,
  "renda_mensal" numeric,
  "profissao" text,
  "fumante" boolean,
  "pratica_esporte_risco" boolean,
  "possui_doenca_preexistente" boolean,
  "usa_medicamento_continuo" boolean,
  "capital_desejado" numeric,
  "beneficiarios_texto" text
);

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "calculo_id" uuid;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "sexo" text;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "data_nascimento" date;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "altura_cm" integer;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "peso_kg" numeric;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "renda_mensal" numeric;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "profissao" text;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "fumante" boolean;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "pratica_esporte_risco" boolean;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "possui_doenca_preexistente" boolean;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "usa_medicamento_continuo" boolean;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "capital_desejado" numeric;

ALTER TABLE erp."calc_vida" ADD COLUMN IF NOT EXISTS "beneficiarios_texto" text;

CREATE TABLE IF NOT EXISTS erp."calc_empresa" (
  "calculo_id" uuid PRIMARY KEY NOT NULL,
  "cnpj" text,
  "razao_social" text,
  "atividade" text,
  "cnae" text,
  "faturamento_anual" numeric,
  "cep" text,
  "endereco" text,
  "numero" text,
  "complemento" text,
  "bairro" text,
  "cidade" text,
  "uf" text,
  "tipo_construcao" text,
  "area_m2" numeric,
  "qtd_funcionarios" integer,
  "possui_extintores" boolean,
  "possui_alarme" boolean,
  "possui_sprinklers" boolean,
  "possui_inflamaveis" boolean,
  "valor_estoque" numeric,
  "valor_equipamentos" numeric
);

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "calculo_id" uuid;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "cnpj" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "razao_social" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "atividade" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "cnae" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "faturamento_anual" numeric;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "cep" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "endereco" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "numero" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "complemento" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "bairro" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "cidade" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "uf" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "tipo_construcao" text;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "area_m2" numeric;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "qtd_funcionarios" integer;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "possui_extintores" boolean;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "possui_alarme" boolean;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "possui_sprinklers" boolean;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "possui_inflamaveis" boolean;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "valor_estoque" numeric;

ALTER TABLE erp."calc_empresa" ADD COLUMN IF NOT EXISTS "valor_equipamentos" numeric;

CREATE TABLE IF NOT EXISTS erp."calc_diversos" (
  "calculo_id" uuid PRIMARY KEY NOT NULL,
  "categoria" text,
  "descricao_risco" text,
  "valor_declarado" numeric,
  "observacoes" text
);

ALTER TABLE erp."calc_diversos" ADD COLUMN IF NOT EXISTS "calculo_id" uuid;

ALTER TABLE erp."calc_diversos" ADD COLUMN IF NOT EXISTS "categoria" text;

ALTER TABLE erp."calc_diversos" ADD COLUMN IF NOT EXISTS "descricao_risco" text;

ALTER TABLE erp."calc_diversos" ADD COLUMN IF NOT EXISTS "valor_declarado" numeric;

ALTER TABLE erp."calc_diversos" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."calculo_coberturas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "calculo_id" uuid NOT NULL,
  "cobertura_id" uuid NOT NULL,
  "selecionada" boolean,
  "limite_solicitado" numeric,
  "percentual_fipe_solicitado" numeric,
  "franquia_tipo_solicitado" text,
  "opcao_solicitada" text,
  "quantidade_solicitada" integer,
  "observacao_transmitida" text
);

ALTER TABLE erp."calculo_coberturas" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."calculo_coberturas" ADD COLUMN IF NOT EXISTS "calculo_id" uuid;

ALTER TABLE erp."calculo_coberturas" ADD COLUMN IF NOT EXISTS "cobertura_id" uuid;

ALTER TABLE erp."calculo_coberturas" ADD COLUMN IF NOT EXISTS "selecionada" boolean;

ALTER TABLE erp."calculo_coberturas" ADD COLUMN IF NOT EXISTS "limite_solicitado" numeric;

ALTER TABLE erp."calculo_coberturas" ADD COLUMN IF NOT EXISTS "percentual_fipe_solicitado" numeric;

ALTER TABLE erp."calculo_coberturas" ADD COLUMN IF NOT EXISTS "franquia_tipo_solicitado" text;

ALTER TABLE erp."calculo_coberturas" ADD COLUMN IF NOT EXISTS "opcao_solicitada" text;

ALTER TABLE erp."calculo_coberturas" ADD COLUMN IF NOT EXISTS "quantidade_solicitada" integer;

ALTER TABLE erp."calculo_coberturas" ADD COLUMN IF NOT EXISTS "observacao_transmitida" text;

CREATE TABLE IF NOT EXISTS erp."calculo_execucoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "calculo_id" uuid NOT NULL,
  "seguradora_id" uuid NOT NULL,
  "reexecucao_de_id" uuid,
  "tentativa" integer NOT NULL,
  "motor" text NOT NULL,
  "comissao_pct_aplicada" numeric,
  "comissao_origem" text NOT NULL,
  "status" text NOT NULL,
  "pendencia_codigo" text,
  "pendencia_mensagem" text,
  "erro_codigo" text,
  "erro_mensagem_segura" text,
  "referencia_externa" text,
  "iniciada_em" timestamp with time zone,
  "concluida_em" timestamp with time zone,
  "criada_em" timestamp with time zone NOT NULL
);

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "calculo_id" uuid;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "seguradora_id" uuid;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "reexecucao_de_id" uuid;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "tentativa" integer;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "motor" text;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "comissao_pct_aplicada" numeric;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "comissao_origem" text;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "pendencia_codigo" text;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "pendencia_mensagem" text;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "erro_codigo" text;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "erro_mensagem_segura" text;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "referencia_externa" text;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "iniciada_em" timestamp with time zone;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "concluida_em" timestamp with time zone;

ALTER TABLE erp."calculo_execucoes" ADD COLUMN IF NOT EXISTS "criada_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."cotacoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "execucao_id" uuid NOT NULL,
  "numero_cotacao_seguradora" text,
  "premio_total" numeric,
  "premio_liquido" numeric,
  "iof" numeric,
  "adicional_fracionamento" numeric,
  "comissao_valor" numeric,
  "validade" date,
  "status" text,
  "link_proposta" text,
  "mensagem_seguradora" text,
  "restricoes" text,
  "recebida_em" timestamp with time zone,
  "aprovada_em" timestamp with time zone,
  "descartada_motivo" text,
  "observacao_interna" text
);

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "execucao_id" uuid;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "numero_cotacao_seguradora" text;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "premio_total" numeric;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "premio_liquido" numeric;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "iof" numeric;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "adicional_fracionamento" numeric;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "comissao_valor" numeric;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "validade" date;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "link_proposta" text;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "mensagem_seguradora" text;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "restricoes" text;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "recebida_em" timestamp with time zone;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "aprovada_em" timestamp with time zone;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "descartada_motivo" text;

ALTER TABLE erp."cotacoes" ADD COLUMN IF NOT EXISTS "observacao_interna" text;

CREATE TABLE IF NOT EXISTS erp."cotacao_coberturas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "cotacao_id" uuid NOT NULL,
  "cobertura_id" uuid,
  "chave_resultado" text NOT NULL,
  "codigo_externo" text,
  "nome_informado" text,
  "incluida" boolean,
  "limite_aceito" numeric,
  "percentual_fipe_aceito" numeric,
  "franquia_tipo" text,
  "franquia_valor" numeric,
  "premio" numeric,
  "carencia_dias" integer,
  "participacao_obrigatoria_pct" numeric,
  "clausula_texto" text,
  "observacao_seguradora" text,
  "ordem" integer
);

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "cotacao_id" uuid;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "cobertura_id" uuid;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "chave_resultado" text;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "codigo_externo" text;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "nome_informado" text;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "incluida" boolean;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "limite_aceito" numeric;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "percentual_fipe_aceito" numeric;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "franquia_tipo" text;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "franquia_valor" numeric;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "premio" numeric;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "carencia_dias" integer;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "participacao_obrigatoria_pct" numeric;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "clausula_texto" text;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "observacao_seguradora" text;

ALTER TABLE erp."cotacao_coberturas" ADD COLUMN IF NOT EXISTS "ordem" integer;

CREATE TABLE IF NOT EXISTS erp."cotacao_parcelamentos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "cotacao_id" uuid NOT NULL,
  "codigo_opcao" text NOT NULL,
  "forma_pagamento" text NOT NULL,
  "quantidade_parcelas" integer NOT NULL,
  "valor_entrada" numeric,
  "valor_parcela" numeric,
  "valor_total" numeric,
  "juros_pct" numeric,
  "adicional_fracionamento" numeric,
  "primeiro_vencimento" date,
  "principal" boolean,
  "ordem" integer
);

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "cotacao_id" uuid;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "codigo_opcao" text;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "forma_pagamento" text;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "quantidade_parcelas" integer;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "valor_entrada" numeric;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "valor_parcela" numeric;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "valor_total" numeric;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "juros_pct" numeric;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "adicional_fracionamento" numeric;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "primeiro_vencimento" date;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "principal" boolean;

ALTER TABLE erp."cotacao_parcelamentos" ADD COLUMN IF NOT EXISTS "ordem" integer;

CREATE TABLE IF NOT EXISTS erp."apresentacoes_comerciais" (
  "id" uuid PRIMARY KEY NOT NULL,
  "oportunidade_id" uuid NOT NULL,
  "criado_por_id" uuid,
  "cotacao_escolhida_id" uuid,
  "titulo" text,
  "layout" text,
  "criterio_ordenacao" text,
  "exibir_percentual_fipe" boolean,
  "exibir_vantagens" boolean,
  "exibir_legenda" boolean,
  "exibir_observacoes" boolean,
  "exibir_comissao" boolean,
  "observacoes_comerciais" text,
  "status" text,
  "criado_em" timestamp with time zone,
  "atualizado_em" timestamp with time zone,
  "gerada_em" timestamp with time zone,
  "escolhida_em" timestamp with time zone
);

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "oportunidade_id" uuid;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "criado_por_id" uuid;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "cotacao_escolhida_id" uuid;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "titulo" text;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "layout" text;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "criterio_ordenacao" text;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "exibir_percentual_fipe" boolean;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "exibir_vantagens" boolean;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "exibir_legenda" boolean;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "exibir_observacoes" boolean;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "exibir_comissao" boolean;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "observacoes_comerciais" text;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "criado_em" timestamp with time zone;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "atualizado_em" timestamp with time zone;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "gerada_em" timestamp with time zone;

ALTER TABLE erp."apresentacoes_comerciais" ADD COLUMN IF NOT EXISTS "escolhida_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."apresentacao_cotacoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "apresentacao_id" uuid NOT NULL,
  "cotacao_id" uuid NOT NULL,
  "parcelamento_id" uuid,
  "ordem" integer NOT NULL,
  "recomendada" boolean,
  "titulo_comercial" text,
  "vantagens_texto" text,
  "observacao_comercial" text
);

ALTER TABLE erp."apresentacao_cotacoes" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."apresentacao_cotacoes" ADD COLUMN IF NOT EXISTS "apresentacao_id" uuid;

ALTER TABLE erp."apresentacao_cotacoes" ADD COLUMN IF NOT EXISTS "cotacao_id" uuid;

ALTER TABLE erp."apresentacao_cotacoes" ADD COLUMN IF NOT EXISTS "parcelamento_id" uuid;

ALTER TABLE erp."apresentacao_cotacoes" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."apresentacao_cotacoes" ADD COLUMN IF NOT EXISTS "recomendada" boolean;

ALTER TABLE erp."apresentacao_cotacoes" ADD COLUMN IF NOT EXISTS "titulo_comercial" text;

ALTER TABLE erp."apresentacao_cotacoes" ADD COLUMN IF NOT EXISTS "vantagens_texto" text;

ALTER TABLE erp."apresentacao_cotacoes" ADD COLUMN IF NOT EXISTS "observacao_comercial" text;

CREATE TABLE IF NOT EXISTS erp."apolices" (
  "id" uuid PRIMARY KEY NOT NULL,
  "segurado_id" uuid NOT NULL,
  "seguradora_id" uuid,
  "ramo_id" uuid,
  "status" text,
  "renovada_de_id" uuid,
  "produtor_id" uuid,
  "numero_apolice" text,
  "numero_controle_documento" text,
  "tipo_contratacao" text,
  "tipo_apolice" text,
  "certificado_individual" text,
  "processo_susep" text,
  "estipulante_nome" text,
  "estipulante_cpf_cnpj" text,
  "subestipulante_nome" text,
  "subestipulante_cpf_cnpj" text,
  "vigencia_inicio" date,
  "vigencia_fim" date,
  "vigencia_inicio_hora" text,
  "vigencia_fim_hora" text,
  "data_emissao" date,
  "data_recebimento_documento" date,
  "premio_total" numeric,
  "premio_liquido" numeric,
  "iof" numeric,
  "adicional_fracionamento" numeric,
  "lmg_total" numeric,
  "moeda" text,
  "periodicidade_pagamento" text,
  "motivo_status" text,
  "canal_emissao" text,
  "observacoes" text
);

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "segurado_id" uuid;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "seguradora_id" uuid;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "ramo_id" uuid;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "renovada_de_id" uuid;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "produtor_id" uuid;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "numero_apolice" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "numero_controle_documento" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "tipo_contratacao" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "tipo_apolice" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "certificado_individual" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "processo_susep" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "estipulante_nome" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "estipulante_cpf_cnpj" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "subestipulante_nome" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "subestipulante_cpf_cnpj" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "vigencia_inicio" date;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "vigencia_fim" date;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "vigencia_inicio_hora" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "vigencia_fim_hora" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "data_emissao" date;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "data_recebimento_documento" date;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "premio_total" numeric;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "premio_liquido" numeric;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "iof" numeric;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "adicional_fracionamento" numeric;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "lmg_total" numeric;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "moeda" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "periodicidade_pagamento" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "motivo_status" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "canal_emissao" text;

ALTER TABLE erp."apolices" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."propostas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "apolice_id" uuid NOT NULL,
  "tipo" text,
  "cotacao_id" uuid,
  "stage_id" uuid NOT NULL,
  "responsavel_id" uuid,
  "recebimento_grade_id" uuid,
  "endosso_subtipo_id" uuid,
  "cancelamento_motivo_id" uuid,
  "numero_proposta" text,
  "numero_endosso" text,
  "numero_controle_documento" text,
  "protocolo_seguradora" text,
  "tipo_movimento_endosso" text,
  "data_transmissao" date,
  "data_recebimento_seguradora" date,
  "data_aceitacao" date,
  "data_recusa" date,
  "motivo_recusa" text,
  "data_emissao" date,
  "vigencia_inicio" date,
  "vigencia_fim" date,
  "premio_total" numeric,
  "premio_liquido" numeric,
  "iof" numeric,
  "adicional_fracionamento" numeric,
  "forma_pagamento" text,
  "periodicidade_pagamento" text,
  "qtd_parcelas" integer,
  "primeira_parcela_vencimento" date,
  "primeira_parcela_valor" numeric,
  "comissao_pct" numeric,
  "agenciamento_pct" numeric,
  "numero_fatura" text,
  "competencia_inicio" date,
  "competencia_fim" date,
  "observacoes" text
);

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "apolice_id" uuid;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "tipo" text;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "cotacao_id" uuid;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "stage_id" uuid;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "responsavel_id" uuid;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "recebimento_grade_id" uuid;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "endosso_subtipo_id" uuid;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "cancelamento_motivo_id" uuid;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "numero_proposta" text;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "numero_endosso" text;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "numero_controle_documento" text;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "protocolo_seguradora" text;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "tipo_movimento_endosso" text;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "data_transmissao" date;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "data_recebimento_seguradora" date;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "data_aceitacao" date;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "data_recusa" date;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "motivo_recusa" text;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "data_emissao" date;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "vigencia_inicio" date;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "vigencia_fim" date;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "premio_total" numeric;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "premio_liquido" numeric;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "iof" numeric;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "adicional_fracionamento" numeric;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "forma_pagamento" text;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "periodicidade_pagamento" text;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "qtd_parcelas" integer;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "primeira_parcela_vencimento" date;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "primeira_parcela_valor" numeric;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "comissao_pct" numeric;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "agenciamento_pct" numeric;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "numero_fatura" text;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "competencia_inicio" date;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "competencia_fim" date;

ALTER TABLE erp."propostas" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."apolice_itens" (
  "id" uuid PRIMARY KEY NOT NULL,
  "apolice_id" uuid NOT NULL,
  "risk_type" text,
  "incluido_por_proposta_id" uuid,
  "excluido_por_proposta_id" uuid,
  "numero_item" integer,
  "descricao" text,
  "identificador_externo" text,
  "valor_risco" numeric,
  "endereco_risco_resumo" text,
  "status" text,
  "observacoes" text
);

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "apolice_id" uuid;

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "risk_type" text;

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "incluido_por_proposta_id" uuid;

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "excluido_por_proposta_id" uuid;

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "numero_item" integer;

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "descricao" text;

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "identificador_externo" text;

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "valor_risco" numeric;

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "endereco_risco_resumo" text;

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."apolice_itens" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."item_veiculo" (
  "apolice_item_id" uuid PRIMARY KEY NOT NULL,
  "codigo_fipe" text,
  "marca" text,
  "modelo" text,
  "versao" text,
  "ano_fabricacao" integer,
  "ano_modelo" integer,
  "placa" text,
  "chassi" text,
  "renavam" text,
  "zero_km" boolean,
  "combustivel" text,
  "cambio" text,
  "categoria" text,
  "uso" text,
  "cep_pernoite" text,
  "classe_bonus" integer,
  "blindado" boolean,
  "alienado" boolean,
  "rastreador" boolean,
  "antifurto" boolean,
  "kit_gas" boolean,
  "condutor_principal_nome" text,
  "condutor_principal_cpf" text,
  "condutor_principal_data_nascimento" date
);

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "apolice_item_id" uuid;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "codigo_fipe" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "marca" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "modelo" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "versao" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "ano_fabricacao" integer;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "ano_modelo" integer;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "placa" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "chassi" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "renavam" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "zero_km" boolean;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "combustivel" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "cambio" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "categoria" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "uso" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "cep_pernoite" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "classe_bonus" integer;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "blindado" boolean;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "alienado" boolean;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "rastreador" boolean;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "antifurto" boolean;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "kit_gas" boolean;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "condutor_principal_nome" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "condutor_principal_cpf" text;

ALTER TABLE erp."item_veiculo" ADD COLUMN IF NOT EXISTS "condutor_principal_data_nascimento" date;

CREATE TABLE IF NOT EXISTS erp."item_imovel" (
  "apolice_item_id" uuid PRIMARY KEY NOT NULL,
  "cep" text,
  "endereco" text,
  "numero" text,
  "complemento" text,
  "bairro" text,
  "cidade" text,
  "uf" text,
  "tipo_imovel" text,
  "tipo_ocupacao" text,
  "tipo_construcao" text,
  "area_m2" numeric,
  "valor_imovel" numeric,
  "condominio_fechado" boolean,
  "desocupado" boolean
);

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "apolice_item_id" uuid;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "cep" text;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "endereco" text;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "numero" text;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "complemento" text;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "bairro" text;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "cidade" text;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "uf" text;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "tipo_imovel" text;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "tipo_ocupacao" text;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "tipo_construcao" text;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "area_m2" numeric;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "valor_imovel" numeric;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "condominio_fechado" boolean;

ALTER TABLE erp."item_imovel" ADD COLUMN IF NOT EXISTS "desocupado" boolean;

CREATE TABLE IF NOT EXISTS erp."item_empresa" (
  "apolice_item_id" uuid PRIMARY KEY NOT NULL,
  "cnpj_risco" text,
  "razao_social_risco" text,
  "atividade" text,
  "cnae" text,
  "faturamento_anual" numeric,
  "cep" text,
  "endereco" text,
  "numero" text,
  "complemento" text,
  "bairro" text,
  "cidade" text,
  "uf" text,
  "tipo_construcao" text,
  "area_m2" numeric,
  "qtd_funcionarios" integer,
  "valor_estoque" numeric,
  "valor_equipamentos" numeric,
  "protecao_incendio" text
);

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "apolice_item_id" uuid;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "cnpj_risco" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "razao_social_risco" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "atividade" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "cnae" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "faturamento_anual" numeric;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "cep" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "endereco" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "numero" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "complemento" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "bairro" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "cidade" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "uf" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "tipo_construcao" text;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "area_m2" numeric;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "qtd_funcionarios" integer;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "valor_estoque" numeric;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "valor_equipamentos" numeric;

ALTER TABLE erp."item_empresa" ADD COLUMN IF NOT EXISTS "protecao_incendio" text;

CREATE TABLE IF NOT EXISTS erp."item_vida" (
  "apolice_item_id" uuid PRIMARY KEY NOT NULL,
  "pessoa_id" uuid,
  "nome_grupo" text,
  "n_vidas" integer,
  "certificado_individual" text,
  "parentesco" text,
  "data_nascimento" date,
  "sexo" text,
  "profissao" text,
  "salario" numeric,
  "capital_individual" numeric,
  "data_inclusao" date,
  "data_exclusao" date,
  "beneficiarios_texto" text
);

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "apolice_item_id" uuid;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "pessoa_id" uuid;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "nome_grupo" text;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "n_vidas" integer;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "certificado_individual" text;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "parentesco" text;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "data_nascimento" date;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "sexo" text;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "profissao" text;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "salario" numeric;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "capital_individual" numeric;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "data_inclusao" date;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "data_exclusao" date;

ALTER TABLE erp."item_vida" ADD COLUMN IF NOT EXISTS "beneficiarios_texto" text;

CREATE TABLE IF NOT EXISTS erp."item_coberturas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "apolice_item_id" uuid NOT NULL,
  "cobertura_id" uuid,
  "incluido_por_proposta_id" uuid,
  "excluido_por_proposta_id" uuid,
  "capital_lmi" numeric,
  "franquia_valor" numeric,
  "franquia_tipo" text,
  "premio" numeric,
  "premio_liquido" numeric,
  "carencia_dias" integer,
  "participacao_obrigatoria_pct" numeric,
  "vigencia_inicio" date,
  "vigencia_fim" date,
  "observacoes" text
);

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "apolice_item_id" uuid;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "cobertura_id" uuid;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "incluido_por_proposta_id" uuid;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "excluido_por_proposta_id" uuid;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "capital_lmi" numeric;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "franquia_valor" numeric;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "franquia_tipo" text;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "premio" numeric;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "premio_liquido" numeric;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "carencia_dias" integer;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "participacao_obrigatoria_pct" numeric;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "vigencia_inicio" date;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "vigencia_fim" date;

ALTER TABLE erp."item_coberturas" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."sinistros" (
  "id" uuid PRIMARY KEY NOT NULL,
  "apolice_id" uuid NOT NULL,
  "stage_id" uuid NOT NULL,
  "responsavel_id" uuid,
  "numero_sinistro" text,
  "numero_aviso" text,
  "protocolo_seguradora" text,
  "cobertura_codigo" text,
  "cobertura_nome" text,
  "data_ocorrencia" date,
  "data_aviso" date,
  "data_registro_aviso" date,
  "data_documentacao_completa" date,
  "data_liquidacao_financeira" date,
  "data_conclusao" date,
  "tipo_sinistro" text,
  "causa" text,
  "descricao" text,
  "local_ocorrencia" text,
  "status" text,
  "valor_estimado" numeric,
  "valor_indenizado" numeric,
  "valor_pendente" numeric,
  "valor_despesas_regulacao" numeric,
  "valor_salvado" numeric,
  "data_salvado" date,
  "valor_ressarcimento" numeric,
  "data_ressarcimento" date,
  "negativa_motivo" text,
  "regulador_nome" text,
  "oficina_nome" text,
  "observacoes" text
);

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "apolice_id" uuid;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "stage_id" uuid;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "responsavel_id" uuid;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "numero_sinistro" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "numero_aviso" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "protocolo_seguradora" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "cobertura_codigo" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "cobertura_nome" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "data_ocorrencia" date;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "data_aviso" date;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "data_registro_aviso" date;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "data_documentacao_completa" date;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "data_liquidacao_financeira" date;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "data_conclusao" date;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "tipo_sinistro" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "causa" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "descricao" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "local_ocorrencia" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "valor_estimado" numeric;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "valor_indenizado" numeric;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "valor_pendente" numeric;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "valor_despesas_regulacao" numeric;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "valor_salvado" numeric;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "data_salvado" date;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "valor_ressarcimento" numeric;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "data_ressarcimento" date;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "negativa_motivo" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "regulador_nome" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "oficina_nome" text;

ALTER TABLE erp."sinistros" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."sinistro_envolvidos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "sinistro_id" uuid NOT NULL,
  "apolice_item_id" uuid,
  "tipo" text,
  "nome" text,
  "cpf_cnpj" text,
  "email" text,
  "telefone" text,
  "placa" text,
  "seguradora_terceiro" text,
  "apolice_terceiro" text,
  "tipo_dano" text,
  "valor_reclamado" numeric,
  "valor_indenizado" numeric,
  "responsavel_pelo_evento" boolean,
  "observacoes" text
);

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "sinistro_id" uuid;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "apolice_item_id" uuid;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "tipo" text;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "cpf_cnpj" text;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "email" text;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "telefone" text;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "placa" text;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "seguradora_terceiro" text;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "apolice_terceiro" text;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "tipo_dano" text;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "valor_reclamado" numeric;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "valor_indenizado" numeric;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "responsavel_pelo_evento" boolean;

ALTER TABLE erp."sinistro_envolvidos" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."pos_vendas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "apolice_id" uuid NOT NULL,
  "stage_id" uuid NOT NULL,
  "responsavel_id" uuid,
  "tipo_processo" text,
  "status" text,
  "prioridade" text,
  "assunto" text,
  "descricao" text,
  "data_abertura" date,
  "data_conclusao_prevista" date,
  "data_conclusao" date,
  "motivo_pendencia" text,
  "resultado" text,
  "observacoes" text
);

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "apolice_id" uuid;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "stage_id" uuid;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "responsavel_id" uuid;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "tipo_processo" text;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "prioridade" text;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "assunto" text;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "descricao" text;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "data_abertura" date;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "data_conclusao_prevista" date;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "data_conclusao" date;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "motivo_pendencia" text;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "resultado" text;

ALTER TABLE erp."pos_vendas" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."recebimento_grades" (
  "id" uuid PRIMARY KEY NOT NULL,
  "seguradora_id" uuid NOT NULL,
  "ramo_id" uuid NOT NULL,
  "nome" text,
  "tipo" text,
  "qtd_parcelas" integer,
  "base_calculo" text,
  "percentual_default" numeric,
  "considera_iof" boolean,
  "considera_adicional_fracionamento" boolean,
  "vitalicio" boolean,
  "ativo" boolean,
  "observacoes" text
);

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "seguradora_id" uuid;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "ramo_id" uuid;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "tipo" text;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "qtd_parcelas" integer;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "base_calculo" text;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "percentual_default" numeric;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "considera_iof" boolean;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "considera_adicional_fracionamento" boolean;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "vitalicio" boolean;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "ativo" boolean;

ALTER TABLE erp."recebimento_grades" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."recebimento_grade_parcelas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "grade_id" uuid NOT NULL,
  "numero" integer,
  "tipo_comissao" text,
  "percentual" numeric,
  "percentual_sobre" text,
  "dias_apos_vencimento" integer,
  "ativo" boolean
);

ALTER TABLE erp."recebimento_grade_parcelas" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."recebimento_grade_parcelas" ADD COLUMN IF NOT EXISTS "grade_id" uuid;

ALTER TABLE erp."recebimento_grade_parcelas" ADD COLUMN IF NOT EXISTS "numero" integer;

ALTER TABLE erp."recebimento_grade_parcelas" ADD COLUMN IF NOT EXISTS "tipo_comissao" text;

ALTER TABLE erp."recebimento_grade_parcelas" ADD COLUMN IF NOT EXISTS "percentual" numeric;

ALTER TABLE erp."recebimento_grade_parcelas" ADD COLUMN IF NOT EXISTS "percentual_sobre" text;

ALTER TABLE erp."recebimento_grade_parcelas" ADD COLUMN IF NOT EXISTS "dias_apos_vencimento" integer;

ALTER TABLE erp."recebimento_grade_parcelas" ADD COLUMN IF NOT EXISTS "ativo" boolean;

CREATE TABLE IF NOT EXISTS erp."repasse_regras" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid,
  "produtor_id" uuid,
  "ramo_id" uuid,
  "papel" text,
  "tipo_documento" text,
  "base" text,
  "percentual" numeric,
  "valor_fixo" numeric,
  "gatilho" text,
  "qtd_parcelas" integer,
  "limite_parcelas" integer,
  "prioridade" integer,
  "inicio_vigencia" date,
  "fim_vigencia" date,
  "ativo" boolean,
  "observacoes" text
);

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "produtor_id" uuid;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "ramo_id" uuid;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "papel" text;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "tipo_documento" text;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "base" text;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "percentual" numeric;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "valor_fixo" numeric;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "gatilho" text;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "qtd_parcelas" integer;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "limite_parcelas" integer;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "prioridade" integer;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "inicio_vigencia" date;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "fim_vigencia" date;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "ativo" boolean;

ALTER TABLE erp."repasse_regras" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."parcelas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "proposta_id" uuid NOT NULL,
  "numero" integer,
  "vencimento" date,
  "valor" numeric,
  "valor_liquido" numeric,
  "iof" numeric,
  "adicional_fracionamento" numeric,
  "status" text,
  "forma_pagamento" text,
  "nosso_numero" text,
  "linha_digitavel" text,
  "codigo_barras" text,
  "data_pagamento" date,
  "valor_pago" numeric,
  "data_baixa" date,
  "numero_fatura" text,
  "competencia_inicio" date,
  "competencia_fim" date,
  "observacoes" text
);

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "proposta_id" uuid;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "numero" integer;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "vencimento" date;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "valor" numeric;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "valor_liquido" numeric;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "iof" numeric;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "adicional_fracionamento" numeric;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "forma_pagamento" text;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "nosso_numero" text;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "linha_digitavel" text;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "codigo_barras" text;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "data_pagamento" date;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "valor_pago" numeric;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "data_baixa" date;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "numero_fatura" text;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "competencia_inicio" date;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "competencia_fim" date;

ALTER TABLE erp."parcelas" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."financeiro_cobrancas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "parcela_id" uuid NOT NULL,
  "stage_id" uuid NOT NULL,
  "responsavel_id" uuid,
  "data_abertura" date,
  "vencimento_followup" date,
  "status" text,
  "prioridade" text,
  "ultima_cobranca_em" timestamp with time zone,
  "proxima_cobranca_em" timestamp with time zone,
  "canal_preferencial" text,
  "observacoes" text,
  "encerrada_em" timestamp with time zone,
  "motivo_encerramento" text
);

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "parcela_id" uuid;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "stage_id" uuid;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "responsavel_id" uuid;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "data_abertura" date;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "vencimento_followup" date;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "prioridade" text;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "ultima_cobranca_em" timestamp with time zone;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "proxima_cobranca_em" timestamp with time zone;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "canal_preferencial" text;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "observacoes" text;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "encerrada_em" timestamp with time zone;

ALTER TABLE erp."financeiro_cobrancas" ADD COLUMN IF NOT EXISTS "motivo_encerramento" text;

CREATE TABLE IF NOT EXISTS erp."comissoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "proposta_id" uuid NOT NULL,
  "parcela_id" uuid,
  "numero" integer,
  "tipo_comissao" text,
  "percentual" numeric,
  "base_calculo" numeric,
  "valor_previsto" numeric,
  "valor_recebido" numeric,
  "valor_diferenca" numeric,
  "status" text,
  "prevista_em" date,
  "recebida_em" date,
  "competencia_inicio" date,
  "competencia_fim" date,
  "observacoes" text
);

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "proposta_id" uuid;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "parcela_id" uuid;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "numero" integer;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "tipo_comissao" text;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "percentual" numeric;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "base_calculo" numeric;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "valor_previsto" numeric;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "valor_recebido" numeric;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "valor_diferenca" numeric;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "prevista_em" date;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "recebida_em" date;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "competencia_inicio" date;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "competencia_fim" date;

ALTER TABLE erp."comissoes" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."comissao_extratos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid NOT NULL,
  "seguradora_id" uuid NOT NULL,
  "identificacao_externa" text,
  "competencia" date,
  "periodo_inicio" date,
  "periodo_fim" date,
  "data_emissao" date,
  "data_recebimento" date,
  "arquivo_nome" text,
  "arquivo_referencia" text,
  "origem_tipo" text NOT NULL,
  "origem_formato" text,
  "arquivo_mime_type" text,
  "arquivo_hash_sha256" text,
  "chave_idempotencia" text NOT NULL,
  "parser_identificador" text,
  "parser_versao" text,
  "tentativa_processamento" integer NOT NULL,
  "status_processamento" text NOT NULL,
  "status_conciliacao" text NOT NULL,
  "quantidade_itens" integer,
  "valor_bruto_total" numeric,
  "valor_liquido_total" numeric,
  "valor_descontos_total" numeric,
  "moeda" text,
  "erro_codigo" text,
  "erro_mensagem_segura" text,
  "recebido_por_id" uuid,
  "processado_por_id" uuid,
  "recebido_em" timestamp with time zone,
  "processamento_iniciado_em" timestamp with time zone,
  "processamento_concluido_em" timestamp with time zone,
  "criado_em" timestamp with time zone NOT NULL,
  "atualizado_em" timestamp with time zone NOT NULL,
  "observacoes" text
);

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "seguradora_id" uuid;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "identificacao_externa" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "competencia" date;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "periodo_inicio" date;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "periodo_fim" date;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "data_emissao" date;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "data_recebimento" date;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "arquivo_nome" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "arquivo_referencia" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "origem_tipo" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "origem_formato" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "arquivo_mime_type" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "arquivo_hash_sha256" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "chave_idempotencia" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "parser_identificador" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "parser_versao" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "tentativa_processamento" integer;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "status_processamento" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "status_conciliacao" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "quantidade_itens" integer;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "valor_bruto_total" numeric;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "valor_liquido_total" numeric;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "valor_descontos_total" numeric;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "moeda" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "erro_codigo" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "erro_mensagem_segura" text;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "recebido_por_id" uuid;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "processado_por_id" uuid;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "recebido_em" timestamp with time zone;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "processamento_iniciado_em" timestamp with time zone;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "processamento_concluido_em" timestamp with time zone;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "criado_em" timestamp with time zone;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "atualizado_em" timestamp with time zone;

ALTER TABLE erp."comissao_extratos" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."comissao_extrato_itens" (
  "id" uuid PRIMARY KEY NOT NULL,
  "extrato_id" uuid NOT NULL,
  "identificacao_externa" text,
  "sequencia_externa" text,
  "chave_idempotencia" text NOT NULL,
  "produtor_id" uuid,
  "ramo_id" uuid,
  "produtor_beneficiario_informado" text,
  "proposta_numero_informado" text,
  "apolice_numero_informado" text,
  "endosso_numero_informado" text,
  "documento_numero_informado" text,
  "parcela_numero_informado" text,
  "segurado_nome_informado" text,
  "competencia" date,
  "data_credito" date,
  "data_recebimento_informada" date,
  "valor_bruto_informado" numeric,
  "valor_liquido_informado" numeric,
  "valor_descontos_informado" numeric,
  "percentual_informado" numeric,
  "tipo_comissao" text,
  "seguradora_lote_informado" text,
  "seguradora_referencia_informada" text,
  "descricao_original" text,
  "status_conciliacao" text NOT NULL,
  "normalizado_em" timestamp with time zone,
  "criado_em" timestamp with time zone NOT NULL,
  "atualizado_em" timestamp with time zone NOT NULL
);

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "extrato_id" uuid;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "identificacao_externa" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "sequencia_externa" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "chave_idempotencia" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "produtor_id" uuid;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "ramo_id" uuid;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "produtor_beneficiario_informado" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "proposta_numero_informado" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "apolice_numero_informado" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "endosso_numero_informado" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "documento_numero_informado" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "parcela_numero_informado" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "segurado_nome_informado" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "competencia" date;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "data_credito" date;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "data_recebimento_informada" date;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "valor_bruto_informado" numeric;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "valor_liquido_informado" numeric;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "valor_descontos_informado" numeric;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "percentual_informado" numeric;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "tipo_comissao" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "seguradora_lote_informado" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "seguradora_referencia_informada" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "descricao_original" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "status_conciliacao" text;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "normalizado_em" timestamp with time zone;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "criado_em" timestamp with time zone;

ALTER TABLE erp."comissao_extrato_itens" ADD COLUMN IF NOT EXISTS "atualizado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."comissao_conciliacoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "item_id" uuid NOT NULL,
  "comissao_id" uuid NOT NULL,
  "chave_idempotencia" text NOT NULL,
  "tipo_associacao" text NOT NULL,
  "status" text NOT NULL,
  "confianca_pct" numeric,
  "valor_previsto_snapshot" numeric,
  "valor_informado_alocado" numeric,
  "valor_conciliado" numeric,
  "valor_diferenca" numeric,
  "percentual_previsto_snapshot" numeric,
  "percentual_informado_snapshot" numeric,
  "percentual_diferenca" numeric,
  "competencia_prevista_inicio" date,
  "competencia_prevista_fim" date,
  "competencia_informada" date,
  "motivo" text,
  "associado_por_id" uuid,
  "confirmado_por_id" uuid,
  "criado_em" timestamp with time zone NOT NULL,
  "confirmado_em" timestamp with time zone,
  "atualizado_em" timestamp with time zone NOT NULL
);

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "item_id" uuid;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "comissao_id" uuid;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "chave_idempotencia" text;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "tipo_associacao" text;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "confianca_pct" numeric;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "valor_previsto_snapshot" numeric;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "valor_informado_alocado" numeric;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "valor_conciliado" numeric;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "valor_diferenca" numeric;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "percentual_previsto_snapshot" numeric;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "percentual_informado_snapshot" numeric;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "percentual_diferenca" numeric;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "competencia_prevista_inicio" date;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "competencia_prevista_fim" date;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "competencia_informada" date;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "motivo" text;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "associado_por_id" uuid;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "confirmado_por_id" uuid;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "criado_em" timestamp with time zone;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "confirmado_em" timestamp with time zone;

ALTER TABLE erp."comissao_conciliacoes" ADD COLUMN IF NOT EXISTS "atualizado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."comissao_conciliacao_ocorrencias" (
  "id" uuid PRIMARY KEY NOT NULL,
  "item_id" uuid NOT NULL,
  "conciliacao_id" uuid,
  "tipo" text NOT NULL,
  "status" text NOT NULL,
  "motivo" text,
  "valor_esperado" numeric,
  "valor_encontrado" numeric,
  "percentual_esperado" numeric,
  "percentual_encontrado" numeric,
  "competencia_esperada_inicio" date,
  "competencia_esperada_fim" date,
  "competencia_encontrada" date,
  "resolucao_tipo" text,
  "resolucao_observacao" text,
  "identificada_por_id" uuid,
  "resolvida_por_id" uuid,
  "identificada_em" timestamp with time zone NOT NULL,
  "resolvida_em" timestamp with time zone,
  "atualizado_em" timestamp with time zone NOT NULL
);

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "item_id" uuid;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "conciliacao_id" uuid;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "tipo" text;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "motivo" text;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "valor_esperado" numeric;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "valor_encontrado" numeric;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "percentual_esperado" numeric;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "percentual_encontrado" numeric;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "competencia_esperada_inicio" date;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "competencia_esperada_fim" date;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "competencia_encontrada" date;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "resolucao_tipo" text;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "resolucao_observacao" text;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "identificada_por_id" uuid;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "resolvida_por_id" uuid;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "identificada_em" timestamp with time zone;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "resolvida_em" timestamp with time zone;

ALTER TABLE erp."comissao_conciliacao_ocorrencias" ADD COLUMN IF NOT EXISTS "atualizado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."comissao_baixas" (
  "id" uuid PRIMARY KEY NOT NULL,
  "comissao_id" uuid NOT NULL,
  "tipo" text NOT NULL,
  "baixa_origem_id" uuid,
  "origem_tipo" text NOT NULL,
  "data_efetiva" date NOT NULL,
  "valor_efetivo" numeric NOT NULL,
  "motivo_tipo" text NOT NULL,
  "justificativa" text,
  "chave_idempotencia" text NOT NULL,
  "saldo_apos" numeric NOT NULL,
  "status_resultante" text NOT NULL,
  "criado_por_id" uuid NOT NULL,
  "criado_em" timestamp with time zone NOT NULL
);

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "comissao_id" uuid;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "tipo" text;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "baixa_origem_id" uuid;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "origem_tipo" text;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "data_efetiva" date;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "valor_efetivo" numeric;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "motivo_tipo" text;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "justificativa" text;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "chave_idempotencia" text;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "saldo_apos" numeric;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "status_resultante" text;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "criado_por_id" uuid;

ALTER TABLE erp."comissao_baixas" ADD COLUMN IF NOT EXISTS "criado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."comissao_baixa_conciliacoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "baixa_id" uuid NOT NULL,
  "conciliacao_id" uuid NOT NULL,
  "valor_aplicado" numeric NOT NULL,
  "criado_em" timestamp with time zone NOT NULL
);

ALTER TABLE erp."comissao_baixa_conciliacoes" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."comissao_baixa_conciliacoes" ADD COLUMN IF NOT EXISTS "baixa_id" uuid;

ALTER TABLE erp."comissao_baixa_conciliacoes" ADD COLUMN IF NOT EXISTS "conciliacao_id" uuid;

ALTER TABLE erp."comissao_baixa_conciliacoes" ADD COLUMN IF NOT EXISTS "valor_aplicado" numeric;

ALTER TABLE erp."comissao_baixa_conciliacoes" ADD COLUMN IF NOT EXISTS "criado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."repasses" (
  "id" uuid PRIMARY KEY NOT NULL,
  "proposta_id" uuid NOT NULL,
  "comissao_id" uuid,
  "beneficiario_id" uuid NOT NULL,
  "regra_id" uuid,
  "numero" integer,
  "papel_beneficiario" text,
  "base" text,
  "percentual" numeric,
  "valor_previsto" numeric,
  "valor_pago" numeric,
  "valor_diferenca" numeric,
  "status" text,
  "previsto_em" date,
  "liberado_em" date,
  "pago_em" date,
  "forma_pagamento" text,
  "comprovante_referencia" text,
  "observacoes" text
);

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "proposta_id" uuid;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "comissao_id" uuid;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "beneficiario_id" uuid;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "regra_id" uuid;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "numero" integer;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "papel_beneficiario" text;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "base" text;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "percentual" numeric;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "valor_previsto" numeric;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "valor_pago" numeric;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "valor_diferenca" numeric;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "previsto_em" date;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "liberado_em" date;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "pago_em" date;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "forma_pagamento" text;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "comprovante_referencia" text;

ALTER TABLE erp."repasses" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."repasse_recibos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "filial_id" uuid NOT NULL,
  "beneficiario_id" uuid NOT NULL,
  "numero" text NOT NULL,
  "sentido" text NOT NULL,
  "status" text NOT NULL,
  "data_pagamento" date NOT NULL,
  "forma_pagamento" text NOT NULL,
  "comprovante_referencia" text,
  "observacoes" text,
  "chave_idempotencia" text NOT NULL,
  "chave_cancelamento" text,
  "filial_nome_snapshot" text NOT NULL,
  "beneficiario_nome_snapshot" text NOT NULL,
  "emitido_por_id" uuid NOT NULL,
  "emitido_em" timestamp with time zone NOT NULL,
  "cancelado_por_id" uuid,
  "cancelado_em" timestamp with time zone,
  "motivo_cancelamento" text,
  "atualizado_em" timestamp with time zone NOT NULL
);

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "beneficiario_id" uuid;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "numero" text;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "sentido" text;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "data_pagamento" date;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "forma_pagamento" text;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "comprovante_referencia" text;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "observacoes" text;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "chave_idempotencia" text;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "chave_cancelamento" text;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "filial_nome_snapshot" text;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "beneficiario_nome_snapshot" text;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "emitido_por_id" uuid;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "emitido_em" timestamp with time zone;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "cancelado_por_id" uuid;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "cancelado_em" timestamp with time zone;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "motivo_cancelamento" text;

ALTER TABLE erp."repasse_recibos" ADD COLUMN IF NOT EXISTS "atualizado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."repasse_recibo_itens" (
  "id" uuid PRIMARY KEY NOT NULL,
  "recibo_id" uuid NOT NULL,
  "repasse_id" uuid NOT NULL,
  "numero_repasse_snapshot" integer,
  "documento_referencia_snapshot" text NOT NULL,
  "segurado_nome_snapshot" text NOT NULL,
  "seguradora_nome_snapshot" text NOT NULL,
  "ramo_nome_snapshot" text NOT NULL,
  "papel_beneficiario_snapshot" text,
  "valor_previsto_snapshot" numeric NOT NULL,
  "valor_pago_snapshot" numeric NOT NULL,
  "criado_em" timestamp with time zone NOT NULL
);

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "recibo_id" uuid;

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "repasse_id" uuid;

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "numero_repasse_snapshot" integer;

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "documento_referencia_snapshot" text;

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "segurado_nome_snapshot" text;

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "seguradora_nome_snapshot" text;

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "ramo_nome_snapshot" text;

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "papel_beneficiario_snapshot" text;

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "valor_previsto_snapshot" numeric;

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "valor_pago_snapshot" numeric;

ALTER TABLE erp."repasse_recibo_itens" ADD COLUMN IF NOT EXISTS "criado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."atividades" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid,
  "responsavel_id" uuid,
  "entidade_tipo" text,
  "entidade_id" uuid,
  "tipo" text,
  "titulo" text,
  "descricao" text,
  "status" text,
  "prioridade" text,
  "vencimento" timestamp with time zone,
  "concluida_em" timestamp with time zone,
  "fixada_em" timestamp with time zone,
  "canal" text,
  "origem" text,
  "lembrete_em" timestamp with time zone,
  "recorrente" boolean,
  "observacoes" text
);

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "responsavel_id" uuid;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "entidade_tipo" text;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "entidade_id" uuid;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "tipo" text;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "titulo" text;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "descricao" text;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "prioridade" text;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "vencimento" timestamp with time zone;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "concluida_em" timestamp with time zone;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "fixada_em" timestamp with time zone;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "canal" text;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "origem" text;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "lembrete_em" timestamp with time zone;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "recorrente" boolean;

ALTER TABLE erp."atividades" ADD COLUMN IF NOT EXISTS "observacoes" text;

CREATE TABLE IF NOT EXISTS erp."atividade_mencoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "atividade_id" uuid NOT NULL,
  "profile_id" uuid NOT NULL,
  "lida_em" timestamp with time zone,
  "notificada_em" timestamp with time zone
);

ALTER TABLE erp."atividade_mencoes" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."atividade_mencoes" ADD COLUMN IF NOT EXISTS "atividade_id" uuid;

ALTER TABLE erp."atividade_mencoes" ADD COLUMN IF NOT EXISTS "profile_id" uuid;

ALTER TABLE erp."atividade_mencoes" ADD COLUMN IF NOT EXISTS "lida_em" timestamp with time zone;

ALTER TABLE erp."atividade_mencoes" ADD COLUMN IF NOT EXISTS "notificada_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."anexos" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid,
  "entidade_tipo" text,
  "entidade_id" uuid,
  "nome_arquivo" text,
  "mime_type" text,
  "tamanho_bytes" integer,
  "url_armazenamento" text,
  "categoria" text,
  "descricao" text,
  "origem" text,
  "status" text,
  "hash_sha256" text,
  "anexado_em" timestamp with time zone
);

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "entidade_tipo" text;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "entidade_id" uuid;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "nome_arquivo" text;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "mime_type" text;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "tamanho_bytes" integer;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "url_armazenamento" text;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "categoria" text;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "descricao" text;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "origem" text;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "hash_sha256" text;

ALTER TABLE erp."anexos" ADD COLUMN IF NOT EXISTS "anexado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."audit_logs" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "user_id" uuid,
  "entidade_tipo" text,
  "entidade_id" uuid,
  "campo" text,
  "valor_antigo" text,
  "valor_novo" text,
  "acao" text,
  "ocorrido_em" timestamp with time zone,
  "origem" text,
  "ip" text,
  "user_agent" text
);

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "user_id" uuid;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "entidade_tipo" text;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "entidade_id" uuid;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "campo" text;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "valor_antigo" text;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "valor_novo" text;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "acao" text;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "ocorrido_em" timestamp with time zone;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "origem" text;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "ip" text;

ALTER TABLE erp."audit_logs" ADD COLUMN IF NOT EXISTS "user_agent" text;

CREATE TABLE IF NOT EXISTS erp."integracao_logs" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "sistema" text,
  "direcao" text,
  "entidade_tipo" text,
  "entidade_id" uuid,
  "operacao" text,
  "status" text,
  "chave_externa" text,
  "correlation_id" text,
  "payload_text" text,
  "resposta_text" text,
  "erro_text" text,
  "iniciado_em" timestamp with time zone,
  "concluido_em" timestamp with time zone
);

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "sistema" text;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "direcao" text;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "entidade_tipo" text;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "entidade_id" uuid;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "operacao" text;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "status" text;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "chave_externa" text;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "correlation_id" text;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "payload_text" text;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "resposta_text" text;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "erro_text" text;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "iniciado_em" timestamp with time zone;

ALTER TABLE erp."integracao_logs" ADD COLUMN IF NOT EXISTS "concluido_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."campo_definicoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "tenant_id" uuid NOT NULL,
  "filial_id" uuid,
  "entidade_tipo" text NOT NULL,
  "chave" text NOT NULL,
  "nome" text,
  "tipo_dado" text,
  "formato" text,
  "obrigatorio" boolean,
  "ativo" boolean,
  "ordem" integer,
  "ajuda" text,
  "min_valor" numeric,
  "max_valor" numeric,
  "tamanho_max" integer,
  "mascara" text,
  "placeholder" text,
  "agrupamento" text,
  "visivel_em_listagem" boolean
);

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "tenant_id" uuid;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "filial_id" uuid;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "entidade_tipo" text;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "chave" text;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "nome" text;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "tipo_dado" text;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "formato" text;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "obrigatorio" boolean;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "ativo" boolean;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "ajuda" text;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "min_valor" numeric;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "max_valor" numeric;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "tamanho_max" integer;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "mascara" text;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "placeholder" text;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "agrupamento" text;

ALTER TABLE erp."campo_definicoes" ADD COLUMN IF NOT EXISTS "visivel_em_listagem" boolean;

CREATE TABLE IF NOT EXISTS erp."campo_opcoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "campo_definicao_id" uuid NOT NULL,
  "rotulo" text,
  "valor" text,
  "ordem" integer,
  "ativo" boolean
);

ALTER TABLE erp."campo_opcoes" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."campo_opcoes" ADD COLUMN IF NOT EXISTS "campo_definicao_id" uuid;

ALTER TABLE erp."campo_opcoes" ADD COLUMN IF NOT EXISTS "rotulo" text;

ALTER TABLE erp."campo_opcoes" ADD COLUMN IF NOT EXISTS "valor" text;

ALTER TABLE erp."campo_opcoes" ADD COLUMN IF NOT EXISTS "ordem" integer;

ALTER TABLE erp."campo_opcoes" ADD COLUMN IF NOT EXISTS "ativo" boolean;

CREATE TABLE IF NOT EXISTS erp."campo_valores" (
  "id" uuid PRIMARY KEY NOT NULL,
  "campo_definicao_id" uuid NOT NULL,
  "entidade_id" uuid NOT NULL,
  "valor_texto" text,
  "valor_numero" numeric,
  "valor_booleano" boolean,
  "valor_data" date,
  "valor_datahora" timestamp with time zone,
  "valor_opcao_id" uuid,
  "preenchido_em" timestamp with time zone,
  "origem" text,
  "validado_em" timestamp with time zone
);

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "campo_definicao_id" uuid;

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "entidade_id" uuid;

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "valor_texto" text;

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "valor_numero" numeric;

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "valor_booleano" boolean;

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "valor_data" date;

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "valor_datahora" timestamp with time zone;

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "valor_opcao_id" uuid;

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "preenchido_em" timestamp with time zone;

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "origem" text;

ALTER TABLE erp."campo_valores" ADD COLUMN IF NOT EXISTS "validado_em" timestamp with time zone;

CREATE TABLE IF NOT EXISTS erp."campo_valor_opcoes" (
  "id" uuid PRIMARY KEY NOT NULL,
  "campo_valor_id" uuid NOT NULL,
  "campo_opcao_id" uuid NOT NULL,
  "ordem" integer
);

ALTER TABLE erp."campo_valor_opcoes" ADD COLUMN IF NOT EXISTS "id" uuid;

ALTER TABLE erp."campo_valor_opcoes" ADD COLUMN IF NOT EXISTS "campo_valor_id" uuid;

ALTER TABLE erp."campo_valor_opcoes" ADD COLUMN IF NOT EXISTS "campo_opcao_id" uuid;

ALTER TABLE erp."campo_valor_opcoes" ADD COLUMN IF NOT EXISTS "ordem" integer;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_filiais_tenant_id'
      AND conrelid = 'erp."filiais"'::regclass
  ) THEN
    ALTER TABLE erp."filiais"
      ADD CONSTRAINT "fk_filiais_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_filiais_matriz_id'
      AND conrelid = 'erp."filiais"'::regclass
  ) THEN
    ALTER TABLE erp."filiais"
      ADD CONSTRAINT "fk_filiais_matriz_id"
      FOREIGN KEY ("matriz_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_filiais_gerente_id'
      AND conrelid = 'erp."filiais"'::regclass
  ) THEN
    ALTER TABLE erp."filiais"
      ADD CONSTRAINT "fk_filiais_gerente_id"
      FOREIGN KEY ("gerente_id")
      REFERENCES erp."produtores" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_filiais_tenant_id_cnpj_cpf" ON erp."filiais" ("tenant_id", "cnpj_cpf");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_profiles_tenant_id'
      AND conrelid = 'erp."profiles"'::regclass
  ) THEN
    ALTER TABLE erp."profiles"
      ADD CONSTRAINT "fk_profiles_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_perfis_tenant_id'
      AND conrelid = 'erp."perfis"'::regclass
  ) THEN
    ALTER TABLE erp."perfis"
      ADD CONSTRAINT "fk_perfis_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_profile_filiais_profile_id'
      AND conrelid = 'erp."profile_filiais"'::regclass
  ) THEN
    ALTER TABLE erp."profile_filiais"
      ADD CONSTRAINT "fk_profile_filiais_profile_id"
      FOREIGN KEY ("profile_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_profile_filiais_filial_id'
      AND conrelid = 'erp."profile_filiais"'::regclass
  ) THEN
    ALTER TABLE erp."profile_filiais"
      ADD CONSTRAINT "fk_profile_filiais_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_profile_filiais_perfil_id'
      AND conrelid = 'erp."profile_filiais"'::regclass
  ) THEN
    ALTER TABLE erp."profile_filiais"
      ADD CONSTRAINT "fk_profile_filiais_perfil_id"
      FOREIGN KEY ("perfil_id")
      REFERENCES erp."perfis" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_role_permissions_perfil_id'
      AND conrelid = 'erp."role_permissions"'::regclass
  ) THEN
    ALTER TABLE erp."role_permissions"
      ADD CONSTRAINT "fk_role_permissions_perfil_id"
      FOREIGN KEY ("perfil_id")
      REFERENCES erp."perfis" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_produtores_tenant_id'
      AND conrelid = 'erp."produtores"'::regclass
  ) THEN
    ALTER TABLE erp."produtores"
      ADD CONSTRAINT "fk_produtores_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_produtores_profile_id'
      AND conrelid = 'erp."produtores"'::regclass
  ) THEN
    ALTER TABLE erp."produtores"
      ADD CONSTRAINT "fk_produtores_profile_id"
      FOREIGN KEY ("profile_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_segurados_tenant_id'
      AND conrelid = 'erp."segurados"'::regclass
  ) THEN
    ALTER TABLE erp."segurados"
      ADD CONSTRAINT "fk_segurados_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_segurados_filial_id'
      AND conrelid = 'erp."segurados"'::regclass
  ) THEN
    ALTER TABLE erp."segurados"
      ADD CONSTRAINT "fk_segurados_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_segurados_produtor_id'
      AND conrelid = 'erp."segurados"'::regclass
  ) THEN
    ALTER TABLE erp."segurados"
      ADD CONSTRAINT "fk_segurados_produtor_id"
      FOREIGN KEY ("produtor_id")
      REFERENCES erp."produtores" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_segurados_gerente_id'
      AND conrelid = 'erp."segurados"'::regclass
  ) THEN
    ALTER TABLE erp."segurados"
      ADD CONSTRAINT "fk_segurados_gerente_id"
      FOREIGN KEY ("gerente_id")
      REFERENCES erp."produtores" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_segurados_filial_id_cpf_cnpj" ON erp."segurados" ("filial_id", "cpf_cnpj");

CREATE INDEX IF NOT EXISTS "ix_segurados_cpf_cnpj" ON erp."segurados" ("cpf_cnpj");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_pessoa_contato_pj_id'
      AND conrelid = 'erp."pessoa_contato"'::regclass
  ) THEN
    ALTER TABLE erp."pessoa_contato"
      ADD CONSTRAINT "fk_pessoa_contato_pj_id"
      FOREIGN KEY ("pj_id")
      REFERENCES erp."segurados" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_pessoa_contato_pf_id'
      AND conrelid = 'erp."pessoa_contato"'::regclass
  ) THEN
    ALTER TABLE erp."pessoa_contato"
      ADD CONSTRAINT "fk_pessoa_contato_pf_id"
      FOREIGN KEY ("pf_id")
      REFERENCES erp."segurados" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_seguradoras_tenant_id'
      AND conrelid = 'erp."seguradoras"'::regclass
  ) THEN
    ALTER TABLE erp."seguradoras"
      ADD CONSTRAINT "fk_seguradoras_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_ramos_tenant_id'
      AND conrelid = 'erp."ramos"'::regclass
  ) THEN
    ALTER TABLE erp."ramos"
      ADD CONSTRAINT "fk_ramos_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_endosso_subtipos_tenant_id'
      AND conrelid = 'erp."endosso_subtipos"'::regclass
  ) THEN
    ALTER TABLE erp."endosso_subtipos"
      ADD CONSTRAINT "fk_endosso_subtipos_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_endosso_subtipos_filial_id'
      AND conrelid = 'erp."endosso_subtipos"'::regclass
  ) THEN
    ALTER TABLE erp."endosso_subtipos"
      ADD CONSTRAINT "fk_endosso_subtipos_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_endosso_subtipos_ramo_id'
      AND conrelid = 'erp."endosso_subtipos"'::regclass
  ) THEN
    ALTER TABLE erp."endosso_subtipos"
      ADD CONSTRAINT "fk_endosso_subtipos_ramo_id"
      FOREIGN KEY ("ramo_id")
      REFERENCES erp."ramos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_cancelamento_motivos_tenant_id'
      AND conrelid = 'erp."cancelamento_motivos"'::regclass
  ) THEN
    ALTER TABLE erp."cancelamento_motivos"
      ADD CONSTRAINT "fk_cancelamento_motivos_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_cancelamento_motivos_filial_id'
      AND conrelid = 'erp."cancelamento_motivos"'::regclass
  ) THEN
    ALTER TABLE erp."cancelamento_motivos"
      ADD CONSTRAINT "fk_cancelamento_motivos_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_cancelamento_motivos_ramo_id'
      AND conrelid = 'erp."cancelamento_motivos"'::regclass
  ) THEN
    ALTER TABLE erp."cancelamento_motivos"
      ADD CONSTRAINT "fk_cancelamento_motivos_ramo_id"
      FOREIGN KEY ("ramo_id")
      REFERENCES erp."ramos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_origens_tenant_id'
      AND conrelid = 'erp."origens"'::regclass
  ) THEN
    ALTER TABLE erp."origens"
      ADD CONSTRAINT "fk_origens_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_motivos_perda_tenant_id'
      AND conrelid = 'erp."motivos_perda"'::regclass
  ) THEN
    ALTER TABLE erp."motivos_perda"
      ADD CONSTRAINT "fk_motivos_perda_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_coberturas_catalogo_ramo_id'
      AND conrelid = 'erp."coberturas_catalogo"'::regclass
  ) THEN
    ALTER TABLE erp."coberturas_catalogo"
      ADD CONSTRAINT "fk_coberturas_catalogo_ramo_id"
      FOREIGN KEY ("ramo_id")
      REFERENCES erp."ramos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_pipelines_tenant_id'
      AND conrelid = 'erp."pipelines"'::regclass
  ) THEN
    ALTER TABLE erp."pipelines"
      ADD CONSTRAINT "fk_pipelines_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_pipelines_filial_id'
      AND conrelid = 'erp."pipelines"'::regclass
  ) THEN
    ALTER TABLE erp."pipelines"
      ADD CONSTRAINT "fk_pipelines_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_pipeline_stages_pipeline_id'
      AND conrelid = 'erp."pipeline_stages"'::regclass
  ) THEN
    ALTER TABLE erp."pipeline_stages"
      ADD CONSTRAINT "fk_pipeline_stages_pipeline_id"
      FOREIGN KEY ("pipeline_id")
      REFERENCES erp."pipelines" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_oportunidades_tenant_id'
      AND conrelid = 'erp."oportunidades"'::regclass
  ) THEN
    ALTER TABLE erp."oportunidades"
      ADD CONSTRAINT "fk_oportunidades_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_oportunidades_filial_id'
      AND conrelid = 'erp."oportunidades"'::regclass
  ) THEN
    ALTER TABLE erp."oportunidades"
      ADD CONSTRAINT "fk_oportunidades_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_oportunidades_segurado_id'
      AND conrelid = 'erp."oportunidades"'::regclass
  ) THEN
    ALTER TABLE erp."oportunidades"
      ADD CONSTRAINT "fk_oportunidades_segurado_id"
      FOREIGN KEY ("segurado_id")
      REFERENCES erp."segurados" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_oportunidades_ramo_id'
      AND conrelid = 'erp."oportunidades"'::regclass
  ) THEN
    ALTER TABLE erp."oportunidades"
      ADD CONSTRAINT "fk_oportunidades_ramo_id"
      FOREIGN KEY ("ramo_id")
      REFERENCES erp."ramos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_oportunidades_origem_id'
      AND conrelid = 'erp."oportunidades"'::regclass
  ) THEN
    ALTER TABLE erp."oportunidades"
      ADD CONSTRAINT "fk_oportunidades_origem_id"
      FOREIGN KEY ("origem_id")
      REFERENCES erp."origens" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_oportunidades_apolice_origem_id'
      AND conrelid = 'erp."oportunidades"'::regclass
  ) THEN
    ALTER TABLE erp."oportunidades"
      ADD CONSTRAINT "fk_oportunidades_apolice_origem_id"
      FOREIGN KEY ("apolice_origem_id")
      REFERENCES erp."apolices" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_oportunidades_responsavel_id'
      AND conrelid = 'erp."oportunidades"'::regclass
  ) THEN
    ALTER TABLE erp."oportunidades"
      ADD CONSTRAINT "fk_oportunidades_responsavel_id"
      FOREIGN KEY ("responsavel_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_oportunidades_stage_id'
      AND conrelid = 'erp."oportunidades"'::regclass
  ) THEN
    ALTER TABLE erp."oportunidades"
      ADD CONSTRAINT "fk_oportunidades_stage_id"
      FOREIGN KEY ("stage_id")
      REFERENCES erp."pipeline_stages" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_oportunidades_motivo_perda_id'
      AND conrelid = 'erp."oportunidades"'::regclass
  ) THEN
    ALTER TABLE erp."oportunidades"
      ADD CONSTRAINT "fk_oportunidades_motivo_perda_id"
      FOREIGN KEY ("motivo_perda_id")
      REFERENCES erp."motivos_perda" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calculos_oportunidade_id'
      AND conrelid = 'erp."calculos"'::regclass
  ) THEN
    ALTER TABLE erp."calculos"
      ADD CONSTRAINT "fk_calculos_oportunidade_id"
      FOREIGN KEY ("oportunidade_id")
      REFERENCES erp."oportunidades" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calculos_ramo_id'
      AND conrelid = 'erp."calculos"'::regclass
  ) THEN
    ALTER TABLE erp."calculos"
      ADD CONSTRAINT "fk_calculos_ramo_id"
      FOREIGN KEY ("ramo_id")
      REFERENCES erp."ramos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calculos_segurado_id'
      AND conrelid = 'erp."calculos"'::regclass
  ) THEN
    ALTER TABLE erp."calculos"
      ADD CONSTRAINT "fk_calculos_segurado_id"
      FOREIGN KEY ("segurado_id")
      REFERENCES erp."segurados" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calculos_seguradora_anterior_id'
      AND conrelid = 'erp."calculos"'::regclass
  ) THEN
    ALTER TABLE erp."calculos"
      ADD CONSTRAINT "fk_calculos_seguradora_anterior_id"
      FOREIGN KEY ("seguradora_anterior_id")
      REFERENCES erp."seguradoras" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calc_auto_calculo_id'
      AND conrelid = 'erp."calc_auto"'::regclass
  ) THEN
    ALTER TABLE erp."calc_auto"
      ADD CONSTRAINT "fk_calc_auto_calculo_id"
      FOREIGN KEY ("calculo_id")
      REFERENCES erp."calculos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calc_residencia_calculo_id'
      AND conrelid = 'erp."calc_residencia"'::regclass
  ) THEN
    ALTER TABLE erp."calc_residencia"
      ADD CONSTRAINT "fk_calc_residencia_calculo_id"
      FOREIGN KEY ("calculo_id")
      REFERENCES erp."calculos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calc_condominio_calculo_id'
      AND conrelid = 'erp."calc_condominio"'::regclass
  ) THEN
    ALTER TABLE erp."calc_condominio"
      ADD CONSTRAINT "fk_calc_condominio_calculo_id"
      FOREIGN KEY ("calculo_id")
      REFERENCES erp."calculos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calc_vida_calculo_id'
      AND conrelid = 'erp."calc_vida"'::regclass
  ) THEN
    ALTER TABLE erp."calc_vida"
      ADD CONSTRAINT "fk_calc_vida_calculo_id"
      FOREIGN KEY ("calculo_id")
      REFERENCES erp."calculos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calc_empresa_calculo_id'
      AND conrelid = 'erp."calc_empresa"'::regclass
  ) THEN
    ALTER TABLE erp."calc_empresa"
      ADD CONSTRAINT "fk_calc_empresa_calculo_id"
      FOREIGN KEY ("calculo_id")
      REFERENCES erp."calculos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calc_diversos_calculo_id'
      AND conrelid = 'erp."calc_diversos"'::regclass
  ) THEN
    ALTER TABLE erp."calc_diversos"
      ADD CONSTRAINT "fk_calc_diversos_calculo_id"
      FOREIGN KEY ("calculo_id")
      REFERENCES erp."calculos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calculo_coberturas_calculo_id'
      AND conrelid = 'erp."calculo_coberturas"'::regclass
  ) THEN
    ALTER TABLE erp."calculo_coberturas"
      ADD CONSTRAINT "fk_calculo_coberturas_calculo_id"
      FOREIGN KEY ("calculo_id")
      REFERENCES erp."calculos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calculo_coberturas_cobertura_id'
      AND conrelid = 'erp."calculo_coberturas"'::regclass
  ) THEN
    ALTER TABLE erp."calculo_coberturas"
      ADD CONSTRAINT "fk_calculo_coberturas_cobertura_id"
      FOREIGN KEY ("cobertura_id")
      REFERENCES erp."coberturas_catalogo" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_calculo_coberturas_calculo_id_cobertura_id" ON erp."calculo_coberturas" ("calculo_id", "cobertura_id");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calculo_execucoes_calculo_id'
      AND conrelid = 'erp."calculo_execucoes"'::regclass
  ) THEN
    ALTER TABLE erp."calculo_execucoes"
      ADD CONSTRAINT "fk_calculo_execucoes_calculo_id"
      FOREIGN KEY ("calculo_id")
      REFERENCES erp."calculos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calculo_execucoes_seguradora_id'
      AND conrelid = 'erp."calculo_execucoes"'::regclass
  ) THEN
    ALTER TABLE erp."calculo_execucoes"
      ADD CONSTRAINT "fk_calculo_execucoes_seguradora_id"
      FOREIGN KEY ("seguradora_id")
      REFERENCES erp."seguradoras" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_calculo_execucoes_reexecucao_de_id'
      AND conrelid = 'erp."calculo_execucoes"'::regclass
  ) THEN
    ALTER TABLE erp."calculo_execucoes"
      ADD CONSTRAINT "fk_calculo_execucoes_reexecucao_de_id"
      FOREIGN KEY ("reexecucao_de_id")
      REFERENCES erp."calculo_execucoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_calculo_execucoes_calculo_id_seguradora_id_tentativa" ON erp."calculo_execucoes" ("calculo_id", "seguradora_id", "tentativa");

CREATE INDEX IF NOT EXISTS "ix_calculo_execucoes_reexecucao_de_id" ON erp."calculo_execucoes" ("reexecucao_de_id");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_cotacoes_execucao_id'
      AND conrelid = 'erp."cotacoes"'::regclass
  ) THEN
    ALTER TABLE erp."cotacoes"
      ADD CONSTRAINT "fk_cotacoes_execucao_id"
      FOREIGN KEY ("execucao_id")
      REFERENCES erp."calculo_execucoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_cotacoes_execucao_id" ON erp."cotacoes" ("execucao_id");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_cotacao_coberturas_cotacao_id'
      AND conrelid = 'erp."cotacao_coberturas"'::regclass
  ) THEN
    ALTER TABLE erp."cotacao_coberturas"
      ADD CONSTRAINT "fk_cotacao_coberturas_cotacao_id"
      FOREIGN KEY ("cotacao_id")
      REFERENCES erp."cotacoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_cotacao_coberturas_cobertura_id'
      AND conrelid = 'erp."cotacao_coberturas"'::regclass
  ) THEN
    ALTER TABLE erp."cotacao_coberturas"
      ADD CONSTRAINT "fk_cotacao_coberturas_cobertura_id"
      FOREIGN KEY ("cobertura_id")
      REFERENCES erp."coberturas_catalogo" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_cotacao_coberturas_cotacao_id_chave_resultado" ON erp."cotacao_coberturas" ("cotacao_id", "chave_resultado");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_cotacao_parcelamentos_cotacao_id'
      AND conrelid = 'erp."cotacao_parcelamentos"'::regclass
  ) THEN
    ALTER TABLE erp."cotacao_parcelamentos"
      ADD CONSTRAINT "fk_cotacao_parcelamentos_cotacao_id"
      FOREIGN KEY ("cotacao_id")
      REFERENCES erp."cotacoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_cotacao_parcelamentos_cotacao_id_codigo_opcao" ON erp."cotacao_parcelamentos" ("cotacao_id", "codigo_opcao");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apresentacoes_comerciais_oportunidade_id'
      AND conrelid = 'erp."apresentacoes_comerciais"'::regclass
  ) THEN
    ALTER TABLE erp."apresentacoes_comerciais"
      ADD CONSTRAINT "fk_apresentacoes_comerciais_oportunidade_id"
      FOREIGN KEY ("oportunidade_id")
      REFERENCES erp."oportunidades" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apresentacoes_comerciais_criado_por_id'
      AND conrelid = 'erp."apresentacoes_comerciais"'::regclass
  ) THEN
    ALTER TABLE erp."apresentacoes_comerciais"
      ADD CONSTRAINT "fk_apresentacoes_comerciais_criado_por_id"
      FOREIGN KEY ("criado_por_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apresentacoes_comerciais_cotacao_escolhida_id'
      AND conrelid = 'erp."apresentacoes_comerciais"'::regclass
  ) THEN
    ALTER TABLE erp."apresentacoes_comerciais"
      ADD CONSTRAINT "fk_apresentacoes_comerciais_cotacao_escolhida_id"
      FOREIGN KEY ("cotacao_escolhida_id")
      REFERENCES erp."cotacoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apresentacao_cotacoes_apresentacao_id'
      AND conrelid = 'erp."apresentacao_cotacoes"'::regclass
  ) THEN
    ALTER TABLE erp."apresentacao_cotacoes"
      ADD CONSTRAINT "fk_apresentacao_cotacoes_apresentacao_id"
      FOREIGN KEY ("apresentacao_id")
      REFERENCES erp."apresentacoes_comerciais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apresentacao_cotacoes_cotacao_id'
      AND conrelid = 'erp."apresentacao_cotacoes"'::regclass
  ) THEN
    ALTER TABLE erp."apresentacao_cotacoes"
      ADD CONSTRAINT "fk_apresentacao_cotacoes_cotacao_id"
      FOREIGN KEY ("cotacao_id")
      REFERENCES erp."cotacoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apresentacao_cotacoes_parcelamento_id'
      AND conrelid = 'erp."apresentacao_cotacoes"'::regclass
  ) THEN
    ALTER TABLE erp."apresentacao_cotacoes"
      ADD CONSTRAINT "fk_apresentacao_cotacoes_parcelamento_id"
      FOREIGN KEY ("parcelamento_id")
      REFERENCES erp."cotacao_parcelamentos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_apresentacao_cotacoes_apresentacao_id_cotacao_id" ON erp."apresentacao_cotacoes" ("apresentacao_id", "cotacao_id");

CREATE UNIQUE INDEX IF NOT EXISTS "ux_apresentacao_cotacoes_apresentacao_id_ordem" ON erp."apresentacao_cotacoes" ("apresentacao_id", "ordem");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apolices_segurado_id'
      AND conrelid = 'erp."apolices"'::regclass
  ) THEN
    ALTER TABLE erp."apolices"
      ADD CONSTRAINT "fk_apolices_segurado_id"
      FOREIGN KEY ("segurado_id")
      REFERENCES erp."segurados" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apolices_seguradora_id'
      AND conrelid = 'erp."apolices"'::regclass
  ) THEN
    ALTER TABLE erp."apolices"
      ADD CONSTRAINT "fk_apolices_seguradora_id"
      FOREIGN KEY ("seguradora_id")
      REFERENCES erp."seguradoras" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apolices_ramo_id'
      AND conrelid = 'erp."apolices"'::regclass
  ) THEN
    ALTER TABLE erp."apolices"
      ADD CONSTRAINT "fk_apolices_ramo_id"
      FOREIGN KEY ("ramo_id")
      REFERENCES erp."ramos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apolices_renovada_de_id'
      AND conrelid = 'erp."apolices"'::regclass
  ) THEN
    ALTER TABLE erp."apolices"
      ADD CONSTRAINT "fk_apolices_renovada_de_id"
      FOREIGN KEY ("renovada_de_id")
      REFERENCES erp."apolices" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apolices_produtor_id'
      AND conrelid = 'erp."apolices"'::regclass
  ) THEN
    ALTER TABLE erp."apolices"
      ADD CONSTRAINT "fk_apolices_produtor_id"
      FOREIGN KEY ("produtor_id")
      REFERENCES erp."produtores" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_propostas_apolice_id'
      AND conrelid = 'erp."propostas"'::regclass
  ) THEN
    ALTER TABLE erp."propostas"
      ADD CONSTRAINT "fk_propostas_apolice_id"
      FOREIGN KEY ("apolice_id")
      REFERENCES erp."apolices" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_propostas_cotacao_id'
      AND conrelid = 'erp."propostas"'::regclass
  ) THEN
    ALTER TABLE erp."propostas"
      ADD CONSTRAINT "fk_propostas_cotacao_id"
      FOREIGN KEY ("cotacao_id")
      REFERENCES erp."cotacoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_propostas_stage_id'
      AND conrelid = 'erp."propostas"'::regclass
  ) THEN
    ALTER TABLE erp."propostas"
      ADD CONSTRAINT "fk_propostas_stage_id"
      FOREIGN KEY ("stage_id")
      REFERENCES erp."pipeline_stages" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_propostas_responsavel_id'
      AND conrelid = 'erp."propostas"'::regclass
  ) THEN
    ALTER TABLE erp."propostas"
      ADD CONSTRAINT "fk_propostas_responsavel_id"
      FOREIGN KEY ("responsavel_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_propostas_recebimento_grade_id'
      AND conrelid = 'erp."propostas"'::regclass
  ) THEN
    ALTER TABLE erp."propostas"
      ADD CONSTRAINT "fk_propostas_recebimento_grade_id"
      FOREIGN KEY ("recebimento_grade_id")
      REFERENCES erp."recebimento_grades" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_propostas_endosso_subtipo_id'
      AND conrelid = 'erp."propostas"'::regclass
  ) THEN
    ALTER TABLE erp."propostas"
      ADD CONSTRAINT "fk_propostas_endosso_subtipo_id"
      FOREIGN KEY ("endosso_subtipo_id")
      REFERENCES erp."endosso_subtipos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_propostas_cancelamento_motivo_id'
      AND conrelid = 'erp."propostas"'::regclass
  ) THEN
    ALTER TABLE erp."propostas"
      ADD CONSTRAINT "fk_propostas_cancelamento_motivo_id"
      FOREIGN KEY ("cancelamento_motivo_id")
      REFERENCES erp."cancelamento_motivos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_propostas_cotacao_id" ON erp."propostas" ("cotacao_id");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apolice_itens_apolice_id'
      AND conrelid = 'erp."apolice_itens"'::regclass
  ) THEN
    ALTER TABLE erp."apolice_itens"
      ADD CONSTRAINT "fk_apolice_itens_apolice_id"
      FOREIGN KEY ("apolice_id")
      REFERENCES erp."apolices" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apolice_itens_incluido_por_proposta_id'
      AND conrelid = 'erp."apolice_itens"'::regclass
  ) THEN
    ALTER TABLE erp."apolice_itens"
      ADD CONSTRAINT "fk_apolice_itens_incluido_por_proposta_id"
      FOREIGN KEY ("incluido_por_proposta_id")
      REFERENCES erp."propostas" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_apolice_itens_excluido_por_proposta_id'
      AND conrelid = 'erp."apolice_itens"'::regclass
  ) THEN
    ALTER TABLE erp."apolice_itens"
      ADD CONSTRAINT "fk_apolice_itens_excluido_por_proposta_id"
      FOREIGN KEY ("excluido_por_proposta_id")
      REFERENCES erp."propostas" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_item_veiculo_apolice_item_id'
      AND conrelid = 'erp."item_veiculo"'::regclass
  ) THEN
    ALTER TABLE erp."item_veiculo"
      ADD CONSTRAINT "fk_item_veiculo_apolice_item_id"
      FOREIGN KEY ("apolice_item_id")
      REFERENCES erp."apolice_itens" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_item_imovel_apolice_item_id'
      AND conrelid = 'erp."item_imovel"'::regclass
  ) THEN
    ALTER TABLE erp."item_imovel"
      ADD CONSTRAINT "fk_item_imovel_apolice_item_id"
      FOREIGN KEY ("apolice_item_id")
      REFERENCES erp."apolice_itens" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_item_empresa_apolice_item_id'
      AND conrelid = 'erp."item_empresa"'::regclass
  ) THEN
    ALTER TABLE erp."item_empresa"
      ADD CONSTRAINT "fk_item_empresa_apolice_item_id"
      FOREIGN KEY ("apolice_item_id")
      REFERENCES erp."apolice_itens" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_item_vida_apolice_item_id'
      AND conrelid = 'erp."item_vida"'::regclass
  ) THEN
    ALTER TABLE erp."item_vida"
      ADD CONSTRAINT "fk_item_vida_apolice_item_id"
      FOREIGN KEY ("apolice_item_id")
      REFERENCES erp."apolice_itens" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_item_vida_pessoa_id'
      AND conrelid = 'erp."item_vida"'::regclass
  ) THEN
    ALTER TABLE erp."item_vida"
      ADD CONSTRAINT "fk_item_vida_pessoa_id"
      FOREIGN KEY ("pessoa_id")
      REFERENCES erp."segurados" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_item_coberturas_apolice_item_id'
      AND conrelid = 'erp."item_coberturas"'::regclass
  ) THEN
    ALTER TABLE erp."item_coberturas"
      ADD CONSTRAINT "fk_item_coberturas_apolice_item_id"
      FOREIGN KEY ("apolice_item_id")
      REFERENCES erp."apolice_itens" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_item_coberturas_cobertura_id'
      AND conrelid = 'erp."item_coberturas"'::regclass
  ) THEN
    ALTER TABLE erp."item_coberturas"
      ADD CONSTRAINT "fk_item_coberturas_cobertura_id"
      FOREIGN KEY ("cobertura_id")
      REFERENCES erp."coberturas_catalogo" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_item_coberturas_incluido_por_proposta_id'
      AND conrelid = 'erp."item_coberturas"'::regclass
  ) THEN
    ALTER TABLE erp."item_coberturas"
      ADD CONSTRAINT "fk_item_coberturas_incluido_por_proposta_id"
      FOREIGN KEY ("incluido_por_proposta_id")
      REFERENCES erp."propostas" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_item_coberturas_excluido_por_proposta_id'
      AND conrelid = 'erp."item_coberturas"'::regclass
  ) THEN
    ALTER TABLE erp."item_coberturas"
      ADD CONSTRAINT "fk_item_coberturas_excluido_por_proposta_id"
      FOREIGN KEY ("excluido_por_proposta_id")
      REFERENCES erp."propostas" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_sinistros_apolice_id'
      AND conrelid = 'erp."sinistros"'::regclass
  ) THEN
    ALTER TABLE erp."sinistros"
      ADD CONSTRAINT "fk_sinistros_apolice_id"
      FOREIGN KEY ("apolice_id")
      REFERENCES erp."apolices" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_sinistros_stage_id'
      AND conrelid = 'erp."sinistros"'::regclass
  ) THEN
    ALTER TABLE erp."sinistros"
      ADD CONSTRAINT "fk_sinistros_stage_id"
      FOREIGN KEY ("stage_id")
      REFERENCES erp."pipeline_stages" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_sinistros_responsavel_id'
      AND conrelid = 'erp."sinistros"'::regclass
  ) THEN
    ALTER TABLE erp."sinistros"
      ADD CONSTRAINT "fk_sinistros_responsavel_id"
      FOREIGN KEY ("responsavel_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_sinistro_envolvidos_sinistro_id'
      AND conrelid = 'erp."sinistro_envolvidos"'::regclass
  ) THEN
    ALTER TABLE erp."sinistro_envolvidos"
      ADD CONSTRAINT "fk_sinistro_envolvidos_sinistro_id"
      FOREIGN KEY ("sinistro_id")
      REFERENCES erp."sinistros" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_sinistro_envolvidos_apolice_item_id'
      AND conrelid = 'erp."sinistro_envolvidos"'::regclass
  ) THEN
    ALTER TABLE erp."sinistro_envolvidos"
      ADD CONSTRAINT "fk_sinistro_envolvidos_apolice_item_id"
      FOREIGN KEY ("apolice_item_id")
      REFERENCES erp."apolice_itens" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_pos_vendas_apolice_id'
      AND conrelid = 'erp."pos_vendas"'::regclass
  ) THEN
    ALTER TABLE erp."pos_vendas"
      ADD CONSTRAINT "fk_pos_vendas_apolice_id"
      FOREIGN KEY ("apolice_id")
      REFERENCES erp."apolices" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_pos_vendas_stage_id'
      AND conrelid = 'erp."pos_vendas"'::regclass
  ) THEN
    ALTER TABLE erp."pos_vendas"
      ADD CONSTRAINT "fk_pos_vendas_stage_id"
      FOREIGN KEY ("stage_id")
      REFERENCES erp."pipeline_stages" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_pos_vendas_responsavel_id'
      AND conrelid = 'erp."pos_vendas"'::regclass
  ) THEN
    ALTER TABLE erp."pos_vendas"
      ADD CONSTRAINT "fk_pos_vendas_responsavel_id"
      FOREIGN KEY ("responsavel_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_recebimento_grades_seguradora_id'
      AND conrelid = 'erp."recebimento_grades"'::regclass
  ) THEN
    ALTER TABLE erp."recebimento_grades"
      ADD CONSTRAINT "fk_recebimento_grades_seguradora_id"
      FOREIGN KEY ("seguradora_id")
      REFERENCES erp."seguradoras" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_recebimento_grades_ramo_id'
      AND conrelid = 'erp."recebimento_grades"'::regclass
  ) THEN
    ALTER TABLE erp."recebimento_grades"
      ADD CONSTRAINT "fk_recebimento_grades_ramo_id"
      FOREIGN KEY ("ramo_id")
      REFERENCES erp."ramos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_recebimento_grade_parcelas_grade_id'
      AND conrelid = 'erp."recebimento_grade_parcelas"'::regclass
  ) THEN
    ALTER TABLE erp."recebimento_grade_parcelas"
      ADD CONSTRAINT "fk_recebimento_grade_parcelas_grade_id"
      FOREIGN KEY ("grade_id")
      REFERENCES erp."recebimento_grades" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasse_regras_tenant_id'
      AND conrelid = 'erp."repasse_regras"'::regclass
  ) THEN
    ALTER TABLE erp."repasse_regras"
      ADD CONSTRAINT "fk_repasse_regras_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasse_regras_filial_id'
      AND conrelid = 'erp."repasse_regras"'::regclass
  ) THEN
    ALTER TABLE erp."repasse_regras"
      ADD CONSTRAINT "fk_repasse_regras_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasse_regras_produtor_id'
      AND conrelid = 'erp."repasse_regras"'::regclass
  ) THEN
    ALTER TABLE erp."repasse_regras"
      ADD CONSTRAINT "fk_repasse_regras_produtor_id"
      FOREIGN KEY ("produtor_id")
      REFERENCES erp."produtores" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasse_regras_ramo_id'
      AND conrelid = 'erp."repasse_regras"'::regclass
  ) THEN
    ALTER TABLE erp."repasse_regras"
      ADD CONSTRAINT "fk_repasse_regras_ramo_id"
      FOREIGN KEY ("ramo_id")
      REFERENCES erp."ramos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_parcelas_proposta_id'
      AND conrelid = 'erp."parcelas"'::regclass
  ) THEN
    ALTER TABLE erp."parcelas"
      ADD CONSTRAINT "fk_parcelas_proposta_id"
      FOREIGN KEY ("proposta_id")
      REFERENCES erp."propostas" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_financeiro_cobrancas_parcela_id'
      AND conrelid = 'erp."financeiro_cobrancas"'::regclass
  ) THEN
    ALTER TABLE erp."financeiro_cobrancas"
      ADD CONSTRAINT "fk_financeiro_cobrancas_parcela_id"
      FOREIGN KEY ("parcela_id")
      REFERENCES erp."parcelas" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_financeiro_cobrancas_stage_id'
      AND conrelid = 'erp."financeiro_cobrancas"'::regclass
  ) THEN
    ALTER TABLE erp."financeiro_cobrancas"
      ADD CONSTRAINT "fk_financeiro_cobrancas_stage_id"
      FOREIGN KEY ("stage_id")
      REFERENCES erp."pipeline_stages" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_financeiro_cobrancas_responsavel_id'
      AND conrelid = 'erp."financeiro_cobrancas"'::regclass
  ) THEN
    ALTER TABLE erp."financeiro_cobrancas"
      ADD CONSTRAINT "fk_financeiro_cobrancas_responsavel_id"
      FOREIGN KEY ("responsavel_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE INDEX IF NOT EXISTS "ix_financeiro_cobrancas_parcela_id" ON erp."financeiro_cobrancas" ("parcela_id");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissoes_proposta_id'
      AND conrelid = 'erp."comissoes"'::regclass
  ) THEN
    ALTER TABLE erp."comissoes"
      ADD CONSTRAINT "fk_comissoes_proposta_id"
      FOREIGN KEY ("proposta_id")
      REFERENCES erp."propostas" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissoes_parcela_id'
      AND conrelid = 'erp."comissoes"'::regclass
  ) THEN
    ALTER TABLE erp."comissoes"
      ADD CONSTRAINT "fk_comissoes_parcela_id"
      FOREIGN KEY ("parcela_id")
      REFERENCES erp."parcelas" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_extratos_tenant_id'
      AND conrelid = 'erp."comissao_extratos"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_extratos"
      ADD CONSTRAINT "fk_comissao_extratos_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_extratos_filial_id'
      AND conrelid = 'erp."comissao_extratos"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_extratos"
      ADD CONSTRAINT "fk_comissao_extratos_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_extratos_seguradora_id'
      AND conrelid = 'erp."comissao_extratos"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_extratos"
      ADD CONSTRAINT "fk_comissao_extratos_seguradora_id"
      FOREIGN KEY ("seguradora_id")
      REFERENCES erp."seguradoras" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_extratos_recebido_por_id'
      AND conrelid = 'erp."comissao_extratos"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_extratos"
      ADD CONSTRAINT "fk_comissao_extratos_recebido_por_id"
      FOREIGN KEY ("recebido_por_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_extratos_processado_por_id'
      AND conrelid = 'erp."comissao_extratos"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_extratos"
      ADD CONSTRAINT "fk_comissao_extratos_processado_por_id"
      FOREIGN KEY ("processado_por_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_comissao_extratos_filial_id_chave_idempotencia" ON erp."comissao_extratos" ("filial_id", "chave_idempotencia");

CREATE UNIQUE INDEX IF NOT EXISTS "ux_comissao_extratos_filial_id_seguradora_id_arquivo_hash_sha25" ON erp."comissao_extratos" ("filial_id", "seguradora_id", "arquivo_hash_sha256");

CREATE UNIQUE INDEX IF NOT EXISTS "ux_comissao_extratos_filial_id_seguradora_id_identificacao_exte" ON erp."comissao_extratos" ("filial_id", "seguradora_id", "identificacao_externa");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_extrato_itens_extrato_id'
      AND conrelid = 'erp."comissao_extrato_itens"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_extrato_itens"
      ADD CONSTRAINT "fk_comissao_extrato_itens_extrato_id"
      FOREIGN KEY ("extrato_id")
      REFERENCES erp."comissao_extratos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_extrato_itens_produtor_id'
      AND conrelid = 'erp."comissao_extrato_itens"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_extrato_itens"
      ADD CONSTRAINT "fk_comissao_extrato_itens_produtor_id"
      FOREIGN KEY ("produtor_id")
      REFERENCES erp."produtores" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_extrato_itens_ramo_id'
      AND conrelid = 'erp."comissao_extrato_itens"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_extrato_itens"
      ADD CONSTRAINT "fk_comissao_extrato_itens_ramo_id"
      FOREIGN KEY ("ramo_id")
      REFERENCES erp."ramos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_comissao_extrato_itens_extrato_id_chave_idempotencia" ON erp."comissao_extrato_itens" ("extrato_id", "chave_idempotencia");

CREATE UNIQUE INDEX IF NOT EXISTS "ux_comissao_extrato_itens_extrato_id_identificacao_externa" ON erp."comissao_extrato_itens" ("extrato_id", "identificacao_externa");

CREATE UNIQUE INDEX IF NOT EXISTS "ux_comissao_extrato_itens_extrato_id_sequencia_externa" ON erp."comissao_extrato_itens" ("extrato_id", "sequencia_externa");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_conciliacoes_item_id'
      AND conrelid = 'erp."comissao_conciliacoes"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_conciliacoes"
      ADD CONSTRAINT "fk_comissao_conciliacoes_item_id"
      FOREIGN KEY ("item_id")
      REFERENCES erp."comissao_extrato_itens" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_conciliacoes_comissao_id'
      AND conrelid = 'erp."comissao_conciliacoes"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_conciliacoes"
      ADD CONSTRAINT "fk_comissao_conciliacoes_comissao_id"
      FOREIGN KEY ("comissao_id")
      REFERENCES erp."comissoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_conciliacoes_associado_por_id'
      AND conrelid = 'erp."comissao_conciliacoes"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_conciliacoes"
      ADD CONSTRAINT "fk_comissao_conciliacoes_associado_por_id"
      FOREIGN KEY ("associado_por_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_conciliacoes_confirmado_por_id'
      AND conrelid = 'erp."comissao_conciliacoes"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_conciliacoes"
      ADD CONSTRAINT "fk_comissao_conciliacoes_confirmado_por_id"
      FOREIGN KEY ("confirmado_por_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_comissao_conciliacoes_item_id_comissao_id" ON erp."comissao_conciliacoes" ("item_id", "comissao_id");

CREATE UNIQUE INDEX IF NOT EXISTS "ux_comissao_conciliacoes_item_id_chave_idempotencia" ON erp."comissao_conciliacoes" ("item_id", "chave_idempotencia");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_conciliacao_ocorrencias_item_id'
      AND conrelid = 'erp."comissao_conciliacao_ocorrencias"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_conciliacao_ocorrencias"
      ADD CONSTRAINT "fk_comissao_conciliacao_ocorrencias_item_id"
      FOREIGN KEY ("item_id")
      REFERENCES erp."comissao_extrato_itens" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_conciliacao_ocorrencias_conciliacao_id'
      AND conrelid = 'erp."comissao_conciliacao_ocorrencias"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_conciliacao_ocorrencias"
      ADD CONSTRAINT "fk_comissao_conciliacao_ocorrencias_conciliacao_id"
      FOREIGN KEY ("conciliacao_id")
      REFERENCES erp."comissao_conciliacoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_conciliacao_ocorrencias_identificada_por_id'
      AND conrelid = 'erp."comissao_conciliacao_ocorrencias"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_conciliacao_ocorrencias"
      ADD CONSTRAINT "fk_comissao_conciliacao_ocorrencias_identificada_por_id"
      FOREIGN KEY ("identificada_por_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_conciliacao_ocorrencias_resolvida_por_id'
      AND conrelid = 'erp."comissao_conciliacao_ocorrencias"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_conciliacao_ocorrencias"
      ADD CONSTRAINT "fk_comissao_conciliacao_ocorrencias_resolvida_por_id"
      FOREIGN KEY ("resolvida_por_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_baixas_comissao_id'
      AND conrelid = 'erp."comissao_baixas"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_baixas"
      ADD CONSTRAINT "fk_comissao_baixas_comissao_id"
      FOREIGN KEY ("comissao_id")
      REFERENCES erp."comissoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_baixas_baixa_origem_id'
      AND conrelid = 'erp."comissao_baixas"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_baixas"
      ADD CONSTRAINT "fk_comissao_baixas_baixa_origem_id"
      FOREIGN KEY ("baixa_origem_id")
      REFERENCES erp."comissao_baixas" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_baixas_criado_por_id'
      AND conrelid = 'erp."comissao_baixas"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_baixas"
      ADD CONSTRAINT "fk_comissao_baixas_criado_por_id"
      FOREIGN KEY ("criado_por_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_comissao_baixas_comissao_id_chave_idempotencia" ON erp."comissao_baixas" ("comissao_id", "chave_idempotencia");

CREATE INDEX IF NOT EXISTS "ix_comissao_baixas_baixa_origem_id" ON erp."comissao_baixas" ("baixa_origem_id");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_baixa_conciliacoes_baixa_id'
      AND conrelid = 'erp."comissao_baixa_conciliacoes"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_baixa_conciliacoes"
      ADD CONSTRAINT "fk_comissao_baixa_conciliacoes_baixa_id"
      FOREIGN KEY ("baixa_id")
      REFERENCES erp."comissao_baixas" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_comissao_baixa_conciliacoes_conciliacao_id'
      AND conrelid = 'erp."comissao_baixa_conciliacoes"'::regclass
  ) THEN
    ALTER TABLE erp."comissao_baixa_conciliacoes"
      ADD CONSTRAINT "fk_comissao_baixa_conciliacoes_conciliacao_id"
      FOREIGN KEY ("conciliacao_id")
      REFERENCES erp."comissao_conciliacoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_comissao_baixa_conciliacoes_baixa_id_conciliacao_id" ON erp."comissao_baixa_conciliacoes" ("baixa_id", "conciliacao_id");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasses_proposta_id'
      AND conrelid = 'erp."repasses"'::regclass
  ) THEN
    ALTER TABLE erp."repasses"
      ADD CONSTRAINT "fk_repasses_proposta_id"
      FOREIGN KEY ("proposta_id")
      REFERENCES erp."propostas" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasses_comissao_id'
      AND conrelid = 'erp."repasses"'::regclass
  ) THEN
    ALTER TABLE erp."repasses"
      ADD CONSTRAINT "fk_repasses_comissao_id"
      FOREIGN KEY ("comissao_id")
      REFERENCES erp."comissoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasses_beneficiario_id'
      AND conrelid = 'erp."repasses"'::regclass
  ) THEN
    ALTER TABLE erp."repasses"
      ADD CONSTRAINT "fk_repasses_beneficiario_id"
      FOREIGN KEY ("beneficiario_id")
      REFERENCES erp."produtores" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasses_regra_id'
      AND conrelid = 'erp."repasses"'::regclass
  ) THEN
    ALTER TABLE erp."repasses"
      ADD CONSTRAINT "fk_repasses_regra_id"
      FOREIGN KEY ("regra_id")
      REFERENCES erp."repasse_regras" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasse_recibos_filial_id'
      AND conrelid = 'erp."repasse_recibos"'::regclass
  ) THEN
    ALTER TABLE erp."repasse_recibos"
      ADD CONSTRAINT "fk_repasse_recibos_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasse_recibos_beneficiario_id'
      AND conrelid = 'erp."repasse_recibos"'::regclass
  ) THEN
    ALTER TABLE erp."repasse_recibos"
      ADD CONSTRAINT "fk_repasse_recibos_beneficiario_id"
      FOREIGN KEY ("beneficiario_id")
      REFERENCES erp."produtores" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasse_recibos_emitido_por_id'
      AND conrelid = 'erp."repasse_recibos"'::regclass
  ) THEN
    ALTER TABLE erp."repasse_recibos"
      ADD CONSTRAINT "fk_repasse_recibos_emitido_por_id"
      FOREIGN KEY ("emitido_por_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasse_recibos_cancelado_por_id'
      AND conrelid = 'erp."repasse_recibos"'::regclass
  ) THEN
    ALTER TABLE erp."repasse_recibos"
      ADD CONSTRAINT "fk_repasse_recibos_cancelado_por_id"
      FOREIGN KEY ("cancelado_por_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_repasse_recibos_filial_id_numero" ON erp."repasse_recibos" ("filial_id", "numero");

CREATE UNIQUE INDEX IF NOT EXISTS "ux_repasse_recibos_filial_id_chave_idempotencia" ON erp."repasse_recibos" ("filial_id", "chave_idempotencia");

CREATE INDEX IF NOT EXISTS "ix_repasse_recibos_beneficiario_id" ON erp."repasse_recibos" ("beneficiario_id");

CREATE INDEX IF NOT EXISTS "ix_repasse_recibos_status" ON erp."repasse_recibos" ("status");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasse_recibo_itens_recibo_id'
      AND conrelid = 'erp."repasse_recibo_itens"'::regclass
  ) THEN
    ALTER TABLE erp."repasse_recibo_itens"
      ADD CONSTRAINT "fk_repasse_recibo_itens_recibo_id"
      FOREIGN KEY ("recibo_id")
      REFERENCES erp."repasse_recibos" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_repasse_recibo_itens_repasse_id'
      AND conrelid = 'erp."repasse_recibo_itens"'::regclass
  ) THEN
    ALTER TABLE erp."repasse_recibo_itens"
      ADD CONSTRAINT "fk_repasse_recibo_itens_repasse_id"
      FOREIGN KEY ("repasse_id")
      REFERENCES erp."repasses" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_repasse_recibo_itens_recibo_id_repasse_id" ON erp."repasse_recibo_itens" ("recibo_id", "repasse_id");

CREATE INDEX IF NOT EXISTS "ix_repasse_recibo_itens_repasse_id" ON erp."repasse_recibo_itens" ("repasse_id");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_atividades_tenant_id'
      AND conrelid = 'erp."atividades"'::regclass
  ) THEN
    ALTER TABLE erp."atividades"
      ADD CONSTRAINT "fk_atividades_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_atividades_filial_id'
      AND conrelid = 'erp."atividades"'::regclass
  ) THEN
    ALTER TABLE erp."atividades"
      ADD CONSTRAINT "fk_atividades_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_atividades_responsavel_id'
      AND conrelid = 'erp."atividades"'::regclass
  ) THEN
    ALTER TABLE erp."atividades"
      ADD CONSTRAINT "fk_atividades_responsavel_id"
      FOREIGN KEY ("responsavel_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_atividade_mencoes_atividade_id'
      AND conrelid = 'erp."atividade_mencoes"'::regclass
  ) THEN
    ALTER TABLE erp."atividade_mencoes"
      ADD CONSTRAINT "fk_atividade_mencoes_atividade_id"
      FOREIGN KEY ("atividade_id")
      REFERENCES erp."atividades" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_atividade_mencoes_profile_id'
      AND conrelid = 'erp."atividade_mencoes"'::regclass
  ) THEN
    ALTER TABLE erp."atividade_mencoes"
      ADD CONSTRAINT "fk_atividade_mencoes_profile_id"
      FOREIGN KEY ("profile_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_anexos_tenant_id'
      AND conrelid = 'erp."anexos"'::regclass
  ) THEN
    ALTER TABLE erp."anexos"
      ADD CONSTRAINT "fk_anexos_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_anexos_filial_id'
      AND conrelid = 'erp."anexos"'::regclass
  ) THEN
    ALTER TABLE erp."anexos"
      ADD CONSTRAINT "fk_anexos_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_audit_logs_tenant_id'
      AND conrelid = 'erp."audit_logs"'::regclass
  ) THEN
    ALTER TABLE erp."audit_logs"
      ADD CONSTRAINT "fk_audit_logs_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_audit_logs_user_id'
      AND conrelid = 'erp."audit_logs"'::regclass
  ) THEN
    ALTER TABLE erp."audit_logs"
      ADD CONSTRAINT "fk_audit_logs_user_id"
      FOREIGN KEY ("user_id")
      REFERENCES erp."profiles" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_integracao_logs_tenant_id'
      AND conrelid = 'erp."integracao_logs"'::regclass
  ) THEN
    ALTER TABLE erp."integracao_logs"
      ADD CONSTRAINT "fk_integracao_logs_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_campo_definicoes_tenant_id'
      AND conrelid = 'erp."campo_definicoes"'::regclass
  ) THEN
    ALTER TABLE erp."campo_definicoes"
      ADD CONSTRAINT "fk_campo_definicoes_tenant_id"
      FOREIGN KEY ("tenant_id")
      REFERENCES erp."tenants" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_campo_definicoes_filial_id'
      AND conrelid = 'erp."campo_definicoes"'::regclass
  ) THEN
    ALTER TABLE erp."campo_definicoes"
      ADD CONSTRAINT "fk_campo_definicoes_filial_id"
      FOREIGN KEY ("filial_id")
      REFERENCES erp."filiais" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_campo_definicoes_tenant_id_entidade_tipo_chave" ON erp."campo_definicoes" ("tenant_id", "entidade_tipo", "chave");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_campo_opcoes_campo_definicao_id'
      AND conrelid = 'erp."campo_opcoes"'::regclass
  ) THEN
    ALTER TABLE erp."campo_opcoes"
      ADD CONSTRAINT "fk_campo_opcoes_campo_definicao_id"
      FOREIGN KEY ("campo_definicao_id")
      REFERENCES erp."campo_definicoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_campo_valores_campo_definicao_id'
      AND conrelid = 'erp."campo_valores"'::regclass
  ) THEN
    ALTER TABLE erp."campo_valores"
      ADD CONSTRAINT "fk_campo_valores_campo_definicao_id"
      FOREIGN KEY ("campo_definicao_id")
      REFERENCES erp."campo_definicoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_campo_valores_valor_opcao_id'
      AND conrelid = 'erp."campo_valores"'::regclass
  ) THEN
    ALTER TABLE erp."campo_valores"
      ADD CONSTRAINT "fk_campo_valores_valor_opcao_id"
      FOREIGN KEY ("valor_opcao_id")
      REFERENCES erp."campo_opcoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_campo_valores_campo_definicao_id_entidade_id" ON erp."campo_valores" ("campo_definicao_id", "entidade_id");

CREATE INDEX IF NOT EXISTS "ix_campo_valores_entidade_id" ON erp."campo_valores" ("entidade_id");

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_campo_valor_opcoes_campo_valor_id'
      AND conrelid = 'erp."campo_valor_opcoes"'::regclass
  ) THEN
    ALTER TABLE erp."campo_valor_opcoes"
      ADD CONSTRAINT "fk_campo_valor_opcoes_campo_valor_id"
      FOREIGN KEY ("campo_valor_id")
      REFERENCES erp."campo_valores" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'fk_campo_valor_opcoes_campo_opcao_id'
      AND conrelid = 'erp."campo_valor_opcoes"'::regclass
  ) THEN
    ALTER TABLE erp."campo_valor_opcoes"
      ADD CONSTRAINT "fk_campo_valor_opcoes_campo_opcao_id"
      FOREIGN KEY ("campo_opcao_id")
      REFERENCES erp."campo_opcoes" ("id")
      ON DELETE RESTRICT;
  END IF;
END
$migration$;

CREATE UNIQUE INDEX IF NOT EXISTS "ux_campo_valor_opcoes_campo_valor_id_campo_opcao_id" ON erp."campo_valor_opcoes" ("campo_valor_id", "campo_opcao_id");

INSERT INTO erp.schema_contract_versions(version, source_file)
VALUES ('3.1.0', 'wassis_erp_esqueleto_v3_1.dbml')
ON CONFLICT (version) DO NOTHING;
