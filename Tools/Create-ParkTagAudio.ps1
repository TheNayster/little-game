# Offline prototype hints; does not imitate a character voice or play PC audio.
param([switch]$ReviseDraft)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Speech
$tagRoot=Split-Path -Parent $PSScriptRoot
$tagFolder=Join-Path $tagRoot 'Unity\FamilyPlayset\Assets\FamilyPlayset\Resources\ParkTag\en-US'
New-Item -ItemType Directory -Path $tagFolder -Force | Out-Null
$tagCues=[ordered]@{
    'tag-hint'='Let''s play tag! The orange star shows who is it. If you have the star, chase a friend! Run close to swap turns. You can stop whenever you like.'
    'tag-it'='You have the star! Chase a friend and run close to give them a turn.'
    'tag-run'='Your friend has the star! Run and play together.'
}
$tagVoice=New-Object System.Speech.Synthesis.SpeechSynthesizer
try {
    $tagVoice.SelectVoice('Microsoft Zira Desktop');$tagVoice.Rate=-1
    foreach($tagId in $tagCues.Keys){
        $tagFile=Join-Path $tagFolder "$tagId.wav"
        if((Test-Path -LiteralPath $tagFile) -and !$ReviseDraft){throw "Preserve existing narration: $tagFile"}
        $tagFormat=New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(22050,[System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen,[System.Speech.AudioFormat.AudioChannel]::Mono)
        $tagVoice.SetOutputToWaveFile($tagFile,$tagFormat);$tagVoice.Speak($tagCues[$tagId]);$tagVoice.SetOutputToNull()
    }
} finally {$tagVoice.Dispose()}
$tagCues | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $tagFolder 'transcript.json') -Encoding utf8
Write-Output 'Created offline English tag hints.'
