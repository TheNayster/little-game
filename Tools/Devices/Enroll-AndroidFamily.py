"""Parent development enrollment through an already authorized USB/ADB connection.

Verifies the exact installed family-signed artifact. Transfers only one player's
record through stdin; never prints or stages a plaintext credential on Windows.
The app consumes the inbox into Android Keystore on its next foreground launch.
"""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse, hashlib, json, os, re, subprocess, uuid
from pathlib import Path
from family_pairing import ROOT, read_record

ADB=Path(os.environ['LOCALAPPDATA'])/'Android/Sdk/platform-tools/adb.exe'
PACKAGE='com.littleweeps.familyplayset'
def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--serial',required=True);p.add_argument('--family',required=True)
    p.add_argument('--player',type=int,choices=range(1,5),required=True);p.add_argument('--build',type=int,required=True)
    args=p.parse_args()
    if args.build<76 or not re.fullmatch(r'[a-zA-Z0-9_.:\-]+',args.serial) or uuid.UUID(args.family).hex!=args.family:raise ValueError('Invalid enrollment target')
    base=[str(ADB),'-s',args.serial]
    def adb(*command,input=None):
        result=subprocess.run(base+list(command),input=input,capture_output=True,timeout=60)
        if result.returncode:raise RuntimeError('ADB step failed; enrollment was not confirmed')
        return result.stdout
    adb('wait-for-device')
    folder=ROOT/f'Builds/AndroidSigned/G3-0.0.{args.build}'
    manifest=json.loads((folder/'artifact-manifest.json').read_text(encoding='utf-8-sig'))
    if isinstance(manifest,list):manifest=next(x for x in manifest if x['path']=='LittleWeeps.apk')
    apk=folder/'LittleWeeps.apk';expected=manifest['sha256']
    if hashlib.sha256(apk.read_bytes()).hexdigest()!=expected:raise RuntimeError('Local APK changed')
    summary=json.loads((folder/'build-summary.json').read_text(encoding='utf-8-sig'))
    if summary['version']!=f'0.0.{args.build}' or summary['development'] or summary['result']!='Succeeded':raise RuntimeError('Unexpected build')
    tool=Path(json.loads((ROOT/'LocalData/android-toolchain.json').read_text())['root'])
    result=subprocess.run([str(tool/'SDK/build-tools/36.0.0/apksigner.bat'),'verify','--print-certs',str(apk)],env=dict(os.environ,JAVA_HOME=str(tool/'OpenJDK')),capture_output=True,timeout=30)
    pin=json.loads((ROOT/'Tools/android-family-signing.json').read_text())['certificateSha256']
    match=re.search(rb'Signer #1 certificate SHA-256 digest: ([a-f0-9]+)',result.stdout)
    if result.returncode or not match or match[1].decode()!=pin:raise RuntimeError('Family signing identity mismatch')
    installed=adb('shell','pm','path',PACKAGE).decode().strip()
    if not re.fullmatch(r'package:/data/app/[a-zA-Z0-9_+=~/\-.]+/base\.apk',installed):raise RuntimeError('Expected one installed base APK')
    if hashlib.sha256(adb('exec-out','cat',installed[8:])).hexdigest()!=expected:raise RuntimeError('Installed app is not the requested artifact')
    location='/sdcard/Android/data/'+PACKAGE+'/files/FamilyLAN'
    # Do not replace an established identity or an unresolved earlier inbox.
    status=adb('shell',f'if [ -f {location}/enrollment-status.txt ]; then cat {location}/enrollment-status.txt; fi').decode().strip()
    if status!='unpaired':raise RuntimeError('Launch the unpaired app first, or review its existing enrollment; no credentials replaced')
    pair=read_record(ROOT/'LocalData/FamilyLAN'/args.family/f'player-{args.player}.pairing')
    if pair.get('role')!='client' or pair.get('worldId')!=args.family or pair.get('privateKey') or pair.get('members'):raise RuntimeError('Player enrollment required')
    adb('shell','am','force-stop',PACKAGE)
    # noclobber prevents overwriting a prior staged operation; rename exposes a complete inbox.
    command=f'mkdir -p {location} && test ! -e {location}/enrollment.json && (set -C; cat > {location}/enrollment.pending) && mv {location}/enrollment.pending {location}/enrollment.json'
    adb('shell',command,input=json.dumps(pair,separators=(',',':')).encode())
    adb('shell','monkey','-p',PACKAGE,'-c','android.intent.category.LAUNCHER','1')
    print(json.dumps(dict(staged=True,build=args.build,player=args.player,next='Check enrollment-status.txt is paired and the inbox was consumed; staging alone is not successful enrollment')))

if __name__=='__main__':main()
