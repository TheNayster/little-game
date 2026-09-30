[CmdletBinding()]
param([switch]$VerifyOnly)
$ErrorActionPreference='Stop'
# A Codex/uv parent can inherit PowerShell 7 module paths. Load the Windows
# PowerShell modules explicitly so unattended checks use the same OS APIs.
foreach($module in @('Microsoft.PowerShell.Utility','Microsoft.PowerShell.Management','NetSecurity')) {
    Import-Module (Join-Path $PSHOME ('Modules\'+$module+'\'+$module+'.psd1')) -Force
}
$root=Split-Path -Parent $PSScriptRoot
$record=Join-Path $root 'LocalData\PCServer\installation.json'
$installation=Get-Content -LiteralPath $record -Raw | ConvertFrom-Json
$exe=Join-Path $root 'LocalData\PCServer\current\Server\LittleWeepsNetwork.exe'
if ($installation.format -ne 1 -or $installation.program -ine $exe -or !(Test-Path -LiteralPath $exe -PathType Leaf)) { throw 'Verified persistent installation required.' }
$manifest=Get-Content -LiteralPath (Join-Path $root 'LocalData\PCServer\current\artifact-manifest.json') -Raw | ConvertFrom-Json
$entry=@($manifest | Where-Object {$_.path -eq 'Server/LittleWeepsNetwork.exe'})
if ($entry.Count -ne 1 -or (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash -ine $entry[0].sha256) { throw 'Installed executable differs from its manifest.' }
$name='LittleWeeps-Home-UDP'
if (!$VerifyOnly) {
    if(![Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'One-time Windows administrator approval is required for the persistent server permission.' }
    if (!(Get-NetFirewallRule -Name $name -ErrorAction SilentlyContinue)) {
        New-NetFirewallRule -Name $name -DisplayName 'Little Weeps persistent home server (local network)' -Direction Inbound -Action Allow -Enabled True -Profile Private,Public -Program $exe -Protocol UDP -RemoteAddress LocalSubnet | Out-Null
    }
    # A dismissed first-run prompt may have left an exact-path blocking rule.
    # Disable only those blocks for the reviewed permanent executable.
    $blocks=@(Get-NetFirewallApplicationFilter | Where-Object {$_.Program -ieq $exe} | Get-NetFirewallRule | Where-Object {$_.Action -eq 'Block' -and $_.Enabled -eq 'True'})
    foreach($rule in $blocks) { Disable-NetFirewallRule -Name $rule.Name | Out-Null }
}
$allowed=Get-NetFirewallRule -Name $name
$app=$allowed | Get-NetFirewallApplicationFilter
$ports=$allowed | Get-NetFirewallPortFilter
$addresses=$allowed | Get-NetFirewallAddressFilter
if($allowed.Enabled -ne 'True' -or $allowed.Action -ne 'Allow' -or $allowed.Direction -ne 'Inbound' -or $allowed.Profile.ToString() -ne 'Private, Public' -or $app.Program -ine $exe -or $ports.Protocol -ne 'UDP' -or @($addresses.RemoteAddress).Count -ne 1 -or @($addresses.RemoteAddress)[0] -ne 'LocalSubnet') { throw 'The existing permission scope does not match the reviewed persistent LAN rule.' }
$remaining=@(Get-NetFirewallApplicationFilter | Where-Object {$_.Program -ieq $exe} | Get-NetFirewallRule | Where-Object {$_.Action -eq 'Block' -and $_.Enabled -eq 'True'})
if($remaining.Count) { throw 'An exact-path blocking rule still prevents family connections.' }
if (!$VerifyOnly) {
    [ordered]@{passed=$true;rule=$name;program=$exe;scope='LocalSubnet';protocol='UDP';utc=[DateTime]::UtcNow.ToString('O')} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $root 'LocalData\PCServer\firewall.json') -Encoding utf8
}
Write-Output 'Persistent PC-server LAN permission verified; no per-build permission is needed.'
