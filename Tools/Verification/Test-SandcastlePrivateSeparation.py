"""One current paired native client: private Sandcastle never merges into authority.
Uses generated test enrollment and GUID save branches, never installed enrollment.
"""
import argparse,copy,hashlib,importlib.util,json,socket,subprocess,sys,time,uuid
from pathlib import Path
sys.path.insert(0,str(next(p for p in Path(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT
from family_pairing import create_family,write_record
from shared_garden_runtime import Instance,read,write,wait,require
s=importlib.util.spec_from_file_location('stage6',Path(__file__).with_name('Test-SandcastleStageSix.py'));ui=importlib.util.module_from_spec(s);s.loader.exec_module(ui)

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--build',type=int,required=True);args=ap.parse_args()
    folder=ROOT/f'Builds/NetworkProbe/G3-0.0.{args.build}'
    ui.verify_inputs(folder)
    authority,players,_=create_family()
    class Lab:pass
    run=Lab();run.run_id=authority['worldId'];run.build=args.build;run.path=ROOT/'LocalData/FamilyLAN'/run.run_id;active=[];passed=False;checks=[]
    out=run.path/'sandcastle-private';out.mkdir(parents=True);print('EVIDENCE '+str(out),flush=True)
    def start(record):
        v=Instance.__new__(Instance);v.run=run;v.role=record['role'];v.profile=record.get('profile','');v.serial=v.garden_serial=0;v.identity=uuid.uuid4().hex;v.out=run.path/v.identity;v.out.mkdir()
        pair=run.path/(v.identity+'.pairing');write_record(pair,record)
        with socket.socket(socket.AF_INET,socket.SOCK_DGRAM) as sock:sock.bind(('0.0.0.0',0));port=sock.getsockname()[1]
        cfg=dict(runId=run.run_id,instanceId=v.identity,role=v.role,port=port if v.role=='server' else 1025,protocol=3,content=70,pairingPath=str(pair),presentation=True,verifyGarden=v.role=='client',interactive=True)
        config=run.path/(v.identity+'.config.json');write(config,cfg)
        exe=folder/('Server' if v.role=='server' else 'Client')/'LittleWeepsNetwork.exe'
        argv=[str(exe),'-familyNetworkConfig',str(config),'-logFile',str(v.out/'player.log'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','591']
        if v.role=='server':argv+=['-batchmode','-nographics']
        si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
        v.process=subprocess.Popen(argv,stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,startupinfo=si,creationflags=subprocess.CREATE_NO_WINDOW);active.append(v)
        wait(lambda:v.status(),'native status');wait(lambda:v.status()['status']=='listening','listening') if v.role=='server' else wait(lambda:v.input('inspect')['ready'],'private presentation')
        return v
    try:
        a=start(players[0]);require(not a.input('inspect')['shared'],'private without authority')
        ui.home.travel(a,'daycare');ui.button(a,'Games');ui.button(a,'Sandcastle club')
        id=ui.place(a,'round',4150,130);ui.button(a,'Water')
        for _ in range(3):ui.button(a,'Next sand tool')
        wait(lambda:a.input('inspect')['sandpit']['moulds'][0]['scoops']==3,'private scoops');ui.button(a,'Tip');wait(lambda:a.input('inspect')['sandpit']['moulds'][0]['built'],'private tip')
        ui.button(a,'Decorate');ui.button(a,'Flag');ui.button(a,'Confirm sand decoration');wait(lambda:a.input('inspect')['sandpit']['moulds'][0]['attachments'],'private flag')
        private=copy.deepcopy(a.input('inspect')['sandpit']['moulds']);ui.home.capture(a,out,'phone-private-built-flag')
        ui.button(a,'Leave sandpit');time.sleep(1);path=Path(a.input('inspect')['savePath']);require(run.run_id in str(path),'owned private save');server=start(authority)
        wait(lambda:a.input('inspect')['shared'],'automatic family handoff',35)
        # Existing housekeeping clocks advance until the durable handoff. The
        # retained creation, rather than whole-envelope byte identity, is the criterion.
        saved=json.loads(path.read_text(encoding='utf-8').split('\n',2)[2]);require(saved['sandpit']['moulds']==private,'private checkpoint retains creation')
        require(not server.state()['view']['sandpit']['moulds'],'offline edits never entered server')
        require(ui.home.command(a,7,value='daycare')['accepted'],'family daycare');ui.button(a,'Games');ui.button(a,'Sandcastle club');require(not a.input('inspect')['sandpit']['moulds'],'shared empty');ui.home.capture(a,out,'phone-shared-empty')
        ui.button(a,'Leave sandpit');ui.button(a,'Menu');ui.button(a,'Play by myself');wait(lambda:not a.input('inspect')['shared'],'private return')
        ui.button(a,'Games');ui.button(a,'Sandcastle club');require(a.input('inspect')['sandpit']['moulds']==private,'private creation returns intact');ui.home.capture(a,out,'phone-private-return')
        require(not server.state()['view']['sandpit']['moulds'],'server unchanged after return')
        checks=['450 one actual paired native client builds privately through normal Daycare/UGUI input','Same process discovers isolated generated authority; private checkpoint retained, no private pieces imported','Explicit solo return retrieves private built piece/flag while shared server creation stays empty']
        passed=True
    finally:
        for v in reversed(active):
            if v.process.poll() is None:v.close()
        errors=[line for v in active for line in (v.out/'player.log').read_text(encoding='utf8',errors='replace').splitlines() if 'Exception:' in line or 'NullReference' in line]
        write(out/'result.json',dict(passed=passed and not errors,build=args.build,checks=checks,peakActualNativeClients=1,runtimeErrors=errors,syntheticEnrollment=True));require(not errors,'native errors')
    print('PASS '+str(checks),flush=True)
if __name__=='__main__':main()
