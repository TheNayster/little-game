"""Observe the oven countdown on four disposable clients, without extra commands."""
import argparse, copy, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, require, read, write, wait

spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)


def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);parser.add_argument('--legacy-client',type=int);args=parser.parse_args()
    run=Run(args.build);out=run.path/'shared-cooking-progress';out.mkdir();samples=[];passed=False
    print('EVIDENCE '+str(out),flush=True)
    try:
        server=run.start('server');clients=[]
        for i,s in enumerate(run.slots):
            client_run=run
            if i==3 and args.legacy_client:
                verified=Run(args.legacy_client)
                require(verified.content==run.content,'Mixed clients must retain the same content contract')
                client_run=copy.copy(run);client_run.build=verified.build;client_run.folder=verified.folder
            clients.append(client_run.start('client',s['profile']))
        def dish(instance):
            return next(t for t in instance.state()['view']['toys'] if t['id']=='cookware-0')['kitchen']['dish']
        def command(action,**kwargs):
            result=home.command(clients[0],action,**kwargs);require(result['accepted'],result['outcome'])
        for client in clients:home.ready(client)
        command(0,x=-2110,y=200)
        command(18,value='easy:start',target='PIZ-01')
        for ingredient in ['sauce','cheese']:
            command(18,value='easy:add',item='ingredient-'+ingredient,target='cookware-0')
        for step in ['shape','spread']:command(18,value='easy:'+step,item='cookware-0')
        clients[0].input('touchButton',text='Cook')
        command(18,value='easy:bake',item='cookware-0')
        start=time.monotonic()
        while time.monotonic()-start<9:
            ui=clients[0].input('inspect')
            samples.append(dict(seconds=round(time.monotonic()-start,2),server=dish(server)['heat'],
                                clients=[dish(c)['heat'] for c in clients],
                                countdown=[t for t in ui.get('visibleText',[]) if t.startswith('Baking')]))
            time.sleep(.3)
        moving=[s for s in samples if 1<s['server']<7]
        require(len(moving)>3,'Missing heating observation window')
        for i in range(4):
            require(len({int(s['clients'][i]) for s in moving})>=4,'Client '+str(i+1)+' countdown froze while the server cooked')
            require(max(abs(s['server']-s['clients'][i]) for s in moving)<1,'Client cooking progress lag exceeded one second')
        require(all(dish(c)['heated'] for c in clients),'Ready state missing')
        if args.build>=172:require(len({t for s in samples for t in s['countdown']})>=5,'Visible countdown did not update')
        passed=True
    finally:
        run.close();write(out/'result.json',dict(build=args.build,legacyClient=args.legacy_client,passed=passed,samples=samples,liveFamilyTouched=False))
        print('RESULT '+str(out/'result.json'),flush=True)


if __name__=='__main__':main()
