"""Verify a completed native G3 signing build before an in-place device update."""
import argparse
import json
import re
import subprocess
from mac_connection import ROOT, SSH, OPTIONS, HOST


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int)
    parser.add_argument('--device', required=True)
    parser.add_argument('--label', choices=('ipad7', 'ipad9'), required=True)
    args = parser.parse_args()
    assert 71 <= args.build <= 9999
    assert re.fullmatch(r'[A-Za-z0-9-]{20,64}', args.device)
    transfer = json.loads((ROOT/f'LocalData/ios-{args.build}-transfer.json').read_text())
    assert transfer['build'] == args.build
    data = dict(build=args.build, device=args.device, label=args.label)
    code = 'data=' + repr(data) + '\n' + r'''
import datetime,hashlib,json,pathlib,plistlib,subprocess
root=pathlib.Path.home()/'Developer/LittleWeeps';build=data['build']
folder=root/f'Builds/G3-0.0.{build}'
assert not (folder/'.native-build-lock').exists(),'Native build is still running'
logs=sorted((root/'Logs').glob(f'g3-ios-0.0.{build}-local-*.log'),key=lambda p:p.stat().st_mtime)
assert logs,'Missing native build log'
log=logs[-1]
assert log.with_suffix('.log.exit').read_text().strip()=='0','Native signed build did not succeed'
text=log.read_text(errors='replace');assert '** BUILD SUCCEEDED **' in text
app=folder/'DerivedData/Build/Products/Release-iphoneos/LittleWeeps.app'
subprocess.run(['codesign','--verify','--deep','--strict',str(app)],check=True,capture_output=True)
info=plistlib.loads((app/'Info.plist').read_bytes());bundle='com.littleweeps.familyplayset'
assert info['CFBundleIdentifier']==bundle and info['CFBundleVersion']==str(build)
assert info['CFBundleShortVersionString']==f'0.0.{build}'
assert info['UIDeviceFamily']==[1,2] and info['MinimumOSVersion']=='15.0'
assert info['NSBonjourServices']==['_lw-playset._udp'] and info['NSLocalNetworkUsageDescription']
profile=plistlib.loads(subprocess.check_output(['security','cms','-D','-i',str(app/'embedded.mobileprovision')],stderr=subprocess.DEVNULL))
entitlements=plistlib.loads(subprocess.check_output(['codesign','-d','--entitlements',':-',str(app)],stderr=subprocess.DEVNULL))
team='6QF59C684U';appid=team+'.'+bundle
assert profile['Entitlements']['application-identifier']==appid
assert entitlements['application-identifier']==appid and entitlements['com.apple.developer.team-identifier']==team
assert profile['TeamIdentifier']==[team]
assert profile['ExpirationDate']>datetime.datetime.now(datetime.timezone.utc).replace(tzinfo=None),'Profile expired'
assert data['device'] in profile.get('ProvisionedDevices',[]),'Requested device is not provisioned'
framework=app/'Frameworks/UnityFramework.framework/UnityFramework'
symbols=subprocess.check_output(['nm','-a',str(framework)],text=True)
defined={line.split()[-1] for line in symbols.splitlines() if len(line.split())>=3 and line.split()[-2] in ('t','T')}
names=['LWFamilyBrowse','LWFamilyPump','LWFamilyTake','LWFamilyStop','LWPairingAdd','LWPairingRead']
assert all('_'+name in defined for name in names),'Native bridge functions not linked'
files=[]
for f in sorted(app.rglob('*')):
    assert not f.is_symlink(),'Unexpected symlink in signed payload'
    if f.is_file():files.append(dict(path=f.relative_to(app).as_posix(),sha256=hashlib.sha256(f.read_bytes()).hexdigest()))
out=root/'Logs'/f'ipad-family-{build}';out.mkdir(exist_ok=True)
record=dict(build=build,version=info['CFBundleShortVersionString'],signedVerified=True,
    nativeCompileAndLinkPassed=True,bundle=bundle,applicationIdentifier=appid,minimumOS=info['MinimumOSVersion'],
    deviceFamilies=info['UIDeviceFamily'],profileExpires=profile['ExpirationDate'].isoformat()+'Z',
    filesHashed=len(files),app=str(app),installed=False,nativeSymbolsVerified=names,
    frameworkSha256=hashlib.sha256(framework.read_bytes()).hexdigest(),unityGeneratedWarningsPresent='warning:' in text)
record[data['label']+'Covered']=True
# Coverage for another device may only be retained from a check of these same
# signed bytes. Rebuilding the app invalidates any prior coverage record.
previous=out/'signed-verification.json'
manifest=folder/'signed-artifact-manifest.json'
if previous.exists() and manifest.exists() and json.loads(manifest.read_text())==files:
    old=json.loads(previous.read_text())
    for label in ('ipad7','ipad9'):
        if old.get(label+'Covered') is True:record[label+'Covered']=True
manifest.write_text(json.dumps(files,indent=2)+'\n')
previous.write_text(json.dumps(record,indent=2)+'\n');print(json.dumps(record))
'''
    result = subprocess.run([str(SSH), *OPTIONS, HOST, 'python3 -'], input=code,
                            text=True, capture_output=True, timeout=60)
    if result.returncode:
        raise RuntimeError(result.stderr + '\nSigned app not verified; no installation performed.')
    record = json.loads(result.stdout)
    (ROOT/f'LocalData/ios-{args.build}-signed-verified.json').write_text(json.dumps(record, indent=2)+'\n')
    print(json.dumps(record, indent=2))


if __name__ == '__main__':
    main()
