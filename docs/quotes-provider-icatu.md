# Quotes Provider - Icatu Seguros

## Fonte oficial

- [Portal de APIs Icatu Seguros](https://portal-api.icatuseguros.com.br/apis)

## O que a documentacao publica ja confirma

- o acesso as APIs depende de parceria e catalogo habilitado
- o portal centraliza `APIs`, `Produtos` e `Jornadas`
- a autenticacao depende de identificadores e credenciais de consumo do parceiro

## O que ficou implementado no ERP

- provider `Icatu Seguros` registrado no multicálculo
- configuracao dedicada em `Quotes:Providers:IcatuSeguros`
- pre-validacao operacional por requisito
- exposicao do status pelo endpoint `GET /api/quotes/providers`

## Requisitos de configuracao atualmente modelados

- `ProductLine`
- `ApiCatalogKey`
- `ClientId`
- `ClientSecret`
- `PartnerId`
- `ApplicationId`
- `CertificateId`

## Estado atual

- o provider ainda nao chama a API real de cotacao
- o provider ja retorna status operacional e lacunas de configuracao
- o proximo passo depende do detalhamento do catalogo e da jornada liberada para a parceria W.Assis

## Observacao importante

Como o portal publico da Icatu e mais fechado do que o da Justos, o provider foi estruturado para nao mascarar incerteza. Ele deixa explicito o que falta para a integracao real, em vez de fingir suporte a um endpoint que ainda nao foi validado.
