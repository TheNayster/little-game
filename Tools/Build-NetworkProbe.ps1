[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(46,9999)][int]$BuildNumber)
# Compatibility entry point; preserve parameter binding.
& (Join-Path $PSScriptRoot 'Build\Build-FamilyGame.ps1') @PSBoundParameters
if (-not $?) { throw 'Delegated tool failed.' }
