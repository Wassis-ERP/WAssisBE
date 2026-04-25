# W.Assis Insurance ERP

Plataforma e ERP digital da corretora W.Assis, construÃ­da como um monÃ³lito modular no estilo Equinox, com foco em `Clean Architecture`, `DDD`, `CQRS`, integraÃ§Ã£o por seguradora e separaÃ§Ã£o clara de responsabilidades entre domÃ­nio, aplicaÃ§Ã£o, infraestrutura e host.

## Stack

- `.NET 8`
- `MediatR`
- `FluentValidation`
- `EF Core + PostgreSQL`
- `MassTransit + RabbitMQ`
- `Dapper`
- `Serilog + OpenTelemetry`
- `Redis`

## Estrutura da soluÃ§Ã£o

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

## MÃ³dulos do negÃ³cio

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

## O que jÃ¡ estÃ¡ implementado

- esqueleto funcional de `Quotes` com persistÃªncia, request idempotente por `CorrelationId` e processamento assÃ­ncrono
- provider real da `Justos` jÃ¡ integrado ao multicÃ¡lculo
- catÃ¡logo operacional de providers em `GET /api/quotes/providers`
- providers de `Bradesco Seguros` e `Icatu Seguros` jÃ¡ modelados com readiness, requisitos e documentaÃ§Ã£o oficial
- `Documents` com upload de PDF de proposta, extraÃ§Ã£o textual e fallback de OCR configurÃ¡vel
- parser inicial de proposta com perfil por seguradora
- `Policies` com `PolicyDraft`, progressÃ£o atÃ© emissÃ£o interna e `PolicyNumber`
- `Financial` e `Documents` persistidos com `EF Core`
- `Billing` preparado para assinatura e fatura das corretoras clientes do ERP
- dashboard operacional e trilha de auditoria
- base de identidade com `JWT`, `claims`, `roles` e polÃ­ticas
- base de isolamento multi-tenant com `TenantId` nos agregados centrais e filtro por tenant no `DbContext`

## Endpoints jÃ¡ disponÃ­veis

- `POST /api/billing/subscriptions`
- `GET /api/billing/subscriptions/{id}`
- `POST /api/billing/subscriptions/{id}/invoices`
- `GET /api/billing/invoices/{id}`
- `POST /api/billing/invoices/{id}/pay`
- `POST /api/quotes/requests`
- `GET /api/quotes/requests/{id}`
- `GET /api/quotes/requests/{id}/results`
- `GET /api/quotes/providers`
- `POST /api/documents/proposals/uploads`
- `GET /api/documents/proposals/{id}`
- `POST /api/documents/proposals/{id}/review`
- `POST /api/documents/proposals/{id}/reprocess`
- `POST /api/policies/drafts/from-document`
- `GET /api/policies/drafts/{id}`
- `POST /api/policies/drafts/{id}/approve-review`
- `POST /api/policies/drafts/{id}/ready`
- `POST /api/policies/drafts/{id}/issue`
- `POST /api/financial/statement-analyses`
- `GET /api/financial/reconciliations/{id}`
- `POST /api/financial/reconciliations/{id}/settle`
- `GET /api/operations/dashboard`
- `POST /api/identity/login`
- `GET /api/identity/me`
- `POST /api/whatsapp/conversations/inbound`
- `GET /api/whatsapp/conversations/queue`
- `GET /api/whatsapp/conversations/{id}`
- `POST /api/whatsapp/conversations/{id}/assign`
- `POST /api/whatsapp/conversations/{id}/close`

## Fluxograma macro

```mermaid
flowchart TD
    A[Cliente ou corretor inicia fluxo] --> B[API recebe request com CorrelationId e TenantId]
    B --> C[PersistÃªncia inicial no mÃ³dulo correto]
    C --> D{Tipo de fluxo}
    D -- Quotes --> E[BackgroundTasks aciona providers de seguradora]
    E --> F[Resultados normalizados em Quotes]
    D -- Documents --> G[Upload, extraÃ§Ã£o textual e OCR]
    G --> H[Parser por seguradora]
    H --> I[PolicyDraft]
    I --> J[ReadyForIssuance / Issued]
    D -- Financial --> K[Registro de recebimento]
    K --> L[ReconciliaÃ§Ã£o e divergÃªncia]
    D -- WhatsApp --> M[Conversa bot ou handoff humano]
    F --> N[Dashboard e auditoria]
    J --> N
    L --> N
    M --> N
```

## Como rodar localmente

### API

```powershell
dotnet run --project src\WAssis.Services.Api\WAssis.Services.Api.csproj
```

### Worker

```powershell
dotnet run --project src\WAssis.BackgroundTasks\WAssis.BackgroundTasks.csproj
```

### Infra local com Docker

Melhor opÃ§Ã£o para agora: subir apenas a infraestrutura local via Docker e manter API/worker rodando por `dotnet run`.

```powershell
docker compose -f docker/docker-compose.yml up -d
```

ServiÃ§os locais:

- PostgreSQL em `localhost:5432`
- RabbitMQ em `localhost:5672`
- painel do RabbitMQ em `http://localhost:15672`
- Redis em `localhost:6379`

Mais detalhes em `docs/local-docker.md`.

### Build

```powershell
dotnet build WAssisInsurance.sln -p:UseSharedCompilation=false -nodeReuse:false
```

### Testes

```powershell
dotnet test WAssisInsurance.sln -p:UseSharedCompilation=false -nodeReuse:false
```

## CI

O repositÃ³rio agora possui validaÃ§Ã£o automÃ¡tica no GitHub Actions em PRs e pushes para `main`, usando o workflow `.github/workflows/pr-validation.yml`.

## ConfiguraÃ§Ã£o

O projeto jÃ¡ possui seÃ§Ãµes de configuraÃ§Ã£o para:

- `Identity:Jwt`
- `Ocr`
- `Quotes:Providers:Justos`
- `Quotes:Providers:BradescoSeguros`
- `Quotes:Providers:IcatuSeguros`

As integraÃ§Ãµes reais por seguradora dependem das credenciais e do detalhamento do produto ou jornada liberado por cada parceiro.
O modelo de dados tambÃ©m jÃ¡ comeÃ§ou a ser preparado para operaÃ§Ã£o multi-tenant entre corretoras.

## DocumentaÃ§Ã£o

DocumentaÃ§Ã£o tÃ©cnica no repositÃ³rio:

- `docs/architecture-overview.md`
- `docs/identity-access-model.md`
- `docs/billing-overview.md`
- `docs/quotes-provider-justos.md`
- `docs/quotes-provider-bradesco.md`
- `docs/quotes-provider-icatu.md`
- `docs/financial-documents-flows.md`
- `docs/fluxos-mercado-mapeamento.md`
- `docs/local-docker.md`

DocumentaÃ§Ã£o executiva e de acompanhamento tambÃ©m estÃ¡ sendo mantida no Notion do projeto.


Bootstrap para o frontend:

- `docs/frontend-api-bootstrap.md`
- `docs/collections/WAssisInsurance.local.postman_collection.json`
- `docs/collections/WAssisInsurance.local.postman_environment.json`
