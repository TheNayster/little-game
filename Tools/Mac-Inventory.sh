#!/bin/sh
# Read-only inventory. Does not accept Xcode terms or change developer settings.
set -u
printf '\nmacOS\n'
sw_vers
printf '\nArchitecture\n'
uname -m
printf '\nXcode\n'
xcodebuild -version
printf '\nDeveloper directory\n'
xcode-select -p
printf '\nUnity editors\n'
if [ -d /Applications/Unity/Hub/Editor ]; then
  ls /Applications/Unity/Hub/Editor
else
  printf 'No Unity Hub editor directory found.\n'
fi
printf '\nDisk space\n'
df -h "$HOME"
