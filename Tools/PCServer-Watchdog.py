"""Keep the opted-in helper available while the signed-in PC is awake."""
import argparse
from datetime import datetime,timezone
import os
from pathlib import Path
import time

from parent_bootstrap import controller_for,ensure_dashboard
from parent_server import operation_lock
from parent_startup import ParentStartup,state_folder
from shared_garden_runtime import read,write


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--family',required=True);parser.add_argument('--isolated',action='store_true')
    args=parser.parse_args();controller=controller_for(args.family if args.isolated else None)
    if controller.family!=args.family:raise RuntimeError('Selected family changed; watcher did not adopt it')
    folder=state_folder(controller);folder.mkdir(parents=True,exist_ok=True)
    with operation_lock(folder,'watchdog.lock'):
        write(folder/'watchdog.json',dict(pid=os.getpid(),family=controller.family))
        while True:
            try:
                # Re-read selection and intent; a compatible app release never
                # replaces them, and an intentional Stop remains stopped.
                controller=controller_for(args.family if args.isolated else None)
                if controller.family!=args.family:raise RuntimeError('Selected family changed')
                preferences=read(controller.root/'parent-operations.json') or {}
                intent=read(controller.root/'server-intent.json') or {}
                configured=ParentStartup(controller).registration()=='configured'
                if configured and preferences.get('supervisionEnabled') is True and intent.get('automaticRestart') is True:
                    ensure_dashboard(controller,at_signin=True)
                    status,message='watching','Parent helper available; native crash recovery retains the selected world.'
                else:status,message='paused','Saved Stop/Pause or startup preference is respected.'
            except Exception:
                status,message='needs-attention','Helper or selected installation could not be verified; no authority was killed or reset.'
            write(folder/'watchdog-state.json',dict(status=status,message=message,checkedAt=datetime.now(timezone.utc).isoformat()))
            time.sleep(10)


if __name__=='__main__':main()
