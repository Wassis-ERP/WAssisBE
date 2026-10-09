# Prompt de continuidade — multicálculo W.Assis (09/10/2026)

Renato, continue o trabalho nos repositórios Wassis-ERP/WAssisBE e Wassis-ERP/WassisCRM, branch `codex/security-remediation-followup` em ambos. Faça fetch e fast-forward depois de conferir alterações locais. A base anterior deste checkpoint era backend `a93e855` e frontend `64d6d22`. Não confunda com main, que segue nos commits anteriores. Leia AGENTS.md e os planos antes de alterar cada repositório.

## Objetivo

Conectar o multicálculo existente respeitando o DBML/instruções v3.1: oportunidade → versões de cálculo → tentativas por seguradora → resultado único por execução → apresentação. A investigação SegFy é fonte de evidência de fluxo; ainda não temos payloads suficientes nem confirmação de contrato de integração. Não implementar chamadas SegFy por inferência.

## O que este checkpoint implementa

- Extensão aditiva `autoSubmission` em POST/GET `/api/quotes/requests`, transportada até o worker. Snapshot tipado de condutor, datas, bônus/renovação e preferências de cobertura, copiado na criação. Sem endpoint de edição desse snapshot.
- Migration `20261009180000_AddAutoQuoteSubmission`: tabelas normalizadas `quotes.quote_auto_submissions` e `quotes.quote_requested_coverages`, FKs, checks e RLS via solicitação pai; campos ProviderCode/ProviderName nos resultados. Snapshot e designer EF atualizados.
- Aggilizador usa condutor e vigência explícitos; os valores de cobertura vêm do pedido. Bônus e limite zero são preservados. Não copia habilitação do segurado para outro condutor.
- Sem snapshot, cobertura não traduzível, quantidade não suportada ou de-para ausente: Restriction local, sem HTTP. Outros adaptadores precisam declarar `SupportsAutoSubmission` e honrar esse contrato antes de receber pedidos novos.
- Novos dicionários opcionais da configuração do Aggilizador: InsuranceTypeCodes, DriverMaritalStatusCodes, DeductibleTypeCodes e CoverageOptionCodes (chave `codigo-cobertura:opcao`). Estão vazios por padrão. Preencher apenas com documentação/evidência legítima; os códigos `fixture-*` dos testes são fictícios.
- Aceite HTTP com referência retorna Pending, sem prêmio comercial presumido. Pedido fica AwaitingResults (enum 5); não há polling/webhook implementado. Seguradora/produto podem ser informados explicitamente pelo provider; o aceite do agregador deixa esses campos vazios e identifica o motor separadamente. Resposta sem referência não confirma envio.
- Todos os resultados com falha/restrição agora levam a Failed, não a falso sucesso parcial.
- Correção incidental necessária para validar: ImageSharp 3.1.12 bloqueou restore por vulnerabilidades. 4.1.2 exigia licença ausente. Removemos ImageSharp (inclusive locks); OCR gera PPM/P6 a partir de BGRA com composição sobre branco, suportado pelo Tesseract. Nenhuma licença foi contornada e nenhum alerta foi suprimido. Verificar OCR real, além dos testes de pixels.
- WassisCRM recebeu somente micro-plano e acompanhamento. `database.ts`, telas e hooks não foram alterados. O frontend continua em memória.

## Validação e limites do checkpoint

- Restore locked e auditoria de pacotes passaram após remover ImageSharp; auditoria sem vulnerabilidades conhecidas nas fontes consultadas.
- Compilação Release e verificação EF de mudanças pendentes de modelo passaram durante a rodada.
- Testes antes das últimas correções: 27/29 focados passaram. As duas falhas foram DateOnly usando formato com hora e coleção owned inicializada como array fixo. Corrigidos para formatação explícita e List, respectivamente.
- A execução seguinte foi BLOQUEADA pelo Controle de Aplicativos do Windows (`0x800711C7`) ao carregar WAssis.Tests.dll. O runner retornou código zero com zero testes; isso NÃO é aprovação. Não desativar o controle de segurança para contornar o bloqueio. Executar os testes em ambiente autorizado/CI.
- PostgreSQL real não foi validado nesta rodada: não foram encontrados Docker/servidor local utilizáveis. Há teste novo `AutoQuoteSubmissionPostgresTests` para roundtrip e rollback atomicamente com outbox. Executar também os testes de isolamento/RLS usando papel sem bypass; não certificar RLS apenas com usuário superuser.
- Nenhuma migration foi aplicada em banco do usuário. Nenhuma chamada a seguradora/SegFy, publicação em produção ou teste OCR real foi feita.

## Próximos passos, em ordem

1. Revisar diff e executar `dotnet restore WAssisInsurance.sln --locked-mode`, build Release e suíte completa. Para PostgreSQL usar Testcontainers ou `scripts/test-security-postgres.ps1` com cluster descartável. Confirmar contagem efetiva de testes, RLS nas duas tabelas novas, rollback, enum AwaitingResults e serialização HTTP. Homologar conversão PPM com PDFium/Tesseract no worker isolado.
2. O envelope novo é adaptação do legado, NÃO implementação das tabelas `erp.calculos`. Implementar os serviços/APIs canônicos usando o esquema v3.1 já presente: snapshots+CTI+coberturas atomicamente, versão imutável, derivação de renovação própria da apólice, escopo de empresa/filial/responsável e idempotência. Auditar a reutilização de CorrelationId com conteúdo diferente: o handler legado ainda devolve a solicitação anterior.
3. Não chamar o Aggilizador para RENOVACAO_PROPRIA até validar/derivar apólice de origem no serviço canônico. Hoje essa opção retorna restrição deliberada. Novo seguro recebe datas explícitas; a derivação da data da filial pertence ao domínio canônico, não ao adaptador.
4. Criar tentativas por seguradora, seleção de seguradoras, reexecução com referência à anterior, comissão aplicada e normalização de resultados/coberturas/pagamentos. Cada execução tem no máximo uma cotação. Separar engine de seguradora e produto. Construir consulta assíncrona de resultados com contrato documentado, sem replay cego de envios ambíguos.
5. Revisar restantes defaults do questionário do Aggilizador (garagem, uso, profissão etc.), ainda fora deste recorte. Mapear dados normalizados e exigir de-para validado antes de considerar integração homologada. Campos legados CalculationOrigin e códigos crus também não equivalem ao domínio canônico.
6. Somente com API canônica disponível, abrir/atualizar micro-plano no WassisCRM e conectar useCalculos/execuções/apresentações via HTTP, preservando demonstração em mock e evitando sucesso simulado em modo conectado. Persistir seleção comparativa de até cinco cotações e opção de pagamento. Impressão via window.print não equivale a PDF armazenado/link público.
7. Integrar novas evidências SegFy à análise: distinguir observação dinâmica de inferência estática, não importar cookies/credenciais, confirmar autorização e contrato antes de qualquer integração real.

Documentos: backend `docs/multicalculo-transmissao-plan.md`; frontend `.codex/plans/micro-plano-multicalculo-transmissao-2026-10-09.md` e macro. Preservar as correções anteriores de Auth0/BFF, sessão e isolamento de workers. Este checkpoint é desenvolvimento em andamento, não liberação de produção.

Referências técnicas da correção incidental: https://github.com/advisories/GHSA-jjfr-hcj7-qf5w ; https://tesseract-ocr.github.io/tessdoc/InputFormats.html ; https://netpbm.sourceforge.net/doc/ppm.html .
