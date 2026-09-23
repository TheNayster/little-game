[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(1,9999)][int]$BuildNumber)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root "Builds\WindowsServer\G1-0.0.$BuildNumber"
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if ($summary.result -ne 'Succeeded' -or $summary.development -or !$summary.dedicatedServer -or $summary.platform -ne 'WindowsServer' -or $summary.version -ne "0.0.$BuildNumber") { throw 'A matching dedicated-server build is required.' }
foreach ($file in (Get-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Raw | ConvertFrom-Json)) {
    if ((Get-FileHash -LiteralPath (Join-Path $folder $file.path) -Algorithm SHA256).Hash -ne $file.sha256) { throw "Artifact changed: $($file.path)" }
}
$runId=[Guid]::NewGuid().ToString('N')
$evidence=Join-Path $root "LocalData\Verification\server-$runId"
New-Item -ItemType Directory -Path $evidence | Out-Null
$log=Join-Path $evidence 'server.log'
$argsList=@('-batchmode','-nographics','-serverEvidence',('"'+$evidence+'"'),'-serverRun',$runId,'-logFile',('"'+$log+'"'))
$process=Start-Process -FilePath (Join-Path $folder 'LittleWeepsServer.exe') -ArgumentList $argsList -WindowStyle Hidden -PassThru
try {
    $deadline=[DateTime]::UtcNow.AddSeconds(45)
    $heartbeatPath=Join-Path $evidence 'heartbeat.json'
    do {
        if ($process.HasExited) { throw "Server exited before verification. Inspect $log" }
        if ([DateTime]::UtcNow -gt $deadline) { throw "Server heartbeat timed out. Inspect $log" }
        Start-Sleep -Milliseconds 250
        $heartbeat=if(Test-Path -LiteralPath $heartbeatPath){ Get-Content -LiteralPath $heartbeatPath -Raw | ConvertFrom-Json } else { $null }
    } until ($heartbeat -and $heartbeat.heartbeats -ge 3)
    # Cooperative shutdown request scoped to the unique process evidence directory.
    New-Item -ItemType File -Path (Join-Path $evidence 'stop.request') | Out-Null
    if (!$process.WaitForExit(15000)) { throw 'Server did not shut down cleanly.' }
    if ($process.ExitCode -ne 0) { throw "Server exited with code $($process.ExitCode)" }
    foreach ($phase in @('ready','heartbeat','stopped')) {
        $record=Get-Content -LiteralPath (Join-Path $evidence "$phase.json") -Raw | ConvertFrom-Json
        if ($record.runId -ne $runId -or $record.phase -ne $phase -or $record.build -ne "0.0.$BuildNumber" -or !$record.dedicatedServer -or !$record.batchMode -or $record.graphics -ne 'Null' -or $record.cameras -ne 0 -or $record.networkingImplemented) { throw "Unexpected $phase evidence" }
    }
    Write-Host "PASS: headless startup, heartbeat and cooperative shutdown. No multiplayer claim. Evidence: $evidence"
} finally {
    if (!$process.HasExited) { $process.Kill(); $process.WaitForExit() }
}
