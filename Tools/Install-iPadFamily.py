"""Install a verified signed export over this app, preserving a verified pre-update backup."""
import argparse,json,subprocess
from pathlib import Path
from mac_connection import ROOT,SSH,OPTIONS,HOST


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--build',type=int,required=True)
    p.add_argument('--backup',type=Path,required=True)
    p.add_argument('--device-timeout',type=int,default=60,help='Seconds per device operation; wireless data copies can take longer.')
    args=p.parse_args();assert 60<=args.device_timeout<=180
    backup=args.backup.resolve();assert backup.is_relative_to((ROOT/'LocalData/iPadBackups').resolve())
    record=json.loads((backup/'backup.json').read_text(encoding='utf-8'))
    assert record['passed'] and record['forBuild']==args.build
    import hashlib
    for e in record['files']:
        f=(backup/e['path']).resolve();assert f.is_relative_to(backup) and hashlib.sha256(f.read_bytes()).hexdigest()==e['sha256']
    data=dict(build=args.build,backup=record,deviceTimeout=args.device_timeout)
    code='data='+repr(data)+'\n'+r'''
import pathlib,subprocess,json,hashlib,plistlib,uuid
root=pathlib.Path.home()/'Developer/LittleWeeps';build=data['build'];before=data['backup'];device=before['device']
folder=root/'Builds'/('G3-0.0.'+str(build));signed=json.loads((root/'Logs'/('ipad-family-'+str(build))/'signed-verification.json').read_text())
assert signed['signedVerified'] and signed[before['label']+'Covered'] and signed['build']==build
app=pathlib.Path(signed['app']);bundle='com.littleweeps.familyplayset'
assert not (folder/'.native-build-lock').exists(),'Build still running'
for e in json.loads((folder/'signed-artifact-manifest.json').read_text()):
    f=(app/e['path']).resolve();assert f.is_relative_to(app) and hashlib.sha256(f.read_bytes()).hexdigest()==e['sha256'],'Signed artifact changed'
r=subprocess.run(['codesign','--verify','--deep','--strict',str(app)],capture_output=True);assert r.returncode==0
out=root/'Logs'/('ipad-family-'+str(build))/(before['label']+'-install-'+uuid.uuid4().hex);out.mkdir()
def run(name,cmd,required=True):
    result=out/(name+'.json')
    # Launch accepts trailing app arguments; options after its bundle ID would
    # be passed into the game instead of being parsed by devicectl.
    r=subprocess.run(['xcrun','devicectl',*cmd[:3],'--device',device,'--timeout',str(data['deviceTimeout']),'--json-output',str(result),*cmd[3:]],capture_output=True,text=True)
    (out/(name+'.log')).write_text(r.stdout+r.stderr)
    if required:assert r.returncode==0,name+' failed; no destructive fallback permitted'
    return json.loads(result.read_text()) if result.exists() else {'error':{'message':'No CoreDevice JSON result'}}
inventory=run('inventory-before',['device','info','apps','--bundle-id',bundle],False)
apps=inventory.get('result',{}).get('apps',[])
already=any(a.get('bundleIdentifier')==bundle and str(a.get('bundleVersion'))==str(build) for a in apps)
if already:raise RuntimeError('Already current; no reinstall performed')
installation=run('install',['device','install','app',str(app)])
installed=installation.get('result',{}).get('installedApplications',[])
assert any(a.get('bundleID')==bundle for a in installed),'Installation did not identify this bundle'
run('documents-after',['device','copy','from','--domain-type','appDataContainer','--domain-identifier',bundle,'--source','Documents','--destination',str(out/'Documents')])
run('preferences-after',['device','copy','from','--domain-type','appDataContainer','--domain-identifier',bundle,'--source','Library/Preferences/'+bundle+'.plist','--destination',str(out/'preferences.plist')])
worlds=[e for e in before['files'] if e['path'].startswith('Documents/')]
for e in worlds:
    assert hashlib.sha256((out/e['path']).read_bytes()).hexdigest()==e['sha256'],'Existing document changed during update'
prefs_before=plistlib.loads((pathlib.Path(before['remoteBackup'])/'preferences.plist').read_bytes())
prefs_after=plistlib.loads((out/'preferences.plist').read_bytes())
assert all(prefs_after.get(k)==v for k,v in prefs_before.items()),'Existing preference changed during update'
inventory_after=run('inventory-after',['device','info','apps','--bundle-id',bundle],False)
launch=run('launch',['device','process','launch',bundle])
result=dict(build=build,label=before['label'],device=device,installed=True,signedArtifactVerified=True,documentsPreserved=len(worlds),allExistingPreferencesPreserved=True,
    inventoryAvailable='result' in inventory_after,launchResult=launch.get('result',{}),remoteEvidence=str(out),backup=before['remoteBackup'])
(out/'result.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
'''
    r=subprocess.run([str(SSH),*OPTIONS,HOST,'python3 -'],input=code,text=True,capture_output=True,timeout=max(300,8*args.device_timeout+30))
    if r.returncode:raise RuntimeError(r.stderr+'\nInspect the install evidence; never uninstall or clear data to retry.')
    result=json.loads(r.stdout);out=ROOT/'LocalData'/('ios-'+str(args.build)+'-'+record['label']+'-installed.json')
    out.write_text(json.dumps(result,indent=2),encoding='utf-8');print(json.dumps(result))


if __name__=='__main__':main()
