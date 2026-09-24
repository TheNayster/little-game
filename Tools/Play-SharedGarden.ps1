[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$record=Join-Path $root 'LocalData\latest-shared-preview.json'
if(!(Test-Path -LiteralPath $record)){throw 'No verified shared garden preview is ready yet.'}
$python=(Get-Command python.exe -ErrorAction Stop).Source
$script=Join-Path $PSScriptRoot 'Play-SharedGarden.py'
$process=Start-Process -FilePath $python -ArgumentList @(('"'+$script+'"')) -WorkingDirectory $root -WindowStyle Hidden -PassThru
if($process.WaitForExit(3000) -and $process.ExitCode -ne 0){throw (Get-Content -LiteralPath (Join-Path $root 'LocalData\shared-preview-error.txt') -Raw)}
Write-Host 'Two shared garden windows are opening. Arrange them side by side. Close both to stop this preview server.'
