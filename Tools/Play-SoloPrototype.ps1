[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$record=Get-Content -LiteralPath (Join-Path $root 'LocalData\latest-solo-preview.json') -Raw | ConvertFrom-Json
$number=[int]$record.buildNumber
if($number -lt 23 -or $number -gt 9999){throw 'Invalid verified solo build number.'}
$folder=Join-Path $root "Builds\WindowsSolo\G2-0.0.$number"
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if($summary.result -ne 'Succeeded' -or $summary.development -or $summary.version -ne "0.0.$number"){throw 'Verified solo build missing.'}
foreach($file in (Get-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Raw | ConvertFrom-Json)){
    if((Get-FileHash -LiteralPath (Join-Path $folder $file.path) -Algorithm SHA256).Hash -ne $file.sha256){throw "Preview artifact changed: $($file.path)"}
}
$existing=@(Get-CimInstance Win32_Process -Filter "Name = 'LittleWeepsSolo.exe'" | Where-Object {$_.ExecutablePath -and $_.ExecutablePath.StartsWith($root,[StringComparison]::OrdinalIgnoreCase)})
if($existing.Count){Write-Host 'A solo preview is already open. Close it before opening a different version.';return}
Start-Process -FilePath (Join-Path $folder 'LittleWeepsSolo.exe') -WorkingDirectory $folder | Out-Null
