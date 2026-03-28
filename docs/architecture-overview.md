# Arquitetura do ERP W.Assis

## Stack base

- `.NET 8`
- `Clean Architecture + DDD + CQRS`
- `EF Core + PostgreSQL`
- `MassTransit + RabbitMQ`
- `MediatR + FluentValidation`
- `Serilog + OpenTelemetry`

## Solução

- `WAssis.Domain.Core`
- `WAssis.Domain`
- `WAssis.Application`
- `WAssis.Infra.Data`
- `WAssis.Infra.CrossCutting.Bus`
- `WAssis.Infra.CrossCutting.Identity`
- `WAssis.Infra.CrossCutting.IoC`
- `WAssis.Services.Api`
- `WAssis.BackgroundTasks`
- `WAssis.UI.Web`
- `WAssis.Tests`

## Princípios operacionais

- `CorrelationId` em todos os fluxos relevantes
- idempotência em escrita sensível
- processamento assíncrono com persistência antes da integração
- auditoria por módulo e ação
- sem logar PII em claro

## Estado atual

### Backend já implementado

- esqueleto funcional de `Quotes`
- primeiro provider real de `Quotes` baseado na API da `Justos`
- fluxo de `Documents` com upload de proposta PDF
- extração por camada textual com fallback de OCR configurável via `Tesseract`
- parser de proposta com perfil por seguradora e fallback genérico
- criação de `PolicyDraft` a partir de documento parseado
- progressão de `PolicyDraft` até `Issued`
- `Financial` e `Documents` persistidos com EF
- visão operacional via `GET /api/operations/dashboard`
- trilha de auditoria em `operations.audit_entries`
- canal inicial de `WhatsAppSupport`

### Integrações em espera por documentação ou mapeamento detalhado

- ampliação de providers além da `Justos`
- integrações documentais automáticas por seguradora
- emissão externa por seguradora

## Endpoints canônicos já disponíveis

- `POST /api/quotes/requests`
- `GET /api/quotes/requests/{id}`
- `GET /api/quotes/requests/{id}/results`
- `GET /api/quotes/providers`
- `POST /api/documents/proposals/uploads`
- `GET /api/documents/proposals/{id}`
- `POST /api/policies/drafts/from-document`
- `GET /api/policies/drafts/{id}`
- `POST /api/policies/drafts/{id}/ready`
- `POST /api/policies/drafts/{id}/issue`
- `GET /api/operations/dashboard`
- `POST /api/whatsapp/conversations/inbound`
- `GET /api/whatsapp/conversations/{id}`

## Observabilidade do multicálculo

- `GET /api/quotes/providers` expõe o estado operacional dos providers de cotação
- cada provider informa:
  - se está habilitado
  - se está pronto de fato
  - modo de autenticação esperado
  - documentação oficial
  - requisitos faltantes de configuração

Isso dá visibilidade para backend, frontend e operação sem depender de leitura manual de `appsettings`.
