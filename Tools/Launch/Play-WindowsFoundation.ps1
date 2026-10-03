[CmdletBinding()]
param()
$toolsRoot=Split-Path -Parent $PSScriptRoot
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $toolsRoot
$record=Get-Content -LiteralPath (Join-Path $root 'LocalData\latest-windows-preview.json') -Raw | ConvertFrom-Json
$number=[int]$record.buildNumber
if ($number -lt 1 -or $number -gt 9999) { throw 'Invalid verified preview build number.' }
$folder=Join-Path $root "Builds\Windows\G1-0.0.$number"
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if ($summary.result -ne 'Succeeded' -or $summary.version -ne "0.0.$number") { throw 'Verified build is missing or mismatched.' }
foreach ($file in (Get-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Raw | ConvertFrom-Json)) {
    if ((Get-FileHash -LiteralPath (Join-Path $folder $file.path) -Algorithm SHA256).Hash -ne $file.sha256) {
        throw "Preview artifact changed: $($file.path)"
    }
}
Write-Host "Opening Little Weeps foundation check 0.0.$number"
$exe=Join-Path $folder 'LittleWeeps.exe'
$existing=@(Get-CimInstance Win32_Process -Filter "Name = 'LittleWeeps.exe'" | Where-Object { $_.ExecutablePath -eq $exe })
if ($existing.Count) { Write-Host 'The verified preview is already open.'; return }
Start-Process -FilePath $exe -WorkingDirectory $folder | Out-Null
