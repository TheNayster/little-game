[CmdletBinding()]
param()
$toolsRoot=Split-Path -Parent $PSScriptRoot
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $toolsRoot
$folder=Join-Path $root 'Unity\FamilyPlayset\Assets\StreamingAssets\Foundation'
New-Item -ItemType Directory -Path $folder -Force | Out-Null
$output=Join-Path $folder 'test-clip.mp4'
if (Test-Path -LiteralPath $output) { throw 'The probe already exists. Do not overwrite a versioned media fixture.' }
& ffmpeg -hide_banner -loglevel error -n -f lavfi -i 'testsrc2=size=640x360:rate=30' -f lavfi -i 'sine=frequency=440:sample_rate=48000' -t 12 -c:v libx264 -profile:v baseline -level 3.0 -pix_fmt yuv420p -crf 28 -c:a aac -b:a 64k -movflags +faststart $output
if ($LASTEXITCODE -ne 0) { throw 'Video fixture generation failed.' }
& ffprobe -v error -show_entries format=duration:stream=codec_name,profile,width,height,pix_fmt -of json $output
if ($LASTEXITCODE -ne 0) { throw 'Video fixture inspection failed.' }
