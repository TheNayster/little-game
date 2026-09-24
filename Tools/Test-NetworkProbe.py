"""Exercise the Unity loopback server with separate native Windows clients.

Only launches hash-verified local probe builds. Every run has fresh test credentials,
storage and control files; no normal game saves or physical devices are accessed.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import secrets
import socket
import subprocess
import time
import uuid
from datetime import datetime, timezone

ROOT = Path(__file__).resolve().parent.parent


def digest(path):
    h = hashlib.sha256()
    with path.open('rb') as f:
        for block in iter(lambda: f.read(1024 * 1024), b''):
            h.update(block)
    return h.hexdigest()


def read(path):
    try:
        return json.loads(path.read_text(encoding='utf-8-sig'))
    except (FileNotFoundError, PermissionError):
        return None


def write(path, value):
    temp = path.with_suffix('.pending')
    temp.write_text(json.dumps(value), encoding='utf-8')
    temp.replace(path)


def wait(predicate, description, seconds=25):
    deadline = time.monotonic() + seconds
    while time.monotonic() < deadline:
        value = predicate()
        if value:
            return value
        time.sleep(.05)
    raise RuntimeError('Timed out: ' + description)


def check(condition, description):
    if not condition:
        raise AssertionError(description)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int)
    args = parser.parse_args()
    check(os.name == 'nt' and 46 <= args.build <= 9999, 'Windows probe build required')
    folder = ROOT / 'Builds/NetworkProbe' / f'G3-0.0.{args.build}'
    summary = read(folder / 'build-summary.json')
    check(summary and summary['contract'] in (1, 2, 3), 'Wrong probe contract')
    wire_version = 2 if summary['contract'] >= 3 else 1
    check({b['role'] for b in summary['builds']} == {'Server', 'Client'}, 'Both builds required')
    for b in summary['builds']:
        check(b['result'] == 'Succeeded' and b['version'] == f'0.0.{args.build}', 'Build identity mismatch')
        check(b['dedicatedServer'] == (b['role'] == 'Server'), 'Wrong player subtarget')
    for entry in read(folder / 'artifact-manifest.json'):
        path = (folder / entry['path']).resolve()
        check(path.is_relative_to(folder.resolve()) and digest(path) == entry['sha256'], 'Artifact mismatch')

    run_id = uuid.uuid4().hex
    run = ROOT / 'LocalData/NetworkProbe' / run_id
    run.mkdir(parents=True)
    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as probe:
        probe.bind(('127.0.0.1', 0))
        port = probe.getsockname()[1]
    slots = [{'profile': f'player-{i}', 'token': secrets.token_hex(32)} for i in range(1, 5)]
    processes, active, results = [], {}, []
    server = None

    class Instance:
        def __init__(self, role, profile='', token='', protocol=wire_version):
            self.instance_id = uuid.uuid4().hex
            self.profile = profile
            self.serial = 0
            self.out = run / self.instance_id
            self.out.mkdir()
            cfg = dict(runId=run_id, instanceId=self.instance_id, role=role, profile=profile,
                       token=token, protocol=protocol, content=wire_version, port=port,
                       slots=slots if role == 'server' else [])
            path = run / (self.instance_id + '.config.json')
            write(path, cfg)
            startup = subprocess.STARTUPINFO()
            startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
            startup.wShowWindow = 0
            exe = folder / ('Server' if role == 'server' else 'Client') / 'LittleWeepsNetwork.exe'
            self.process = subprocess.Popen([str(exe), '-batchmode', '-nographics', '-familyNetworkConfig', str(path),
                                             '-logFile', str(self.out / 'player.log')],
                                            stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                                            startupinfo=startup, creationflags=subprocess.CREATE_NO_WINDOW)
            processes.append(self)

        def status(self):
            value = read(self.out / 'status.json')
            if value:
                check(value['runId'] == run_id and value['build'] == f'0.0.{args.build}' and value['pid'] == self.process.pid, 'Wrong process evidence')
                check(value['status'] != 'failed', 'Probe failed: ' + value.get('reason', ''))
            return value

        def view(self):
            self.status()
            return read(self.out / 'view.json')

        def control(self, kind, request=None):
            self.serial += 1
            write(self.out / 'control.json', dict(serial=self.serial, kind=kind, request=request))

        def close(self):
            if self.process.poll() is None:
                self.control('quit')
                self.process.wait(timeout=12)
                check(self.process.returncode == 0, 'Probe did not exit normally')

    def passed(name, **details):
        results.append(dict(check=name, passed=True, **details))
        print('PASS ' + name, flush=True)

    def start_client(profile, token=None, protocol=wire_version, rejected=None):
        if token is None:
            token = next(s['token'] for s in slots if s['profile'] == profile)
        client = Instance('client', profile, token, protocol)
        if rejected:
            status = wait(lambda: (v if (v := client.status()) and v['status'] == 'disconnected' else None), 'rejected client ' + rejected)
            check(status['reason'] == rejected, 'Wrong rejection: ' + status['reason'])
            client.close()
        else:
            def initial_view():
                status = client.status()
                check(not status or status['status'] != 'disconnected', 'Join disconnected: ' + str(status))
                return client.view()
            wait(initial_view, 'client initial snapshot')
            active[profile] = client
        return client

    def authority():
        return wait(lambda: server.view(), 'server view')

    def converge():
        expected = authority()
        wait(lambda: all((v := c.view()) and v['epoch'] == expected['epoch'] and v['view'] == expected['view'] and v['connected'] == expected['connected'] for c in active.values()), 'all active client snapshots converge')
        return expected

    def command(client, action, item='', target='', value='', x=0, y=0, actor=None):
        rid = uuid.uuid4().hex
        player = next(p for p in authority()['view']['players'] if p['id'] == client.profile)
        return dict(requestId=rid, protocol=wire_version, command=dict(requestId=rid, actor=actor or client.profile,
                    expectedRevision=authority()['view']['revision'], zone=player.get('zone', 'garden'), visit=player.get('visit', 0), action=action, item=item, target=target, value=value, x=x, y=y))

    def send(client, request, previous_sequence=0):
        client.control('command', request)
        return wait(lambda: (v if (v := read(client.out / ('reply-' + request['requestId'] + '.json'))) and v['sequence'] > previous_sequence else None), 'command response')

    def act(client, action, **kw):
        request = command(client, action, **kw)
        result = send(client, request)
        check(result['accepted'] and result['durable'], 'Action rejected: ' + result['outcome'])
        wait(lambda: authority()['view'] == result['view'], 'published authority state')
        converge()
        return request, result

    def toy(state, item):
        return next(t for t in state['view']['toys'] if t['id'] == item)

    try:
        server = Instance('server')
        wait(lambda: (v if (v := server.status()) and v['status'] == 'listening' else None), 'server listening')
        bindings = [line.strip() for line in subprocess.check_output([str(Path(os.environ['SystemRoot']) / 'System32/netstat.exe'), '-ano', '-p', 'udp'], text=True).splitlines()
                    if line.split() and line.split()[-1] == str(server.process.pid)]
        check(any(f'127.0.0.1:{port}' in line for line in bindings), 'No observed loopback binding')
        check(all(f'127.0.0.1:{port}' in line for line in bindings), 'Unexpected server network binding')
        first = start_client('player-1'); second = start_client('player-2')
        wait(lambda: len(authority()['connected']) == 2, 'two clients'); baseline = converge()
        passed('loopback server and two independent clients', serverPid=server.process.pid, bind=bindings, worldId=baseline['view']['worldId'])

        start_client('player-3', token='0' * 64, rejected='unpaired-profile')
        start_client('player-3', protocol=wire_version+1, rejected='incompatible-version')
        start_client('player-1', rejected='profile-already-connected')
        start_client('player-5', token='1' * 64, rejected='unpaired-profile')
        check(authority()['view'] == baseline['view'] and len(authority()['connected']) == 2, 'Rejected join altered active play')
        passed('bad credentials, incompatible version, duplicate player and fifth profile rejected without reset')

        a = command(first, 2, item='bucket-1'); b = command(second, 2, item='bucket-1')
        check(a['command']['expectedRevision'] == b['command']['expectedRevision'], 'Race must use the same revision')
        first.control('command', a); second.control('command', b)
        ra = wait(lambda: read(first.out / ('reply-' + a['requestId'] + '.json')), 'first race response')
        rb = wait(lambda: read(second.out / ('reply-' + b['requestId'] + '.json')), 'second race response')
        check(int(ra['accepted']) + int(rb['accepted']) == 1, 'Race produced zero or multiple holders')
        owner, sibling = (first, second) if ra['accepted'] else (second, first)
        wait(lambda: toy(authority(), 'bucket-1')['holder'] == owner.profile, 'winner published'); converge()
        retry = send(sibling, command(sibling, 2, item='bucket-1'))
        check(not retry['accepted'] and retry['outcome'] == 'already-held', 'Held bucket was stolen')
        act(owner, 3, item='bucket-1', target='tap-1', x=150, y=340)
        act(owner, 2, item='bucket-1')
        act(owner, 1, value='orange-pup')
        passed('contested bucket has exactly one holder; avatar switch keeps the hold', winner=owner.profile)

        third = start_client('player-3'); fourth = start_client('player-4')
        wait(lambda: len(authority()['connected']) == 4, 'four clients'); state = converge()
        check(toy(state, 'bucket-1')['holder'] == owner.profile and toy(state, 'bucket-1')['water'] == 3, 'Late join lost held/filled bucket')
        passed('four native clients converge; late join sees current holder and water')
        forged = send(fourth, command(fourth, 4, item='bucket-1', actor=owner.profile))
        check(not forged['accepted'] and forged['outcome'] == 'wrong-player' and authority()['view'] == state['view'], 'Connection impersonated another player')
        passed('commands cannot impersonate another admitted player')

        act(sibling, 2, item='sponge-1'); act(sibling, 5, value='cleanup'); act(sibling, 0, x=210, y=120)
        pour, poured = act(owner, 3, item='bucket-1', target='plant-1', x=810, y=330)
        act(fourth, 0, x=600, y=100); before_duplicate = authority()['view']
        replay = send(owner, pour, poured['sequence'])
        check(replay['accepted'] and replay['duplicate'] and replay['view'] == before_duplicate and toy(replay, 'plant-1')['water'] == 3, 'Repeated pour changed state')
        passed('independent actions continue and delayed duplicate pour applies once')

        act(owner, 2, item='bucket-1'); sibling_before = next(p for p in authority()['view']['players'] if p['id'] == sibling.profile)
        lost_at = time.monotonic(); owner.process.kill(); owner.process.wait(timeout=8); active.pop(owner.profile)
        # Continue through the transport's failure-detection interval.
        act(fourth, 0, x=610, y=110)
        wait(lambda: owner.profile not in authority()['connected'] and toy(authority(), 'bucket-1')['holder'] == '', 'abrupt departure cleanup', seconds=12)
        elapsed = time.monotonic() - lost_at; state = converge()
        check(toy(state, 'sponge-1')['holder'] == sibling.profile and next(p for p in state['view']['players'] if p['id'] == sibling.profile) == sibling_before, 'Departure reset the sibling')
        passed('hard-killed client releases only its hold while others keep playing', observedRecoverySeconds=round(elapsed, 3))
        owner = start_client(owner.profile); wait(lambda: len(authority()['connected']) == 4, 'returning player'); state = converge()
        check(next(p for p in state['view']['players'] if p['id'] == owner.profile)['avatar'] == 'orange-pup', 'Returning avatar identity changed')
        replay = send(owner, pour)
        check(replay['accepted'] and replay['duplicate'] and replay['view'] == state['view'], 'Reconnect repeated a completed pour')
        passed('returning player retains identity and replay history')

        fourth.close(); active.pop(fourth.profile)
        wait(lambda: len(authority()['connected']) == 3, 'graceful departure')
        act(third, 0, x=330, y=140)
        passed('graceful leave does not interrupt remaining client commands')

        committed = authority(); old_epoch = committed['epoch']
        check(toy(committed, 'sponge-1')['holder'] == sibling.profile, 'Restart fixture must contain a held prop')
        server.process.kill(); server.process.wait(timeout=8)
        for c in list(active.values()):
            wait(lambda c=c: (v := c.status()) and v['status'] == 'disconnected', 'server-loss notification', seconds=15)
            c.close()
        active.clear()
        server = Instance('server')
        wait(lambda: (v := server.status()) and v['status'] == 'listening', 'restarted server')
        restored = authority()
        expected = json.loads(json.dumps(committed['view']))
        for t in expected['toys']:
            t['holder'] = ''
        expected['revision'] += 1
        check(restored['epoch'] != old_epoch and restored['view'] == expected and restored['connected'] == [], 'Server recovery lost committed state or retained stale holds')
        start_client('player-1'); start_client('player-2'); wait(lambda: len(authority()['connected']) == 2, 'rejoin restored authority'); recovered = converge()
        replay = send(active[owner.profile], pour)
        check(replay['accepted'] and replay['duplicate'] and replay['view'] == recovered['view'], 'Server restart lost the committed command receipt')
        passed('server hard-stop/restart restores committed world with a new epoch and cleared stale holds')

        report = dict(passed=True, utc=datetime.now(timezone.utc).isoformat(), build=args.build, runId=run_id,
                      checks=results, separateClientProcesses=4, physicalDevicesAccessed=False, loopbackOnly=True,
                      automaticDiscoveryImplemented=False, mobileHostingImplemented=False, independentAreasExercised=False,
                      limitation='Windows native process qualification only. Reconnect/restart was driven by the test harness, not automatic host recovery.')
        write(run / 'result.json', report)
        print('PASS all loopback checks. Evidence: ' + str(run), flush=True)
    except Exception as error:
        write(run / 'result.json', dict(passed=False, build=args.build, runId=run_id, checks=results, error=str(error),
              finalAuthority=read(server.out/'view.json') if server else None,
              processStatuses=[dict(profile=p.profile, exitCode=p.process.poll(), status=read(p.out/'status.json')) for p in processes]))
        print('FAIL ' + str(error) + '. Evidence: ' + str(run), flush=True)
        raise
    finally:
        # Only processes returned by this test's Popen calls are stopped.
        for instance in reversed(processes):
            if instance.process.poll() is None:
                try:
                    instance.close()
                except Exception:
                    instance.process.kill(); instance.process.wait(timeout=10)


if __name__ == '__main__':
    main()
