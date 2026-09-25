[CmdletBinding()]
param([switch]$NoBrowser)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$folder = Join-Path $root 'LocalData\ParentServer'
$settings = Get-Content -LiteralPath (Join-Path $folder 'settings.json') -Raw | ConvertFrom-Json
if ($settings.family -notmatch '^[0-9a-f]{32}$' -or $settings.build -lt 84 -or $settings.build -gt 9999) { throw 'Parent server settings are not configured.' }
$record = Join-Path $folder 'dashboard.json'
function Get-ReadyDashboard {
    if (-not (Test-Path -LiteralPath $record)) { return $null }
    try {
        $dashboard = Get-Content -LiteralPath $record -Raw | ConvertFrom-Json
        $uri = [uri]$dashboard.url
        if ($dashboard.family -ne $settings.family -or $uri.Scheme -ne 'http' -or $uri.Host -ne '127.0.0.1' -or $uri.Fragment.Length -lt 30) { return $null }
        $response = Invoke-WebRequest -UseBasicParsing -Uri ($uri.GetLeftPart([System.UriPartial]::Authority) + '/api/status') -Headers @{'X-Little-Weeps'=$uri.Fragment.Substring(1)} -TimeoutSec 4
        if ($response.StatusCode -eq 200) { return $dashboard }
    } catch { return $null }
    return $null
}
$dashboard = Get-ReadyDashboard
if (-not $dashboard) {
    $uv = (Get-Command uv -ErrorAction Stop).Source
    $arguments = @('run','--with','cryptography','python',('"' + (Join-Path $PSScriptRoot 'Parent-Server.py') + '"'),'--family',$settings.family,'--build',"$($settings.build)",'--no-browser')
    Start-Process -FilePath $uv -ArgumentList $arguments -WorkingDirectory $root -WindowStyle Hidden -RedirectStandardOutput (Join-Path $folder 'dashboard.log') -RedirectStandardError (Join-Path $folder 'dashboard-error.log') | Out-Null
    $deadline = [DateTime]::UtcNow.AddSeconds(25)
    while (-not $dashboard -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 300; $dashboard = Get-ReadyDashboard }
    if (-not $dashboard) { throw 'Parent page could not start. See LocalData\ParentServer\dashboard-error.log. The game server was not stopped.' }
}
if (-not $NoBrowser) { Start-Process -FilePath $dashboard.url }
Write-Output 'Parent control page ready. Closing it does not stop the game server.'
