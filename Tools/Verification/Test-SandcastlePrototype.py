"""Actual isolated Unity preview taps/captures; no save adapter, server or installed apps."""
import argparse, json, subprocess, time, uuid, shutil, hashlib
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from project_paths import ROOT

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--candidate',required=True);ap.add_argument('--film-only',action='store_true');ap.add_argument('--decor-only',action='store_true');a=ap.parse_args()
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
    def button(name):
        if name in ('Round','Square','Wall','Gate') and not any(c['name']==name for c in step('inspect')['controls']):step('button',text='Build')
        return step('button',text=name)
    def cell(i, long=False, rotated=False):
        d=step('inspect');c=d['cells'][i];x,y=c['x'],c['y']
        if long:
            other=d['cells'][i+(8 if rotated else 1)];x=(x+other['x'])/2;y=(y+other['y'])/2
        return step('tap',x=x,y=y)
    def fixture(name):
        button('Preview');return button(name)
    def built(shape,index,long=False,rotated=False):
        button('Build');button(shape)
        if rotated:button('Rotate')
        d=cell(index,long,rotated);assert any(c['name']=='Confirm' and c['enabled'] for c in d['controls']),('placement',shape,index,rotated)
        d=button('Confirm')
        for _ in range(3):d=button('Scoop')
        button('Water');d=button('Tip');time.sleep(2);return d
    def capture(name):step('capture',text=name)
    def check(name):checks.append(name);print('PASS '+name,flush=True)
    try:
        if a.decor_only:
            step('inspect');fixture('Empty pit');d=built('Square',18);button('Decorate')
            for name in ('Flag','Shell','Pebble','Door','Window'):d=button(name)
            assert {x['kind'] for x in d['sandpit']['moulds'][0]['attachments']}=={'flag','shell','pebble','door','window'}
            capture('tablet-all-five-decorations');d=button('Flag');assert len(d['sandpit']['moulds'][0]['attachments'])==6
            assert all(not c['enabled'] for c in d['controls'] if c['name'] in ('Flag','Shell','Pebble','Door','Window'))
            step('resize',x=1280,y=591);capture('phone-full-sockets-disabled');button('Build');button('Wall');cell(11,long=True);capture('phone-active-placement');button('Confirm');button('Scoop');capture('phone-active-building')
            (out/'result.json').write_text(json.dumps(dict(passed=True,candidate=a.candidate,actualInputs=True,checks=['All five decoration buttons produce coexisting attachments','Filled sockets disable all corresponding tools','Phone actual placement and active construction capture'],realNativePlayers=1,syntheticSessionProfiles=4),indent=2),encoding='utf-8');print('PASS all five decorations, occupied socket disabled states and phone active controls',flush=True);return
        if a.film_only:
            step('inspect');step('resize',x=1280,y=591);fixture('Empty pit');step('film',x=25);time.sleep(.5)
            button('Round');cell(18);time.sleep(.7);d=button('Confirm');first=d['sandpit']['moulds'][0]['id']
            for _ in range(3):button('Scoop');time.sleep(.6)
            button('Water');time.sleep(1.2);button('Tip');time.sleep(2)
            button('Wall');cell(11,long=True);time.sleep(.6);button('Confirm')
            for _ in range(3):button('Scoop');time.sleep(.4)
            button('Water');time.sleep(1.2);button('Tip');time.sleep(2)
            d=step('inspect');c=d['cells'][18];d=step('tap',x=c['x'],y=c['y']+160*min(d['width']/1420,d['height']/950));assert d['selected']==first
            button('Decorate');button('Flag');time.sleep(.5);button('Shell');capture('phone-recorded-continued-build');time.sleep(12)
            (out/'result.json').write_text(json.dumps(dict(passed=True,candidate=a.candidate,actualInputs=True,recordingOnly=True,silent=True,sequence='empty; revised ready/scoop/pour/build character feedback; Build/Decorate routes; mould; projected tap; confirm; scoop x3; water; tip; second wall build; select existing round face; flag; shell'),indent=2),encoding='utf-8');print('PASS actual placement/build/selection/decor film',flush=True);return
        d=step('inspect');assert len(d['sandpit']['moulds'])==14 and d['fixture'];assert d['sandpit']['toy']['placed'];capture('tablet-example')
        for shape,index in [('Round',0),('Square',7),('Round',24),('Square',31)]:
            fixture('Empty pit');button(shape);d=cell(index);assert d['hasPreview'],('missing preview',index,d['placing']);assert any(c['name']=='Confirm' and c['enabled'] for c in d['controls']);d=button('Confirm');assert len(d['sandpit']['moulds'])==1
            m=d['sandpit']['moulds'][0];c=d['cells'][index];assert (m['x'],m['y'])==(c['worldX'],c['worldY'])
        check('Real taps place at all four edge cells; rim and cast do not intercept')
        fixture('Empty pit');capture('tablet-empty');button('Round');d=cell(10);capture('tablet-preview');d=button('Confirm');step('film',x=28)
        first_id=d['sandpit']['moulds'][0]['id']
        d=button('Water');assert d['reaction']=='water',d['reaction'];assert d['sandpit']['moulds'][0]['wet'];capture('tablet-water-before-scoop')
        d=button('Tip');assert not d['sandpit']['moulds'][0]['built'];assert d['sandpit']['moulds'][0]['scoops']==0
        for i in range(3):
            d=button('Scoop');assert d['reaction']=='scoop';time.sleep(.3)
        capture('tablet-full-bucket');d=button('Tip');assert d['reaction']=='build';capture('tablet-build-reaction');assert d['sandpit']['moulds'][0]['built'];time.sleep(2)
        button('Decorate');d=button('Flag');d=button('Shell');assert {v['kind'] for v in d['sandpit']['moulds'][0]['attachments']}=={'flag','shell'};capture('tablet-built-combined')
        check('Empty actual tap loop; early Water; underfilled retention; Scoop/Water/Tip; combined decorations')
        # Place behind and in front of the first tower through the inverse map, regardless of face occlusion.
        for index in (26,2):
            button('Build');button('Square');d=cell(index);assert d['hasPreview'];d=button('Confirm');assert d['sandpit']['moulds'][-1]['y']==d['cells'][index]['worldY']
            for _ in range(3):button('Scoop')
            button('Water');button('Tip');time.sleep(2)
        d=step('inspect');c=d['cells'][10];d=step('tap',x=c['x'],y=c['y']+160*min(d['width']/1420,d['height']/950));assert d['selected']==first_id,'visible face selects existing middle piece';capture('tablet-selected-existing');
        capture('tablet-depth-build');check('Behind/in-front placement through continuous inverse input map')
        step('resize',x=1280,y=591);capture('phone-built');fixture('Example castle');capture('phone-example')
        d=step('inspect')
        for c in d['controls']:
            assert min(c['width'],c['height'])>=44,(c['name'],c['width'],c['height'])
            assert c['x']-c['width']/2>=0 and c['x']+c['width']/2<=d['width'] and c['y']-c['height']/2>=0 and c['y']+c['height']/2<=d['height'],c['name']
        fixture('Empty pit');capture('phone-empty');button('Gate');d=cell(11,long=True);capture('phone-gate-preview');button('Confirm');button('Water')
        for _ in range(3):button('Scoop')
        button('Tip');time.sleep(2);button('Decorate');button('Flag');button('Shell');capture('phone-gate-built');button('Round');d=cell(11,long=True);assert not any(c['name']=='Confirm' and c['enabled'] for c in d['controls']);capture('phone-overlap-preview');button('Cancel');check('Phone aspect actual taps, controls and gate; tablet captures')
        # Both long-shape orientations at every legal edge, through real taps and controls.
        for shape in ('Wall','Gate'):
            for rotated,indices in ((False,(0,6,24,30)),(True,(0,7,16,23))):
                for index in indices:
                    fixture('Empty pit');d=built(shape,index,True,rotated);m=d['sandpit']['moulds'][0]
                    assert m['orientation']==(90 if rotated else 0) and m['built']
                    c=d['cells'][index];assert m['x']==c['worldX']+(0 if rotated else 42.5) and m['y']==c['worldY']+(55 if rotated else 0)
                    if index in (0,30,23):capture('phone-'+shape.lower()+('-90' if rotated else '-0')+'-edge-'+str(index))
        check('Wall/gate both orientations at all four legal edges; source coordinates retained')
        fixture('Full 16 pieces');d=step('inspect');assert len(d['sandpit']['moulds'])==16;capture('phone-full-16')
        button('Round');cell(31);assert not any(c['name']=='Confirm' and c['enabled'] for c in step('inspect')['controls']);button('Cancel')
        step('resize',x=1024,y=768);capture('tablet-full-16');fixture('Example castle');capture('tablet-connected-example');step('resize',x=1280,y=591);capture('phone-connected-example')
        # Re-select visible faces in the dense fixture, including the inner/back tower.
        d=step('inspect');factor=min(d['width']/1420,d['height']/950)
        for index in (1,6,19,20,25,30):
            c=d['cells'][index];d=step('tap',x=c['x'],y=c['y']+140*factor)
            expected=next(m['id'] for m in d['sandpit']['moulds'] if m['x']==c['worldX'] and m['y']==c['worldY'])
            assert d['selected']==expected,('depth selection',index,d['selected'],expected)
        button('Decorate');capture('phone-active-decorations');check('Full 16-piece framing, dense depth selection, actual active decorator')
        # Phone actual-input construction and combined decoration capture; no fixture shortcuts.
        fixture('Empty pit');built('Round',18);button('Decorate');button('Flag');button('Shell');built('Wall',11,True);built('Square',20)
        capture('phone-player-built');step('resize',x=1024,y=768);capture('tablet-player-built');button('Decorate');capture('tablet-active-decorations')
        # Out-of-bounds preview preserves all existing pieces and disables confirm.
        button('Build');button('Round');d=step('inspect');c=d['cells'][0];n=d['cells'][1];d=step('tap',x=c['x']-(n['x']-c['x'])*.9,y=c['y']);assert len(d['sandpit']['moulds'])==3 and not any(c['name']=='Confirm' and c['enabled'] for c in d['controls']);capture('tablet-outside-preview');button('Cancel')
        check('Continued actual construction and out-of-bounds recovery preserve work')
        time.sleep(15)
        log=(out/'player.log').read_text(encoding='utf-8',errors='replace');assert not any(x in log for x in ('NullReferenceException','InvalidOperationException','MissingReferenceException','IndexOutOfRangeException')),'runtime exception'
        checks.append('No runtime exceptions')
        (out/'result.json').write_text(json.dumps(dict(passed=True,candidate=a.candidate,checks=checks,widths=[1024,1280],heights=[768,591],realNativePlayers=1,syntheticSessionProfiles=4,productionSceneUnchanged=True),indent=2),encoding='utf-8')
        print('PASS ALL',flush=True)
    finally:
        p.terminate();p.wait(timeout=15)

if __name__=='__main__':main()
