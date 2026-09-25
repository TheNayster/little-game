"""Parent-facing adapter for qualified local backup and crash supervision."""
from datetime import datetime, timezone
import hashlib
from pathlib import Path
import threading
import time
import uuid

from parent_server import OperationError, operation_lock
from server_recovery import Recovery
from server_supervisor import Supervisor
from shared_garden_runtime import ROOT, read, write


class ParentOperations:
    def __init__(self, controller):
        self.controller = controller
        self.path = controller.root / 'parent-operations.json'
        self.mutex = threading.Lock()
        self.thread = None
        self.cancel = threading.Event()
        self.ready = threading.Event()
        self.error = None
        # Reopening the control helper preserves an earlier explicit opt-in,
        # but never changes the separately persisted parent Start/Stop intent.
        if (read(self.path) or {}).get('supervisionEnabled') is True:
            self._launch()

    def _preferences(self, **changes):
        with operation_lock(self.controller.root):
            record = read(self.path) or {}
            record.update(changes); write(self.path, record)

    def _worker(self):
        try:
            with operation_lock(self.controller.root, 'supervisor.lock'):
                self.ready.set()
                supervisor = Supervisor(self.controller)
                while not self.cancel.is_set():
                    supervisor.tick()
                    self.cancel.wait(2)
        except Exception as error:
            self.error = str(error) if isinstance(error, OperationError) else 'Automatic recovery stopped because its state could not be verified.'
        finally:
            self.ready.set()

    def _launch(self):
        if self.thread and self.thread.is_alive(): return
        self.cancel.clear(); self.ready.clear(); self.error = None
        self.thread = threading.Thread(target=self._worker, daemon=True)
        self.thread.start()
        self.ready.wait(3)

    def snapshot(self, native):
        record = read(self.path) or {}
        enabled = record.get('supervisionEnabled') is True
        running = bool(self.thread and self.thread.is_alive())
        observation = read(self.controller.root / 'supervisor-state.json') if running else None
        fresh = observation and 0 <= time.time() - observation.get('checkedAt', 0) <= 8
        recovery = dict(enabled=enabled, status='off', message='Automatic recovery is not enabled.',
                        canEnable=native['state'] == 'ready' and native['build'] == self.controller.build
                        and native['build'] >= 85 and native['save']['state'] == 'verified'
                        and (self.controller.isolated or self.controller.network_prepared()), canPause=enabled)
        if enabled:
            recovery['canEnable'] = False
            recovery['status'] = observation['status'] if fresh else 'checking' if running else 'needs-attention'
            recovery['message'] = observation['message'] if fresh else self.error or ('Waiting for a fresh recovery update.' if running else 'The recovery helper needs attention. The game has not been stopped.')
            intent = read(self.controller.root / 'server-intent.json')
            if running and intent and intent.get('automaticRestart') is False:
                recovery.update(status='paused', message='Restarts are paused after the parent stopped the server. Start server to resume.')
        elif not recovery['canEnable']:
            recovery['message'] = 'Activate the prepared server update before enabling automatic recovery.'
        backup = dict(state='none', verifiedAt=None)
        stored = record.get('backup')
        if stored:
            try:
                name = stored['name']; identity = name.removesuffix('.lwbackup')
                if uuid.UUID(identity).hex != identity or name != identity + '.lwbackup': raise ValueError()
                path = ROOT / 'LocalData/ServerBackups' / name
                if path.stat().st_size > 3 * 1024 * 1024: raise ValueError()
                raw = path.read_bytes()
                valid = len(raw) <= 3 * 1024 * 1024 and hashlib.sha256(raw).hexdigest() == stored['sha256']
                backup.update(state='verified' if valid else 'changed', verifiedAt=stored['verifiedAt'])
            except (OSError, ValueError, KeyError): backup['state'] = 'unavailable'
        return dict(recovery=recovery, backup=backup,
                    canBackup=native['state'] in ('ready', 'stopped') and native['save']['state'] == 'verified')

    def backup(self):
        with self.mutex:
            result = Recovery(self.controller.family, self.controller.build).backup()
            self.remember_backup(result)
            return dict(result='backup-verified')

    def remember_backup(self, result):
        metadata = dict(name=Path(result['path']).name, sha256=result['sha256'],
                        verifiedAt=datetime.now(timezone.utc).isoformat())
        self._preferences(backup=metadata)

    def enable(self):
        with self.mutex:
            native = self.controller.snapshot()
            current = self.snapshot(native)['recovery']
            if current['enabled'] and self.thread and self.thread.is_alive():
                return dict(result='recovery-enabled')
            if not current['canEnable']:
                raise OperationError('Apply the prepared server update and verify its network/save status before enabling recovery.')
            # A parent requested this operation; unlike helper resumption, it is
            # allowed to establish a fresh restart preference and retry budget.
            self.controller.start()
            self._launch()
            if self.error or not self.thread.is_alive():
                raise OperationError(self.error or 'The recovery helper could not start.')
            self._preferences(supervisionEnabled=True)
            return dict(result='recovery-enabled')

    def pause(self):
        with self.mutex:
            self.controller.pause_recovery()
            self._preferences(supervisionEnabled=False)
            self.close()
            return dict(result='recovery-paused')

    def close(self):
        # Backend closure is not a parent Stop/Pause: retain the saved choice
        # so a later control-helper launch can resume it without changing intent.
        self.cancel.set()
        if self.thread: self.thread.join(timeout=3)
