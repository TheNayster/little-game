param([string]$Voice = 'Microsoft Zira Desktop')
$toolsRoot=Split-Path -Parent $PSScriptRoot
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Speech
$projectRoot = Split-Path $toolsRoot -Parent
$audioRoot = Join-Path $projectRoot 'docs/implementation/science-playground/hints'
$manifestPath = Join-Path $audioRoot 'manifest.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$speaker = New-Object System.Speech.Synthesis.SpeechSynthesizer
try {
    $speaker.SelectVoice($Voice)
    $speaker.Rate = -1
    $speaker.Volume = 85
    $format = New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(22050, [System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen, [System.Speech.AudioFormat.AudioChannel]::Mono)
    foreach ($hint in $manifest.hints) {
        $speaker.SetOutputToWaveFile((Join-Path $audioRoot ($hint.id + '.wav')), $format)
        $speaker.Speak($hint.text)
        $speaker.SetOutputToNull()
    }
    $manifest.voice = $speaker.Voice.Name
    [IO.File]::WriteAllText($manifestPath, ($manifest | ConvertTo-Json -Depth 5) + "`n", [Text.UTF8Encoding]::new($false))
    Write-Output ('Generated {0} local prototype hints using {1}.' -f $manifest.hints.Count, $speaker.Voice.Name)
} finally { $speaker.Dispose() }
