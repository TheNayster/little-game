[CmdletBinding()]
param([ValidateSet('Windows','iOS')][string]$Target='Windows', [ValidateRange(1,9999)][int]$BuildNumber=1)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$project=Join-Path $root 'Unity\FamilyPlayset'
$versionLine=Get-Content -LiteralPath (Join-Path $project 'ProjectSettings\ProjectVersion.txt') | Where-Object { $_ -match '^m_EditorVersion:' }
$version=($versionLine -split ':',2)[1].Trim()
$editor=Join-Path $env:ProgramFiles "Unity\Hub\Editor\$version\Editor\Unity.exe"
$open=@(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" | Where-Object { $_.CommandLine -and $_.CommandLine.Contains($project) })
if ($open.Count) { throw 'Save and close this Unity project before running a batch build.' }
$platform=if ($Target -eq 'iOS') { 'iOS' } else { 'StandaloneWindows64' }
$method=if ($Target -eq 'iOS') { 'IOS' } else { 'Windows' }
$logs=Join-Path $root 'LocalData\Logs'
New-Item -ItemType Directory -Path $logs -Force | Out-Null
$log=Join-Path $logs ("build-$Target-$BuildNumber-"+[DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')+'.log')
$argsList=@('-batchmode','-quit','-projectPath',('"'+$project+'"'),'-buildTarget',$platform,'-executeMethod',"LittleWeeps.EditorTools.FoundationBuild.$method",'-familyBuildNumber',"$BuildNumber",'-logFile',('"'+$log+'"'))
Write-Host "Building $Target 0.0.$BuildNumber. Log: $log"
$process=Start-Process -FilePath $editor -ArgumentList $argsList -WindowStyle Hidden -PassThru -Wait
if ($process.ExitCode -ne 0) { throw "Unity build failed (exit $($process.ExitCode)). See $log" }
Write-Host "Build completed. Evidence: $root\Builds\$Target\G1-0.0.$BuildNumber\build-summary.json"
