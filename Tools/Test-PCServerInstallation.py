"""Focused contract/publication checks in owned disposable folders; no live server changes."""
import hashlib
import json
from pathlib import Path
import socket
import tempfile
import unittest
import uuid
import ctypes
import os
from contextlib import contextmanager
from unittest.mock import patch

from pc_server_installation import executable, home, install, installed, verify_bundle
from server_release import ReleaseError, compare, incoming_release, network_release

ROOT=Path(__file__).resolve().parent.parent


def record(path,value):
    path.parent.mkdir(parents=True,exist_ok=True)
    path.write_text(json.dumps(value),encoding='utf-8')


class InstallationTests(unittest.TestCase):
    def setUp(self):
        parent=ROOT/'LocalData/Verification';parent.mkdir(parents=True,exist_ok=True)
        self.temp=tempfile.TemporaryDirectory(prefix='pc-server-',dir=parent)
        self.root=Path(self.temp.name).resolve()
        assert self.root.is_relative_to(parent.resolve())
        self.family=uuid.uuid4().hex
        self.world=self.root/'LocalData/FamilyLAN'/self.family
        record(self.world/'family.json',dict(worldId=self.family,purpose='isolated parent-control acceptance'))
        (self.world/'authority.pairing').write_bytes(b'test-only-no-native-credentials')
        self.release(229);self.release(230);self.release(231,content=34,schema=33,shared='new shared rules')

    def tearDown(self):
        assert self.root.is_relative_to((ROOT/'LocalData/Verification').resolve())
        self.temp.cleanup()

    def release(self,build,content=33,schema=32,shared='unchanged shared rules'):
        folder=self.root/f'Builds/NetworkProbe/G3-0.0.{build}'
        entries=[]
        for role in ('Server','Client'):
            path=folder/role/'LittleWeepsNetwork.exe';path.parent.mkdir(parents=True)
            raw=f'{role} build {build}'.encode();path.write_bytes(raw)
            entries.append(dict(path=f'{role}/LittleWeepsNetwork.exe',sha256=hashlib.sha256(raw).hexdigest()))
        record(folder/'artifact-manifest.json',entries)
        record(folder/'build-summary.json',dict(contract=17,content=content,schema=schema,builds=[dict(role=r,result='Succeeded',errors=0,version=f'0.0.{build}')for r in ('Server','Client')]))
        digest=hashlib.sha256(shared.encode()).hexdigest()
        record(folder/'source-manifest.json',dict(files=[dict(path='Unity/FamilyPlayset/Assets/FamilyPlayset/Code/'+p,sha256=digest)for p in ('Core/WorldLayout.cs','NetworkProbe/NetworkProbe.cs')]))
        return folder

    def provision(self,build=229):return install(self.root,self.family,build,process_reader=lambda:[])

    def test_different_client_build_keeps_compatible_server(self):
        self.provision();before=(home(self.root)/'installation.json').read_bytes()
        decision=compare(network_release(self.root,229),network_release(self.root,230))
        self.assertEqual(decision['result'],'app-only');self.assertFalse(decision['serverChangesAllowed'])
        self.assertEqual(before,(home(self.root)/'installation.json').read_bytes())
        self.assertEqual(compare(network_release(self.root,229),network_release(self.root,231))['result'],'server-update-required')

    def test_replacement_keeps_path_port_and_save(self):
        first=self.provision();port=first['installation']['port']
        raw=json.dumps(dict(schema=32,revision=7,creation='kept'))
        save=self.world/'server-world/world.save';save.parent.mkdir()
        original=('LITTLEWEEPS-SOLO-1\n'+hashlib.sha256(raw.encode()).hexdigest()+'\n'+raw).encode();save.write_bytes(original)
        second=self.provision(230)
        self.assertEqual(second['installation']['port'],port)
        self.assertEqual(second['installation']['program'],first['installation']['program'])
        self.assertEqual(save.read_bytes(),original)
        self.assertTrue(Path(second['installation']['previousBundle']).exists())
        self.assertEqual(self.provision(230)['result'],'already-current')

    def test_running_authority_blocks_publication(self):
        self.provision();original=executable(self.root).read_bytes()
        with self.assertRaises(ReleaseError):install(self.root,self.family,230,process_reader=lambda:[dict(ExecutablePath=str(executable(self.root)),CommandLine='owned test')])
        self.assertEqual(executable(self.root).read_bytes(),original)

    def test_future_save_and_occupied_port_are_not_overwritten(self):
        first=self.provision();raw=json.dumps(dict(schema=33,revision=1))
        save=self.world/'server-world/world.save';save.parent.mkdir()
        save.write_text('LITTLEWEEPS-SOLO-1\n'+hashlib.sha256(raw.encode()).hexdigest()+'\n'+raw,encoding='utf-8')
        with self.assertRaises(ReleaseError):self.provision(230)
        save.unlink()
        with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as occupied:
            occupied.bind(('0.0.0.0',first['installation']['port']))
            with self.assertRaises(OSError):self.provision(230)
        self.assertEqual(installed(self.root)['build'],229)

    def test_changed_artifact_or_incomplete_install_is_blocked(self):
        self.provision();(executable(self.root)).write_bytes(b'changed')
        with self.assertRaises(ReleaseError):verify_bundle(home(self.root)/'current','Server')
        record(home(self.root)/'installation.pending.json',dict(test=True))
        with self.assertRaises(ReleaseError):installed(self.root)

    def test_old_android_metadata_requires_matching_shared_provenance(self):
        folder=self.root/'Builds/AndroidFamilyLAN/G3-0.0.230'
        record(folder/'build-summary.json',dict(result='Succeeded',platform='Android',development=False,version='0.0.230'))
        record(folder/'source-manifest.json',json.loads((self.root/'Builds/NetworkProbe/G3-0.0.230/source-manifest.json').read_text()))
        server=network_release(self.root,229)
        self.assertEqual(compare(server,incoming_release(self.root,230,'android',server))['result'],'app-only')
        record(folder/'source-manifest.json',dict(files=[]))
        with self.assertRaises(ReleaseError):incoming_release(self.root,230,'android',server)

    def test_status_observer_allows_atomic_replacement(self):
        from parent_server import read
        target=self.root/'status.json';next_file=self.root/'next.json'
        record(target,dict(state='before'));record(next_file,dict(state='after'))
        native_fdopen=os.fdopen
        kernel=ctypes.WinDLL('kernel32',use_last_error=True)
        kernel.ReplaceFileW.argtypes=[ctypes.c_wchar_p,ctypes.c_wchar_p,ctypes.c_wchar_p,ctypes.c_ulong,ctypes.c_void_p,ctypes.c_void_p]
        kernel.ReplaceFileW.restype=ctypes.c_int
        @contextmanager
        def replace_while_open(*args,**kwargs):
            with native_fdopen(*args,**kwargs) as opened:
                if not kernel.ReplaceFileW(str(target),str(next_file),None,0,None,None):raise ctypes.WinError(ctypes.get_last_error())
                yield opened
        with patch('parent_server.os.fdopen',side_effect=replace_while_open):
            self.assertEqual(read(target)['state'],'before')
        self.assertEqual(read(target)['state'],'after')


if __name__=='__main__':unittest.main()
