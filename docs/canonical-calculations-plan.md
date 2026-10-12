# Pedidos de cálculo Auto — 12/10/2026

Pré-condição resolvida: raízes CRM canônicas, CI 38248283807 aprovado com 160 testes. Este recorte implementa criação/lista/leitura de snapshots Auto nas tabelas `erp.calculos`, `erp.calc_auto` e `erp.calculo_coberturas`, conforme v3.1, com idempotência e autorização herdada da oportunidade.

HTTP: `POST/GET /api/oportunidades/{id}/calculos`, `GET /api/calculos/{id}`. POST exige `Idempotency-Key`, grava pedido+CTI+coberturas atomicamente e retorna agregado com campos canônicos. Origem do pedido é MANUAL/ASSISTIDA/IMPORTADA; motor e status técnico ficam fora do pedido. Rótulo gerado pelo serviço, sem edição da versão. Alteração do pedido usa novo POST e nova chave.

Vigência nova deriva a data atual no timezone cadastrado da filial e um ano-calendário; sem timezone válido, exigir configuração. Renovação própria deriva a apólice vinculada; renovação externa exige dados anteriores. Dados desconhecidos permanecem nulos. Coberturas pertencem ao ramo, pessoa pertence à mesma corretora. Nota interna não integra transmissão.

- [~] DTOs/validação, serviço transacional e RLS descendente.
- [ ] Testes de versões/retry/escopo/rollback e calendário em CI.
- [ ] Conectar frontend após APIs de execução/resultados estarem disponíveis.

Sem chamada externa ou envio automático neste recorte. Outros formulários são etapas posteriores. Micro-plano frontend será atualizado com o contrato; `database.ts` permanece v3.1.
