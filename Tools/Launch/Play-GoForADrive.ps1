[CmdletBinding()]
param([ValidateRange(1,4)][int]$Players=2,[switch]$Verify)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$uv=(Get-Command uv.exe -ErrorAction Stop).Source
$script=Join-Path $PSScriptRoot 'Play-GoForADrive.py'
$arguments=@('run','--offline',('"'+$script+'"'),'--players',"$Players")
if($Verify){$arguments+='--verify'; & $uv run --offline $script --players $Players --verify; if($LASTEXITCODE){throw 'Go for a Drive preview verification failed.'}; return}
$process=Start-Process -FilePath $uv -ArgumentList $arguments -WorkingDirectory $root -WindowStyle Hidden -PassThru
if($process.WaitForExit(3000) -and $process.ExitCode -ne 0){throw (Get-Content -LiteralPath (Join-Path $root 'LocalData/go-for-a-drive-preview-error.txt') -Raw)}
Write-Host 'Opening Go for a Drive. Close its windows to stop this separate preview server.'
