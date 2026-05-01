# Database and Deploy

## Decisao de banco

O banco oficial do ERP deve ser PostgreSQL controlado pelo `WAssisBE`.

Os frontends nao devem acessar banco direto, carregar connection string, executar RPCs ou conhecer detalhes de schema. Todo acesso deve passar por endpoints do backend com JWT, policies e `TenantId`.

## Ambientes

### Local

- PostgreSQL via `docker/docker-compose.yml`
- API via `dotnet run`
- migrations EF aplicadas localmente pelo desenvolvedor

```powershell
docker compose -f docker/docker-compose.yml up -d postgres rabbitmq redis
dotnet ef database update --project src\WAssis.Infra.Data --startup-project src\WAssis.Services.Api
dotnet run --project src\WAssis.Services.Api\WAssis.Services.Api.csproj
```

### Homologacao

- PostgreSQL gerenciado, separado de producao
- API em container
- frontends apontando para a URL publica da API
- migrations aplicadas antes do deploy da API

### Producao

- PostgreSQL gerenciado com backup automatico e PITR quando disponivel
- API em container com health check em `/health/ready`
- segredos somente no provedor de deploy
- migrations aplicadas em etapa controlada, nunca automaticamente a cada start sem revisao

## Connection string

Configurar por variavel de ambiente:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=wassis;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
```

Em producao, nao usar usuario `postgres` como usuario da aplicacao. Criar um usuario com permissao limitada ao banco/schema da aplicacao.

## Segredos obrigatorios

- `ConnectionStrings__DefaultConnection`
- `Identity__Jwt__SigningKey`
- `Identity__Jwt__RequireHttpsMetadata=true`
- `Frontend__AllowedOrigins__0=https://...`
- credenciais de seguradoras e certificados, quando habilitados

## Deploy da API

O backend agora possui `Dockerfile` para gerar imagem da API.

Build local:

```powershell
docker build -t wassisbe-api .
docker run --rm -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Production `
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=wassis;Username=postgres;Password=postgres" `
  -e Identity__Jwt__SigningKey="configure-uma-chave-com-mais-de-32-caracteres" `
  -e Identity__Jwt__RequireHttpsMetadata=true `
  wassisbe-api
```

## Ordem segura de release

1. Build e testes passam no GitHub Actions.
2. Backup do banco de destino.
3. Aplicar migrations EF.
4. Publicar nova imagem da API.
5. Verificar `/health` e `/health/ready`.
6. Atualizar `VITE_API_BASE_URL` nos frontends se a URL da API mudar.
7. Validar login e `/api/identity/me`.

## Pendencias da migracao Supabase -> WAssisBE

Os gateways adicionados para `/api/data-gateway/*`, `/api/storage/*` e `/api/functions/*` sao temporarios. A cada tela migrada, devemos criar endpoints dedicados no modulo correto e remover o uso do gateway correspondente.
