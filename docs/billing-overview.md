# Billing Overview

## Objetivo

Separar a cobrança SaaS do ERP da operação financeira da corretora.

## Escopo do módulo

`Billing` é responsável por:

- assinatura da corretora cliente no ERP
- plano contratado
- valor recorrente
- dia de cobrança
- emissão de fatura interna
- marcação de pagamento

## O que não entra aqui

- prêmio do seguro pago pelo segurado
- comissão repassada pela seguradora
- conciliação operacional de comissões

Esses fluxos continuam no módulo `Financial`.

## Fluxo inicial implementado

1. corretora cria ou recebe uma `BillingSubscription`
2. sistema emite uma `BillingInvoice` para um período de referência
3. operador ou automação marca a fatura como paga

## Endpoints

- `POST /api/billing/subscriptions`
- `GET /api/billing/subscriptions/{id}`
- `POST /api/billing/subscriptions/{id}/invoices`
- `GET /api/billing/invoices/{id}`
- `POST /api/billing/invoices/{id}/pay`

## Próximas evoluções

1. gateway real de cobrança
2. webhooks de pagamento
3. inadimplência e retry
4. cancelamento e downgrade de plano
5. visão platform-admin multi-tenant para billing
