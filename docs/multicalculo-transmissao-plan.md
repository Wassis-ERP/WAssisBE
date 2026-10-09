# Integridade da transmissão Auto — 09/10/2026

Base: WAssisBE a93e855 e WassisCRM 64d6d22. Contrato canônico v3.1 preservado.

Primeiro recorte: corrigir a transmissão legada sem fingir que a API `quotes/requests` implementa todo o domínio canônico. O snapshot `AutoSubmission` é um envelope persistido de envio, com colunas e coleção relacionais, sem JSON de negócio. É criado uma vez com a solicitação e percorre API → comando → armazenamento → worker → adaptador. Pedidos antigos sem snapshot não serão enviados ao Aggilizador com dados inventados.

Aceite: condutor distinto, vigência explícita, bônus zero e limites por versão chegam intactos; ausência de de-para gera restrição local; HTTP aceito não vira cotação concluída. Validar unidade, cadeia de mapeamento e roundtrip PostgreSQL/RLS em banco descartável. Não aplicar migration em ambiente do usuário.

Continuidade: implementar serviços canônicos ERP (pedido/execuções/resultados), ligação do frontend, renovação própria derivada de apólice e apresentações persistidas. Antes disso, consolidar a raiz de oportunidade: a API conectada atual persiste `public.oportunidades`, enquanto as FKs canônicas apontam para `erp.oportunidades`; não criar dual-write nem copiar vínculos silenciosamente para contornar essa fronteira. Essa etapa não depende da engenharia reversa SegFy; integração específica aguarda contrato e acesso autorizados. Não criar endpoint ou código de fio SegFy por inferência.

## Checkpoint

Implementados envelope de transmissão, persistência relacional/migration, validações, mapeamento explícito do Aggilizador, capacidade declarada por adaptador, identidade separada do motor e estado AwaitingResults. Ainda não implementar compartilhamento ou alegar integração de ponta a ponta. Dicionários de de-para vazios impedem envio até configuração homologada. Renovação própria bloqueada até validar a apólice no serviço canônico. Questionário legado ainda usa defaults e precisa de etapa própria.

ImageSharp foi removido após alertas de segurança no restore e exigência de licença na versão corrigida; conversão de pixels para PPM substitui seu único uso no OCR. Sem mudança de política de auditoria.

Restore locked com SDK 10.0.401, compilação Release sem avisos, auditoria NuGet e alinhamento do modelo EF verificados novamente em 09/10/2026. A suíte local sem PostgreSQL passou com 143/143 testes, inclusive as correções de DateOnly, coleção EF e conversão PPM. O run 37962842731 aprovou os 148/148 testes, incluindo as cinco integrações PostgreSQL/Testcontainers; a repetição local não iniciou porque o Docker Desktop não ficou disponível. OCR real permanece pendente porque não há binário Tesseract instalado. Nenhum banco de usuário foi migrado.

O CI do checkpoint falhou apenas no scan da imagem API: o digest anterior do runtime ASP.NET 10.0.12 continha `CVE-2026-84782` corrigível em `libssl3t64` e `openssl`. API e worker foram atualizados para o digest oficial corrente do mesmo runtime; Trivy 0.75.0 confirmou zero HIGH/CRITICAL no novo artefato remoto. A etapa canônica (`calculos`, execuções, resultados e apresentações) continua sendo o próximo recorte funcional e não foi disfarçada como concluída por esta correção de pipeline.
