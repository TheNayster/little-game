"""Install the pinned Android SDK tools in this user's Little Weeps toolchain folder.

Does not install Unity platform modules or modify administrator-owned editor files.
Uses the Hub cache where possible and verifies every archive before extraction.
"""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import urllib.request
import zipfile


def digest(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def register_sdk(target, project):
    sdk = target / 'SDK'
    manager = sdk / 'cmdline-tools/16.0/bin/sdkmanager.bat'
    # These modern archives need SDK Manager registration. Preserve the raw
    # extraction outside SDK before asking SDK Manager to install the same version.
    for relative, name in [('cmdline-tools/16.0', 'cmdline-tools-16.0'), ('cmake/3.22.1', 'cmake-3.22.1')]:
        folder = sdk / relative
        if (folder / 'package.xml').exists():
            continue
        backup = target / 'PreparedArchiveBackup' / name
        for path in (folder, backup):
            if not path.resolve().is_relative_to(target.resolve()):
                raise RuntimeError('Unsafe toolchain move')
        if backup.exists():
            raise RuntimeError('Unregistered package and existing backup need inspection: ' + str(folder))
        backup.parent.mkdir(parents=True, exist_ok=True)
        folder.rename(backup)
        if name == 'cmdline-tools-16.0':
            manager = backup / 'bin/sdkmanager.bat'
    environment = os.environ.copy()
    environment['JAVA_HOME'] = str(target / 'OpenJDK')
    logs = project / 'LocalData/Logs'
    logs.mkdir(parents=True, exist_ok=True)
    for action, arguments in [('licenses', ['--licenses']), ('register', ['cmdline-tools;16.0', 'cmake;3.22.1']), ('installed', ['--list_installed'])]:
        with (logs / ('android-sdk-' + action + '.log')).open('w') as log:
            subprocess.run([str(manager), '--sdk_root=' + str(sdk), *arguments],
                           env=environment, input='y\n' * 20, text=True,
                           stdout=log, stderr=subprocess.STDOUT, check=True)


def main():
    project = PROJECT_ROOT
    manifest = json.loads((project / 'Tools/android-toolchain-6000.3.24f1.json').read_text())
    target = Path(os.environ['LOCALAPPDATA']) / 'LittleWeeps/Toolchains' / ('Unity-' + manifest['unity'])
    cache = Path(os.environ['APPDATA']) / 'UnityHub/downloads'
    downloads = project / 'LocalData/Downloads/Android'
    records = []
    for item in manifest['archives']:
        name = item['url'].rsplit('/', 1)[-1]
        archive = cache / name
        if not archive.exists():
            downloads.mkdir(parents=True, exist_ok=True)
            archive = downloads / name
            if not archive.exists():
                urllib.request.urlretrieve(item['url'], archive)
        if archive.stat().st_size != item['bytes'] or digest(archive) != item['sha256']:
            raise RuntimeError('Archive failed verification: ' + str(archive))
        destination = target / item['destination']
        with zipfile.ZipFile(archive) as source:
            for entry in source.infolist():
                name = entry.filename
                prefix = item['stripPrefix']
                if prefix:
                    if not name.startswith(prefix):
                        raise RuntimeError('Unexpected archive prefix: ' + name)
                    name = name[len(prefix):]
                output = (destination / name).resolve()
                if not output.is_relative_to(target.resolve()):
                    raise RuntimeError('Unsafe archive entry: ' + name)
                if entry.is_dir():
                    output.mkdir(parents=True, exist_ok=True)
                    continue
                output.parent.mkdir(parents=True, exist_ok=True)
                if output.exists():
                    with source.open(entry) as contents:
                        expected = hashlib.file_digest(contents, 'sha256').hexdigest()
                    if digest(output) != expected:
                        raise RuntimeError('Existing toolchain file differs: ' + str(output))
                else:
                    pending = output.with_name(output.name + '.littleweeps-partial')
                    with source.open(entry) as contents, pending.open('wb') as stream:
                        shutil.copyfileobj(contents, stream)
                    pending.replace(output)
        records.append({'id': item['id'], 'sha256': item['sha256'], 'destination': str(destination)})
        print('Prepared ' + item['id'], flush=True)
    register_sdk(target, project)
    for package in ['build-tools/36.0.0', 'platform-tools', 'platforms/android-36', 'cmdline-tools/16.0', 'cmake/3.22.1']:
        if not (target / 'SDK' / package / 'package.xml').is_file():
            raise RuntimeError('SDK Manager registration is missing: ' + package)
    evidence = {'unity': manifest['unity'], 'root': str(target), 'archives': records, 'sdkManagerRegistered': True}
    (project / 'LocalData').mkdir(exist_ok=True)
    (project / 'LocalData/android-toolchain.json').write_text(json.dumps(evidence, indent=2) + '\n')
    print('Toolchain ready at ' + str(target), flush=True)


if __name__ == '__main__':
    main()
