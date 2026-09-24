"""Compile the actual native adapters into an isolated emulator instrumentation package."""
import argparse, hashlib, json, os, re, subprocess, uuid, zipfile
from pathlib import Path

ROOT=Path(__file__).resolve().parent.parent
SDK=Path(os.environ['LOCALAPPDATA'])/'Android/Sdk'
ADB=SDK/'platform-tools/adb.exe'
TOOL=Path(json.loads((ROOT/'LocalData/android-toolchain.json').read_text())['root'])
JDK=TOOL/'OpenJDK'; BT=TOOL/'SDK/build-tools/36.0.0'; ANDROID=TOOL/'SDK/platforms/android-36/android.jar'

def run(args,**kwargs):
    result=subprocess.run([str(a) for a in args],capture_output=True,**kwargs)
    if result.returncode: raise RuntimeError(result.stdout.decode(errors='replace')[-2000:]+result.stderr.decode(errors='replace')[-2000:])
    return result.stdout

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--serial',required=True);p.add_argument('--family');args=p.parse_args()
    if not re.fullmatch(r'emulator-\d+',args.serial):raise ValueError('Isolated emulator only; never runs against a family device')
    run([ADB,'-s',args.serial,'wait-for-device'],timeout=30)
    root=ROOT/'LocalData/Verification'/('android-native-'+uuid.uuid4().hex);root.mkdir(parents=True)
    source=ROOT/'Unity/FamilyPlayset/Assets/Plugins/Android'
    sources=[source/'FamilyDiscovery.java',source/'FamilyEnrollment.java',ROOT/'Tools/AndroidBridgeTests/BridgeTests.java']
    (root/'classes').mkdir();(root/'dex').mkdir()
    env=dict(os.environ,JAVA_HOME=str(JDK))
    run([JDK/'bin/javac.exe','-source','8','-target','8','-classpath',ANDROID,'-d',root/'classes',*sources])
    run([JDK/'bin/jar.exe','cf',root/'classes.jar','-C',root/'classes','.'])
    run([BT/'d8.bat','--lib',ANDROID,'--min-api','26','--output',root/'dex',root/'classes.jar'],env=env)
    manifest='''<manifest xmlns:android="http://schemas.android.com/apk/res/android" package="com.littleweeps.familybridge.tests" android:versionCode="1" android:versionName="1"><uses-sdk android:minSdkVersion="26" android:targetSdkVersion="36"/><uses-permission android:name="android.permission.INTERNET"/><uses-permission android:name="android.permission.ACCESS_NETWORK_STATE"/><application android:label="Little Weeps native checks" android:debuggable="false" android:allowBackup="false"/><instrumentation android:name="com.littleweeps.family.tests.BridgeTests" android:targetPackage="com.littleweeps.familybridge.tests"/></manifest>'''
    (root/'AndroidManifest.xml').write_text(manifest)
    run([BT/'aapt.exe','package','-f','-M',root/'AndroidManifest.xml','-I',ANDROID,'-F',root/'unsigned.apk'])
    with zipfile.ZipFile(root/'unsigned.apk','a') as apk:apk.write(root/'dex/classes.dex','classes.dex')
    run([BT/'zipalign.exe','-f','4',root/'unsigned.apk',root/'aligned.apk'])
    # Standard emulator test identity only. The family signing key is never used here.
    key=Path.home()/'.android/debug.keystore'
    run([BT/'apksigner.bat','sign','--ks',key,'--ks-pass','pass:android','--out',root/'tests.apk',root/'aligned.apk'],env=env)
    run([ADB,'-s',args.serial,'install','-r',root/'tests.apk'])
    results=[]
    for phase in (['check','resume','pc'] if args.family else ['check','resume']):
        command=[ADB,'-s',args.serial,'shell','am','instrument','-w','-e','phase',phase]
        if phase=='pc':
            family=ROOT/'LocalData/FamilyLAN'/uuid.UUID(args.family).hex
            public=json.loads((family/'family.json').read_text());server=json.loads((family/'latest-server.json').read_text())
            for key,value in dict(family=public['familyId'],authority=public['authorityId'],world=public['worldId'],port=str(server['port'])).items():command+=['-e',key,value]
        command+=['com.littleweeps.familybridge.tests/com.littleweeps.family.tests.BridgeTests']
        output=run(command,timeout=100).decode(errors='replace')
        (root/(phase+'.txt')).write_text(output)
        if 'INSTRUMENTATION_CODE: -1' not in output or 'failure=' in output:raise RuntimeError(output[-3000:])
        match=re.search(r'INSTRUMENTATION_RESULT: result=(.+)',output)
        results.append(json.loads(match[1]))
    record=dict(passed=True,serial=args.serial,scope='Actual Java adapters in separate native test package; not Unity gameplay or physical LAN qualification',
                sources=[dict(path=str(x.relative_to(ROOT)),sha256=hashlib.sha256(x.read_bytes()).hexdigest()) for x in sources],
                count=sum(x['count'] for x in results),runs=results)
    (root/'result.json').write_text(json.dumps(record,indent=2));print(json.dumps(dict(count=record['count'],evidence=str(root/'result.json'))))

if __name__=='__main__':main()
