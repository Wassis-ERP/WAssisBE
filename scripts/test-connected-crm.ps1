param(
    [Parameter(Mandatory)][string]$PostgresBinDirectory,
    [Parameter(Mandatory)][string]$CrmDirectory
)
$ErrorActionPreference = 'Stop'
# Run only inside test-security-postgres.ps1's ephemeral cluster. No production inputs.
$settings = [System.Data.Common.DbConnectionStringBuilder]::new()
$settings.set_ConnectionString($env:WASSIS_TEST_POSTGRES_CONNECTION)
if ($settings['Host'] -ne '127.0.0.1' -or $settings['Database'] -ne 'postgres' -or
    $settings['Username'] -ne 'wassis_fixture' -or $settings['Ssl Mode'] -ne 'VerifyFull') {
    throw 'Use the disposable cluster created by test-security-postgres.ps1.'
}
$repo = Split-Path $PSScriptRoot -Parent
$crm = (Resolve-Path -LiteralPath $CrmDirectory).Path
$psql = Join-Path $PostgresBinDirectory 'psql.exe'
$database = 'wassis_browser_' + [Guid]::NewGuid().ToString('N')
$role = 'wassis_browser_app_' + [Guid]::NewGuid().ToString('N')
$rolePassword = [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
$runDirectory = Join-Path ([IO.Path]::GetTempPath()) $database
$null = New-Item -ItemType Directory -Path $runDirectory
$api = $null
$frontend = $null
$created = $false
$roleCreated = $false
$env:PGHOST = '127.0.0.1'
$env:PGPORT = [string]$settings['Port']
$env:PGUSER = 'wassis_fixture'
$env:PGPASSWORD = [string]$settings['Password']
$env:PGSSLMODE = 'verify-full'
$env:PGSSLROOTCERT = [string]$settings['Root Certificate']

function Invoke-FixtureSql([string]$Sql, [string]$Target = $database) {
    $Sql | & $psql -X -q -v ON_ERROR_STOP=1 -d $Target
    if ($LASTEXITCODE -ne 0) { throw 'Disposable database setup failed.' }
}
function Get-FreePort {
    $probe = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
    $probe.Start()
    $result = $probe.LocalEndpoint.Port
    $probe.Stop()
    return $result
}
function Wait-LocalHttp([string]$Url, [Diagnostics.Process]$Process) {
    for ($attempt = 0; $attempt -lt 90; $attempt++) {
        if ($Process.HasExited) { throw 'Local test service exited. Inspect the test logs.' }
        try { if ((Invoke-WebRequest $Url -TimeoutSec 2).StatusCode -eq 200) { return } } catch { }
        Start-Sleep -Milliseconds 500
    }
    throw 'Local test service did not become ready.'
}
try {
    Invoke-FixtureSql "CREATE DATABASE $database" 'postgres'
    $created = $true
    $settings['Database'] = $database
    $env:ConnectionStrings__DefaultConnection = $settings.get_ConnectionString()
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:Database__AutoMigrate = 'false'
    $env:Identity__Jwt__SigningKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
    $env:Identity__Auth0__Enabled = 'false'
    $env:Identity__DevelopmentAuth__Enabled = 'true'
    $env:Documents__PrivateStorage__RootPath = Join-Path $runDirectory 'documents'
    $env:Serilog__MinimumLevel__Default = 'Warning'
    $dll = Join-Path $repo 'src/WAssis.Services.Api/bin/Release/net10.0/WAssis.Services.Api.dll'
    $dotnet = (Get-Command dotnet).Source
    Push-Location (Join-Path $repo 'src/WAssis.Services.Api')
    try {
        & $dotnet $dll --migrate *> (Join-Path $runDirectory 'migrate.log')
        if ($LASTEXITCODE -ne 0) { throw 'Disposable migration failed. Inspect migrate.log.' }
    } finally { Pop-Location }
    Invoke-FixtureSql (Get-Content (Join-Path $repo 'tests/Fixtures/connected-ui.sql') -Raw)
    Invoke-FixtureSql "CREATE ROLE $role LOGIN PASSWORD '$rolePassword' NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS"
    $roleCreated = $true
    # Fixture grants only; this is not the operational least-privilege role model.
    foreach ($schema in @('public','erp','quotes','documents','financial','policies','billing','whatsapp_support','operations','infrastructure','security')) {
        Invoke-FixtureSql "GRANT USAGE ON SCHEMA $schema TO $role; GRANT SELECT,INSERT,UPDATE,DELETE ON ALL TABLES IN SCHEMA $schema TO $role; GRANT USAGE,SELECT ON ALL SEQUENCES IN SCHEMA $schema TO $role"
    }
    $settings['Username'] = $role
    $settings['Password'] = $rolePassword
    $env:ConnectionStrings__DefaultConnection = $settings.get_ConnectionString()
    $fixture = @{
        Username='user0@example.invalid'; Password='local-integration-fixture-password'
        UserId='11111111-1111-1111-1111-111111111111'; TenantId='22222222-2222-2222-2222-222222222222'
        BrokerageId='44444444-4444-4444-4444-444444444444'; BranchId='44444444-4444-4444-4444-444444444444'
        BranchIds__0='44444444-4444-4444-4444-444444444444'; UserType='brokerage_staff'; Roles__0='brokerage_admin'
    }
    foreach ($key in $fixture.Keys) { [Environment]::SetEnvironmentVariable('Identity__DevelopmentAuth__Users__0__'+$key,$fixture[$key],'Process') }
    $apiPort = Get-FreePort
    $frontendPort = Get-FreePort
    $env:WASSIS_E2E_API_URL = "http://127.0.0.1:$apiPort"
    $env:WASSIS_E2E_URL = "http://127.0.0.1:$frontendPort"
    $env:Frontend__AllowedOrigins__0 = $env:WASSIS_E2E_URL
    $api = Start-Process $dotnet -ArgumentList @(('"'+$dll+'"'),'--urls',$env:WASSIS_E2E_API_URL) -WorkingDirectory (Join-Path $repo 'src/WAssis.Services.Api') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runDirectory 'api.log') -RedirectStandardError (Join-Path $runDirectory 'api-error.log')
    Wait-LocalHttp ($env:WASSIS_E2E_API_URL+'/health') $api
    $env:VITE_AUTH_MODE = 'backend'
    $env:VITE_AUTH_PROVIDER = ''
    $env:VITE_DATA_MODE = 'backend'
    $env:VITE_API_BASE_URL = $env:WASSIS_E2E_API_URL
    $env:BROWSER = 'none'
    $node = (Get-Command node).Source
    $vite = Join-Path $crm 'node_modules/vite/bin/vite.js'
    $frontend = Start-Process $node -ArgumentList @(('"'+$vite+'"'),'--host','127.0.0.1','--port',$frontendPort,'--strictPort') -WorkingDirectory $crm -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runDirectory 'crm.log') -RedirectStandardError (Join-Path $runDirectory 'crm-error.log')
    Wait-LocalHttp $env:WASSIS_E2E_URL $frontend
    Push-Location $crm
    try {
        & $node node_modules/@playwright/test/cli.js test --config playwright.config.ts
        if ($LASTEXITCODE -ne 0) { throw 'Connected browser journey failed.' }
    } finally { Pop-Location }
    Write-Output 'Connected CRM journey passed with a disposable PostgreSQL database and non-owner/NOBYPASSRLS application role.'
} finally {
    foreach ($process in @($frontend,$api)) { if ($process -and !$process.HasExited) { $process.Kill($true); $process.WaitForExit() } }
    if ($created) { Invoke-FixtureSql "DROP DATABASE $database WITH (FORCE)" 'postgres' }
    if ($roleCreated) { Invoke-FixtureSql "DROP ROLE $role" 'postgres' }
    $env:PGPASSWORD = $null
    $env:ConnectionStrings__DefaultConnection = $null
    $env:Identity__Jwt__SigningKey = $null
    Write-Output "Disposable test logs: $runDirectory"
}
