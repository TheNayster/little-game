[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(75,9999)][int]$BuildNumber)
$toolsRoot=Split-Path -Parent $PSScriptRoot
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $toolsRoot
$project=Join-Path $root 'Unity\FamilyPlayset'
if(@(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" | Where-Object {$_.CommandLine -and $_.CommandLine.Contains($project)}).Count){throw 'Close this project before a batch build.'}
$version=((Get-Content (Join-Path $project 'ProjectSettings\ProjectVersion.txt') | Where-Object {$_ -match '^m_EditorVersion:'}) -split ':',2)[1].Trim()
$editor=Join-Path $env:ProgramFiles "Unity\Hub\Editor\$version\Editor\Unity.exe"
$folder=Join-Path $root "Builds\AndroidFamilyLAN\G3-0.0.$BuildNumber"
if(Test-Path -LiteralPath $folder){throw 'Use a fresh build number.'}
$toolchain=Get-Content -LiteralPath (Join-Path $root 'LocalData\android-toolchain.json') -Raw | ConvertFrom-Json
if($toolchain.unity -ne $version){throw 'Android tools/editor mismatch.'}
$log=Join-Path $root "LocalData\Logs\build-android-lan-$BuildNumber.log"
$arguments=@('-batchmode','-quit','-projectPath',('"'+$project+'"'),'-buildTarget','Android','-executeMethod','LittleWeeps.EditorTools.AndroidFamilyBuild.Build','-familyBuildNumber',"$BuildNumber",'-familyAndroidToolchain',('"'+$toolchain.root+'"'),'-logFile',('"'+$log+'"'))
Write-Output "Building Android family client $BuildNumber. Log: $log"
$process=Start-Process -FilePath $editor -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.WaitForExit()
if($process.ExitCode){throw "Android build failed. See $log"}
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if($summary.result -ne 'Succeeded' -or $summary.version -ne "0.0.$BuildNumber" -or $summary.development){throw 'Unexpected Android build result.'}
$files=@(& git -C $root ls-files --cached --others --exclude-standard -- Unity/FamilyPlayset/Assets Unity/FamilyPlayset/Packages Unity/FamilyPlayset/ProjectSettings Tools | Sort-Object -Unique | ForEach-Object {
    $path=Join-Path $root $_
    if(Test-Path -LiteralPath $path -PathType Leaf){[ordered]@{path=$_;sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()}}
})
[ordered]@{sourceCommit=(& git -C $root rev-parse HEAD).Trim();dirtyPaths=@(& git -C $root status --porcelain -- Unity Tools);files=$files} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $folder 'source-manifest.json') -Encoding utf8
@([ordered]@{path='LittleWeeps.apk';sha256=(Get-FileHash -LiteralPath (Join-Path $folder 'LittleWeeps.apk') -Algorithm SHA256).Hash.ToLowerInvariant()}) | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Encoding utf8
# Reuse the established signing/payload-verification route, with this exact fresh input.
& (Join-Path $toolsRoot 'Build\Build-AndroidFamily.ps1') -BuildNumber $BuildNumber -FamilyLan
