# Identity, Claims and Roles

## Objetivo

Preparar o ERP para operar em três contextos sem misturar autorização e dados:

- plataforma W.Assis
- corretoras clientes do ERP
- corretora digital com usuários finais

## Princípio de separação

- cada corretora deve operar em seu próprio tenant
- usuários de uma corretora não podem acessar dados de outra
- usuários finais da corretora digital pertencem a um tenant próprio do canal digital
- claims carregam contexto de acesso
- roles definem permissões dentro do contexto

## Claims padronizados

- `sub`
- `tenant_id`
- `brokerage_id`
- `seller_id`
- `user_type`
- `role`

## Tipos de usuário

- `platform`
- `brokerage_staff`
- `customer`

## Roles iniciais

- `platform_admin`
- `brokerage_owner`
- `brokerage_admin`
- `brokerage_manager`
- `brokerage_seller`
- `brokerage_agent`
- `digital_customer`
- `end_customer`

## Políticas iniciais

- `authenticated_user`
- `platform_admin`
- `brokerage_staff`
- `brokerage_admin`
- `digital_customer`

## Estado atual da implementação

- autenticação JWT base configurada
- claims e roles padronizados em infraestrutura
- `ICurrentUserContext` disponível para aplicação e API
- endpoint `GET /api/identity/me` disponível para inspecionar o contexto autenticado

## Próxima etapa obrigatória

1. propagar `TenantId` para os agregados centrais do ERP
2. aplicar filtros e repositórios por tenant
3. proteger endpoints internos com políticas por perfil
4. separar claramente rotas e experiências de corretora e canal digital
