[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(23,9999)][int]$BuildNumber)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root "Builds\WindowsSolo\G2-0.0.$BuildNumber"
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if($summary.result -ne 'Succeeded' -or $summary.development -or $summary.platform -ne 'Windows' -or $summary.version -ne "0.0.$BuildNumber" -or $summary.verificationContract -lt 2){throw 'This suite requires a matching native build with isolated recovery-test support.'}
foreach($entry in (Get-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Raw | ConvertFrom-Json)){
    if((Get-FileHash -LiteralPath (Join-Path $folder $entry.path) -Algorithm SHA256).Hash -ne $entry.sha256){throw "Artifact changed: $($entry.path)"}
}
$run=[Guid]::NewGuid().ToString('N')
$data=Join-Path $env:USERPROFILE "AppData\LocalLow\Little Weeps\Little Weeps\SoloPrototype\$run"
$evidence=Join-Path $root "LocalData\Verification\solo-crash-$run"
New-Item -ItemType Directory -Path $evidence | Out-Null
function Start-Probe([string]$mode,[string]$namespace=$run,[string]$logName=$mode){
    $log=Join-Path $evidence "$logName.log"
    Start-Process -FilePath (Join-Path $folder 'LittleWeepsSolo.exe') -ArgumentList @('-screen-fullscreen','0','-screen-width','1280','-screen-height','800','-soloVerify',$mode,'-soloRun',$namespace,'-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
}
function Wait-Probe($process,[int]$expectedExit){
    if(!$process.WaitForExit(30000)){$process.Kill();$process.WaitForExit();throw 'Test process timed out; only that process was terminated.'}
    if($process.ExitCode -ne $expectedExit){throw "Unexpected test exit $($process.ExitCode), expected $expectedExit"}
}
function Assert-Blocked([string]$case){
    $before=(Get-FileHash -LiteralPath $backup -Algorithm SHA256).Hash
    $process=Start-Probe 'blocked' $run $case
    Wait-Probe $process 1
    $record=Get-Content -LiteralPath (Join-Path $data 'blocked.json') -Raw | ConvertFrom-Json
    if(!$record.loadBlocked -or $record.runId -ne $run -or $record.build -ne "0.0.$BuildNumber"){throw "$case did not stop at the save load barrier."}
    if((Get-FileHash -LiteralPath $backup -Algorithm SHA256).Hash -ne $before){throw "$case changed backup progress."}
    Copy-Item -LiteralPath (Join-Path $data 'blocked.json') -Destination (Join-Path $evidence "$case.json")
}

# Malformed opt-in arguments must exit before the normal family's slot is opened.
$invalid=Start-Probe 'crash-hold' 'invalid-guid' 'invalid-arguments'
Wait-Probe $invalid 2
$holder=Start-Probe 'crash-hold'
try{
    $readyPath=Join-Path $data 'crash-ready.json'
    $until=[DateTime]::UtcNow.AddSeconds(30)
    while(!(Test-Path -LiteralPath $readyPath)){
        if($holder.HasExited -or [DateTime]::UtcNow -gt $until){throw 'Crash fixture did not become ready.'}
        Start-Sleep -Milliseconds 100
    }
    $ready=Get-Content -LiteralPath $readyPath -Raw | ConvertFrom-Json
    if(!$ready.passed -or $ready.runId -ne $run -or $ready.build -ne "0.0.$BuildNumber"){throw 'Wrong crash fixture identity.'}
    # This Process object came directly from Start-Process above, not a name search.
    $holder.Kill();$holder.WaitForExit()
}finally{if(!$holder.HasExited){$holder.Kill();$holder.WaitForExit()}}
$save=Join-Path $data 'world.save';$backup=$save+'.bak'
if(Test-Path -LiteralPath (Join-Path $data 'crash-graceful-exit.txt')){throw 'Graceful cleanup ran; hard-stop test is invalid.'}
$held=((Get-Content -LiteralPath $save -Raw) -split "`n",3)[2] | ConvertFrom-Json
if(@($held.toys | Where-Object holder).Count -ne 1){throw 'Hard-stop checkpoint did not contain a held toy.'}
Copy-Item -LiteralPath $save -Destination (Join-Path $evidence 'after-kill.save')
$resume=Start-Probe 'crash-resume';Wait-Probe $resume 0
$resumed=Get-Content -LiteralPath (Join-Path $data 'crash-resume.json') -Raw | ConvertFrom-Json
if(!$resumed.passed -or !$resumed.exactStateRestored -or !$resumed.staleHoldReleased -or !$resumed.toyUsableAgain -or $resumed.runId -ne $run){throw 'Native crash recovery failed.'}
Copy-Item -LiteralPath (Join-Path $data 'crash-resume.json') -Destination $evidence
Copy-Item -LiteralPath (Join-Path $data 'crash-baseline.json') -Destination $evidence

# Preserve the recovered test save, then inject faults only in this fresh GUID slot.
Move-Item -LiteralPath $save -Destination ($save+'.preserved')
New-Item -ItemType Directory -Path $save | Out-Null
Assert-Blocked 'directory-at-save'
if(!(Test-Path -LiteralPath $save -PathType Container)){throw 'Directory obstacle was changed.'}
$obstacleDestination=[IO.Path]::GetFullPath($save+'.directory-obstacle')
$isolatedPrefix=[IO.Path]::GetFullPath($data)+[IO.Path]::DirectorySeparatorChar
if(!(Resolve-Path -LiteralPath $save).Path.StartsWith($isolatedPrefix,[StringComparison]::OrdinalIgnoreCase) -or !$obstacleDestination.StartsWith($isolatedPrefix,[StringComparison]::OrdinalIgnoreCase) -or @(Get-ChildItem -LiteralPath $save -Force).Count){throw 'Refusing to move an unexpected/nonempty directory.'}
Move-Item -LiteralPath $save -Destination $obstacleDestination
Copy-Item -LiteralPath ($save+'.preserved') -Destination $save
$lock=[IO.File]::Open($save,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::None)
try{Assert-Blocked 'locked-primary'}finally{$lock.Dispose()}
if((Get-FileHash -LiteralPath $save).Hash -ne (Get-FileHash -LiteralPath ($save+'.preserved')).Hash){throw 'Locked primary changed.'}
[ordered]@{passed=$true;build=$BuildNumber;runId=$run;invalidArgumentsRejected=$true;hardStopWithoutQuitCallback=$true;heldCheckpointObserved=$true;exactStateRestored=$true;toyUsableAgain=$true;directoryBlockedWithoutOverwrite=$true;lockedPrimaryBlockedWithoutRollback=$true;physicalDevicesAccessed=$false;utc=[DateTime]::UtcNow.ToString('O')} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidence 'result.json') -Encoding utf8
Write-Output "PASS: hard-stop recovery and unreadable-save barriers. Evidence: $evidence"
