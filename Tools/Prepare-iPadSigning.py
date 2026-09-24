"""Prepare a local Mac signing shortcut for an already verified G3 export.

Does not unlock the keychain or install an app. --check-unsigned starts a native
compile/link check only; an unsigned result must never be reported as installed.
"""
import argparse
import json
import os
from pathlib import Path
import subprocess
from family_pairing import ROOT


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build',type=int);parser.add_argument('--check-unsigned',action='store_true');args=parser.parse_args()
    assert 71<=args.build<=9999
    transfer=json.loads((ROOT/f'LocalData/ios-{args.build}-transfer.json').read_text())
    assert transfer['build']==args.build
    data=dict(build=args.build,helper=(ROOT/'Tools/Build-iOS-Mac.sh').read_text(),check=args.check_unsigned)
    code=f'data=__import__("json").loads({json.dumps(data)!r})\n'+r'''
import hashlib,json,os,pathlib,plistlib,subprocess,time
root=pathlib.Path.home()/'Developer/LittleWeeps';build=data['build'];folder=root/f'Builds/G3-0.0.{build}'
manifest=json.loads((folder/'artifact-manifest.json').read_text(encoding='utf-8-sig'))
for entry in manifest:
    path=(folder/entry['path']).resolve()
    assert path.is_relative_to(folder) and hashlib.sha256(path.read_bytes()).hexdigest()==entry['sha256'],'Export changed'
info=plistlib.loads((folder/'Xcode/Info.plist').read_bytes())
assert info['CFBundleIdentifier'] in ('com.littleweeps.familyplayset','${PRODUCT_BUNDLE_IDENTIFIER}') and info['CFBundleVersion']==str(build)
assert 'PRODUCT_BUNDLE_IDENTIFIER = com.littleweeps.familyplayset;' in (folder/'Xcode/Unity-iPhone.xcodeproj/project.pbxproj').read_text()
assert info['NSBonjourServices']==['_lw-playset._udp'] and info['NSLocalNetworkUsageDescription']
assert not (folder/'.native-build-lock').exists(),'Another native build holds this export'
scripts=root/'Scripts';scripts.mkdir(exist_ok=True)
helper=scripts/f'Build-iOS-G3-{build}.sh';helper.write_text(data['helper']);helper.chmod(0o755)
shortcut=pathlib.Path.home()/f'Desktop/Finish-Little-Weeps-{build}.command'
shortcut.write_text('#!/bin/sh\n'+f'"{helper}" {build} 6QF59C684U 00008030-000C6DCE01F8C02E G3\n'+
    'result=$?\nprintf "\\nKeep this window open. Press Return to close it.\\n"\nread reply\nexit "$result"\n');shortcut.chmod(0o755)
record=dict(build=build,exportFilesVerified=len(manifest),helper=str(shortcut),installsApp=False)
if data['check']:
    logs=root/f'Logs/ipad-family-{build}';logs.mkdir(exist_ok=False)
    lock=folder/'.native-build-lock';lock.mkdir()
    # An exclusive lock prevents the local signing shortcut sharing Xcode's DB.
    runner=logs/'compile.py'
    runner.write_text('import pathlib,subprocess\n'+
        'folder=pathlib.Path('+repr(str(folder))+');logs=pathlib.Path('+repr(str(logs))+')\n'+
        'try:\n'+
        '    with (logs/"unsigned-build.log").open("w") as log:\n'+
        '        code=subprocess.call(["xcodebuild","-project",str(folder/"Xcode/Unity-iPhone.xcodeproj"),"-scheme","Unity-iPhone","-configuration","Release","-destination","generic/platform=iOS","-derivedDataPath",str(folder/"DerivedData"),"CODE_SIGNING_ALLOWED=NO","build"],stdout=log,stderr=subprocess.STDOUT)\n'+
        '    (logs/"unsigned-build.exit").write_text(str(code))\n'+
        'finally:\n'+
        '    (folder/".native-build-lock/pid").unlink(missing_ok=True)\n'+
        '    (folder/".native-build-lock").rmdir()\n')
    proc=subprocess.Popen(['python3',str(runner)],stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,start_new_session=True)
    (lock/'pid').write_text(str(proc.pid));record['unsignedCheckPid']=proc.pid;record['logs']=str(logs)
    (logs/'prepare.json').write_text(json.dumps(record,indent=2))
print(json.dumps(record))
'''
    ssh=Path(os.environ['WINDIR'])/'System32/OpenSSH/ssh.exe'
    result=subprocess.run([str(ssh),'-i',str(Path.home()/'.ssh/little_weeps_mac_ed25519'),'-o','IdentitiesOnly=yes','-o','BatchMode=yes','-o','ConnectTimeout=8','nayster@eduardos-mbp.lan','python3 -'],input=code,text=True,capture_output=True,timeout=45)
    if result.returncode:raise RuntimeError(result.stderr)
    record=json.loads(result.stdout);(ROOT/f'LocalData/ios-{args.build}-signing-prepared.json').write_text(json.dumps(record,indent=2))
    print(result.stdout.strip())


if __name__=='__main__':main()
