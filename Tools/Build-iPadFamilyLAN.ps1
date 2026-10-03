[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(71,9999)][int]$BuildNumber)
# Compatibility entry point; preserve parameter binding.
& (Join-Path $PSScriptRoot 'Build\Build-iPadFamilyLAN.ps1') @PSBoundParameters
if (-not $?) { throw 'Delegated tool failed.' }
