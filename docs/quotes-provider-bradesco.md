# Quotes Provider - Bradesco Seguros

## Fonte oficial

- [Portal de APIs Bradesco Seguros](https://apiportal.bradescoseguros.com.br/pages/Portal_UI_Bundle/index.html#!)
- [Documentacao tecnica / credenciais](https://apiportal.bradescoseguros.com.br/pages/Portal_UI_Bundle/documentacaotecnica/credenciais.html)

## O que a documentacao publica ja confirma

- o consumo de APIs da Bradesco passa por credenciais de parceiro
- existe fluxo com `OAuth`
- parte das jornadas menciona `JWT`
- parte das jornadas menciona `certificado cliente` para `mTLS`
- o endpoint de autenticacao aparece no portal tecnico como `https://susc.hml.bradescoseguros.com.br:8443/V3/Auth`

## O que ficou implementado no ERP

- modulo `Bradesco/Auto` criado dentro de `Carriers`
- provider `Bradesco Seguros Auto` registrado no multicálculo
- configuracao dedicada em `Quotes:Providers:BradescoSeguros:Auto`
- pre-validacao operacional por requisito
- exposicao do status pelo endpoint `GET /api/quotes/providers`

## Requisitos de configuracao atualmente modelados

- `ProductLine`
- `TokenUrl`
- `ClientId`
- `ClientSecret`
- `ClientCertificatePath` ou `ClientCertificateThumbprint` quando `RequiresMutualTls=true`

## Estado atual

- o provider ainda nao chama a API real de cotacao
- o provider ja retorna status operacional e lacunas de configuracao
- a modelagem atual assume `auto` como primeiro ramo priorizado para evolucao nessa seguradora
- o proximo passo depende do mapeamento da jornada/produto exato que sera vendido pela corretora

## Observacao importante

O portal publico da Bradesco nao foi usado para inventar endpoint de cotacao. O que entrou no ERP foi apenas o que a documentacao publica realmente sustenta: requisitos de autenticacao, necessidade de parceria e necessidade de definir a jornada/produto alvo.
