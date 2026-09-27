"""Launch only verified local garden builds, with isolated loopback credentials."""
import hashlib
import json
import os
from pathlib import Path
import secrets
import socket
import subprocess
import time
import uuid

ROOT = Path(__file__).resolve().parent.parent


def read(path):
    # Atomic evidence replacement can briefly deny a Windows reader. Retry a
    # fresh read; never substitute cached state or hide a malformed JSON file.
    for attempt in range(6):
        try:
            return json.loads(path.read_text(encoding='utf-8-sig'))
        except (FileNotFoundError, PermissionError):
            if attempt == 5:
                return None
            time.sleep(.01)


def write(path, value):
    temp = path.with_suffix('.pending')
    temp.write_text(json.dumps(value, indent=2), encoding='utf-8')
    until = time.monotonic() + 2
    while True:
        try:
            temp.replace(path)
            break
        except PermissionError:
            if time.monotonic() >= until:
                raise
            time.sleep(.01)


def wait(predicate, message, seconds=20):
    until = time.monotonic() + seconds
    while time.monotonic() < until:
        value = predicate()
        if value:
            return value
        time.sleep(.05)
    raise RuntimeError('Timed out: ' + message)


def require(value, message):
    if not value:
        raise AssertionError(message)


class Run:
    def __init__(self, build, interactive=False, resume=None, motion_conditions=None):
        require(os.name == 'nt' and 51 <= build <= 9999, 'Windows shared garden build required')
        self.build, self.interactive = build, interactive
        self.folder = ROOT / f'Builds/NetworkProbe/G3-0.0.{build}'
        summary = read(self.folder / 'build-summary.json')
        require(summary and summary['contract'] in (2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16) and summary['gardenPresentation'], 'Playable garden build required')
        self.content = summary["content"] if summary["contract"] >= 8 else 6 if summary["contract"] >= 7 else 5 if summary["contract"] >= 6 else 4 if summary["contract"] >= 5 else (3 if summary["contract"] >= 4 else 2 if summary["contract"] >= 3 else 1)
        self.protocol = 3 if summary['contract'] >= 4 else 2 if summary['contract'] >= 3 else 1
        self.motion_conditions = motion_conditions or {}
        require(len(summary['builds']) == 2, 'Both binaries required')
        for b in summary['builds']:
            require(b['result'] == 'Succeeded' and b['version'] == f'0.0.{build}' and b['errors'] == 0, 'Wrong build')
            require(b['dedicatedServer'] == (b['role'] == 'Server'), 'Wrong subtarget')
        artifacts = read(self.folder / 'artifact-manifest.json')
        require(artifacts and {f'{v}/LittleWeepsNetwork.exe' for v in ('Client', 'Server')} <= {e['path'] for e in artifacts}, 'Missing executable hashes')
        for e in artifacts:
            p = (self.folder / e['path']).resolve()
            require(p.is_relative_to(self.folder.resolve()) and hashlib.sha256(p.read_bytes()).hexdigest() == e['sha256'], 'Artifact changed: ' + e['path'])
        self.run_id = uuid.UUID(resume).hex if resume else uuid.uuid4().hex
        self.path = ROOT / 'LocalData/SharedGarden' / self.run_id
        self.path.mkdir(parents=True, exist_ok=bool(resume))
        if resume:
            self.slots = read(self.path / 'slots.json')
            require(self.slots and len(self.slots) == 4, 'Saved lab session credentials are missing; no reset performed')
        else:
            self.slots = [dict(profile=f'player-{i}', token=secrets.token_hex(32)) for i in range(1, 5)]
            write(self.path / 'slots.json', self.slots)
        with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as s:
            s.bind(('127.0.0.1', 0)); self.port = s.getsockname()[1]
        self.instances = []

    def start(self, role, profile=''):
        instance = Instance(self, role, profile)
        self.instances.append(instance)
        if role == 'server':
            wait(lambda: instance.status() and instance.status()['status'] == 'listening', 'server ready')
        else:
            wait(lambda: instance.state(), 'admitted client snapshot')
            if not self.interactive:
                wait(lambda: instance.input('inspect')['ready'], 'garden presentation ready')
        return instance

    def close(self):
        for instance in reversed(self.instances):
            if instance.process.poll() is None:
                try:
                    instance.close()
                except Exception:
                    instance.process.kill(); instance.process.wait(timeout=10)


class Instance:
    def __init__(self, run, role, profile):
        self.run, self.role, self.profile = run, role, profile
        self.serial, self.garden_serial = 0, 0
        self.identity = uuid.uuid4().hex
        self.out = run.path / self.identity
        self.out.mkdir()
        cfg = dict(runId=run.run_id, instanceId=self.identity, role=role, profile=profile,
                   token=next((s['token'] for s in run.slots if s['profile'] == profile), ''),
                   protocol=run.protocol, content=run.content, port=run.port, slots=run.slots if role == 'server' else [],
                   presentation=True, verifyGarden=role == 'client' and not run.interactive, interactive=run.interactive)
        if role == 'client' and not run.interactive: cfg.update(run.motion_conditions)
        config = run.path / (self.identity + '.config.json'); write(config, cfg)
        exe = run.folder / ('Server' if role == 'server' else 'Client') / 'LittleWeepsNetwork.exe'
        args = [str(exe), '-familyNetworkConfig', str(config), '-logFile', str(self.out / 'player.log')]
        if role == 'server':
            args += ['-batchmode', '-nographics']
        else:
            args += ['-screen-fullscreen', '0', '-screen-width', '960', '-screen-height', '640']
        startup = subprocess.STARTUPINFO(); startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
        startup.wShowWindow = 1 if role == 'client' and run.interactive else 0
        self.process = subprocess.Popen(args, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                                        startupinfo=startup, creationflags=subprocess.CREATE_NO_WINDOW)

    def status(self):
        s = read(self.out / 'status.json')
        if s:
            require(s['pid'] == self.process.pid and s['build'] == f'0.0.{self.run.build}' and s['runId'] == self.run.run_id, 'Wrong process identity')
            require(s['status'] != 'failed', s.get('reason', 'Failed'))
        return s

    def state(self):
        self.status()
        return read(self.out / 'view.json')

    def input(self, action, **kw):
        self.garden_serial += 1
        write(self.out / 'garden-control.json', dict(serial=self.garden_serial, action=action, **kw))
        result = wait(lambda: (v if (v := read(self.out / 'garden-evidence.json')) and v['serial'] == self.garden_serial else None), 'UI input ' + action)
        require(result['passed'] and result['build'] == f'0.0.{self.run.build}' and result['actor'] == self.profile, 'UI verification failed: ' + result.get('error', ''))
        return result

    def close(self):
        if self.process.poll() is None:
            self.serial += 1; write(self.out / 'control.json', dict(serial=self.serial, kind='quit'))
            self.process.wait(timeout=12); require(self.process.returncode == 0, 'Abnormal process exit')
