# Mapeamento Inicial de Fluxos de Mercado

## Objetivo

Traduzir os fluxos observados no mercado para o desenho alvo do ERP W.Assis, preservando:

- isolamento do dominio
- idempotencia
- rastreabilidade por `CorrelationId`
- processamento assíncrono sem perda
- adaptadores especificos por seguradora

## Fluxos observados no mercado

### Quotes / Multicalculo

Origem observada:

- fluxo de listagem e acompanhamento de orcamentos em plataforma de mercado

Mapeamento alvo:

- `POST /api/quotes/requests`
- `GET /api/quotes/requests/{id}`
- `GET /api/quotes/requests/{id}/results`
- `GET /api/quotes/providers`
- `BackgroundTasks` para disparo e polling

Status atual:

- modulo canonico de `Quotes` ja existe
- persistencia inicial pronta
- request idempotente por `CorrelationId`
- provider inicial da `Justos` ja integrado com autenticacao e criacao de cotacao
- `Bradesco Seguros` e `Icatu Seguros` agora expoem status operacional, requisitos e documentacao oficial pelo endpoint `GET /api/quotes/providers`
- a integracao HTTP real de `Bradesco Seguros` e `Icatu Seguros` segue aguardando o mapeamento detalhado do produto ou jornada liberado para a parceria

### Policies / Importacao e emissao

Origem observada:

- fluxo de importacao documental de proposta
- fluxo de gestao de parcelas apos proposta

Mapeamento alvo:

- `Documents` para upload, parsing e OCR
- `Policies` para rascunho, validacao e emissao
- `Financial` para suporte a comissao e conciliacao

Status atual:

- documento parseado ja gera `PolicyDraft`
- `PolicyDraft` pode avancar para `ReadyForIssuance`
- emissao interna ja gera `PolicyNumber` e auditoria
- emissao externa por seguradora segue em espera

### Financial / Baixa de comissao

Origem observada:

- fluxo de baixa manual de comissao
- fluxo de busca automatica de comissao

Mapeamento alvo:

- `Documents` para ingestao do PDF ou extrato
- `Financial` para leitura, sugestao de match, baixa e auditoria

### WhatsApp / Atendimento

Mapeamento alvo:

- bot recebe mensagem
- sistema cria ou atualiza conversa
- quando necessario, conversa vai para handoff humano
- operacao acompanha fila no dashboard

Status atual:

- endpoint de inbound disponivel
- entidade de conversa persistida
- handoff humano inicial suportado

## Como deixar o fluxo mais eficiente

- criar a requisicao uma unica vez por `CorrelationId`
- processar integracao externa sempre fora do request síncrono
- persistir resultado parcial por etapa
- separar parsing, conciliacao e emissao em passos independentes
- manter dashboards simples com contadores operacionais

## Como deixar o fluxo mais seguro

- nao logar CPF, CNPJ, placa ou tokens em claro
- usar `CorrelationId` em todas as operacoes e logs
- salvar auditoria de transicoes relevantes
- permitir reprocessamento sem duplicar operacao
- tratar falha de integracao por adapter sem quebrar o agregado principal

## Proximos passos dependentes de docs externas

1. providers reais por seguradora em `Quotes`
2. integracao documental automatica por seguradora
3. emissao externa e acompanhamento por seguradora
