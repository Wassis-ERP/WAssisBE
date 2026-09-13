# Identidade temporária de homologação e rotação

## Decisão

O login local de Development não é promovido a autenticação de produção. A implementação `HomologationIdentityAuthenticationService` atende exclusivamente `ASPNETCORE_ENVIRONMENT=Staging`, com opt-in `Identity__HomologationAuth__Enabled=true`. O valor padrão é false. Não aceitar aliases como HML/PRD: ambiente desconhecido permanece fechado. Em Production, habilitar o mecanismo faz a validação de startup falhar; desabilitado, login retorna `identity.login.disabled`. Não há fallback para mock ou credenciais de Development.

A infraestrutura continua usando `IIdentityAuthenticationService`. Staging valida opções no startup e novamente antes do login: duração de 1 a 15 minutos, usuários únicos, UUIDs de usuário/grupo/corretora/filial, filial ativa dentro de `BranchIds` e roles explícitas de corretora. `platform_admin` e usuários do canal digital não são provisionáveis por este mecanismo temporário.

Senha somente como `PasswordHash` no formato ASP.NET Identity V3 PBKDF2 SHA512, mínimo 100.000 iterações. Gerar com `PasswordHasher<HomologationAuthUserOptions>` em ferramenta administrativa confiável; nunca inserir senha em linha de comando, arquivo versionado ou transcript. O formato é validado antes da verificação; erros não incluem valores de configuração. Token com validade absoluta de até 15 minutos, sem refresh, exige novo login após expirar. Rate limit de login existente permanece; antes de escalar API, impor limite global no proxy/WAF.

Configuração não secreta: `Identity__HomologationAuth__TokenExpirationMinutes=15`. Usuários em `Identity__HomologationAuth__Users__<índice>__{Username,PasswordHash,UserId,TenantId,BrokerageId,BranchId,BranchIds__<índice>,HasAllBranchesAccess,SellerId,UserType,Roles__<índice>}`. `UserType=brokerage_staff`; não usar UUIDs inventados: provisionamento deve conferir vínculos reais no banco. A validação estrutural não substitui essa conferência, revogação de usuários nem RBAC persistido.

Fora de Development, `Identity__Jwt__SigningKey` deve ser segredo aleatório em base64 de pelo menos 32 bytes, sem placeholders ou conteúdo repetitivo. A assinatura mantém o contrato existente: **bytes UTF-8 do texto base64**, sem decodificar na emissão/validação JWT. Toda réplica e todo consumidor precisam usar o mesmo valor; HML e PRD usam valores distintos. `RequireHttpsMetadata=true`, issuer e audience obrigatórios.

## Inventário de rotação — valores nunca documentados

Tratar credenciais HML e chave JWT previamente expostas como comprometidas. Inventariar também seus consumidores antes de revogar:

| Dado | Nome Docker secret proposto (versionado) | Consumidor |
|---|---|---|
| Connection string | `wassis_hml_db_connection_v2` | API, worker, migrator |
| JWT signing key | `wassis_hml_jwt_signing_key_v2` | emissor e validadores |
| Hashes/identidades HML | `wassis_hml_identity_users_v2` | API HML |
| PostgreSQL administrativo | `wassis_hml_postgres_admin_v2` | operação restrita |
| Credencial GHCR read-only | `wassis_ghcr_pull_v2` | Portainer registry |
| Webhook Portainer API/CRM | secrets GitHub `PORTAINER_API_HML_WEBHOOK_URL` / `PORTAINER_CRM_HML_WEBHOOK_URL` | Actions |
| Seguradoras, storage, OCR, cache/fila | `wassis_hml_<servico>_<finalidade>_v2` | somente serviço habilitado |

Produção usa prefixo `wassis_prd_`, nunca compartilha valores. Apenas nomes e referências entram no Git. Docker secrets são imutáveis; nova versão permite troca e posterior remoção da anterior.

## Procedimento manual restante

1. Com autorização operacional, mapear referências no Portainer/Actions sem copiar os valores para relatórios. Confirmar backup/restore do banco e imagem anterior.
2. Gerar secrets novos em cofre confiável. Alterar senha/credencial no provedor correspondente e criar nova versão do Docker secret. Hash de senha também é segredo.
3. Montar secrets nos serviços corretos. API e worker agora leem os targets fixos `/run/secrets/wassis_db_connection`, `/run/secrets/wassis_jwt_signing_key` e `/run/secrets/wassis_identity_users`. O último é um array JSON de opções de identidade, com **hashes**, nunca senhas. Não misturar esse arquivo com usuários configurados em environment/appsettings: o startup rejeita a mistura para impedir usuários residuais após rotação. Limite de 64 KiB por secret. Nomes versionados do Docker são mapeados nesses targets estáveis. Conexão fora de Development exige senha com ao menos 20 caracteres não repetitivos, host/banco/usuário e desativa detalhes de erro/parâmetros.
4. Provisionar usuários HML e conferir UUIDs e vínculos reais. Desabilitar `DevelopmentAuth` nas configurações do serviço por clareza, embora o código já o bloqueie fora de Development.
5. Executar migration singleton quando necessária; validar schema; atualizar API e worker coordenadamente. Nunca contornar a validação de chave para manter segredo comprometido em uso.
6. Conferir SHA, readiness, login e `/api/identity/me`; conferir expiração e recusa de tenant/filial indevidos. JWT antigo deve ser rejeitado após rotação; usuários precisarão entrar novamente.
7. Revogar credenciais anteriores nos provedores, remover referências e versões antigas após janela controlada. Rollback de aplicação não deve restaurar segredo comprometido.

Nenhuma rotação ou mudança na infraestrutura publicada foi realizada nesta entrega. A nova validação pode impedir o startup até a chave antiga ser rotacionada; preparar os secrets antes de publicar a imagem.

## Arquitetura-alvo

OIDC Authorization Code + PKCE com provedor real, MFA e políticas de sessão; resolver vínculos de `sub` externo com profiles/grupos/corretoras/filiais no servidor. Claims jamais derivadas de campos enviados pelo navegador. JWT curto com chave assimétrica/JWKS, rotação e revogação/refresh controlados. Preferir BFF/cookie HttpOnly quando origem, CSRF, CORS e operação estiverem definidos. Remover `HomologationAuth` após homologar identidade real; não habilitá-lo em PRD como atalho.

Referência de implementação: [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-8.0).
