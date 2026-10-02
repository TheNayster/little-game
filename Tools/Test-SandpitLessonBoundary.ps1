param([string]$ResponseDirectory='')
$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$taskUnity=Join-Path $taskRoot 'Unity/FamilyPlayset'
$taskOutput=Join-Path $taskRoot 'LocalData/SandpitLessonBoundary'
$taskVersion=((Get-Content -LiteralPath (Join-Path $taskUnity 'ProjectSettings/ProjectVersion.txt') | Select-Object -First 1) -split ': ')[1]
$taskEditor=Join-Path $env:ProgramFiles ('Unity/Hub/Editor/'+$taskVersion+'/Editor')
if(!$ResponseDirectory){
    $taskResponses=@(& rg --files (Join-Path $taskUnity 'Library/Bee') -g 'LittleWeeps.Core.rsp')
    $ResponseDirectory=$taskResponses | Get-Item | Where-Object { $_.Directory.Name.EndsWith('E.dag') -and (Test-Path -LiteralPath (Join-Path $_.DirectoryName 'LittleWeeps.Client.rsp')) } | Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty DirectoryName
}
if(!$ResponseDirectory){throw 'Existing Unity editor compiler responses are required; no editor will be launched.'}
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
foreach($taskAssembly in @('Core','Client')){
    $taskResponse=Join-Path $taskOutput ('LittleWeeps.'+$taskAssembly+'.rsp')
    $taskLines=Get-Content -LiteralPath (Join-Path $ResponseDirectory ('LittleWeeps.'+$taskAssembly+'.rsp'))
    # A cached file list must include every current source in this assembly.
    Push-Location -LiteralPath $taskUnity
    try {
        $taskSources=@(& rg --files ('Assets/FamilyPlayset/Code/'+$taskAssembly) -g '*.cs')
        foreach($taskSource in $taskSources){if(('"'+$taskSource.Replace('\','/')+'"') -notin $taskLines){throw ('Compiler response omits current source: '+$taskSource)}}
    } finally { Pop-Location }
    $taskLines=$taskLines | ForEach-Object {
        if($_ -match '^-out:'){ '-out:"'+(Join-Path $taskOutput ('LittleWeeps.'+$taskAssembly+'.dll'))+'"' }
        elseif($_ -match '^-refout:'){ '-refout:"'+(Join-Path $taskOutput ('LittleWeeps.'+$taskAssembly+'.ref.dll'))+'"' }
        elseif($taskAssembly -eq 'Client' -and $_ -match '^-r:.*LittleWeeps.Core.ref.dll'){ '-r:"'+(Join-Path $taskOutput 'LittleWeeps.Core.ref.dll')+'"' }
        else { $_ }
    }
    $taskLines | Set-Content -LiteralPath $taskResponse -Encoding utf8
    Push-Location -LiteralPath $taskUnity
    try {
        & (Join-Path $taskEditor 'Data/NetCoreRuntime/dotnet.exe') (Join-Path $taskEditor 'Data/DotNetSdkRoslyn/csc.dll') ('@'+$taskResponse)
        if($LASTEXITCODE -ne 0){throw ($taskAssembly+' compile failed: '+$LASTEXITCODE)}
        Write-Output ('PASS '+$taskAssembly+' current-source Unity-reference compilation')
    } finally { Pop-Location }
}
& dotnet run --project (Join-Path $taskRoot 'Tools/Sandpit.Tests/Sandpit.Tests.csproj') --configuration Release -- (Join-Path $taskOutput 'LittleWeeps.Client.dll')
if($LASTEXITCODE -ne 0){throw ('Sandpit lesson-boundary checks failed: '+$LASTEXITCODE)}
