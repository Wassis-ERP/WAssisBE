# Comparativo de prontidão SaaS — 14/09/2026

## Escopo verificado

- WAssisBE `main` em `816dd2a` após os merges dos PRs de prontidão e correção do secret de HML.
- WassisCRM `main` em `3df5949` após o merge do PR de prontidão SaaS.
- Comparação realizada contra o prompt de Principal Engineer que cobria autenticação, fail-closed, secrets, isolamento, migrations, testes reais, observabilidade, processamento durável, segurança do CRM, bundle, supply chain, containers, HA e limites do CQRS.

## Resultado

| Objetivo | Situação | Evidência/limite atual |
|---|---|---|
| Autenticação segura em HML | Parcial avançado | HML possui autenticação opt-in, hash forte, JWT curto e validação de startup. Ainda não há IdP real, convite, MFA, revogação de sessão nem BFF. |
| CRM fail-closed | Alcançado | Builds implantáveis exigem auth e dados pelo backend; o modo memória não pode simular gravação em HML/PRD. |
| Jornadas reais no CRM | Parcial | Segurados e Oportunidades estão integrados. Os demais módulos continuam bloqueados ou demonstrativos. |
| Secrets | Parcial | Leitura por Docker secrets e validação existem. Rotação efetiva no Portainer e segregação operacional ainda dependem da implantação. |
| Isolamento tenant/corretora | Parcial avançado | Escopo explícito, filtros, guardas e testes existem. RLS PostgreSQL e autorização RBAC persistente ainda não estão aplicadas ponta a ponta. |
| Migration gate singleton | Parcial avançado | Lock, job e script de release existem. A conexão segura e obrigatória do gate no fluxo GitHub/Portainer precisa ser confirmada na infraestrutura. |
| Testes reais | Alcançado para o recorte | Há testes com PostgreSQL/HTTP no BE e Playwright para Segurados/Oportunidades no CRM. Falta ampliar para os módulos restantes e administração. |
| OpenTelemetry | Parcial | Instrumentação existe; collector, dashboards, alertas e SLOs operacionais não estão fechados. |
| Outbox/inbox e concorrência | Parcial | A fila durável com lease/fencing cobre Cotações. Outros workers e integrações ainda não usam o padrão. |
| Segurança do CRM | Parcial avançado | HSTS, fontes locais, URLs seguras, erros sanitizados, timeout/cancelamento e ausência de retry em escrita. JWT em `localStorage` permanece como dívida registrada. |
| Bundle e carregamento | Alcançado | Rotas foram divididas e o entry bundle permanece em aproximadamente 275 kB minificado / 84 kB gzip. |
| Supply chain/container | Alcançado no código | Actions e imagens pinadas, SBOM/provenance/scan e execução non-root foram adicionados. O rollout precisa preservar as configurações de porta e segurança. |
| HA/load balance real | Não alcançado | Swarm com um nó e PostgreSQL único não oferece HA. Traefik distribui apenas quando houver múltiplas réplicas/nós seguros. |
| Banco dedicado PRD, backup/PITR | Não alcançado | A separação HML/PRD, restauração ensaiada e PITR continuam operacionais/infraestrutura. |
| PDF durável/OCR seguro | Parcial inicial | Há parser e extração, mas faltam storage durável, streaming, antivírus, limites, fila OCR, revisão humana e lifecycle. |
| CQRS | Alcançado no nível recomendado | O monólito modular usa comandos/queries e transações. Event sourcing, microserviços e read replica permanecem corretamente adiados. |

Conclusão: a fundação de código prevista no prompt foi substancialmente entregue, mas a prontidão operacional e o fechamento do produto ainda são parciais. O maior risco remanescente não é CQRS; é identidade/RBAC real, RLS, infraestrutura PRD/HA e a persistência dos módulos ainda demonstrativos.

## Nova fase: gestão administrativa

A branch `codex/admin-management-foundation` cria a fundação persistente desta fase:

- dados do grupo/empresa e indicadores;
- cadastro persistente de usuários em estado de convite;
- ativação/inativação administrativa com proteção contra auto-inativação e último Master;
- vínculo de usuário a N corretoras, com perfil por corretora, vigência e corretora principal;
- autoria persistente de perfis e permissões por módulo/ação/escopo;
- CRUD persistente de corretoras;
- isolamento por `tenant_id` em todas as consultas e mutações;
- auditoria das mutações administrativas.

Limite intencional: cadastrar o perfil não cria credencial no provedor de identidade. Entrega de e-mail, aceite do convite, MFA, revogação imediata e login dependem da adoção do IdP. Até isso existir, não se deve anunciar o convite como enviado.

## Ordem recomendada para terminar

1. Integrar um IdP OIDC e fluxo BFF/cookie, incluindo convite, aceite, MFA e revogação.
2. Resolver `role_permissions` no backend por usuário+corretora e aplicar autorização em cada comando/query.
3. Ativar RLS após os testes de equivalência e de isolamento negativo.
4. Adicionar E2E administrativo com PostgreSQL descartável e Playwright.
5. Ligar migration gate, OTel collector/alertas e rotação de secrets no deploy real.
6. Completar os demais módulos conectados antes de declarar PRD pronta.
7. Só então evoluir HA: banco gerenciado/dedicado, backup/PITR testado e Swarm com múltiplos nós.
