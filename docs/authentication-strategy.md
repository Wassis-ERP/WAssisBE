# Authentication Strategy

## Decisao atual

O `WAssisBE` e o emissor do token usado para consumir endpoints protegidos da API. Os frontends podem continuar usando Supabase para a sessao da interface enquanto a migracao acontece, mas chamadas para a API .NET devem usar um JWT emitido pelo backend no header:

```http
Authorization: Bearer <accessToken>
```

## Fluxo local

1. O frontend chama `POST /api/identity/login`.
2. A API valida o usuario configurado em `Identity:DevelopmentAuth`.
3. A API retorna `accessToken`, expiracao e claims de contexto.
4. O frontend guarda esse token em uma sessao local do cliente.
5. Chamadas protegidas usam o token contra `GET /api/identity/me` e demais endpoints.

O login local fica habilitado apenas em `Development`, salvo configuracao explicita em `Identity:DevelopmentAuth:AllowOutsideDevelopment`.

## Fluxo de producao desejado

O backend deve validar o contexto de negocio antes de emitir ou aceitar acesso aos modulos internos. A decisao recomendada para a proxima etapa e implementar uma troca controlada de identidade:

1. O usuario autentica no provedor escolhido para a UI.
2. O frontend envia a identidade autenticada para um endpoint de troca do backend.
3. O backend resolve tenant, corretora, vendedor, tipo de usuario e roles.
4. O backend emite um JWT proprio com as claims padronizadas.

Isso mantem Supabase desacoplado da autorizacao do ERP e deixa o `WAssisBE` como fonte de verdade para regras multi-tenant.

## Claims obrigatorias

- `sub`
- `tenant_id`
- `brokerage_id`
- `seller_id`
- `user_type`
- `role`

## Contrato para os frontends

Os frontends devem centralizar a sessao da API em um helper proprio:

- `loginToBackend(username, password)`
- `getBackendAccessToken()`
- `getBackendCurrentUser()`
- `clearBackendSession()`

Esse helper nao substitui imediatamente o Supabase Auth; ele apenas padroniza as chamadas para a API .NET durante a migracao.

## Proximos passos

1. Criar endpoint de troca de identidade para substituir o login local em ambientes reais.
2. Mapear roles Supabase/CRM para roles do backend.
3. Persistir vinculo entre usuario externo, tenant, corretora e vendedor.
4. Proteger endpoints internos com politicas especificas.
5. Adicionar testes de integracao para `POST /api/identity/login` e `GET /api/identity/me`.

## Remocao do Supabase nos frontends

Os frontends nao devem mais importar SDK, URL ou chave do Supabase. Durante a migracao, chamadas antigas de tabela/RPC/storage sao roteadas para endpoints de gateway do `WAssisBE`:

- `POST /api/data-gateway/query`
- `POST /api/data-gateway/rpc/{name}`
- `POST /api/storage/{bucket}/{action}`
- `POST /api/functions/{name}`
- `GET /api/migration/frontend-contracts`

Esses endpoints existem para tornar a dependencia do backend explicita e retornar `501 Not Implemented` ate que cada fluxo ganhe um contrato dedicado. Eles nao devem virar um clone permanente da API do Supabase.

Toda resposta pendente deve retornar `ProblemDetails` com `code`, `contractKey`, `ownerIssue`, `nextStep` e, quando existir, `replacementEndpoint`. Isso evita pontas soltas no frontend: cada chamada legada tem dono, issue e proximo passo rastreavel.
