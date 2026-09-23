#!/bin/sh
# Run from a local Mac Terminal when codesign requires an interactive keychain.
# The Xcode export must already be transferred and hash-verified.
set -eu

build_number=${1:?Supply the exported build number}
team=${2:?Supply the Apple development team ID}
device=${3:?Supply the connected device UDID}
case "$build_number" in ''|*[!0-9]*) echo 'Invalid build number'; exit 2;; esac
case "$team" in ''|*[!A-Z0-9]*) echo 'Invalid team ID'; exit 2;; esac
case "$device" in ''|*[!a-zA-Z0-9-]*) echo 'Invalid device ID'; exit 2;; esac

game_root="$HOME/Developer/LittleWeeps"
build_root="$game_root/Builds/G1-0.0.$build_number"
project="$build_root/Xcode/Unity-iPhone.xcodeproj"
test -d "$project" || { echo "Missing export: $project"; exit 2; }
mkdir -p "$game_root/Logs"
stamp=$(date -u +%Y%m%d-%H%M%S)
log="$game_root/Logs/g1-ios-0.0.$build_number-local-$stamp.log"
result="$game_root/Logs/g1-ios-0.0.$build_number-local-$stamp.xcresult"

printf '\nLittle Weeps: finish the iPad build\n'
printf 'If asked, enter your Mac login password here. Nothing is sent to Windows.\n'
printf 'Password characters will not appear while typing.\n\n'
security unlock-keychain "$HOME/Library/Keychains/login.keychain-db"
printf '\nBuilding. If macOS asks to let codesign use the Apple Development key, approve that request.\n'
printf 'Build log: %s\n' "$log"

build_result=0
xcodebuild -project "$project" -scheme Unity-iPhone -configuration Release \
  -destination "id=$device" -derivedDataPath "$build_root/DerivedData" \
  -resultBundlePath "$result" -allowProvisioningUpdates -allowProvisioningDeviceRegistration \
  "DEVELOPMENT_TEAM=$team" CODE_SIGN_STYLE=Automatic build > "$log" 2>&1 || build_result=$?
printf '%s\n' "$build_result" > "$game_root/Logs/g1-ios-0.0.$build_number-local.exit"
tail -18 "$log"
if [ "$build_result" -ne 0 ]; then
  printf '\nBuild stopped. Keep this window open so the error can be checked.\n'
  exit "$build_result"
fi
codesign --verify --deep --strict "$build_root/DerivedData/Build/Products/Release-iphoneos/LittleWeeps.app"
printf '\nBuild and signature checks passed. Codex can now install this app on the connected iPad.\n'
