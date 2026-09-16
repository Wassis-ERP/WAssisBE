# Checkpoint de segurança — 16/09/2026

Base: WAssisBE `7bea743`, WassisCRM `be9627e`.
Continuação: branch `codex/security-remediation-followup` nos dois repositórios.
Este documento complementa o histórico `SECURITY_REMEDIATION.md`; não certifica Production.

## Alterações

- SEC-12: action Gitleaks substituída pela CLI oficial 8.30.1 com checksum fixo
  por plataforma. Mantidos histórico completo, redação e falha em achados.
- SEC-05: consultas de sessão usam conexão curta de `NpgsqlDataSource`, evitando
  construir o DbContext durante autenticação, antes de `HttpContext.User` existir.
  O CRM invalida CSRF após login para obter um token ligado à nova identidade.
- SEC-01/02: autorização e contexto de dados devem selecionar a mesma filial.
  Consulta de permissões sem branch retorna os vínculos vigentes do usuário,
  permitindo ao CRM montar a lista de corretoras sem restringi-la ao JWT inicial.
- SEC-01: `PROPRIO` agora produz restrição explícita em Segurados (produtor/gerente
  vinculados ao profile) e Oportunidades (responsável). Filtros cobrem listagem/ID;
  escritas verificam propriedade atual e original. Nos demais endpoints, PROPRIO
  é negado até existir suporte de ownership; não equivale a acesso CORRETORA.
  Escopos desconhecidos não concedem autorização. Isso não encerra toda a matriz.
- SEC-04: contadores separados por usuário, tenant e IP, compartilhados entre
  réplicas. Por operação/janela: limite do atributo por usuário, 10 vezes para
  tenant e 20 vezes para IP; login anônimo mantém limite por IP. Consumo usa
  transação independente, persistindo mesmo se a operação de negócio falhar.
  SQL retorna aliases explícitos para Dapper e satura contadores para evitar overflow.

## Validação

- SDK .NET 10.0.401 instalado por WinGet, origem Microsoft.
- Restore locked: aprovado com `--no-http-cache` (cache local anterior estava desatualizado).
- Base antes das alterações: build sem avisos/erros e 109 testes sem PostgreSQL.
- Código alterado: 117 testes sem PostgreSQL aprovados; build sem avisos/erros.
- CRM: 334 testes, lint e build aprovados; permanecem avisos de fontes Mulish/Rubik.
- Teste `SecurityServicesPostgresTests` cobre SQL real de permissões, sessão/revogação,
  quotas concorrentes entre duas instâncias, IP rotativo, orçamento por tenant e rollback.
- `scripts/test-security-postgres.ps1` cria um cluster Windows descartável em loopback
  usando binários PostgreSQL fornecidos explicitamente, roda esse teste e para o servidor.
  Não usa banco/credenciais da aplicação. Resultado registrado no fechamento.
- Gitleaks CLI: histórico backend (54 commits) e frontend (73 commits), zero achados.
- O teste real detectou incompatibilidade Dapper entre `DateTime` retornado por
  Npgsql e `DateTimeOffset` no construtor do contador; corrigida para UTC `DateTime`.
- Após a correção, `SecurityServicesPostgresTests` passou em PostgreSQL 16.15 real:
  migrations, permissões, revogação, concorrência/quotas e rollback. Cluster temporário
  encerrado. Não foi executada a suíte completa Testcontainers nem jornada no navegador.
- Checkpoint frontend publicado: `d98b84c`, branch `codex/security-remediation-followup`.

## Pendências para Renato

1. Executar os gates GitHub na branch e a suíte completa em Docker. Os workflows da
   base falharam no Gitleaks antes dos testes; publicação de imagem verde não substitui gates.
2. Completar matriz real de tenant/filial/PROPRIO, RLS com role não-owner e worker.
   O teste de serviços não substitui as jornadas HTTP/PostgreSQL existentes.
3. Investigar transações aninhadas: `RlsTransactionMiddleware` abre uma transação por
   request e três métodos de `AdministrationRepository` abrem outra na mesma conexão.
   Reutilizar a transação com comportamento correto em sucesso, erro e rollback.
4. Testes HTTP existentes usam Staging com PostgreSQL de container sem TLS. Ajustar
   a infraestrutura de teste para VerifyFull sem enfraquecer validação de Production/Staging.
5. Concluir Auth0/Production, MFA, recuperação, rotação e ciclo completo de sessão.
   Revisar também idle timeout/reload e claims de filial após mudança de vínculos.
6. Completar PDF por streaming/storage privado/fila e processo encerrável com limites.
   Quotas atuais são controles de frequência; não implementam ainda orçamento por custo de seguradora.
7. Completar RLS de quotes/worker, grants operacionais, backfill revisado, retenção,
   backup/restore/PITR e evidências externas. Não executar migrations em dados reais.
8. Revisar escopo GRUPO nos endpoints tenant-wide: consolidar somente filiais permitidas
   quando o contrato exigir; não assumir que um vínculo autoriza todos os registros do tenant.

Nenhum deploy ou alteração de dados reais foi executado nesta continuação.
