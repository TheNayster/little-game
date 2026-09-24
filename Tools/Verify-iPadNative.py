"""Verify an unsigned native G3 build without installing or using signing keys."""
import argparse
import json
import os
from pathlib import Path
import subprocess
from family_pairing import ROOT
from mac_connection import SSH,OPTIONS,HOST


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('build',type=int);args=parser.parse_args()
    assert 71<=args.build<=9999
    code=f'build={args.build}\n'+r'''
import pathlib,json,plistlib,hashlib,subprocess
root=pathlib.Path.home()/'Developer/LittleWeeps';folder=root/f'Builds/G3-0.0.{build}';logs=root/f'Logs/ipad-family-{build}'
if not (logs/'unsigned-build.exit').exists():
    print(json.dumps(dict(build=build,status='pending',nativeCompileAndLinkPassed=False)));raise SystemExit(0)
assert (logs/'unsigned-build.exit').read_text().strip()=='0','Native build failed; inspect its log'
text=(logs/'unsigned-build.log').read_text(errors='replace');assert '** BUILD SUCCEEDED **' in text
app=folder/'DerivedData/Build/Products/Release-iphoneos/LittleWeeps.app';info=plistlib.loads((app/'Info.plist').read_bytes())
assert info['CFBundleIdentifier']=='com.littleweeps.familyplayset' and info['CFBundleVersion']==str(build) and info['CFBundleShortVersionString']==f'0.0.{build}'
assert info['NSBonjourServices']==['_lw-playset._udp'] and info['NSLocalNetworkUsageDescription']
framework=app/'Frameworks/UnityFramework.framework/UnityFramework'
# IL2CPP __Internal calls are statically linked. Xcode keeps these functions
# local, so an exported/global-only symbol listing would wrongly reject them.
symbols=subprocess.check_output(['nm','-a',str(framework)],text=True)
names=['LWFamilyBrowse','LWFamilyPump','LWFamilyTake','LWFamilyStop','LWPairingAdd','LWPairingRead']
defined={line.split()[-1] for line in symbols.splitlines() if len(line.split())>=3 and line.split()[-2] in ('t','T')}
assert all('_'+name in defined for name in names),'Native bridge functions not linked'
record=dict(build=build,status='unsigned-verified',version=info['CFBundleShortVersionString'],bundleId=info['CFBundleIdentifier'],nativeCompileAndLinkPassed=True,xcodeExit=0,signing='CODE_SIGNING_ALLOWED=NO',installed=False,minimumOS=info['MinimumOSVersion'],deviceFamilies=info['UIDeviceFamily'],nativeSymbolsVerified=names,frameworkSha256=hashlib.sha256(framework.read_bytes()).hexdigest(),bonjourServices=info['NSBonjourServices'],unityGeneratedWarningsPresent='warning:' in text)
(logs/'unsigned-verification.json').write_text(json.dumps(record,indent=2));print(json.dumps(record))
'''
    result=subprocess.run([str(SSH),*OPTIONS,HOST,'python3 -'],input=code,text=True,capture_output=True,timeout=30)
    if result.returncode:raise RuntimeError(result.stderr)
    record=json.loads(result.stdout)
    if record['nativeCompileAndLinkPassed']:(ROOT/f'LocalData/ios-{args.build}-native-verified.json').write_text(json.dumps(record,indent=2))
    print(json.dumps(record,indent=2))


if __name__=='__main__':main()
