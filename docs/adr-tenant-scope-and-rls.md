# ADR — escopo de dados e preparação de RLS

Data: 13/09/2026. Decisão: filtros EF e validação de escrita agora falham fechados quando não há identidade. Não ativar RLS nesta entrega.

## Implementação e inventário

`WAssisDbContext` foi revisado como ponto central das queries EF: todos os agregados com TenantId têm filtro de tenant; Segurados, Oportunidades e QuoteRequests também têm filtro de filial. Tabelas de filhos que apenas têm TenantId continuam com isolamento de grupo, não de filial. `QuoteProviderActivationSetting` é configuração global legada e exige revisão de autorização administrativa antes de PRD.

`SaveChanges`/`SaveChangesAsync` validam ownership atual/original. TenantId e OfficeBranchId entram como tokens de concorrência no predicado UPDATE/DELETE, impedindo que um attach com tenant forjado sobrescreva linha de outro grupo. Não confundir essa proteção com versionamento otimista completo: alterações simultâneas de campos de negócio ainda precisam de versão/ETag e tratamento de conflito.

O acesso global só existe com `SystemDataScope.ForWorker(purpose, audit)`, não registrado na API. Worker recebe capability explícita e registra seu propósito; a auditoria de cotações grava o tenant real do item, nunca um tenant global presumido. Esta capability é para código interno confiável; não é uma fronteira contra código malicioso já executando no processo.

Inventário completo de SQL/Dapper fora de migrations na revisão:

| Local | Escopo e decisão |
|---|---|
| CoreBranchReadRepository | tenant parametrizado + allowlist de filiais; inclui inativas para histórico. Escritas verificam ativa. |
| CoreCatalogReadRepository | lista fechada de recursos; SQL fixo. Catálogos por tenant; pipelines/campos/repasse por tenant e filial; filhos por JOIN ao proprietário. Sem nomes de tabelas livres do request. `to_jsonb` é serialização de leitura, não dado de negócio novo. |
| OpportunityScope | tenant e filiais autenticados, funil comercial e etapas ativas; valida filial real e vínculo do segurado. Ganho/perda respeitam flags da etapa sem mudar a etapa por consequência. |
| DatabaseMigrationGate | SQL constante de advisory lock, somente comando operacional `--migrate`; sem dados do request. |
| DurableWorkQueue | Tabelas técnicas privadas; capability de worker obrigatória; claim global explícito, conclusão/renovação com tenant+token+prazo. |
| QuoteRequestRepository.SaveChangesAsync | INSERT parametrizado na outbox com tenant do agregado validado pelo EF, na mesma transação da cotação. |

Não foram encontradas chamadas de `IgnoreQueryFilters` em produção. Outros repositórios usam LINQ/EF filtrado; a ausência de SQL manual não prova RBAC completo. Antes de PRD, fechar escopo de filial de documentos, financeiro, opções de cotação e auditoria; validar todas as referências de catálogo/responsável nos commands; substituir claims estáticos por autorização persistida. Segurados e Oportunidades verificam tenant/filial e vínculos básicos, não constituem certificação de todos os módulos.

## RLS proposto

1. Reconciliar primeiro duplicidade de schema `public`/`erp` e chave de grupo/corretora do DBML v3.1. Usar roles separadas para aplicação sem BYPASSRLS, migrator e manutenção auditada.
2. Na mesma transação de cada unidade de trabalho, aplicar `set_config('app.tenant_id', tenant, true)` e escopo de filiais. Usar SET LOCAL; nunca SET de sessão em conexão reutilizada no pool.
3. Policy USING/WITH CHECK deve recusar configuração ausente, impor tenant e propriedade da filial; FORCE ROW LEVEL SECURITY quando aplicável. Cobrir filhos por FK/ownership consistente, sem consultas recursivas.
4. Worker processa lote global apenas por rotina restrita de claim; abre transação do tenant do item para trabalho normal. Migrações e backfills usam credenciais próprias.
5. Testes preparatórios já exercitam dois tenants, duas filiais, attach forjado, contexto anônimo e capability de worker em PostgreSQL real. A suíte RLS deverá ainda repetir esses cenários com usuário sem ownership/BYPASSRLS, reutilização do mesmo pool entre tenants, rollback/cancelamento, Dapper e worker.

Nenhuma policy RLS foi aplicada, nem SQL executado em banco publicado. Ativar RLS agora sem reconciliar esses pontos poderia interromper workers e migrations ou dar falsa proteção com owner/BYPASSRLS.
