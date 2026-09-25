"""Password-encrypted server recovery; plaintext enrollment exists only in memory.

Version 1 fixes the crypto profile so an untrusted file cannot select a costly KDF:
scrypt N=131072/r=8/p=1, 16-byte salt, AES-256-GCM, 12-byte nonce, full 16-byte tag.
The complete header is authenticated. No compression, archive extraction, network,
credential rotation, existing-family replacement or automatic startup occurs.
"""
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import uuid

from cryptography.hazmat.primitives.ciphers.aead import AESGCM
from cryptography.hazmat.primitives.kdf.scrypt import Scrypt

from family_pairing import protect, unprotect
from server_recovery import (ENROLLMENT, FILES, LIMIT, MAX_QUALIFIED_BUILD,
                             OperationError, canonical, check, decoded, digest,
                             durable, encoded, file_bytes, json_bytes,
                             publish_missing, unpack, validate_enrollment_records,
                             validate_enrollment, validate_world)

MAGIC = b'LITTLE-WEEPS-PORTABLE-1\n'
HEADER_SIZE = len(MAGIC) + 16 + 12
FILE_LIMIT = HEADER_SIZE + LIMIT + 16
PAIRINGS = tuple(n for n in ENROLLMENT if n.endswith('.pairing'))
FIELDS = {'format', 'version', 'id', 'world', 'checkpointWorld', 'revision',
          'build', 'protocol', 'createdAt', 'files'}


def password_bytes(password):
    check(isinstance(password, str) and 16 <= len(password) <= 1024
          and password.strip() == password, 'Use a passphrase of 16–1024 characters without surrounding spaces.')
    return password.encode('utf-8')


def key(password, salt):
    return Scrypt(salt=salt, length=32, n=2**17, r=8, p=1).derive(password_bytes(password))


def seal(payload, password):
    check(len(payload) <= LIMIT, 'Portable backup exceeds its size limit.')
    salt, nonce = os.urandom(16), os.urandom(12)
    header = MAGIC + salt + nonce
    return header + AESGCM(key(password, salt)).encrypt(nonce, payload, header)


def open_sealed(raw, password):
    check(HEADER_SIZE + 16 <= len(raw) <= FILE_LIMIT and raw.startswith(MAGIC),
          'Unsupported or incomplete portable backup.')
    header = raw[:HEADER_SIZE]
    return AESGCM(key(password, header[len(MAGIC):len(MAGIC) + 16])).decrypt(
        header[-12:], raw[HEADER_SIZE:], header)


def strict_json(raw):
    def pairs(values):
        result = {}
        for name, value in values:
            check(name not in result, 'Duplicate recovery field.')
            result[name] = value
        return result
    def invalid_constant(_):
        raise OperationError('Non-finite recovery value.')
    return json.loads(raw, object_pairs_hook=pairs, parse_constant=invalid_constant)


def validate_payload(payload):
    bundle = strict_json(payload)
    check(set(bundle) == FIELDS and bundle['format'] == 'little-weeps-portable-payload'
          and type(bundle['version']) is int and bundle['version'] == 1
          and type(bundle['protocol']) is int and bundle['protocol'] == 3
          and type(bundle['build']) is int and 83 <= bundle['build'] <= MAX_QUALIFIED_BUILD,
          'Unsupported portable backup/build version.')
    canonical(bundle['world']); canonical(bundle['id'])
    check(set(bundle['files']) == set(FILES), 'Unexpected or missing portable file.')
    files = {name: decoded(entry) for name, entry in bundle['files'].items()}
    check(all(len(files[n]) <= 32768 for n in ENROLLMENT), 'Enrollment record exceeds its size limit.')
    records = {name: strict_json(files[name]) for name in PAIRINGS}
    profiles = validate_enrollment_records(strict_json(files['family.json']), records, bundle['world'])
    body = validate_world(files['world.save'], profiles)
    check(type(bundle['revision']) is int and bundle['revision'] == body['revision']
          and bundle['checkpointWorld'] == body['worldId'], 'Portable checkpoint metadata differs.')
    return bundle, files, records, body


def load(path, password):
    try:
        return validate_payload(open_sealed(file_bytes(path, FILE_LIMIT), password))
    except Exception:
        # JSON/crypto exceptions can include data. Never expose their arguments,
        # tracebacks or whether a particular credential was present in the file.
        raise OperationError('Portable backup could not be verified. Check the passphrase and backup file; nothing was restored.') from None


def summary(bundle, body):
    return dict(world=bundle['world'], revision=body['revision'], build=bundle['build'],
                verified=True, profiles=4, independentRecoveryQualified=False)


def export(local_backup, destination, password):
    password_bytes(password)
    destination = Path(destination).absolute()
    check(destination.suffix == '.lwportable', 'Choose a .lwportable destination file.')
    check(destination.parent.is_dir() and destination.parent.resolve() == destination.parent,
          'Choose an existing destination directory without links.')
    check(not os.path.lexists(destination), 'Portable backup already exists; choose a new filename.')
    bundle, files, body = unpack(local_backup)
    portable_files = dict(files)
    for name in PAIRINGS:
        portable_files[name] = json_bytes(unprotect(files[name]))
    portable = dict(format='little-weeps-portable-payload', version=1, id=uuid.uuid4().hex,
                    world=bundle['world'], checkpointWorld=bundle['checkpointWorld'],
                    revision=bundle['revision'], build=bundle['build'], protocol=3,
                    createdAt=datetime.now(timezone.utc).isoformat(),
                    files={name: encoded(raw) for name, raw in portable_files.items()})
    # Only authenticated ciphertext crosses the disk boundary. The original
    # DPAPI bundle is untouched and remains useful for same-user recovery.
    durable(destination, seal(json_bytes(portable), password))
    verified, _, _, verified_body = load(destination, password)
    check(verified_body == body, 'Read-back verification differs from the source checkpoint.')
    return dict(summary(verified, verified_body), path=str(destination),
                sha256=digest(file_bytes(destination, FILE_LIMIT)))


def verify(path, password):
    bundle, _, _, body = load(path, password)
    return summary(bundle, body)


def verify_bytes(raw, password):
    """Read-only verification for a bounded upload; never write decrypted data."""
    try:
        bundle, _, _, body = validate_payload(open_sealed(raw, password))
        return summary(bundle, body)
    except Exception:
        raise OperationError('Portable backup could not be verified. Check the passphrase and backup file; nothing was restored.') from None


def recover_missing(path, password, expected_family, build, fault=None):
    bundle, files, records, body = load(path, password)
    check(bundle['world'] == canonical(expected_family), 'Wrong family selected for reconstruction.')
    check(type(build) is int and bundle['build'] <= build <= MAX_QUALIFIED_BUILD,
          'Select a qualified destination build at least as new as the backup.')
    # Rewrap with THIS Windows user's DPAPI. Original machine blobs are neither
    # required nor copied. Plaintext records are never written as recovery files.
    protected = dict(files)
    for name in PAIRINGS:
        protected[name] = protect(records[name])
    validate_enrollment(protected, expected_family)
    return publish_missing(bundle, protected, body, expected_family, fault=fault)
