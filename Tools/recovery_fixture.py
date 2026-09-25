"""Disposable enrolled world for native recovery tests; never selects the family world."""
import socket
import subprocess
import uuid
from family_pairing import create_family, write_record
from parent_server import ParentServer
from shared_garden_runtime import ROOT, Instance, read, write, wait


class RecoveryFixture:
    def __init__(self, build):
        authority, players, issuer = create_family()
        self.build, self.run_id = build, authority['worldId']
        self.path = ROOT / 'LocalData/FamilyLAN' / self.run_id
        self.path.mkdir(parents=True)
        write(self.path / 'family.json', dict(worldId=self.run_id, familyId=authority['familyId'],
              authorityId=authority['authorityId'], profiles=[p['profile'] for p in players],
              purpose='isolated parent-control acceptance'))
        write_record(self.path / 'authority.pairing', authority)
        write_record(self.path / 'issuer.pairing', issuer)
        for i, player in enumerate(players, 1): write_record(self.path / f'player-{i}.pairing', player)
        self.players, self.instances = players, []
        self.controller = ParentServer(self.run_id, build)

    def launch(self, index=0, pairing_path=None):
        v = Instance.__new__(Instance)
        v.run = self; v.role = 'client' if index else 'server'
        v.profile = self.players[index - 1]['profile'] if index else ''
        v.serial = 0; v.garden_serial = 0; v.identity = uuid.uuid4().hex
        v.out = self.path / v.identity; v.out.mkdir()
        with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as probe:
            probe.bind(('127.0.0.1', 0)); port = probe.getsockname()[1]
        config = self.path / (v.identity + '.config.json')
        write(config, dict(runId=self.run_id, instanceId=v.identity, role=v.role, port=port,
              protocol=3, content=3, pairingPath=str(pairing_path or self.path / (f'player-{index}.pairing' if index else 'authority.pairing')),
              presentation=True, verifyGarden=True, interactive=True, persistentServer=not index))
        exe = ROOT / f'Builds/NetworkProbe/G3-0.0.{self.build}' / ('Client' if index else 'Server') / 'LittleWeepsNetwork.exe'
        args = [str(exe), '-familyNetworkConfig', str(config), '-logFile', str(v.out / 'player.log')]
        args += ['-screen-fullscreen', '0', '-screen-width', '960', '-screen-height', '640'] if index else ['-batchmode', '-nographics']
        startup = subprocess.STARTUPINFO(); startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW; startup.wShowWindow = 0
        v.process = subprocess.Popen(args, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                                     startupinfo=startup, creationflags=subprocess.CREATE_NO_WINDOW)
        self.instances.append(v)
        return v

    def join(self, index):
        v = self.launch(index)
        wait(lambda: v.status() and v.status()['status'] == 'connected' and v.input('inspect')['shared'], 'fixture player joins', 45)
        return v

    def stop(self):
        for v in self.instances:
            if v.role == 'client' and v.process.poll() is None: v.close()
        state = wait(lambda: (s if (s := self.controller.snapshot())['players'] == 0 else None), 'empty fixture')
        return self.controller.stop(state['instanceId'])

    def cleanup(self):
        for v in reversed(self.instances):
            if v.process.poll() is None:
                try: v.close()
                except Exception: v.process.kill(); v.process.wait(timeout=10)
        for native in self.controller.processes():
            control = read(native['output'] / 'control.json') or {}
            write(native['output'] / 'control.json', dict(serial=control.get('serial', 0) + 1, kind='quit'))
            wait(lambda: read(native['output'] / 'status.json')['status'] == 'stopped', 'test authority cleanup')
