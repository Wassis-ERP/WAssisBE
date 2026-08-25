# Security assessment - 2026-08-25

## Scope

Review of the backend and CRM delivery paths, covering authentication, authorization, tenant and branch isolation, database integrity, uploads, secrets, dependencies, containers, and automatic deployment.

## Findings addressed

- API endpoints now require authentication by default. Login and health checks are the only explicit anonymous routes.
- Local username/password authentication cannot run outside `Development`.
- Invalid credentials return `401`, and login attempts are rate-limited per remote address.
- Production processes fail fast when `ConnectionStrings:DefaultConnection` is absent instead of using local PostgreSQL credentials.
- Quote requests enforce tenant and branch query filters. Restricted users cannot read another branch or unscoped quote requests.
- The ERP schema adds tenant consistency triggers for profiles, producers, branch managers, contacts, calculations, receipt grades, and transfer rules.
- Catalog SQL uses a fixed server-side resource allowlist and Dapper parameters. User input is never interpolated into SQL identifiers or predicates.
- Proposal uploads require a `.pdf` extension, `application/pdf`, a 20 MB maximum size, and a PDF signature.
- The API container runs as the unprivileged .NET runtime user.
- The original ERP schema generator no longer emits duplicate index statements.
- NuGet and npm audits found no known vulnerable dependencies at review time.
- Repository secret-pattern review found placeholders and local development defaults only, with no tracked production credential.

## Deployment finding

The image build and GHCR push complete successfully. Portainer then fails to pull the private image with `unauthorized`. Configure a `ghcr.io` registry credential in Portainer using a GitHub user and a token with `read:packages`, and associate it with the service or stack. Repository workflow changes cannot install that credential in Portainer.

## Residual risks

- Production authentication still depends on the planned external identity provider and MFA rollout; the repository login is development-only.
- The CRM currently persists its bearer token in browser storage. The CSP reduces script injection exposure, but an HttpOnly refresh-cookie flow is the preferred production design.
- GitHub secret scanning and Dependabot alerts must remain enabled at repository or organization level for continuous coverage.
- The PostgreSQL migrations were compiled and rendered as an idempotent script, but a live PostgreSQL execution was not available in the local review environment.
- Portainer registry credentials, production secrets, and insurer credentials must be managed outside the repositories.

## Verification

- Release build and automated tests
- formatting verification
- idempotent EF migration script generation
- dependency vulnerability audit
- controller authorization inventory
- dangerous API and secret-pattern search
- duplicate database index detection
- GitHub CodeQL and CI checks on the pull request
