# Escalabilidade e CQRS

## Decisao atual

O SaaS adota CQRS logico e escala horizontal da API de forma incremental. CQRS, neste estágio, não significa event sourcing nem bancos separados para leitura e escrita.

Até HML e PRD possuírem infraestrutura própria, cada ambiente deve usar uma única `ConnectionStrings__DefaultConnection`, exclusiva daquele ambiente. HML e PRD nunca devem compartilhar banco, usuário, JWT, filas, cache ou armazenamento.

## Fase 0 operacional — obrigatoria antes de duas replicas

Esta fase depende de infraestrutura e ainda não pode ser concluída somente pelo repositório:

1. provisionar um PostgreSQL dedicado para HML e outro para PRD, com usuários e segredos distintos;
2. manter `Database__AutoMigrate=false` nos serviços da API;
3. executar uma tarefa singleton `--migrate` antes do rollout;
4. configurar um emissor de identidade compartilhado; o login de desenvolvimento não funciona fora de Development;
5. isolar JWT, CORS, filas, cache, storage e credenciais de parceiros por ambiente;
6. testar backup, restauração e PITR antes da abertura de PRD.

## Fase 1 — implementada no backend

### Preparacao para load balance

- a API processa `X-Forwarded-For`, `X-Forwarded-Host` e `X-Forwarded-Proto` antes de HTTPS, logs e rate limiting;
- apenas redes CIDR declaradas em `ReverseProxy:KnownNetworks` são confiáveis como proxy;
- `/health` e `/health/ready` retornam `buildSha` e `instance`, permitindo identificar a revisão e o container atendente;
- o encerramento gracioso tem janela de 30 segundos para drenagem de requisições;
- `Database:AutoMigrate=true` é aceito somente em Development;
- a mesma imagem pode executar migrations como tarefa única com `dotnet WAssis.Services.Api.dll --migrate`;
- Staging e Production desabilitam autenticação de desenvolvimento e exigem metadata HTTPS para JWT.

O serviço da API deve ficar acessível somente pelo Traefik. Se a rede overlay mudar, `ReverseProxy__KnownNetworks` deve ser ajustado antes do deploy.

### CQRS logico

- comandos e queries novos usam `ICommand<T>`, `IQuery<T>` e, quando necessário, `ITransactionalCommand<T>`;
- Segurados e Oportunidades possuem contratos de repositório distintos para leitura e escrita;
- consultas desses módulos usam `AsNoTracking`; comandos carregam agregados com tracking;
- o pipeline transacional envolve comandos opt-in que gravam estado e auditoria, garantindo commit ou rollback conjunto;
- `PolicyDraft` pronto/aprovado/emitido e entrada de WhatsApp com auditoria já usam essa transação.

A separação é lógica e permanece no mesmo `WAssisDbContext` e PostgreSQL. Isso reduz acoplamento e prepara projeções sem criar consistência eventual prematuramente.

## Fase 2 — escala horizontal da API

Depois da fase operacional:

- iniciar com duas réplicas da API por ambiente atrás do Traefik, sem sticky session;
- usar a mesma configuração de issuer, audience e signing key em todas as réplicas;
- validar todas as tasks pelo digest da imagem, `/health/ready`, `buildSha` e `instance`;
- usar rollout compatível N/N+1 e migrations no padrão expand/backfill/contract;
- manter o worker com uma réplica;
- mover o limite de login para Traefik/WAF ou armazenamento distribuído antes de depender dele como controle global. O limitador atual é por processo e sua capacidade é multiplicada pelo número de réplicas.

## Fase 3 — processamento distribuido e projecoes

Implementar antes de escalar workers:

- outbox/inbox;
- claim/lease atômico ou fila durável;
- idempotência por mensagem e recuperação de itens presos em `Processing`;
- storage durável para documentos e OCR fora do request;
- projeções de leitura para dashboard, WhatsApp, clientes e cotações quando as medições mostrarem necessidade.

## Fase 4 — banco de leitura

Uma réplica física de leitura só deve ser considerada quando:

- HML e PRD estiverem isolados e estáveis;
- projeções e índices já tiverem sido medidos;
- houver volume que justifique custo e complexidade;
- o produto aceitar explicitamente atraso de replicação nas telas envolvidas.

Queries sensíveis a leitura imediata após escrita continuam no primário. Event sourcing não faz parte do roadmap atual.

## Critérios para finalizar o SaaS

- banco e identidade dedicados por ambiente;
- migration singleton validada antes do rollout;
- duas réplicas da API saudáveis e identificáveis;
- worker singleton até existir processamento distribuído seguro;
- restore de backup testado;
- métricas, logs centralizados, alertas e runbook de rollback;
- testes de carga, falha de réplica e concorrência aprovados em HML.
