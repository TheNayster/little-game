"""Exercise parent controls with an isolated enrolled world and four native clients."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
import ctypes
from copy import deepcopy
from contextlib import contextmanager
from datetime import datetime, timezone
import hashlib
import importlib.util
import json
import os
import socket
import subprocess
import threading
import time
import uuid
from urllib.error import HTTPError
from urllib.request import Request, urlopen
from unittest.mock import patch

from family_pairing import create_family, write_record
from parent_server import ParentServer, OperationError, operation_lock, checkpoint_bytes, ROOT
from shared_garden_runtime import Instance, read, write, wait, require


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int)
    args = parser.parse_args()
    authority, players, _ = create_family()
    world = authority['worldId']; folder = ROOT / 'LocalData/FamilyLAN' / world
    folder.mkdir(parents=True)
    write(folder / 'family.json', dict(worldId=world, purpose='isolated parent-control acceptance'))
    write_record(folder / 'authority.pairing', authority)
    for i, record in enumerate(players, 1):
        write_record(folder / f'player-{i}.pairing', record)
    controller = ParentServer(world, args.build)
    spec = importlib.util.spec_from_file_location('parent_http', ROOT / 'Tools/Parent-Server.py')
    module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
    http = module.ParentHTTP(controller)
    thread = threading.Thread(target=http.serve_forever, daemon=True); thread.start()
    instances = []; checks = []; success = False

    def passed(name):
        checks.append(dict(check=name, passed=True)); print('PASS ' + name, flush=True)

    def api(route, payload=None, token=True, origin=None):
        headers = {'X-Little-Weeps': http.token} if token else {}
        if origin: headers['Origin'] = origin
        if payload is not None: headers['Content-Type'] = 'application/json'
        request = Request(http.origin + '/api/' + route, data=json.dumps(payload).encode() if payload is not None else None, headers=headers)
        try:
            with urlopen(request, timeout=80) as response:
                return response.status, json.load(response)
        except HTTPError as error:
            return error.code, json.load(error)

    class Run: pass
    run = Run(); run.build = args.build; run.run_id = world; run.path = folder

    def client(record, index):
        v = Instance.__new__(Instance); v.run = run; v.role = 'client'; v.profile = record['profile']
        v.serial = 0; v.garden_serial = 0; v.identity = uuid.uuid4().hex
        v.out = folder / v.identity; v.out.mkdir()
        config = folder / (v.identity + '.config.json')
        write(config, dict(runId=world, instanceId=v.identity, role='client', port=1025,
                           protocol=3, content=3, pairingPath=str(folder / f'player-{index}.pairing'),
                           presentation=True, verifyGarden=True, interactive=True))
        exe = ROOT / f'Builds/NetworkProbe/G3-0.0.{args.build}/Client/LittleWeepsNetwork.exe'
        startup = subprocess.STARTUPINFO(); startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW; startup.wShowWindow = 0
        v.process = subprocess.Popen([str(exe), '-familyNetworkConfig', str(config), '-logFile', str(v.out / 'player.log'),
                                      '-screen-fullscreen', '0', '-screen-width', '960', '-screen-height', '640'],
                                     stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                                     startupinfo=startup, creationflags=subprocess.CREATE_NO_WINDOW)
        instances.append(v)
        wait(lambda: v.status() and v.status()['status'] == 'connected' and v.input('inspect')['shared'], 'native client joins', 40)
        return v

    def refused(call):
        try: call()
        except OperationError: return
        raise AssertionError('Unsafe action was not refused')

    try:
        require(api('status', token=False)[0] == 403 and api('start', {}, origin='https://untrusted.example')[0] == 403, 'HTTP access boundary')
        require(controller.snapshot()['state'] == 'stopped', 'New test world status')
        passed('local HTTP status/actions require the session capability and same-origin request')
        with patch.object(controller, 'isolated', False), patch.object(controller, 'network_prepared', return_value=False):
            require(not controller.snapshot()['canStart'], 'Unconfigured production network start enabled')
            refused(controller.start)
        passed('production start stays locked until the selected executable network setup is recorded')
        probe = folder / 'checkpoint-read-probe'; staged = folder / 'checkpoint-read-probe.next'
        probe.write_bytes(b'previous'); staged.write_bytes(b'next')
        native_fdopen = os.fdopen
        kernel = ctypes.WinDLL('kernel32', use_last_error=True)
        kernel.ReplaceFileW.argtypes = [ctypes.c_wchar_p, ctypes.c_wchar_p, ctypes.c_wchar_p,
                                       ctypes.c_ulong, ctypes.c_void_p, ctypes.c_void_p]
        kernel.ReplaceFileW.restype = ctypes.c_int
        @contextmanager
        def replace_while_open(*a, **kw):
            with native_fdopen(*a, **kw) as opened:
                # Match CheckpointStore's File.Replace operation, not Python's
                # MoveFileEx-based os.replace (different open-target semantics).
                if not kernel.ReplaceFileW(str(probe), str(staged), None, 0, None, None):
                    raise ctypes.WinError(ctypes.get_last_error())
                yield opened
        with patch('parent_server.os.fdopen', side_effect=replace_while_open):
            require(checkpoint_bytes(probe)[0] == b'previous', 'Open snapshot changed while replacing')
        require(checkpoint_bytes(probe)[0] == b'next', 'Replacement not observable')
        passed('save status reads permit atomic checkpoint replacement while the observer handle is open')
        code, result = api('start', {})
        require(code == 200 and result['status']['state'] == 'ready', 'HTTP start failed')
        first = result['status']['instanceId']; native = controller.processes()[0]
        require(result['status']['save']['state'] == 'verified', 'No verified durable save')
        passed('stopped enrolled world starts as a fresh ready persistent server with a verified checkpoint')
        code, result = api('start', {})
        require(code == 200 and result['result'] == 'already-running' and result['status']['instanceId'] == first, 'Duplicate start')
        other = ParentServer(world, args.build)
        with operation_lock(folder): refused(other.start)
        require(len(controller.processes()) == 1, 'Duplicate native authority')
        passed('repeated start and cross-controller operation lock preserve one process and enrolled identity')
        original_reader = controller.process_reader
        def wrong_pid():
            entries = deepcopy(original_reader())
            for e in entries:
                if e['ProcessId'] == native['pid']: e['ProcessId'] += 1
            return entries
        controller.process_reader = wrong_pid
        require(controller.snapshot()['state'] == 'unreachable', 'Unverified PID accepted')
        refused(controller.start); controller.process_reader = original_reader
        clock = time.time
        with patch('parent_server.time.time', side_effect=lambda: clock() + 600):
            require(controller.snapshot()['state'] == 'unreachable', 'Stale evidence accepted')
            refused(controller.start)
        passed('PID mismatch and stale heartbeat lock controls instead of claiming ready or starting a duplicate')
        clients = [client(p, i) for i, p in enumerate(players, 1)]
        wait(lambda: controller.snapshot()['players'] == 4, 'four observed players')
        code, _ = api('stop', dict(instanceId=first)); require(code == 409, 'Occupied UI/API stop allowed')
        # Bypass desktop preflight to exercise the authoritative race guard itself.
        out = folder / first; request_id = uuid.uuid4().hex
        write(out / 'parent-control.json', dict(requestId=request_id, instanceId=first, kind='stop-if-empty'))
        response = wait(lambda: (r if (r := read(out / 'parent-response.json')) and r['requestId'] == request_id else None), 'native occupied refusal')
        require(response['result'] == 'players-connected' and response['players'] == 4 and len(read(out / 'view.json')['connected']) == 4, 'Native occupied guard failed')
        clients[0].input('press', role='bucket-1'); clients[0].input('release', x=730, y=160)
        wait(lambda: not clients[0].input('inspect')['pending'], 'bucket command settled')
        clients[1].input('button', text='Creek')
        wait(lambda: not clients[1].input('inspect')['pending'], 'travel settled')
        require(controller.snapshot()['instanceId'] == first, 'Refusal restarted server')
        passed('four native clients keep playing after both API and authoritative occupied-stop refusal')
        for c in clients: c.close()
        wait(lambda: controller.snapshot()['players'] == 0, 'players left')
        before = json.loads((folder / 'server-world/world.save').read_text(encoding='utf-8-sig').split('\n', 2)[2])
        code, result = api('stop', dict(instanceId=first))
        require(code == 200 and result['status']['state'] == 'stopped' and result['status']['save']['state'] == 'verified', 'Empty stop failed')
        after = json.loads((folder / 'server-world/world.save').read_text(encoding='utf-8-sig').split('\n', 2)[2])
        require(before == after, 'Empty stop changed world')
        passed('empty stop is acknowledged, exits cleanly and preserves the full checkpoint including receipts')
        save = folder / 'server-world/world.save'; original = save.read_bytes()
        try:
            save.write_bytes(b'corrupt isolated test checkpoint')
            require(controller.snapshot()['save']['state'] == 'unverified', 'Corrupt save displayed as verified')
            refused(controller.start)
        finally: save.write_bytes(original)
        passed('unverified checkpoint blocks parent start without overwriting progress')
        _, result = api('start', {})
        second = result['status']['instanceId']
        restored = json.loads(save.read_text(encoding='utf-8-sig').split('\n', 2)[2])
        require(second != first and restored == after, 'Restart lost identity/progress')
        require(api('stop', dict(instanceId=first))[0] == 409 and controller.snapshot()['instanceId'] == second, 'Stale page stopped replacement')
        passed('restart restores exact saved state and a stale page cannot stop the replacement instance')
        require(api('stop', dict(instanceId=second))[0] == 200, 'Final clean stop')
        success = True
    finally:
        for c in reversed(instances):
            if c.process.poll() is None:
                try: c.close()
                except Exception: c.process.kill(); c.process.wait(timeout=10)
        # Cleanup only this test's newly enrolled world, never enumerate/quit
        # other authorities and never touch the actual family's control file.
        controller.process_reader = original_reader if 'original_reader' in locals() else controller.process_reader
        for native in controller.processes():
            control = read(native['output'] / 'control.json') or {}
            write(native['output'] / 'control.json', dict(serial=control.get('serial', 0) + 1, kind='quit'))
        http.shutdown(); http.server_close()
        result = dict(passed=success, build=args.build, utc=datetime.now(timezone.utc).isoformat(), checks=checks,
                      scope='Isolated Windows authority, four native Windows clients and real HTTP operations. No physical devices or live family authority changed; not independent backup restore, crash supervision or VPS qualification.')
        write(folder / 'parent-controls-result.json', result)
        print('Evidence:', folder / 'parent-controls-result.json', flush=True)


if __name__ == '__main__': main()
