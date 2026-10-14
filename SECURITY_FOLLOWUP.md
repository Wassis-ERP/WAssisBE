# Checkpoint de segurança — 16/09/2026

Base: WAssisBE `7bea743`, WassisCRM `be9627e`.
Branch de continuação nos dois repositórios: `codex/security-remediation-followup`.
Primeiro checkpoint: backend `42a86ca`, frontend `d98b84c`.
Este documento complementa `SECURITY_REMEDIATION.md`; não certifica Production.

## Correções implementadas

- SEC-12: Gitleaks CLI oficial 8.30.1, checksum fixo Windows/Linux, histórico
  completo e saída redigida. Resolve a dependência de licença da action anterior.
- SEC-07/12: NuGet Audit bloqueia restore/build por vulnerabilidades diretas ou
  transitivas (low a critical) e falhas da fonte de auditoria. Configuração também
  copiada antes do restore Docker. Relatório sozinho não era um gate de falha.
- SEC-05: sessão e permissões usam conexões curtas independentes do DbContext da
  requisição, evitando capturar contexto anônimo durante OnTokenValidated.
  CRM descarta o CSRF anônimo depois do login e busca token da identidade atual.
- SEC-01/02/05: vínculos de filial são reconstruídos do banco durante validação da
  sessão, sem depender de claims antigos. Novo vínculo permite seleção sem novo
  login; remoção perde acesso imediatamente. Sessão sem vínculo válido é recusada.
  Login com profile/vínculo inválido retorna 401 genérico, sem emitir cookie novo.
- SEC-01/02: autorização e dados usam a mesma filial. Permissões sem filtro de
  branch retornam todos os vínculos vigentes para o seletor de corretoras do CRM.
- SEC-01: PROPRIO restringe Segurados por produtor/gerente ligado ao profile e
  Oportunidades por responsável, em listagem, ID e escrita. Outros módulos negam
  PROPRIO até implementarem ownership. Escopos desconhecidos não concedem acesso.
- SEC-01: campos de ownership também são tokens de concorrência EF. Reatribuição
  entre leitura e escrita impede UPDATE/DELETE antigo. Conflito retorna 409 genérico.
  Migration `20260916203520_ProtectConcurrentRecordOwnership` atualiza apenas o
  snapshot; Up/Down não alteram esquema físico ou dados.
- SEC-04: orçamentos independentes por usuário, tenant e IP, compartilhados entre
  réplicas. Limite do atributo por usuário, 10x tenant e 20x IP; anônimo por IP.
  Consumo persiste mesmo quando a transação de negócio sofre rollback.
- Administração: três operações usam transação existente com savepoint ou criam
  transação própria. Erro ou retorno antecipado desfaz a operação sem encerrar a
  transação externa. Middleware RLS também desfaz respostas HTTP >=400.
- Administração: datas PostgreSQL/Npgsql são lidas como DateOnly; parâmetros
  Dapper enviados como DbType.Date. Datas preenchidas e nulas foram verificadas.

## Evidências locais

- .NET SDK 10.0.401 instalado via WinGet/Microsoft; ferramenta EF 10.0.12 restaurada.
- Restore locked aprovado com `--no-http-cache`; não houve downgrade para contornar cache.
- 117 testes sem PostgreSQL aprovados; quatro testes de integração PostgreSQL
  aprovados, cobrindo múltiplas jornadas/assertivas cada. Execução final conjunta:
  121/121 aprovados. Build Release da solução: zero avisos e zero erros.
- PostgreSQL 16.15 oficial em cluster temporário, loopback, senha aleatória e
  TLS VerifyFull com certificado de teste. Nenhum banco da aplicação foi utilizado.
- Fixtures HTTP usam role LOGIN sem SUPERUSER, ownership ou BYPASSRLS. As permissões
  amplas concedidas nas tabelas da fixture servem para testar RLS e NÃO constituem
  uma proposta de grants mínimos para Production.
- Jornadas HTTP: tenant/filial, PROPRIO, permissão alterada sem novo login, vínculos
  concedidos/removidos durante sessão, cookie HttpOnly/Secure, CSRF anônimo recusado,
  logout e replay do cookie revogado, CRUD, proteção do último Master e auditoria.
- Concorrência: ownership alterado após leitura reproduziu escrita indevida antes
  da correção; agora gera DbUpdateConcurrencyException. Testado com oportunidades
  e vínculo de produtor de segurados.
- Savepoints: recusa de remover último Master preserva vínculo; criação bem-sucedida
  de perfil respeita rollback externo. Datas HTTP, criação de perfil e concorrência
  de inativação de dois Masters cobertas.
- Outbox/inbox: atomicidade, leases/claims concorrentes e recuperação; upgrade desde
  `20260912120359_AlignFrontendV31ConnectedContracts`, antes da migration de outbox.
- RLS: consultas e escritas SQL diretas com role comum, tenant/filial divergentes,
  contexto ausente e conexão reutilizada; jornadas HTTP executadas sob RLS.
- NuGet: solução sem avisos de vulnerabilidade; fixture externa com Newtonsoft.Json
  12.0.3 falhou com NU1903 como esperado. Não foi adicionado pacote vulnerável ao produto.
- CRM: 334 testes, lint e build passaram no primeiro checkpoint. Avisos conhecidos
  de fontes Mulish/Rubik permanecem. Nenhuma alteração de tela/contrato database.ts.

## Execução reproduzível

Com Docker disponível, os quatro testes usam Testcontainers PostgreSQL 16-bookworm,
certificado efêmero e VerifyFull. Alternativa Windows validada localmente:

```powershell
./scripts/test-security-postgres.ps1 -PostgresBinDirectory '<binários PostgreSQL>\bin'
```

O script cria cluster descartável, executa `Category=Postgres`, para o servidor e
remove senha/chave privada. Dados/logs temporários ficam para diagnóstico. Aceita
`-Filter` para um teste específico. Nunca fornecer banco compartilhado/de produção.

## Gates GitHub

- Frontend `d98b84c`: [CI 35145654570](https://github.com/Wassis-ERP/WassisCRM/actions/runs/35145654570)
  aprovado, incluindo testes/build/auditoria e scan de imagem.
- Backend `42a86ca`: [CI 35145647720](https://github.com/Wassis-ERP/WAssisBE/actions/runs/35145647720)
  passou Gitleaks/restore/build e falhou nos testes existentes. Esta continuação
  corrige as fixtures TLS, datas e transações que impediam a suíte local completa.
- A versão Docker da nova fixture e os gates do próximo commit precisam de execução
  remota após push. Não confundir validação local PostgreSQL com validação Docker.

## Pendências reais

1. Validar gates GitHub do novo commit e resolver falhas sem desativar controles.
2. Ampliar matriz em todos os módulos/filhos/Dapper e worker. Não considerar SEC-01,
   SEC-02 ou SEC-09 encerrados pelos testes de Segurados/Oportunidades.
3. Auth0/Production: autenticação, MFA, recuperação, rotação, idle timeout e jornada
   de reload no navegador. Provedor de HML continua proibido em Production.
4. PDF/OCR: streaming/storage privado/fila durável e processo encerrável, limites de
   CPU/memória/objetos/descompressão. Quotas atuais são de frequência, não de custo
   por seguradora. Parser PDF ainda síncrono e em memória: SEC-04 não está encerrado.
5. RLS quotes/worker, grants operacionais mínimos, backfill revisado, retenção,
   backup/restore/PITR e evidências externas. Sem migrations em dados reais aqui.
6. Revisar GRUPO em endpoints tenant-wide e consolidação somente de corretoras
   autorizadas quando exigido pelo contrato. Completar ciclo administrativo.
7. Smoke no navegador com frontend/backend reais; os testes HTTP não são browser E2E.

Nenhum deploy ou alteração de dados reais foi executado nesta continuação.

## Continuação — Auth0, sessão e RLS de cotações/worker

Base inspecionada antes das mudanças: WAssisBE `039b55b`, WassisCRM `ae41f12`,
ambos limpos e acompanhando `origin/codex/security-remediation-followup`. O run
backend 35148569842 desse HEAD ainda falhava no passo Test; os logs não estavam
disponíveis sem permissão administrativa no repositório. Os workflows agora também
executam automaticamente em push nessa branch, para produzir nova evidência.

Implementado:

- BFF Auth0 com OIDC confidencial Authorization Code + PKCE usando o handler
  Microsoft 10.0.12. Production não seleciona mais o autenticador de homologação e
  falha no startup se Auth0, redirects HTTPS ou segredo externo estiverem ausentes.
- `security.external_identities` mapeia somente `issuer/sub` explicitamente
  provisionado para profile local. Não existe backfill automático. Resolver exige
  tenant, profile, filial, perfil e vínculo vigentes e deriva funções apenas para
  compatibilidade; a matriz persistida continua decidindo cada ação.
- Perfil com `configuracoes/manage` exige `amr=mfa`. Universal Login é o limite de
  confiança para enrollment, recuperação e convite. Essas políticas ainda precisam
  ser configuradas e evidenciadas no tenant Auth0 real.
- Sessão ganhou idle timeout server-side (30 minutos) e janela revisável de rotação:
  a chave atual assina; chaves anteriores fortes e distintas apenas validam. O CRM
  usa o mesmo timeout e nunca recebe/persiste o token.
- Quotes/opções/filhos receberam FORCE RLS tenant+filial. A outbox guarda filial
  derivada do pai; legado ambíguo vai para `NeedsReview`. O worker abre unidades DB
  curtas com contexto transacional local e mantém chamadas a seguradoras fora delas.
  Grants propostos foram reduzidos e API/worker seguem sem `BYPASSRLS`.

Evidências deste checkpoint:

- restore locked com SDK 10.0.401 e EF 10.0.12: aprovado;
- build Release: zero avisos/erros; 124/124 testes sem PostgreSQL aprovados;
- migration model check sem divergência; script idempotente gerado com sucesso;
- NuGet e npm audit: zero vulnerabilidades reportadas nas fontes consultadas;
- CRM: lint sem avisos, 335/335 testes e build backend/Auth0 aprovados;
- Gitleaks 8.30.1: 55 commits BE e 75 commits CRM, zero achados, saída redigida;
- smoke local: `/health` do BE retornou apenas `{"status":"ok"}`; navegador
  carregou Dashboard e a lista de Segurados do CRM em modo mock.

Não executado neste host:

- as quatro integrações PostgreSQL/TLS/RLS atualizadas, porque não há daemon Docker
  nem binários PostgreSQL locais. A nova cobertura (external identity, idle timeout,
  quote/filhos, role worker e pool sem contexto) não é declarada aprovada;
- login/logout/recovery/MFA contra Auth0, por faltar tenant, client/secret e vínculos
  externos provisionados;
- build/scan de imagem local e qualquer migration/deploy em infraestrutura real.

Continuam bloqueadores: pipeline PDF/OCR ainda síncrono/em memória e sem processo
encerrável; orçamento de custo por seguradora; validação CI/PostgreSQL do novo RLS;
provisionamento/políticas/evidências Auth0; grants, backup/restore/PITR e retenção.

## Continuação — pipeline privado e isolado de documentos

O bloqueador de arquitetura do upload foi reduzido sem aplicar migration ou tocar em
dados reais:

- multipart é lido diretamente do corpo HTTP com limite de 8 MB e assinatura PDF;
  o arquivo não é mais materializado pelo model binder da API;
- storage privado usa diretório absoluto configurado, IDs de tenant/filial/documento
  gerados/validados no servidor e não incorpora o nome fornecido no caminho;
- persistência de `ImportedDocument` e criação da outbox `documents.parse` são
  atômicas; a outbox leva filial explícita e o worker abre contexto RLS transacional;
- PdfPig/OCR rodam em processo filho descartável, fora da API e da transação, com
  timeout, limite observado de memória/CPU, limite de saída e concorrência. Timeout
  ou cancelamento encerra toda a árvore de processos;
- API não retorna a chave física do storage. O CI agora publica, constrói e examina
  uma imagem não-root dedicada ao worker, além da imagem da API.

Evidências locais desta etapa:

- restore locked `--no-http-cache`, SDK 10.0.401 e EF 10.0.12: aprovado;
- build Release da solução: zero avisos/erros;
- 129/129 testes sem PostgreSQL aprovados, inclusive storage privado, chave física
  ausente da resposta e PDF corrompido contido no processo filho;
- modelo EF sem alterações pendentes; script idempotente gerado (365.255 bytes);
- auditoria NuGet sem pacote vulnerável reportado nas fontes consultadas.

As quatro integrações PostgreSQL foram solicitadas, mas não chegaram a iniciar: o
Testcontainers não conectou a `npipe://./pipe/docker_engine` e não há binários locais.
Portanto, as novas assertivas de Auth0/RLS/worker/outbox permanecem **não executadas**
neste host até o CI. Também não foram executados build/scan local das imagens.

Ainda é necessário impor e comprovar limites rígidos do runtime (cgroup/Job Object),
provisionar storage privado compartilhado e observabilidade/backpressure, validar um
corpus PDF/OCR representativo e hostil e definir orçamento monetário por seguradora.
Frequência distribuída não é evidência desse orçamento. O sistema continua sem
condição de liberação para Production.

Fontes oficiais consultadas para a integração: documentação Microsoft do cliente
OIDC confidencial/BFF no ASP.NET Core 10
(`https://learn.microsoft.com/aspnet/core/security/authentication/configure-oidc-web-authentication?view=aspnetcore-10.0`)
e documentação Auth0 de Authorization Code + PKCE e Universal Login
(`https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow-with-pkce/add-login-using-the-authorization-code-flow-with-pkce`,
`https://auth0.com/docs/authenticate/login/auth0-universal-login/universal-login-vs-classic-login/universal-experience`).

## Checkpoint corretivo — Testcontainers e imagens

O CI backend do commit `b7f643c` confirmou restore, scanner e build, mas falhou no
passo de testes. A falha foi reproduzida localmente com Docker Desktop: a fixture
passava `postgres` como primeiro argumento para uma imagem cujo entrypoint já o
executa, fazendo o servidor rejeitar esse argumento posicional. A fixture agora
envia somente as opções `-c` de TLS. As factories HTTP PostgreSQL também recebem um
diretório privado efêmero e único para documentos, sem relaxar a validação de
configuração de Staging/Production.

Evidências após a correção:

- suíte backend completa: **133/133 aprovados**, zero falhas e zero ignorados;
- quatro integrações executadas em PostgreSQL 16 real via Testcontainers, com
  certificado efêmero e `SslMode=VerifyFull`;
- cenários cobertos: role comum sem ownership/`BYPASSRLS`, HTTP/CSRF, sessão e
  replay, permissão/filial alterada durante a sessão, ownership/concorrência,
  rollback administrativo, RLS/pool e outbox/upgrade;
- imagens locais API e worker construídas com contexto reduzido (a árvore `.dotnet`
  é excluída) e usuário não-root UID 1654;
- Trivy 0.69.3, severidades HIGH/CRITICAL, `--ignore-unfixed` e exit code bloqueante:
  zero vulnerabilidades corrigíveis nas duas imagens.
- o CI do commit `81af4f3` repetiu com sucesso todos os gates acima no run
  35157591250.

Isso comprova a fixture Testcontainers/Docker deste checkpoint; não deve ser
confundido com evidência de grants, storage, limites de runtime ou deploy em
Production. Docker Compose completo, migration em banco real e deploy não foram
executados.

## Atualização de dependências — 09/10/2026

O run 37962842731 passou scanner de segredos, restore, build, 148 testes e
auditoria NuGet, mas falhou no scan Trivy da imagem API. A reprodução sobre o
digest fixado do ASP.NET 10.0.12 encontrou duas ocorrências HIGH corrigíveis da
mesma `CVE-2026-84782` em `libssl3t64` e `openssl` (3.0.13-0ubuntu3.15). Os
Dockerfiles da API e do worker agora fixam a republicação oficial corrente do
mesmo runtime, com OpenSSL 3.0.13-0ubuntu3.16 ou superior.

Trivy 0.75.0, baixado do release oficial e validado pelo checksum publicado,
retornou zero HIGH/CRITICAL corrigíveis para o novo digest remoto. O SDK permanece
10.0.401 e o runtime 10.0.12; não houve afrouxamento de severidade, `ignore-unfixed`
ou exit code. O scan da imagem final, incluindo os assemblies publicados, continua
sendo repetido pelo CI após o push.

## Continuação — 09/10/2026: navegador, filiais da sessão e idempotência

Fetch/status/log confirmaram BE `3752478` e CRM `0295172`, com os CIs
37965321761 e 37965793466 aprovados. As árvores estavam limpas antes deste recorte.

O teste de navegador real revelou regressões que o build e os testes Staging não
detectavam: DI da API Development dependia do parser registrado só no worker;
bootstrap Production do CRM omitia o provedor Auth0; seletor de filial exigia
permissão administrativa GRUPO; conclusão após reload dependia do snapshot de login
ausente. Esses quatro pontos foram corrigidos e exercitados.

`GET /api/identity/me/branches` entrega somente ID/nome/atividade das filiais
vigentes da própria sessão. As claims utilizadas são renovadas na autenticação
pelos vínculos persistidos; o contexto dos repositórios permanece limitado à filial
selecionada. O diretório `/api/core/branches` continua exigindo `plataforma/read`
GRUPO. A nova cobertura PostgreSQL verifica negação administrativa, duas filiais,
concessão/remoção durante a sessão e usuário sem vínculo; não houve ampliação de
acesso tenant-wide ou de PROPRIO.

O handler legado de quotes aceitava a mesma chave com conteúdo diferente. Agora
compara a entrada persistida e usa lock transacional PostgreSQL tenant/chave:
retry equivalente retorna o ID anterior, conteúdo diferente gera 409 sanitizado.
RLS oculta outra filial, inclusive no conflito do índice único. O teste com oito
conexões concorrentes confirmou um pedido/uma outbox, e cobre disputa de versões,
reuso por outro tenant e cancelamento sem gravação parcial. Sem nova migration.

Evidências locais finais:

- SDK 10.0.401 / EF 10.0.12, restore locked `--no-http-cache`, build Release:
  aprovados, zero avisos/erros; modelo EF sem alteração pendente;
- 152/152 testes sem PostgreSQL e 6/6 integrações PostgreSQL 16.15 real/TLS
  `VerifyFull`: aprovados, nenhum ignorado;
- CRM: 337/337 testes, lint, build backend/Auth0 e 1/1 smoke Production aprovados;
- Playwright conectado: 1/1 aprovado, com API real e role não-owner/NOBYPASSRLS,
  criação/edição/reload/ganho/lista/Kanban/logout/nova sessão. GET/POST/PUT do BE
  observados; destinos externos bloqueados; Cálculos/Tarefas exibem a pendência;
- revisão visual a 1440×900, sem sobreposição ou overflow horizontal. Capturas
  sintéticas permanecem em `.screens/e2e`, ignoradas no Git;
- NuGet e npm audit: zero vulnerabilidades reportadas. Gitleaks 8.30.1 em árvores
  e histórico local disponível (61 commits BE / 79 CRM antes deste checkpoint):
  zero achados, saída redigida.

Reprodução: `scripts/test-security-postgres.ps1 -PostgresBinDirectory <bin>
-ConnectedCrmDirectory <CRM/nexus-crm>`. A opção `-ConnectedOnly` repete só o
navegador e anuncia explicitamente que não executa a categoria PostgreSQL.
Fixtures usam cluster/banco/roles exclusivos e credenciais efêmeras. Seus grants
amplos não são um modelo operacional. Bancos/roles do navegador foram excluídos,
seus processos encerrados e clusters parados; senha e chave TLS privadas removidas.

Limites: PostgreSQL Windows não comprova Testcontainers/Docker; Docker Desktop
local está indisponível e imagens finais não foram construídas/examinadas neste
recorte local. O smoke Production intercepta um 401 da identidade e não comprova
Auth0 real. Nenhuma migration em dados reais e nenhum deploy foram executados.

Próxima fatia canônica depende de consolidar **Segurados e Oportunidades juntos**:
as APIs persistem `public.*` e as FKs de cálculo apontam para `erp.*`. Não foram
copiados vínculos antigos nem criado dual-write. Cálculos, execuções, resultados,
renovação própria e apresentações conectados continuam pendentes. O CRM não tem
client Supabase remoto: o arquivo de mesmo nome é adapter demonstrativo em memória,
recusado em modo conectado/Production. Os testes comprovam BE nos módulos citados,
não integração de todos os módulos do ERP.

Continuam pendentes tenant/configuração e evidências Auth0/MFA/recuperação/rotação,
OCR real e corpus hostil, limites rígidos do worker/storage compartilhado,
orçamento de custo por seguradora, reconciliação do legado, grants operacionais,
retenção e evidências backup/restore/PITR. O sistema não está liberado para Production.

### Gate local de segredos

O ensaio de publicação confirmou um falso negativo do hook: `--pre-commit` sem
`--staged` examinava zero bytes depois do `git add`. Corrigido nos dois repositórios
com `--staged`, modo executável e LF explícito. A regressão usa um repositório
temporário e credencial sintética gerada: o hook real deve impedir o commit em
stage, aceitar o commit limpo e redigir toda saída. Os dois ensaios Windows
passaram; o CI ganhou o mesmo gate, além da varredura de histórico existente.
Nenhum segredo real foi introduzido e os repositórios temporários foram removidos.
Comandos e fonte oficial constam de `docs/security-scanning.md`.

### CI do checkpoint funcional

BE `aeb00c0`: [run 37982882040](https://github.com/Wassis-ERP/WAssisBE/actions/runs/37982882040)
integralmente aprovado, incluindo Testcontainers/PostgreSQL, build/publish e scans
das imagens finais API/worker. CRM `39153b9`:
[run 37982890014](https://github.com/Wassis-ERP/WassisCRM/actions/runs/37982890014)
integralmente aprovado, incluindo smoke do bundle Production e scan da imagem CRM.
Isso comprova os artefatos do CI; não comprova deploy, Auth0 real ou infraestrutura
operacional. O ajuste posterior de hooks preserva o código funcional validado e
será reexecutado automaticamente pelo CI do novo HEAD.

### Integração das telas de cálculo — 14/10/2026

Recorte incremental sobre `codex/calculation-historical-insurers` (`792b777`).
A jornada do CRM encontrou uma regressão em `Opportunity.Update`: limpar a
navegação `Stage` quando `stage_id` não mudava fazia o relationship fixup do EF
anular a FK obrigatória. A navegação agora é preservada nessa situação; a troca
explícita de etapa continua suportada. Nenhum filtro, trigger, RLS ou validação de
escopo foi removido.

O teste HTTP PostgreSQL existente foi ampliado para alterar prêmio/data mantendo
a etapa, reler ambos os campos e a FK, e depois trocar a etapa. Passou com a role
comum da aplicação. A fixture do navegador recebeu fuso e catálogos sintéticos
para o cálculo Auto e o limite de cinco resultados; são somente dados de teste
descartáveis, sem backfill de registros reais e sem migration nova.

Validação local: SDK 10.0.401, restore `--locked-mode --no-http-cache`, build API
Release com zero avisos/erros, 161 testes sem PostgreSQL e oito integrações reais
com TLS `VerifyFull` aprovados (zero ignorados). Reprodução PostgreSQL:
`scripts/test-security-postgres.ps1 -PostgresBinDirectory <bin>`.
A jornada do navegador é executada separadamente com `-ConnectedOnly` e fica
documentada no micro-plano do CRM. CI e evidência do navegador serão registrados
após os gates dos commits publicados.

Esta fatia não comprova deploy, Auth0 real/MFA, OCR, orçamento por seguradora,
grants operacionais, retenção ou backup/restore/PITR. Os bloqueadores anteriores
de Production continuam pendentes.
