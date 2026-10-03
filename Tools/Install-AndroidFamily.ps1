[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateRange(1,9999)][int]$BuildNumber,
    [ValidateSet('G1','G3')][string]$BuildProfile='G1',
    [ValidatePattern('^[a-zA-Z0-9_.:\-]+$')][string]$Serial
)
# Compatibility entry point; preserve parameter binding.
& (Join-Path $PSScriptRoot 'Devices\Install-AndroidFamily.ps1') @PSBoundParameters
if (-not $?) { throw 'Delegated tool failed.' }
