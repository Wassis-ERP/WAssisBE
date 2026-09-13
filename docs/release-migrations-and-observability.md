# Release, migrations e observabilidade

## Release executável

O workflow publica imagem imutável por SHA/digest e gera SBOM/provenance. O webhook antigo foi retirado do workflow do BE: atualizar serviço sem comprovar a migration não satisfaz o gate. O passo automático restante depende de um runner privado no manager ou endpoint Portainer autenticado com permissão limitada para criar/acompanhar jobs e atualizar apenas o serviço alvo. URL de webhook de serviço não fornece essa capacidade.

Com autorização operacional, no manager Linux, exportar **somente referências não secretas** e executar:

```bash
export IMAGE='ghcr.io/wassis-erp/wassisbe-api@sha256:<digest-da-imagem>'
export BUILD_SHA='<sha-completo>'
export API_SERVICE='<servico-do-ambiente>'
export NETWORK='<overlay-do-postgres-do-ambiente>'
export DB_SECRET='wassis_hml_db_connection_v2'
export READY_URL='https://api20-hml.wassis.com.br/health/ready'
bash scripts/release-swarm.sh
```

Pré-requisitos: Docker Swarm manager, Python 3, curl, imagem acessível no GHCR, rede e secret já existentes. O script não provisiona banco, rede, secret ou usuário. Autenticar previamente o manager; jobs e update usam `--with-registry-auth`. Não imprimir credenciais. Se houver worker ativo, interrompê-lo antes da migration e atualizá-lo antes de retomar, conforme o runbook de processamento durável.

Sequência: job singleton `--migrate` → job `--validate-schema` → atualizar API por digest → readiness conferindo SHA. Advisory lock de sessão impede migrações concorrentes; perder a conexão libera o lock. Falha/timeout interrompe rollout; job de falha permanece para inspeção. API fora de Development recusa iniciar se houver migration pendente. Readiness permanece separado de liveness (`/health`). Execução em banco vazio e upgrade do schema anterior estão na suíte Testcontainers.

Migration nova: `20260913180000_AddDurableQuoteDispatch`, aditiva, outbox/inbox técnicas e backfill de cotações pendentes; Processing vira NeedsReview. Nenhuma migration foi aplicada em banco publicado. Medir volume/tempo em cópia antes da janela. Prática futura: expand, backfill em lotes com reconciliação, contract somente em release posterior. Rollback por digest anterior exige conferir compatibilidade e pausar/reconciliar workers; não executar Down automaticamente nem apagar dados. Falha em contract pode exigir forward fix ou restore/PITR aprovado.

## Telemetria implementada

OpenTelemetry 1.18: ASP.NET Core, HttpClient e ActivitySource Npgsql; métricas HTTP, pool PostgreSQL e worker. Exportação OTLP somente quando `OTEL_EXPORTER_OTLP_ENDPOINT` estiver configurado; sem endpoint, não envia a serviço externo. `BUILD_SHA` identifica versão. O exporter e o collector precisam de destino privado, autenticação/TLS e retenção definidos na infraestrutura.

`X-Correlation-ID` do CRM é aceito somente como UUID; caso contrário é substituído. A resposta e o log recebem o ID; spans contêm correlação e propagação W3C do HttpClient. Não propagamos X-Correlation-ID arbitrário para terceiros; a correlação entre integrações usa traceparent padrão.

Processador de exportação mantém somente allowlist de atributos operacionais. Remove SQL, URL completa, headers e detalhes de exceção; nomes de span não contêm comandos SQL. Métricas excluem tags de connection string, usuário, documentos e IDs de alta cardinalidade. Log da API usa tipo de exceção e template da rota. Auditoria de negócio no banco é diferente de log operacional e precisa política própria de acesso/retenção.

Workers: `wassis.worker.processed`, `wassis.worker.duration`, `wassis.quote.pending`, `wassis.quote.oldest_pending_age`, `wassis.quote.provider_failures` por código de seguradora. Contagem/idade são amostras do ciclo (atualmente 5 min), não relógio em tempo real. Falhas de dispatcher ainda exigem watchdog/restart e alertas do orchestrador.

Dashboards a provisionar: p50/p95/p99 e 4xx/5xx por rota; readiness/replicas; backlog e idade por worker; falhas por seguradora; saturação PostgreSQL. Alertas iniciais para calibrar: readiness indisponível por 2 min; erro 5xx > 2%/5 min com volume mínimo; p95 > 2 s/10 min; cotação pendente > 15 min; worker sem progresso > 15 min. Não incluir documentos, clientes ou tokens nos rótulos.

Fontes: [instrumentação ASP.NET](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.AspNetCore), [tracing Npgsql](https://www.npgsql.org/doc/diagnostics/tracing.html). Collector, dashboards e alertas não foram provisionados nem testados em ambiente publicado.
