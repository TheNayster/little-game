#!/bin/sh
# Run from a local Mac Terminal when codesign requires an interactive keychain.
# The Xcode export must already be transferred and hash-verified.
set -eu

build_number=${1:?Supply the exported build number}
team=${2:?Supply the Apple development team ID}
device=${3:?Supply the connected device UDID}
phase=${4:-G1}
output_tag=${5:-}
case "$build_number" in ''|*[!0-9]*) echo 'Invalid build number'; exit 2;; esac
case "$team" in ''|*[!A-Z0-9]*) echo 'Invalid team ID'; exit 2;; esac
case "$device" in ''|*[!a-zA-Z0-9-]*) echo 'Invalid device ID'; exit 2;; esac
case "$phase" in G1|G2) ;; *) echo 'Invalid build phase (expected G1 or G2)'; exit 2;; esac
case "$output_tag" in *[!a-zA-Z0-9_-]*) echo 'Invalid device output tag'; exit 2;; esac

game_root="$HOME/Developer/LittleWeeps"
build_root="$game_root/Builds/$phase-0.0.$build_number"
project="$build_root/Xcode/Unity-iPhone.xcodeproj"
test -d "$project" || { echo "Missing export: $project"; exit 2; }
export_version=$(/usr/libexec/PlistBuddy -c 'Print :CFBundleShortVersionString' "$build_root/Xcode/Info.plist")
export_build=$(/usr/libexec/PlistBuddy -c 'Print :CFBundleVersion' "$build_root/Xcode/Info.plist")
test "$export_version" = "0.0.$build_number" && test "$export_build" = "$build_number" || {
  echo 'Export version does not match the requested build'; exit 2;
}
phase_label=$(printf '%s' "$phase" | tr '[:upper:]' '[:lower:]')
product_root="$build_root"
log_prefix="$phase_label-ios-0.0.$build_number"
if [ -n "$output_tag" ]; then
  product_root="$build_root/DeviceBuilds/$output_tag"
  log_prefix="$log_prefix-$output_tag"
fi
# An accidental second double-click must not start another Xcode writer.
lock="$build_root/.native-build-lock"
if ! mkdir "$lock" 2>/dev/null; then
  echo "This export already has a build lock: $lock"
  echo 'Leave the original build window open. If it crashed, inspect the lock before retrying.'
  exit 3
fi
printf '%s\n' "$$" > "$lock/pid"
trap 'rm -f "$lock/pid"; rmdir "$lock"' 0
trap 'exit 130' 2
trap 'exit 143' 15
mkdir -p "$game_root/Logs"
stamp="$(date -u +%Y%m%d-%H%M%S)-$$"
log="$game_root/Logs/$log_prefix-local-$stamp.log"
result="$game_root/Logs/$log_prefix-local-$stamp.xcresult"

printf '\nLittle Weeps: finish the iPad build\n'
printf 'If asked, enter your Mac login password here. Nothing is sent to Windows.\n'
printf 'Password characters will not appear while typing.\n\n'
security unlock-keychain "$HOME/Library/Keychains/login.keychain-db"
printf '\nBuilding. If macOS asks to let codesign use the Apple Development key, approve that request.\n'
printf 'Build log: %s\n' "$log"

build_result=0
xcodebuild -project "$project" -scheme Unity-iPhone -configuration Release \
  -destination "id=$device" -derivedDataPath "$product_root/DerivedData" \
  -resultBundlePath "$result" -allowProvisioningUpdates -allowProvisioningDeviceRegistration \
  "DEVELOPMENT_TEAM=$team" CODE_SIGN_STYLE=Automatic build > "$log" 2>&1 || build_result=$?
printf '%s\n' "$build_result" > "$log.exit"
printf '%s\n' "$build_result" > "$game_root/Logs/$log_prefix-local.exit"
tail -18 "$log"
if [ "$build_result" -ne 0 ]; then
  printf '\nBuild stopped. Keep this window open so the error can be checked.\n'
  exit "$build_result"
fi
codesign --verify --deep --strict "$product_root/DerivedData/Build/Products/Release-iphoneos/LittleWeeps.app"
printf '\nSigned app: %s\n' "$product_root/DerivedData/Build/Products/Release-iphoneos/LittleWeeps.app"
printf '\nBuild and signature checks passed. Codex can now install this app on the connected iPad.\n'
