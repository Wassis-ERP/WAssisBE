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

Isso comprova a fixture Testcontainers/Docker deste checkpoint; não deve ser
confundido com evidência de grants, storage, limites de runtime ou deploy em
Production. Docker Compose completo, migration em banco real e deploy não foram
executados.
