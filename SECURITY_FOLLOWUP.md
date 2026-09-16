# Checkpoint de segurança — 16/09/2026

Base: WAssisBE `7bea743`, WassisCRM `be9627e`.
Branch de continuação nos dois repositórios: `codex/security-remediation-followup`.
Primeiro checkpoint: backend `42a86ca`, frontend `d98b84c`.
Este documento complementa `SECURITY_REMEDIATION.md`; não certifica Production.

## Correções implementadas

- SEC-12: Gitleaks CLI oficial 8.30.1, checksum fixo Windows/Linux, histórico
  completo e saída redigida. Resolve a dependência de licença da action anterior.
- SEC-07/12: NuGet Audit bloqueia restore/build por vulnerabilidades diretas ou
  transitivas (low a critical) e falhas da fonte de auditoria. Configuração também
  copiada antes do restore Docker. Relatório sozinho não era um gate de falha.
- SEC-05: sessão e permissões usam conexões curtas independentes do DbContext da
  requisição, evitando capturar contexto anônimo durante OnTokenValidated.
  CRM descarta o CSRF anônimo depois do login e busca token da identidade atual.
- SEC-01/02/05: vínculos de filial são reconstruídos do banco durante validação da
  sessão, sem depender de claims antigos. Novo vínculo permite seleção sem novo
  login; remoção perde acesso imediatamente. Sessão sem vínculo válido é recusada.
  Login com profile/vínculo inválido retorna 401 genérico, sem emitir cookie novo.
- SEC-01/02: autorização e dados usam a mesma filial. Permissões sem filtro de
  branch retornam todos os vínculos vigentes para o seletor de corretoras do CRM.
- SEC-01: PROPRIO restringe Segurados por produtor/gerente ligado ao profile e
  Oportunidades por responsável, em listagem, ID e escrita. Outros módulos negam
  PROPRIO até implementarem ownership. Escopos desconhecidos não concedem acesso.
- SEC-01: campos de ownership também são tokens de concorrência EF. Reatribuição
  entre leitura e escrita impede UPDATE/DELETE antigo. Conflito retorna 409 genérico.
  Migration `20260916203520_ProtectConcurrentRecordOwnership` atualiza apenas o
  snapshot; Up/Down não alteram esquema físico ou dados.
- SEC-04: orçamentos independentes por usuário, tenant e IP, compartilhados entre
  réplicas. Limite do atributo por usuário, 10x tenant e 20x IP; anônimo por IP.
  Consumo persiste mesmo quando a transação de negócio sofre rollback.
- Administração: três operações usam transação existente com savepoint ou criam
  transação própria. Erro ou retorno antecipado desfaz a operação sem encerrar a
  transação externa. Middleware RLS também desfaz respostas HTTP >=400.
- Administração: datas PostgreSQL/Npgsql são lidas como DateOnly; parâmetros
  Dapper enviados como DbType.Date. Datas preenchidas e nulas foram verificadas.

## Evidências locais

- .NET SDK 10.0.401 instalado via WinGet/Microsoft; ferramenta EF 10.0.12 restaurada.
- Restore locked aprovado com `--no-http-cache`; não houve downgrade para contornar cache.
- 117 testes sem PostgreSQL aprovados; quatro testes de integração PostgreSQL
  aprovados, cobrindo múltiplas jornadas/assertivas cada. Execução final conjunta:
  121/121 aprovados. Build Release da solução: zero avisos e zero erros.
- PostgreSQL 16.15 oficial em cluster temporário, loopback, senha aleatória e
  TLS VerifyFull com certificado de teste. Nenhum banco da aplicação foi utilizado.
- Fixtures HTTP usam role LOGIN sem SUPERUSER, ownership ou BYPASSRLS. As permissões
  amplas concedidas nas tabelas da fixture servem para testar RLS e NÃO constituem
  uma proposta de grants mínimos para Production.
- Jornadas HTTP: tenant/filial, PROPRIO, permissão alterada sem novo login, vínculos
  concedidos/removidos durante sessão, cookie HttpOnly/Secure, CSRF anônimo recusado,
  logout e replay do cookie revogado, CRUD, proteção do último Master e auditoria.
- Concorrência: ownership alterado após leitura reproduziu escrita indevida antes
  da correção; agora gera DbUpdateConcurrencyException. Testado com oportunidades
  e vínculo de produtor de segurados.
- Savepoints: recusa de remover último Master preserva vínculo; criação bem-sucedida
  de perfil respeita rollback externo. Datas HTTP, criação de perfil e concorrência
  de inativação de dois Masters cobertas.
- Outbox/inbox: atomicidade, leases/claims concorrentes e recuperação; upgrade desde
  `20260912120359_AlignFrontendV31ConnectedContracts`, antes da migration de outbox.
- RLS: consultas e escritas SQL diretas com role comum, tenant/filial divergentes,
  contexto ausente e conexão reutilizada; jornadas HTTP executadas sob RLS.
- NuGet: solução sem avisos de vulnerabilidade; fixture externa com Newtonsoft.Json
  12.0.3 falhou com NU1903 como esperado. Não foi adicionado pacote vulnerável ao produto.
- CRM: 334 testes, lint e build passaram no primeiro checkpoint. Avisos conhecidos
  de fontes Mulish/Rubik permanecem. Nenhuma alteração de tela/contrato database.ts.

## Execução reproduzível

Com Docker disponível, os quatro testes usam Testcontainers PostgreSQL 16-bookworm,
certificado efêmero e VerifyFull. Alternativa Windows validada localmente:

```powershell
./scripts/test-security-postgres.ps1 -PostgresBinDirectory '<binários PostgreSQL>\bin'
```

O script cria cluster descartável, executa `Category=Postgres`, para o servidor e
remove senha/chave privada. Dados/logs temporários ficam para diagnóstico. Aceita
`-Filter` para um teste específico. Nunca fornecer banco compartilhado/de produção.

## Gates GitHub

- Frontend `d98b84c`: [CI 35145654570](https://github.com/Wassis-ERP/WassisCRM/actions/runs/35145654570)
  aprovado, incluindo testes/build/auditoria e scan de imagem.
- Backend `42a86ca`: [CI 35145647720](https://github.com/Wassis-ERP/WAssisBE/actions/runs/35145647720)
  passou Gitleaks/restore/build e falhou nos testes existentes. Esta continuação
  corrige as fixtures TLS, datas e transações que impediam a suíte local completa.
- A versão Docker da nova fixture e os gates do próximo commit precisam de execução
  remota após push. Não confundir validação local PostgreSQL com validação Docker.

## Pendências reais

1. Validar gates GitHub do novo commit e resolver falhas sem desativar controles.
2. Ampliar matriz em todos os módulos/filhos/Dapper e worker. Não considerar SEC-01,
   SEC-02 ou SEC-09 encerrados pelos testes de Segurados/Oportunidades.
3. Auth0/Production: autenticação, MFA, recuperação, rotação, idle timeout e jornada
   de reload no navegador. Provedor de HML continua proibido em Production.
4. PDF/OCR: streaming/storage privado/fila durável e processo encerrável, limites de
   CPU/memória/objetos/descompressão. Quotas atuais são de frequência, não de custo
   por seguradora. Parser PDF ainda síncrono e em memória: SEC-04 não está encerrado.
5. RLS quotes/worker, grants operacionais mínimos, backfill revisado, retenção,
   backup/restore/PITR e evidências externas. Sem migrations em dados reais aqui.
6. Revisar GRUPO em endpoints tenant-wide e consolidação somente de corretoras
   autorizadas quando exigido pelo contrato. Completar ciclo administrativo.
7. Smoke no navegador com frontend/backend reais; os testes HTTP não são browser E2E.

Nenhum deploy ou alteração de dados reais foi executado nesta continuação.
