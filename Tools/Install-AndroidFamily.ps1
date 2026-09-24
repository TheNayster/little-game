[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(1,9999)][int]$BuildNumber)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$device=Get-Content -LiteralPath (Join-Path $root 'LocalData\android-device.json') -Raw | ConvertFrom-Json
$pin=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'android-family-signing.json') -Raw | ConvertFrom-Json
$toolchain=Get-Content -LiteralPath (Join-Path $root 'LocalData\android-toolchain.json') -Raw | ConvertFrom-Json
$folder=Join-Path $root "Builds\AndroidSigned\G1-0.0.$BuildNumber"
$apk=Join-Path $folder 'LittleWeeps.apk'
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if ($summary.result -ne 'Succeeded' -or $summary.platform -ne 'Android' -or $summary.development -or $summary.version -ne "0.0.$BuildNumber") { throw 'Expected a matching successful non-development Android build.' }
$hash=(Get-FileHash -LiteralPath $apk -Algorithm SHA256).Hash.ToLowerInvariant()
$manifest=@(Get-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Raw | ConvertFrom-Json)
if (@($manifest | Where-Object { $_.path -eq 'LittleWeeps.apk' -and $_.sha256 -eq $hash }).Count -ne 1) { throw 'Signed artifact does not match its manifest.' }
$adb=$device.adb
$serial=$device.endpoint
# Unity may restart ADB while building. Reuse the existing pairing and endpoint.
& $adb connect $serial | Out-Host
function Invoke-Phone([string[]]$Arguments) {
    $value=@(& $adb -s $serial @Arguments 2>&1)
    if ($LASTEXITCODE -ne 0) { throw "ADB failed: $($value -join ' ')" }
    return ($value -join "`n").Trim()
}
if ((Invoke-Phone @('get-state')) -ne 'device') { throw 'Recorded phone is disconnected or unpaired.' }
if ((Invoke-Phone @('shell','getprop','ro.product.model')) -ne $device.model) { throw 'Connected phone is not the recorded device.' }
$user=Invoke-Phone @('shell','am','get-current-user')
if ($user -ne '0') { throw 'This foundation installer is limited to the verified main Android user 0.' }
$pageBytes=Invoke-Phone @('shell','getconf','PAGE_SIZE')
if ($pageBytes -ne '4096') { throw 'This physical probe is qualified for the observed 4 KB setup only; the separate 16 KB gate remains open.' }
$dir=Join-Path $root ('LocalData\Verification\android-phone-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $dir | Out-Null
$buildTools=Join-Path $toolchain.root 'SDK\build-tools\36.0.0'
$previousJava=$env:JAVA_HOME
try {
    $env:JAVA_HOME=Join-Path $toolchain.root 'OpenJDK'
    function Confirm-Certificate([string]$Path) {
        $output=@(& (Join-Path $buildTools 'apksigner.bat') verify --verbose --print-certs $Path 2>&1)
        if ($LASTEXITCODE -ne 0) { throw 'APK signature verification failed.' }
        $text=$output -join "`n"
        if ($text -notmatch 'Signer #1 certificate SHA-256 digest: ([0-9a-fA-F]+)' -or $Matches[1].ToLowerInvariant() -ne $pin.certificateSha256 -or $text -notmatch '(?m)^Number of signers: 1\s*$') { throw 'Signature mismatch. Stop; do not uninstall or clear data.' }
    }
    function Read-Badging([string]$Path) {
        $badging=@(& (Join-Path $buildTools 'aapt2.exe') dump badging $Path 2>&1)
        if ($LASTEXITCODE -ne 0) { throw 'APK identity inspection failed.' }
        return ($badging -join "`n")
    }
    Confirm-Certificate $apk
    $badging=Read-Badging $apk
    if ($badging -notmatch "package: name='com\.littleweeps\.familyplayset' versionCode='$BuildNumber' versionName='0\.0\.$BuildNumber'" -or $badging -match '(?m)^application-debuggable' -or $badging -notmatch "(?m)^minSdkVersion:'26'" -or $badging -notmatch "(?m)^targetSdkVersion:'36'" -or $badging -notmatch "(?m)^native-code: 'arm64-v8a'\s*$") { throw 'Unexpected package/version/debug/SDK/ABI configuration.' }
    $installed=Invoke-Phone @('shell','pm','list','packages','--user','0',$pin.package)
    $oldHash=$null;$oldVersion=$null
    if ($installed -and $installed -ne "package:$($pin.package)") { throw 'Unexpected package inventory; refusing to guess.' }
    function Pull-Installed([string]$Destination) {
        $path=Invoke-Phone @('shell','pm','path','--user','0',$pin.package)
        if ($path -notmatch '^package:(/data/app/[^\r\n]+/base\.apk)$') { throw 'Expected one installed base APK.' }
        [void](Invoke-Phone @('pull',$Matches[1],$Destination))
    }
    if ($installed) {
        $oldApk=Join-Path $dir 'before.apk'
        Pull-Installed $oldApk
        Confirm-Certificate $oldApk
        $oldBadging=Read-Badging $oldApk
        if ($oldBadging -notmatch "package: name='com\.littleweeps\.familyplayset' versionCode='(\d+)'") { throw 'Unexpected installed app identity.' }
        $oldVersion=[int]$Matches[1]
        if ($oldVersion -gt $BuildNumber) { throw 'Downgrades are not allowed.' }
        $oldHash=(Get-FileHash -LiteralPath $oldApk -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    $operation=if ($oldHash -eq $hash) { 'already current' } elseif ($installed) { 'updated' } else { 'installed' }
    if ($operation -ne 'already current') {
        $result=Invoke-Phone @('install','-r','--user','0',$apk)
        if ($result -notmatch '(?m)^Success\s*$') { throw "Installation did not report success: $result" }
    }
    $actual=Join-Path $dir 'installed.apk'
    Pull-Installed $actual
    if ((Get-FileHash -LiteralPath $actual -Algorithm SHA256).Hash.ToLowerInvariant() -ne $hash) { throw 'Installed APK differs from the intended artifact.' }
    Confirm-Certificate $actual
    $launch=Invoke-Phone @('shell','am','start','-W','--user','0','-n',($pin.package+'/com.unity3d.player.UnityPlayerGameActivity'))
    if ($launch -notmatch '(?m)^Status: ok\s*$') { throw "Launch failed: $launch" }
    $appPid=Invoke-Phone @('shell','pidof',$pin.package)
    [ordered]@{utc=[DateTime]::UtcNow.ToString('O');operation=$operation;version=$summary.version;package=$pin.package;apkSha256=$hash;certificateSha256=$pin.certificateSha256;previousBuild=$oldVersion;previousApkSha256=$oldHash;installedApkHashMatched=$true;deviceModel=$device.model;pageBytes=4096;activeUser=0;launchCommandSucceeded=$true;pid=$appPid;visibleUiAndSaveRetentionVerified=$false;native16KbQualified=$false} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'installation.json') -Encoding utf8
    Write-Host "App $operation and exact installed artifact verified. UI/save checks remain required. Evidence: $dir"
} finally { $env:JAVA_HOME=$previousJava }
