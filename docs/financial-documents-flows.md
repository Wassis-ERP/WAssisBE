# Financial and Documents Flows

## Objetivo

Consolidar o fluxo documental e financeiro sem perda operacional, cobrindo:

- busca automática de documentos
- upload e leitura de PDF
- baixa automática de comissão
- baixa manual com divergência de valor
- auditoria e acompanhamento operacional

## Fluxo documental atual

### Proposta PDF

1. usuário envia arquivo em `POST /api/documents/proposals/uploads`
2. API transmite o PDF para storage privado usando um ID do servidor
3. sistema cria `ImportedDocument` e outbox durável na mesma transação
4. worker abre escopo tenant/filial e inicia um processo filho descartável
5. parser tenta a camada textual e, se necessário, OCR configurável via `Tesseract`
6. parser aplica perfil por seguradora quando reconhece o layout
7. resultado e conclusão da outbox são persistidos juntos para revisão e reuso em `Policies`

### Estado implementado

- upload multipart por streaming, com limite de tamanho e assinatura PDF
- storage privado configurável; a chave física não é retornada ao cliente
- fila PostgreSQL durável com filial explícita
- persistência EF pronta
- parser inicial por perfil de seguradora
- fallback de OCR em processo encerrável, com limites de tempo/CPU/memória observada
- conversão para `PolicyDraft`

## Fluxo financeiro alvo

### Busca automática

Endpoints observados nos HARs:

- `POST /Financeiro/Recebimentos/BaixarComissoesAg`
- `POST /hfy/api/documentos/search_commissions`
- `POST /hfy/api/documentos/document_searches`

Fluxo alvo:

1. operador inicia busca automática
2. sistema cria `DocumentSearch`
3. worker consulta integrações habilitadas
4. documentos localizados são persistidos
5. extração gera sugestões de conciliação

### Baixa manual com diferença

Fluxo alvo:

1. recebimento é registrado
2. comissão esperada é localizada
3. diferença é calculada
4. conciliação fica `Divergent`
5. operador confirma baixa parcial, total ou rejeição

## Princípios de segurança

- nunca logar PII ou tokens em claro
- toda operação usa `CorrelationId`
- OCR e parsing falham sem perder o documento recebido
- divergência nunca apaga o recebimento original
- baixa automática e manual devem ser idempotentes
- toda transição relevante gera auditoria
- limites rígidos do runtime, antimalware e storage durável devem ser comprovados
  no ambiente; a configuração preparada não é evidência de infraestrutura

## Próximas evoluções

1. ingestão automática de documento por seguradora
2. parser de extrato/comprovante para comissão
3. sugestão de conciliação automática
4. fila de revisão humana para divergências
5. dashboards financeiros por status e aging
