"""Versioned local recovery bundles. DPAPI enrollment remains bound to this user/PC.

Backups can observe a live checkpoint; restores require the native exclusive save
lock. Immutable preimages and a durable marker make interrupted restores fail
closed. No credential import, world merge, automatic deletion or remote upload.
"""
import base64
from contextlib import contextmanager
import ctypes
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import subprocess
import uuid

from family_pairing import unprotect
from parent_server import ParentServer, OperationError, checkpoint_bytes, operation_lock, windows_processes, command_args
from shared_garden_runtime import ROOT, read

ENROLLMENT = ('family.json', 'authority.pairing', 'issuer.pairing',
              'player-1.pairing', 'player-2.pairing', 'player-3.pairing', 'player-4.pairing')
FILES = ENROLLMENT + ('world.save',)
SAVES = ('world.save', 'world.save.bak', 'world.save.pending')
LIMIT = 3 * 1024 * 1024
# Keep experimental writers out even when a newer family build is qualified.
# 110 and 128 passed native multi-area restore/rollback/re-enrollment acceptance.
# 128 preserves home switches/storage; 130 also passed exact paused-balloon
# schema-5 recovery, interrupted restore and all four enrolled clients rejoining.
# 131 repeats those checks for the outdoor spawn. 171 passed six native recovery
# groups and exact 128-to-171 migration/restoration before coordinated family rollout.
# 172 repeats all six recovery groups with unchanged schema/content and the
# compact activity stream; physical mixed-device acceptance remains separate.
# 174 repeats all six groups with four populated schema-16 discovery workspaces.
# 178 repeats them with sixteen schema-17 mixing trays and original enrollment.
# 187 passes all recovery groups with eighteen pages per player and encrypted
# four-client packet-queue health, preserving original enrollment.
# 191 passes all seven isolated native recovery groups with four rescue trays.
# A concurrent-build load run warned on packet queues; stress/physical gates remain open.
# 192 repeats all seven groups with four prepared schema-20 bubble trays.
# 194 repeats all seven groups with four saved schema-21 liquid-color mixtures.
# 200 repeats all seven groups with schema-22 durable Home cleanup clocks.
# 205 repeats all seven groups with four saved pictures and four stored foods.
QUALIFIED_BUILDS = frozenset(range(83, 92)) | {110, 128, 130, 131, 171, 172, 174, 178, 187, 191, 192, 194, 200, 205}
MAX_QUALIFIED_BUILD = max(QUALIFIED_BUILDS)


def check(value, message):
    if not value:
        raise OperationError(message)


def digest(raw):
    return hashlib.sha256(raw).hexdigest()


def canonical(value):
    check(isinstance(value, str) and uuid.UUID(value).hex == value, 'Invalid recovery identity.')
    return value


def encoded(raw):
    return dict(sha256=digest(raw), data=base64.b64encode(raw).decode('ascii'))


def decoded(entry):
    raw = base64.b64decode(entry['data'], validate=True)
    check(len(raw) <= 1024 * 1024 and digest(raw) == entry['sha256'], 'Backup checksum or size mismatch.')
    return raw


def durable(path, raw, replace=False):
    # Create-only immutable files; replacements use a unique same-directory stage.
    path = Path(path)
    if not replace:
        with path.open('xb') as output:
            output.write(raw); output.flush(); os.fsync(output.fileno())
    else:
        staged = path.with_name(path.name + '.' + uuid.uuid4().hex + '.staged')
        durable(staged, raw)
        os.replace(staged, path)


def json_bytes(value):
    return json.dumps(value, indent=2, allow_nan=False).encode()


def file_bytes(path, limit=1024 * 1024):
    # Reject junction/symlink redirection within a recovery input or destination.
    path = Path(path)
    check(not path.is_symlink() and not path.is_junction(), 'Recovery does not follow linked files.')
    with path.open('rb') as source:
        raw = source.read(limit + 1)
    check(len(raw) <= limit, 'Recovery file exceeds its size limit.')
    return raw


def checkpoint(raw):
    header, checksum, payload = raw.decode('utf-8-sig').split('\n', 2)
    check(header == 'LITTLEWEEPS-SOLO-1' and digest(payload.encode()) == checksum, 'Checkpoint checksum mismatch.')
    value = json.loads(payload)
    check(value.get('schema') in (2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26), 'Unsupported checkpoint version.')
    return value


def build_schema(build):
    # Retained backups must remain verifiable without their old build folders.
    # A newer validator accepting a save does not mean an older writer can load it.
    legacy = {**dict.fromkeys(range(83, 92), 2), 110: 3, 128: 4, 130: 5, 131: 5}
    if build in legacy:
        return legacy[build]
    summary = read(ROOT / f'Builds/NetworkProbe/G3-0.0.{build}/build-summary.json')
    contract = summary.get('contract') if summary else None
    schemas = {4: 2, 5: 3, 6: 4, 7: 5, 8: 6, 9: 7, 10: 8, 11: 9, 12: 10, 13: 11, 14: 12, 15: 13, 16: 14, 17: 15}
    # Discovery retains the build-manifest contract, but has new save/content
    # versions. Accept only the explicitly supported pair, not arbitrary schemas.
    if contract == 17 and (summary.get('schema'), summary.get('content')) == (16, 17):
        return 16
    if contract == 17 and (summary.get('schema'), summary.get('content')) == (26, 27):
        return 26
    if contract == 17 and (summary.get('schema'), summary.get('content')) == (25, 26):
        return 25
    if contract == 17 and (summary.get('schema'), summary.get('content')) == (24, 25):
        return 24
    if contract == 17 and (summary.get('schema'), summary.get('content')) == (23, 24):
        return 23
    if contract == 17 and (summary.get('schema'), summary.get('content')) == (22, 23):
        return 22
    if contract == 17 and (summary.get('schema'), summary.get('content')) == (21, 22):
        return 21
    if contract == 17 and (summary.get('schema'), summary.get('content')) == (20, 21):
        return 20
    if contract == 17 and (summary.get('schema'), summary.get('content')) == (19, 20):
        return 19
    if contract == 17 and (summary.get('schema'), summary.get('content')) == (18, 19):
        return 18
    if contract == 17 and (summary.get('schema'), summary.get('content')) == (17, 18):
        return 17
    check(contract in schemas and (contract < 8 or
          summary.get('schema') == schemas[contract] and summary.get('content') == schemas[contract]+1), 'Unverified build save compatibility.')
    return schemas[contract]


def validate_enrollment(files, world):
    public = json.loads(files['family.json'].decode('utf-8-sig'))
    records = {name: unprotect(files[name]) for name in ENROLLMENT if name.endswith('.pairing')}
    return validate_enrollment_records(public, records, world)


def validate_enrollment_records(public, records, world):
    """Validate identities without requiring their original Windows DPAPI wrapper."""
    from cryptography import x509
    from cryptography.hazmat.primitives import serialization
    check(set(records) == {n for n in ENROLLMENT if n.endswith('.pairing')}, 'Unexpected enrollment records.')
    server, issuer = records['authority.pairing'], records['issuer.pairing']
    clients = [records[f'player-{i}.pairing'] for i in range(1, 5)]
    for field in ('worldId', 'familyId', 'authorityId'):
        canonical(server[field])
        check(public[field] == server[field] and all(c[field] == server[field] for c in clients), 'Enrollment identities do not match.')
    check(server['worldId'] == world and server['schema'] == 1 and server['role'] == 'server', 'Wrong family or enrollment version.')
    check(issuer['familyId'] == server['familyId'] and issuer['caCertificate'] == server['caCertificate'], 'Wrong issuer enrollment.')
    members = {m['profile']: m['credentialHash'] for m in server['members']}
    profiles = [canonical(c['profile']) for c in clients]
    check(len(server['members']) == len(public['profiles']) == len(members) == len(set(profiles)) == 4
          and set(public['profiles']) == set(profiles) == set(members), 'Wrong four-player roster.')
    for client in clients:
        check(client['schema'] == 1 and client['role'] == 'client' and client['serverName'] == server['serverName']
              and client['caCertificate'] == server['caCertificate']
              and digest(client['credential'].encode('ascii')) == members[client['profile']], 'Player enrollment mismatch.')
    check(server['serverName'] == 'lw-' + server['authorityId'] + '.local', 'Wrong authority name.')
    ca = x509.load_pem_x509_certificate(server['caCertificate'].encode())
    cert = x509.load_pem_x509_certificate(server['certificate'].encode())
    cert.verify_directly_issued_by(ca)
    def public_key(key):
        return key.public_bytes(serialization.Encoding.DER, serialization.PublicFormat.SubjectPublicKeyInfo)
    for record, certificate in ((server, cert), (issuer, ca)):
        key = serialization.load_pem_private_key(record['privateKey'].encode(), password=None)
        check(public_key(key.public_key()) == public_key(certificate.public_key()), 'Certificate and protected key differ.')
    return profiles


def validate_world(raw, profiles):
    body = checkpoint(raw)
    result = subprocess.run(['dotnet', 'run', '--project', str(ROOT / 'Tools/RecoveryValidator'),
                             '--configuration', 'Release', '--verbosity', 'quiet'],
                            input=json.dumps(dict(world=body, profiles=profiles)), capture_output=True,
                            text=True, timeout=45, creationflags=subprocess.CREATE_NO_WINDOW)
    check(result.returncode == 0 and result.stdout.strip().endswith('VALID'), 'Checkpoint failed the game rules validator. No world was replaced.')
    return body


def unpack(path):
    try:
        bundle = json.loads(file_bytes(path, LIMIT))
        check(bundle['format'] == 'little-weeps-server-backup' and bundle['version'] == 1
              and bundle['protection'] == 'windows-current-user-dpapi'
              and bundle['protocol'] == 3 and bundle['build'] in QUALIFIED_BUILDS, 'Unsupported backup/build version.')
        canonical(bundle['world']); canonical(bundle['id'])
        check(set(bundle['files']) == set(FILES), 'Unexpected or missing backup file. No paths were extracted.')
        files = {name: decoded(entry) for name, entry in bundle['files'].items()}
        profiles = validate_enrollment(files, bundle['world'])
        body = validate_world(files['world.save'], profiles)
        check(body['schema'] <= build_schema(bundle['build']), 'Checkpoint is newer than its declared source build.')
        check(body['worldId'] == bundle['checkpointWorld'] and body['revision'] == bundle['revision'], 'Backup metadata differs from checkpoint.')
        return bundle, files, body
    except OperationError:
        raise
    except Exception as error:
        # Never relay a deserializer/crypto exception that could contain secrets.
        raise OperationError('Backup cannot be verified for this Windows user. No world was replaced.') from error


def recover_missing(path, expected_family):
    """Reconstruct a missing canonical family directory, never replace one.

    All files are prepared privately before the directory becomes startable.
    Windows directory rename refuses an existing target, including a concurrent
    recovery winner. This is same-user recovery, not a DPAPI portability bypass.
    """
    bundle, files, body = unpack(path)
    return publish_missing(bundle, files, body, expected_family)


def publish_missing(bundle, files, body, expected_family, fault=None):
    """Publish validated, destination-protected files with create-only semantics."""
    check(bundle['world'] == canonical(expected_family), 'Wrong family selected for reconstruction.')
    destination = ROOT / 'LocalData/FamilyLAN' / expected_family
    check(destination.parent.resolve() == destination.parent, 'Recovery does not follow linked directories.')
    check(not os.path.lexists(destination), 'The family directory exists. Use guarded restore; reconstruction never replaces it.')
    for native in windows_processes():
        args = command_args(native.get('CommandLine') or '')
        if '-familyNetworkConfig' in args:
            index = args.index('-familyNetworkConfig') + 1
            check(index < len(args) and Path(args[index]).resolve().parent != destination,
                  'A process still refers to this family. Stop it before reconstruction.')
    staged = destination.parent / ('recovery-stage-' + uuid.uuid4().hex)
    destination.parent.mkdir(parents=True, exist_ok=True)
    staged.mkdir(); (staged / 'server-world').mkdir()
    for name in ENROLLMENT: durable(staged / name, files[name])
    if fault: fault('enrollment')
    for name in ('world.save', 'world.save.bak'): durable(staged / 'server-world' / name, files['world.save'])
    check(all(file_bytes(staged / n) == files[n] for n in ENROLLMENT), 'Staged enrollment differs.')
    check(all(file_bytes(staged / 'server-world' / n) == files['world.save']
              for n in ('world.save', 'world.save.bak')), 'Staged checkpoint differs.')
    if fault: fault('publish')
    # No startup config/launcher record or process is copied from the old host.
    os.rename(staged, destination)
    return dict(reconstructed=True, revision=body['revision'], started=False, independentRecoveryQualified=False)


@contextmanager
def authority_lock(folder):
    kernel = ctypes.WinDLL('kernel32', use_last_error=True)
    kernel.CreateFileW.argtypes = [ctypes.c_wchar_p, ctypes.c_ulong, ctypes.c_ulong, ctypes.c_void_p,
                                  ctypes.c_ulong, ctypes.c_ulong, ctypes.c_void_p]
    kernel.CreateFileW.restype = ctypes.c_void_p
    kernel.CloseHandle.argtypes = [ctypes.c_void_p]
    # Same lock file and FileShare.None as the native authority. A concurrent
    # direct launch fails, even if it raced the desktop process/status preflight.
    handle = kernel.CreateFileW(str(folder / 'authority.lock'), 0xC0000000, 0, None, 4, 0x80, None)
    check(handle != ctypes.c_void_p(-1).value, 'The world is in use. Stop the server before restoring.')
    try:
        yield
    finally:
        kernel.CloseHandle(handle)


class Recovery:
    def __init__(self, family, build=85):
        self.controller = ParentServer(canonical(family), build)
        self.family, self.build, self.root = family, build, self.controller.root
        self.marker = self.root / 'recovery.pending.json'
        check(self.root.resolve() == self.root and not (self.root / 'server-world').is_symlink()
              and not (self.root / 'server-world').is_junction(), 'Recovery world must be a normal project directory.')

    def enrollment(self):
        return {name: file_bytes(self.root / name, 65536) for name in ENROLLMENT}

    def backup(self, destination=None):
        with operation_lock(self.root):
            check(not self.marker.exists(), 'Resolve interrupted recovery before backing up.')
            files = self.enrollment()
            profiles = validate_enrollment(files, self.family)
            files['world.save'] = checkpoint_bytes(self.root / 'server-world/world.save')[0]
            body = validate_world(files['world.save'], profiles)
            check({n: files[n] for n in ENROLLMENT} == self.enrollment(), 'Enrollment changed while backing up.')
            candidates = self.controller.processes()
            # A live backup labels the running writer, not the prepared next build.
            latest = read(self.root / 'latest-server.json')
            source_build = candidates[0]['build'] if len(candidates) == 1 else latest['build'] if latest else self.build
            check(len(candidates) <= 1 and source_build in QUALIFIED_BUILDS, 'Unverified source build.')
            check(body['schema'] <= build_schema(source_build), 'Checkpoint is newer than its source build.')
            bundle = dict(format='little-weeps-server-backup', version=1, id=uuid.uuid4().hex,
                          world=self.family, checkpointWorld=body['worldId'], revision=body['revision'],
                          build=source_build, protocol=3, createdAt=datetime.now(timezone.utc).isoformat(),
                          protection='windows-current-user-dpapi', files={name: encoded(raw) for name, raw in files.items()})
            destination = Path(destination or ROOT / 'LocalData/ServerBackups').resolve()
            destination.mkdir(parents=True, exist_ok=True)
            path = destination / (bundle['id'] + '.lwbackup')
            durable(path, json_bytes(bundle))
            unpack(path)  # Read back the actual file, including DPAPI and game rules.
            return dict(path=str(path), sha256=digest(file_bytes(path, LIMIT)), revision=body['revision'],
                        build=source_build, verified=True, independentRecoveryQualified=False)

    def _stopped(self):
        check(not self.controller.processes(), 'Stop the server before restoring; no player will be disconnected automatically.')

    def _matching(self, bundle, files):
        check(bundle['world'] == self.family and bundle['build'] <= self.build, 'Wrong family or newer-build backup.')
        check(self.enrollment() == {n: files[n] for n in ENROLLMENT}, 'Protected enrollment differs. Credential migration is a separate operation.')

    def restore(self, path, expected_current_sha256, fault=None):
        bundle, files, body = unpack(path)
        check(body['schema'] <= build_schema(self.build), 'Checkpoint is newer than the selected destination build.')
        self._matching(bundle, files)
        with operation_lock(self.root):
            check(not self.marker.exists(), 'Resolve interrupted recovery first.')
            self._stopped()
            folder = self.root / 'server-world'
            with authority_lock(folder):
                self._matching(bundle, files)
                current = file_bytes(folder / 'world.save') if (folder / 'world.save').exists() else None
                check((digest(current) if current is not None else 'missing') == expected_current_sha256,
                      'The current save changed. Inspect it before choosing a restore.')
                self.controller.save_intent(False)
                # Keep exact preimages, including a corrupt save if this is a
                # repair. Checksums permit rollback without claiming it is valid.
                previous = {name: encoded(file_bytes(folder / name)) if (folder / name).exists() else None for name in SAVES}
                job = uuid.uuid4().hex
                target = ROOT / 'LocalData/RecoveryJobs' / job; target.mkdir(parents=True)
                preimage = dict(world=self.family, enrollment={n: digest(files[n]) for n in ENROLLMENT}, files=previous)
                durable(target / 'before.json', json_bytes(preimage))
                durable(target / 'incoming.lwbackup', file_bytes(path, LIMIT))
                durable(self.marker, json_bytes(dict(job=job, beforeSha256=digest(json_bytes(preimage)))))
                if fault: fault('marker')
                # Marker is durable before touching any of the three load paths.
                for name in SAVES:
                    if name == 'world.save.pending':
                        if (folder / name).exists(): (folder / name).unlink()
                    else:
                        durable(folder / name, files['world.save'], replace=True)
                    if fault: fault(name)
                check(file_bytes(folder / 'world.save') == files['world.save'] and file_bytes(folder / 'world.save.bak') == files['world.save'], 'Restore verification failed; recovery remains blocked.')
                durable(target / 'completed.json', json_bytes(dict(revision=body['revision'], utc=datetime.now(timezone.utc).isoformat())))
                self.marker.unlink()
                return dict(restored=True, revision=body['revision'], rollbackJob=job, started=False)

    def rollback(self, job=None, expected_current_sha256=None):
        with operation_lock(self.root):
            self._stopped()
            marker = read(self.marker)
            check(marker or job, 'No interrupted restore or rollback job was selected.')
            check(not marker or not job or marker['job'] == job, 'A different recovery is pending.')
            job = canonical(marker['job'] if marker else job)
            folder = self.root / 'server-world'
            with authority_lock(folder):
                target = ROOT / 'LocalData/RecoveryJobs' / job
                raw = file_bytes(target / 'before.json', LIMIT); previous = json.loads(raw)
                check(not marker or digest(raw) == marker['beforeSha256'], 'Recovery preimage checksum mismatch.')
                check(previous['world'] == self.family and previous['enrollment'] == {n: digest(v) for n, v in self.enrollment().items()}, 'Recovery enrollment mismatch.')
                check(set(previous['files']) == set(SAVES), 'Unexpected rollback file.')
                files = {n: decoded(v) if v else None for n, v in previous['files'].items()}
                if not marker:
                    current = file_bytes(folder / 'world.save') if (folder / 'world.save').exists() else None
                    check((digest(current) if current is not None else 'missing') == expected_current_sha256, 'Inspect the current save before rolling back.')
                    # Never roll back an earlier restore by discarding play since
                    # then: preserve those bytes in a separate create-only record.
                    retained = {n: encoded(file_bytes(folder / n)) if (folder / n).exists() else None for n in SAVES}
                    durable(target / ('before-rollback-' + uuid.uuid4().hex + '.json'), json_bytes(retained))
                    durable(self.marker, json_bytes(dict(job=job, beforeSha256=digest(raw))))
                self.controller.save_intent(False)
                for name, value in files.items():
                    if value is None:
                        if (folder / name).exists(): (folder / name).unlink()
                    else: durable(folder / name, value, replace=True)
                check(all((file_bytes(folder / n) == v if v is not None else not (folder / n).exists()) for n, v in files.items()), 'Rollback verification failed.')
                self.marker.unlink()
                return dict(rolledBack=True, started=False)
