# Auth0 / Production — configuração externa necessária

Esta entrega prepara o BFF, mas não provisiona nem comprova o tenant Auth0. Use uma
aplicação **Regular Web Application** confidencial. O client secret existe apenas no
secret store da API; nunca deve entrar no CRM, Git ou evidência de CI.

Configuração mínima a revisar no tenant:

- Allowed Callback URL: URL pública da API terminada em `/signin-auth0`;
- Allowed Logout URL: valor HTTPS de `Identity__Auth0__FrontendLogoutUrl`;
- origem web/CORS: somente as origens HTTPS reais do CRM;
- Authorization Code com PKCE e Universal Login; não habilitar password grant;
- MFA obrigatório para perfis sensíveis, recuperação forte, proteção contra ataques
  e política documentada de enrollment/reset;
- rotação do client secret e das chaves internas JWT com janela controlada.

Variáveis obrigatórias da API em Production:

```text
Identity__Auth0__Enabled=true
Identity__Auth0__RequiredInProduction=true
Identity__Auth0__Authority=https://<tenant-auth0>
Identity__Auth0__ClientId=<secret-store>
Identity__Auth0__ClientSecret=<secret-store>
Identity__Auth0__FrontendCallbackUrl=https://<crm>/
Identity__Auth0__FrontendLogoutUrl=https://<crm>/login
Identity__Jwt__SigningKey=<base64-aleatorio-32-bytes-ou-mais>
Identity__Jwt__RequireHttpsMetadata=true
```

O issuer deve corresponder exatamente ao tenant, normalizado sem `/` final. Cada
`issuer/sub` precisa ser reconciliado e inserido explicitamente em
`security.external_identities`; não existe backfill automático. Antes da liberação,
testar login, MFA, recuperação, logout local/remoto, replay, usuário/tenant/filial
inativos, vínculo expirado, indisponibilidade do IdP e rotação. Registrar apenas IDs
de evidência e resultados sanitizados, nunca tokens ou segredos.
