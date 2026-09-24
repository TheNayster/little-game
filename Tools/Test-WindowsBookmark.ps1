[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(1,9999)][int]$BuildNumber)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root "Builds\Windows\G1-0.0.$BuildNumber"
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if ($summary.result -ne 'Succeeded' -or $summary.development -or $summary.version -ne "0.0.$BuildNumber" -or $summary.platform -ne 'Windows') { throw 'Matching non-development Windows build required.' }
foreach ($file in (Get-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Raw | ConvertFrom-Json)) {
    if ((Get-FileHash -LiteralPath (Join-Path $folder $file.path) -Algorithm SHA256).Hash -ne $file.sha256) { throw "Artifact changed: $($file.path)" }
}
$runId=[Guid]::NewGuid().ToString('N')
$evidence=Join-Path $env:USERPROFILE "AppData\LocalLow\Little Weeps\Little Weeps\Verification\$runId"
$localEvidence=Join-Path $root "LocalData\Verification\$runId"
New-Item -ItemType Directory -Path $localEvidence -Force | Out-Null
foreach ($mode in @('bookmark','bookmark-resume')) {
    $log=Join-Path $localEvidence "$mode.log"
    $argsList=@('-screen-fullscreen','0','-screen-width','1200','-screen-height','800','-foundationVerify',$mode,'-foundationRun',$runId,'-logFile',('"'+$log+'"'))
    Write-Host "Checking $mode on 0.0.$BuildNumber; run $runId"
    $player=Start-Process -FilePath (Join-Path $folder 'LittleWeeps.exe') -ArgumentList $argsList -WindowStyle Hidden -PassThru
    if (!$player.WaitForExit(120000)) { $player.Kill(); throw "Test process timed out; see $log" }
    $recordPath=Join-Path $evidence "$mode.json"
    if (Test-Path -LiteralPath $recordPath) { Copy-Item -LiteralPath $recordPath -Destination $localEvidence }
    if ($player.ExitCode -ne 0) { throw "Bookmark regression failed (exit $($player.ExitCode)); evidence: $localEvidence" }
    $record=Get-Content -LiteralPath $recordPath -Raw | ConvertFrom-Json
    if (!$record.passed -or $record.runId -ne $runId -or $record.build -ne "0.0.$BuildNumber" -or $record.reopenCycles -ne 30) { throw 'Bookmark evidence is missing or mismatched.' }
}
Write-Host "PASS: 60 paused reopenings and a process relaunch. Evidence: $localEvidence"
