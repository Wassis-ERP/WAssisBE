# Quotes Provider - Liberty / Yelum

## Fonte oficial analisada

- arquivo OpenAPI recebido: `liberty-brazil-business-systems-marketplace.yaml`
- titulo da especificacao: `MulticalculoContratoAPI`
- contato publicado no arquivo: `Yelum Seguros - Sistemas - Equipe CPL`

## O que a documentacao recebida ja confirma

- a especificacao esta em `OpenAPI 3.0.1`
- a API cobre jornada de multicaculo para `cotacao`, `proposta`, `consulta de proposta` e emissao de PDFs
- ha endpoints de apoio para `broker`, `branch`, dominios cadastrais e listas de apoio comercial
- ha jornadas de `medical subscription` / `subscricao medica`
- o conjunto de schemas mostra estruturas para linhas como `auto`, `vida`, `residencial`, `empresarial` e `seguro viagem`
- parte relevante das jornadas usa rotas `newsell`, principalmente para `Cotacao`, `Proposta`, `ObterProposta` e PDFs

## Endpoints que mais importam para o ERP

- `POST /api/newsell/v{version}/Cotacao`
- `POST /api/newsell/v{version}/Proposta`
- `POST /api/newsell/v{version}/ObterProposta`
- `POST /api/newsell/v{version}/ObterPDFBoleto`
- `POST /api/newsell/v{version}/ObterPDFCotacao`
- `POST /api/newsell/v{version}/ObterPDFProposta`
- `POST /api/newsell/v{version}/Apolice/ObterPDFApolice`
- `POST /api/v{version}/Quote`
- `GET /api/v{version}/Quote/{BrokerProposalNumber}`
- `POST /api/v{version}/Proposal`
- `GET /api/v{version}/GetProposal/{ProductCode}/{ProposalNumber}`

## O que ainda nao esta claro na especificacao

- o arquivo nao expõe `securitySchemes` nem descreve claramente o fluxo de autenticacao
- a URL base publicada nao aparece de forma explicita no documento
- o produto prioritario para a W.Assis ainda precisa ser escolhido dentro das jornadas disponiveis
- tambem falta validar se a parceria alvo e de fato Liberty legada, Yelum, ou outro canal interno do mesmo grupo

## O que faz sentido modelar no ERP a partir desta doc

- provider `Liberty` ou `Liberty / Yelum` no multicaculo
- configuracao dedicada em `Quotes:Providers:Liberty`
- pre-validacao operacional por ambiente, versao e credenciais
- definicao explicita da `ProductLine` antes de implementar chamada real
- mapeamento inicial de operacoes de `cotacao`, `proposta`, `consulta` e `download de PDF`

## Requisitos de configuracao sugeridos neste momento

- `BaseUrl`
- `DocumentationSource`
- `ProductLine`
- `ApiVersion`
- `BrokerCode`
- `BrokerBranchCode`
- `CommercialProductCode`
- `User`
- `ClientId`
- `ClientSecret`
- `AccessToken` ou outro mecanismo que a parceria confirmar

## Estado atual recomendado

- ainda nao implementar endpoint real de cotacao com base apenas neste YAML
- primeiro confirmar autenticacao, URL base, linha de negocio e contrato de resposta alvo
- a documentacao ja e suficiente para registrar a seguradora no backlog tecnico e preparar o desenho do adapter

## Observacao importante

O arquivo recebido permite documentar o escopo funcional da API, mas nao sustenta sozinho a implementacao de autenticacao ou de ambiente produtivo. O caminho seguro e tratar esta seguradora como `documentada parcialmente` ate a parceria confirmar credenciais, base URL e jornada comercial.
