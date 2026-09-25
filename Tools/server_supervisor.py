"""Opt-in process-crash recovery. Never kills a process, resets a save or bypasses parent intent."""
import time
import subprocess
from parent_server import OperationError
from shared_garden_runtime import read, write


class Supervisor:
    def __init__(self, controller, clock=time.time):
        self.controller, self.clock = controller, clock
        self.path = controller.root / 'supervisor-state.json'

    def tick(self):
        now = self.clock()
        intent = read(self.controller.root / 'server-intent.json')
        previous = read(self.path)
        # Lost/unreadable accounting is never an excuse for unlimited retries.
        if self.path.exists() and previous is None:
            raise OperationError('Supervisor accounting cannot be read.')
        state = previous or dict(intentRevision=None, attempts=0, healthySince=None, nextAttempt=None)
        def record(status, message):
            state.update(status=status, message=message, checkedAt=now)
            write(self.path, state)
            return state.copy()
        if not intent or intent.get('automaticRestart') is not True:
            return record('paused', 'Automatic crash recovery is paused. The game is not stopped by this helper.')
        if not isinstance(intent.get('revision'), str) or len(intent['revision']) != 32:
            return record('blocked', 'Parent restart preference could not be verified.')
        if state.get('intentRevision') != intent['revision']:
            state = dict(intentRevision=intent['revision'], attempts=0, healthySince=None, nextAttempt=None)
        if type(state.get('attempts')) is not int or not 0 <= state['attempts'] <= 3:
            return record('blocked', 'Supervisor retry accounting needs attention.')
        snapshot = self.controller.snapshot()
        if snapshot['state'] == 'ready':
            if snapshot['build'] != self.controller.build:
                return record('blocked', 'A different server build is running. Deploy the selected build before enabling recovery.')
            if state['healthySince'] is None or state['healthySince'] > now:
                state['healthySince'] = now
            if now - state['healthySince'] >= 600:
                state['attempts'] = 0
            state['nextAttempt'] = None
            return record('healthy', 'Server ready; no restart needed.')
        state['healthySince'] = None
        if snapshot['state'] != 'stopped' or snapshot['save']['state'] != 'verified' or not snapshot['canStart']:
            return record('blocked', 'Server, save or network setup needs attention. No process was killed or save reset.')
        if state['attempts'] >= 3:
            return record('blocked', 'Three recovery attempts used without ten healthy minutes. A parent must inspect and start the server.')
        if state['nextAttempt'] is None:
            state['nextAttempt'] = now + min(60, 10 * 2 ** state['attempts'])
        if now < state['nextAttempt']:
            return record('waiting', 'Server exited; waiting before a guarded restart.')
        state['attempts'] += 1
        state['nextAttempt'] = now + min(60, 10 * 2 ** state['attempts'])
        record('restarting', 'Attempting to restore the existing enrolled world.')
        try:
            self.controller.start(expected_intent=intent)
            return record('recovered', 'Server restarted from its saved world. Clients can reconnect.')
        except (OperationError, RuntimeError, OSError, subprocess.SubprocessError):
            return record('waiting', 'Restart did not complete; bounded retry remains subject to fresh checks.')
