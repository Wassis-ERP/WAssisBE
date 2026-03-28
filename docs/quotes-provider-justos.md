# Quotes Provider - Justos

## Fonte oficial

- https://justos.notion.site/Documenta-o-da-API-Justos-4f0a82f7b85547319fa5ca7cef7790ce

## Ambientes

- produção: `https://api.justos.com.br`
- staging: `https://api.staging.justos.com.br`

## Autenticação

Fluxo implementado no provider:

1. gerar JWT parceiro assinado com `ES256`
2. enviar o JWT para `POST /brokers/auth/api-token`
3. informar `brokerId`
4. receber token de acesso para as chamadas seguintes

## Criação de cotação

Endpoint integrado:

- `POST /brokers/quote`

Campos atualmente mapeados no ERP:

- `plate`
- `cep`
- `user.cpf`
- `user.name`
- `user.surname`
- `user.gender`
- `user.birth_date`
- `vehicle_fipe_code`
- `vehicle_model_year`
- `under_24`
- `is_insured`
- `previous_bonus`
- `broker_commission_percentage`
- `insurer_code` quando houver renovação

## Estado atual da implementação

- provider `Justos` registrado no multicálculo
- autenticação JWT `ES256` implementada
- client HTTP para token e cotação implementado
- worker de `Quotes` processa pendências usando providers configurados
- quando faltam dados obrigatórios, o provider retorna `restriction`
- quando faltam credenciais ou token, o provider retorna `login_invalid`
- quando a resposta é válida, a cotação é persistida como opção no fluxo canônico de `Quotes`

## Limites atuais

- parsing detalhado das coberturas da resposta ainda está em evolução
- ainda não há polling separado para a Justos
- ainda não há suporte a catálogos auxiliares específicos da seguradora

## Configuração

Seções de configuração adicionadas:

- `Quotes:Providers:Justos:BaseUrl`
- `Quotes:Providers:Justos:BrokerId`
- `Quotes:Providers:Justos:Issuer`
- `Quotes:Providers:Justos:PrivateKeyPemPath`
- `Quotes:Providers:Justos:DefaultCommissionPercentage`
