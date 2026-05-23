# Aggilizador Auto

## Objetivo

O provider `aggilizador_auto` foi adicionado ao módulo de `Quotes` como contingência operacional de multicálculo. Ele entra no pipeline como se fosse uma seguradora/provedor canônico, sem acoplar controller, domínio ou aplicação à API externa.

## Como funciona

- O provider usa o fluxo consolidado `POST /Auto` da API do Aggilizador.
- A ativação operacional é controlada por uma flag persistida no banco na tabela `quotes.quote_provider_activation_settings`.
- A configuração técnica continua em `appsettings`, com `BaseUrl`, `InsuranceBrokerId`, `PartnerId` e defaults do formulário.
- Se o toggle do banco estiver desligado, o provider não entra no processamento e aparece desativado em `GET /api/quotes/providers`.

## Estratégia de contingência

- O módulo foi modelado para permitir fallback futuro quando o multicálculo principal falhar.
- O Aggilizador não substitui o modelo canônico de `Quotes`; ele é apenas mais um provider dentro do orquestrador.
- Campos adicionais de auto foram incorporados ao request canônico para suportar integrações que exigem questionário mais detalhado.

## Pendências operacionais

- Preencher `BaseUrl`, `InsuranceBrokerId` e `PartnerId`.
- Popular no banco a flag de ativação do provider.
- Validar resposta real da API para ampliar parsing de prêmio, parcelas e coberturas.
