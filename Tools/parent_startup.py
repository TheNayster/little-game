"""Opt-in current-user sign-in shortcut; never starts/stops the game itself."""
import ctypes
import hashlib
import json
import os
from pathlib import Path
import subprocess
import threading

from parent_server import OperationError, operation_lock
from shared_garden_runtime import ROOT, read

HELPER_PROTOCOL = 2
SHORTCUT = 'Little Weeps Server.lnk'


def state_folder(controller):
    return controller.root / 'parent-startup' if controller.isolated else ROOT / 'LocalData/ParentServer'


def powershell():
    return str(Path(os.environ['SystemRoot']) / 'System32/WindowsPowerShell/v1.0/powershell.exe')


def process_alive(pid):
    if type(pid) is not int or pid <= 0:
        return False
    kernel = ctypes.WinDLL('kernel32', use_last_error=True)
    kernel.OpenProcess.argtypes = [ctypes.c_ulong, ctypes.c_int, ctypes.c_ulong]
    kernel.OpenProcess.restype = ctypes.c_void_p
    kernel.WaitForSingleObject.argtypes = [ctypes.c_void_p, ctypes.c_ulong]
    kernel.CloseHandle.argtypes = [ctypes.c_void_p]
    handle = kernel.OpenProcess(0x100000, False, pid)  # SYNCHRONIZE only
    if not handle:
        if ctypes.get_last_error() == 87:  # PID no longer exists
            return False
        raise OperationError('The earlier parent helper cannot be verified. No replacement was started.')
    try:
        return kernel.WaitForSingleObject(handle, 0) == 258  # WAIT_TIMEOUT
    finally:
        kernel.CloseHandle(handle)


def shortcut_tool(action, path, expected):
    # Structured stdin keeps paths/data out of PowerShell source text.
    value = dict(action=action, path=str(path), **expected)
    result = subprocess.run([powershell(), '-NoProfile', '-File', str(ROOT/'Tools/Startup-Shortcut.ps1')],
                            input=json.dumps(value), text=True, encoding='utf-8', capture_output=True,
                            creationflags=subprocess.CREATE_NO_WINDOW, timeout=15)
    if result.returncode:
        raise OperationError('The sign-in shortcut could not be verified. No unrelated shortcut was replaced.')
    return json.loads(result.stdout)


class ParentStartup:
    def __init__(self, controller, operations=None):
        self.controller, self.operations = controller, operations
        self.folder = state_folder(controller)
        if controller.isolated:
            self.directory = self.folder / 'startup-test'
        else:
            # Resolve the current user's actual known Startup folder, including
            # redirected profiles. Never use the all-users Startup folder.
            result = subprocess.run([powershell(), '-NoProfile', '-Command',
                                     '[Environment]::GetFolderPath("Startup")'],
                                    capture_output=True, text=True, timeout=15,
                                    creationflags=subprocess.CREATE_NO_WINDOW)
            if result.returncode or not result.stdout.strip():
                raise OperationError('Windows sign-in folder is unavailable.')
            self.directory = Path(result.stdout.strip())
        self.path = self.directory / SHORTCUT
        args = ['-NoProfile', '-WindowStyle', 'Hidden', '-File', str(ROOT/'Tools/Open-ParentServer.ps1'), '-NoBrowser', '-AtSignIn']
        if controller.isolated:
            args += ['-IsolatedFamily', controller.family]
        self.expected = dict(target=powershell(), arguments=subprocess.list2cmdline(args), workingDirectory=str(ROOT),
                             description='Little Weeps parent helper — resume saved recovery choices', windowStyle=7)
        self.cached = None
        self.mutex = threading.Lock()

    def registration(self):
        if not self.path.exists():
            self.cached = None
            return 'off'
        try:
            if self.path.is_symlink() or self.path.stat().st_size > 65536:
                return 'changed'
            digest = hashlib.sha256(self.path.read_bytes()).hexdigest()
            if self.cached and self.cached[0] == digest:
                return self.cached[1]
            actual = shortcut_tool('inspect', self.path, self.expected)
            valid = (Path(actual['target']).resolve() == Path(self.expected['target']).resolve()
                     and actual['arguments'] == self.expected['arguments']
                     and Path(actual['workingDirectory']).resolve() == ROOT.resolve()
                     and actual['description'] == self.expected['description']
                     and actual['windowStyle'] == self.expected['windowStyle'])
            state = 'configured' if valid else 'changed'
            self.cached = digest, state
            return state
        except (OSError, ValueError, KeyError, OperationError, subprocess.SubprocessError):
            return 'unavailable'

    def settings_match(self):
        settings = read(self.folder/'settings.json')
        return settings and settings.get('family') == self.controller.family and settings.get('build') == self.controller.build

    def snapshot(self, native, recovery):
        state = self.registration()
        ready = (self.settings_match() and native['state'] == 'ready'
                 and native['build'] == self.controller.build and native['save']['state'] == 'verified'
                 and recovery.get('enabled') and recovery.get('status') in ('healthy', 'recovered')
                 and (self.controller.isolated or self.controller.network_prepared()))
        messages = {
            'off': 'Enable recovery on the selected server first, then add its sign-in shortcut.',
            'configured': 'Shortcut verified. After you sign in, saved recovery choices resume; a deliberate Stop or Pause is preserved.',
            'changed': 'A different shortcut uses this name. It was not changed; a parent needs to inspect it.',
            'unavailable': 'The sign-in shortcut could not be checked. No changes were made.'}
        if state == 'off' and ready:
            messages['off'] = 'Ready to add a shortcut for this Windows account. No administrator access is needed.'
        return dict(state=state, canEnable=bool(state=='off' and ready), canDisable=state=='configured',
                    message=messages[state])

    def enable(self):
        with self.mutex:
            self.folder.mkdir(parents=True, exist_ok=True)
            with operation_lock(self.folder, 'startup-registration.lock'):
                native = self.controller.snapshot()
                recovery = self.operations.snapshot(native)['recovery']
                status = self.snapshot(native, recovery)
                if status['state'] == 'configured':
                    return dict(result='startup-configured')
                if not status['canEnable']:
                    raise OperationError('Use the selected, healthy server with recovery enabled before adding sign-in startup.')
                self.directory.mkdir(parents=True, exist_ok=True)
                shortcut_tool('create', self.path, self.expected)
                if self.registration() != 'configured':
                    raise OperationError('Shortcut verification failed. Inspect the startup setting before relying on it.')
                return dict(result='startup-configured')

    def disable(self):
        with self.mutex:
            self.folder.mkdir(parents=True, exist_ok=True)
            with operation_lock(self.folder, 'startup-registration.lock'):
                state = self.registration()
                if state == 'off':
                    return dict(result='startup-removed')
                if state != 'configured':
                    raise OperationError('The changed shortcut was left untouched. A parent needs to inspect it.')
                # Compare its native contents again inside the removal operation;
                # a status response or cached file hash is not delete authority.
                shortcut_tool('remove', self.path, self.expected)
                self.cached = None
                if self.path.exists():
                    raise OperationError('The startup shortcut is still present.')
                return dict(result='startup-removed')
