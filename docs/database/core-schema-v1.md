# Core schema v1

## Source of truth

`wassis_erp_esqueleto_v1_0.dbml` is the versioned structural contract received from the product design. The generated PostgreSQL migration lives in the `erp` schema so the canonical model can be introduced without breaking the legacy tables currently used by API endpoints.

Regenerate the SQL after changing the DBML:

```powershell
node tools/generate-erp-core-schema.mjs
```

DBML and generated SQL must be committed together. Structural breaking changes require a new major contract version; additive tables and nullable columns require a minor version.

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

The existing `public` tables remain operational during the transition. New endpoints should target the canonical `erp` schema. Legacy modules can then be migrated independently, avoiding a flag day for customers, opportunities, quotes and policy drafts.
