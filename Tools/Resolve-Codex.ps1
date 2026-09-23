function Resolve-CodexExecutable {
    # A desktop launch need not inherit Codex's own process PATH. Resolve on every
    # invocation because the desktop app changes its versioned CLI directory.
    $found = Get-Command codex.exe -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found -and (Test-Path -LiteralPath $found.Source -PathType Leaf)) { return $found.Source }
    $bin = Join-Path $env:LOCALAPPDATA 'OpenAI\Codex\bin'
    $candidates = @()
    if (Test-Path -LiteralPath $bin -PathType Container) {
        $direct = Join-Path $bin 'codex.exe'
        if (Test-Path -LiteralPath $direct -PathType Leaf) { $candidates += Get-Item -LiteralPath $direct }
        $candidates += @(Get-ChildItem -LiteralPath $bin -Directory | ForEach-Object {
            $candidate = Join-Path $_.FullName 'codex.exe'
            if (Test-Path -LiteralPath $candidate -PathType Leaf) { Get-Item -LiteralPath $candidate }
        })
    }
    $latest = $candidates | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    if ($latest) { return $latest.FullName }
    throw 'Codex CLI was not found. Open the Codex desktop app once, then run this shortcut again.'
}
