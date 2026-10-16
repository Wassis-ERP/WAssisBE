# Seguradoras vinculadas ao histórico — 12/10/2026

Recorte de dados autorizado pelo usuário; design permanece com Renato. Base BE
PR #35. O lookup comercial ativo atende o editor, mas omite nomes de seguradoras
inativadas que já têm execuções na oportunidade. Adicionar opt-in de leitura
`includeReferencedInactive=true`, incluindo apenas inativas já vinculadas ao
histórico desta oportunidade. Mesmo tenant/filial/PROPRIO, projeção mínima e
paginação; nenhum diretório administrativo ou campo sensível adicional.

- [x] Query/controller/store mantêm padrão ativo e aceitam opt-in para histórico.
- [x] Testar nome histórico disponível sem Configurações, inativa não relacionada
  excluída, tentativa nova recusada e isolamento de outra oportunidade/PROPRIO.
- [x] Gates e checkpoint. Nenhuma migration ou reativação do cadastro.

Conferidas tabelas v3.1: oportunidades → calculos → calculo_execucoes → seguradoras.
Este retorno serve rótulos da leitura, não opções habilitadas para novas tentativas.

Release/161 testes sem PostgreSQL aprovados; sete integrações passaram no gate
completo e a jornada HTTP ampliada passou na repetição focada. Corrigida a fixture
para usar retry da última tentativa terminal: criar uma primeira tentativa para
seguradora com histórico retorna 409 por regra do ciclo de vida; retry com seguradora
inativada retorna 400. Nenhum cadastro foi reativado em ambiente compartilhado;
apenas fixture descartável restaurada no teste. Projeção continua id/tenant_id/nome.

## Atualização do runtime após gate remoto — 14/10/2026

O CI de `4d7a0b6` aprovou build/testes/audit e falhou no scan da imagem API:
ASP.NET/.NET 10.0.12 apresentou 12 ocorrências HIGH/CRITICAL corrigíveis, duas
CRITICAL. A Microsoft publicou o patch 10.0.13 em 13/10/2026:
[release oficial](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.13/10.0.13.md).

Atualizado somente o estágio final das imagens API e worker para ASP.NET 10.0.13.
Digest do índice obtido e conferido com SHA-256 do manifesto em MCR:
`sha256:d5633778c754e89a386882eddf8dbee6942447a8a4b8267c19cf1e1158d0b0ac`;
contém linux/amd64. SDK de build, pacotes e migrations preservados. O gate de scan
continua com HIGH/CRITICAL, ignore-unfixed e exit-code 1. Build e scan das duas
imagens serão validados pelo novo CI; Docker Desktop local indisponível. Nenhum
deploy ou atualização em servidor compartilhado.
