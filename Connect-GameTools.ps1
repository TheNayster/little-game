[CmdletBinding()]
param([switch]$BlenderOnly, [switch]$UnityOnly, [switch]$NoLaunch,
      [switch]$CheckOnly, [ValidateRange(1,600)][int]$WaitSeconds = 120)
$ErrorActionPreference = 'Stop'
if ($BlenderOnly -and $UnityOnly) { throw 'Choose only one app filter.' }
. (Join-Path $PSScriptRoot 'Tools\Resolve-Codex.ps1')
$codexExe = Resolve-CodexExecutable
$project = Join-Path $PSScriptRoot 'Unity\FamilyPlayset'
$state = Join-Path $env:USERPROFILE '.little-weeps-tools'
$uvx = Join-Path $env:USERPROFILE '.local\bin\uvx.exe'
$blender = 'C:\Program Files\Blender Foundation\Blender 5.0\blender.exe'
$env:PYTHONIOENCODING = 'utf-8'
$env:UNITY_MCP_TELEMETRY_ENABLED = 'false'
$env:BLENDER_MCP_DISABLE_TELEMETRY = 'true'

function Test-LocalPort([int]$Port) {
    $client = New-Object Net.Sockets.TcpClient
    try { return ($client.ConnectAsync('127.0.0.1', $Port).Wait(500) -and $client.Connected) }
    catch { return $false } finally { $client.Dispose() }
}
function Read-BlenderScene {
    $client = New-Object Net.Sockets.TcpClient
    try {
        if (!$client.ConnectAsync('127.0.0.1',9876).Wait(500)) { return $null }
        $stream = $client.GetStream(); $stream.ReadTimeout = 1500; $stream.WriteTimeout = 1500
        $bytes = [Text.Encoding]::UTF8.GetBytes('{"type":"get_scene_info","params":{}}')
        $stream.Write($bytes,0,$bytes.Length)
        $buffer = New-Object byte[] 8192
        $memory = New-Object IO.MemoryStream
        try {
            $deadline = [DateTime]::UtcNow.AddSeconds(3)
            while ([DateTime]::UtcNow -lt $deadline -and $memory.Length -lt 1048576) {
                $count = $stream.Read($buffer,0,$buffer.Length)
                if (!$count) { break }
                $memory.Write($buffer,0,$count)
                try { $response = [Text.Encoding]::UTF8.GetString($memory.ToArray()) | ConvertFrom-Json }
                catch { continue }
                if ($response.status -eq 'success') { return $response.result }
                return $null
            }
        } finally { $memory.Dispose() }
    } catch { return $null } finally { $client.Dispose() }
}

Write-Host "Little Weeps tools: $PSScriptRoot"
Write-Host "Codex located: $codexExe"
$entries = & $codexExe mcp list --json | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw 'Could not read Codex tool registrations.' }
if ($CheckOnly) {
    Write-Host ('Blender responds: ' + [bool](Read-BlenderScene))
    Write-Host ('New Unity project exists: ' + (Test-Path -LiteralPath (Join-Path $project 'ProjectSettings\ProjectVersion.txt')))
    Write-Host "Unity target: $project"
    Write-Host 'Check complete. No applications opened or registrations changed.'
    return
}
if (!(Test-Path -LiteralPath $uvx)) { throw "Missing Python tool runner: $uvx" }
New-Item -ItemType Directory -Path $state -Force | Out-Null
$changed = $false
if (!$UnityOnly) {
    $entry = $entries | Where-Object name -eq 'blender'
    if (!$entry.enabled -or $entry.transport.command -ne $uvx.Replace('\','/') -or
        ($entry.transport.args -join '|') -ne '--python|3.11|mcp-for-blender==2.0.3') {
        & $codexExe mcp add blender --env BLENDER_MCP_DISABLE_TELEMETRY=true --env PYTHONIOENCODING=utf-8 --env BLENDER_HOST=127.0.0.1 -- $uvx --python 3.11 mcp-for-blender==2.0.3
        if ($LASTEXITCODE -ne 0) { throw 'Blender registration failed.' }
        $changed = $true
    }
    [IO.File]::WriteAllText((Join-Path $state 'blender-connect.request'),[Guid]::NewGuid().ToString())
    if (!(Get-Process blender -ErrorAction SilentlyContinue) -and !$NoLaunch) {
        if (!(Test-Path -LiteralPath $blender)) { throw "Blender missing: $blender" }
        # The user requested a shortcut that opens these visible creative tools.
        Start-Process -FilePath $blender | Out-Null
    }
    $deadline = [DateTime]::UtcNow.AddSeconds($WaitSeconds)
    do {
        $scene = Read-BlenderScene
        if ($scene) { break }
        Start-Sleep -Milliseconds 750
    } while ([DateTime]::UtcNow -lt $deadline)
    if (!$scene) { throw 'Blender did not respond. Its Little Weeps Tools Reconnect helper must be enabled.' }
    Write-Host "Blender connected: $($scene.name). Current work left open and unsaved."
}
if (!$BlenderOnly) {
    $versionFile = Join-Path $project 'ProjectSettings\ProjectVersion.txt'
    if (!(Test-Path -LiteralPath $versionFile)) {
        throw 'The new FamilyPlayset Unity project is still being set up. Blender is available. This shortcut will not open the unrelated Meeps project.'
    }
    $version = ((Get-Content -LiteralPath $versionFile | Where-Object { $_ -match '^m_EditorVersion:' }) -split ':',2)[1].Trim()
    $unityExe = Join-Path $env:ProgramFiles "Unity\Hub\Editor\$version\Editor\Unity.exe"
    if (!(Test-Path -LiteralPath $unityExe)) { throw "Install Unity $version in Hub. This launcher never substitutes another editor version." }
    $entry = $entries | Where-Object name -eq 'unityMCP'
    if (!$entry.enabled -or $entry.transport.url -ne 'http://127.0.0.1:8080/mcp') {
        & $codexExe mcp add unityMCP --url http://127.0.0.1:8080/mcp
        if ($LASTEXITCODE -ne 0) { throw 'Unity registration failed.' }
        $changed = $true
    }
    if (!(Test-LocalPort 8080)) {
        $serverArgs = @('--python','3.11','--from','mcpforunityserver==9.7.3','mcp-for-unity','--transport','http','--http-url','http://127.0.0.1:8080')
        Start-Process -FilePath $uvx -ArgumentList $serverArgs -WindowStyle Hidden -WorkingDirectory $PSScriptRoot -RedirectStandardOutput (Join-Path $state 'unity-server.out.log') -RedirectStandardError (Join-Path $state 'unity-server.err.log') | Out-Null
        $deadline = [DateTime]::UtcNow.AddSeconds(30)
        while (!(Test-LocalPort 8080) -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 500 }
        if (!(Test-LocalPort 8080)) { throw "Unity connection server failed. See $state\unity-server.err.log" }
    }
    $editors = @(Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'")
    $matching = @($editors | Where-Object { $_.CommandLine -and $_.CommandLine.IndexOf($project,[StringComparison]::OrdinalIgnoreCase) -ge 0 })
    if (!$matching.Count -and $editors.Count) { throw 'A different Unity project is open. Save and close it before connecting Little Weeps.' }
    $bridgeDir = Join-Path $project 'Library\LittleWeepsTools'
    New-Item -ItemType Directory -Path $bridgeDir -Force | Out-Null
    $request = [Guid]::NewGuid().ToString()
    [IO.File]::WriteAllText((Join-Path $bridgeDir 'connect.request'),$request)
    if (!$matching.Count -and !$NoLaunch) {
        Start-Process -FilePath $unityExe -ArgumentList @('-projectPath',('"'+$project+'"')) | Out-Null
    }
    $deadline = [DateTime]::UtcNow.AddSeconds($WaitSeconds)
    $ready = $false
    do {
        $statusFile = Join-Path $bridgeDir 'status.json'
        if (Test-Path -LiteralPath $statusFile) {
            try { $status = Get-Content -LiteralPath $statusFile -Raw | ConvertFrom-Json } catch { $status = $null }
            if ($status -and $status.request -eq $request -and $status.project.TrimEnd('\','/') -eq $project.TrimEnd('\','/')) {
                if (!$status.connected) { throw "Unity connection failed: $($status.message)" }
                $ready = $true; break
            }
        }
        Start-Sleep -Seconds 1
    } while ([DateTime]::UtcNow -lt $deadline)
    if (!$ready) { throw 'The new Unity project has not confirmed its connection. Let compilation finish, then run this shortcut again.' }
    # A transport may be open before its server has registered tools. Verify an
    # actual project-info resource through our client, which rejects other roots.
    $uv = Join-Path $env:USERPROFILE '.local\bin\uv.exe'
    if (!(Test-Path -LiteralPath $uv)) { throw "Missing project-verification runner: $uv" }
    $verified = $false
    $verifyDeadline = [DateTime]::UtcNow.AddSeconds(30)
    do {
        $proof = & $uv run --python 3.11 --with mcp==2.2.0 python (Join-Path $PSScriptRoot 'Tools\unity_mcp.py') 2>&1
        if ($LASTEXITCODE -eq 0) { $verified = $true; break }
        Start-Sleep -Seconds 2
    } while ([DateTime]::UtcNow -lt $verifyDeadline)
    if (!$verified) { throw "Unity opened, but live project verification failed: $proof" }
    Write-Host "Unity connected and project verified: $project"
}
if ($changed) { Write-Host 'Tool registrations changed. Restart Codex once to load them.' }
Write-Host 'Requested tools are ready. Return to Codex.'
