"""Build the exact Apple bridge on the paired Mac and discover a Windows family."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import uuid
from family_pairing import ROOT


def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--family',required=True);parser.add_argument('--port',type=int,required=True);args=parser.parse_args()
    world=uuid.UUID(args.family).hex;assert world==args.family and 1024<=args.port<=65535
    public=json.loads((ROOT/'LocalData/FamilyLAN'/world/'family.json').read_text())
    relative='Assets/Plugins/iOS/LittleWeepsFamily.mm'
    source=ROOT/'Unity/FamilyPlayset'/relative;harness=ROOT/'Tools/Verification/Test-AppleFamilyBridge.mm'
    ssh=Path(os.environ['WINDIR'])/'System32/OpenSSH/ssh.exe'
    remote=f'/Users/nayster/Developer/LittleWeeps/NativeTests/apple-bridge-{uuid.uuid4().hex}'
    data=dict(source=source.read_text(),harness=harness.read_text(),family=public['familyId'],authority=public['authorityId'],world=world,port=args.port)
    # Only public discovery identities/source are sent; no enrollment token.
    script='import json,pathlib,subprocess\n'
    script+=f'r=pathlib.Path({remote!r});r.mkdir(parents=True)\ndata=json.loads({json.dumps(data)!r})\n'
    script+='''(r/'bridge.mm').write_text(data['source']);(r/'probe.mm').write_text(data['harness'])
sdk=subprocess.check_output(['xcrun','--sdk','iphoneos','--show-sdk-path'],text=True).strip()
subprocess.run(['xcrun','clang++','-std=c++17','-fobjc-arc','-Wall','-Wextra','-fsyntax-only','-isysroot',sdk,'-target','arm64-apple-ios15.0',str(r/'bridge.mm')],check=True)
subprocess.run(['xcrun','--sdk','macosx','clang++','-std=c++17','-fobjc-arc',str(r/'bridge.mm'),str(r/'probe.mm'),'-framework','Foundation','-framework','Security','-o',str(r/'probe')],check=True)
result=subprocess.check_output([str(r/'probe'),data['authority'],data['family'],data['world'],str(data['port'])],text=True,timeout=50)
(r/'result.json').write_text(result);print(result)
'''
    result=subprocess.run([str(ssh),'-i',str(Path.home()/'.ssh/little_weeps_mac_ed25519'),'-o','IdentitiesOnly=yes','-o','BatchMode=yes','-o','ConnectTimeout=8','nayster@eduardos-mbp.lan','python3 -'],input=script,text=True,capture_output=True,timeout=90)
    if result.returncode:raise RuntimeError(result.stderr)
    record=json.loads(result.stdout);record['sourceSha256']=hashlib.sha256(source.read_bytes()).hexdigest();record['remoteEvidence']=remote
    output=ROOT/'LocalData/Verification'/f'apple-bridge-{uuid.uuid4().hex}.json';output.parent.mkdir(parents=True,exist_ok=True)
    output.write_text(json.dumps(record,indent=2));print(json.dumps(record));print('Evidence:',output)


if __name__=='__main__':main()
