[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(1,9999)][int]$BuildNumber,
      [ValidateSet('DebugProbe','Family')][string]$Signing='DebugProbe')
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$artifactKind=if ($Signing -eq 'Family') { 'AndroidSigned' } else { 'Android' }
$folder=Join-Path $root "Builds\$artifactKind\G1-0.0.$BuildNumber"
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if ($summary.result -ne 'Succeeded' -or $summary.development -or $summary.platform -ne 'Android' -or $summary.version -ne "0.0.$BuildNumber") { throw 'A matching non-development Android build is required.' }
$manifest=@(Get-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Raw | ConvertFrom-Json)
if (!($manifest | Where-Object path -eq 'LittleWeeps.apk')) { throw 'APK is missing from build artifact evidence.' }
foreach ($file in $manifest) {
    if ((Get-FileHash -LiteralPath (Join-Path $folder $file.path) -Algorithm SHA256).Hash -ne $file.sha256) { throw "Artifact changed: $($file.path)" }
}
$toolchain=Get-Content -LiteralPath (Join-Path $root 'LocalData\android-toolchain.json') -Raw | ConvertFrom-Json
if ($toolchain.unity -ne $summary.unity) { throw 'Inspection tools do not match the recorded build toolchain.' }
$buildTools=Join-Path $toolchain.root 'SDK\build-tools\36.0.0'
$apk=Join-Path $folder 'LittleWeeps.apk'
$badging=@(& (Join-Path $buildTools 'aapt2.exe') dump badging $apk)
if ($LASTEXITCODE -ne 0) { throw 'aapt2 could not inspect the APK.' }
$text=$badging -join "`n"
if ($text -notmatch "package: name='com\.littleweeps\.familyplayset' versionCode='$BuildNumber' versionName='0\.0\.$BuildNumber'") { throw 'Unexpected APK application identity or version.' }
if ($text -notmatch "(?m)^minSdkVersion:'26'" -or $text -notmatch "(?m)^targetSdkVersion:'36'") { throw 'Unexpected minimum or target SDK.' }
if ($text -notmatch "(?m)^native-code: 'arm64-v8a'\s*$") { throw 'APK must contain only the intended ARM64 ABI.' }
if ($text -match '(?m)^application-debuggable') { throw 'APK is debuggable; expected non-development player.' }
$previousJava=$env:JAVA_HOME
try {
    $env:JAVA_HOME=Join-Path $toolchain.root 'OpenJDK'
    $signatureOutput=@(& (Join-Path $buildTools 'apksigner.bat') verify --verbose --print-certs $apk 2>&1)
    if ($LASTEXITCODE -ne 0) { throw 'APK signature verification failed.' }
} finally { $env:JAVA_HOME=$previousJava }
$signingText=$signatureOutput -join "`n"
if ($signingText -notmatch 'Signer #1 certificate SHA-256 digest: ([0-9a-fA-F]+)') { throw 'No signing fingerprint returned.' }
$certificate=$Matches[1].ToLowerInvariant()
if ($signingText -notmatch '(?m)^Number of signers: 1\s*$') { throw 'Expected exactly one APK signer.' }
if ($Signing -eq 'Family') {
    $pin=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'android-family-signing.json') -Raw | ConvertFrom-Json
    if ($certificate -ne $pin.certificateSha256) { throw 'APK signer does not match the pinned family identity.' }
    $signingDescription='Pinned Little Weeps family key; device qualification remains separate'
} else {
    if ($signingText -notmatch 'Signer #1 certificate DN:.*CN=Android Debug') { throw 'Unexpected certificate for this explicitly debug-signed build-only probe.' }
    $signingDescription='Android Debug certificate; build-only probe'
}
$dir=Join-Path $root "LocalData\Verification\android-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $dir | Out-Null
# Static checks for modern Android memory-page sizes; device execution is still required.
# https://developer.android.com/guide/practices/page-sizes#check-alignment
$alignment=@(& (Join-Path $buildTools 'zipalign.exe') -v -c -P 16 4 $apk)
if ($LASTEXITCODE -ne 0) { throw 'APK ZIP alignment failed for 16 KB pages.' }
$alignment | Set-Content -LiteralPath (Join-Path $dir 'zipalign.txt') -Encoding utf8
$readelf=Join-Path $toolchain.root 'NDK\toolchains\llvm\prebuilt\windows-x86_64\bin\llvm-readelf.exe'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive=[IO.Compression.ZipFile]::OpenRead($apk)
$nativeLibraries=@()
$alignmentIssues=@()
$elfAligned=$true
$relroAligned=$true
try {
    foreach ($entry in $archive.Entries | Where-Object { $_.FullName -match '^lib/arm64-v8a/[^/]+\.so$' }) {
        $libraryPath=Join-Path $dir $entry.Name
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry,$libraryPath,$false)
        $headers=@(& $readelf -W -l $libraryPath)
        if ($LASTEXITCODE -ne 0) { throw "Could not inspect native library: $($entry.Name)" }
        $loads=@($headers | Where-Object { $_ -match '^\s*LOAD\s' })
        if (!$loads.Count) { throw "Missing ELF load segments: $($entry.Name)" }
        foreach ($line in $loads) {
            $fields=$line.Trim() -split '\s+'
            if ([Convert]::ToUInt64($fields[-1].Substring(2),16) -lt 16384) {
                $elfAligned=$false
                $alignmentIssues += "ELF load alignment failed: $($entry.Name)"
            }
        }
        foreach ($line in $headers | Where-Object { $_ -match '^\s*GNU_RELRO\s' }) {
            $fields=$line.Trim() -split '\s+'
            $end=[Convert]::ToUInt64($fields[2].Substring(2),16)+[Convert]::ToUInt64($fields[5].Substring(2),16)
            if ($end % 16384 -ne 0) {
                $relroAligned=$false
                $alignmentIssues += "ELF RELRO alignment failed: $($entry.Name); end=0x$($end.ToString('x'))"
            }
        }
        $headers | Set-Content -LiteralPath (Join-Path $dir ($entry.Name+'.headers.txt')) -Encoding utf8
        $nativeLibraries += $entry.FullName
    }
    if (!$nativeLibraries.Count) { throw 'No ARM64 libraries inspected.' }
} finally { $archive.Dispose() }
$badging | Set-Content -LiteralPath (Join-Path $dir 'aapt2-badging.txt') -Encoding utf8
$signatureOutput | Set-Content -LiteralPath (Join-Path $dir 'apksigner.txt') -Encoding utf8
[ordered]@{
    passed=($alignmentIssues.Count -eq 0);identityAndSignaturePassed=$true;utc=[DateTime]::UtcNow.ToString('O');version=$summary.version;package='com.littleweeps.familyplayset'
    minSdk=26;targetSdk=36;abi='arm64-v8a';debuggable=$false;certificateSha256=$certificate
    signing=$signingDescription;familyReleaseQualified=$false;deviceInstalled=$false
    zip16KbAligned=$true;elf16KbAligned=$elfAligned;relro16KbAligned=$relroAligned;nativeLibraries=$nativeLibraries;page16KbRuntimeTested=$false;alignmentIssues=$alignmentIssues
    apkSha256=(Get-FileHash -LiteralPath $apk -Algorithm SHA256).Hash.ToLowerInvariant()
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'apk-inspection.json') -Encoding utf8
if ($alignmentIssues.Count) { throw "APK identity/signature passed, but 16 KB static inspection needs follow-up. Evidence: $dir. $($alignmentIssues -join '; ')" }
Write-Host "PASS: APK identity, ARM64, SDK levels, signature and static 16 KB alignment inspection. No device or family-release qualification. Evidence: $dir"
