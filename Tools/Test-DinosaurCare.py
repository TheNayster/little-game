# /// script
# dependencies = ["cryptography"]
# ///
"""Real feeding/petting controls and shared turns on four disposable native clients."""
import argparse, importlib.util, time, uuid
from pathlib import Path
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location('home',Path(__file__).with_name('Test-HomeWorld.py'))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    parser=argparse.ArgumentParser();parser.add_argument('build',type=int);args=parser.parse_args()
    run=Run(args.build,extended_test_lifetime=True);out=run.path/'dinosaur-care';out.mkdir();checks=[];passed=False
    for slot in run.slots:slot['profile']=uuid.uuid4().hex
    names=['tyrannosaurus','triceratops','brachiosaurus','parasaurolophus']
    def state():return server.state()['view']['dinosaurWorld']
    def animal(name):return next(a for a in state()['animals'] if a['species']==name)
    def care(v):return next(f for f in state()['care'] if f['actor']==v.profile)
    def player(v):return next(p for p in server.state()['view']['players'] if p['id']==v.profile)
    def cmd(v,action,**kw):
        r=home.command(v,action,**kw);require(r['accepted'],str(r));home.ready(v)
    def record(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        server=run.start('server');clients=[run.start('client',s['profile']) for s in run.slots]
        for i,v in enumerate(clients):
            home.ready(v);v.input('resize',x=1280 if i%2==0 else 1024,y=591 if i%2==0 else 768);cmd(v,7,value='dinosaur-world');cmd(v,1,value='orange-pup' if i%2 else 'blue-pup')
        for i,v in enumerate(clients):
            name=names[i];bucket=min(4400,900+i*1200);cmd(v,0,x=bucket-100,y=100);time.sleep(.65)
            before=animal(name)['fed'];v.input('touchButton',text='Feed '+name);v.input('touchButton',text='Feed '+name)
            deadline=time.monotonic()+25;ticket=None
            while care(v)['phase']<3 and time.monotonic()<deadline:
                f=care(v)
                if f['ticket']:
                    if ticket is None:ticket=f['ticket']
                    require(ticket==f['ticket'],'Repeated food tap replaced portion')
                v.input('touchButton',text='Feed '+name);time.sleep(.12)
            require(care(v)['phase']>=3,'Repeated taps interrupted automatic food flow')
            wait(lambda:care(v)['phase']==4,'eating '+name,20);v.input('touchButton',text='Feed '+name)
            home.capture(v,out,name+'-feeding');wait(lambda:animal(name)['fed']==before+1 and care(v)['phase']==0,'one completed portion',12)
        record('all four species use real bucket controls; repeated taps through both walks/approach/eating retain one portion and one meal')
        for i,v in enumerate(clients):
            name=names[i];a=animal(name);cmd(v,0,x=max(510,min(4400,a['x']+(110 if i==2 else 65))),y=100);time.sleep(.65)
            before=a['petted'];v.input('touchButton',text='Pet '+name);v.input('touchButton',text='Pet '+name)
            wait(lambda:care(v)['phase']==5,'petting '+name,15);home.capture(v,out,name+'-petting')
            wait(lambda:animal(name)['petted']==before+1 and care(v)['phase']==0,'one finished pet turn',12)
        record('four dinosaur hand pictures walk into contact, animate the child/animal and display affection; repeat taps retain one pet turn')
        # One common dinosaur and four real reserved turns, without private copies.
        name='triceratops';before=animal(name)['fed']
        for v in clients:cmd(v,0,x=2100,y=100)
        time.sleep(.65)
        for v in clients:v.input('touchButton',text='Feed '+name)
        wait(lambda:all(care(v)['phase']>=2 for v in clients),'four shared care turns',22)
        require(len({care(v)['slot'] for v in clients})==4 and len({care(v)['ticket'] for v in clients})==4,'Shared care spots overlap')
        tickets={v.profile:care(v)['ticket'] for v in clients}
        home.capture(clients[3],out,'four-player-feeding-turns')
        cmd(clients[0],7,value='creek');clients[1].close();wait(lambda:care(clients[1])['phase']==0,'disconnect clears own care')
        for v in clients[2:]:require(care(v)['ticket']==tickets[v.profile],'Departure reset sibling turn')
        expected=animal(name)['fed']+sum(not care(v)['consumed'] for v in clients[2:])
        wait(lambda:animal(name)['fed']==expected and all(care(v)['phase']==0 for v in clients[2:]),'remaining sibling meals',40)
        record('four shared feeding slots/tickets; travel and disconnect release only their own turns while two siblings finish')
        v=clients[2];a=animal(name);cmd(v,0,x=max(510,min(4400,a['x']+65)),y=100);time.sleep(.65);v.input('touchButton',text='Pet '+name)
        wait(lambda:care(v)['phase']==5,'pet before pause',15);before=animal(name)['petted'];v.input('application-pause');wait(lambda:care(v)['phase']==0,'pause releases own pet turn');before=animal(name)['petted'];v.input('application-resume');home.ready(v);time.sleep(1.5)
        require(animal(name)['petted']==before,'Paused pet continued consuming')
        cmd(v,23,value='mount',target=name);v.input('touchButton',text='Feed '+name)
        wait(lambda:player(v)['fixture']=='','feed gets off first',10);wait(lambda:care(v)['phase']>=2,'ride to feed flow',25)
        wait(lambda:care(v)['phase']==0,'ride to feed completed',18)
        cmd(v,23,value='mount',target=name);require(player(v)['fixture']=='dinosaur-'+name,'care left dinosaur permanently busy')
        record('application pause releases petting; a food tap gets off an owned ride before feeding; completed care permits riding again')
        write(out/'progress.json',dict(animals=state()['animals'],profileIdLength=32));passed=True
    finally:
        if not passed and "server" in locals():write(out/"failure-state.json",server.state())
        run.close();write(out/'results.json',dict(passed=passed,build=args.build,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False));print('RESULT '+str(out/'results.json'),flush=True)

if __name__=='__main__':main()
