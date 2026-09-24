"""Parent-operated Windows enrollment for the G3 LAN qualification.

Run with `uv run --with cryptography python Tools/family_pairing.py`.
Creates a new isolated family; never resets an existing family or saves. Device
enrollment UI/QR transfer and iOS Keychain integration are separate next steps.
"""
import argparse
import ctypes as C
from ctypes import wintypes as W
from datetime import datetime, timedelta, timezone
import hashlib
import json
import os
from pathlib import Path
import secrets
import uuid

ROOT = Path(__file__).resolve().parent.parent


class Blob(C.Structure):
    _fields_ = [('length', W.DWORD), ('data', C.POINTER(C.c_ubyte))]


def protect(value):
    if os.name != 'nt':
        raise RuntimeError('Windows parent enrollment required')
    plain = json.dumps(value, separators=(',', ':')).encode()
    buffer = (C.c_ubyte * len(plain)).from_buffer_copy(plain)
    source, target = Blob(len(plain), buffer), Blob()
    crypt = C.WinDLL('crypt32', use_last_error=True)
    crypt.CryptProtectData.argtypes = [C.POINTER(Blob), W.LPCWSTR, C.c_void_p, C.c_void_p, C.c_void_p, W.DWORD, C.POINTER(Blob)]
    crypt.CryptProtectData.restype = W.BOOL
    free = C.WinDLL('kernel32').LocalFree
    free.argtypes, free.restype = [C.c_void_p], C.c_void_p
    try:
        if not crypt.CryptProtectData(C.byref(source), 'Little Weeps family enrollment', None, None, None, 1, C.byref(target)):
            raise RuntimeError('Windows could not protect enrollment')
        return C.string_at(target.data, target.length)
    finally:
        C.memset(buffer, 0, len(plain))
        if target.data:
            free(target.data)


def write_record(path, value):
    # Create-only: replacing a credential must be an explicit later revocation
    # operation, never a side effect of starting the game again.
    with path.open('xb') as output:
        output.write(protect(value))


def create_family():
    from cryptography import x509
    from cryptography.hazmat.primitives import hashes, serialization
    from cryptography.hazmat.primitives.asymmetric import rsa
    from cryptography.x509.oid import NameOID, ExtendedKeyUsageOID
    ids = {name: uuid.uuid4().hex for name in ('familyId', 'authorityId', 'worldId')}
    name = 'lw-' + ids['authorityId'] + '.local'
    now = datetime.now(timezone.utc)
    root_key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
    server_key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
    root_name = x509.Name([x509.NameAttribute(NameOID.COMMON_NAME, 'Little Weeps ' + ids['familyId'])])
    ca = (x509.CertificateBuilder().subject_name(root_name).issuer_name(root_name).public_key(root_key.public_key())
          .serial_number(x509.random_serial_number()).not_valid_before(now-timedelta(days=1)).not_valid_after(now+timedelta(days=730))
          .add_extension(x509.BasicConstraints(ca=True, path_length=0), critical=True)
          .add_extension(x509.KeyUsage(False, False, False, False, False, True, True, False, False), critical=True)
          .sign(root_key, hashes.SHA256()))
    certificate = (x509.CertificateBuilder().subject_name(x509.Name([x509.NameAttribute(NameOID.COMMON_NAME, name)]))
                   .issuer_name(root_name).public_key(server_key.public_key()).serial_number(x509.random_serial_number())
                   .not_valid_before(now-timedelta(days=1)).not_valid_after(now+timedelta(days=365))
                   .add_extension(x509.BasicConstraints(ca=False, path_length=None), critical=True)
                   .add_extension(x509.SubjectAlternativeName([x509.DNSName(name)]), critical=False)
                   .add_extension(x509.ExtendedKeyUsage([ExtendedKeyUsageOID.SERVER_AUTH]), critical=False)
                   .add_extension(x509.KeyUsage(True, False, True, False, False, False, False, False, False), critical=True)
                   .sign(root_key, hashes.SHA256()))
    def pem(key):
        return key.private_bytes(serialization.Encoding.PEM, serialization.PrivateFormat.TraditionalOpenSSL,
                                 serialization.NoEncryption()).decode('ascii')
    base = dict(schema=1, **ids, serverName=name, caCertificate=ca.public_bytes(serialization.Encoding.PEM).decode('ascii'))
    clients = [dict(base, role='client', profile=uuid.uuid4().hex, credential=secrets.token_hex(32)) for _ in range(4)]
    server = dict(base, role='server', certificate=certificate.public_bytes(serialization.Encoding.PEM).decode('ascii'),
                  privateKey=pem(server_key), members=[dict(profile=c['profile'], credentialHash=hashlib.sha256(c['credential'].encode()).hexdigest()) for c in clients])
    issuer = dict(familyId=ids['familyId'], caCertificate=base['caCertificate'], privateKey=pem(root_key))
    return server, clients, issuer


def provision():
    server, clients, issuer = create_family()
    folder = ROOT / 'LocalData/FamilyLAN' / server['worldId']
    folder.mkdir(parents=True, exist_ok=False)
    write_record(folder / 'authority.pairing', server)
    write_record(folder / 'issuer.pairing', issuer)
    for i, client in enumerate(clients, 1):
        write_record(folder / f'player-{i}.pairing', client)
    public = dict(worldId=server['worldId'], familyId=server['familyId'], authorityId=server['authorityId'],
                  profiles=[c['profile'] for c in clients], certificateExpiresInDays=365,
                  status='Windows parent-provisioned proof; mobile enrollment pending')
    (folder / 'family.json').write_text(json.dumps(public, indent=2), encoding='utf-8')
    print('Created protected family enrollment:', folder)


if __name__ == '__main__':
    argparse.ArgumentParser(description=__doc__).parse_args()
    provision()
