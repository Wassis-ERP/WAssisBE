# Gestão administrativa SaaS — evidência e limites de 15/09/2026

Esta fase continua da branch `codex/admin-management-foundation`. O monólito modular e o CQRS lógico permanecem. O contrato v3.1 define tenant como grupo e filial como corretora; `profiles` identifica o usuário do negócio, `profile_filiais` define perfil por corretora e vigência, e `role_permissions` define ações e escopo. `nivel_acesso` não é autorização.

## Indicadores de `GET /api/administration/statistics`

Todas as contagens são filtradas por `tenant_id`; não são telemetria técnica. `activeBranches` conta `erp.filiais` ativas. `activeUsers` conta `erp.profiles` ativas, com `status=ATIVO` e sem convite pendente. `inactiveUsers` conta `status=INATIVO`. `pendingInvitations` conta cadastros com `convite_status=PENDENTE` que não foram inativados; **não significa e-mail enviado** enquanto Auth0 não estiver integrado. `insuredPeople` conta linhas do módulo conectado em `public.segurados`. `openOpportunities`, `wonOpportunities` e `lostOpportunities` contam `public.oportunidades` por status `pending`, `won` e `lost`, respectivamente. A contagem usa os índices existentes de tenant e tenant+status. A divisão entre `public` persistido e `erp` v3.1 permanece dívida de migração.

## Matriz de status

| Gate | Estado em 15/09 | Evidência ou impedimento |
|---|---|---|
| Contrato de perfil, conflitos PostgreSQL seguros e cadastro pendente | Implementado em código | `AdministrationRepository`, `AdministrationController`, `ApiExceptionHandler`; cadastro não concede acesso e não afirma envio. |
| Autorização administrativa persistida | Parcial | A API preserva o gate de identidade `BrokerageAdmin` e exige também `configuracoes`/`GRUPO` e vínculo vigente de usuário ativo para leitura ou gestão. Ainda falta resolver ações independentes e restringir dados de cada corretora conforme o conjunto de vínculos; não declarar isolamento de corretora completo. |
| Testes negativos PostgreSQL/HTTP | Escrito, execução pendente | `PostgresAdministrationTests` semeia dois tenants, testa conflitos, datas, auto-inativação, último Master concorrente, permissão persistida, métricas e auditoria. Requer Docker/PostgreSQL local funcional. |
| Auth0 Organizations, convite, MFA, BFF e revogação | Pendente | Não existe tenant Auth0 HML. Não há credencial, convite enviado, sessão revogada no IdP ou MFA real. |
| RLS aditiva/equivalência | Pendente | Exige testes reais de isolamento em PostgreSQL antes de aplicar policies. Guardas atuais devem permanecer até equivalência demonstrada. |
| Gate singleton no deploy, secrets, OTel, outbox restante, PRD, PDF e HA | Pendente | Dependem de implementação e/ou infraestrutura própria. Nenhum build ou teste local prova backup/PITR, restore, rotação, alertas ou failover. |

## Reproduzir a validação

Na raiz `C:\dev\WAssisBE`, com .NET SDK, Docker Engine e imagem `postgres:16-alpine` disponíveis:

```powershell
dotnet build src/WAssis.Services.Api/WAssis.Services.Api.csproj -c Release
dotnet test tests/WAssis.Tests/WAssis.Tests.csproj -c Release --filter 'Category!=Postgres'
dotnet test tests/WAssis.Tests/WAssis.Tests.csproj -c Release --filter 'Category=Postgres'
```

O teste de integração inicia um PostgreSQL descartável via Testcontainers, executa o migration gate singleton e usa HTTP real pelo `WebApplicationFactory`. Nesta sessão, `dotnet` não foi encontrado no PATH nem nos locais usuais da máquina, e o Docker Engine não pôde ser conectado; o teste novo e o build após a última edição não têm resultado executado. Antes de promover HML/PRD, executar também o ensaio de migrations idempotentes contra o legado documentado e registrar saída, SHA e plano de rollback. A escolha de Auth0 foi confirmada, mas o usuário informou que ainda não existe tenant HML; esse gate depende de provisionamento e acesso administrativo limitado sem envio de segredo no chat.
