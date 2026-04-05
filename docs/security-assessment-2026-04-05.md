# Security Assessment - 2026-04-05

## Scope

Review of the current W.Assis backend security posture with focus on:

- authentication and authorization
- multi-tenant isolation
- auditability
- endpoint protection
- sensitive configuration
- external integration resilience
- secret exposure

## Current strengths

- JWT authentication is already part of the platform
- access policies exist for `AuthenticatedUser`, `BrokerageStaff`, `BrokerageAdmin` and `DigitalCustomer`
- core claims already include `tenant_id`, `brokerage_id`, `seller_id` and `user_type`
- tenant filtering is enforced at `DbContext` level for central aggregates
- audit trail is persisted with tenant, module, action and entity context
- `CorrelationId` is used in relevant operational flows
- CI already validates build and tests on PRs and pushes to `main`
- the API host now applies baseline security headers
- the API host no longer exposes the Kestrel `Server` header
- outbound insurer HTTP clients now use timeout, retry and circuit breaker policies

## Improvements applied in this review

- protected `DocumentSearchesController` with `BrokerageStaff`
- added startup validation for JWT settings by environment
- replaced repository JWT key placeholder with an explicit non-production marker
- hardened JWT token validation parameters
- added baseline security headers middleware
- added outbound HTTP resilience with timeout, retry and circuit breaker

## SQL injection

### Current status

- the current backend relies primarily on `EF Core` with LINQ for persistence
- no active use of `FromSqlRaw`, `ExecuteSqlRaw`, `SqlCommand` or manual SQL string concatenation was found in the reviewed paths
- `Dapper` is present as a package, but no active runtime usage was found in the current code reviewed here

### Practical reading

The current codebase is in a good position regarding SQL injection because:

- central flows are implemented through EF Core repositories
- user input is not being interpolated into SQL in the reviewed flows
- controllers remain thin and route input through dedicated commands and handlers

### Required rule for future work

If Dapper or raw SQL is introduced:

- always use parameterized queries
- never concatenate user-provided text into SQL
- explicitly review every report or dashboard query before production

## Authentication and authorization

### Strong points

- JWT issuer, audience and signing key are validated
- token lifetime and signed token requirements are now explicit
- claims and roles are separated by user type
- most sensitive business endpoints already require policy-based authorization

### Remaining gaps

- production configuration still depends on proper external secret injection
- further hardening is still needed for operational admin flows and future external callbacks

## Multi-tenant isolation

### Strong points

- tenant scope is derived from authenticated context
- central aggregates are filtered by tenant in the EF model
- tenant-aware writes are already part of handlers and repositories

### Residual risk

- every new aggregate or read model must continue following the same pattern
- cross-tenant reporting and platform admin flows will need explicit bypass rules and review

## Secrets and sensitive data

### Current position

- the repository no longer carries a pretend "real-looking" JWT key; it now uses an explicit placeholder
- insurer credentials are represented as configuration placeholders
- no real secret should be committed to the repository

### Next step

- move JWT keys, insurer credentials, billing secrets and private keys to a secret manager or vault per environment

## External resilience and circuit breaker

### Current position

Outbound HTTP integrations now have:

- per-attempt timeout
- total request timeout
- retry with bounded attempts
- circuit breaker with sampling window, minimum throughput and break duration

### Why it matters

This helps prevent:

- cascading failures when an insurer degrades
- thread and socket exhaustion from hanging requests
- uncontrolled retry storms

## Priority actions

### High priority

1. Move all production secrets to a vault or secret manager
2. Enforce production-safe JWT values in deployment pipelines
3. Expand audit coverage for sensitive administrative and integration failure events

### Medium priority

4. Add richer health checks for database, queue, OCR and external dependencies
5. Add structured security review gates for any future Dapper or raw SQL usage
6. Add stronger operational guidance for webhook signature validation when billing and partner callbacks go live

### Low priority

7. Maintain a deployment checklist covering HTTPS, secrets, headers, monitoring and environment validation

## Conclusion

The platform already has a solid security base for its current stage, especially in:

- tenant segregation
- policy-based authorization
- auditability
- endpoint protection
- outbound resilience

The biggest next steps are:

- secret management
- richer health checks and observability
- expanded audit coverage
- operational hardening for future external callbacks
