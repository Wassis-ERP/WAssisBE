# Frontend API Bootstrap

## Objetivo

Dar ao frontend um ponto de partida claro para autenticação e consumo dos endpoints já estáveis do backend.

## Repositório do backend

- `C:\Users\PC\source\repos\WAssisInsurance`

## Fluxo inicial de autenticação

### Login local de desenvolvimento

Endpoint:
- `POST /api/identity/login`

Credenciais de desenvolvimento:
- configurar em `Identity:DevelopmentAuth:Users` via `dotnet user-secrets`, variaveis de ambiente ou `appsettings.Development.json` local
- usuarios sugeridos: `broker.admin@wassis.local` e `broker.seller@wassis.local`

Observações:
- esse login é apenas para ambiente de desenvolvimento
- nao versionar senhas reais de desenvolvimento
- após login, o frontend deve armazenar o `accessToken`
- usar `Authorization: Bearer <token>` nas rotas protegidas

### Usuário autenticado

Endpoint:
- `GET /api/identity/me`

Uso esperado:
- validar contexto autenticado
- obter `tenantId`, `brokerageId`, `sellerId`, `userType` e `roles`

## Endpoints estáveis para o FE começar

### Saúde
- `GET /health`
- `GET /health/ready`

### Identity
- `POST /api/identity/login`
- `GET /api/identity/me`

### Quotes
- `GET /api/quotes/providers`
- `POST /api/quotes/requests`
- `GET /api/quotes/requests/{id}`
- `GET /api/quotes/requests/{id}/results`

### Billing
- `POST /api/billing/subscriptions`
- `GET /api/billing/subscriptions/{id}`
- `POST /api/billing/subscriptions/{id}/invoices`
- `GET /api/billing/invoices/{id}`
- `POST /api/billing/invoices/{id}/pay`

### Documents
- `POST /api/documents/proposals/uploads`
- `GET /api/documents/proposals/{id}`
- `POST /api/documents/proposals/{id}/review`
- `POST /api/documents/proposals/{id}/reprocess`
- `POST /api/documents/searches`
- `GET /api/documents/searches/{id}`

### Policies
- `POST /api/policies/drafts/from-document`
- `GET /api/policies/drafts/{id}`
- `POST /api/policies/drafts/{id}/approve-review`
- `POST /api/policies/drafts/{id}/ready`
- `POST /api/policies/drafts/{id}/issue`

### Financial
- `POST /api/financial/commission-receipts`
- `POST /api/financial/statement-analyses`
- `GET /api/financial/reconciliations/{id}`
- `POST /api/financial/reconciliations/{id}/settle`

### WhatsAppSupport
- `POST /api/whatsapp/conversations/inbound`
- `GET /api/whatsapp/conversations/queue`
- `GET /api/whatsapp/conversations/{id}`
- `POST /api/whatsapp/conversations/{id}/assign`
- `POST /api/whatsapp/conversations/{id}/close`

## Como rodar localmente

```powershell
dotnet run --project src\WAssis.Services.Api\WAssis.Services.Api.csproj
```

## Artefatos entregues

- collection Postman inicial em `docs/collections/WAssisInsurance.local.postman_collection.json`
- environment Postman inicial em `docs/collections/WAssisInsurance.local.postman_environment.json`
