[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(1,9999)][int]$BuildNumber,[switch]$UseRecoveryKey,[switch]$FamilyLan)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$record=Get-Content -LiteralPath (Join-Path $root 'LocalData\android-signing.json') -Raw | ConvertFrom-Json
$pin=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'android-family-signing.json') -Raw | ConvertFrom-Json
if ($record.certificateSha256 -ne $pin.certificateSha256 -or $record.alias -ne $pin.alias) { throw 'Private registration does not match the pinned family identity.' }
$key=if ($UseRecoveryKey) { Join-Path $record.recovery 'family-release.p12' } else { $record.keystore }
if ((Get-FileHash -LiteralPath $key -Algorithm SHA256).Hash.ToLowerInvariant() -ne $record.keystoreSha256) { throw 'Signing key hash mismatch.' }
$phase=if($FamilyLan){'G3'}else{'G1'}
$output=Join-Path $root "Builds\AndroidSigned\$phase-0.0.$BuildNumber"
$source=if($FamilyLan){Join-Path $root "Builds\AndroidFamilyLAN\G3-0.0.$BuildNumber"}else{Join-Path $root "Builds\Android\G1-0.0.$BuildNumber"}
if(Test-Path -LiteralPath $output){throw 'Use a fresh build number. Signed outputs are never overwritten.'}
if($FamilyLan){
    # Build-AndroidLAN just produced this intermediate. Refuse stale source even
    # if this signing entry point is invoked directly at a later time.
    $sourceRecord=Get-Content -LiteralPath (Join-Path $source 'source-manifest.json') -Raw | ConvertFrom-Json
    $currentPaths=@(& git -C $root ls-files --cached --others --exclude-standard -- Unity/FamilyPlayset/Assets Unity/FamilyPlayset/Packages Unity/FamilyPlayset/ProjectSettings Tools | Sort-Object -Unique | Where-Object {Test-Path -LiteralPath (Join-Path $root $_) -PathType Leaf})
    if(@(Compare-Object $currentPaths @($sourceRecord.files.path | Sort-Object -Unique)).Count){throw 'Family LAN source file set changed after the build.'}
    foreach($entry in $sourceRecord.files){
        $path=Join-Path $root $entry.path
        if(!(Test-Path -LiteralPath $path -PathType Leaf) -or (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $entry.sha256){throw 'Family LAN source changed after the build.'}
    }
    $built=Get-Content -LiteralPath (Join-Path $source 'build-summary.json') -Raw | ConvertFrom-Json
    if($built.version -ne "0.0.$BuildNumber" -or $built.result -ne 'Succeeded' -or $built.development -or $built.profile -ne 'Assets/BuildProfiles/Android Family LAN.asset'){throw 'Wrong family LAN artifact.'}
}else{
    if(Test-Path -LiteralPath $source){throw 'Use a fresh build number.'}
    & (Join-Path $PSScriptRoot 'Build-Foundation.ps1') -Target Android -BuildNumber $BuildNumber
}
$manifest=@(Get-Content -LiteralPath (Join-Path $source 'artifact-manifest.json') -Raw | ConvertFrom-Json)
$inputApk=Join-Path $source 'LittleWeeps.apk'
$inputHash=(Get-FileHash -LiteralPath $inputApk -Algorithm SHA256).Hash.ToLowerInvariant()
if (@($manifest | Where-Object { $_.path -eq 'LittleWeeps.apk' -and $_.sha256 -eq $inputHash }).Count -ne 1) { throw 'Fresh build artifact hash verification failed.' }
$toolchain=Get-Content -LiteralPath (Join-Path $root 'LocalData\android-toolchain.json') -Raw | ConvertFrom-Json
$buildTools=Join-Path $toolchain.root 'SDK\build-tools\36.0.0'
$previousJava=$env:JAVA_HOME
$previousSecret=$env:LITTLE_WEEPS_KEY_PASS
$passwordPointer=[IntPtr]::Zero
try {
    $env:JAVA_HOME=Join-Path $toolchain.root 'OpenJDK'
    if ($UseRecoveryKey) {
        $env:LITTLE_WEEPS_KEY_PASS=[IO.File]::ReadAllText((Join-Path $record.recovery 'recovery-password.txt'))
    } else {
        $secure=Import-Clixml -LiteralPath $record.passwordDpapi
        $passwordPointer=[Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
        $env:LITTLE_WEEPS_KEY_PASS=[Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)
    }
    & (Join-Path $buildTools 'zipalign.exe') -c -P 16 4 $inputApk
    if ($LASTEXITCODE -ne 0) { throw 'Unity input APK packaging alignment failed.' }
    New-Item -ItemType Directory -Path $output | Out-Null
    $apk=Join-Path $output 'LittleWeeps.apk'
    & (Join-Path $buildTools 'apksigner.bat') sign --ks $key --ks-key-alias $pin.alias --ks-pass env:LITTLE_WEEPS_KEY_PASS --key-pass env:LITTLE_WEEPS_KEY_PASS --v4-signing-enabled false --out $apk $inputApk
    if ($LASTEXITCODE -ne 0) { throw 'Family APK signing failed.' }
    $verification=@(& (Join-Path $buildTools 'apksigner.bat') verify --verbose --print-certs $apk 2>&1)
    if ($LASTEXITCODE -ne 0) { throw 'Signed APK verification failed.' }
    $text=$verification -join "`n"
    if ($text -notmatch 'Signer #1 certificate SHA-256 digest: ([0-9a-fA-F]+)' -or $Matches[1].ToLowerInvariant() -ne $pin.certificateSha256 -or $text -notmatch '(?m)^Number of signers: 1\s*$') { throw 'Unexpected APK signing identity.' }
    & (Join-Path $buildTools 'zipalign.exe') -c -P 16 4 $apk
    if ($LASTEXITCODE -ne 0) { throw 'Signed APK packaging alignment failed.' }
    # Compare every non-signature ZIP entry, so external signing cannot silently change game content.
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    function Get-PayloadHashes($path) {
        $archive=[IO.Compression.ZipFile]::OpenRead($path)
        try {
            $items=[Collections.Generic.Dictionary[string,string]]::new([StringComparer]::Ordinal)
            foreach ($entry in $archive.Entries | Where-Object { $_.FullName -notmatch '^META-INF/(MANIFEST\.MF|[^/]+\.(SF|RSA|DSA|EC))$' }) {
                if ($items.ContainsKey($entry.FullName)) { throw 'Duplicate APK entry.' }
                $stream=$entry.Open();$hash=[Security.Cryptography.SHA256]::Create()
                try { $items[$entry.FullName]=[BitConverter]::ToString($hash.ComputeHash($stream)) } finally { $stream.Dispose();$hash.Dispose() }
            }
            return $items
        } finally { $archive.Dispose() }
    }
    $before=Get-PayloadHashes $inputApk
    $after=Get-PayloadHashes $apk
    if ($before.Count -ne $after.Count) { throw 'Signing changed the payload entry count.' }
    foreach ($name in $before.Keys) { if ($after[$name] -ne $before[$name]) { throw "Signing changed payload: $name" } }
    $verification | Set-Content -LiteralPath (Join-Path $output 'apksigner.txt') -Encoding utf8
    Copy-Item -LiteralPath (Join-Path $source 'source-manifest.json') -Destination $output
    $summary=Get-Content -LiteralPath (Join-Path $source 'build-summary.json') -Raw | ConvertFrom-Json
    $summary.output=$apk
    $summary.signing='Pinned Little Weeps family key; externally signed unchanged Unity payload'
    $summary | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'build-summary.json') -Encoding utf8
    [ordered]@{utc=[DateTime]::UtcNow.ToString('O');sourceApkSha256=$inputHash;certificateSha256=$pin.certificateSha256;usedRecoveryKey=[bool]$UseRecoveryKey;payloadEntriesVerified=$before.Count;payloadUnchanged=$true;offDeviceBackupVerified=$false} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'signing-evidence.json') -Encoding utf8
    @([ordered]@{path='LittleWeeps.apk';bytes=(Get-Item -LiteralPath $apk).Length;sha256=(Get-FileHash -LiteralPath $apk -Algorithm SHA256).Hash.ToLowerInvariant()}) | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'artifact-manifest.json') -Encoding utf8
    Write-Host "Fresh family-signed build ready: $apk. Payload unchanged; recovery key used: $UseRecoveryKey"
} finally {
    $env:JAVA_HOME=$previousJava
    $env:LITTLE_WEEPS_KEY_PASS=$previousSecret
    if ($passwordPointer -ne [IntPtr]::Zero) { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer) }
}
