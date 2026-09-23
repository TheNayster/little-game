[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$version='6000.3.24f1'
$projectVersion=Get-Content -LiteralPath (Join-Path $root 'Unity\FamilyPlayset\ProjectSettings\ProjectVersion.txt')
if ($projectVersion -notcontains "m_EditorVersion: $version") { throw 'This installer is pinned to Unity 6000.3.24f1. Review it before changing editor versions.' }
$editorRoot=Join-Path $env:ProgramFiles "Unity\Hub\Editor\$version"
$editor=Join-Path $editorRoot 'Editor\Unity.exe'
$module=Join-Path $editorRoot 'Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_server_nondevelopment_mono\UnityPlayer.dll'
if (Test-Path -LiteralPath $module) { Write-Host 'The matching Windows Dedicated Server module is already present. Its build/run checks are separate.'; exit 0 }
if (@(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" | Where-Object { $_.ExecutablePath -and $_.ExecutablePath -eq $editor }).Count) {
    throw 'Save and close Unity 6000.3.24f1 before installing its module.'
}
$installer=Join-Path $env:APPDATA "UnityHub\downloads\UnitySetup-Windows-Server-Support-for-Editor-$version.exe"
if (!(Test-Path -LiteralPath $installer)) { throw 'The verified cached server installer is missing.' }
$hash=(Get-FileHash -LiteralPath $installer -Algorithm MD5).Hash
$signature=Get-AuthenticodeSignature -LiteralPath $installer
if ($hash -ne '92F0C663173692F7F1CE0CDF8031FFD0' -or $signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'Unity Technologies SF') {
    throw 'Installer checksum or Unity signature verification failed.'
}
Write-Host 'Windows will ask to allow the Unity installer. Click Yes to install the server module.'
$process=Start-Process -FilePath $installer -ArgumentList ("/S /D="+$editorRoot) -Verb RunAs -WindowStyle Hidden -PassThru -ErrorAction Stop
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw "Unity module installer failed: $($process.ExitCode)" }
if (!(Test-Path -LiteralPath $module)) { throw 'Installer exited, but the expected server module is still missing.' }
$record=Join-Path $root 'LocalData\server-module-install.json'
[ordered]@{utc=[DateTime]::UtcNow.ToString('O');unity=$version;installerMd5=$hash;signature='Valid';exitCode=$process.ExitCode;modulePresent=$true;nativeServerBuildTested=$false} |
    ConvertTo-Json | Set-Content -LiteralPath $record -Encoding utf8
Write-Host 'Windows Dedicated Server module installed. The server build and lifecycle check can now run.'
