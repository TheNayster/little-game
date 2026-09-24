# Run with Windows PowerShell 5.1; renders files without playing host audio.
[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Speech
$root=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $root 'Unity\FamilyPlayset\Assets\FamilyPlayset\Resources\SoloNarration\en-US'
New-Item -ItemType Directory -Path $folder -Force | Out-Null
$cues=[ordered]@{
    garden='Let''s give the flower a drink! Drag the bucket to the tap. Then drag it to the plant.'
    cleanup='Let''s soak up the puddle! Drag the yellow sponge onto the water. You can stop whenever you like.'
    freeplay='Let''s explore! You can walk around, move the toys, or choose a game.'
    tapwalk='Touch the ground where you want to walk. Drag a toy to move it.'
    joystick='Move the circle to walk. You can use another finger to move a toy.'
}
$voice=New-Object System.Speech.Synthesis.SpeechSynthesizer
try {
    $voice.SelectVoice('Microsoft Zira Desktop')
    $voice.Rate=-1
    foreach($id in $cues.Keys) {
        $file=Join-Path $folder "$id.wav"
        if(Test-Path -LiteralPath $file){throw "Narration exists; preserve it or select an explicit revision before regenerating: $file"}
        $format=New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(22050,[System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen,[System.Speech.AudioFormat.AudioChannel]::Mono)
        $voice.SetOutputToWaveFile($file,$format)
        $voice.Speak($cues[$id])
        $voice.SetOutputToNull()
    }
} finally { $voice.Dispose() }
[ordered]@{locale='en-US';voice='Microsoft Zira Desktop';rate=-1;purpose='Temporary offline prototype narration; not character voice imitation';cues=$cues} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $folder 'transcript.json') -Encoding utf8
Write-Host 'Created five offline English prototype hints.'
