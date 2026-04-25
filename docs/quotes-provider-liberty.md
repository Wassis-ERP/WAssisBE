# Quotes Provider - Liberty / Yelum

## Fonte oficial analisada

- arquivo OpenAPI recebido: `liberty-brazil-business-systems-marketplace.yaml`
- titulo da especificacao: `MulticalculoContratoAPI`
- contato publicado no arquivo: `Yelum Seguros - Sistemas - Equipe CPL`

## O que a documentacao recebida ja confirma

- a especificacao esta em `OpenAPI 3.0.1`
- a API se apresenta como multicaculo, mas a descricao inicial do arquivo menciona `Frotas`
- a API cobre jornada de multicaculo para `cotacao`, `proposta`, `consulta de proposta` e emissao de PDFs
- ha endpoints de apoio para `broker`, `branch`, dominios cadastrais e listas de apoio comercial
- ha jornadas de `medical subscription` / `subscricao medica`
- o conjunto de schemas mostra estruturas para linhas como `auto`, `vida`, `residencial`, `empresarial` e `seguro viagem`
- parte relevante das jornadas usa rotas `newsell`, principalmente para `Cotacao`, `Proposta`, `ObterProposta` e PDFs

## Endpoints que mais importam para o ERP

- `POST /api/v{version}/Quote`
- `PUT /api/v{version}/Quote/{BrokerProposalNumber}`
- `POST /api/newsell/v{version}/Cotacao`
- `POST /api/newsell/v{version}/Proposta`
- `POST /api/newsell/v{version}/ObterProposta`
- `POST /api/newsell/v{version}/ObterPDFBoleto`
- `POST /api/newsell/v{version}/ObterPDFCotacao`
- `POST /api/newsell/v{version}/ObterPDFProposta`
- `POST /api/newsell/v{version}/Apolice/ObterPDFApolice`
- `GET /api/v{version}/Quote/{BrokerProposalNumber}`
- `POST /api/v{version}/Proposal`
- `GET /api/v{version}/GetProposal/{ProductCode}/{ProposalNumber}`

## O que o YAML sugere especificamente para `auto`

- o endpoint `POST /api/v{version}/Quote` usa o schema generico `CriarCotacaoRequest`
- dentro desse request existe `Items.Vehicles[]`, que referencia o schema `CriarCotacao.Auto.Veiculo`
- isso e o indício mais forte para a implementacao futura de `auto`
- ja o endpoint `POST /api/newsell/v{version}/Cotacao` aponta para o schema `CotacaoNewLifeCommand`, entao ele nao pode ser tratado como rota confiavel de `auto` sem validacao adicional da seguradora

## O que ainda nao esta claro na especificacao

- o arquivo nao expõe `securitySchemes` nem descreve claramente o fluxo de autenticacao
- a URL base publicada nao aparece de forma explicita no documento
- o produto prioritario para a W.Assis ainda precisa ser escolhido dentro das jornadas disponiveis
- tambem falta validar se a parceria alvo e de fato Liberty legada, Yelum, ou outro canal interno do mesmo grupo
- tambem falta validar se a descricao inicial de `Frotas` e apenas heranca do contrato ou se restringe parte das jornadas publicadas

## O que faz sentido modelar no ERP a partir desta doc

- providers separados por ramo da seguradora
- configuracoes dedicadas em:
- `Quotes:Providers:Liberty:Auto`
- `Quotes:Providers:Liberty:Life`
- `Quotes:Providers:Liberty:Residence`
- `Quotes:Providers:Liberty:Business`
- `Quotes:Providers:Liberty:Travel`
- pre-validacao operacional por ambiente, versao e credenciais
- definicao explicita da `ProductLine` antes de implementar chamada real
- mapeamento inicial de operacoes de `cotacao`, `proposta`, `consulta` e `download de PDF`
- preservacao da arquitetura por ramo da seguradora sem introduzir um campo global novo no contrato canonico de cotacao
- apesar de a especificacao citar varios ramos, a prioridade operacional definida agora para evolucao e `auto`

## Ramos mapeados a partir do OpenAPI

- `auto`
- `vida`
- `residencial`
- `empresarial`
- `seguro viagem`

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

## Como ficou modularizado no codigo

- providers implementados em:
- `src/WAssis.Infra.Data/Integrations/Modules/Quotes/Carriers/Liberty/Auto`
- `src/WAssis.Infra.Data/Integrations/Modules/Quotes/Carriers/Liberty/Life`
- `src/WAssis.Infra.Data/Integrations/Modules/Quotes/Carriers/Liberty/Residence`
- `src/WAssis.Infra.Data/Integrations/Modules/Quotes/Carriers/Liberty/Business`
- `src/WAssis.Infra.Data/Integrations/Modules/Quotes/Carriers/Liberty/Travel`
- opcoes de configuracao em `src/WAssis.Infra.Data/Configuration/Liberty*QuoteOptions.cs`
- registro de DI em `InfraDataServiceCollectionExtensions`
- configuracao base adicionada aos `appsettings` da API e do worker

## Estado atual recomendado

- os modulos da Liberty para `auto`, `vida`, `residencial`, `empresarial` e `viagem` ja estao registrados no ERP
- ainda nao implementar endpoint real de cotacao com base apenas neste YAML
- ao aprofundar a integracao, o primeiro ramo a receber chamada HTTP real deve ser `auto`
- para `auto`, o candidato tecnico mais coerente hoje e `POST /api/v{version}/Quote`, nao `newsell/Cotacao`
- primeiro confirmar autenticacao, URL base, linha de negocio e contrato de resposta alvo
- a documentacao ja foi convertida em adapters modulares iniciais, prontos para evoluir sem misturar os ramos da seguradora

## Observacao importante

O arquivo recebido permite documentar o escopo funcional da API, mas nao sustenta sozinho a implementacao de autenticacao ou de ambiente produtivo. O caminho seguro e tratar esta seguradora como `documentada parcialmente` ate a parceria confirmar credenciais, base URL e jornada comercial.
