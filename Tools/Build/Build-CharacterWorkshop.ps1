[CmdletBinding()]
param([Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]+$')][string]$RunId)
$toolsRoot=Split-Path -Parent $PSScriptRoot
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $toolsRoot
$project=Join-Path $projectRoot 'Unity\FamilyPlayset'
if (@(Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object { $_.CommandLine -and $_.CommandLine.Contains($project) }).Count) {
    throw 'Save and close this Unity project before its batch build.'
}
$version=((Get-Content (Join-Path $project 'ProjectSettings\ProjectVersion.txt') | Where-Object { $_ -match '^m_EditorVersion:' }) -split ':',2)[1].Trim()
$editor=Join-Path $env:ProgramFiles "Unity\Hub\Editor\$version\Editor\Unity.exe"
$folder=Join-Path $projectRoot "Builds\CharacterWorkshop\$RunId"
if(Test-Path -LiteralPath $folder){throw 'Use a fresh run ID.'}
$logFolder=Join-Path $projectRoot 'LocalData\Logs'
New-Item -ItemType Directory -Path $logFolder -Force | Out-Null
$log=Join-Path $logFolder "character-workshop-$RunId.log"
foreach($characterId in @('bluey','bingo')){
$art=Join-Path $project "Assets\FamilyPlayset\Art\Characters\$characterId"
$manifest=Get-Content -LiteralPath (Join-Path $art 'import-manifest.json') -Raw | ConvertFrom-Json
if((Get-FileHash -LiteralPath (Join-Path $projectRoot $manifest.source)).Hash.ToLowerInvariant() -ne $manifest.sourceSha256){throw 'Character source changed; re-export sprites.'}
if((Get-FileHash -LiteralPath (Join-Path $projectRoot $manifest.contract)).Hash.ToLowerInvariant() -ne $manifest.contractSha256){throw 'Character contract changed; re-export sprites.'}
foreach($layer in $manifest.layers){
    if((Get-FileHash -LiteralPath (Join-Path $art ($layer.name+'.png'))).Hash.ToLowerInvariant() -ne $layer.sha256){throw "Sprite changed: $($layer.name)"}
}
}
$settingsPaths=@('ProjectSettings\ProjectSettings.asset','ProjectSettings\Packages\com.unity.dedicated-server\MultiplayerRolesSettings.asset')
$settingsBefore=@{}
foreach($relative in $settingsPaths){$settingsBefore[$relative]=[IO.File]::ReadAllBytes((Join-Path $project $relative))}
$arguments=@('-batchmode','-quit','-projectPath',('"'+$project+'"'),'-buildTarget','StandaloneWindows64','-standaloneBuildSubtarget','Player',
    '-executeMethod','LittleWeeps.EditorTools.CharacterWorkshopBuild.Windows','-characterOutput',('"'+$folder+'"'),'-logFile',('"'+$log+'"'))
$process=Start-Process -FilePath $editor -ArgumentList $arguments -WindowStyle Hidden -PassThru
Write-Host "Building the isolated character workshop. Log: $log"
$process.WaitForExit()
if($process.ExitCode -ne 0){throw "Unity build failed: $log"}
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if($summary.result -ne 'Succeeded' -or $summary.development -or $summary.project -ne $project){throw 'Unexpected workshop build result.'}
# Unity rewrites empty YAML values with trailing spaces on import. Only restore
# the saved bytes when all non-whitespace content is identical; never mask a
# changed setting or overwrite a concurrent semantic edit.
foreach($relative in $settingsPaths){
    $path=Join-Path $project $relative
    $before=[Text.Encoding]::UTF8.GetString($settingsBefore[$relative]).Replace("`r`n","`n") -replace '(?m)[ \t]+$',''
    $after=[IO.File]::ReadAllText($path).Replace("`r`n","`n") -replace '(?m)[ \t]+$',''
    if($before -cne $after){throw "Family settings changed: $relative"}
    [IO.File]::WriteAllBytes($path,$settingsBefore[$relative])
}
$sources=@(& git -C $projectRoot ls-files --cached --others --exclude-standard -- SourceArt/Characters Unity/FamilyPlayset/Assets Unity/FamilyPlayset/Packages Unity/FamilyPlayset/ProjectSettings Tools/Content/Export-CharacterStudy.py Tools/Build/Build-CharacterWorkshop.ps1 | Sort-Object -Unique | ForEach-Object {
    $path=Join-Path $projectRoot $_
    if(Test-Path -LiteralPath $path -PathType Leaf){[ordered]@{path=$_;sha256=(Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant()}}
})
[ordered]@{sourceCommit=(& git -C $projectRoot rev-parse HEAD).Trim();files=$sources;familySettingsUnchanged=$true} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $folder 'source-manifest.json') -Encoding utf8
Write-Host "Built: $folder"
