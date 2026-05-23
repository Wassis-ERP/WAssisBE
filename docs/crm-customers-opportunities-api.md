# CRM - Segurados e Oportunidades

Contrato inicial para migrar as telas do CRM para o `WAssisBE`.

## Decisoes

- Banco oficial: PostgreSQL, controlado pelo backend.
- Tabelas iniciais: `public.segurados` e `public.oportunidades`.
- `oportunidades` e `segurados` possuem `tenant_id` e `filial_id`.
- `tenant_id` representa a corretora; `filial_id` representa a filial/unidade da corretora.
- Usuarios comuns enxergam apenas as filiais nas claims `branch_id`/`branch_ids`; usuarios com `all_branches=true` enxergam todas as filiais do tenant.
- `oportunidades.segurado_id` referencia `segurados.id`.
- Campos flexiveis por ramo ficam em `oportunidades.metadata` como `jsonb`.
- O FE deve consumir endpoints dedicados, nao acessar banco direto.

## Segurados

### `GET /api/segurados`

Query params opcionais:

- `search`: busca por nome, CPF/CNPJ ou e-mail.
- `status`: filtra por status, por exemplo `Ativo`, `Inativo` ou `Prospecto`.

### `GET /api/segurados/{id}`

Retorna um segurado por `id`.

### `POST /api/segurados`

Payload minimo:

```json
{
  "officeBranchId": "filial-matriz",
  "name": "Maria Silva",
  "personType": "PF",
  "status": "Ativo",
  "documentNumber": "12345678900",
  "email": "maria@example.com",
  "phoneNumber": "11999999999",
  "lgpdAuthorized": true
}
```

Campos opcionais alinhados ao CRM legado:

- `birthDateUtc`
- `officeBranchId`: se omitido, o backend usa a filial principal do usuario autenticado.
- `tradeName`
- `gender`
- `maritalStatus`
- `companySize`
- `cnae`
- `website`
- `postalCode`
- `street`
- `number`
- `complement`
- `neighborhood`
- `city`
- `state`
- `notes`
- `producerId`
- `managerId`
- `chatwootId`

### `PUT /api/segurados/{id}`

Atualiza o cadastro inteiro do segurado usando o mesmo shape do `POST`.

## Oportunidades

### `GET /api/oportunidades`

Query params opcionais:

- `pipelineId`
- `stageId`
- `status`, por exemplo `pending`, `won` ou `lost`.

### `GET /api/oportunidades/{id}`

Retorna uma oportunidade por `id`.

### `POST /api/oportunidades`

Payload minimo:

```json
{
  "officeBranchId": "filial-matriz",
  "name": "Seguro Auto - Maria Silva",
  "insuredPersonId": "00000000-0000-0000-0000-000000000000",
  "pipelineId": "comercial",
  "stageId": "novo",
  "status": "pending",
  "businessType": "novo",
  "metadata": {
    "placa": "ABC1D23",
    "modelo": "Honda Fit",
    "bonus_atual": 3
  }
}
```

Campos core opcionais:

- `responsibleId`: se omitido, usa o usuario autenticado.
- `officeBranchId`: filial da corretora. Se omitido, usa a filial principal do usuario autenticado.
- `insuranceLineId`: equivalente ao `ramo_id` legado.
- `insurerId`: equivalente ao `seguradora_id` legado.
- `originId`: equivalente ao `origem_id` legado.
- `contactType`
- `netPremium`
- `commissionPercentage`
- `agencyPercentage`
- `validityStartUtc`
- `validityEndUtc`
- `nextFollowUpUtc`
- `referrer`
- `notes`

### `PUT /api/oportunidades/{id}`

Atualiza a oportunidade inteira. Inclui tambem:

- `lossReasonId`
- `productionAmount`
- `concludedAtUtc`

### `PATCH /api/oportunidades/{id}/stage`

Payload:

```json
{
  "stageId": "em-negociacao"
}
```

Uso previsto: drag-and-drop do Kanban.

## Proxima etapa

Depois que o FE apontar para estes endpoints, o proximo bloco natural e separar a parte financeira da proposta/venda concluida em tabela propria, em vez de ampliar `oportunidades`.
