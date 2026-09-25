"""Watch one enrolled Windows server for process exit. Explicit parent Start enables recovery."""
import argparse
import time
from parent_server import ParentServer, OperationError, operation_lock
from server_supervisor import Supervisor


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--family', required=True)
    parser.add_argument('--build', required=True, type=int)
    parser.add_argument('--pause', action='store_true', help='Disable future automatic restarts without disconnecting current players.')
    args = parser.parse_args()
    try:
        controller = ParentServer(args.family, args.build)
        if args.pause:
            controller.pause_recovery(); print('Automatic recovery paused; existing game untouched.'); return
        supervisor = Supervisor(controller)
        with operation_lock(controller.root, 'supervisor.lock'):
            print('Supervision active for selected world; no game process is force-stopped.', flush=True)
            previous = None
            while True:
                state = supervisor.tick()
                if state['status'] != previous:
                    print(state['message'], flush=True); previous = state['status']
                time.sleep(2)
    except OperationError as error:
        parser.exit(1, str(error) + '\n')


if __name__ == '__main__': main()
