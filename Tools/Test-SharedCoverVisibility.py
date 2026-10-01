# /// script
# dependencies = ["cryptography"]
# ///
"""Verify mutual visibility from every client across different places in one cover."""
import argparse, importlib.util, time
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build);folder=run.path/'shared-cover';folder.mkdir();passed=False;checks=[]
    print('EVIDENCE '+str(folder),flush=True)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots];a,b,c,d=clients
        def game():return server.state()['view']['hideAndSeek']
        def hider(client):return next(h for h in game()['hiders'] if h['actor']==client.profile)
        for client,color in zip(clients,('blue-pup','orange-pup','bandit','chilli')):
            home.ready(client);require(home.command(client,1,value=color)['accepted'],'Avatar setup')
        for cover,places,positions in [('wardrobe',(3,4,13,3),(-3560,-3430,-3495,-3560)),('sofa',(1,2,12,1),(-4050,-3870,-3960,-4050))]:
            require(home.command(a,20,value='start')['accepted'],'Shared hiding start')
            for client,slot,x in zip(clients,places,positions):
                require(home.command(client,0,x=x,y=50)['accepted'],'Approach')
                require(home.command(client,20,target=str(slot),value='hide')['accepted'],'Select cover place')
            time.sleep(.5)
            for client in clients:
                state=client.input('inspect');visible={p['id'] for p in state['players'] if p['visible']}
                require({p.profile for p in clients}<=visible,'Co-hider missing from '+client.profile+' view')
                points=[p['position']['x'] for p in state['players'] if p['id'] in visible]
                require(len(set(round(x,2) for x in points))==4,'Co-hiders overlap')
            for client,size in [(a,(1280,591)),(b,(1024,768))]:
                client.input('resize',x=size[0],y=size[1]);home.ready(client);home.capture(client,folder,cover+'-'+client.profile)
            require(home.command(d,20,value='out')['accepted'],'Independent exit')
            require(all(hider(client)['mode']==2 for client in (a,b,c)),'Exit released sibling')
            wait(lambda:all(hider(client)['mode']==3 for client in (a,b,c)),'Physical shared reveal',35)
            require(hider(d)['mode']==0,'Exited player was found')
            wait(lambda:game()['phase']==0,'Finished parent reaction',10)
            checks.append(cover+': all four mutually visible in different/same places; distinct drawing positions; one shared reveal; independent exit')
            print('PASS '+checks[-1],flush=True)
        # An unrelated cover remains concealed to a hidden player.
        require(home.command(a,20,value='start')['accepted'],'Separate-cover start')
        for client,slot,x in [(a,3,-3560),(b,4,-3430),(c,5,-3150)]:
            require(home.command(client,0,x=x,y=50)['accepted'],'Approach')
            require(home.command(client,20,target=str(slot),value='hide')['accepted'],'Hide')
        time.sleep(.4)
        for client in (a,b):
            visible={p['id'] for p in client.input('inspect')['players'] if p['visible']}
            require(a.profile in visible and b.profile in visible and c.profile not in visible,'Cover visibility leaked or lost neighbour')
        checks.append('Players in other covers stay concealed');passed=True
    finally:write(folder/'result.json',dict(build=args.build,passed=passed,checks=checks));run.close()
    print('ALL PASS',flush=True)

if __name__=='__main__':main()
