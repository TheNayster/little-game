[CmdletBinding()]
param([switch]$NoBrowser, [switch]$AtSignIn, [string]$IsolatedFamily)
$ErrorActionPreference = 'Stop'
$uv = (Get-Command uv -ErrorAction Stop).Source
$arguments = @('run','--offline','--with','cryptography','python',(Join-Path $PSScriptRoot 'parent_bootstrap.py'))
if ($NoBrowser) { $arguments += '--no-browser' }
if ($AtSignIn) { $arguments += '--at-signin' }
if ($IsolatedFamily) {
    if ($IsolatedFamily -notmatch '^[0-9a-f]{32}$') { throw 'Invalid isolated family.' }
    $arguments += @('--isolated-family',$IsolatedFamily)
}
# Dependencies are prepared and cached. Sign-in also works without internet;
# a missing environment produces an error, never a new family world.
Push-Location -LiteralPath (Split-Path -Parent $PSScriptRoot)
try {
    & $uv @arguments
    if ($LASTEXITCODE -ne 0) { throw 'Parent helper needs attention. The game was not stopped.' }
} finally { Pop-Location }
