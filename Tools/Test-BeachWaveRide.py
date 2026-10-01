"""One isolated four-client check of real wave-ride menu/input and departures."""
import argparse,importlib.util,shutil,time
from pathlib import Path
from shared_garden_runtime import Run,wait,require,write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'wave-ride';out.mkdir();passed=False;checks=[]
    print('EVIDENCE '+str(out),flush=True)
    def check(what):checks.append(what);print('PASS '+what,flush=True)
    def capture(client,name):
        file=client.out/'garden.png';old=file.stat().st_mtime_ns if file.exists() else 0
        state=client.input('capture');wait(lambda:file.exists() and file.stat().st_mtime_ns!=old,'new screenshot')
        shutil.copyfile(file,out/(name+'.png'));write(out/(name+'.json'),state);return state
    try:
        server=run.start('server');clients=[run.start('client',f'player-{i}') for i in range(1,5)]
        for i,client in enumerate(clients):
            require(home.command(client,7,value='beach')['accepted'],'beach travel')
            home.ready(client);client.input('resize',x=1280 if i%2==0 else 1024,y=591 if i%2==0 else 768)
            client.input('touchButton',text='Tap to walk');require(client.input('inspect')['joystickVisible'],'walking mode established')
            require(home.command(client,0,x=1400+i*70,y=235)['accepted'],'boarding point')
        def snapshot():return wait(lambda:(s if (s:=server.state()) and s.get('view') else None),'read authority view')
        def ride():return snapshot()['view']['shore']['ride']
        a,b,c,d=clients
        for i,client in enumerate(clients):
            client.input('touchButton',text='Games');client.input('touchButton',text='Ride the waves')
            client.input('touchButton',text=('Ride whale','Ride dolphin','Ride mermaid','Ride whale')[i])
            wait(lambda:len(ride()['riders'])==i+1,'shared ready '+str(i+1))
            if i==3:capture(client,'tablet-four-ready')
            client.input('touchButton',text='Back to beach')
        g=ride();require(g['phase']==1 and len({r['seat'] for r in g['riders']})==4 and len({r['visitor'] for r in g['riders']})==3,'four ready in one lobby')
        check('Beach Games picture menu: four ready seats and all three visitor choices')
        a.input('touchButton',text='Games');a.input('touchButton',text='Ride the waves');a.input('touchButton',text='Start wave ride')
        wait(lambda:ride()['phase']==2,'one shared countdown')
        states=[client.input('inspect') for client in clients]
        require(all(s['shore']['ride']['round']==g['round'] and len(s['shore']['ride']['riders'])==4 for s in states),'common round broadcast')
        check('One Start broadcasts the same countdown/round to four clients')
        wait(lambda:ride()['phase']==3,'riding')
        wait(lambda:2.1<=ride()['age']-3<=3.5,'first splash window',10)
        for client in clients:client.input('touchButton',text='Splash together')
        wait(lambda:ride()['splashes']==4,'four cooperative splashes',5)
        states=[capture(a,'phone-four-riding'),capture(b,'tablet-four-riding')]
        require(all(s['visibleWaveRiders']==4 and s['visiblePlayers']==4 and not s['joystickVisible'] and s['sceneryReady'] for s in states),'convoy presentation')
        require(all(all(p['position']['y']>760 for p in s['players']) for s in states),'a render refresh detached seated players from visitors')
        check('Four visitors carry four visible players; phone/tablet captures and shared splash reward')
        before=ride();a.input('touchButton',text='All done riding')
        wait(lambda:len(ride()['riders'])==3,'own exit')
        require(home.command(b,7,value='creek')['accepted'],'sibling travels independently')
        c.close();wait(lambda:len(ride()['riders'])==1,'disconnect removes only one rider')
        after=ride();require(after['round']==before['round'] and after['age']>=before['age'] and after['riders'][0]['actor']=='player-4','remaining rider retains route')
        check('All done, travel and disconnect leave the remaining rider on the common route')
        capture(d,'tablet-last-rider')
        wait(lambda:ride()['phase']==4 and len(ride()['riders'])==0,'safe automatic shore return',30)
        wait(lambda:(s:=d.input('inspect'))['visibleWaveRiders']==0 and s['joystickVisible'],'normal walking resumes')
        require(next(p for p in snapshot()['view']['players'] if p['id']=='player-4')['y']==235,'original beach point restored')
        d.input('touchButton',text='Games');d.input('touchButton',text='Ride the waves');d.input('touchButton',text='Ride mermaid')
        wait(lambda:ride()['round']==g['round']+1 and len(ride()['riders'])==1,'deliberate new lobby')
        d.input('touchButton',text='Back to beach');d.input('touchButton',text='All done riding')
        wait(lambda:len(ride()['riders'])==0,'last lobby rider leaves')
        check('Automatic shore return restores walking; deliberate replay and empty lobby exit')
        passed=True
    finally:
        run.close();write(out/'result.json',dict(passed=passed,build=args.build,checks=checks,layouts=['phone 1280x591','tablet 1024x768'],liveFamilyTouched=False,physicalDevicesTested=False))
        print('RESULT '+str(out/'result.json'),flush=True)
if __name__=='__main__':main()
