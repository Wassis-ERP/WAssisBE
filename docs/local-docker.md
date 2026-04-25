# Docker Local

## Melhor escolha agora

Para o momento, a melhor dockerizacao para o projeto e subir somente a infraestrutura local:

- PostgreSQL
- RabbitMQ
- Redis

Isso reduz friccao para o time sem obrigar a containerizar a API e o worker agora. O backend continua rodando normalmente via `dotnet run`, mas com dependencias padronizadas entre as maquinas.

## Como subir

Na raiz do repositorio:

```powershell
docker compose -f docker/docker-compose.yml up -d
```

Para derrubar:

```powershell
docker compose -f docker/docker-compose.yml down
```

Para derrubar removendo volumes:

```powershell
docker compose -f docker/docker-compose.yml down -v
```

## Servicos expostos

- PostgreSQL: `localhost:5432`
- RabbitMQ AMQP: `localhost:5672`
- RabbitMQ Management: `http://localhost:15672`
- Redis: `localhost:6379`

## Credenciais padrao

### PostgreSQL

- database: `wassis`
- user: `postgres`
- password: `postgres`

### RabbitMQ

- user: `guest`
- password: `guest`

## String de conexao atual

O backend ja possui fallback local compativel com o container do PostgreSQL:

```text
Host=localhost;Port=5432;Database=wassis;Username=postgres;Password=postgres
```

## Fluxo sugerido para desenvolvimento

1. Subir a infraestrutura com Docker.
2. Rodar a API com `dotnet run --project src/WAssis.Services.Api/WAssis.Services.Api.csproj`.
3. Rodar o worker com `dotnet run --project src/WAssis.BackgroundTasks/WAssis.BackgroundTasks.csproj`.

## Observacao

Mesmo que RabbitMQ e Redis ainda nao estejam sendo usados em todo o fluxo, manter esses servicos disponiveis agora ajuda a estabilizar o ambiente local e evita retrabalho quando o uso crescer.
