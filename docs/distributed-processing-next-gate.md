# Processamento durável de cotações — 13/09/2026

A migration 20260913180000_AddDurableQuoteDispatch cria infrastructure.work_outbox e work_inbox. São tabelas técnicas SQL-owned, fora do snapshot EF de negócio; evoluem por migrations SQL explícitas. Nenhum payload de negócio em JSON: a outbox guarda tenant, tipo e ID do agregado. QuoteRequestRepository grava cotação e referência na mesma transação. Inserções futuras devem respeitar esse repositório.

IDurableWorkQueue é reutilizável. UPDATE RETURNING sobre FOR UPDATE SKIP LOCKED reserva uma referência. A lease de 15 minutos tem token aleatório e é renovada antes de cada chamada externa. Conclusão exige tenant, token e prazo válidos; resultado, estado da outbox e recibo único da inbox são gravados na mesma transação. Confirmação duplicada/atrasada é recusada. Claim/conclusão exigem capability explícita de worker, ausente na API.

O upgrade inclui Pending anteriores e coloca Processing anteriores em NeedsReview. Leases expiradas também vão para NeedsReview no ciclo seguinte, sem reenvio automático. Retries HTTP de escrita são desabilitados; GET/HEAD/OPTIONS conservam retries de transientes. Mensagens de exceção de transporte não são persistidas cruas.

Testes PostgreSQL: rollback de cotação+outbox, disputa entre duas conexões, token incorreto, recibo único, conclusão duplicada, lease expirada e quarentena. Upgrade comprova backfill de Pending/Processing. Não equivale a ensaio com seguradoras reais.

## Operação e limites

**Manter worker singleton. Parar worker antes da migration e atualizar sua imagem antes de retomá-lo.** Worker antigo ignora a outbox e não pode disputar com o novo. API/worker novos recusam schema pendente fora de Development. Rollback para worker antigo exige interromper processamento e reconciliar a outbox; Down destrutivo não é permitido.

Consultar apenas contagens em monitoramento: SELECT kind, state, count(*), min(created_at) FROM infrastructure.work_outbox GROUP BY kind, state.

NeedsReview exige consulta à seguradora, decisão auditada e reconciliação individual. Não executar UPDATE em lote para Pending. Falta comando administrativo autorizado para essa resolução. Lease não cancela trabalho remoto nem garante exactly-once externo. Options são persistidas ao concluir o agregado; uma queda intermediária exige reconciliar os provedores já chamados.

Antes de escalar: outbox por provedor/operação, Idempotency-Key e consulta remota, inbox de callbacks com unique de evento externo (a inbox atual é recibo interno), heartbeat durante chamadas longas, recuperação após consulta remota, testes de morte antes/depois de envio e redelivery, retenção de recibos, storage durável e quotas. Não aumentar réplicas com base somente nesta entrega.

Documentos pendentes: streaming, storage por ambiente, checksum/retenção, malware e OCR isolado com limites de CPU/memória/tempo/rede. Duas réplicas no mesmo nó não protegem contra perda do host. PostgreSQL/volumes locais seguem pontos únicos de falha. PRD exige backup, restore ensaiado, PITR e isolamento de banco/usuário/JWT/cache/storage/credenciais.
