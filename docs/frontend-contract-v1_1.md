# CRM contract v1.1

This increment aligns the backend baseline with the WassisCRM hand-off merged through frontend PR #40.

## Runtime endpoints

- `GET /api/core/schema`: current schema contract and authenticated tenant/branch scope.
- `GET /api/core/branches`: branches available to the authenticated identity.
- `GET /api/core/catalogs`: catalog resources currently available to brokerage administrators.
- `GET /api/core/catalogs/{resource}?activeOnly=true&search=`: tenant-scoped administrator catalog rows.
- `POST /api/quotes/requests`: creates an auto calculation round; it now accepts `officeBranchId`, `opportunityId`, `insuranceBranchId`, `insuredPersonId`, `calculationType`, `calculationOrigin`, and `versionLabel`.
- `GET /api/quotes/requests?opportunityId=&officeBranchId=`: lists calculation versions and insurer results visible in the authenticated branch scope.

Supported read catalogs are `filiais`, `perfis`, `produtores`, `seguradoras`, `ramos`, `origens`, `motivos_perda`, `coberturas_catalogo`, `pipelines`, `pipeline_stages`, `recebimento_grades`, `recebimento_grade_parcelas`, `repasse_regras`, `campo_definicoes`, and `campo_opcoes`.

Catalog writes remain intentionally outside this increment. They require command-specific validation and audit records; the frontend can replace its mock reads first without weakening the write model.

## Multicalculation mapping

- A calculation round/version is represented at runtime by `quotes.quote_requests`.
- Insurer responses are represented by `quotes.quote_options`.
- `opportunityId` groups multiple versions under the same commercial opportunity.
- `officeBranchId` is validated against JWT branch claims and participates in the EF query filter.
- The canonical ERP contract remains `erp.calculos` plus one `erp.calc_*` specialization and `erp.cotacoes`.

This transitional mapping reuses the provider engine already implemented. A later data migration can materialize runtime quote requests into the canonical ERP tables without changing the frontend contract.

## Security guarantees

- All API routes require authentication by default; health checks and login are explicit exceptions.
- Catalog reads always derive tenant and branch scope from authenticated claims.
- Cross-tenant references in the new ERP contract are rejected by database triggers.
- Local development authentication cannot be enabled outside the Development environment.
- Login attempts are rate-limited and uploaded proposal PDFs are size, MIME, extension, and signature checked.
