[CmdletBinding()]
param([switch]$OpenBrowser)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$docs=Join-Path $root 'docs'
$url='http://127.0.0.1:8917/family-playset-build-guide-2026-09-23.html'
$expected=[IO.File]::ReadAllText((Join-Path $docs 'family-playset-build-guide-2026-09-23.html'))
function Test-CurrentGuide {
    try {
        $page=Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2
        # Windows PowerShell 5.1 otherwise guesses a legacy HTML text encoding.
        $content=[Text.Encoding]::UTF8.GetString($page.RawContentStream.ToArray())
        return $page.StatusCode -eq 200 -and $content -eq $expected
    }
    catch {return $false}
}
if(!(Test-CurrentGuide)){
    if(Get-NetTCPConnection -LocalPort 8917 -State Listen -ErrorAction SilentlyContinue){throw 'Port 8917 already serves different content. It has not been stopped or replaced.'}
    $python=(Get-Command python.exe -ErrorAction Stop).Source
    $logRoot=Join-Path $root 'LocalData\Logs'
    New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
    $stamp=[DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')
    $process=Start-Process -FilePath $python -ArgumentList @('-m','http.server','8917','--bind','127.0.0.1','--directory',('"'+$docs+'"')) -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $logRoot "guide-$stamp.log") -RedirectStandardError (Join-Path $logRoot "guide-$stamp-error.log")
    $ready=$false
    for($i=0;$i -lt 10;$i++){Start-Sleep -Milliseconds 250;if(Test-CurrentGuide){$ready=$true;break};if($process.HasExited){break}}
    if(!$ready){if(!$process.HasExited){Stop-Process -Id $process.Id};throw 'Guide server did not return this project''s current page. Check LocalData/Logs.'}
    [ordered]@{pid=$process.Id;url=$url;root=$docs;startedUtc=[DateTime]::UtcNow.ToString('O')} | ConvertTo-Json | Set-Content (Join-Path $root 'LocalData\guide-server.json') -Encoding utf8
}
if($OpenBrowser){Start-Process $url | Out-Null}
Write-Output $url
