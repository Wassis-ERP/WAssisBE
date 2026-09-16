param(
    [Parameter(Mandatory)][string]$PostgresBinDirectory,
    [string]$Filter = 'Category=Postgres'
)
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
    $rsa = [Security.Cryptography.RSA]::Create(2048)
    $request = [Security.Cryptography.X509Certificates.CertificateRequest]::new('CN=localhost', $rsa, [Security.Cryptography.HashAlgorithmName]::SHA256, [Security.Cryptography.RSASignaturePadding]::Pkcs1)
    $request.CertificateExtensions.Add([Security.Cryptography.X509Certificates.X509BasicConstraintsExtension]::new($true, $false, 0, $true))
    $keyUsage = [Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature -bor [Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyCertSign
    $request.CertificateExtensions.Add([Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new($keyUsage, $true))
    $names = [Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder]::new()
    $names.AddDnsName('localhost')
    $names.AddIpAddress([Net.IPAddress]::Loopback)
    $request.CertificateExtensions.Add($names.Build())
    $certificate = $request.CreateSelfSigned([DateTimeOffset]::UtcNow.AddMinutes(-5), [DateTimeOffset]::UtcNow.AddDays(1))
    $rootCertificate = Join-Path $data 'server.crt'
    [IO.File]::WriteAllText($rootCertificate, $certificate.ExportCertificatePem())
    [IO.File]::WriteAllText((Join-Path $data 'server.key'), $rsa.ExportPkcs8PrivateKeyPem())
    $certificate.Dispose()
    $rsa.Dispose()
    & (Join-Path $PostgresBinDirectory 'pg_ctl.exe') -D $data -l (Join-Path $cluster 'postgres.log') -o "-h 127.0.0.1 -p $port -c ssl=on" -w start
    if ($LASTEXITCODE -ne 0) { throw 'Could not start disposable PostgreSQL.' }
    $started = $true
    $env:WASSIS_TEST_POSTGRES_CONNECTION = "Host=127.0.0.1;Port=$port;Database=postgres;Username=wassis_fixture;Password=$password;Ssl Mode=VerifyFull;Root Certificate=$rootCertificate"
    & dotnet test tests/WAssis.Tests/WAssis.Tests.csproj --configuration Release --no-restore --filter $Filter --logger 'console;verbosity=normal' --verbosity quiet
    $testExitCode = $LASTEXITCODE
} finally {
    $env:WASSIS_TEST_POSTGRES_CONNECTION = $oldConnection
    if ($started) { & (Join-Path $PostgresBinDirectory 'pg_ctl.exe') -D $data -w -m fast stop }
    Remove-Item -LiteralPath $passwordFile -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath (Join-Path $data 'server.key') -ErrorAction SilentlyContinue
}
exit $testExitCode
