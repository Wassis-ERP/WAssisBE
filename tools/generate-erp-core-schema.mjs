import { readFileSync, writeFileSync } from 'node:fs'
import { resolve } from 'node:path'

const inputPath = resolve(process.argv[2] ?? 'docs/database/wassis_erp_esqueleto_v1_0.dbml')
const outputPath = resolve(process.argv[3] ?? 'src/WAssis.Infra.Data/Migrations/Sql/20260619213000_CreateErpCoreV1Schema.sql')
const source = readFileSync(inputPath, 'utf8')

const tables = []
let table = null
let inIndexes = false

for (const rawLine of source.split(/\r?\n/)) {
  const line = rawLine.replace(/\/\/.*$/, '').trim()
  if (!line) continue

  const tableStart = line.match(/^Table\s+([A-Za-z_][A-Za-z0-9_]*)\s*\{$/)
  if (tableStart) {
    table = { name: tableStart[1], columns: [], indexes: [] }
    tables.push(table)
    continue
  }

  if (!table) continue
  if (line === 'Indexes {') {
    inIndexes = true
    continue
  }
  if (line === '}') {
    if (inIndexes) {
      inIndexes = false
    } else {
      table = null
    }
    continue
  }

  if (inIndexes) {
    const indexMatch = line.match(/^(?:\(([^)]+)\)|([A-Za-z_][A-Za-z0-9_]*))(?:\s+\[([^\]]+)\])?$/)
    if (!indexMatch) continue
    const columns = (indexMatch[1] ?? indexMatch[2]).split(',').map((value) => value.trim())
    table.indexes.push({ columns, unique: indexMatch[3]?.includes('unique') ?? false })
    continue
  }

  const columnMatch = line.match(/^([A-Za-z_][A-Za-z0-9_]*)\s+(uuid|text|numeric|date|timestamptz|boolean)(?:\s+\[([^\]]+)\])?$/)
  if (!columnMatch) continue

  const attributes = columnMatch[3] ?? ''
  const reference = attributes.match(/ref:\s*[>\-<]\s*([A-Za-z_][A-Za-z0-9_]*)\.([A-Za-z_][A-Za-z0-9_]*)/)
  table.columns.push({
    name: columnMatch[1],
    type: columnMatch[2],
    primaryKey: /(?:^|,)\s*pk\s*(?:,|$)/.test(attributes),
    required: /not null/.test(attributes),
    unique: /(?:^|,)\s*unique\s*(?:,|$)/.test(attributes),
    reference: reference ? { table: reference[1], column: reference[2] } : null,
  })
}

if (tables.length === 0) throw new Error(`No DBML tables found in ${inputPath}`)

const quote = (value) => `"${value.replaceAll('"', '""')}"`
const sqlType = (type) => ({
  uuid: 'uuid',
  text: 'text',
  numeric: 'numeric',
  date: 'date',
  timestamptz: 'timestamp with time zone',
  boolean: 'boolean',
})[type]
const indexName = (prefix, tableName, columns) =>
  `${prefix}_${tableName}_${columns.join('_')}`.slice(0, 63)

const statements = [
  '-- Generated from docs/database/wassis_erp_esqueleto_v1_0.dbml.',
  '-- Do not edit manually; update the DBML and rerun tools/generate-erp-core-schema.mjs.',
  'CREATE SCHEMA IF NOT EXISTS erp;',
  "COMMENT ON SCHEMA erp IS 'W.Assis ERP core schema contract v1.0';",
]

for (const current of tables) {
  const columnSql = current.columns.map((column) => {
    const parts = [quote(column.name), sqlType(column.type)]
    if (column.primaryKey) parts.push('PRIMARY KEY')
    if (column.required || column.primaryKey) parts.push('NOT NULL')
    if (column.unique) parts.push('UNIQUE')
    return `  ${parts.join(' ')}`
  })
  statements.push(`CREATE TABLE erp.${quote(current.name)} (\n${columnSql.join(',\n')}\n);`)
}

statements.push(`
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
ON erp.role_permissions (papel, modulo);`)

for (const current of tables) {
  for (const column of current.columns.filter((item) => item.reference)) {
    const constraintName = `fk_${current.name}_${column.name}`.slice(0, 63)
    statements.push(
      `ALTER TABLE erp.${quote(current.name)} ADD CONSTRAINT ${quote(constraintName)} ` +
      `FOREIGN KEY (${quote(column.name)}) REFERENCES erp.${quote(column.reference.table)} (${quote(column.reference.column)}) ON DELETE RESTRICT;`,
    )
  }

  const indexedKeys = new Set()
  for (const index of current.indexes) {
    const key = index.columns.join('|')
    indexedKeys.add(key)
    statements.push(
      `CREATE ${index.unique ? 'UNIQUE ' : ''}INDEX ${quote(indexName(index.unique ? 'ux' : 'ix', current.name, index.columns))} ` +
      `ON erp.${quote(current.name)} (${index.columns.map(quote).join(', ')});`,
    )
  }

  for (const column of current.columns.filter((item) => item.reference)) {
    if (indexedKeys.has(column.name) || column.primaryKey || column.unique) continue
    indexedKeys.add(column.name)
    statements.push(
      `CREATE INDEX ${quote(indexName('ix', current.name, [column.name]))} ` +
      `ON erp.${quote(current.name)} (${quote(column.name)});`,
    )
  }

  for (const scopeColumn of ['tenant_id', 'filial_id']) {
    const column = current.columns.find((item) => item.name === scopeColumn)
    if (!column || indexedKeys.has(scopeColumn)) continue
    statements.push(
      `CREATE INDEX ${quote(indexName('ix', current.name, [scopeColumn]))} ` +
      `ON erp.${quote(current.name)} (${quote(scopeColumn)});`,
    )
  }
}

const tenantBranchTables = tables
  .filter((current) => current.columns.some((column) => column.name === 'tenant_id') && current.columns.some((column) => column.name === 'filial_id'))
  .map((current) => current.name)

statements.push(`
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
$$;`)

for (const tableName of tenantBranchTables) {
  statements.push(
    `CREATE TRIGGER ${quote(`trg_${tableName}_filial_tenant`.slice(0, 63))} ` +
    `BEFORE INSERT OR UPDATE OF tenant_id, filial_id ON erp.${quote(tableName)} ` +
    'FOR EACH ROW EXECUTE FUNCTION erp.enforce_filial_tenant_match();',
  )
}

statements.push(`
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
VALUES ('1.0.0', 'wassis_erp_esqueleto_v1_0.dbml');`)

writeFileSync(outputPath, `${statements.join('\n\n')}\n`, 'utf8')
console.log(`Generated ${tables.length} tables in ${outputPath}`)
