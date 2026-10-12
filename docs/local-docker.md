# Docker Local

## Melhor escolha agora

Para o momento, a melhor dockerizacao para o projeto e subir somente a infraestrutura local:

- PostgreSQL
- RabbitMQ
- Redis

Isso reduz friccao para o time sem obrigar a containerizar a API e o worker agora. O backend continua rodando normalmente via `dotnet run`, mas com dependencias padronizadas entre as maquinas.

## Como subir

Na raiz do repositorio:

Copie `docker/.env.example` para `docker/.env`, gere valores locais aleatorios
para todos os segredos e mantenha esse arquivo fora do Git. Depois execute:

```powershell
docker compose --env-file docker/.env -f docker/docker-compose.yml up -d
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

As publicacoes estao restritas a `127.0.0.1`; o painel administrativo do
RabbitMQ nao e exposto nas demais interfaces da maquina. PostgreSQL, RabbitMQ e
Redis recusam a inicializacao quando os segredos locais obrigatorios nao foram
informados.

## String de conexao local

Monte a string com os mesmos valores do arquivo local, sem salva-la no Git:

```text
Host=localhost;Port=5432;Database=<database>;Username=<user>;Password=<secret>
```

## Fluxo sugerido para desenvolvimento

1. Subir a infraestrutura com Docker.
2. Rodar a API com `dotnet run --project src/WAssis.Services.Api/WAssis.Services.Api.csproj`.
3. Rodar o worker com `dotnet run --project src/WAssis.BackgroundTasks/WAssis.BackgroundTasks.csproj`.

API e worker devem receber o mesmo diretório absoluto privado por
`Documents__PrivateStorage__RootPath`. Crie o diretório fora da árvore publicada,
restrinja a ACL ao usuário dos serviços e não o compartilhe por HTTP. Em container,
monte o mesmo volume privado nos dois serviços; `Dockerfile.worker` não declara nem
cria esse recurso automaticamente. Configure `Ocr__ExecutablePath` somente
para um binário administrado e mantenha os limites `Documents__Processing__*`.

As imagens usam tags de patch e digests verificaveis. Atualizacoes devem revisar
release notes, obter o novo digest do registry e validar o compose novamente.
