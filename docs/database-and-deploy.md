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

- PostgreSQL gerenciado e exclusivo de homologacao
- API em container
- frontends apontando para a URL publica da API
- migrations aplicadas por tarefa singleton antes do deploy da API

### Producao

- PostgreSQL gerenciado com backup automatico e PITR quando disponivel
- API em container com health check em `/health/ready`
- segredos somente no provedor de deploy
- migrations aplicadas por tarefa singleton em etapa controlada

## Connection string

Configurar por variavel de ambiente:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=wassis;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
```

Em producao, nao usar usuario `postgres` como usuario da aplicacao. Criar um usuario com permissao limitada ao banco/schema da aplicacao.

## Segredos obrigatorios

- `ConnectionStrings__DefaultConnection`
- `Database__AutoMigrate=false` em HML e PRD
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
3. Executar uma tarefa única usando a mesma imagem da release:

```text
dotnet WAssis.Services.Api.dll --migrate
```

4. Confirmar o término bem-sucedido da tarefa de migration.
5. Publicar/atualizar as réplicas da API.
6. Verificar `/health` e `/health/ready`, conferindo `buildSha` e `instance`.
7. Atualizar `VITE_API_BASE_URL` nos frontends se a URL da API mudar.
8. Validar login e `/api/identity/me`.

## Banco limpo de homologacao

Use um nome PostgreSQL simples, sem ponto, para evitar identificadores que exigem aspas. O nome recomendado e `wassis_hml`.

1. Crie o database vazio no mesmo servidor PostgreSQL: `CREATE DATABASE wassis_hml;`.
2. No servico HML da API no Portainer, altere `ConnectionStrings__DefaultConnection` para usar `Database=wassis_hml`.
3. Mantenha `Database__AutoMigrate=false` no serviço HML.
4. Execute uma tarefa única da imagem `hml` com o argumento `--migrate` e aguarde código de saída zero.
5. Force o redeploy da API somente depois da migration.
6. Confirme `GET /health/ready`: HTTP 200 significa conexão válida e nenhuma migration pendente.

O workflow de publicacao consulta `/health/ready` depois do webhook e falha caso o container, o banco ou as migrations nao fiquem prontos.

Não escale a API para duas réplicas enquanto HML e PRD não tiverem bancos e identidade próprios. Consulte [`scalability-cqrs-roadmap.md`](scalability-cqrs-roadmap.md).

## Pendencias da migracao Supabase -> WAssisBE

Os gateways adicionados para `/api/data-gateway/*`, `/api/storage/*` e `/api/functions/*` sao temporarios. A cada tela migrada, devemos criar endpoints dedicados no modulo correto e remover o uso do gateway correspondente.
