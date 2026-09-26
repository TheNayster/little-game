[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(46,9999)][int]$BuildNumber)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$project=Join-Path $root 'Unity\FamilyPlayset'
if(@(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" | Where-Object {$_.CommandLine -and $_.CommandLine.Contains($project)}).Count){throw 'Close this project before a batch build.'}
$version=((Get-Content (Join-Path $project 'ProjectSettings\ProjectVersion.txt') | Where-Object {$_ -match '^m_EditorVersion:'}) -split ':',2)[1].Trim()
$editor=Join-Path $env:ProgramFiles "Unity\Hub\Editor\$version\Editor\Unity.exe"
$folder=Join-Path $root "Builds\NetworkProbe\G3-0.0.$BuildNumber"
if(Test-Path -LiteralPath $folder){throw 'Use a fresh build number.'}
$log=Join-Path $root "LocalData\Logs\build-network-$BuildNumber.log"
$argsList=@('-batchmode','-quit','-projectPath',('"'+$project+'"'),'-buildTarget','StandaloneWindows64','-executeMethod','LittleWeeps.EditorTools.NetworkProbeBuild.Windows','-familyBuildNumber',"$BuildNumber",'-logFile',('"'+$log+'"'))
Write-Output "Building Windows server/client $BuildNumber. Log: $log"
$process=Start-Process -FilePath $editor -ArgumentList $argsList -WindowStyle Hidden -PassThru
$process.WaitForExit()
if($process.ExitCode){throw "Network build failed. See $log"}
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if($summary.contract -notin @(1,2,3,4,5,6,7,8,9,10) -or @($summary.builds).Count -ne 2 -or @($summary.builds | Where-Object {$_.result -ne 'Succeeded' -or $_.version -ne "0.0.$BuildNumber"}).Count){throw 'Network build summary mismatch.'}
$files=@(& git -C $root ls-files --cached --others --exclude-standard -- Unity/FamilyPlayset/Assets Unity/FamilyPlayset/Packages Unity/FamilyPlayset/ProjectSettings Tools | Sort-Object -Unique | ForEach-Object {
    $path=Join-Path $root $_
    if(Test-Path -LiteralPath $path -PathType Leaf){[ordered]@{path=$_;sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()}}
})
[ordered]@{sourceCommit=(& git -C $root rev-parse HEAD).Trim();dirtyPaths=@(& git -C $root status --porcelain -- Unity Tools);files=$files} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $folder 'source-manifest.json') -Encoding utf8
@(Get-ChildItem -LiteralPath $folder -Recurse -File | Where-Object {$_.Name -notin @('build-summary.json','source-manifest.json','artifact-manifest.json')} | ForEach-Object {
    [ordered]@{path=$_.FullName.Substring($folder.Length+1).Replace('\','/');sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()}
}) | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Encoding utf8
Write-Output "Built: $folder"
