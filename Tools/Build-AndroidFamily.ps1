[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(1,9999)][int]$BuildNumber,[switch]$UseRecoveryKey,[switch]$FamilyLan)
# Compatibility entry point; preserve parameter binding.
& (Join-Path $PSScriptRoot 'Build\Build-AndroidFamily.ps1') @PSBoundParameters
if (-not $?) { throw 'Delegated tool failed.' }
