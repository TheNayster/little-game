"""Resume the parent helper after exit; preserve Stop/Pause and one native authority."""
import json
from pathlib import Path
import shutil
import subprocess

from parent_server import OperationError, command_args, operation_lock
from parent_startup import process_alive, state_folder


def watcher_process(pid, root, family):
    if not process_alive(pid):return False
    command=f'Get-CimInstance Win32_Process -Filter "ProcessId = {int(pid)}" | Select-Object CommandLine | ConvertTo-Json -Compress'
    result=subprocess.run(['powershell.exe','-NoProfile','-Command',command],capture_output=True,text=True,
                          check=True,timeout=12,creationflags=subprocess.CREATE_NO_WINDOW)
    native=json.loads(result.stdout) if result.stdout.strip() else None
    if not native:return False
    if not native.get('CommandLine'):raise OperationError('Watchdog process identity is unavailable.')
    args=command_args(native['CommandLine'])
    return (str(Path(root)/'Tools/PCServer-Watchdog.py') in args
            and '--family' in args and args[args.index('--family')+1]==family)


def launch(controller):
    root=Path(__file__).resolve().parent.parent
    folder=state_folder(controller);folder.mkdir(parents=True,exist_ok=True)
    with operation_lock(folder,'watchdog-launch.lock'):
        from shared_garden_runtime import read
        record=read(folder/'watchdog.json')
        if record and watcher_process(record.get('pid'),root,controller.family):
            return 'already-running'
        uv=shutil.which('uv')
        if not uv:raise OperationError('The prepared watchdog runtime is unavailable.')
        args=[uv,'run','--offline','--with','cryptography','python',str(root/'Tools/PCServer-Watchdog.py'),'--family',controller.family]
        if controller.isolated:args+=['--isolated']
        with (folder/'watchdog.log').open('ab') as out:
            subprocess.Popen(args,cwd=root,stdin=subprocess.DEVNULL,stdout=out,stderr=out,creationflags=subprocess.CREATE_NO_WINDOW)
        # The worker writes its real Python PID after acquiring its lifetime
        # lock; a uv PID is not a validated identity for a later launch.
        return 'starting'
