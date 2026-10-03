$toolsRoot=Split-Path -Parent $PSScriptRoot
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $toolsRoot
$receiptPath=Join-Path $projectRoot 'LocalData\CharacterWorkshop\current.json'
if(-not (Test-Path -LiteralPath $receiptPath)){throw 'Build and verify the character workshop first; see its implementation record.'}
$receipt=Get-Content -LiteralPath $receiptPath -Raw | ConvertFrom-Json
if((Get-FileHash -LiteralPath $receipt.exe).Hash -ne $receipt.exeSha256 -or (Get-FileHash -LiteralPath $receipt.assembly).Hash -ne $receipt.assemblySha256){throw 'The verified workshop files changed; rebuild and verify.'}
$arguments=@('-screen-width','1280','-screen-height','800','-screen-fullscreen','0','-logFile',('"'+(Join-Path $projectRoot 'LocalData\CharacterWorkshop\preview.log')+'"'))
# This is the user-invoked interactive workshop, so open its visible window.
Start-Process -FilePath $receipt.exe -ArgumentList $arguments -WindowStyle Normal | Out-Null
