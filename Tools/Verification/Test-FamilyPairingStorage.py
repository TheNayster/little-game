"""Isolated Windows DPAPI round trip; no real family credential is used/logged."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import json
import secrets
import uuid
from family_pairing import ROOT,read_record,write_record

folder=ROOT/'LocalData/Verification'/('pairing-storage-'+uuid.uuid4().hex)
folder.mkdir(parents=True)
sample=dict(role='client',credential=secrets.token_hex(32),profile=uuid.uuid4().hex)
file=folder/'client.pairing';write_record(file,sample)
assert sample['credential'].encode() not in file.read_bytes()
assert read_record(file)==sample
try:write_record(file,dict(credential='replacement'))
except FileExistsError:pass
else:raise AssertionError('Credential replaced')
assert read_record(file)==sample
bad=folder/'malformed.pairing';bad.write_bytes(b'not an encrypted family credential')
try:read_record(bad)
except RuntimeError:pass
else:raise AssertionError('Unprotected data accepted')
result=dict(passed=True,encryptedRoundTrip=True,createOnlyRetainsIdentity=True,plaintextRejected=True)
(folder/'result.json').write_text(json.dumps(result,indent=2));print(json.dumps(result));print('Evidence:',folder/'result.json')
