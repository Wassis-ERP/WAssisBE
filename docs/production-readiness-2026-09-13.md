# Prontidão SaaS — 13/09/2026

## Diagnóstico verificado

- Base BE: `0a8d709`; CRM: `7f1e25d`. Branches `codex/saas-production-readiness`, repositórios separados em `C:\dev\WAssisBE` e `C:\dev\WassisCRM`.
- HML: `/health` e `/health/ready` HTTP 200, SHA BE confirmado na resposta; CRM HTTP 200. SHA CRM confirmado na main, ainda não atestado pelo HTML.
- Login com credenciais inválidas de diagnóstico: HTTP 409 `identity.login.disabled`.
- HSTS presente na API, ausente no CRM. CSP CRM permite somente fontes locais e bloqueia imagens externas.
- SDK .NET ausente no PATH e Docker parado no diagnóstico inicial; .NET 8 foi preparado em `C:\dev\.tools\dotnet` e Docker iniciado para bancos descartáveis. Revalidação: 93 testes BE (64 na referência), 329 CRM antes do teste adicional de renovação (304 na referência), auditorias NuGet/npm sem vulnerabilidades conhecidas.
- Nenhuma operação no Portainer, banco publicado ou secrets foi executada.

## Plano e critérios

- [x] P0.1 código: autenticação Staging opt-in, hash Identity, JWT máximo 15 min, validação e testes negativos. Production rejeita mecanismo; ativação HML e OIDC real pendentes.
- [x] P0.2 recorte: build conectado fail-closed; inventário de 53 imports de memória; Segurados/Oportunidades com persistência real e layout original; demais operações pendentes bloqueadas.
- [x] P0.3 código/documentação: secrets montados, validação de força/configuração, inventário/runbook. Rotação real não executada.
- [~] P1.4: escopo normal fail-closed, capability auditada de worker, ownership em escrita/concorrência, inventário EF/Dapper e testes negativos. Desenho RLS entregue; ativação e RBAC completo pendentes.
- [~] P1.5–6: lock singleton, schema gate e release executável; banco vazio/upgrade/HTTP e Playwright aprovados. Conexão segura GitHub/Portainer para operação do gate pendente.
- [~] P1.7–8: OpenTelemetry com atributos filtrados, métricas e outbox/inbox interna/lease/fencing em cotações. Collector/alertas, idempotência externa e recuperação administrativa ainda pendentes; worker singleton.
- [~] P1.9–10: ADR, URL/erros/CSP/fontes/HSTS, lazy routes e timeout/cancelamento implementados. BFF, cliente OpenAPI completo, divisão ampla de hooks e módulos restantes pendentes.
- [~] P2: Actions por SHA, bases por digest, SBOM/provenance e scan em CI; CRM não-root validado read-only/cap-drop. Assinatura, WAF/rate limit, streaming, malware/OCR/storage durável e HA pendentes.

## Evidências e arquivos

PR BE: [#30](https://github.com/Wassis-ERP/WAssisBE/pull/30). PR CRM: [#50](https://github.com/Wassis-ERP/WassisCRM/pull/50). Nenhum merge. No primeiro CI, testes/CodeQL passaram e o scan identificou duas CVEs HIGH no PCRE2 da imagem Debian (sem vulnerabilidades NuGet). A imagem recebe a versão oficial 10.42-1+deb12u1; o scan permanece bloqueante e é reexecutado na revisão corrigida.

Comandos: `dotnet test tests/WAssis.Tests/WAssis.Tests.csproj -c Release`, `dotnet build -c Release`, `dotnet list tests/WAssis.Tests/WAssis.Tests.csproj package --vulnerable --include-transitive`. No CRM: npm test, build/tsc, audit, lint focado e global, Playwright. BE: 93 testes passaram; após ajuste de retry/trace/upgrade, os quatro testes afetados passaram novamente. CRM: 329 passaram e o arquivo de contrato com teste adicional passou (10 testes). Lint global mantém 27 erros legados; focado limpo. Builds .NET/CRM e imagens Docker aprovados. Resultado de CI/PR é evidência separada da execução local.

Testcontainers cobre schema vazio, upgrade preservando dados, backfill da outbox, HTTP Staging/login/claims, isolamento de tenant/filial, attach forjado, migration concorrente, rollback da outbox, disputa de claim, recibo duplicado e lease expirada. Playwright cobre as duas jornadas no layout original com reload; não são apenas mocks de HTTP. Não houve chamadas de teste às seguradoras reais.

Grupos BE alterados: Identity/Authentication/Jwt; DbContext/Configuration e escopo de escrita; handlers de Segurados/Oportunidades e OpportunityScope; DurableWorkQueue/QuoteRequestRepository/QuoteProcessingService; Program API/Worker, CorrelationMiddleware e Observability; migration técnica; testes Integration/Security; Docker/workflows/script de release; ADRs/runbooks. Lista exata no diff do PR. CRM: relatório correspondente em docs/prontidao-saas-2026-09-13.md.

Migration criada: `20260913180000_AddDurableQuoteDispatch`. Sem aplicação em banco publicado. Tabelas técnicas SQL-owned, sem alterar DBML de negócio. Antes do rollout parar worker antigo, medir backfill, migrar/validar, atualizar API/worker e só então retomar uma réplica. Rollback de worker exige reconciliação, não Down automático.

## Próximas operações, mediante autorização

1. Revisar PRs/checks, rotacionar DB/JWT/usuários e credenciais expostas conforme runbook; montar secrets e habilitar HML Staging com hashes. Revalidar login no domínio publicado.
2. Preparar acesso limitado do gate ao manager/Portainer; executar release por digest com worker interrompido, schema validado e readiness/SHA comprovado. Webhook BE antigo foi removido para impedir rollout sem migration.
3. Atualizar porta interna do CRM/Traefik para **8080** antes de publicar sua imagem não-root; coordenar o webhook existente do frontend. Validar HSTS/CSP/fontes e jornadas HML.
4. Antes de PRD: identidade real/sessões, RBAC e referências de catálogo completas, reconciliação public/erp, RLS ensaiado com pooling, demais módulos persistidos, collector/alertas, idempotência por seguradora, storage/streaming/malware, limites globais, backup/restore/PITR e ambientes separados.

Riscos eliminados neste recorte: acesso global implícito sem tenant, attach forjado entre proprietários, aparência de persistência em memória, build publicado em mock, exposição de corpo bruto no cliente, migration concorrente e dupla reserva de cotação. Riscos residuais estão nos ADRs; isso não é certificação geral de produção. Nenhum merge/deploy/rotação foi autorizado por consequência desta implementação.

## Limites

Manter monólito modular, CQRS lógico e banco único por ambiente. Não introduzir event sourcing ou microserviços. Não considerar duas réplicas no mesmo host como HA; volumes e PostgreSQL locais permanecem pontos únicos de falha. Worker singleton até coordenação durável. PRD exige isolamento de ambientes, backup, restore e PITR demonstrados. Nenhuma fase pode ser marcada concluída com teste simulado no lugar de integração real.
