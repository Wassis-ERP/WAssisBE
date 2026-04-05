# Document for Insurers

## W.Assis - Business Plan, Auditability and Security

## 1. Business overview

W.Assis is building a digital insurance brokerage platform with two complementary fronts:

- operation of its own digital brokerage
- commercialization of the brokerage ERP for other brokerages

The backend was designed to support both internal operations and B2B expansion while preserving brokerage segregation, operational traceability and secure partner integrations.

## 2. Purpose of insurer integrations

The insurer APIs are intended to support:

- multi-quote orchestration
- proposal progression up to issuance
- document retrieval and processing
- operational automation with traceability

Each insurer is isolated behind its own integration adapter so that carrier-specific behavior does not contaminate the core business domain.

## 3. Platform architecture

The platform follows a modular monolith architecture with clear separation between:

- `Domain`
- `Application`
- `Infra.Data`
- `Infra.CrossCutting`
- `Services.Api`
- `BackgroundTasks`

Current business modules include:

- `Identity`
- `Customers`
- `Billing`
- `Quotes`
- `Policies`
- `Claims`
- `Documents`
- `Financial`
- `Notifications`
- `WhatsAppSupport`

This design supports partner isolation, maintainability and controlled operational growth.

## 4. Data segregation and protection

The platform already includes a multi-tenant foundation to isolate brokerages.

Current controls:

- `TenantId` propagated through central aggregates
- global tenant filters in the EF Core context
- claims carrying brokerage and user context
- separation between internal operation, ERP customers and end customers

This model is intended to prevent cross-brokerage data exposure and support future shared or dedicated deployment strategies according to contractual needs.

## 5. Authentication and authorization

The backend uses JWT-based authentication and policy-based authorization.

Current controls:

- token-based authentication
- claims such as `tenant_id`, `brokerage_id`, `seller_id` and `user_type`
- role separation by user type
- dedicated policies for brokerage staff, brokerage admins and digital customers

Controllers remain thin and business rules stay concentrated in domain and application layers.

## 6. Auditability and traceability

Critical flows are designed with traceability from origin to final processing.

Current controls:

- `CorrelationId` in relevant business flows
- persisted audit trail per tenant, module, action and entity
- linkage between operational actions and affected entities
- support for human review in document and financial flows

This model supports investigation, reconciliation and partner-related operational audits.

## 7. Operational security

Current principles and controls:

- no real secrets are stored in the repository
- no clear-text PII should be logged in operational logs
- critical writes aim to be idempotent
- sensitive flows persist state before external integration
- internal endpoints are protected with authorization policies
- CI validates build and tests on pull requests and pushes
- external HTTP integrations use timeout, retry and circuit breaker controls

## 8. SQL injection protection

The persistence layer currently relies primarily on EF Core with LINQ-based queries.

Protection model:

- avoid manual SQL concatenation
- prefer ORM-safe queries and parameterization
- review any future Dapper or raw SQL usage before production

No active raw SQL interpolation pattern was identified in the reviewed central flows.

## 9. Document handling

The platform already supports:

- controlled document upload
- text extraction
- OCR fallback
- insurer-aware parsing profiles
- human review when parsing confidence is low

This reduces operational loss and improves governance over documents used in proposal, issuance and reconciliation flows.

## 10. Financial and commission flow

The financial model follows the brokerage operating model:

1. the insured pays the premium to the insurer
2. the insurer processes the payment
3. the insurer transfers the commission to the brokerage
4. the brokerage records and reconciles that transfer in the ERP

Therefore, the `Financial` module is focused on:

- commission receipts
- expected versus received divergence
- reconciliation
- documentary evidence
- operational auditability

This module is intentionally separate from `Billing`, which only handles SaaS charging for ERP customers.

## 11. Security and governance roadmap

Planned next steps:

- move secrets and credentials to a dedicated vault
- strengthen JWT configuration enforcement by environment
- adopt signed webhook validation for future callbacks
- expand operational dashboards and audit coverage
- support higher isolation strategies for enterprise customers when required

## 12. Executive summary

W.Assis is building a platform focused on:

- operational scalability
- secure insurer integrations
- brokerage-level segregation
- auditability and traceability
- security through identity, claims, authorization and data isolation

The goal is to operate with governance compatible with corporate integrations while enabling sustainable growth of both the digital brokerage and the ERP product.
