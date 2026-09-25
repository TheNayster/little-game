"""Export, verify or reconstruct a missing family using a portable encrypted backup.

Passphrases are entered in this terminal, never in arguments, environment or chat.
This tool does not start a server. Keep the previous authority stopped before
starting a recovered world on another PC. Existing families are never replaced.
"""
import argparse
import getpass
import hmac
import json
from pathlib import Path
import sys
import warnings

from portable_recovery import export, verify, recover_missing, password_bytes
from server_recovery import OperationError, check


def ask_password(confirm=False):
    check(sys.stdin.isatty(), 'Run this command in an interactive terminal; redirected passphrase input is disabled.')
    with warnings.catch_warnings():
        # getpass otherwise falls back to echoed input. Refuse that fallback.
        warnings.simplefilter('error', getpass.GetPassWarning)
        password = getpass.getpass('Backup passphrase (hidden): ')
        password_bytes(password)
        if confirm:
            repeated = getpass.getpass('Repeat passphrase (hidden): ')
            check(hmac.compare_digest(password.encode(), repeated.encode()), 'Passphrases do not match; no backup created.')
    return password


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=['export', 'verify', 'recover-missing'])
    parser.add_argument('--backup', type=Path, required=True)
    parser.add_argument('--destination', type=Path, help='New .lwportable file for export')
    parser.add_argument('--family', help='Expected world ID for missing-family recovery')
    parser.add_argument('--build', type=int, help='Qualified destination server build for recovery')
    args = parser.parse_args()
    try:
        if args.action == 'export':
            check(args.destination is not None, 'Export needs --destination.')
        elif args.action == 'recover-missing':
            check(args.family is not None and args.build is not None, 'Recovery needs --family and --build.')
        password = ask_password(confirm=args.action == 'export')
        if args.action == 'export': result = export(args.backup, args.destination, password)
        elif args.action == 'verify': result = verify(args.backup, password)
        else: result = recover_missing(args.backup, password, args.family, args.build)
        print(json.dumps(result, indent=2))
    except (KeyboardInterrupt, EOFError):
        parser.exit(1, 'Cancelled. No server was started.\n')
    except Exception as error:
        message = str(error) if isinstance(error, OperationError) else 'Portable recovery failed; no server was started. Keep the backup and retry after checking the destination.'
        parser.exit(1, message + '\n')


if __name__ == '__main__': main()
