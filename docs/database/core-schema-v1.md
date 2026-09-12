# Core schema v1

## Source of truth

`wassis_erp_esqueleto_v1_0.dbml` is the original structural baseline. The current product contract is `wassis_erp_esqueleto_v3_1.dbml`, synchronized from the frontend hand-off completed on 2026-09-11. Generated PostgreSQL migrations live in the `erp` schema so the canonical model can be introduced without breaking the legacy tables currently used by API endpoints.

Regenerate the SQL after changing the DBML:

```powershell
node tools/generate-erp-core-schema.mjs
```

Generate the additive v3.1 upgrade with:

```powershell
node tools/generate-erp-contract-upgrade.mjs
```

DBML, generator and generated SQL must be committed together. The v3.1 upgrade only creates missing tables, columns, foreign keys and indexes; it intentionally keeps transitional columns already used by `public.*` and by the v1/v1.1 `erp.*` schema. Renames, drops, backfills and stricter nullability require a later expand/backfill/contract migration after the runtime modules move to the canonical schema.

## Tenant model

- `tenant` is the insurance group using the SaaS.
- `filial` is a brokerage/legal entity inside the group and is the unit of data isolation.
- users belong to a tenant through `profiles` and receive branch access through `profile_filiais`.
- records rooted at a branch carry both `tenant_id` and `filial_id`; the migration installs triggers that reject a branch from another tenant.
- `profile_filiais` rejects profile/branch relationships across tenants and has a unique `(profile_id, filial_id)` index.
- child tables derive tenant and branch through required foreign keys, following rules R1 and R6 in the DBML.

Application endpoints must continue applying the authenticated tenant and allowed branch set. Direct client access to PostgreSQL is not supported.

## API bootstrap

Authenticated frontends can call `GET /api/core/schema` to obtain:

- contract version and PostgreSQL schema;
- authenticated tenant and active branch;
- all allowed branches;
- module and table catalog used by the frontend rollout.

The endpoint is metadata only. Business fields and CRUD endpoints are added module by module as the screens are designed.

`GET /api/core/branches` returns only branches from the authenticated tenant and intersects them with the `branch_ids` claim unless the user has group-wide branch access. It is the canonical source for the CRM branch selector.

## Migration strategy

The existing `public` tables remain operational during the transition. Current Segurados and Oportunidades endpoints still use those tables, with additive EF columns matching the connected frontend DTO. New modules should target the canonical `erp` schema. Legacy modules can then be migrated independently, avoiding a flag day for customers, opportunities, quotes and policy drafts.
