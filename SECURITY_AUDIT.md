# Auditoria de segurança e preparação para produção — WAssisBE + WassisCRM

**Data da revisão:** 16/09/2026  
**Revisões inspecionadas:** WAssisBE `d9c3b97`; WassisCRM `908d73e`  
**Escopo:** análise estática dos dois repositórios, histórico Git local disponível, configurações de build/deploy, migrations, testes locais não destrutivos e consultas a fontes oficiais.  
**Regra de classificação:** “falha confirmada” significa que o controle está ausente ou é contornável pelo caminho de código inspecionado; “risco potencial” depende de configuração, requisito de negócio ou condição externa não disponível; “não verificável” exige evidência do ambiente publicado.

## 1. Resumo executivo

O conjunto **não está pronto para produção** no escopo verificado. Não foi encontrada vulnerabilidade crítica nem segredo de produção confirmado no repositório, e há bons controles de base: isolamento de tenant fail-closed no EF, validação de escrita, SQL parametrizado, JWT validado com expiração/assinatura, cabeçalhos de segurança, imagens de produção por digest, usuário de container não-root, SBOM/Trivy no CI e build do frontend obrigado a usar autenticação/dados do backend.

Há, porém, quatro bloqueadores de lançamento:

1. **Autorização funcional quebrada (SEC-01, alta):** quase todos os módulos de negócio aceitam qualquer identidade `brokerage_staff`. A matriz persistida `role_permissions` só é aplicada no controller de Administração. Um vendedor/agente autenticado pode chamar diretamente operações financeiras, revisão/emissão de apólice, upload/reprocessamento e mutações de CRM, mesmo quando a UI esconde o botão.
2. **Escopo de filial incompleto (SEC-02, alta):** documentos, financeiro, apólices em rascunho, WhatsApp e auditoria têm filtro apenas de tenant, não de filial. Em documentos e financeiro existem buscas/mutações por GUID sem verificação de filial/ownership. A própria ADR existente registra que esse fechamento é requisito anterior a PRD.
3. **Não existe provedor de identidade para Production (SEC-03, alta):** fora de Development o DI seleciona o autenticador de homologação; esse serviço recusa qualquer ambiente que não seja Staging. Também não existem refresh/revogação, recuperação, MFA ou logout servidor.
4. **Consumo de recursos e automação abusiva (SEC-04, alta):** somente o login tem rate limit, local à réplica. Uploads de até 20 MB são duplicados em memória e PDFs não confiáveis são analisados sincronamente, sem limite de páginas/texto/CPU no PdfPig. A análise financeira aceita texto sem limite de negócio e devolve todas as linhas. Cotações podem disparar integrações externas sem quota por usuário/tenant.

Riscos médios relevantes incluem JWT no `localStorage` sem revogação (SEC-05), proteção de dados/backup/TLS do PostgreSQL não demonstrada ou exigida pela aplicação (SEC-06), dependência PDF pré-release antiga e fim de suporte próximo do .NET 8 (SEC-07), respostas de coleção sem paginação (SEC-08) e ausência de RLS como segunda barreira (SEC-09).

**Parecer:** não liberar PRD antes de corrigir SEC-01 a SEC-04 e demonstrar, em PostgreSQL real e com perfis negativos, os limites de tenant/filial. Se a entrada em produção ocorrer em ou após **10/11/2026**, a migração do runtime .NET 8 para uma versão suportada também é bloqueadora.

## 2. Escopo, método e limitações

### Inspecionado

- Rotas/controllers da API, policies, contexto de usuário, emissão/validação JWT, DTOs/validators, handlers e transições principais.
- `WAssisDbContext`, query filters, validação de escrita, migrations, SQL/Dapper, outbox/worker e integrações HTTP.
- Fluxos do CRM até a API, autenticação, armazenamento do token, gating da UI, URLs, renderização, CSP e modo de build.
- Upload e processamento PDF/OCR, uso de processos externos e diretórios temporários.
- Dockerfiles, Compose local, Nginx, workflows GitHub Actions, scripts e documentação operacional.
- Manifests/lockfile, auditorias NuGet/npm, histórico Git local e padrões de segredo de alta confiança.
- XSS, CSRF/CORS, SSRF, redirects, command/path injection, desserialização, cache, webhooks, WebSockets e IA na medida em que existem.

### Limitações

- Nenhum ambiente remoto/produção foi acessado; nenhuma credencial encontrada foi usada.
- Docker Desktop não estava disponível. Três testes Testcontainers/PostgreSQL e scans locais de imagem não puderam executar. As suites sem container passaram.
- Não houve pentest dinâmico autenticado porque não foi permitido usar credenciais presentes no repositório e não existe provedor Production funcional. Os achados de autorização decorrem de caminhos estáticos inequívocos; devem ser convertidos em testes de integração negativos.
- `gitleaks` e `trufflehog` não estão instalados. Foi feita busca de padrões de alta confiança no conteúdo atual e histórico local; ela não substitui scanner dedicado nem histórico remoto completo.
- TLS no edge, WAF, regras de rede, roles reais do PostgreSQL, criptografia de disco/backups, KMS, secrets do Portainer/GitHub, restauração/PITR e alertas são externos aos repositórios e ficaram não verificáveis.
- O changelog bruto do Supabase não pôde ser lido pelo cliente web por tipo de conteúdo; as páginas oficiais foram consultadas. Isso não altera o resultado: o arquivo chamado `supabase.ts` é apenas um adaptador em memória e não usa Supabase em runtime.
- O issue de fuzzing do PdfPig citado em SEC-04/SEC-07 foi confirmado no upstream v0.1.14/master; **não foi demonstrado** que os mesmos crash cases atinjam o fork `1.7.0-custom-5` usado aqui.

## 3. Arquitetura, dados e superfície de ataque

### Fluxo publicado pretendido

`Browser/React/Vite` → HTTPS/Traefik/Nginx → `WAssis.Services.Api` → EF Core/Dapper → PostgreSQL. O worker usa escopo sistêmico explícito, outbox e integrações com seguradoras. O CRM envia JWT Bearer e seleciona filial no contexto de usuário; o servidor deve ser a fronteira definitiva de autorização.

### Limites de confiança

- **Não confiável:** navegador, payloads/IDs/nomes de arquivo, PDF, texto de demonstrativo, parâmetros de busca, respostas de terceiros.
- **Fronteira de autenticação:** JWT emitido pelo backend. O CRM não é fronteira de segurança.
- **Fronteira multi-tenant:** claims `tenant_id`/filiais → `ICurrentUserContext` → filtros EF/SQL explícito. O banco não adiciona RLS.
- **Capacidade privilegiada:** `SystemDataScope` do worker e credencial do banco. Compromisso do processo/role de aplicação ignora a separação em código.
- **Dados sensíveis:** CPF/CNPJ/documentos, CNH, renda, endereço, telefones/e-mails, propostas/apólices, prêmios/comissões, mensagens WhatsApp, texto extraído de documentos, credenciais de integrações e chaves JWT.

### Entradas mapeadas

- Login e identidade; Administração; Segurados; Oportunidades; Cotações/provedores; Documentos/upload/OCR; Apólices; Financeiro; Billing; WhatsApp; dashboard; catálogos/core; endpoints de compatibilidade de migração.
- `POST /api/whatsapp/conversations/inbound` é um endpoint autenticado comum, não um webhook público de provedor.
- Não foram encontrados WebSockets/SignalR, GraphQL, mecanismo de cache de aplicação, endpoint público de webhook, upload para webroot ou integração com IA.
- Redis/RabbitMQ aparecem no Compose local, mas o caminho analisado usa outbox/worker em PostgreSQL; portanto, controles de cache/canal não se aplicam ao runtime atual.

## 4. Matriz dos 20 pontos

| # | Controle | Status | Evidência e conclusão |
|---|---|---|---|
| 1 | API keys e segredos | Aprovado no escopo verificado | Não foram encontrados tokens/chaves privilegiadas de alta confiança no código/bundle. `.env` de deploy é ignorado, o CRM só expõe URL/modos/build SHA e o adapter “Supabase” não possui chave. Há credenciais de exemplo de Development rastreadas; o provider é limitado a `IsDevelopment`, mas não se deve expor esse ambiente. |
| 2 | Segredos no Git | Risco potencial | Busca no histórico local por private keys, URLs com senha e formatos de tokens conhecidos não achou exposição confirmada. `.gitignore` cobre secrets usuais, mas não há scanner de segredo/pre-commit no CI e há arquivos ignorados ainda rastreados. Scanner dedicado + histórico remoto continuam necessários. |
| 3 | Chaves públicas/acesso ao banco | Aprovado no cliente; não verificável na infra | Não existe Supabase client/Data API: `nexus-crm/src/lib/supabase.ts:1-21` opera só em memória. O navegador não recebe credencial PostgreSQL. Role, grants e alcance de rede da credencial da API não estão no repo. |
| 4 | RLS/isolamento | Risco potencial + falha de filial | Tenant filters e validação de escrita estão em `WAssisDbContext.cs:54-82,107-124`; Dapper revisado parametriza tenant. Não há RLS (`docs/adr-tenant-scope-and-rls.md:26-34`). Filtragem de filial cobre apenas Segurados/Oportunidades/QuoteRequests, deixando módulos sensíveis tenant-wide (SEC-02/SEC-09). |
| 5 | Criptografia | Risco potencial / não verificável | HTTPS é exigido na URL do CRM e recomendado no deploy. `DatabaseConnectionPolicy.cs:7-18` valida senha/diagnóstico, mas não exige `SslMode=VerifyFull`, e a documentação usa `Trust Server Certificate=true`. Criptografia de volume/backup/KMS, rotação e restore não foram demonstrados (SEC-06). |
| 6 | Autenticação no servidor | Falha confirmada | JWT valida issuer, audience, assinatura, lifetime e expiração (`ServiceCollectionExtensions.cs:50-67`), com segredo forte fora de dev. Porém Production seleciona serviço que só aceita Staging (`:45-48`; `HomologationIdentityAuthenticationService.cs:25-33`) e não há refresh/revogação (SEC-03/SEC-05). |
| 7 | Autorização/restrição | Falha confirmada | `BrokerageStaff` só verifica `user_type` (`ServiceCollectionExtensions.cs:83-85`). Financeiro, documentos, apólices, WhatsApp, cotações e CRM usam essa policy ampla. `role_permissions` é consultado somente em Administração (SEC-01); escopo por filial é incompleto (SEC-02). |
| 8 | Mass assignment | Risco potencial | Controllers projetam requests em commands/DTOs explícitos; `tenantId` vem do contexto e `OfficeBranchId` é validado, evitando binding direto de entidades. Contudo `ResponsibleId`, `ProducerId`, `ManagerId` e referências semelhantes são aceitas sem validação persistida uniforme de tenant/perfil/atribuição; combinar com RBAC por propriedade (SEC-01). |
| 9 | Cookies/sessões | Falha confirmada no modelo atual | Não há cookie de sessão/CSRF; Bearer explícito torna CSRF clássico não aplicável. O JWT e snapshot ficam em `localStorage` (`backendApi.ts:203-211`) e logout só apaga o cliente (`:239-243`), sem revogação no servidor (SEC-05). |
| 10 | Senhas | Parcial / risco potencial | Homologação usa `PasswordHasher` com dummy hash para usuário inexistente e validator exige parâmetros fortes; Production não tem IdP. Development contém senhas de exemplo em configuração rastreada e aceita autenticação apenas em Development. Não há recuperação, token de uso único ou MFA (SEC-03). |
| 11 | Rate limiting | Falha confirmada | Só login: 5/min por IP, memória local (`Program.cs:31-44`; `IdentityController.cs:20-22`). Não há limites por usuário/tenant, distribuídos, em upload, análise, cotações, mensagens ou listas (SEC-04). |
| 12 | Automação abusiva | Falha confirmada | Resposta de credencial inválida é uniforme, mas credential stuffing entre réplicas, criação de cotações/uso de terceiros, OCR/PDF e spam autenticado não têm quota/custo máximo. CAPTCHA não é necessário por padrão; controles graduais por risco são (SEC-04). |
| 13 | Consultas parametrizadas | Aprovado no escopo verificado | EF usa LINQ; SQL/Dapper revisado usa parâmetros. Catálogos usam allowlist imutável de recursos; não há `IgnoreQueryFilters` em produção. Não foi encontrado SQL derivado de tabela/ordenação livre do request. |
| 14 | Validação de entradas | Parcial | FluentValidation cobre os principais comandos e tamanhos; URL do backend e path HTTP do CRM são validados. Lacunas: texto financeiro sem limite, coleções sem paginação, IDs de responsável/produtor sem validação de pertencimento uniforme e opções de integrações sem validator HTTPS/host allowlist (SEC-04/SEC-08). |
| 15 | Vazamento de conteúdo/informações | Risco baixo/potencial | Erros 500 são genéricos e o CRM não renderiza corpo arbitrário; telemetria tem allowlist. O readiness anônimo expõe build/instância/migrations e parsing notes persistem `ex.Message` (`UploadProposalDocumentCommandHandler.cs:79-93`) (SEC-10). Textos/documentos PII ficam no banco sem retenção demonstrada (SEC-06). |
| 16 | Uploads | Falha confirmada | Há auth, limite 20 MB, extensão/MIME/signature e temp GUID; Tesseract usa `ArgumentList`, sem shell. Porém MIME/header são controláveis, nome original entra no path lógico, cópia integral em memória, PdfPig percorre todas as páginas no request e não há quota/sandbox/malware/CDR (SEC-04/SEC-07). |
| 17 | Respostas da API | Parcial | ViewModels/DTOs explícitos e erros sanitizados são positivos. QuoteRequests e fila WhatsApp retornam coleção completa; documento por ID inclui metadados sensíveis e parsing notes, sem filial; não há paginação/limite consistente (SEC-02/SEC-08). |
| 18 | Cabeçalhos de segurança | Aprovado no escopo de configuração | Nginx envia HSTS, CSP, `nosniff`, `DENY`, referrer e permissions policy (`nginx.conf:8-13`); API também envia headers. CSP não permite script inline; `style-src 'unsafe-inline'` é a exceção conhecida. Não houve validação contra headers efetivamente publicados. |
| 19 | HTTPS | Parcial / não verificável | CRM rejeita HTTP em build publicado (`environmentPolicy.ts:8-23`), API usa HSTS/redirect e forwarded headers. Origens CORS são explícitas, sem credentials. TLS/certificado no edge e conexão PostgreSQL não foram verificados; redes privadas inteiras são proxies confiáveis configuráveis e exigem alinhamento com o deploy. |
| 20 | Dependências | Parcial | `npm audit` e `dotnet list ... --vulnerable --include-transitive` retornaram zero advisories. npm usa lockfile; NuGet não. PdfPig é fork pré-release de 2023; .NET 8 encerra suporte em 10/11/2026; scans de imagem só puderam ser confirmados como configuração de CI (SEC-07). |

## 5. Achados por severidade

### SEC-01 — Permissões granulares não protegem as funções de negócio

- **Severidade:** Alta
- **Confiança:** Alta
- **Tipo:** Vulnerabilidade confirmada — Broken Function Level Authorization (OWASP API5:2023)
- **Projetos/evidência:**
  - `src/WAssis.Infra.CrossCutting.Identity/Extensions/ServiceCollectionExtensions.cs:83-90`
  - `src/WAssis.Services.Api/Modules/Financial/Controllers/CommissionReceiptsController.cs:12-69`
  - `src/WAssis.Services.Api/Modules/Policies/Controllers/PolicyDraftsController.cs:16-52`
  - `src/WAssis.Services.Api/Modules/Documents/Controllers/ProposalDocumentsController.cs:13-67`
  - `src/WAssis.Infra.Data/Modules/Administration/AdministrationRepository.cs:19-37`
  - `WassisCRM/nexus-crm/src/hooks/usePermission.ts:11-30`
- **Causa raiz:** `BrokerageStaff` exige somente usuário autenticado com `user_type=brokerage_staff`. A matriz `role_permissions` (`can_read/create/update/delete/export/manage`) só é consultada pelo filtro do controller de Administração. A própria UI declara que seu gating é simulação e, no modo backend, libera ações fixas por role.
- **Pré-condição/cenário:** obter qualquer JWT válido de vendedor/agente/staff; chamar diretamente `POST /api/financial/reconciliations/{id}/settle`, `POST /api/policies/drafts/{id}/issue`, upload/reprocessamento ou mutations equivalentes. Ocultar o botão não interfere na chamada.
- **Impacto comprovado:** o pipeline de autorização aceita o token antes de executar a operação. O impacto de negócio é alteração financeira, emissão/estado de apólices, documentos e dados comerciais fora da permissão configurada.
- **Reprodução mínima recomendada:** teste de integração com dois perfis no mesmo tenant: `can_read=true/can_update=false` e vendedor sem `can_manage`. As chamadas GET permitidas devem responder 200; cada mutation não concedida deve responder 403. O teste atual não existe e não foi executado com credenciais reais.
- **Correção:** criar autorização server-side por `(módulo, ação, escopo)` com deny-by-default; consultar permissões vigentes e vínculos de filial a cada operação sensível (cache curto e invalidável, se necessário). Aplicar policies/handlers por endpoint e testes de matriz. Não confiar em roles estáticas do JWT para autorização mutável de longa duração.
- **Regressão:** tabela de testes positivos/negativos por role, ação, tenant, filial, status do usuário e validade do vínculo; garantir que trocar método/ID não contorne o handler.

Referência: [OWASP API5:2023 Broken Function Level Authorization](https://api-security.owasp.org/editions/2023/en/0xa5-broken-function-level-authorization/).

### SEC-02 — Objetos sensíveis não possuem isolamento por filial

- **Severidade:** Alta
- **Confiança:** Alta sobre a ausência do controle; média-alta sobre impacto, pois a confidencialidade entre filiais deve ser confirmada com o negócio
- **Tipo:** Falha confirmada do mecanismo de filial / BOLA potencialmente explorável
- **Evidência:**
  - `src/WAssis.Infra.Data/Context/WAssisDbContext.cs:64-82` — filtro de filial apenas em `InsuredPerson`, `Opportunity` e `QuoteRequest`.
  - `src/WAssis.Infra.Data/Modules/Documents/Repositories/ImportedDocumentRepository.cs:15-18` — busca só por GUID; filtro implícito apenas de tenant.
  - `src/WAssis.Services.Api/Modules/Documents/Controllers/ProposalDocumentsController.cs:43-67` — ler, revisar e reprocessar por ID.
  - `src/WAssis.Infra.Data/Modules/Financial/Repositories/CommissionReceiptRepository.cs:20-23` — reconciliação por ID, tenant-wide.
  - `docs/adr-tenant-scope-and-rls.md:24` — o próprio ADR exige fechar documentos, financeiro, opções e auditoria antes de PRD.
- **Causa raiz:** alguns agregados não carregam `OfficeBranchId`; os que carregam nem sempre recebem policy de filial. O query filter de tenant impede outra organização, mas não outra filial da mesma corretora.
- **Pré-condição/cenário:** usuário limitado à filial A e com qualquer role `brokerage_staff`; conhecer/obter GUID de documento ou reconciliação criado pela filial B do mesmo tenant; fazer GET ou mutation correspondente.
- **Impacto:** leitura de metadados pessoais/financeiros, revisão/reprocessamento de documento e liquidação/alteração de registros de outra filial. GUID reduz descoberta casual, mas não é autorização.
- **Reprodução:** criar duas filiais no mesmo tenant no teste PostgreSQL, criar os objetos sob B e chamar como usuário restrito a A; esperar 404/403. Hoje o caminho de query aceita o registro pelo filtro de tenant.
- **Correção:** decidir e documentar quais módulos são `FILIAL` versus `CORRETORA`; adicionar ownership/filial às entidades e FKs, migrar dados, aplicar `USING`/`WITH CHECK` equivalente no repositório e exigir permissão de escopo. Para objetos tenant-wide, exigir permissão explícita de corretora, não o staff genérico.
- **Regressão:** matriz 2 tenants × 2 filiais × all-branches true/false, incluindo GET, UPDATE, transições, IDs forjados, includes/filhos, Dapper e worker.

### SEC-03 — Production não possui autenticação utilizável nem ciclo de sessão

- **Severidade:** Alta (bloqueador de disponibilidade/segurança de lançamento)
- **Confiança:** Alta
- **Tipo:** Falha confirmada de preparação
- **Evidência:**
  - `ServiceCollectionExtensions.cs:45-48` seleciona Development só em Development e Homologation em todo o restante.
  - `HomologationIdentityAuthenticationService.cs:25-33` retorna falha se o ambiente não for Staging.
  - `HomologationAuthOptions.cs:3-9` declara explicitamente “Never valid in Production”.
  - `IdentityController.cs:20-55` possui apenas login/me; criação retorna 501.
- **Causa raiz:** o autenticador real OIDC/BFF/Identity ainda é roadmap; Production cai no componente temporário que falha fechado.
- **Cenário/impacto:** em `ASPNETCORE_ENVIRONMENT=Production` ninguém obtém token; se alguém tentar habilitar o autenticador de homologação como atalho, o guard ainda bloqueia — comportamento seguro, mas serviço inutilizável. Não há refresh, revogação, logout servidor, recuperação ou MFA.
- **Correção:** integrar IdP real via Authorization Code + PKCE/BFF; mapear `sub` para usuário/tenant/filiais/perfis ativos no servidor; usar signing assimétrico/JWKS e rotação; definir access/refresh, revogação, logout global, MFA para perfis sensíveis, recuperação com token único/curto e auditoria. Remover o provider temporário de builds PRD.
- **Regressão:** testes Production para login real, usuário inativo, vínculo expirado, token revogado, rotação de chave, logout, recuperação replay/expirada e indisponibilidade do IdP; jamais aceitar configuração HML em PRD.

### SEC-04 — Operações caras não têm limites proporcionais nem isolamento

- **Severidade:** Alta
- **Confiança:** Alta
- **Tipo:** Vulnerabilidade confirmada de consumo irrestrito de recursos/abuso autenticado
- **Evidência:**
  - `Program.cs:31-44` define apenas rate limit de login, por IP e memória da réplica.
  - `ProposalDocumentsController.cs:18-38` aceita 20 MB, copia para `MemoryStream` e depois `ToArray` no request.
  - `PdfPigTextExtractor.cs:9-21` abre PDF e concatena texto de todas as páginas, sem page/text/time/memory cap e de forma síncrona.
  - `UploadProposalDocumentCommandHandler.cs:34-73` processa parser/OCR antes de responder.
  - `CommissionStatementParser.cs:10-22` divide e materializa todo o texto, sem limite de caracteres/linhas.
  - `InfraDataServiceCollectionExtensions.cs:155-171` limita/resiste a HTTP externo, mas não impõe quota de negócio por solicitante.
- **Pré-condição/cenário:** qualquer `brokerage_staff` (SEC-01) envia PDFs especialmente complexos ou concorrentes, textos grandes, ou cria cotações repetidas. Em múltiplas réplicas, o limiter de login é independente. Proxy/IP compartilhado também pode causar bloqueio coletivo.
- **Impacto:** pressão de memória/CPU, starvation do thread pool, indisponibilidade, crescimento de filas/banco e custo/abuso de integrações externas. Não foi executado DoS/fuzzing.
- **Correção:** mover parsing/OCR para worker isolado com fila durável; streaming para storage privado, checksum e estado assíncrono; limites de páginas, objetos, bytes descomprimidos, texto e tempo; CPU/memória/pids/rede do worker; rate limit distribuído por usuário+tenant+IP e quota por operação/custo; paginação; backpressure/circuit breaker; scan/sandbox/CDR conforme risco. Rejeitar antes de alocar/processar.
- **Regressão:** testes concorrentes com PDF multi-page/corrompido/fuzz corpus, cancelamento e timeout; payload/linhas máximas; 429 por identidade/tenant em múltiplas instâncias; orçamento de chamadas a seguradoras; métricas/alertas sem PII.

Referências: [OWASP API4:2023 Unrestricted Resource Consumption](https://api-security.owasp.org/editions/2023/en/0xa4-unrestricted-resource-consumption/) e [OWASP File Upload Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/File_Upload_Cheat_Sheet.html).

### SEC-05 — JWT persistente no localStorage e sem revogação/logout servidor

- **Severidade:** Média
- **Confiança:** Alta
- **Tipo:** Falha confirmada de gestão de sessão
- **Evidência:** `WassisCRM/nexus-crm/src/lib/backendApi.ts:198-211,239-248`; `AuthContext.tsx:248-254`; ausência de endpoints refresh/logout/revoke no `IdentityController`.
- **Cenário:** XSS futuro, dependência JS comprometida ou extensão/processo com acesso ao perfil lê `wassis.backend.accessToken`; logout apenas remove a cópia local, então um token já copiado segue válido até expirar.
- **Mitigadores atuais:** React escapa texto; não foram encontrados `dangerouslySetInnerHTML`, `eval` ou script inline; CSP restringe `script-src 'self'`; homologação usa token de 15 minutos.
- **Impacto:** sequestro de sessão com os privilégios do usuário e nenhuma contenção imediata no servidor.
- **Correção:** preferir BFF/cookie `HttpOnly; Secure; SameSite` com CSRF por token/origin; ou access token curto apenas em memória + refresh rotativo protegido, reuse detection e revogação. Registrar `jti`/família de sessão, invalidar em logout/alteração de vínculo e reduzir a janela conforme risco.
- **Regressão:** token não deve aparecer em Web Storage/bundle/log; logout/revogação deve tornar chamadas subsequentes 401; testes de rotação/replay/CSRF.

Referência: [OWASP HTML5 Security — Web Storage](https://cheatsheetseries.owasp.org/cheatsheets/HTML5_Security_Cheat_Sheet.html).

### SEC-06 — Proteção e ciclo de vida de PII não são exigidos nem demonstrados

- **Severidade:** Média
- **Confiança:** Alta sobre lacunas no repo; baixa sobre o estado da infraestrutura externa
- **Tipo:** Risco potencial / controle não verificável
- **Evidência:**
  - `DatabaseConnectionPolicy.cs:7-18` não exige TLS nem validação de certificado.
  - `docs/database-and-deploy.md:42` exemplifica `SSL Mode=Require;Trust Server Certificate=true`, o que cifra sem autenticar adequadamente o servidor.
  - `ImportedDocument` persiste texto extraído, nome de segurado, valores e notas; `InsuredPersonMappings.cs` expõe amplo conjunto de dados pessoais ao DTO autorizado.
  - A busca não encontrou política/código de retenção, eliminação/anonymização ou exercício de titular; documentos operacionais declaram backup/restore/PITR pendentes.
- **Cenário/impacto:** conexão DB pode ser configurada sem verificação forte; comprometimento de banco/backup expõe PII em claro; dados podem permanecer além da finalidade. Não há prova de que a produção esteja assim configurada.
- **Correção:** exigir `SslMode=VerifyFull` + CA confiável fora de dev; credenciais distintas e mínimo privilégio; storage/volumes/backups cifrados com KMS e rotação; inventário/classificação, minimização de texto extraído, retenção por finalidade, deleção/anonymização auditável, acesso a backup e restore/PITR ensaiado.
- **Regressão/evidência operacional:** teste de startup rejeitando DB sem TLS/cert verification; restore drill documentado; teste de purge por tenant/titular preservando auditoria legal mínima; revisão de grants/backup por evidência sem copiar segredos.

### SEC-07 — Dependência PDF não rastreável e runtime próximo do fim de suporte

- **Severidade:** Média
- **Confiança:** Alta para proveniência/ciclo de vida; média para explorabilidade do parser
- **Tipo:** Risco de cadeia de suprimentos e manutenção
- **Evidência:**
  - `src/WAssis.Infra.Data/WAssis.Infra.Data.csproj:24` usa `UglyToad.PdfPig 1.7.0-custom-5`.
  - NuGet identifica a versão como pré-release, atualizada em 05/06/2023 e publicada pelo owner `grinay`, sem proveniência upstream clara no `.nuspec` local.
  - O upstream recebeu em 28/03/2026 um relato de fuzzing com crashes/possível OOM em v0.1.14/master; aplicabilidade ao custom fork não foi demonstrada.
  - Todos os projetos têm `<TargetFramework>net8.0`; o suporte oficial termina em 10/11/2026.
  - Não existem `packages.lock.json` nem `global.json`; restore NuGet não é totalmente reprodutível. npm possui lockfile e usa `npm ci`.
- **Impacto:** parser de entrada hostil com correções/proveniência difíceis de auditar; deriva de transitivas/SDK; perda iminente de patches do runtime.
- **Correção:** substituir por release oficial mantida ou fork interno reproduzível com repositório/commit/SBOM e processo de patch; fuzz/sandbox do parser; habilitar lockfile NuGet/locked mode e pin do SDK; migrar e testar para .NET 10 LTS antes de 10/11/2026.
- **Regressão:** restore `--locked-mode`, SBOM com hashes/proveniência, corpus malicioso e scan SCA/imagem a cada build.

Fontes: [NuGet — PdfPig 1.7.0-custom-5](https://www.nuget.org/packages/UglyToad.PdfPig/1.7.0-custom-5), [issue upstream de fuzzing #1277](https://github.com/UglyToad/PdfPig/issues/1277), [política oficial de suporte .NET](https://dotnet.microsoft.com/en-us/platform/support/policy) e [NuGet lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies).

### SEC-08 — Coleções de API sem paginação/limite consistente

- **Severidade:** Média
- **Confiança:** Alta
- **Tipo:** Falha confirmada de limite; impacto depende do volume
- **Evidência:** `QuoteRequestRepository.cs:32-54` lista todas as cotações com options; `WhatsAppConversationRepository.cs:32-53` materializa toda a fila. Em contraste, Segurados e Oportunidades usam `Take(200/300)`.
- **Cenário/impacto:** tenant grande ou fila acumulada gera query, memória, serialização e payload crescentes; combinado com repetição sem rate limit causa degradação.
- **Correção:** paginação cursor-based com máximo server-side, campos/projeções mínimas, filtros indexados e total opcional; não aceitar page size ilimitado.
- **Regressão:** seed acima do limite, verificar tamanho máximo/ordem estável/continuação e plano de query; carga concorrente com orçamento de latência/memória.

### SEC-09 — Ausência de RLS deixa isolamento dependente de todo caminho da aplicação

- **Severidade:** Média
- **Confiança:** Alta
- **Tipo:** Risco potencial / defesa em profundidade ausente
- **Evidência:** `docs/adr-tenant-scope-and-rls.md:26-34` confirma que nenhuma policy RLS foi aplicada. Query filters e Dapper atualmente revisados estão corretos no escopo tenant, mas `SystemDataScope`, SQL futuro ou credencial com ownership podem ignorar a barreira de aplicação.
- **Impacto:** um único endpoint/raw SQL/regressão pode virar acesso cross-tenant; não foi encontrado bypass cross-tenant concreto nesta revisão.
- **Correção:** após reconciliar schemas/ownership, roles separadas (app sem owner/BYPASSRLS, migrator, manutenção), `SET LOCAL app.tenant_id`, policies `USING`/`WITH CHECK`, `FORCE RLS` quando aplicável e rotina global restrita do worker.
- **Regressão:** dois tenants/filiais, pool reutilizado, configuração ausente, rollback/cancelamento, EF+Dapper+worker, usuário DB sem ownership/BYPASSRLS. Manter os filtros de aplicação mesmo com RLS.

### SEC-10 — Metadados operacionais e mensagens do parser são expostos

- **Severidade:** Baixa
- **Confiança:** Alta
- **Tipo:** Vazamento de informação limitado
- **Evidência:** `Program.cs:129-158` publica anonimamente build SHA, nome da instância e nomes de migrations pendentes; `UploadProposalDocumentCommandHandler.cs:79-93` grava `ex.Message`, retornado em `ProposalDocumentsController.cs:70-94`.
- **Cenário/impacto:** reconhecimento de rollout/topologia/schema e divulgação de detalhes internos do parser a staff. Não foi observada stack trace em resposta.
- **Correção:** endpoint público minimalista (`status`), detalhes só em endpoint interno/autenticado; converter erros do parser em códigos seguros e guardar diagnóstico completo apenas em log restrito, com sanitização.
- **Regressão:** snapshots de 4xx/5xx/health não devem conter hostname, migration, path, SQL, token, PII ou mensagem arbitrária de biblioteca.

### SEC-11 — Compose local publica serviços com credenciais padrão

- **Severidade:** Baixa (Média se executado em host compartilhado/remoto)
- **Confiança:** Alta
- **Tipo:** Risco potencial de infraestrutura de desenvolvimento
- **Evidência:** `docker/docker-compose.yml:4-46` publica PostgreSQL `5432`, RabbitMQ `5672/15672` com credenciais padrão e Redis `6379` sem autenticação; imagens usam tags móveis.
- **Cenário/impacto:** executar esse Compose em máquina acessível pela rede expõe banco, broker/admin e Redis.
- **Correção:** bind em `127.0.0.1`, rede interna e sem host ports quando possível; secrets locais aleatórios; desabilitar management; autenticar Redis; digests/renovação; banner claro “local only”.
- **Regressão:** scan de portas no host e teste de conexão anônima/padrão devem falhar fora do perfil local.

### SEC-12 — Gate de qualidade do CRM não executa lint

- **Severidade:** Informativa
- **Confiança:** Alta
- **Tipo:** Preparação/CI
- **Evidência:** `.github/workflows/ci.yml:36-63` executa audit/test/build/image scan, mas não lint. Localmente `npm run lint` falhou com 27 erros; testes e build passaram. O build também avisou que `VITE_BUILD_SHA` não estava definido apenas na execução local desta auditoria.
- **Impacto:** regressões de regras React/typing entram mesmo com CI verde; não foi demonstrada vulnerabilidade direta.
- **Correção:** corrigir baseline e adicionar lint como gate; manter pin de actions, permissões mínimas, SBOM e Trivy já existentes.

## 6. Dependências e advisories

| Componente | Instalado/verificado | Advisory/CVE | Aplicabilidade e correção | Fonte |
|---|---|---|---|---|
| npm, grafo completo | 277 deps reportadas; lockfile + `npm ci` | Nenhum advisory em 16/09/2026 (`0` em todas as severidades) | Resultado pontual das fontes npm; manter audit/Trivy/renovação. Não prova ausência futura. | `npm audit --json` |
| NuGet, solução + transitivas | PackageReferences da solução | Nenhum advisory retornado em 16/09/2026 | `dotnet list WAssisInsurance.sln package --vulnerable --include-transitive`; adicionar lockfile e locked mode. | NuGet v3 oficial |
| UglyToad.PdfPig | `1.7.0-custom-5` pré-release | Sem CVE confirmado pela auditoria; issue upstream #1277 relata crashes/OOM em outra linha | Não rotular como CVE nem como explorabilidade confirmada. Trocar por origem mantida, rastreável; sandbox/fuzz. | [NuGet](https://www.nuget.org/packages/UglyToad.PdfPig/1.7.0-custom-5), [GitHub #1277](https://github.com/UglyToad/PdfPig/issues/1277) |
| .NET / ASP.NET Core | `net8.0`; imagens 8.0 por digest | Ciclo de suporte, não CVE | .NET 8 em Maintenance termina em **10/11/2026**; migrar para .NET 10 LTS e manter patch mensal. | [Microsoft .NET Support Policy](https://dotnet.microsoft.com/en-us/platform/support/policy) |
| Imagens de container | digests fixos nos Dockerfiles | Dockerfile do backend declara atualização de `libpcre2` para CVE-2026-86145/89161 | Build/scan local não executou por Docker indisponível. CI Trivy falha em HIGH/CRITICAL corrigíveis; confirmar execução verde e re-scan do digest publicado. | `Dockerfile:1,26-30`; workflows PR/CI |

Notas:

- Versão direta fixa não fecha transitivas NuGet; a ausência de `packages.lock.json`/SDK pin permite resolução diferente. A orientação oficial do NuGet recomenda lockfile/locked mode para restores reprodutíveis em CI.
- O CI gera SBOM/proveniência e fixa GitHub Actions por commit, controles positivos contra cadeia de suprimentos.
- `ignore-unfixed: true` no Trivy é uma decisão operacional: registrar exceções e compensações; não interpretar scan verde como “sem vulnerabilidades”.

## 7. Plano de correção priorizado

### Bloqueadores de lançamento

| Ordem | Ação | Esforço aproximado | Dependências / critério de saída |
|---|---|---:|---|
| 1 | Implementar autorização server-side por módulo/ação/escopo e negar por padrão (SEC-01) | 2–4 semanas | Matriz de permissões estável; testes positivos/negativos para todos os endpoints sensíveis. |
| 2 | Fechar modelo de ownership por filial e BOLA de documentos/financeiro/policies/WhatsApp/auditoria (SEC-02) | 2–4 semanas | Decisão de negócio FILIAL vs CORRETORA; migrations/backfill; matriz 2×2 em PostgreSQL real. Pode evoluir junto de RLS. |
| 3 | Integrar identidade Production e ciclo de sessão (SEC-03/05) | 3–6 semanas | IdP/tenant mapping, MFA, refresh/revogação, recuperação, secrets/JWKS, UX e runbook. |
| 4 | Isolar operações caras e aplicar quotas/limites distribuídos (SEC-04) | 2–4 semanas | Worker/storage privado, limites de parser/container, rate-limit store, métricas/custo de terceiros. |
| 5 | Executar gate completo em PostgreSQL/containers e evidenciar infra PRD | 1–2 semanas | Docker/CI disponível; TLS edge+DB, roles/grants, secrets, backup+restore/PITR, alertas e scan do digest. |

### Correções prioritárias

1. Exigir TLS com verificação de certificado no PostgreSQL, política de PII/retention e restore drill (SEC-06) — 1–3 semanas, parte dependente de infraestrutura/compliance.
2. Substituir/rastrear PdfPig, limitar/sandboxar parser, gerar lockfiles/pinar SDK (SEC-07) — 1–2 semanas.
3. Paginar QuoteRequests/WhatsApp e qualquer lista sem máximo (SEC-08) — 3–5 dias.
4. Migrar para .NET 10 LTS — 1–3 semanas. **Obrigatório antes de 10/11/2026** para permanecer suportado.
5. Sanitizar health/parsing notes e restringir detalhes operacionais (SEC-10) — 1–2 dias.

### Melhorias posteriores

- RLS como defesa em profundidade com roles DB separadas e testes de pool/worker (SEC-09) — 2–4 semanas após estabilizar schema/ownership.
- Endurecer Compose local e fixar digests (SEC-11) — 1 dia.
- Corrigir lint e adicioná-lo ao CI (SEC-12) — 1–3 dias.
- Secret scanning no pre-commit/CI e varredura inicial do histórico remoto — 1 dia, mais rotação se houver achado.
- Threat model/abuse cases por cotações, financeiro, emissão e WhatsApp; alertas de anomalia por tenant/usuário sem registrar PII.

## 8. Registro de verificações

### Comandos executados (sem valores secretos)

```text
git status --short
git log -G <padrões de segredo de alta confiança> --all
rg <fontes, rotas, policies, SQL raw, sinks, logs, uploads, URLs e padrões XSS>
dotnet list WAssisInsurance.sln package --vulnerable --include-transitive
dotnet list WAssisInsurance.sln package --outdated --include-transitive
npm audit --json
npm outdated
dotnet test WAssisInsurance.sln --configuration Release --no-restore
dotnet test WAssisInsurance.sln --configuration Release --no-restore --filter "FullyQualifiedName!~WAssis.Tests.Integration"
npm test
npm run build  (com URL HTTPS e AUTH_MODE/DATA_MODE=backend; nenhum segredo)
npm run lint
```

### Resultados

- WAssisBE: 96/96 testes sem Testcontainers aprovados. Suite completa: 96 aprovados e 3 falharam exclusivamente por `DockerUnavailableException` antes de criar PostgreSQL.
- WassisCRM: 48 arquivos/333 testes aprovados; build de produção aprovado; sem source maps gerados. Lint: 27 erros (typing em testes/query builder e regra React em `ConcludeCardModal`).
- SCA: zero advisories NuGet e npm nas fontes consultadas na data.
- Secret scan manual/histórico: nenhum segredo privilegiado confirmado; ferramentas dedicadas ausentes.
- Docker/image scan e integração PostgreSQL: não executados localmente por Docker indisponível. Workflows contêm Trivy e testes Testcontainers, mas o status de execuções remotas não foi consultado.

### Testes a adicionar antes de PRD

1. Autorização: role/action/scope, usuário inativo, vínculo expirado, propriedade e enumeração de IDs.
2. Isolamento: dois tenants × duas filiais em EF, Dapper, filhos, worker e pool reutilizado; depois repetir com RLS.
3. Sessão: refresh rotation/replay, revogação/logout, troca de permissão, MFA e recuperação.
4. Recursos: concorrência, quotas distribuídas, paginação, PDF fuzz/corrompido/muitas páginas, OCR timeout e budget de integrações.
5. Infra: TLS/cert verification, security headers/CORS efetivos, scan do digest, restore/PITR e failover.

## 9. Controles positivos observados

- Falha fechada quando identidade autenticada não contém tenant; tenant e filial entram no predicado de escrita por concurrency token.
- Dapper de Administração/Core usa SQL fixo/allowlist e parâmetros; nenhuma chamada `IgnoreQueryFilters` de produção foi encontrada.
- HML usa password hash e comparação dummy; JWT exige chave forte/base64 fora de Development e valida assinatura/lifetime.
- Erros da API e do CRM evitam stack/body arbitrário; OpenTelemetry remove SQL, URL, headers, identidades e payloads.
- Vite build publicado exige backend+HTTPS; Nginx/API têm CSP/headers; React não usa sinks HTML perigosos encontrados.
- Dockerfiles usam digests e usuário não-root; Actions são pinadas por commit; CI gera SBOM/proveniência e escaneia imagens.
- Outbox, advisory lock de migration, retries apenas em métodos HTTP seguros e timeouts/circuit breaker reduzem corridas e duplicidades em partes do fluxo.

Esses controles reduzem risco, mas não compensam os bloqueadores de identidade, autorização, filial e consumo de recursos descritos acima.
