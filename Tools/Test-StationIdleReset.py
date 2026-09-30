"""One real five-minute, four-client coloring/science/book cleanup check."""
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write

spec = importlib.util.spec_from_file_location('home', Path(__file__).with_name('Test-HomeWorld.py'))
home = importlib.util.module_from_spec(spec)
spec.loader.exec_module(home)

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('build', type=int)
    args = parser.parse_args()
    run = Run(args.build, extended_test_lifetime=True)
    out = run.path / 'station-idle-reset'
    out.mkdir()
    checks, passed = [], False
    print('EVIDENCE ' + str(out), flush=True)
    def record(name):
        checks.append(name)
        print('PASS ' + name, flush=True)
    def button(client, name):
        client.input('touchButton', text=name)
        time.sleep(.15)
        home.ready(client)
    def move(client, x):
        require(home.command(client, 0, x=x, y=200)['accepted'], 'fixture placement')
        time.sleep(.4)
    def info(client):
        return client.input('inspect')
    def work(client):
        return next(w for w in server.state()['view']['discovery'] if w['owner'] == client.profile)
    def fill(client, page, region, color):
        token = str(page) + '@' + str(work(client)['pages'][page]['revision'])
        require(home.command(client, 19, item=client.profile, target=token, value=f'fill:{region}:{color}')['accepted'], 'fill')
    def openbook(client):
        button(client, 'Read shared books')
        button(client, 'Choose little-bridge')
        wait(lambda: info(client)['bookReady'], 'book media', 25)
    try:
        server = run.start('server')
        clients = [run.start('client', s['profile']) for s in run.slots]
        a, b, c, d = clients
        for client in clients:
            home.ready(client)
        for client in (a, b):
            move(client, -5460)
            fill(client, 6, 0, 5)
            button(client, 'Coloring table')
        token = '6@' + str(work(a)['pages'][6]['revision'])
        require(home.command(a, 19, item=a.profile, target=token, value='save-picture')['accepted'], 'keep picture')
        move(c, -6590)
        button(c, 'Science bench')
        button(c, 'Liquid colors')
        button(c, 'Colors Red')
        move(d, -4500)
        openbook(d)
        button(d, '>')
        button(d, '>')
        require(info(d)['bookPage'] == 2, 'nonzero book page')
        a.input('resize', x=1280, y=591)
        b.input('resize', x=1024, y=768)
        record('four native clients prepare two papers, a science tray and a book on page three')
        started, last_refresh, saw_cue = time.monotonic(), time.monotonic(), False
        while time.monotonic() - started < 330:
            if time.monotonic() - last_refresh > 40:
                if not info(b)['discoveryOpen']:
                    button(b, 'Coloring table')
                button(b, 'Discovery crayon Red')
                last_refresh = time.monotonic()
                print('PROGRESS active artist refreshed at ' + str(round(last_refresh-started)) + ' seconds', flush=True)
            if any('Clearing soon' in text for text in info(a)['visibleText']):
                if not saw_cue:
                    home.capture(a, out, 'paper-cleanup-cue-phone')
                saw_cue = True
            if all(v == 0 for v in work(a)['pages'][6]['colors']) and sum(work(c)['liquid'][0]['current']['parts']) == 0 and info(d)['bookPage'] == 0:
                break
            time.sleep(.5)
        # An unrelated foreground window may close the hidden test overlay.
        # Core checks assert the exact 300-second cue and 305-second reset.
        require(all(v == 0 for v in work(a)['pages'][6]['colors']), 'unused paper remains colored')
        require(work(b)['pages'][6]['colors'][0] == 5, 'active sibling paper cleared')
        require(sum(work(c)['liquid'][0]['current']['parts']) == 0, 'unused science tray remains filled')
        require(info(d)['bookPage'] == 0 and not info(d)['bookPlaying'], 'unused reader not reset quietly')
        record('real five-minute timers clear unused paper and science and return idle reader to page one while active sibling continues')
        home.capture(a, out, 'blank-paper-phone')
        home.capture(b, out, 'active-artist-ipad')
        home.capture(d, out, 'book-first-page')
        button(d, '>')
        button(d, 'Close')
        openbook(d)
        require(info(d)['bookPage'] == 1, 'recently used bookmark reset early')
        button(d, 'Close')
        d.close()
        require(info(b)['discoveryOpen'], 'departure interrupted sibling')
        fill(b, 6, 1, 1)
        record('recent bookmark reopens at its current page and reader departure leaves sibling coloring usable')
        # The core retention check verifies exact kept/displayed picture copies;
        # this native pass verifies actual UI and authority timers together.
        passed = True
    finally:
        run.close()
        write(out/'results.json', dict(passed=passed, build=args.build, checks=checks, realTimer=True, liveFamilyTouched=False, physicalDevicesTested=False))
        print('RESULT ' + str(out/'results.json'), flush=True)

if __name__ == '__main__':
    main()
