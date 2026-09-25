"""Back up, verify, restore or roll back an enrolled server world on this Windows user account."""
import argparse
import json
from pathlib import Path
from server_recovery import Recovery, unpack, recover_missing, OperationError


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['backup', 'verify', 'restore', 'rollback', 'recover-missing'])
    parser.add_argument('--family')
    parser.add_argument('--build', type=int, default=85)
    parser.add_argument('--backup', type=Path)
    parser.add_argument('--destination', type=Path)
    parser.add_argument('--expected-save-sha256', help='Current world.save SHA-256 (or missing); prevents a stale restore decision.')
    parser.add_argument('--job', help='Completed restore job to roll back; omit for an interrupted restore.')
    args = parser.parse_args()
    try:
        if args.action == 'verify':
            bundle, _, _ = unpack(args.backup)
            result = dict(verified=True, build=bundle['build'], revision=bundle['revision'],
                          protection=bundle['protection'], independentRecoveryQualified=False)
        elif args.action == 'recover-missing': result = recover_missing(args.backup, args.family)
        else:
            recovery = Recovery(args.family, args.build)
            if args.action == 'backup': result = recovery.backup(args.destination)
            elif args.action == 'restore': result = recovery.restore(args.backup, args.expected_save_sha256)
            else: result = recovery.rollback(args.job, args.expected_save_sha256)
        print(json.dumps(result, indent=2))
    except (OperationError, OSError, ValueError, TypeError) as error:
        parser.exit(1, 'Recovery stopped: ' + (str(error) if isinstance(error, OperationError) else 'invalid input or unavailable local file; no automatic reset was attempted.') + '\n')


if __name__ == '__main__': main()
