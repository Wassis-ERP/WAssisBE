# Raízes CRM canônicas — 12/10/2026

## Escopo autorizado
Segurados e Oportunidades passam a usar `erp.segurados` e `erp.oportunidades`, desbloqueando as FKs do multicálculo v3.1. O usuário confirmou ausência de dados produtivos e dispensou reconciliação/backfill de IDs antigos. A migration remove as duas tabelas legadas `public` com seus dados de teste; não executar em produção. Aplicação somente em fixtures descartáveis nesta rodada.

## Decisões
- Sem dual-write. Modelos usam UUID nas referências e `date` para datas de calendário, preservando DTOs HTTP compatíveis.
- Oportunidade mantém título, estimativas, etapa, origem, lead e conclusão canônicos. Nome, valores legados e status são aliases/projeções de leitura; `pipeline_id` é derivado da etapa.
- Dados arbitrários em metadata não são aceitos nem persistidos. Campos legados sem destino canônico são recusados, evitando perda silenciosa.
- Timestamps de criação/atualização e autoria são metadados operacionais adicionais, separados de dados de negócio.
- Manter RLS de empresa/filial e ownership PROPRIO, validar vínculo do segurado e referências da oportunidade no banco.

## Etapas
- [x] Mapas/domínio CRM canônicos, migration de reset não produtivo e atualização de consultas/fixtures.
- [x] Testes unitários, PostgreSQL/RLS e jornada HTTP conectada em CI 38248283807 (160 testes, zero ignorados).
- [ ] Playwright conectado local: bloqueado pelo Controle de Aplicativos ao iniciar psql; sem alegação de aprovação nesta rodada.
- [ ] Serviços de pedido de cálculo e CTI/coberturas.
- [ ] Execuções, resultados normalizados, renovação própria e apresentações.

O contrato de referência continua v3.1. Sem integração SegFy inferida e sem alteração de telas neste recorte. Evidências serão registradas ao concluir.

## Validação intermediária
Build Release sem avisos/erros e modelo EF sem divergência. Os 152 testes existentes sem PostgreSQL passaram após adaptar as fixtures UUID; foram acrescentadas coberturas de aliases/metadata e da FK cálculo → oportunidade → segurado. A tentativa PostgreSQL local em 12/10 foi bloqueada pelo Controle de Aplicativos do Windows ao carregar `WAssis.Infra.Data.dll` (0x800711C7), antes da execução de negócio. Não é aprovação da suíte. Fixture Windows corrigida para pg_ctl não herdar pipes nem aguardar descendentes do servidor; cluster parado e credenciais temporárias removidas. Validação final segue no CI autorizado.

Validação final da raiz: [CI 38248283807](https://github.com/Wassis-ERP/WAssisBE/actions/runs/38248283807), commit ab95578, aprovado integralmente (160 testes e scans finais). A falha do primeiro CI era fixture tentando alterar o alias Name; corrigida para Title. Vínculo direto de segurado de outra corretora foi recusado pelo banco. O recorte posterior dos pedidos Auto está em `canonical-calculations-plan.md`.
