"""Inspect matched G3 mobile artifacts on Windows; never contacts devices or a Mac.

This verifies export/source identity and bridge inclusion, not native iOS linking,
Android page-size runtime support, installed signing, saves, or mobile recovery.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import plistlib
import re
import zipfile


ROOT = Path(__file__).resolve().parent.parent


def sha(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def require(value, message):
    if not value:
        raise RuntimeError(message)


def artifacts(folder):
    manifest = read(folder / 'artifact-manifest.json')
    entries = manifest if isinstance(manifest, list) else [manifest]
    require(entries and len({e['path'] for e in entries}) == len(entries), 'Empty/duplicate artifact manifest')
    for entry in entries:
        path = (folder / entry['path']).resolve()
        require(path.is_relative_to(folder.resolve()), 'Artifact path escaped its build directory')
        require(path.is_file() and sha(path) == entry['sha256'], f"Changed artifact: {entry['path']}")
    return entries


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int)
    parser.add_argument('--windows-build', type=int, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    require(91 <= args.build <= 9999 and 91 <= args.windows_build <= 9999, 'Recovery-capable G3 builds required')
    require(not args.output.exists(), 'Use a new evidence output')
    android = ROOT / f'Builds/AndroidSigned/G3-0.0.{args.build}'
    ios = ROOT / f'Builds/iOSFamilyLAN/G3-0.0.{args.build}'
    windows = ROOT / f'Builds/NetworkProbe/G3-0.0.{args.windows_build}'
    manifests = [read(folder / 'source-manifest.json') for folder in (windows, android, ios)]
    sources = [{e['path']: e['sha256'] for e in m['files']} for m in manifests]
    csharp = {p: h for p, h in sources[0].items() if p.endswith('.cs') and p.startswith('Unity/')}
    require(csharp, 'Missing qualified Windows C# provenance')
    for current in sources[1:]:
        require({p: h for p, h in current.items() if p.endswith('.cs') and p.startswith('Unity/')} == csharp,
                'Mobile C# source set differs from the selected Windows build')
    bridges = ['Unity/FamilyPlayset/Assets/Plugins/iOS/LittleWeepsFamily.mm',
               'Unity/FamilyPlayset/Assets/Plugins/Android/FamilyDiscovery.java',
               'Unity/FamilyPlayset/Assets/Plugins/Android/FamilyEnrollment.java']
    shared = [*csharp, *bridges, 'Unity/FamilyPlayset/Packages/manifest.json', 'Unity/FamilyPlayset/Packages/packages-lock.json']
    for path in shared:
        digest = sha(ROOT / path)
        require(all(s.get(path) == digest for s in sources), f'Compiled source/configuration changed: {path}')
    a, i = read(android / 'build-summary.json'), read(ios / 'build-summary.json')
    for summary, platform, profile in ((a, 'Android', 'Android'), (i, 'iOS', 'iPad')):
        require(summary['result'] == 'Succeeded' and not summary['development'] and summary['errors'] == 0, 'Build failed or is development')
        require(summary['version'] == f'0.0.{args.build}' and summary['platform'] == platform, 'Build version/platform changed')
        require(summary['profile'] == f'Assets/BuildProfiles/{profile} Family LAN.asset', 'Wrong mobile profile')
    require(a['unity'] == i['unity'] == '6000.3.24f1', 'Pinned Unity version changed')
    android_entries, ios_entries = artifacts(android), artifacts(ios)
    require({e['path'] for e in android_entries} == {'LittleWeeps.apk'}, 'Unexpected signed output')
    actual_export = {p.relative_to(ios).as_posix() for p in (ios / 'Xcode').rglob('*') if p.is_file()}
    require(actual_export == {e['path'] for e in ios_entries}, 'iOS export file set changed')
    signing = read(android / 'signing-evidence.json')
    pin = read(ROOT / 'Tools/android-family-signing.json')
    require(signing['certificateSha256'] == pin['certificateSha256'] and signing['payloadUnchanged'], 'Android signing/payload mismatch')
    with zipfile.ZipFile(android / 'LittleWeeps.apk') as apk:
        dex = b''.join(apk.read(name) for name in apk.namelist() if re.fullmatch(r'classes\d*\.dex', name))
        for bridge in ('FamilyDiscovery', 'FamilyEnrollment'):
            require(f'Lcom/littleweeps/family/{bridge};'.encode() in dex, f'Android native bridge missing: {bridge}')
    xcode = ios / 'Xcode'
    info = plistlib.loads((xcode / 'Info.plist').read_bytes())
    require(info['CFBundleVersion'] == str(args.build) and info['CFBundleShortVersionString'] == i['version'], 'iOS plist version changed')
    require(info['NSBonjourServices'] == ['_lw-playset._udp'] and info['NSLocalNetworkUsageDescription'], 'Local discovery declarations missing')
    require('arm64' in info['UIRequiredDeviceCapabilities'], 'Missing ARM64 device capability')
    orientations = info['UISupportedInterfaceOrientations']
    require(orientations and all('Landscape' in name for name in orientations), 'Unexpected orientation')
    project = (xcode / 'Unity-iPhone.xcodeproj/project.pbxproj').read_text(encoding='utf-8')
    for text in ('PRODUCT_BUNDLE_IDENTIFIER = com.littleweeps.familyplayset;', 'TARGETED_DEVICE_FAMILY = "1,2";',
                 'SUPPORTED_PLATFORMS = iphoneos;', 'IPHONEOS_DEPLOYMENT_TARGET = 15.0;', 'Security.framework in Frameworks'):
        require(text in project, f'Missing iOS identity/target requirement: {text}')
    require(re.search(r'LittleWeepsFamily\.mm in Sources[^\n]+COMPILER_FLAGS = "-fobjc-arc"', project), 'Native bridge ARC/source configuration missing')
    require(sha(xcode / 'Libraries/Plugins/iOS/LittleWeepsFamily.mm') == sha(ROOT / bridges[0]), 'Exported native bridge changed')
    generated = xcode / 'Il2CppOutputProject/Source/il2cppOutput'
    symbols = {
        'LittleWeeps.NetworkProbe.cpp': ['NetworkProbe_TickInterruptedArchives', 'NetworkProbe_TryLocalContinuation', 'NetworkProbe_TickRecovery'],
        'LittleWeeps.Adapters.cpp': ['CheckpointStore_Save', 'RecoveryReplica_Commit', 'ContinuationLibrary'],
        'LittleWeeps.Core.cpp': ['OutageClock', 'SoloWorld'],
    }
    for file, expected in symbols.items():
        text = (generated / file).read_text(encoding='utf-8-sig')
        require(all(symbol in text for symbol in expected), f'Missing generated recovery code: {file}')
    result = dict(passed=True, utc=datetime.now(timezone.utc).isoformat(), build=args.build, windowsBuild=args.windows_build,
                  sourceCommits=[m['sourceCommit'] for m in manifests], currentCSharpFilesMatched=len(csharp),
                  bridgesAndPackageLocksMatch=True, unity=i['unity'], identity='com.littleweeps.familyplayset',
                  android=dict(artifactFilesVerified=len(android_entries), apkSha256=sha(android / 'LittleWeeps.apk'),
                               nativeBridgeClassesPresent=True, signingCertificateSha256=pin['certificateSha256'],
                               signatureVerification='Run Test-AndroidArtifact.ps1 separately; signing log also retained',
                               payloadUnchanged=True, installed=False, page16KbRuntimeTested=False),
                  ios=dict(artifactFilesVerified=len(ios_entries), minimumIOS='15.0', deviceFamilies=['iPhone', 'iPad'],
                           orientations=orientations, bonjourServices=info['NSBonjourServices'],
                           nativeBridgeSourceAndARC=True, il2cppRecoveryCodeGenerated=True,
                           nativeCompileAndLinkPassed=False, signed=False, installed=False),
                  physicalDevicesAccessed=False, macAccessed=False, familyServerUpdated=False,
                  limitation='Windows artifact preparation only. Separate Android static/runtime gates, Mac native compile/sign and physical save/recovery qualification remain open.')
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open('x', encoding='utf-8') as stream:
        json.dump(result, stream, indent=2)
        stream.write('\n')
    print(json.dumps(result, indent=2))


if __name__ == '__main__':
    main()
