# Offline candidate voice; no host playback and no cloud voice request.
[CmdletBinding()]
param()
$toolsRoot=Split-Path -Parent $PSScriptRoot
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Speech
$root=Split-Path -Parent $toolsRoot
$folder=Join-Path $root 'Unity\FamilyPlayset\Assets\FamilyPlayset\Resources\Worlds\Home\Books\hello-dinosaurs'
$book=Get-Content -Raw -LiteralPath (Join-Path $folder 'content.json') | ConvertFrom-Json
$audio=Join-Path $folder 'audio'
New-Item -ItemType Directory -Force -Path $audio | Out-Null
$cues=[ordered]@{}
for($i=0;$i -lt $book.pages.Count;$i++){$cues['page-'+$i]=$book.pages[$i].speech}
for($i=0;$i -lt $book.names.Count;$i++){$cues['name-'+$i]=$book.names[$i]}
$voice=New-Object System.Speech.Synthesis.SpeechSynthesizer
try {
    $voice.SelectVoice('Microsoft Zira Desktop');$voice.Rate=-1
    foreach($id in $cues.Keys){
        $file=Join-Path $audio ($id+'.wav')
        if(Test-Path -LiteralPath $file){throw "Preserve existing candidate audio: $file"}
        $format=New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(22050,[System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen,[System.Speech.AudioFormat.AudioChannel]::Mono)
        $voice.SetOutputToWaveFile($file,$format)
        # Separate name clips remain replaceable after pronunciation listening.
        # Use the spelling normally first; forced syllable-by-syllable aliases
        # are metadata for a targeted future correction, never guessed timing.
        $voice.Speak($cues[$id]);$voice.SetOutputToNull()
    }
} finally {$voice.Dispose()}
[ordered]@{locale='en-US';voice='Microsoft Zira Desktop';rate=-1;source='content.json';purpose='Offline candidate narration; family voice/pronunciation acceptance pending';cues=$cues} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $audio 'transcript.json') -Encoding utf8
Write-Host 'Created eight original page recordings and six dinosaur name recordings.'
