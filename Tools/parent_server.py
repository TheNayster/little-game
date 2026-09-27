"""Local parent operations; game identity/state stay in the existing authority.

The status DTO and operation boundary can later have a VPS adapter. This adapter
only observes Windows processes and the selected enrolled world's local files.
"""
from contextlib import contextmanager
from datetime import datetime, timezone
import ctypes
import hashlib
import json
import msvcrt
import os
from pathlib import Path
import re
import subprocess
import sys
import threading
import time
import uuid

from shared_garden_runtime import ROOT, read, write, wait


class OperationError(RuntimeError):
    pass


def command_args(command):
    shell = ctypes.WinDLL('shell32', use_last_error=True)
    shell.CommandLineToArgvW.argtypes = [ctypes.c_wchar_p, ctypes.POINTER(ctypes.c_int)]
    shell.CommandLineToArgvW.restype = ctypes.POINTER(ctypes.c_wchar_p)
    free = ctypes.WinDLL('kernel32').LocalFree
    free.argtypes, free.restype = [ctypes.c_void_p], ctypes.c_void_p
    count = ctypes.c_int()
    pointer = shell.CommandLineToArgvW(command, ctypes.byref(count))
    if not pointer:
        raise OperationError('Windows could not identify the server process.')
    try:
        return [pointer[i] for i in range(count.value)]
    finally:
        free(pointer)


def windows_processes():
    # Fixed query: no world names, paths, credentials or shell-interpolated input.
    command = "@(Get-CimInstance Win32_Process -Filter \"Name = 'LittleWeepsNetwork.exe'\" | Select-Object ProcessId,ExecutablePath,CommandLine) | ConvertTo-Json -Compress"
    result = subprocess.run(['powershell.exe', '-NoProfile', '-Command', command],
                            capture_output=True, text=True, timeout=12,
                            creationflags=subprocess.CREATE_NO_WINDOW)
    if result.returncode:
        raise OperationError('Windows process status is unavailable. No action was taken.')
    value = json.loads(result.stdout) if result.stdout.strip() else []
    return value if isinstance(value, list) else [value]


def checkpoint_bytes(path):
    # Ordinary Python open() on Windows does not share deletion. Holding that
    # handle can make the game's atomic checkpoint replacement fail. Observers
    # must share read/write/delete and read one immutable file handle snapshot.
    kernel = ctypes.WinDLL('kernel32', use_last_error=True)
    kernel.CreateFileW.argtypes = [ctypes.c_wchar_p, ctypes.c_ulong, ctypes.c_ulong,
                                  ctypes.c_void_p, ctypes.c_ulong, ctypes.c_ulong, ctypes.c_void_p]
    kernel.CreateFileW.restype = ctypes.c_void_p
    kernel.CloseHandle.argtypes = [ctypes.c_void_p]
    handle = kernel.CreateFileW(str(path), 0x80000000, 7, None, 3, 0x80, None)
    if handle == ctypes.c_void_p(-1).value:
        raise ctypes.WinError(ctypes.get_last_error())
    try:
        descriptor = msvcrt.open_osfhandle(handle, os.O_RDONLY | os.O_BINARY)
    except Exception:
        kernel.CloseHandle(handle); raise
    with os.fdopen(descriptor, 'rb') as file:
        return file.read(1024 * 1024 + 1), os.fstat(file.fileno()).st_mtime


@contextmanager
def operation_lock(folder, name='parent-operations.lock'):
    # Serialize operations across tabs AND separate controller processes. The
    # game's authority.lock remains the final single-writer guard for its save.
    with (folder / name).open('a+b') as file:
        if file.tell() == 0:
            file.write(b'0'); file.flush()
        file.seek(0)
        try:
            msvcrt.locking(file.fileno(), msvcrt.LK_NBLCK, 1)
        except OSError as error:
            raise OperationError('Another server operation is in progress.') from error
        try:
            yield
        finally:
            file.seek(0); msvcrt.locking(file.fileno(), msvcrt.LK_UNLCK, 1)


class ParentServer:
    def __init__(self, family, build, process_reader=windows_processes):
        if uuid.UUID(family).hex != family or not 84 <= build <= 9999:
            raise ValueError('A canonical enrolled family and build 84 or later are required.')
        self.family, self.build = family, build
        self.root = ROOT / 'LocalData/FamilyLAN' / family
        public = read(self.root / 'family.json')
        if not public or public.get('worldId') != family or not (self.root / 'authority.pairing').is_file():
            raise OperationError('The selected family is not enrolled. No new world was created.')
        self.isolated = public.get('purpose') == 'isolated parent-control acceptance'
        self.process_reader = process_reader
        self.mutex = threading.Lock()

    def processes(self):
        matches = []
        for native in self.process_reader():
            if not native.get('CommandLine'):
                continue
            args = command_args(native['CommandLine'])
            if '-familyNetworkConfig' not in args:
                continue
            index = args.index('-familyNetworkConfig') + 1
            if index >= len(args):
                continue
            config_path = Path(args[index]).resolve()
            if config_path.parent != self.root.resolve():
                continue
            config = read(config_path)
            if not config or config.get('role') != 'server':
                continue
            name = re.fullmatch(r'([0-9a-f]{32})\.config\.json', config_path.name)
            instance = name[1] if name else ''
            exe = Path(native.get('ExecutablePath') or '').resolve()
            version = re.fullmatch(r'G3-0\.0\.(\d+)', exe.parent.parent.name)
            build = int(version[1]) if version else 0
            expected = ROOT / f'Builds/NetworkProbe/G3-0.0.{build}/Server/LittleWeepsNetwork.exe'
            if not name or config.get('runId') != self.family or config.get('instanceId') != instance or exe != expected.resolve():
                raise OperationError('A server process for this world could not be verified. Controls are locked.')
            matches.append(dict(pid=native['ProcessId'], instanceId=instance, build=build,
                                output=self.root / instance))
        return matches

    def checkpoint(self):
        path = self.root / 'server-world/world.save'
        try:
            # Read timestamp and payload from the same open file, never a cached
            # label. Checksum verification is not a backup/restore qualification.
            raw, stamp = checkpoint_bytes(path)
            if len(raw) > 1024 * 1024:
                raise ValueError('Oversize save')
            header, digest, payload = raw.decode('utf-8-sig').split('\n', 2)
            body = json.loads(payload)
            if header != 'LITTLEWEEPS-SOLO-1' or hashlib.sha256(payload.encode()).hexdigest() != digest or body.get('schema') not in (2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12):
                raise ValueError('Unverified save')
            return dict(state='verified', savedAt=datetime.fromtimestamp(stamp, timezone.utc).isoformat(), revision=body['revision'])
        except FileNotFoundError:
            return dict(state='missing', savedAt=None, revision=None)
        except (OSError, ValueError, KeyError):
            return dict(state='unverified', savedAt=None, revision=None)

    def snapshot(self):
        result = dict(state='unreachable', message='Checking the server.', players=None,
                      build=None, instanceId=None, canStart=False, canStop=False,
                      save=self.checkpoint(), checkedAt=datetime.now(timezone.utc).isoformat())
        try:
            if (self.root / 'recovery.pending.json').exists():
                raise OperationError('A restore was interrupted. Resolve recovery before starting this world; progress has not been reset.')
            candidates = self.processes()
            if not candidates:
                # A missing/stale launcher record is not permission to overwrite
                # a locked world. Opening its lock never writes or resets a save.
                lock = self.root / 'server-world/authority.lock'
                if lock.exists():
                    with lock.open('rb'):
                        pass
                result.update(state='stopped', message='The server is stopped.',
                              canStart=result['save']['state'] != 'unverified')
                if not self.isolated:
                    if not self.network_prepared():
                        result.update(canStart=False, message='The prepared server update needs its Windows network setup before starting. Existing progress is kept.')
                return result
            if len(candidates) != 1:
                raise OperationError('More than one authority process was found. Controls are locked.')
            native = candidates[0]; out = native['output']
            result.update(build=native['build'], instanceId=native['instanceId'])
            status = read(out / 'status.json')
            evidence_path = out / 'connection-evidence.json'
            observation = read(evidence_path)
            age = time.time() - evidence_path.stat().st_mtime
            matches = status and all(status.get(k) == v for k, v in dict(pid=native['pid'], instanceId=native['instanceId'], runId=self.family, role='server', build=f"0.0.{native['build']}").items())
            if not matches or not observation or not 0 <= age <= 5:
                raise OperationError('The process exists, but fresh server status is unavailable. Controls are locked.')
            ready = status.get('status') == 'listening' and status.get('persistentServer') is True and all(observation.get(k) is True for k in ('listening', 'driverCreated', 'driverBound', 'driverListening')) and observation.get('receiveError') == 0
            if not ready:
                raise OperationError('The process is running but the game server is not ready. Controls are locked.')
            profiles = observation.get('profiles')
            if not isinstance(profiles, list) or len(profiles) > 4:
                raise OperationError('Player status is unavailable. Controls are locked.')
            result.update(state='ready', message='Ready for family play.', players=len(profiles),
                          canStop=native['build'] >= 84 and not profiles)
            if native['build'] < 84:
                result['message'] = 'Ready for family play. Safe stop controls require the prepared server update.'
            elif profiles:
                result['message'] = 'Family members are playing. Stop is protected until everyone leaves.'
        except (OSError, ValueError, KeyError, subprocess.SubprocessError, OperationError) as error:
            result.update(state='unreachable', canStart=False, canStop=False,
                          message=str(error) if isinstance(error, OperationError) else 'Server status could not be verified. No action was taken.')
        return result

    def network_prepared(self):
        permission = read(ROOT / f'LocalData/Verification/server-firewall-{self.build}.json')
        return bool(permission and permission.get('passed') is True and permission.get('build') == self.build)

    def save_intent(self, automatic_restart):
        value = dict(revision=uuid.uuid4().hex, automaticRestart=automatic_restart)
        staged = self.root / ('server-intent-' + value['revision'] + '.pending')
        with staged.open('x', encoding='utf-8') as output:
            json.dump(value, output); output.flush(); os.fsync(output.fileno())
        staged.replace(self.root / 'server-intent.json')
        return value

    def pause_recovery(self):
        with self.mutex, operation_lock(self.root):
            self.save_intent(False)
            return dict(result='automatic-recovery-paused', status=self.snapshot())

    def start(self, expected_intent=None):
        with self.mutex, operation_lock(self.root):
            # Recheck inside the same lock as parent Stop. A supervisor which
            # observed an old desire to run cannot undo a later deliberate stop.
            if expected_intent is not None and (expected_intent.get('automaticRestart') is not True or read(self.root / 'server-intent.json') != expected_intent):
                raise OperationError('Parent restart preference changed. Automatic start cancelled.')
            state = self.snapshot()
            if state['state'] == 'ready':
                if expected_intent is None: self.save_intent(True)
                return dict(result='already-running', status=state)
            if not state['canStart']:
                raise OperationError('Start is blocked until server and save status can be verified.')
            if expected_intent is None: self.save_intent(True)
            # Preserve the selected world and protected enrollment. This helper
            # verifies every build artifact and the native persistent-mode ack.
            result = subprocess.run([sys.executable, str(ROOT / 'Tools/Start-FamilyLAN.py'),
                                     '--build', str(self.build), '--family', self.family],
                                    capture_output=True, text=True, timeout=65,
                                    creationflags=subprocess.CREATE_NO_WINDOW)
            if result.returncode:
                # A launcher can fail before creating a native log (for example
                # a sign-in environment/import error). Keep its bounded stderr
                # in the private world folder, never in the parent HTTP response.
                write(self.root / 'server-start-error.json', dict(returncode=result.returncode,
                      stderr=result.stderr[-16000:], utc=datetime.now(timezone.utc).isoformat()))
                raise OperationError('Server startup did not complete. Existing saves were not reset; inspect the local server log.')
            state = wait(lambda: (s if (s := self.snapshot())['state'] == 'ready' else None), 'fresh server readiness', 12)
            return dict(result='started', status=state)

    def stop(self, expected_instance):
        with self.mutex, operation_lock(self.root):
            state = self.snapshot()
            if state['state'] != 'ready' or state['instanceId'] != expected_instance:
                raise OperationError('Server status changed. Refresh before trying again.')
            if state['build'] < 84:
                raise OperationError('This server needs the prepared update before guarded stopping is supported.')
            if state['players']:
                raise OperationError('Someone is playing. Leave the server running until everyone has left.')
            request_id = uuid.uuid4().hex
            out = self.root / expected_instance
            previous_intent = read(self.root / 'server-intent.json')
            self.save_intent(False)
            write(out / 'parent-control.json', dict(requestId=request_id, instanceId=expected_instance, kind='stop-if-empty'))
            response = wait(lambda: (r if (r := read(out / 'parent-response.json')) and r.get('requestId') == request_id and r.get('instanceId') == expected_instance else None), 'guarded stop acknowledgement', 12)
            if response['result'] != 'stopping':
                self.save_intent(bool(previous_intent and previous_intent.get('automaticRestart')))
                raise OperationError('The server refused to stop because its state changed or a player joined. Family play continues.')
            state = wait(lambda: (s if (s := self.snapshot())['state'] == 'stopped' else None), 'clean server exit', 20)
            stopped = read(out / 'status.json')
            if not stopped or stopped.get('status') != 'stopped' or state['save']['state'] != 'verified':
                raise OperationError('The stop could not be fully verified. No recovery or reset was attempted.')
            return dict(result='stopped', status=state)
