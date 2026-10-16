"""Resolve a recovered Addressables text catalog without changing source artifacts.

Input is the retained ResourceLocation dump, not raw Addressables catalog JSON.
Outputs belong in ignored research storage. Local presence does not prove that
all serialized object references resolve or that an activity works at runtime.
"""
import argparse
from collections import deque
import hashlib
import json
from pathlib import Path
from urllib.parse import urlparse


def digest(path):
    h = hashlib.sha256()
    with path.open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            h.update(block)
    return h.hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--catalog', type=Path, required=True)
    parser.add_argument('--key', required=True, help='Exact asset path or GUID alias')
    parser.add_argument('--bundle-directory', type=Path, action='append', default=[])
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    catalog = args.catalog.resolve()
    directories = [p.resolve() for p in args.bundle_directory]
    output = args.output.resolve()
    if output == catalog or any(output.is_relative_to(p) for p in directories):
        parser.error('Output must remain outside source artifacts')
    locations, aliases, current = {}, {}, None
    for raw in catalog.open(encoding='utf-8-sig'):
        line = raw.rstrip('\r\n')
        if not line:
            continue
        if not line.startswith('\t'):
            if ' -> ' in line:
                alias, target = line.split(' -> ', 1)
                aliases[alias], current = target, None
            else:
                current = locations.setdefault(line, {'key': line, 'dependencies': []})
        elif current is not None:
            if line.startswith('\t\t'):
                current['dependencies'].append(line.strip())
            elif ': ' in line:
                key, _, value = line.strip().partition(':')
                current[key] = value.strip()

    def canonical(key):
        chain = set()
        while key in aliases:
            if key in chain:
                raise ValueError('Catalog alias cycle: ' + key)
            chain.add(key)
            key = aliases[key]
        return key

    pending, seen, missing = deque([args.key]), set(), []
    while pending:
        key = canonical(pending.popleft())
        if key in seen:
            continue
        seen.add(key)
        if key not in locations:
            missing.append(key)
        else:
            pending.extend(locations[key]['dependencies'])
    closure = [locations[key] for key in sorted(seen) if key in locations]
    bundles = []
    for location in closure:
        url = location.get('InternalId', '')
        if not url.startswith('https://'):
            continue
        name = Path(urlparse(url).path).name
        candidates = [p / name for p in directories if (p / name).is_file()]
        files = []
        for path in candidates:
            with path.open('rb') as stream:
                valid = stream.read(8) == b'UnityFS\0'
            files.append({'path': str(path), 'bytes': path.stat().st_size,
                          'sha256': digest(path), 'unityfs': valid})
        bundles.append({'key': location['key'], 'url': url, 'local_files': files})
    report = {'schema': 1, 'catalog': str(catalog), 'catalog_sha256': digest(catalog),
              'requested_key': args.key, 'resolved_key': canonical(args.key),
              'locations': closure, 'missing_keys': missing, 'remote_bundles': bundles,
              'limits': 'Catalog closure and local byte identity only; no object or runtime acceptance.'}
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(json.dumps({'resolved_key': report['resolved_key'], 'locations': len(closure),
                      'missing_keys': missing, 'remote_bundles': len(bundles),
                      'remote_bundles_without_local_file': sum(not b['local_files'] for b in bundles),
                      'invalid_local_files': sum(not f['unityfs'] for b in bundles for f in b['local_files'])}))
    return int(bool(missing) or any(not f['unityfs'] for b in bundles for f in b['local_files']))


if __name__ == '__main__':
    raise SystemExit(main())
