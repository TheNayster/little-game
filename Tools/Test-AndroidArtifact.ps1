[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(1,9999)][int]$BuildNumber)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root "Builds\Android\G1-0.0.$BuildNumber"
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
if ($text -notmatch "(?m)^sdkVersion:'26'" -or $text -notmatch "(?m)^targetSdkVersion:'36'") { throw 'Unexpected minimum or target SDK.' }
if ($text -notmatch "(?m)^native-code: 'arm64-v8a'\s*$") { throw 'APK must contain only the intended ARM64 ABI.' }
if ($text -match '(?m)^application-debuggable') { throw 'APK is debuggable; expected non-development player.' }
$previousJava=$env:JAVA_HOME
try {
    $env:JAVA_HOME=Join-Path $toolchain.root 'OpenJDK'
    $signing=@(& (Join-Path $buildTools 'apksigner.bat') verify --verbose --print-certs $apk 2>&1)
    if ($LASTEXITCODE -ne 0) { throw 'APK signature verification failed.' }
} finally { $env:JAVA_HOME=$previousJava }
$signingText=$signing -join "`n"
if ($signingText -notmatch 'Signer #1 certificate DN:.*CN=Android Debug') { throw 'Unexpected certificate for this explicitly debug-signed build-only probe.' }
if ($signingText -notmatch 'Signer #1 certificate SHA-256 digest: ([0-9a-fA-F]+)') { throw 'No signing fingerprint returned.' }
$certificate=$Matches[1].ToLowerInvariant()
$dir=Join-Path $root "LocalData\Verification\android-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $dir | Out-Null
$badging | Set-Content -LiteralPath (Join-Path $dir 'aapt2-badging.txt') -Encoding utf8
$signing | Set-Content -LiteralPath (Join-Path $dir 'apksigner.txt') -Encoding utf8
[ordered]@{
    passed=$true;utc=[DateTime]::UtcNow.ToString('O');version=$summary.version;package='com.littleweeps.familyplayset'
    minSdk=26;targetSdk=36;abi='arm64-v8a';debuggable=$false;certificateSha256=$certificate
    signing='Android Debug certificate; build-only probe';familyReleaseQualified=$false;deviceInstalled=$false
    apkSha256=(Get-FileHash -LiteralPath $apk -Algorithm SHA256).Hash.ToLowerInvariant()
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'apk-inspection.json') -Encoding utf8
Write-Host "PASS: APK identity, ARM64, SDK levels and signature inspection. No device or family-release qualification. Evidence: $dir"
