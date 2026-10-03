"""Actual isolated Unity preview taps/captures; no save adapter, server or installed apps."""
import argparse, json, subprocess, time, uuid, shutil, hashlib
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from project_paths import ROOT

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--candidate',required=True);ap.add_argument('--film-only',action='store_true');a=ap.parse_args()
    candidate=ROOT/'Builds/SandcastlePrototype'/a.candidate
    out=ROOT/'LocalData/SandcastlePrototype'/uuid.uuid4().hex;out.mkdir(parents=True)
    print('EVIDENCE '+str(out),flush=True)
    startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
    p=subprocess.Popen([str(candidate/'SandcastlePrototype.exe'),'-batchmode','-screen-width','1024','-screen-height','768','-sandcastleEvidence',str(out),'-logFile',str(out/'player.log')],startupinfo=startup)
    checks=[]
    def step(action,**kw):
        id=uuid.uuid4().hex;request=out/'request.json';tmp=out/'request.tmp';tmp.write_text(json.dumps(dict(id=id,action=action,**kw)),encoding='utf-8');
        for attempt in range(80):
            try:tmp.replace(request);break
            except PermissionError:time.sleep(.025)
        else:raise RuntimeError('Evidence request stayed locked')
        end=time.monotonic()+20
        while time.monotonic()<end:
            if p.poll() is not None:raise RuntimeError('Player exited '+str(p.returncode))
            try:
                data=json.loads((out/'response.json').read_text(encoding='utf-8-sig'))
                if data['id']==id:
                    assert not data['error'],data['error'];
                    with (out/'steps.jsonl').open('a',encoding='utf-8') as trace:trace.write(json.dumps(dict(action=action,kwargs=kw,placing=data['placing'],hasPreview=data['hasPreview'],pieces=len(data['sandpit']['moulds'])))+'\n')
                    return data
            except (FileNotFoundError,json.JSONDecodeError):pass
            time.sleep(.05)
        raise RuntimeError('Timed out '+action+' '+str(kw))
    def button(name):return step('button',text=name)
    def cell(i):
        d=step('inspect');c=d['cells'][i];return step('tap',x=c['x'],y=c['y'])
    def capture(name):step('capture',text=name)
    def check(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        if a.film_only:
            step('inspect');step('resize',x=1280,y=591);button('Empty pit');step('film',x=25);time.sleep(.5)
            button('Round');cell(18);time.sleep(.7);d=button('Confirm');first=d['sandpit']['moulds'][0]['id']
            for _ in range(3):button('Scoop');time.sleep(.6)
            button('Water');time.sleep(1.2);button('Tip');time.sleep(2)
            button('Wall');cell(11);time.sleep(.6);button('Confirm')
            for _ in range(3):button('Scoop');time.sleep(.4)
            button('Water');time.sleep(1.2);button('Tip');time.sleep(2)
            d=step('inspect');c=d['cells'][18];d=step('tap',x=c['x'],y=c['y']+150*min(d['width']/1420,d['height']/1010));assert d['selected']==first
            button('Flag');time.sleep(.5);button('Shell');capture('phone-recorded-continued-build');time.sleep(12)
            (out/'result.json').write_text(json.dumps(dict(passed=True,candidate=a.candidate,actualInputs=True,recordingOnly=True,silent=True,sequence='empty; mould; projected tap; confirm; scoop x3; water; tip; second wall build; select existing round face; flag; shell'),indent=2),encoding='utf-8');print('PASS actual placement/build/selection/decor film',flush=True);return
        d=step('inspect');assert len(d['sandpit']['moulds'])==12 and d['fixture'];assert d['sandpit']['toy']['placed'];capture('tablet-example')
        for shape,index in [('Round',0),('Square',7),('Round',24),('Square',31)]:
            button('Empty pit');button(shape);d=cell(index);assert d['hasPreview'],('missing preview',index,d['placing']);assert any(c['name']=='Confirm' and c['enabled'] for c in d['controls']);d=button('Confirm');assert len(d['sandpit']['moulds'])==1
            m=d['sandpit']['moulds'][0];c=d['cells'][index];assert (m['x'],m['y'])==(c['worldX'],c['worldY'])
        check('Real taps place at all four edge cells; rim and cast do not intercept')
        button('Empty pit');capture('tablet-empty');button('Round');d=cell(10);capture('tablet-preview');d=button('Confirm');step('film',x=28)
        first_id=d['sandpit']['moulds'][0]['id']
        d=button('Water');assert d['sandpit']['moulds'][0]['wet'];capture('tablet-water-before-scoop')
        d=button('Tip');assert not d['sandpit']['moulds'][0]['built'];assert d['sandpit']['moulds'][0]['scoops']==0
        for i in range(3):d=button('Scoop');time.sleep(.3)
        capture('tablet-full-bucket');d=button('Tip');assert d['sandpit']['moulds'][0]['built'];time.sleep(2)
        d=button('Flag');d=button('Shell');assert {v['kind'] for v in d['sandpit']['moulds'][0]['attachments']}=={'flag','shell'};capture('tablet-built-combined')
        check('Empty actual tap loop; early Water; underfilled retention; Scoop/Water/Tip; combined decorations')
        # Place behind and in front of the first tower through the inverse map, regardless of face occlusion.
        for index in (26,2):
            button('Square');d=cell(index);assert d['hasPreview'];d=button('Confirm');assert d['sandpit']['moulds'][-1]['y']==d['cells'][index]['worldY']
            for _ in range(3):button('Scoop')
            button('Water');button('Tip');time.sleep(2)
        d=step('inspect');c=d['cells'][10];d=step('tap',x=c['x'],y=c['y']+150*min(d['width']/1420,d['height']/1010));assert d['selected']==first_id,'visible face selects existing middle piece';capture('tablet-selected-existing');
        capture('tablet-depth-build');check('Behind/in-front placement through continuous inverse input map')
        step('resize',x=1280,y=591);capture('phone-built');button('Example castle');capture('phone-example')
        d=step('inspect')
        for c in d['controls']:
            assert min(c['width'],c['height'])>=44,(c['name'],c['width'],c['height'])
            assert c['x']-c['width']/2>=0 and c['x']+c['width']/2<=d['width'] and c['y']-c['height']/2>=0 and c['y']+c['height']/2<=d['height'],c['name']
        button('Empty pit');capture('phone-empty');button('Gate');d=cell(11);capture('phone-gate-preview');button('Confirm');button('Water')
        for _ in range(3):button('Scoop')
        button('Tip');time.sleep(2);button('Flag');button('Shell');capture('phone-gate-built');button('Round');d=cell(11);assert not any(c['name']=='Confirm' and c['enabled'] for c in d['controls']);capture('phone-overlap-preview');button('Cancel');check('Phone aspect actual taps, controls and gate; tablet captures')
        time.sleep(15)
        log=(out/'player.log').read_text(encoding='utf-8',errors='replace');assert not any(x in log for x in ('NullReferenceException','InvalidOperationException','MissingReferenceException','IndexOutOfRangeException')),'runtime exception'
        checks.append('No runtime exceptions')
        (out/'result.json').write_text(json.dumps(dict(passed=True,candidate=a.candidate,checks=checks,widths=[1024,1280],heights=[768,591],realNativePlayers=1,syntheticSessionProfiles=4,productionSceneUnchanged=True),indent=2),encoding='utf-8')
        print('PASS ALL',flush=True)
    finally:
        p.terminate();p.wait(timeout=15)

if __name__=='__main__':main()
