# Pedidos de cálculo Auto — 12/10/2026

Pré-condição resolvida: raízes CRM canônicas, CI 38248283807 aprovado com 160 testes. Este recorte implementa criação/lista/leitura de snapshots Auto nas tabelas `erp.calculos`, `erp.calc_auto` e `erp.calculo_coberturas`, conforme v3.1, com idempotência e autorização herdada da oportunidade.

HTTP: `POST/GET /api/oportunidades/{id}/calculos`, `GET /api/calculos/{id}`. POST exige `Idempotency-Key`, grava pedido+CTI+coberturas atomicamente e retorna agregado com campos canônicos. Origem do pedido é MANUAL/ASSISTIDA/IMPORTADA; motor e status técnico ficam fora do pedido. Rótulo gerado pelo serviço, sem edição da versão. Alteração do pedido usa novo POST e nova chave.

Vigência nova deriva a data atual no timezone do grupo, herdado pela filial no esquema v3.1, e um ano-calendário; sem timezone válido, exigir configuração. Renovação própria deriva a apólice vinculada do mesmo ramo/escopo e recusa campos anteriores contraditórios; renovação externa exige dados anteriores. Dados desconhecidos permanecem nulos. Coberturas pertencem ao ramo, pessoa pertence à mesma corretora. Nota interna não integra transmissão.

- [x] DTOs/validação, serviço transacional, auditoria de criação e RLS descendente. Snapshots bloqueiam UPDATE/DELETE no banco. Rótulo com ordinal gerado sob lock da oportunidade.
- [x] CI inicial 38251301576 aprovado integralmente; versão ef0a5e3. Local: 156 testes sem PostgreSQL e seis integrações PostgreSQL/TLS aprovadas.
- [~] Cobertura adicional: HTTP de renovação própria/rollback, oito retries concorrentes com role NOBYPASSRLS e normalização decimal aprovados em PostgreSQL local; CI do checkpoint final pendente.
- [ ] Conectar frontend após APIs de execução/resultados estarem disponíveis.

Sem chamada externa ou envio automático neste recorte. Outros formulários são etapas posteriores. Micro-plano frontend será atualizado com o contrato; `database.ts` permanece v3.1.

Receipts em `infrastructure.calculation_receipts` guardam apenas escopo/chave/hash/ID, sem JSON de negócio. Nova versão usa nova chave; retry equivalente preserva o resultado histórico, mesmo após mudar a data corrente. Hash normaliza a ordem de coberturas e a escala decimal. Create valida ramo Auto ativo, catálogos e cotado, segura o lock da oportunidade e reavalia escopo PROPRIO após lock. GET/lista verificam acesso à oportunidade, além do RLS herdado. Falha depois do INSERT de pedido/CTI/cobertura não deixa receipt ou versão parcial.

Ainda pendentes: endpoints de duplicação assistida, outros formulários, lookup/execuções/resultados e apresentações. O Aggilizador legado não foi conectado ao pedido canônico; sua restrição de renovação própria continua até o bridge de execuções validar/transmitir os dados canônicos. APIs de pedidos existentes não tornam a UI conectada automaticamente.
