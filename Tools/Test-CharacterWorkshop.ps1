[CmdletBinding()]
param([Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]+$')][string]$RunId)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $projectRoot "Builds\CharacterWorkshop\$RunId"
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if($summary.result -ne 'Succeeded' -or $summary.development){throw 'A successful release workshop build is required.'}
$manifest=Get-Content -LiteralPath (Join-Path $folder 'source-manifest.json') -Raw | ConvertFrom-Json
foreach($entry in $manifest.files){
    if((Get-FileHash -LiteralPath (Join-Path $projectRoot $entry.path)).Hash.ToLowerInvariant() -ne $entry.sha256){throw "Source changed after build: $($entry.path)"}
}
$evidence=Join-Path $projectRoot "LocalData\CharacterWorkshop\$RunId"
if(Test-Path -LiteralPath $evidence){throw 'Evidence already exists for this run ID.'}
New-Item -ItemType Directory -Path $evidence | Out-Null
$exe=Join-Path $folder 'LittleWeepsCharacterWorkshop.exe'
$arguments=@('-screen-width','1280','-screen-height','800','-screen-fullscreen','0','-characterEvidence',('"'+$evidence+'"'),'-logFile',('"'+(Join-Path $evidence 'player.log')+'"'))
$process=Start-Process -FilePath $exe -ArgumentList $arguments -WindowStyle Hidden -PassThru
if(-not $process.WaitForExit(45000)){Stop-Process -Id $process.Id;throw 'Workshop verification timed out.'}
$result=Get-Content -LiteralPath (Join-Path $evidence 'runtime-checks.json') -Raw | ConvertFrom-Json
if($process.ExitCode -ne 0 -or -not $result.passed){throw 'Native character verification failed; inspect its evidence.'}
foreach($id in @('bluey','bingo')){foreach($name in @('idle','walk','wave','carry','carry-left')){if(-not (Test-Path -LiteralPath (Join-Path $evidence ($id+'-'+$name+'.png')))){throw "Missing render: $id $name"}}}
$assembly=Join-Path $folder 'LittleWeepsCharacterWorkshop_Data\Managed\LittleWeeps.Client.dll'
[ordered]@{runId=$RunId;exe=$exe;exeSha256=(Get-FileHash -LiteralPath $exe).Hash;assembly=$assembly;assemblySha256=(Get-FileHash -LiteralPath $assembly).Hash;evidence=$evidence} |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $projectRoot 'LocalData\CharacterWorkshop\current.json') -Encoding utf8
Write-Host ($result | ConvertTo-Json -Depth 4)
Write-Host "Inspect renders: $evidence"
