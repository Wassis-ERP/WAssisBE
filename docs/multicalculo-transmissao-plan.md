# Integridade da transmissão Auto — 09/10/2026

Base: WAssisBE a93e855 e WassisCRM 64d6d22. Contrato canônico v3.1 preservado.

Primeiro recorte: corrigir a transmissão legada sem fingir que a API `quotes/requests` implementa todo o domínio canônico. O snapshot `AutoSubmission` é um envelope persistido de envio, com colunas e coleção relacionais, sem JSON de negócio. É criado uma vez com a solicitação e percorre API → comando → armazenamento → worker → adaptador. Pedidos antigos sem snapshot não serão enviados ao Aggilizador com dados inventados.

Aceite: condutor distinto, vigência explícita, bônus zero e limites por versão chegam intactos; ausência de de-para gera restrição local; HTTP aceito não vira cotação concluída. Validar unidade, cadeia de mapeamento e roundtrip PostgreSQL/RLS em banco descartável. Não aplicar migration em ambiente do usuário.

Continuidade: implementar serviços canônicos ERP (pedido/execuções/resultados), ligação do frontend, renovação própria derivada de apólice e apresentações persistidas. Antes disso, consolidar a raiz de oportunidade: a API conectada atual persiste `public.oportunidades`, enquanto as FKs canônicas apontam para `erp.oportunidades`; não criar dual-write nem copiar vínculos silenciosamente para contornar essa fronteira. Essa etapa não depende da engenharia reversa SegFy; integração específica aguarda contrato e acesso autorizados. Não criar endpoint ou código de fio SegFy por inferência.

## Checkpoint

Implementados envelope de transmissão, persistência relacional/migration, validações, mapeamento explícito do Aggilizador, capacidade declarada por adaptador, identidade separada do motor e estado AwaitingResults. Ainda não implementar compartilhamento ou alegar integração de ponta a ponta. Dicionários de de-para vazios impedem envio até configuração homologada. Renovação própria bloqueada até validar a apólice no serviço canônico. Questionário legado ainda usa defaults e precisa de etapa própria.

ImageSharp foi removido após alertas de segurança no restore e exigência de licença na versão corrigida; conversão de pixels para PPM substitui seu único uso no OCR. Sem mudança de política de auditoria.

Restore locked com SDK 10.0.401, compilação Release sem avisos, auditoria NuGet e alinhamento do modelo EF verificados novamente em 09/10/2026. A suíte local sem PostgreSQL passou com 143/143 testes, inclusive as correções de DateOnly, coleção EF e conversão PPM. O run 37962842731 aprovou os 148/148 testes, incluindo as cinco integrações PostgreSQL/Testcontainers; a repetição local não iniciou porque o Docker Desktop não ficou disponível. OCR real permanece pendente porque não há binário Tesseract instalado. Nenhum banco de usuário foi migrado.

O CI do checkpoint falhou apenas no scan da imagem API: o digest anterior do runtime ASP.NET 10.0.12 continha `CVE-2026-84782` corrigível em `libssl3t64` e `openssl`. API e worker foram atualizados para o digest oficial corrente do mesmo runtime; Trivy 0.75.0 confirmou zero HIGH/CRITICAL no novo artefato remoto. A etapa canônica (`calculos`, execuções, resultados e apresentações) continua sendo o próximo recorte funcional e não foi disfarçada como concluída por esta correção de pipeline.

## Continuação verificada — idempotência e jornada conectada

Base conferida com fetch: BE `3752478` e CRM `0295172`, ambos na branch `codex/security-remediation-followup`. Os respectivos CIs 37965321761 e 37965793466 estavam integralmente aprovados.

Confirmado no código: o handler retornava a solicitação de `CorrelationId` existente antes de comparar seu conteúdo. Agora o tenant/chave é serializado por advisory lock transacional PostgreSQL e todos os campos persistidos de entrada são comparados, inclusive snapshot Auto, vigência, condutor e coberturas. Mesmo conteúdo retorna o ID existente; conteúdo diferente retorna **409** seguro, sem dados do pedido. A ordem de coberturas não define outra versão. O índice tenant/chave existente permanece; uma chave oculta em outra filial gera conflito, sem leitura através do RLS. Solicitação e outbox continuam atômicas, sem nova migration.

Teste PostgreSQL com role comum verificou oito retries concorrentes, duas versões concorrentes, tenant diferente, filial diferente, cancelamento durante a espera pelo lock e ausência de enqueue parcial. A jornada HTTP verificou 201/mesmo ID/409 e preservação do pedido original. O lock é liberado pelo commit/rollback; não existe mutex somente local à API. Referência oficial: [PostgreSQL 16 — advisory locks](https://www.postgresql.org/docs/16/explicit-locking.html#ADVISORY-LOCKS).

O ensaio de navegador revelou e corrigiu outros bloqueadores: registro do processamento de documentos movido exclusivamente para o worker; bootstrap do CRM agora passa `VITE_AUTH_PROVIDER` ao validador; seletor conectado consome `GET /api/identity/me/branches`, limitado aos vínculos vigentes da própria sessão, em vez do diretório administrativo GRUPO. `ICurrentUserContext` continua reduzido à filial selecionada. O contexto de tenant confirmado por `/me` é mantido em memória para os adapters após reload/Auth0, sem recriar token ou expiração e sem autorizar acesso no cliente; logout/401 o limpam.

Validação: build Release sem avisos, restore locked sem cache HTTP, NuGet/npm audit sem advisories, modelo EF sem divergência, 152 testes sem PostgreSQL e seis integrações PostgreSQL 16.15/TLS `VerifyFull` aprovados. CRM: 337 testes, lint, build e smoke do bundle Production/Auth0 aprovados. Playwright conectado aprovado com API real, banco exclusivo e role não-owner/NOBYPASSRLS: GET/POST/PUT de Segurados/Oportunidades, edição após reload, ganho, lista/Kanban e logout/nova sessão. Destinos fora das duas origens locais são bloqueados pelo teste. O smoke Production usa somente um 401 sintético de identidade; não comprova Auth0 real.

Fronteira ainda pendente: **as duas** APIs conectadas persistem `public.segurados` e `public.oportunidades`, enquanto as FKs de cálculos apontam para `erp.segurados` e `erp.oportunidades`. Consolidar as raízes conjuntamente com reconciliação explícita de IDs, campos e registros antigos antes de ligar cálculo/execuções/cotações/apresentações. Não houve dual-write, backfill, mudança do DBML v3.1 ou habilitação de `useCalculos` conectado. `src/lib/supabase.ts` no CRM é um adapter local em memória, sem SDK/servidor Supabase; recusa uso conectado/Production. Demo permanece separado.

PostgreSQL Windows comprova banco real e TLS, mas não a fixture Docker. Docker Desktop local continuou indisponível; scans das imagens finais serão repetidos no CI do checkpoint. Auth0 provisionado/MFA/recuperação, OCR real/corpus hostil, limites rígidos do runtime e orçamento por seguradora permanecem pendentes. Nenhuma migration foi aplicada em dados reais e nenhum deploy foi realizado.
