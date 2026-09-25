"""In-place update of the retained project emulator only; never a physical device."""
import argparse
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import uuid

from shared_garden_runtime import ROOT, read, write, require

ADB = Path(os.environ['LOCALAPPDATA']) / 'Android/Sdk/platform-tools/adb.exe'
PACKAGE = 'com.littleweeps.familyplayset'
AVD = 'LittleWeeps_G3_AndroidLAN'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--serial', default='emulator-5580')
    parser.add_argument('--from-build', type=int, required=True)
    parser.add_argument('--build', type=int, required=True)
    args = parser.parse_args()
    require(re.fullmatch(r'emulator-\d+', args.serial), 'Project emulator only')
    base = [str(ADB), '-s', args.serial]
    def adb(*command):
        return subprocess.check_output(base + list(command), stderr=subprocess.PIPE, timeout=90)
    require(adb('emu', 'avd', 'name').decode().splitlines()[0] == AVD, 'Wrong emulator')
    require(adb('shell', 'getprop', 'sys.boot_completed').strip() == b'1', 'Emulator is not ready')
    # Keep adb pull paths below the Windows path limit as adventure paths grow.
    evidence = ROOT / 'LocalData/Verification' / ('eupd-' + uuid.uuid4().hex)
    evidence.mkdir()
    def sha(raw): return hashlib.sha256(raw).hexdigest()
    def artifact(build):
        folder = ROOT / f'Builds/AndroidSigned/G3-0.0.{build}'
        summary = read(folder / 'build-summary.json')
        require(summary['version'] == f'0.0.{build}' and summary['result'] == 'Succeeded'
                and not summary['development'], 'Verified release artifact required')
        apk = folder / 'LittleWeeps.apk'; manifest = read(folder / 'artifact-manifest.json')
        entry = next(v for v in (manifest if isinstance(manifest, list) else [manifest]) if v['path'] == apk.name)
        require(sha(apk.read_bytes()) == entry['sha256'], 'Artifact hash changed')
        return apk, entry['sha256']
    def installed():
        value = adb('shell', 'pm', 'path', PACKAGE).decode().strip()
        require(re.fullmatch(r'package:/data/app/[a-zA-Z0-9_+=~/\-.]+/base\.apk', value), 'Expected installed base APK')
        return adb('exec-out', 'cat', value[8:])
    old_apk, old_hash = artifact(args.from_build); new_apk, new_hash = artifact(args.build)
    current = installed()
    require(sha(current) == old_hash and args.build > args.from_build, 'Unexpected baseline; no downgrade or fallback')
    current_path = evidence / 'before.apk'; current_path.write_bytes(current)
    tool = Path(read(ROOT / 'LocalData/android-toolchain.json')['root'])
    pin = read(ROOT / 'Tools/android-family-signing.json')['certificateSha256']
    for apk in (current_path, new_apk):
        result = subprocess.run([str(tool / 'SDK/build-tools/36.0.0/apksigner.bat'), 'verify', '--print-certs', str(apk)],
                                env=dict(os.environ, JAVA_HOME=str(tool / 'OpenJDK')), capture_output=True, timeout=30)
        match = re.search(rb'Signer #1 certificate SHA-256 digest: ([a-f0-9]+)', result.stdout)
        require(result.returncode == 0 and match and match[1].decode() == pin, 'Installed/new signing identity mismatch')
    data = '/sdcard/Android/data/' + PACKAGE + '/files'
    require(adb('exec-out', 'cat', data + '/FamilyLAN/enrollment-status.txt').strip() == b'paired', 'Retained emulator enrollment required')
    adb('shell', 'am', 'force-stop', PACKAGE)
    # Preserve all externally accessible test files before installing. App-private
    # Keystore data is not read/exported and must survive the normal update.
    adb('pull', data, str(evidence / 'before-files'))
    saves = {str(p.relative_to(evidence / 'before-files')).replace('\\', '/'): sha(p.read_bytes())
             for p in (evidence / 'before-files/SoloPrototype').rglob('world.save*')}
    require(len(saves) >= 4, 'Expected original and paired solo saves before update')
    require('Success' in adb('install', '-r', str(new_apk)).decode(), 'Update installation failed')
    require(sha(installed()) == new_hash, 'Installed artifact is not the intended build')
    for name, expected in saves.items():
        require(sha(adb('exec-out', 'cat', data + '/' + name)) == expected, 'In-place update changed a retained save')
    adb('shell', 'monkey', '-p', PACKAGE, '-c', 'android.intent.category.LAUNCHER', '1')
    record = dict(passed=True, utc=datetime.now(timezone.utc).isoformat(), previousBuild=args.from_build, build=args.build,
                  previousApkSha256=old_hash, apkSha256=new_hash, certificateSha256=pin,
                  installedArtifactMatched=True, preservedSaveHashes=saves, operation='adb install -r',
                  avd=AVD, physicalDeviceTouched=False,
                  scope='Installation and pre-launch external-save retention only. UI, Keystore admission and recovery verified separately.')
    write(evidence / 'result.json', record)
    print('PASS: exact release update and retained save bytes; runtime acceptance is next. Evidence:', evidence, flush=True)


if __name__ == '__main__': main()
