# Execuções e resultados manuais — 12/10/2026

Continuidade autorizada pelo usuário sem dados específicos das seguradoras. Contrato: DBML/instruções v3.1; pedido imutável já implementado. Este bloco persiste tentativas e resultados reais informados pelo corretor, sem acionar provedores nem criar resultados fictícios.

- [x] Execução MANUAL por seguradora ativa do grupo, comissão padrão ou sobrescrita explícita, tentativa e reexecução preservadas.
- [x] Idempotência, lock da oportunidade, autorização PROPRIO reavaliada, RLS e integridade descendente no banco.
- [x] Cancelamento de tentativa aberta; registro atômico de resultado, coberturas devolvidas e opções de parcelamento. Resultado não altera o pedido.
- [x] Leituras paginadas e auditoria sem conteúdo sensível.
- [x] Testes HTTP e concorrentes com PostgreSQL real e role NOBYPASSRLS; 161 testes sem PostgreSQL e oito integrações PostgreSQL/TLS aprovadas localmente após o bloco de apresentação (sete integrações gerais + uma jornada HTTP após ajuste isolado de fixture).
- [ ] CI e publicação na branch autorizada.

Endpoints previstos: POST/GET `/api/calculos/{id}/execucoes`, POST `/api/execucoes/{id}/reexecutar`, POST `/api/execucoes/{id}/cancelar`, POST `/api/execucoes/{id}/cotacao-manual`, GET `/api/execucoes/{id}`, GET `/api/cotacoes/{id}`, GET `/api/oportunidades/{id}/cotacoes`. Escritas exigem Idempotency-Key. POST de execução aceita somente MANUAL neste bloco; motores PROPRIO/AGGER aguardam bridge, fila e contratos reais.

Tabelas: `calculo_execucoes`, `cotacoes`, `cotacao_coberturas`, `cotacao_parcelamentos`. Receipts são infraestrutura, com hash/IDs, sem JSON de negócio. Estados terminais não reabrem a tentativa; nova tentativa tem novo ID. Uma tentativa aberta por cálculo/seguradora. Comparativo persistente, ligação dos hooks e processamento automático são blocos seguintes. Nenhuma migration será aplicada em ambiente compartilhado nesta rodada.

Migration v3.1 anterior era aditiva e deixou `cotacoes.calculo_id NOT NULL`, vínculo direto da seguradora e campos legados duplicados. Este recorte remove essas colunas e exige `execucao_id NOT NULL`, conforme D23. Se houver cotações legadas sem execução em alguma fixture não produtiva, resetar a fixture antes de migrar; não inventar uma execução nem duplicar os vínculos para satisfazer o legado. Alteração de schema autorizada pelo usuário para este contexto não produtivo; banco compartilhado não foi acessado.

Testes focados aprovados: jornada HTTP de criação/reexecução/cancelamento, rollback após cabeçalho/coberturas e antes do parcelamento, retries equivalentes e isolamento de outro grupo/corretora; teste NOBYPASSRLS com oito requisições concorrentes por execução, cotação e retry, race cancelamento × resultado, acesso PROPRIO e bloqueio de alteração de histórico. Valores zero/nulos e coberturas sem de/para preservados. Suite geral e CI ainda em fechamento. Down é deliberadamente forward-only devido à retirada das colunas antigas.

O comparativo já foi implementado no bloco seguinte (`canonical-presentations-plan.md`); ligação frontend e processamento automático continuam pendentes. Não foi criada massa fictícia em ambiente compartilhado: fixtures sintéticas existem apenas nos testes descartáveis. Backend Release compilado sem avisos/erros; nenhum schema produtivo foi alterado.
