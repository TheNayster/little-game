"""Serialize parent-helper launches and reuse only the matching local service."""
import argparse
import http.client
import json
import shutil
import subprocess
import sys
import time
from urllib.parse import urlsplit
import webbrowser

from parent_server import ParentServer, OperationError, operation_lock
from parent_startup import HELPER_PROTOCOL, ParentStartup, process_alive, state_folder
from shared_garden_runtime import ROOT, read
from pc_server_installation import installed


def controller_for(isolated_family=None):
    if isolated_family is not None:
        import re
        if not re.fullmatch(r'[0-9a-f]{32}', isolated_family):
            raise OperationError('Invalid isolated family.')
    folder = ROOT/'LocalData/ParentServer' if isolated_family is None else ROOT/'LocalData/FamilyLAN'/isolated_family/'parent-startup'
    settings = read(folder/'settings.json')
    if not isinstance(settings, dict):
        raise OperationError('Parent server settings are not configured.')
    installation = installed(ROOT)
    build = installation['build'] if installation and installation['family']==settings.get('family') else settings.get('build')
    controller = ParentServer(settings.get('family'), build)
    if (isolated_family is None and controller.isolated) or (isolated_family is not None and
            (not controller.isolated or controller.family != isolated_family)):
        raise OperationError('Parent launcher scope does not match the selected family.')
    return controller


def ready_dashboard(folder, controller):
    record = read(folder/'dashboard.json')
    if not record:
        return None
    try:
        uri = urlsplit(record['url'])
        if (uri.scheme != 'http' or uri.hostname != '127.0.0.1' or uri.username or uri.password or
                uri.path != '/' or uri.query or len(uri.fragment) < 30 or not 1024 <= uri.port <= 65535):
            return None
        connection = http.client.HTTPConnection('127.0.0.1', uri.port, timeout=3)
        try:
            connection.request('GET', '/api/status', headers={'X-Little-Weeps': uri.fragment})
            response = connection.getresponse()
            # Never follow redirects with the local session capability.
            if response.status != 200:
                return None
            status = json.loads(response.read(65537))
        finally:
            connection.close()
        if (record.get('family') == controller.family == status.get('family') and
                record.get('build') == controller.build == status.get('selectedBuild') and
                record.get('helperProtocol') == HELPER_PROTOCOL == status.get('helperProtocol') and
                process_alive(record.get('pid'))):
            return record
    except (OSError, ValueError, TypeError, KeyError, http.client.HTTPException):
        return None
    return None


def ensure_dashboard(controller, at_signin=False):
    folder = state_folder(controller); folder.mkdir(parents=True, exist_ok=True)
    # Launchers use a short lock; the helper owns a separate lifetime lock.
    # Concurrent desktop/sign-in launches then reuse one verified helper.
    until = time.monotonic()+40
    while True:
        try:
            with operation_lock(folder, 'bootstrap.lock'):
                if at_signin:
                    startup = ParentStartup(controller)
                    if startup.registration() != 'configured' or (read(controller.root/'parent-operations.json') or {}).get('supervisionEnabled') is not True:
                        return None
                existing = ready_dashboard(folder, controller)
                if existing:
                    return existing
                previous = read(folder/'dashboard.json')
                if previous and process_alive(previous.get('pid')):
                    raise OperationError('An older or different parent helper is still running. It was left untouched; close that helper before switching versions.')
                uv = shutil.which('uv')
                if not uv:
                    raise OperationError('The prepared Python launcher is unavailable. No helper was started.')
                # A detached service must own its uv lifetime. Borrowing this
                # short-lived launcher's sys.executable leaves it pointing into
                # a removed temporary environment after the shortcut exits.
                args = [uv, 'run', '--offline', '--with', 'cryptography', 'python',
                        str(ROOT/'Tools/Parent-Server.py'), '--family', controller.family,
                        '--build', str(controller.build), '--no-browser']
                if controller.isolated:
                    args += ['--isolated']
                with (folder/'dashboard.log').open('ab') as output, (folder/'dashboard-error.log').open('ab') as error:
                    process = subprocess.Popen(args, cwd=ROOT, stdin=subprocess.DEVNULL, stdout=output, stderr=error,
                                               creationflags=subprocess.CREATE_NO_WINDOW)
                deadline = time.monotonic()+30
                while time.monotonic()<deadline:
                    ready = ready_dashboard(folder, controller)
                    if ready:
                        return ready
                    if process.poll() is not None:
                        break
                    time.sleep(.3)
                raise OperationError('The parent helper did not become ready. Inspect its local error log; no game process was stopped.')
        except OperationError as error:
            if str(error) != 'Another server operation is in progress.' or time.monotonic()>=until:
                raise
            time.sleep(.2)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--no-browser',action='store_true');parser.add_argument('--at-signin',action='store_true')
    parser.add_argument('--isolated-family')
    args=parser.parse_args()
    try:
        record=ensure_dashboard(controller_for(args.isolated_family),args.at_signin)
        if record:
            controller=controller_for(args.isolated_family)
            if ParentStartup(controller).registration()=='configured':
                from pc_server_watchdog import launch
                launch(controller)
        if record and not args.no_browser and not args.at_signin:
            webbrowser.open(record['url'])
        print('Parent helper ready.' if record else 'Sign-in launch skipped; the saved startup/recovery choice is off.')
    except (OperationError, ValueError, TypeError) as error:
        print(str(error),file=sys.stderr);return 1
    return 0


if __name__=='__main__':
    raise SystemExit(main())
