"""Report whether an app can use the selected server. Never mutates or restarts it."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
import json
from pathlib import Path
from pc_server_installation import selected
from server_release import compare, incoming_release, load, network_release


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--client-build', type=int, required=True)
    parser.add_argument('--platform', choices=['windows', 'android'], default='windows')
    parser.add_argument('--family')
    parser.add_argument('--server-build', type=int, help='Read-only explicit comparison; does not select/deploy a server')
    args = parser.parse_args(); root = PROJECT_ROOT
    try:
        family = args.family or load(root / 'LocalData/ParentServer/settings.json')['family']
        build = args.server_build or selected(root, family)
        server = network_release(root, build)
        result = compare(server, incoming_release(root, args.client_build, args.platform, server))
        result['comparisonOnly'] = args.server_build is not None
    except (OSError, ValueError, KeyError, RuntimeError) as error:
        result = dict(result='unknown', serverChangesAllowed=False, message=str(error))
    print(json.dumps(result, indent=2))
    return 0


if __name__ == '__main__': raise SystemExit(main())
