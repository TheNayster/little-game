[CmdletBinding()]
param([Parameter(Mandatory)][int]$FirstBuild, [Parameter(Mandatory)][int]$UpdatedBuild)
$toolsRoot=Split-Path -Parent $PSScriptRoot
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $toolsRoot
if ($FirstBuild -eq $UpdatedBuild) { throw 'Use two distinct builds for the update check.' }
$runId=[Guid]::NewGuid().ToString('N')
$evidence=Join-Path $env:USERPROFILE "AppData\LocalLow\Little Weeps\Little Weeps\Verification\$runId"
$localEvidence=Join-Path $root "LocalData\Verification\$runId"
New-Item -ItemType Directory -Path $localEvidence -Force | Out-Null
foreach ($step in @(@{mode='seed';build=$FirstBuild},@{mode='resume';build=$FirstBuild},@{mode='update';build=$UpdatedBuild})) {
    $folder=Join-Path $root "Builds\Windows\G1-0.0.$($step.build)"
    $summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
    if ($summary.result -ne 'Succeeded' -or $summary.development -or $summary.version -ne "0.0.$($step.build)") { throw 'A matching non-development build is required.' }
    foreach ($file in (Get-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Raw | ConvertFrom-Json)) {
        $actual=(Get-FileHash -LiteralPath (Join-Path $folder $file.path) -Algorithm SHA256).Hash
        if ($actual -ne $file.sha256) { throw "Artifact changed after build: $($file.path)" }
    }
    $log=Join-Path $localEvidence "$($step.mode).log"
    $exe=Join-Path $folder 'LittleWeeps.exe'
    $argsList=@('-screen-fullscreen','0','-screen-width','1200','-screen-height','800','-foundationVerify',$step.mode,'-foundationRun',$runId,'-logFile',('"'+$log+'"'))
    Write-Host "Verifying $($step.mode) on build 0.0.$($step.build)"
    $player=Start-Process -FilePath $exe -ArgumentList $argsList -WindowStyle Hidden -PassThru
    if (!$player.WaitForExit(90000)) {
        # Only terminate the test process created by this script; never another app.
        $player.Kill()
        throw "Verification timed out. Inspect $log"
    }
    if ($player.ExitCode -ne 0) { throw "Player verification failed (exit $($player.ExitCode)). Inspect $log and $evidence" }
    $record=Get-Content -LiteralPath (Join-Path $evidence "$($step.mode).json") -Raw | ConvertFrom-Json
    if (!$record.passed -or $record.runId -ne $runId -or $record.build -ne "0.0.$($step.build)") { throw 'Player evidence did not match the requested run.' }
}
Get-ChildItem -LiteralPath $evidence -File | Copy-Item -Destination $localEvidence
Write-Host "PASS: native save, restart, update and local-video checks. Evidence: $localEvidence"
