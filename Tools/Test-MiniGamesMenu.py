# /// script
# dependencies = ["cryptography"]
# ///
"""Focused native Games-menu check with four disposable family clients."""
import argparse
import importlib.util
import time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec = importlib.util.spec_from_file_location('home', Path(__file__).with_name('Test-HomeWorld.py'))
home = importlib.util.module_from_spec(spec)
spec.loader.exec_module(home)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('build', type=int)
    args = parser.parse_args()
    run = Run(args.build)
    folder = run.path / 'mini-games-menu'
    folder.mkdir()
    checks = []
    passed = False
    print('EVIDENCE ' + str(folder), flush=True)

    def tap(client, name):
        result = client.input('touchButton', text=name)
        time.sleep(.15)
        home.ready(client)
        return result

    def record(name):
        checks.append(name)
        print('PASS ' + name, flush=True)

    try:
        server = run.start('server')
        clients = [run.start('client', slot['profile']) for slot in run.slots]
        a, b, c, d = clients
        for index,client in enumerate(clients):
            home.ready(client)
            require(home.command(client, 0, x=4310 if index==2 else -3800, y=50)['accepted'], 'Fixture placement failed')
        for client in clients:
            wait(lambda: not client.input('inspect')['menuOpen'], 'fixture scenery settled')
        before = server.state()['view']['hideAndSeek']['round']
        for width, height, name in [(1280, 591, 'phone-menu'), (1024, 768, 'ipad-menu')]:
            a.input('resize', x=width, y=height)
            tap(a, 'Games')
            state = home.capture(a, folder, name)
            names = [v['name'] for v in state['controls']]
            require(state['menuOpen'] and 'Hide & seek' in names and 'Back to play' in names, 'Menu controls missing')
            require(not any(v in names for v in ['Play Keepy Uppy', 'Grow a flower', 'Splash cleanup']), 'Other activities entered the menu')
            tap(a, 'Back to play')
            require(not a.input('inspect')['menuOpen'], 'Back failed to close menu')
        require(server.state()['view']['hideAndSeek']['round'] == before, 'Opening menu started a round')
        record('phone and iPad picture menu contains only hide-and-seek; Back starts nothing')

        tap(a, 'Games')
        tap(a, 'Hide & seek')
        wait(lambda: server.state()['view']['hideAndSeek']['phase'] == 1, 'shared countdown')
        require(not a.input('inspect')['menuOpen'], 'Selection did not dismiss menu')
        round_id = server.state()['view']['hideAndSeek']['round']
        for client in clients:
            state = client.input('inspect')
            require(state['hideAndSeek']['round'] == round_id and state['hideAndSeek']['phase'] == 1, 'Not one broadcast round')
        record('one card tap closes menu and broadcasts one shared countdown to four clients')

        tap(c, 'Hide Garden bush')
        wait(lambda: next(h for h in server.state()['view']['hideAndSeek']['hiders'] if h['actor'] == c.profile)['mode'] == 2, 'hider entered cover')
        tap(b, 'Games')
        b.close()
        require(home.command(d, 0, x=-3600, y=50)['accepted'], 'Sibling controls stopped')
        shared = server.state()['view']['hideAndSeek']
        require(shared['round'] == round_id and next(h for h in shared['hiders'] if h['actor'] == c.profile)['mode'] == 2, 'Menu-owner exit disturbed hider')
        record('leaving with menu open keeps sibling controls and hidden participant intact')
        passed = True
    finally:
        write(folder / 'result.json', dict(build=args.build, passed=passed, checks=checks))
        run.close()
    print('ALL PASS', flush=True)


if __name__ == '__main__':
    main()
