# Fundação HTTP do cálculo — 12/10/2026

Novo recorte após PR #33, branch `codex/calculation-http-foundation`. Preparar dados de apoio para o frontend usar os endpoints canônicos já mesclados: seguradoras ativas do grupo, coberturas ativas do ramo Auto e calendário efetivo da filial. Permissão comercial e acesso à oportunidade, sem exigir leitura administrativa de Configurações. Busca de pessoas permanece no módulo Segurados.

- [x] GET paginado `/api/oportunidades/{id}/calculos/lookups/seguradoras`.
- [x] GET paginado `/api/oportunidades/{id}/calculos/lookups/coberturas?ramoId=...`.
- [x] GET `/api/oportunidades/{id}/calculos/contexto`: data/fuso e ramo/cotado/apólice vinculados; dados mínimos da apólice de origem do mesmo ramo/escopo, sem varredura de apólices.
- [x] Testes HTTP/RLS/PROPRIO, projeção mínima e paginação. Sem migration, nova integração externa ou alteração de autorização dos catálogos administrativos.

Adapter frontend em repositório separado; não conectar tela enquanto contrato não estiver validado. Quaisquer timestamps/documentos desconhecidos continuam nulos. Calendário é fornecido pelo backend, evitando usar o fuso do navegador como regra de negócio.

Validação local: Release sem avisos/erros, 161 testes sem PostgreSQL e oito integrações PostgreSQL/TLS aprovadas. Testes demonstram operador sem leitura de Configurações usando lookups comerciais, exclusão de seguradora inativa/estrangeira, paginação/busca, catálogo de outro grupo recusado, contexto da apólice vinculada, timezone inválido, isolamento de outro grupo/corretora e PROPRIO. Clock fixo às 01:30 UTC comprova data do dia anterior em São Paulo com role NOBYPASSRLS. CI do novo PR será registrado separadamente; nenhum rollout ou alteração de banco compartilhado realizado.
