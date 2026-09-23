[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$ssh = Join-Path $env:WINDIR 'System32\OpenSSH\ssh.exe'
$key = Join-Path $env:USERPROFILE '.ssh\little_weeps_mac_ed25519'
if (!(Test-Path -LiteralPath $key)) { throw 'This PC is missing its Little Weeps Mac SSH key.' }
$inventory = (Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Mac-Inventory.sh') -Raw).Replace("`r",'')
$inventory | & $ssh -i $key -o IdentitiesOnly=yes -o BatchMode=yes -o ConnectTimeout=8 nayster@eduardos-mbp.lan 'tr -d "\r" | sh'
if ($LASTEXITCODE -ne 0) { throw 'Mac inventory did not complete. Check Remote Login and the installed public key.' }
