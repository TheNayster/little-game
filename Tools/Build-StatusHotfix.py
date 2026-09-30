"""Recompile only baseline 227 networking with the disposable-observation fix."""
import hashlib
import json
from pathlib import Path
import shutil
import subprocess

from pc_server_installation import verify_bundle
from shared_garden_runtime import ROOT,write


def digest(raw):return hashlib.sha256(raw).hexdigest()


def main():
    baseline=ROOT/'Builds/NetworkProbe/G3-0.0.227';verify_bundle(baseline,'Server')
    folder=ROOT/'Builds/ServerHotfix/227-status-io-1'
    if folder.exists():raise RuntimeError('Use the existing verified hotfix or a fresh revision; never overwrite it')
    sources=folder/'Source';sources.mkdir(parents=True)
    rows=json.loads((baseline/'source-manifest.json').read_text(encoding='utf-8-sig'))['files']
    proof=[]
    for row in rows:
        if '/Code/NetworkProbe/' not in row['path'] or not row['path'].endswith('.cs'):continue
        found=None
        for line in subprocess.check_output(['git','rev-list','--objects','--all','--',row['path']],cwd=ROOT).decode().splitlines():
            parts=line.split(' ',1)
            if len(parts)!=2 or parts[1]!=row['path']:continue
            raw=subprocess.check_output(['git','cat-file','blob',parts[0]],cwd=ROOT)
            for value in (raw,raw.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n')):
                if digest(value)==row['sha256']:found=value;break
            if found is not None:break
        if found is None:raise RuntimeError('Exact baseline source unavailable: '+row['path'])
        (sources/Path(row['path']).name).write_bytes(found)
        proof.append(dict(path=row['path'],originalSha256=digest(found)))
    target=sources/'NetworkProbe.cs';text=target.read_text(encoding='utf-8')
    start=text.index('        private void WriteJson<T>');end=text.index('        private void WriteStatus',start)
    body='''        private void WriteJson<T>(string path,T value)
        {
            if(!DiagnosticFileWriter.TryWrite(path,JsonUtility.ToJson(value,true),Utf8))diagnosticWriteConflicts++;
        }
'''
    target.write_text(text[:start]+body+text[end:],encoding='utf-8')
    diagnostic=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Code/NetworkProbe/DiagnosticFileWriter.cs'
    shutil.copy2(diagnostic,sources/diagnostic.name)
    shutil.copytree(baseline/'Server',folder/'Server')
    managed=folder/'Server/LittleWeepsNetwork_Data/Managed';assembly=managed/'LittleWeeps.NetworkProbe.dll'
    flags=['/nologo','/target:library','/nostdlib+','/langversion:latest','/define:UNITY_STANDALONE_WIN,UNITY_STANDALONE,UNITY_64,UNITY_SERVER','/out:"'+str(assembly)+'"']
    flags+=['/reference:"'+str(p)+'"' for p in managed.glob('*.dll') if p!=assembly]
    flags+=['"'+str(p)+'"' for p in sources.glob('*.cs')]
    rsp=folder/'compile.rsp';rsp.write_text('\n'.join(flags),encoding='utf-8')
    editor=Path('C:/Program Files/Unity/Hub/Editor/6000.3.24f1/Editor/Data')
    result=subprocess.run([str(editor/'NetCoreRuntime/dotnet.exe'),str(editor/'DotNetSdkRoslyn/csc.dll'),'@'+str(rsp)],capture_output=True,text=True)
    (folder/'compile.log').write_text(result.stdout+result.stderr,encoding='utf-8')
    result.check_returncode()
    relative='Server/LittleWeepsNetwork_Data/Managed/LittleWeeps.NetworkProbe.dll'
    original=json.loads((baseline/'artifact-manifest.json').read_text(encoding='utf-8-sig'))
    artifacts=[dict(e,sha256=digest((folder/e['path']).read_bytes())) for e in original if e['path'].startswith('Server/')]
    changed=[e['path'] for e in artifacts if e['sha256']!=next(x['sha256']for x in original if x['path']==e['path'])]
    assert changed==[relative],changed
    write(folder/'artifact-manifest.json',artifacts)
    write(folder/'hotfix.json',dict(format=1,id='227-status-io-1',build=227,protocol=3,content=31,schema=30,
          changedArtifacts=changed,baselineSources=proof,diagnosticSourceSha256=digest(diagnostic.read_bytes()),assemblySha256=digest(assembly.read_bytes()),
          sourceChanges='Only WriteJson body plus DiagnosticFileWriter; all other baseline networking sources exact. Core/save/wire code unchanged.'))
    verify_bundle(folder,'Server')
    print('Built verified server-only status hotfix; only networking assembly changed; baseline source and gameplay retained.')


if __name__=='__main__':main()
