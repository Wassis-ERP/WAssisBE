import { readFileSync, writeFileSync } from 'node:fs'
import { resolve } from 'node:path'

const inputPath = resolve(process.argv[2] ?? 'docs/database/wassis_erp_esqueleto_v3_1.dbml')
const outputPath = resolve(process.argv[3] ?? 'src/WAssis.Infra.Data/Migrations/Sql/20260912120000_AlignErpContractV31.sql')
const contractVersion = process.argv[4] ?? '3.1.0'
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
  if (/^indexes\s*\{$/i.test(line)) {
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

  const columnMatch = line.match(/^([A-Za-z_][A-Za-z0-9_]*)\s+(uuid|text|numeric|integer|date|timestamptz|boolean)(?:\s+\[([^\]]+)\])?$/)
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
  integer: 'integer',
  date: 'date',
  timestamptz: 'timestamp with time zone',
  boolean: 'boolean',
})[type]
const identifier = (prefix, tableName, columns) =>
  `${prefix}_${tableName}_${columns.join('_')}`.slice(0, 63)

const statements = [
  '-- Additive upgrade generated from docs/database/wassis_erp_esqueleto_v3_1.dbml.',
  '-- Existing columns are preserved so this migration is safe for databases already using the v1/v1.1 transition schema.',
  '-- New tables receive the DBML primary-key and not-null constraints; destructive renames and drops require a later data migration.',
  'CREATE SCHEMA IF NOT EXISTS erp;',
  `COMMENT ON SCHEMA erp IS 'W.Assis ERP canonical contract v${contractVersion}';`,
]

for (const current of tables) {
  const createColumns = current.columns.map((column) => {
    const parts = [quote(column.name), sqlType(column.type)]
    if (column.primaryKey) parts.push('PRIMARY KEY')
    if (column.required || column.primaryKey) parts.push('NOT NULL')
    return `  ${parts.join(' ')}`
  })

  statements.push(`CREATE TABLE IF NOT EXISTS erp.${quote(current.name)} (\n${createColumns.join(',\n')}\n);`)

  for (const column of current.columns) {
    statements.push(
      `ALTER TABLE erp.${quote(current.name)} ADD COLUMN IF NOT EXISTS ${quote(column.name)} ${sqlType(column.type)};`,
    )
  }
}

for (const current of tables) {
  for (const column of current.columns.filter((item) => item.reference)) {
    const constraintName = identifier('fk', current.name, [column.name])
    statements.push(`DO $migration$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = '${constraintName}'
      AND conrelid = 'erp.${quote(current.name)}'::regclass
  ) THEN
    ALTER TABLE erp.${quote(current.name)}
      ADD CONSTRAINT ${quote(constraintName)}
      FOREIGN KEY (${quote(column.name)})
      REFERENCES erp.${quote(column.reference.table)} (${quote(column.reference.column)})
      ON DELETE RESTRICT;
  END IF;
END
$migration$;`)
  }

  const indexes = [...current.indexes]
  for (const column of current.columns.filter((item) => item.unique)) {
    indexes.push({ columns: [column.name], unique: true })
  }

  const seen = new Set()
  for (const index of indexes) {
    const key = `${index.unique ? 'unique' : 'index'}:${index.columns.join('|')}`
    if (seen.has(key)) continue
    seen.add(key)
    const indexName = identifier(index.unique ? 'ux' : 'ix', current.name, index.columns)
    statements.push(
      `CREATE ${index.unique ? 'UNIQUE ' : ''}INDEX IF NOT EXISTS ${quote(indexName)} ` +
      `ON erp.${quote(current.name)} (${index.columns.map(quote).join(', ')});`,
    )
  }
}

statements.push(`INSERT INTO erp.schema_contract_versions(version, source_file)
VALUES ('${contractVersion}', 'wassis_erp_esqueleto_v3_1.dbml')
ON CONFLICT (version) DO NOTHING;`)

writeFileSync(outputPath, `${statements.join('\n\n')}\n`, 'utf8')
console.log(`Generated additive v${contractVersion} upgrade for ${tables.length} tables in ${outputPath}`)
