# Comparativo persistente — 12/10/2026

Continuação autorizada: apresentação comercial independente das seguradoras. Fonte: DBML/instruções v3.1 e domínio demonstrativo `commercialPresentationDomain.ts`. Resultado manual não significa aprovação ou transmissão contratual.

- [x] POST/GET por oportunidade, GET por apresentação e PUT com revisão esperada; até cinco cotações distintas da mesma oportunidade, inclusive versões diferentes.
- [x] Configuração de layout/ordenação/exibição, textos comerciais e escolha do parcelamento; seleção do cliente registrada sem aprovar a cotação ou criar proposta.
- [x] Idempotência, concorrência otimista, RLS, integridade no banco e auditoria. Leitura do agregado segura lock compartilhado da oportunidade para não misturar cabeçalho/itens/revisão de edições diferentes.
- [x] Testes PostgreSQL/HTTP de escopo, limite, parcelamento, ordenação e rollback. Oito creates concorrentes resultam em uma apresentação; oito edições com a mesma revisão e chaves distintas permitem um único vencedor. DB rejeita sexto item e resultado de outra oportunidade, inclusive do mesmo grupo/corretora. Falha na substituição dos itens preserva cabeçalho, seleção e revisão anteriores.
- [ ] CI publicado e conexão frontend.

Cabeçalho em `apresentacoes_comerciais`, itens em `apresentacao_cotacoes`; revisão e receipts exclusivamente em infraestrutura. JSON de negócio não será persistido. PUT exige `expectedRevision`; edição concorrente retorna 409. Retry de chave anterior depois de uma edição posterior retorna conflito e exige releitura, sem restaurar conteúdo antigo. `GERADA` registra configuração pronta para renderização; este bloco não exporta PDF nem publica link externo. Hooks, impressão e navegador são etapa frontend posterior.

Validação local: Release sem avisos/erros, 161 testes sem PostgreSQL e oito integrações PostgreSQL/TLS aprovadas (sete testes gerais e a jornada HTTP repetida após corrigir apenas sua fixture). Interface/demonstração existente não foi alterada. Skill `wassis-design`, obrigatória pelo AGENTS.md frontend para implementação de telas, ausente do caminho configurado e das pastas de skills/plugins pesquisadas; localização solicitada ao usuário enquanto este bloco backend foi concluído. Controle de Aplicativos já bloqueava o psql da jornada Playwright; evidência anterior não foi reutilizada como validação desta rodada.
