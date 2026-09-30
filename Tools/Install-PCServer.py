"""Publish a verified idle PC-server bundle. Saves/enrollment live outside the bundle."""
import argparse
import json
from pathlib import Path
from pc_server_installation import install
from server_release import load


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', type=int, required=True)
    parser.add_argument('--family')
    parser.add_argument('--port', type=int)
    args = parser.parse_args(); root = Path(__file__).resolve().parent.parent
    family = args.family or load(root / 'LocalData/ParentServer/settings.json')['family']
    result = install(root, family, args.build, args.port)
    # Enrollment identifiers never need to appear in routine tool output.
    value = result['installation']
    print(json.dumps(dict(result=result['result'], build=value['build'], content=value['content'],
                         port=value['port'], program=value['program']), indent=2))


if __name__ == '__main__': main()
