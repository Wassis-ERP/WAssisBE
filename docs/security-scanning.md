# Varredura local de segredos

O CI executa a CLI oficial Gitleaks 8.30.1 no histórico completo. O script
`scripts/install-gitleaks.ps1` verifica um SHA-256 fixo antes de extrair o binário
x64 Windows/Linux. A action comercial deixou de ser usada; o scanner continua
obrigatório, com saída redigida e falha em achados. Nenhuma licença da action é necessária.

Para habilitar o mesmo gate antes de commits neste clone:

```text
git config core.hooksPath .githooks
```

O binário oficial `gitleaks` deve estar no `PATH`. Para uma varredura manual de todo o histórico, execute `scripts/scan-secrets.ps1`. A saída deve ser compartilhada somente como categoria/regra, caminho e linha; nunca copie o valor detectado para tickets ou logs.

## Dependências NuGet

`Directory.Build.props` habilita auditoria de dependências diretas e transitivas,
de severidade low em diante. NU1900–NU1905 são erros: vulnerabilidade conhecida,
falha na consulta ou fonte sem dados de segurança interrompem o restore/build.
`NuGet.Config` define a fonte oficial de auditoria sem alterar fontes de pacotes.
O Dockerfile copia ambos antes do restore. O relatório `dotnet list package
--vulnerable` é informativo; o bloqueio ocorre no restore, conforme os
[controles oficiais do NuGet](https://learn.microsoft.com/en-us/nuget/concepts/auditing-packages).

Validação em 16/09/2026: restore locked da solução aprovado; projeto descartável
com Newtonsoft.Json 12.0.3 rejeitado com NU1903/GHSA-5crp-9r3c-p9vr. Esse pacote
vulnerável não foi adicionado à solução.
