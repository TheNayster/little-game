$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding $false
$request = [Console]::In.ReadToEnd() | ConvertFrom-Json
if ($request.action -notin @('inspect','create','remove') -or [IO.Path]::GetExtension($request.path) -ne '.lnk') { throw 'Invalid shortcut operation.' }
$shell = New-Object -ComObject WScript.Shell
if ($request.action -eq 'create') {
    if (Test-Path -LiteralPath $request.path) { throw 'An existing shortcut will not be replaced.' }
    # Save under a unique sibling and use a non-overwriting move for publication.
    $temporary = Join-Path ([IO.Path]::GetDirectoryName($request.path)) ([guid]::NewGuid().ToString('N') + '.lnk')
    try {
        $shortcut = $shell.CreateShortcut($temporary)
        $shortcut.TargetPath = $request.target
        $shortcut.Arguments = $request.arguments
        $shortcut.WorkingDirectory = $request.workingDirectory
        $shortcut.Description = $request.description
        $shortcut.WindowStyle = $request.windowStyle
        $shortcut.Save()
        [IO.File]::Move($temporary, $request.path)
    } finally { if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary } }
}
if (-not (Test-Path -LiteralPath $request.path -PathType Leaf)) { throw 'Shortcut unavailable.' }
$shortcut = $shell.CreateShortcut($request.path)
$value = [ordered]@{target=$shortcut.TargetPath; arguments=$shortcut.Arguments; workingDirectory=$shortcut.WorkingDirectory; description=$shortcut.Description; windowStyle=$shortcut.WindowStyle}
if ($request.action -eq 'remove') {
    foreach ($key in @('target','arguments','workingDirectory','description','windowStyle')) {
        if ($value[$key] -cne $request.$key) { throw 'Changed shortcut was left untouched.' }
    }
    Remove-Item -LiteralPath $request.path
}
$value | ConvertTo-Json -Compress
