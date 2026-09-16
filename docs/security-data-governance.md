# Segurança de dados e evidências de infraestrutura

Este documento separa controles preparados no código de controles que só podem ser comprovados no ambiente de hospedagem. Ele não define prazo legal de retenção e não autoriza expurgo em produção.

## Controles preparados

- Conexões PostgreSQL fora de Development exigem TLS com verificação completa do certificado (`SslMode=VerifyFull`).
- O script `ops/postgres/least-privilege-roles.sql` prepara roles distintas para aplicação, worker e migration, todas sem superuser e sem `BYPASSRLS`.
- A migration de remediação adiciona ownership de filial sem valor default ou backfill automático; registros legados ambíguos permanecem inacessíveis.
- Logs de auditoria não armazenam prévia de mensagem do WhatsApp; notas livres são sanitizadas e truncadas.
- Respostas públicas de health e erros de parser não expõem mensagens internas.

## Reconciliação e retenção

Antes de qualquer backfill, o responsável pelos dados deve produzir uma relação assinada entre cada registro legado ambíguo e sua filial correta. A execução deve ser uma migration revisada, transacional e auditável. Registros sem decisão explícita continuam com filial nula e, portanto, negados pelos filtros e pelo RLS.

Antes de habilitar exclusão ou anonimização automatizada, Jurídico/DPO deve aprovar, por categoria de dado: fundamento, prazo, evento inicial, exceções de preservação e método de anonimização. Até essa decisão, nenhum job destrutivo está habilitado.

## Evidências externas obrigatórias antes de Production

- configuração de criptografia em repouso e identificação da chave/KMS, sem exportar material secreto;
- resultado dos grants da role efetiva da API e do worker, demonstrando ausência de owner/superuser/BYPASSRLS;
- política e execução de backup, restore testado e objetivo de PITR;
- rotação de credenciais e chaves, com responsáveis e evidência da última execução;
- configuração do IdP, MFA dos perfis sensíveis e recuperação de conta;
- relatório de execução dos testes PostgreSQL/RLS usando as roles efetivas.

Esses itens são pendências de verificação de infraestrutura, não controles considerados comprovados por este repositório.
