"""Read-only release compatibility. Build numbers never select a replacement server."""
import hashlib
import json
from pathlib import Path
import re


class ReleaseError(RuntimeError):
    pass


def load(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def number(value, label):
    if type(value) is not int or not 1 <= value <= 9999:
        raise ReleaseError('Invalid ' + label)
    return value


def shared_digest(folder):
    # Compare only compiled shared rules/wire sources and package versions.
    # Art, local UI and Editor/build-tool edits do not change this contract.
    rows = load(Path(folder) / 'source-manifest.json')['files']
    chosen = {r['path']: r['sha256'] for r in rows if (
        '/Code/Core/' in r['path'] or '/Code/NetworkProbe/' in r['path']
        or r['path'].endswith('/Packages/manifest.json')
        or r['path'].endswith('/Packages/packages-lock.json'))
        and not r['path'].endswith('.meta')}
    if not any(p.endswith('/Core/WorldLayout.cs') for p in chosen) or not any(p.endswith('/NetworkProbe/NetworkProbe.cs') for p in chosen):
        raise ReleaseError('Shared source provenance is missing')
    if any(not re.fullmatch('[0-9a-f]{64}', h) for h in chosen.values()):
        raise ReleaseError('Invalid shared source provenance')
    return hashlib.sha256(json.dumps(chosen, sort_keys=True, separators=(',', ':')).encode()).hexdigest()


def network_release(root, build):
    build = number(build, 'build')
    folder = Path(root) / f'Builds/NetworkProbe/G3-0.0.{build}'
    summary = load(folder / 'build-summary.json')
    rows = summary.get('builds', [])
    if len(rows) != 2 or {r.get('role') for r in rows} != {'Client', 'Server'} or any(
            r.get('result') != 'Succeeded' or r.get('errors') != 0
            or r.get('version') != f'0.0.{build}' for r in rows):
        raise ReleaseError('Successful matching release client/server required')
    contract = number(summary['contract'], 'build contract')
    if contract > 17:
        raise ReleaseError('Unrecognized build contract; explicit support is required')
    protocol = number(summary.get('protocol', 3 if contract >= 4 else 2 if contract >= 3 else 1), 'protocol')
    legacy = {4: 3, 5: 4, 6: 5, 7: 6}
    content = number(summary['content'] if contract >= 8 else legacy.get(contract, 2 if contract == 3 else 1), 'content')
    schema = number(summary.get('schema', max(2, content - 1)) if contract < 8 else summary['schema'], 'schema')
    return dict(build=build, protocol=protocol, content=content, schema=schema,
                folder=str(folder), sharedDigest=shared_digest(folder))


def incoming_release(root, build, platform, server):
    if platform == 'windows':
        return network_release(root, build)
    if platform != 'android':
        raise ReleaseError('Use the corresponding Windows release to review this platform')
    folder = Path(root) / f'Builds/AndroidFamilyLAN/G3-0.0.{number(build, "build")}'
    summary = load(folder / 'build-summary.json')
    if summary.get('result') != 'Succeeded' or summary.get('platform') != 'Android' or summary.get('development') is not False or summary.get('version') != f'0.0.{build}':
        raise ReleaseError('Successful non-development Android release required')
    digest = shared_digest(folder)
    # Old Android summaries lack compatibility fields. Derive them only from
    # an actual compiled authority with exactly the same shared source hashes.
    references = [server]
    if (Path(root) / f'Builds/NetworkProbe/G3-0.0.{build}/build-summary.json').exists():
        references.append(network_release(root, build))
    for reference in references:
        if digest == reference['sharedDigest']:
            return dict(build=build, protocol=reference['protocol'], content=reference['content'],
                        schema=reference['schema'], folder=str(folder), sharedDigest=digest)
    raise ReleaseError('Android shared sources have no matching compiled server contract')


def compare(server, client):
    compatible = (server['protocol'], server['content']) == (client['protocol'], client['content'])
    result = 'app-only' if compatible else 'server-update-required'
    if compatible and server['sharedDigest'] != client['sharedDigest']:
        result = 'review-required'
    return dict(result=result, serverBuild=server['build'], clientBuild=client['build'],
                serverProtocol=server['protocol'], clientProtocol=client['protocol'],
                serverContent=server['content'], clientContent=client['content'],
                serverChangesAllowed=False,
                message='Install the app only; preserve the server and its settings.' if result == 'app-only'
                else 'Shared rules changed without a compatibility change; review before rollout. Leave the server running.' if result == 'review-required'
                else 'Prepare a coordinated server/client update. Do not replace the server during an app install.')
