"""Open the built Zoo for owner play, with normal input and no scripted actions."""
import argparse
import sys
import time
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from shared_garden_runtime import Run


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build', type=int)
    args = parser.parse_args()
    # Reuse the established hash-checked launcher and loopback-only sandbox.
    # No real enrollment, installed authority or personal saves are selected.
    run = Run(args.build, interactive=True, review_controls=False)
    try:
        run.start('server')
        player = run.start('client', run.slots[0]['profile'])
        print('Choose Zoo in the places menu. Manual review session: ' + str(run.path), flush=True)
        while player.process.poll() is None:
            time.sleep(.5)
    finally:
        run.close()


if __name__ == '__main__':
    main()
