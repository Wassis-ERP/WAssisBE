# Contexto comercial da oportunidade — 12/10/2026

Continuação autorizada pelo usuário, independente da integração visual. Branch
`codex/commercial-opportunity-context`, baseada na fundação HTTP do PR #34.

`useOportunidade` hoje busca ramo/origem/motivo/etapa pelo catálogo administrativo,
exigindo Configurações para abrir um fluxo comercial. Preparar GET
`/api/oportunidades/{id}/contexto-comercial` com apenas as referências vinculadas,
sem diretório de pessoas, catálogo completo ou permissão administrativa ampliada.

Referências DBML/instruções v3.1: oportunidades, ramos, origens, motivos_perda,
pipeline_stages e pipelines. Sem alteração de schema/migration ou integração externa.

- [x] Autorizar pela oportunidade com permissão comercial, PROPRIO, tenant e
  filial selecionada. Todas as referências devem pertencer ao mesmo grupo; o
  funil deve ser comercial, global do grupo ou da mesma filial.
- [x] Retornar somente campos de exibição/forma do ramo e etapa vinculados.
  Referências inativas continuam legíveis para histórico; criação/edição mantêm
  suas próprias validações e APIs administrativas continuam protegidas.
- [x] Verificar HTTP sem Configurações, escopo estrangeiro/PROPRIO, campos mínimos,
  nulabilidade e referência histórica em PostgreSQL/TLS descartável.
- [x] Release/testes relevantes; checkpoint separado da integração das telas.

Busca de segurados e seus dados pessoais continua no módulo Segurados e exige
sua própria autorização. Esta API não substitui a autorização de leitura pessoal.

Validação local: Release e 161 testes sem PostgreSQL aprovados. Os sete testes
PostgreSQL não afetados passaram; a jornada ampliada passou na repetição focada
após corrigir a fixture: ramo no pedido não significa ramo vinculado à oportunidade.
Foram verificados ramo ainda não vinculado (null), projeção mínima, catálogo/etapa
inativos, referências opcionais, Configurações negado, PROPRIO e revogação ao vivo,
outro grupo/filial e troca da filial selecionada. Somente banco descartável com TLS;
nenhuma migration, alteração compartilhada ou rollout. API aguarda integrar os
consumidores; os gates locais não certificam novas telas.
