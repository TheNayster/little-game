[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$primary=Join-Path $env:LOCALAPPDATA 'LittleWeeps\Signing\Android'
$recovery=Join-Path $env:USERPROFILE 'Documents\Little Weeps Recovery\Android Signing'
$pin=Join-Path $PSScriptRoot 'android-family-signing.json'
$recordPath=Join-Path $root 'LocalData\android-signing.json'
if ((Test-Path -LiteralPath $pin) -or (Test-Path -LiteralPath $recordPath)) {
    if (!(Test-Path -LiteralPath $pin) -or !(Test-Path -LiteralPath $recordPath)) { throw 'Incomplete signing registration. Preserve the existing key and investigate; do not generate a replacement.' }
    $record=Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json
    $identity=Get-Content -LiteralPath $pin -Raw | ConvertFrom-Json
    if ($identity.certificateSha256 -ne $record.certificateSha256) { throw 'Signing registration does not match the pinned public identity.' }
    foreach ($path in @($record.keystore,(Join-Path $record.recovery 'family-release.p12'))) {
        if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $record.keystoreSha256) { throw 'Signing key is missing or changed; do not replace it.' }
    }
    Write-Host "Signing identity already registered: $($identity.certificateSha256)"
    return
}
foreach ($directory in @($primary,$recovery)) {
    if (Test-Path -LiteralPath $directory) { throw "Signing destination already exists without a registration. Inspect it before continuing: $directory" }
}
$toolchain=Get-Content -LiteralPath (Join-Path $root 'LocalData\android-toolchain.json') -Raw | ConvertFrom-Json
$keytool=Join-Path $toolchain.root 'OpenJDK\bin\keytool.exe'
if (!(Test-Path -LiteralPath $keytool)) { throw 'Verified JDK keytool is missing.' }
foreach ($directory in @($primary,$recovery)) {
    New-Item -ItemType Directory -Path $directory | Out-Null
    $acl=New-Object System.Security.AccessControl.DirectorySecurity
    $acl.SetAccessRuleProtection($true,$false)
    foreach ($sid in @([Security.Principal.WindowsIdentity]::GetCurrent().User, [Security.Principal.SecurityIdentifier]::new('S-1-5-18'))) {
        $acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($sid,'FullControl','ContainerInherit,ObjectInherit','None','Allow'))
    }
    Set-Acl -LiteralPath $directory -AclObject $acl
}
$key=Join-Path $primary 'family-release.p12'
$secretBytes=New-Object byte[] 32
$rng=[Security.Cryptography.RandomNumberGenerator]::Create()
try { $rng.GetBytes($secretBytes) } finally { $rng.Dispose() }
$secret=[Convert]::ToBase64String($secretBytes)
$oldSecret=$env:LITTLE_WEEPS_KEY_PASS
try {
    $env:LITTLE_WEEPS_KEY_PASS=$secret
    & $keytool -genkeypair -keystore $key -storetype PKCS12 -alias littleweeps-family -keyalg RSA -keysize 3072 -sigalg SHA256withRSA -validity 10000 -dname 'CN=Little Weeps Family' -storepass:env LITTLE_WEEPS_KEY_PASS -keypass:env LITTLE_WEEPS_KEY_PASS
    if ($LASTEXITCODE -ne 0) { throw 'Key generation failed. Preserve the destination for investigation.' }
    $certificateFile=Join-Path $primary 'family-certificate.der'
    & $keytool -exportcert -keystore $key -alias littleweeps-family -storepass:env LITTLE_WEEPS_KEY_PASS -file $certificateFile
    if ($LASTEXITCODE -ne 0) { throw 'Public certificate export failed.' }
    $fingerprint=(Get-FileHash -LiteralPath $certificateFile -Algorithm SHA256).Hash.ToLowerInvariant()
    ConvertTo-SecureString $secret -AsPlainText -Force | Export-Clixml -LiteralPath (Join-Path $primary 'password.dpapi.xml')
    Copy-Item -LiteralPath $key -Destination (Join-Path $recovery 'family-release.p12')
    Copy-Item -LiteralPath $certificateFile -Destination (Join-Path $recovery 'family-certificate.der')
    # Portable recovery is protected by the directory ACL, not tied to this Windows account's DPAPI.
    [IO.File]::WriteAllText((Join-Path $recovery 'recovery-password.txt'),$secret,[Text.UTF8Encoding]::new($false))
    $keyHash=(Get-FileHash -LiteralPath $key -Algorithm SHA256).Hash.ToLowerInvariant()
    if ((Get-FileHash -LiteralPath (Join-Path $recovery 'family-release.p12') -Algorithm SHA256).Hash.ToLowerInvariant() -ne $keyHash) { throw 'Recovery key copy does not match.' }
    [ordered]@{package='com.littleweeps.familyplayset';alias='littleweeps-family';certificateSha256=$fingerprint;algorithm='RSA-3072';createdUtc=[DateTime]::UtcNow.ToString('O')} | ConvertTo-Json | Set-Content -LiteralPath $pin -Encoding utf8
    [ordered]@{keystore=$key;passwordDpapi=(Join-Path $primary 'password.dpapi.xml');recovery=$recovery;alias='littleweeps-family';certificateSha256=$fingerprint;keystoreSha256=$keyHash;offDeviceBackupVerified=$false} | ConvertTo-Json | Set-Content -LiteralPath $recordPath -Encoding utf8
    @'
Little Weeps Android signing recovery

This folder contains the encrypted PKCS12 private key, its recovery password and public certificate.
Keep the whole folder private. It is restricted to your Windows account and SYSTEM on this PC.
The password file makes recovery possible on another computer; do not publish or commit it.
This is a local recovery copy, not an off-device backup. Store a protected copy independently before family distribution.
Never generate a replacement key for the existing Android package. Updates must preserve this signing identity.
Tools/Build-AndroidFamily.ps1 -BuildNumber N -UseRecoveryKey verifies recovery by signing an update with this copy.
'@ | Set-Content -LiteralPath (Join-Path $recovery 'READ-ME.txt') -Encoding utf8
    Write-Host "Created this game's signing identity and protected local recovery copy. Public SHA256: $fingerprint"
} finally {
    $env:LITTLE_WEEPS_KEY_PASS=$oldSecret
    $secret=$null
    [Array]::Clear($secretBytes,0,$secretBytes.Length)
}
