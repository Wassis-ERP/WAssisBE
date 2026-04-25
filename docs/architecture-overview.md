# Arquitetura do ERP W.Assis

## Stack base

- `.NET 8`
- `Clean Architecture + DDD + CQRS`
- `EF Core + PostgreSQL`
- `MassTransit + RabbitMQ`
- `MediatR + FluentValidation`
- `Serilog + OpenTelemetry`

## Solucao

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

## Principios operacionais

- `CorrelationId` em todos os fluxos relevantes
- idempotencia em escrita sensivel
- processamento assincrono com persistencia antes da integracao
- auditoria por modulo e acao
- sem logar PII em claro
- isolamento multi-tenant nos agregados centrais

## Estado atual

### Backend ja implementado

- esqueleto funcional de `Quotes`
- primeiro provider real de `Quotes` baseado na API da `Justos`
- catalogo operacional de seguradoras em `GET /api/quotes/providers`
- fluxo de `Documents` com upload de proposta PDF
- extracao por camada textual com fallback de OCR configuravel via `Tesseract`
- parser de proposta com perfil por seguradora e fallback generico
- criacao de `PolicyDraft` a partir de documento parseado
- progressao de `PolicyDraft` ate `Issued`
- `Billing` separado de `Financial` para assinatura e faturamento das corretoras clientes do ERP
- `Financial` e `Documents` persistidos com EF
- visao operacional via `GET /api/operations/dashboard`
- trilha de auditoria em `operations.audit_entries`
- canal inicial de `WhatsAppSupport`
- base de identidade JWT com claims e roles
- isolamento por `TenantId` nos agregados principais, filtros globais no EF e migration dedicada

### Integracoes em espera por documentacao ou mapeamento detalhado

- ampliacao de providers alem da `Justos`
- integracoes documentais automaticas por seguradora
- emissao externa por seguradora

## Arquitetura alvo para seguradoras

- cada seguradora deve ter um modulo proprio dentro de `Quotes`
- dentro do modulo da seguradora, os ramos devem ser separados apenas quando a seguradora realmente tiver APIs distintas para eles
- nem toda seguradora precisa ter as mesmas subpastas ou os mesmos ramos
- o contrato canonico de `Quotes` deve continuar enxuto; a variacao de ramo fica encapsulada no modulo da seguradora
- a prioridade atual de integracao para novos provedores e `auto`

Exemplo de desenho esperado:

- `Carriers/SeguradoraX/Auto`
- `Carriers/SeguradoraX/Life`
- `Carriers/SeguradoraX/Residence`

Se a seguradora nao oferecer um ramo, a pasta correspondente nao deve existir.

## Documentacao acessivel hoje

- `Justos`: documentacao oficial acessivel e integracao real de `auto` ja implementada
- `Bradesco Seguros`: portal oficial e documentacao tecnica de credenciais acessiveis
- `Icatu Seguros`: portal oficial de APIs acessivel
- `Liberty / Yelum`: especificacao OpenAPI recebida localmente e convertida em modulo tecnico inicial

Ainda sem documentacao validada no repositorio:

- `Allianz`
- `Porto`
- `Tokio`

## Endpoints canonicos ja disponiveis

- `POST /api/quotes/requests`
- `POST /api/billing/subscriptions`
- `GET /api/billing/subscriptions/{id}`
- `POST /api/billing/subscriptions/{id}/invoices`
- `GET /api/billing/invoices/{id}`
- `POST /api/billing/invoices/{id}/pay`
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

## Observabilidade do multicalculo

- `GET /api/quotes/providers` expoe o estado operacional dos providers de cotacao
- cada provider informa:
  - se esta habilitado
  - se esta pronto de fato
  - modo de autenticacao esperado
  - documentacao oficial
  - requisitos faltantes de configuracao

Isso da visibilidade para backend, frontend e operacao sem depender de leitura manual de `appsettings`.
