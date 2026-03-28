# Identity, Claims and Roles

## Objetivo

Preparar o ERP para operar em tres contextos sem misturar autorizacao e dados:

- plataforma W.Assis
- corretoras clientes do ERP
- corretora digital com usuarios finais

## Principio de separacao

- cada corretora opera em seu proprio tenant logico
- usuarios de uma corretora nao podem acessar dados de outra
- usuarios finais da corretora digital pertencem a um tenant proprio do canal digital
- claims carregam contexto de acesso
- roles definem permissoes dentro do contexto

## Claims padronizados

- `sub`
- `tenant_id`
- `brokerage_id`
- `seller_id`
- `user_type`
- `role`

## Tipos de usuario

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

## Politicas iniciais

- `authenticated_user`
- `platform_admin`
- `brokerage_staff`
- `brokerage_admin`
- `digital_customer`

## Estado atual da implementacao

- autenticacao JWT base configurada
- claims e roles padronizados em infraestrutura
- `ICurrentUserContext` disponivel para aplicacao e API
- endpoint `GET /api/identity/me` disponivel para inspecionar o contexto autenticado
- `TenantId` propagado para agregados principais do ERP
- `DbContext` com filtro global por tenant quando ha contexto autenticado
- repositórios e dashboard operacional já obedecem o escopo do tenant
- migration `20260328203038_AddTenantIsolation` criada para persistir o isolamento no banco

## Como o isolamento funciona hoje

- requests autenticados com `tenant_id` enxergam apenas os dados do proprio tenant
- requests autenticados sem `tenant_id` nao recebem acesso implicito a dados de outros tenants
- workers e processos sistemicos sem tenant autenticado continuam podendo processar filas multi-tenant
- comandos de escrita passam a persistir `TenantId` nos agregados centrais

## Proximas etapas obrigatorias

1. proteger endpoints internos com politicas por perfil e contexto
2. evoluir a resolucao de conexao para suportar `shared` e `dedicated database`
3. separar claramente rotas e experiencias de corretora e canal digital
4. validar jornadas reais com o frontend consumindo claims e papeis corretos
