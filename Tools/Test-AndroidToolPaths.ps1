[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$project=Join-Path $root 'Unity\FamilyPlayset'
$versionLine=Get-Content -LiteralPath (Join-Path $project 'ProjectSettings\ProjectVersion.txt') | Where-Object { $_ -match '^m_EditorVersion:' }
$version=($versionLine -split ':',2)[1].Trim()
$editor=Join-Path $env:ProgramFiles "Unity\Hub\Editor\$version\Editor\Unity.exe"
$toolchain=Get-Content -LiteralPath (Join-Path $root 'LocalData\android-toolchain.json') -Raw | ConvertFrom-Json
if ($toolchain.unity -ne $version) { throw 'Toolchain/editor mismatch.' }
if (@(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" | Where-Object { $_.CommandLine -and $_.CommandLine.Contains($project) }).Count) {
    throw 'Save and close this Unity project before the integration check.'
}
$dir=Join-Path $root ('LocalData\Verification\android-tools-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $dir | Out-Null
$log=Join-Path $dir 'unity.log'
$argsList=@('-batchmode','-quit','-projectPath',('"'+$project+'"'),'-buildTarget','Android','-executeMethod','LittleWeeps.EditorTools.AndroidFoundationTools.VerifyRestoration','-familyAndroidToolchain',('"'+$toolchain.root+'"'),'-logFile',('"'+$log+'"'))
$process=Start-Process -FilePath $editor -ArgumentList $argsList -WindowStyle Hidden -PassThru
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw "Tool path integration check failed. Inspect $log" }
if (!(Select-String -LiteralPath $log -SimpleMatch 'LITTLE_WEEPS_ANDROID_TOOLS_PASS: bundled defaults and custom paths restored.' -Quiet)) {
    throw 'Missing tool path restoration evidence.'
}
[ordered]@{passed=$true;utc=[DateTime]::UtcNow.ToString('O');unity=$version;bundledDefaultsRestored=$true;customPathsRestored=$true;unityExit=$process.ExitCode} |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $dir 'tool-paths.json') -Encoding utf8
Write-Host "PASS: Android bundled/default and custom tool path restoration. Evidence: $dir"
