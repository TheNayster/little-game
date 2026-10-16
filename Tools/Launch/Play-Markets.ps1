[CmdletBinding()]
param([ValidateRange(1,4)][int]$Players=2)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$uv=(Get-Command uv.exe -ErrorAction Stop).Source
$script=Join-Path $PSScriptRoot 'Play-Markets.py'
$process=Start-Process -FilePath $uv -ArgumentList @('run','--offline',('"'+$script+'"'),'--players',"$Players") -WorkingDirectory $root -WindowStyle Hidden -PassThru
if($process.WaitForExit(3000) -and $process.ExitCode -ne 0){throw (Get-Content -LiteralPath (Join-Path $root 'LocalData/markets-preview-error.txt') -Raw)}
Write-Host 'Opening Markets in its separate saved family. Close all its windows to stop this preview server.'
