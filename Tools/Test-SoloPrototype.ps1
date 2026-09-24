[CmdletBinding()]
param([Parameter(Mandatory)][int]$BuildNumber,[switch]$InputOnly,[int]$UpdatedBuildNumber=0,
      [ValidateRange(640,2560)][int]$Width=1280,[ValidateRange(480,1440)][int]$Height=800)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if($UpdatedBuildNumber -and ($InputOnly -or $UpdatedBuildNumber -eq $BuildNumber)){throw 'Use a distinct updated build for the save/update suite.'}
foreach($number in @($BuildNumber,$UpdatedBuildNumber) | Where-Object {$_ -gt 0} | Select-Object -Unique){
    $folder=Join-Path $root "Builds\WindowsSolo\G2-0.0.$number"
    $summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
    if($summary.result -ne 'Succeeded' -or $summary.development -or $summary.version -ne "0.0.$number"){throw 'Matching release-configured prototype required.'}
    foreach($file in (Get-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Raw | ConvertFrom-Json)){
        if((Get-FileHash -LiteralPath (Join-Path $folder $file.path) -Algorithm SHA256).Hash -ne $file.sha256){throw "Artifact changed: $($file.path)"}
    }
}
$run=[Guid]::NewGuid().ToString('N')
$data=Join-Path $env:USERPROFILE "AppData\LocalLow\Little Weeps\Little Weeps\SoloPrototype\$run"
$evidence=Join-Path $root "LocalData\Verification\solo-$run"
New-Item -ItemType Directory -Path $evidence | Out-Null
$modes=if($InputOnly){@('input')}else{@('seed','resume','recover')}
foreach($mode in $modes){
    $currentBuild=if($mode -ne 'seed' -and $UpdatedBuildNumber){$UpdatedBuildNumber}else{$BuildNumber}
    $folder=Join-Path $root "Builds\WindowsSolo\G2-0.0.$currentBuild"
    if($mode -eq 'recover'){
        # Fault injection only in this script's fresh, explicitly identified test namespace.
        $save=Join-Path $data 'world.save'
        if(!(Test-Path -LiteralPath ($save+'.bak'))){throw 'No backup for recovery test.'}
        Copy-Item -LiteralPath $save -Destination (Join-Path $evidence 'before-corruption.save')
        [IO.File]::WriteAllText($save,'interrupted test write')
    }
    $log=Join-Path $evidence "$mode.log"
    $arguments=@('-screen-fullscreen','0','-screen-width',"$Width",'-screen-height',"$Height",'-soloVerify',$mode,'-soloRun',$run,'-logFile',('"'+$log+'"'))
    Write-Host "Solo $mode on build $currentBuild; run $run"
    $player=Start-Process -FilePath (Join-Path $folder 'LittleWeepsSolo.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if(!$player.WaitForExit(90000)){$player.Kill();throw "Prototype test timed out: $log"}
    Get-ChildItem -LiteralPath $data -File | Where-Object {$_.Extension -in @('.json','.png')} | Copy-Item -Destination $evidence
    if($player.ExitCode -ne 0){throw "Prototype failed; see $evidence"}
    $result=Get-Content -LiteralPath (Join-Path $evidence "$mode.json") -Raw | ConvertFrom-Json
    if(!$result.passed -or $result.runId -ne $run -or $result.build -ne "0.0.$currentBuild"){throw 'Mismatched prototype evidence.'}
    if($result.PSObject.Properties.Name -contains 'screenWidth'){
        if($result.screenWidth -ne $Width -or $result.screenHeight -ne $Height -or !$result.layoutBounds){throw 'Requested viewport or layout checks did not pass.'}
    }
}
if($InputOnly){Write-Host "PASS: full Input System mouse/touch integration. Evidence: $evidence"}
else{Write-Host "PASS: handlers, interactions, saved play and corrupted-primary recovery. Evidence: $evidence"}
