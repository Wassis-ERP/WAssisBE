param([Parameter(Mandatory)][string]$PostgresBinDirectory)
$ErrorActionPreference = 'Stop'
$cluster = Join-Path ([IO.Path]::GetTempPath()) ('wassis-security-' + [Guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $cluster
$data = Join-Path $cluster 'data'
$passwordFile = Join-Path $cluster 'password.txt'
$password = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
[IO.File]::WriteAllText($passwordFile, $password)
$portProbe = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
$portProbe.Start()
$port = $portProbe.LocalEndpoint.Port
$portProbe.Stop()
$started = $false
$oldConnection = $env:WASSIS_TEST_POSTGRES_CONNECTION
try {
    & (Join-Path $PostgresBinDirectory 'initdb.exe') -D $data -U wassis_fixture --pwfile=$passwordFile --auth=scram-sha-256 --encoding=UTF8 --locale=C
    if ($LASTEXITCODE -ne 0) { throw 'Could not initialize disposable PostgreSQL.' }
    & (Join-Path $PostgresBinDirectory 'pg_ctl.exe') -D $data -l (Join-Path $cluster 'postgres.log') -o "-h 127.0.0.1 -p $port" -w start
    if ($LASTEXITCODE -ne 0) { throw 'Could not start disposable PostgreSQL.' }
    $started = $true
    $env:WASSIS_TEST_POSTGRES_CONNECTION = "Host=127.0.0.1;Port=$port;Database=postgres;Username=wassis_fixture;Password=$password"
    & dotnet test tests/WAssis.Tests/WAssis.Tests.csproj --configuration Release --no-restore --filter 'FullyQualifiedName~SecurityServicesPostgresTests' --logger 'console;verbosity=normal' --verbosity quiet
    $testExitCode = $LASTEXITCODE
} finally {
    $env:WASSIS_TEST_POSTGRES_CONNECTION = $oldConnection
    if ($started) { & (Join-Path $PostgresBinDirectory 'pg_ctl.exe') -D $data -w -m fast stop }
    Remove-Item -LiteralPath $passwordFile -ErrorAction SilentlyContinue
}
exit $testExitCode
