# Checkpoint e walkthrough de fechamento — 15/10/2026

## Conclusão

O núcleo comercial Auto/manual está integrado e tem evidência de persistência real
até o comparativo. O ERP completo ainda não está fechado: propostas/apólices,
financeiro, atendimento e várias funções transversais continuam sem a jornada
canônica conectada. A prioridade recomendada é completar uma operação manual de
ponta a ponta e homologá-la, aproveitando as telas e regras existentes.

Este documento registra o estado do código, dos PRs e das evidências consultadas.
Não atesta a versão hoje instalada em HML/Production, que não foi acessada nesta
rodada. Nenhum merge, deploy ou migration compartilhada foi executado.

## Bases conferidas e atualização local

| Repositório | Branch de produto atualizada localmente | Código analisado | main remota |
|---|---|---|---|
| WAssisBE | `codex/calculation-screen-support` | `5691a2a` | `851f1d2` |
| WassisCRM | `codex/calculation-screen-integration` | `2e01c14` | `c1da8cd` |

Fetch dos dois remotos e pull fast-forward das branches de produto concluídos.
As árvores estavam limpas e sincronizadas antes deste checkpoint documental.
`main` não recebeu as entregas posteriores ao merge BE #33 / CRM #52.

- BE: cinco commits de produto à frente de main; cadeia aberta em rascunho
  [#34](https://github.com/Wassis-ERP/WAssisBE/pull/34) →
  [#35](https://github.com/Wassis-ERP/WAssisBE/pull/35) →
  [#36](https://github.com/Wassis-ERP/WAssisBE/pull/36) →
  [#37](https://github.com/Wassis-ERP/WAssisBE/pull/37).
- CRM: sete commits de produto à frente de main; cadeia aberta em rascunho
  [#53](https://github.com/Wassis-ERP/WassisCRM/pull/53) →
  [#54](https://github.com/Wassis-ERP/WassisCRM/pull/54) →
  [#55](https://github.com/Wassis-ERP/WassisCRM/pull/55) →
  [#56](https://github.com/Wassis-ERP/WassisCRM/pull/56) →
  [#57](https://github.com/Wassis-ERP/WassisCRM/pull/57).

Há nove PRs incrementais em rascunho. As contagens acima identificam a base
funcional antes dos commits exclusivamente documentais deste checkpoint.

## Evidência disponível

| Evidência | Resultado e alcance |
|---|---|
| [CI BE 38625978541](https://github.com/Wassis-ERP/WAssisBE/actions/runs/38625978541) | `5691a2a`: 169 testes, zero ignorados, PostgreSQL/Testcontainers, build/publish, scanner de segredos e scans API/worker aprovados. |
| [CI CRM 38626879591](https://github.com/Wassis-ERP/WassisCRM/actions/runs/38626879591) | `2e01c14`: lint global, testes, audit, build, smoke Auth0 compilado e build/scan da imagem aprovados. |
| Micro-plano CRM de integração de 14/10 | 395 testes locais e jornada BE+CRM 2/2 com PostgreSQL descartável/TLS/role comum, recargas, falha após commit, idempotência, concorrência, sexta seleção e revisão 409; capturas 1440×900/100% registradas. |
| Revisão deste checkpoint | Conferidos Git/PRs/SHAs/CIs, contratos e limites nos adapters, guardas de rotas, mapas de persistência e testes da jornada. Nenhum código funcional alterado. |

A jornada 2/2 e a inspeção visual são evidências registradas pela implementação
anterior, não uma nova execução nesta análise. O CI do CRM roda o smoke de
bootstrap; isso não equivale a executar a jornada completa com BE nem a login
real no Auth0. Gates de código aprovados são reaproveitados neste checkpoint
documental, conforme a política do repositório.

## Situação por capacidade

| Capacidade | Estado verificado | O que falta para fechamento |
|---|---|---|
| Sessão, escopo e segurança | BFF/Auth0 preparado, permissões persistidas, tenant/filial/PROPRIO e testes negativos; imagens finais BE com runtime 10.0.13. | Evidência de IdP real, MFA/recuperação/revogação/rotação e operação publicada. |
| Segurados e oportunidades | Persistência canônica `erp.*`, listas/detalhes e jornada com reload. BE #37 corrige perda da etapa ao editar prêmio/data. | Homologação publicada; funções transversais ainda precisam de avaliação própria. |
| Cálculo Auto, tentativa e cotação manual | Editor, versões, preparar/retomar/cancelar/reexecutar e resultado persistido; zero/null/false preservados. | Outros ramos e motores automáticos são recortes próprios. |
| Comparativo | Até cinco resultados, nomes históricos, revisão explícita e recuperação de resposta perdida. | Exportação/publicação PDF e formalização da escolha como proposta. Seleção atual não aprova a cotação. |
| Administração e configurações | APIs e hooks de organização, filiais, usuários/perfis/permissões existem; lookups comerciais de leitura funcionam. | `/configuracoes` é bloqueada pelo shell conectado. Escritas de catálogos/funis ainda usam o adapter demonstrativo. Cadastro de usuário não comprova convite/credencial no IdP. |
| Propostas, apólices e itens | Frontend e regras demonstrativas existem; BE possui `policies.policy_drafts` a partir de documentos. | Jornada canônica `erp.apolices` + `erp.propostas` + itens/CTI, leitura/edição/confirmar/reload e vínculo explícito à cotação. Draft técnico existente não entrega esse agregado. |
| Financeiro da corretora | Front tem regras demonstrativas; BE possui recibos/conciliações em `financial.*`. | Conectar o contrato vigente: parcelas do cliente, agenda de comissões, conciliação, baixas/estornos e repasses/recibos. Billing SaaS é outra capacidade. |
| Sinistros, pós-venda, atividades/anexos/EAV, notificações e dashboard | Há telas/modelos e capacidades parciais. Rotas principais de atendimento/dashboard/notificações permanecem bloqueadas no modo conectado; diversos consumidores transversais usam memória. | APIs/comandos por módulo, autorização, persistência e jornada de cada recorte. Schema/tela existente não comprova módulo operacional. |
| Documentos/OCR e transmissão | BE tem pipeline de documentos/worker e provedores legados; execução canônica nova aceita somente MANUAL. | Integração ao contrato atual, storage/limites e evidência operacional; bridge por provedor, credenciais, orçamento, idempotência/reconciliação externos. |

Fontes de código: CRM `src/App.tsx:185` e `:220` (allowlist),
`src/main.tsx` (provider demonstrativo ausente no modo conectado),
`src/lib/backendCalculationApi.ts` (somente Auto), `useLookupsAdmin.ts` e
`usePipelinesAdmin.ts` (escritas demonstrativas), `commercial/quoteProposalOrigin.ts`
e `financeiro/*Domain.ts` (memória). No BE: `CalculationValidators.cs`,
`ExecutionValidators.cs`, `PolicyDraftMap.cs`, `CommissionReceiptMap.cs` e
controllers dedicados. Os paths CRM são relativos a `nexus-crm`.

O gateway genérico `/api/data-gateway/query` e RPCs retornam 501 por decisão de
migração. Acelerá-los como acesso arbitrário a tabelas não fecha os módulos;
o caminho previsto é aproveitar regras do front em comandos/querys dedicados.

## Ordem recomendada para fechar com menos retrabalho

### 0 — Consolidar o pacote comercial já validado

Revisar os pequenos diffs da cadeia e preparar promoção das pontas completas BE
e CRM para main. A base final deve conter tanto o suporte BE #37 quanto o CRM
#57 e o patch de runtime de #36. Os PRs empilhados não são independentes.

Planejar a integração como duas versões coerentes, usando os PRs incrementais
para revisão e preservando branches/histórico. Validar CI da integração resultante.
O workflow CRM em main pode chamar redeploy via webhook; o BE publica imagem e
exige o gate de migration/schema/readiness do manager. Preparar BE/schema antes
do CRM e conferir os SHAs efetivamente publicados.

Aceite: branches de release inequívocas, checks aprovados, HML com as versões
esperadas e jornada comercial repetida no ambiente publicado.

### 1 — Habilitar a configuração mínima de uma corretora

Liberar apenas as áreas administrativas com API correspondente e implementar os
comandos faltantes para os catálogos/funis necessários: ramo Auto, seguradora,
coberturas, origem e etapas. Reaproveitar organização/filiais/perfis já existentes.
Demais guias continuam com indicação honesta de disponibilidade.

Aceite: uma corretora de teste consegue preparar seus cadastros e operar sem
edição manual do banco ou dependência das fixtures de desenvolvimento; usuário
sem permissão é recusado pelo backend. Login e vínculo real no IdP têm evidência.

### 2 — Fechar venda manual: cotação → proposta → apólice

Primeiro recorte: formalização explícita de um resultado manual Auto, com
segurado, oportunidade, cotação e parcelamento consistentes; sem alterar o
snapshot cotado. Depois, cadastro/revisão do documento e contrato/item/CTI,
estados e datas/números oficiais conforme DBML v3.1. Permitir registro manual dos
dados oficiais recebidos, sem alegar emissão em seguradora por uma ação interna.

Reusar telas e validações existentes; persistência/autorização/concorrência ficam
no BE. Cada recorte precisa passar por gravação → reload → revisão/confirmação.
Exportação do comparativo pode ser uma PR pequena independente neste bloco.

Aceite: a escolha gera documento/contrato apenas por comando explícito;
repetição não duplica, vínculos entre grupos/filiais são recusados e os dados
permanecem após nova sessão.

### 3 — Fechar o financeiro básico da operação

Persistir primeiro agendas de parcelas e comissões a partir do documento
confirmado. Comissão da corretora e repasse são fatos distintos. Continuar por
conciliação, baixa manual/parcial e estorno auditável; depois repasse e recibo
persistente. Conciliar não confirma recebimento. Preservar os bloqueios de
estorno quando houver repasse pago e as regras de recibo do contrato vigente.

Aceite: um caso simples percorre documento → agenda → conciliação → recebimento
→ repasse/recibo e pode ser reaberto/reconsultado; operações repetidas e negativas
não duplicam valores nem apagam histórico.

### 4 — Completar os módulos do ERP

Conectar atividades/anexos/EAV prioritários dos detalhes, cobrança operacional,
sinistros, pós-venda/renovação, notificações e indicadores com números reais.
Cada módulo entra pela sua jornada principal e testes de escopo. Financeiro e
contratos consolidados alimentam as leituras, evitando duplicar o modelo.

### 5 — Automatizar o que depender de terceiros

Integrar seguradoras individualmente, começando por um provedor com contrato e
sandbox confirmados. Tratar pedido/aceite/resultado, custo, timeout, recuperação
e duplicidade de chamadas. Completar OCR/importação com revisão humana e storage
durável. A operação manual recomendada nos blocos anteriores permite avançar
enquanto esses dados externos são obtidos.

Os blocos 0–2 formam um primeiro lote comercial; o bloco 3 acrescenta controle
financeiro. O ERP completo inclui o bloco 4 e os critérios operacionais abaixo.
Essa é uma proposta de fatiamento, não uma redução do escopo contratado nem uma
estimativa de prazo. Cada bloco ainda será dividido em PRs pequenas.

## Walkthrough de aceite

Usar duas corretoras/grupos de teste, um operador comum e um administrador.
Exercitar o modo backend e conferir recargas e uma segunda sessão.

| Passo | Ação e resultado esperado | Situação |
|---|---|---|
| 1 | Login real; selecionar corretora; vínculo vencido/inativo recusado; logout/MFA/recuperação testados. | Backend/smoke preparados; IdP publicado a comprovar. |
| 2 | Preparar catálogos/funil/permissões pela aplicação e criar um segurado. | Cadastro de segurado validado; configuração autossuficiente pendente. |
| 3 | Criar oportunidade, editar prêmio/data mantendo etapa, recarregar. | Jornada/integração registrada; correção BE #37. |
| 4 | Criar pedido Auto com zero/null/false; recarregar e duplicar em nova versão. | Cobertura registrada na jornada; conferir duplicação na homologação final. |
| 5 | Preparar tentativa MANUAL; cancelar/reexecutar/retomar com comissão zero; registrar resultado e recarregar. | Jornada registrada. |
| 6 | Recuperar resposta perdida após commit e clique duplicado; conservar chave/payload e um único resultado. | Jornada registrada em ambiente descartável. |
| 7 | Comparar versões; selecionar cinco; sexto recusado; remover/reabrir; conflito entre duas abas conserva rascunho e exige decisão. | Jornada registrada. |
| 8 | Formalizar proposta, revisar e registrar apólice/item sem alterar cotação; recarregar. | Próximo bloco funcional. |
| 9 | Conferir agendas, conciliar, baixar/estornar e emitir recibo de repasse. | Bloco financeiro pendente. |
| 10 | Tentar leitura/escrita/exportação de outro grupo/filial e ação não permitida; conferir auditoria. | Há testes negativos BE; repetir nas novas jornadas. |
| 11 | Repetir o caso no HML publicado, conferir SHA/readiness e continuidade após reinício dos serviços. | Evidência operacional a registrar. |

Reprodução local já disponível, a partir do WAssisBE, usando exclusivamente banco
descartável e substituindo os marcadores pelos diretórios locais:

```powershell
./scripts/test-security-postgres.ps1 -PostgresBinDirectory '<bin-postgres>'
./scripts/test-security-postgres.ps1 -PostgresBinDirectory '<bin-postgres>' -ConnectedOnly -ConnectedCrmDirectory '<WassisCRM/nexus-crm>'
```

O segundo comando executa a jornada do navegador, não repete a categoria
PostgreSQL. Consultar também o CRM `docs/e2e-local.md`.

## Critérios operacionais para liberação

Além das jornadas do recorte que será liberado: identidade real e ciclo de
usuários; secrets/rotação; grants de aplicação sem ownership/BYPASSRLS;
release/migration/schema/readiness comprovados; backup e restore/PITR ensaiados;
logs/alertas sanitizados; storage/retenção compatíveis com anexos habilitados.
Worker e integrações externas só entram com seus limites e recuperação validados.

Fontes: `docs/auth0-production-configuration.md`,
`docs/release-migrations-and-observability.md`,
`docs/distributed-processing-next-gate.md` e `SECURITY_FOLLOWUP.md`.
Documentos de setembro são históricos: pendências já resolvidas de raízes CRM,
BFF ou ligação do cálculo não devem ser reabertas por um checkbox antigo.
Neste checkpoint, não foram atribuídos percentuais de conclusão ou prazos sem
evidência. O critério de fechamento é a jornada persistida e homologada por módulo.
