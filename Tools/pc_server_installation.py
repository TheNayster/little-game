"""One persistent PC server bundle, endpoint and recorded release; no credentials here."""
from contextlib import contextmanager
from datetime import datetime, timezone
import hashlib
import json
import msvcrt
from pathlib import Path
import shutil
import socket
import stat
import subprocess
import time
import uuid

from server_release import ReleaseError, load, network_release
from shared_garden_runtime import write


def home(root):
    return Path(root) / 'LocalData/PCServer'


def executable(root):
    return home(root) / 'current/Server/LittleWeepsNetwork.exe'


@contextmanager
def installation_lock(root):
    folder = home(root); folder.mkdir(parents=True, exist_ok=True)
    with (folder / 'installation.lock').open('a+b') as file:
        if file.tell() == 0: file.write(b'0'); file.flush()
        file.seek(0)
        try: msvcrt.locking(file.fileno(), msvcrt.LK_NBLCK, 1)
        except OSError as error: raise ReleaseError('Another PC-server installation/start is in progress') from error
        try: yield
        finally: file.seek(0); msvcrt.locking(file.fileno(), msvcrt.LK_UNLCK, 1)


def installed(root):
    path = home(root) / 'installation.json'
    if (home(root) / 'installation.pending.json').exists():
        raise ReleaseError('Interrupted server installation needs recovery; no server was started')
    if not path.exists(): return None
    value = load(path)
    if value.get('format') != 1 or uuid.UUID(value['family']).hex != value['family'] or not 1024 <= value['port'] <= 65535:
        raise ReleaseError('Invalid PC-server installation record')
    if Path(value['program']).resolve() != executable(root).resolve():
        raise ReleaseError('Server installation path changed')
    release = network_release(root, value['build'])
    if any(value.get(k) != release[k] for k in ('protocol', 'content', 'schema', 'sharedDigest')):
        raise ReleaseError('Installed contract does not match its source release')
    return value


def selected(root, family):
    value = installed(root)
    if value and value['family'] == family: return value['build']
    latest = load(Path(root) / 'LocalData/FamilyLAN' / family / 'latest-server.json')
    return latest['build']


def bundle(root, family, build):
    value = installed(root)
    if value and value['family'] == family:
        if value['build'] != build:
            raise ReleaseError('Use the selected installed server; an app build must not change it')
        return home(root) / 'current', value
    return Path(root) / f'Builds/NetworkProbe/G3-0.0.{build}', None


def verify_bundle(folder, role=None):
    entries = load(Path(folder) / 'artifact-manifest.json')
    found = set()
    for entry in entries:
        relative = entry['path']
        parts = relative.split('/')
        if not relative or any(p in ('', '.', '..') for p in parts) or '\\' in relative or ':' in relative or relative in found:
            raise ReleaseError('Invalid release artifact path')
        if role and not entry['path'].startswith(role + '/'): continue
        original = Path(folder) / relative
        if any(p.is_symlink() or getattr(p.lstat(), 'st_file_attributes', 0) & stat.FILE_ATTRIBUTE_REPARSE_POINT for p in (original, *original.parents) if p.is_relative_to(Path(folder))):
            raise ReleaseError('Linked release artifact refused')
        path = (Path(folder) / entry['path']).resolve()
        if not path.is_relative_to(Path(folder).resolve()) or path.is_symlink() or hashlib.sha256(path.read_bytes()).hexdigest() != entry['sha256']:
            raise ReleaseError('Server release artifact changed: ' + entry['path'])
        found.add(entry['path'])
    if role and role + '/LittleWeepsNetwork.exe' not in found:
        raise ReleaseError('Missing release executable')
    return entries


def native_processes():
    command = "@(Get-CimInstance Win32_Process -Filter \"Name = 'LittleWeepsNetwork.exe'\" | Select-Object ProcessId,ExecutablePath,CommandLine) | ConvertTo-Json -Compress"
    result = subprocess.run(['powershell.exe', '-NoProfile', '-Command', command], capture_output=True, text=True,
                            timeout=15, check=True, creationflags=subprocess.CREATE_NO_WINDOW)
    value = json.loads(result.stdout) if result.stdout.strip() else []
    return value if isinstance(value, list) else [value]


def install(root, family, build, port=None, process_reader=native_processes):
    root = Path(root).resolve()
    if uuid.UUID(family).hex != family: raise ReleaseError('Canonical enrolled family required')
    family_root = root / 'LocalData/FamilyLAN' / family
    public = load(family_root / 'family.json')
    if public['worldId'] != family or not (family_root / 'authority.pairing').is_file():
        raise ReleaseError('Existing enrollment required; no new family is created')
    release = network_release(root, build)
    settings = root / 'LocalData/ParentServer/settings.json'
    existing_settings = load(settings) if settings.exists() else {}
    if public.get('purpose') != 'isolated parent-control acceptance' and existing_settings.get('family') not in (None, family):
        raise ReleaseError('A different parent-helper family is selected; its settings were preserved')
    with installation_lock(root):
        previous = installed(root)
        if previous and previous['family'] == family and previous['build'] == build:
            if port not in (None, previous['port']): raise ReleaseError('Explicit endpoint migration is required')
            verify_bundle(home(root) / 'current', 'Server')
            return dict(result='already-current', installation=previous)
        for process in process_reader():
            native_path = Path(process.get('ExecutablePath') or '.').resolve()
            command = (process.get('CommandLine') or '').lower()
            if native_path == executable(root).resolve() or str(family_root).lower() in command:
                raise ReleaseError('Stop the verified idle authority before publishing; running servers are never overwritten')
        if (family_root / 'recovery.pending.json').exists(): raise ReleaseError('Resolve the pending world recovery first')
        save = family_root / 'server-world/world.save'
        if save.exists():
            header, checksum, payload = save.read_text(encoding='utf-8-sig').split('\n', 2)
            if header != 'LITTLEWEEPS-SOLO-1' or hashlib.sha256(payload.encode()).hexdigest() != checksum or not 2 <= load_payload_schema(payload) <= release['schema']:
                raise ReleaseError('Saved world is invalid or newer than the requested server; no downgrade/reset allowed')
        if port is None:
            if previous and previous['family'] == family: port = previous['port']
            elif (family_root / 'server-network.json').exists(): port = load(family_root / 'server-network.json')['port']
            elif (family_root / 'latest-server.json').exists(): port = load(family_root / 'latest-server.json')['port']
            else: port = 0
        if type(port) is not int or not (port == 0 or 1024 <= port <= 65535):
            raise ReleaseError('Invalid persistent server port')
        with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as probe:
            probe.setsockopt(socket.SOL_SOCKET, socket.SO_EXCLUSIVEADDRUSE, 1)
            probe.bind(('0.0.0.0', port)); port = probe.getsockname()[1]
        if not 1024 <= port <= 65535: raise ReleaseError('Invalid persistent server port')
        source = Path(release['folder']); entries = verify_bundle(source, 'Server')
        folder = home(root).resolve(); stage = folder / ('stage-' + uuid.uuid4().hex)
        current = folder / 'current'; backup = folder / ('previous-' + uuid.uuid4().hex)
        # Check all resolved paths before any directory move on Windows.
        if not all(p.resolve().is_relative_to(folder) and not p.is_symlink() for p in (stage, current, backup)):
            raise ReleaseError('Unsafe server publication path')
        stage.mkdir()
        server_entries = [e for e in entries if e['path'].startswith('Server/')]
        for entry in server_entries:
            target = stage / entry['path']; target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source / entry['path'], target)
        shutil.copy2(source / 'build-summary.json', stage / 'build-summary.json')
        shutil.copy2(source / 'source-manifest.json', stage / 'source-manifest.json')
        write(stage / 'artifact-manifest.json', server_entries)
        verify_bundle(stage, 'Server')
        value = {k: release[k] for k in ('build', 'protocol', 'content', 'schema', 'sharedDigest')}
        value.update(format=1, family=family, port=port, program=str(executable(root)),
                     installedAt=datetime.now(timezone.utc).isoformat(), previousBundle=str(backup) if current.exists() else None)
        write(folder / 'installation.pending.json', dict(next=value, previous=previous, stage=str(stage), backup=str(backup)))
        try:
            if current.exists(): publish_move(current,backup)
            publish_move(stage,current)
            write(folder / 'installation.json', value)
            (folder / 'installation.pending.json').unlink()
        except Exception:
            # Leave the journal and both bundles for explicit recovery; startup
            # refuses an ambiguous publication instead of opening a saved world.
            raise
        write(family_root / 'server-network.json', dict(format=1, port=port))
        if public.get('purpose') != 'isolated parent-control acceptance':
            settings.parent.mkdir(parents=True, exist_ok=True)
            write(settings, dict(existing_settings, family=family, build=build))
        return dict(result='installed', installation=value)


def load_payload_schema(payload):
    schema = json.loads(payload)['schema']
    if type(schema) is not int: raise ReleaseError('Invalid saved schema')
    return schema


def publish_move(source, target):
    # Windows scanning may briefly hold a newly copied executable directory.
    # Retry only sharing/access failures; retain the journal if it persists.
    until=time.monotonic()+3
    while True:
        try:source.rename(target);return
        except PermissionError:
            if time.monotonic()>=until:raise
            time.sleep(.05)
