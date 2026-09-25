[CmdletBinding()]
param([Parameter(Mandatory)][ValidateRange(83,9999)][int]$BuildNumber,[switch]$VerifyOnly)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$result=Join-Path $root "LocalData\Verification\server-firewall-$BuildNumber.json"
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $result) | Out-Null
try {
    if(-not $VerifyOnly -and -not [Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Windows administrator approval is required.'}
    $folder=Join-Path $root "Builds\NetworkProbe\G3-0.0.$BuildNumber"
    $exe=Join-Path $folder 'Server\LittleWeepsNetwork.exe'
    $summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
    if(@($summary.builds | Where-Object {$_.role -eq 'Server' -and $_.result -eq 'Succeeded' -and $_.errors -eq 0 -and $_.version -eq "0.0.$BuildNumber"}).Count -ne 1){throw 'Verified server build required.'}
    $manifest=Get-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Raw | ConvertFrom-Json
    $entry=@($manifest | Where-Object {$_.path -eq 'Server/LittleWeepsNetwork.exe'})
    if($entry.Count -ne 1 -or (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash -ne $entry[0].sha256){throw 'Server executable does not match the build manifest.'}
    # A dismissed Windows first-run prompt creates exact-path block rules.
    # Replace only those for this build with a local-subnet UDP permission.
    $blocks=@(Get-NetFirewallApplicationFilter | Where-Object {$_.Program -ieq $exe} | Get-NetFirewallRule | Where-Object {$_.Action -eq 'Block' -and $_.Enabled -eq 'True'})
    $name="LittleWeeps-Home-UDP-$BuildNumber"
    if(-not $VerifyOnly -and -not (Get-NetFirewallRule -Name $name -ErrorAction SilentlyContinue)) {
        New-NetFirewallRule -Name $name -DisplayName "Little Weeps home server $BuildNumber (local network)" -Direction Inbound -Action Allow -Enabled True -Profile Private,Public -Program $exe -Protocol UDP -RemoteAddress LocalSubnet | Out-Null
    }
    if(-not $VerifyOnly){foreach($rule in $blocks){Disable-NetFirewallRule -Name $rule.Name | Out-Null}}
    $allowed=Get-NetFirewallRule -Name $name
    if($allowed.Enabled -ne 'True' -or $allowed.Action -ne 'Allow'){throw 'Firewall permission was not enabled.'}
    $application=$allowed | Get-NetFirewallApplicationFilter
    $ports=$allowed | Get-NetFirewallPortFilter
    $addresses=$allowed | Get-NetFirewallAddressFilter
    # CIM returns a scalar for a single address; index an explicit array so we
    # compare LocalSubnet itself instead of the first character of that string.
    if($application.Program -ine $exe -or $ports.Protocol -ne 'UDP' -or @($addresses.RemoteAddress).Count -ne 1 -or @($addresses.RemoteAddress)[0] -ne 'LocalSubnet'){throw 'Existing rule scope differs from the reviewed local-network permission.'}
    $remaining=@(Get-NetFirewallApplicationFilter | Where-Object {$_.Program -ieq $exe} | Get-NetFirewallRule | Where-Object {$_.Action -eq 'Block' -and $_.Enabled -eq 'True'})
    if($remaining.Count){throw 'An exact-path block still prevents this server from accepting connections.'}
    [ordered]@{passed=$true;build=$BuildNumber;rule=$name;program=$exe;remoteAddress='LocalSubnet';protocol='UDP';verifiedReadOnly=[bool]$VerifyOnly;disabledExactPathBlocks=@($blocks | ForEach-Object {$_.Name})} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $result -Encoding utf8
} catch {
    [ordered]@{passed=$false;build=$BuildNumber;error=$_.Exception.Message} | ConvertTo-Json | Set-Content -LiteralPath $result -Encoding utf8
    throw
}
