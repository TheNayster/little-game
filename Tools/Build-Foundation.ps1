[CmdletBinding()]
param([ValidateSet('Windows','iOS','Android','WindowsServer')][string]$Target='Windows', [ValidateRange(1,9999)][int]$BuildNumber=1)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$project=Join-Path $root 'Unity\FamilyPlayset'
$versionLine=Get-Content -LiteralPath (Join-Path $project 'ProjectSettings\ProjectVersion.txt') | Where-Object { $_ -match '^m_EditorVersion:' }
$version=($versionLine -split ':',2)[1].Trim()
$editor=Join-Path $env:ProgramFiles "Unity\Hub\Editor\$version\Editor\Unity.exe"
$engines=Join-Path (Split-Path $editor) 'Data\PlaybackEngines'
if ($Target -eq 'Android') {
    foreach($relative in @('AndroidPlayer\UnityEditor.Android.Extensions.dll','AndroidPlayer\OpenJDK\bin\java.exe','AndroidPlayer\NDK\source.properties','AndroidPlayer\SDK\platforms\android-36\android.jar','AndroidPlayer\SDK\build-tools\36.0.0\aapt2.exe','AndroidPlayer\SDK\cmdline-tools\16.0\bin\sdkmanager.bat','AndroidPlayer\SDK\cmake\3.22.1\bin\cmake.exe')) {
        if (!(Test-Path -LiteralPath (Join-Path $engines $relative))) { throw "Matching Android toolchain is incomplete ($relative). Install the $version Android module and dependencies first." }
    }
}
if ($Target -eq 'WindowsServer' -and !(Test-Path -LiteralPath (Join-Path $engines 'windowsstandalonesupport\Variations\win64_server_nondevelopment_mono'))) { throw "Install the $version Windows Dedicated Server module first." }
$open=@(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" | Where-Object { $_.CommandLine -and $_.CommandLine.Contains($project) })
if ($open.Count) { throw 'Save and close this Unity project before running a batch build.' }
$platform=switch ($Target) { 'iOS' {'iOS'} 'Android' {'Android'} default {'StandaloneWindows64'} }
$method=if ($Target -eq 'iOS') { 'IOS' } else { $Target }
$logs=Join-Path $root 'LocalData\Logs'
New-Item -ItemType Directory -Path $logs -Force | Out-Null
$log=Join-Path $logs ("build-$Target-$BuildNumber-"+[DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')+'.log')
$argsList=@('-batchmode','-quit','-projectPath',('"'+$project+'"'),'-buildTarget',$platform,'-executeMethod',"LittleWeeps.EditorTools.FoundationBuild.$method",'-familyBuildNumber',"$BuildNumber",'-logFile',('"'+$log+'"'))
if ($Target -eq 'WindowsServer') { $argsList += @('-standaloneBuildSubtarget','Server') }
Write-Host "Building $Target 0.0.$BuildNumber. Log: $log"
$process=Start-Process -FilePath $editor -ArgumentList $argsList -WindowStyle Hidden -PassThru
# Wait for Unity itself. Start-Process -Wait also waits for persistent Roslyn child servers.
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw "Unity build failed (exit $($process.ExitCode)). See $log" }
$folder=Join-Path $root "Builds\$Target\G1-0.0.$BuildNumber"
$summary=Get-Content -LiteralPath (Join-Path $folder 'build-summary.json') -Raw | ConvertFrom-Json
if ($summary.result -ne 'Succeeded' -or $summary.version -ne "0.0.$BuildNumber") { throw 'Build evidence does not match the requested build.' }
$revision=(& git -C $root rev-parse HEAD).Trim()
$dirty=@(& git -C $root status --porcelain -- Unity Tools)
$sourcePaths=@(& git -C $root ls-files --cached --others --exclude-standard -- Unity/FamilyPlayset/Assets Unity/FamilyPlayset/Packages Unity/FamilyPlayset/ProjectSettings Tools | Sort-Object -Unique)
$sourceFiles=@($sourcePaths | ForEach-Object {
    $path=Join-Path $root $_
    if (Test-Path -LiteralPath $path -PathType Leaf) { [ordered]@{path=$_;sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()} }
})
[ordered]@{sourceCommit=$revision;dirtyPaths=$dirty;unity=$version;build="0.0.$BuildNumber";files=$sourceFiles} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $folder 'source-manifest.json') -Encoding utf8
if ($Target -in @('Windows','Android','WindowsServer')) {
    $files=@(Get-ChildItem -LiteralPath $folder -Recurse -File | Where-Object { $_.Name -notin @('source-manifest.json','build-summary.json','artifact-manifest.json') } | Sort-Object FullName | ForEach-Object {
        [ordered]@{path=$_.FullName.Substring($folder.Length+1).Replace('\','/');bytes=$_.Length;sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()}
    })
    $files | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $folder 'artifact-manifest.json') -Encoding utf8
}
Write-Host "Build completed. Evidence: $root\Builds\$Target\G1-0.0.$BuildNumber\build-summary.json"
