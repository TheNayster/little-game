import assert from 'node:assert/strict';
import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve,dirname} from 'node:path';
import {fresh,choose,state,action,step,clone,valid,IDS,slimeReady,powered,lightPath,iceAmount} from '../docs/implementation/science-playground/model.mjs';
import {kidFlow,performKidAction,HINTS} from '../docs/implementation/science-playground/kids-flow.mjs';
import {picture} from '../docs/implementation/science-playground/pictures.mjs';
const results=[],clips=[];
const test=(name,fn)=>{try{fn();results.push({name,passed:true});}catch(e){results.push({name,passed:false,error:e.message});}};
const setup=id=>{const w=fresh();choose(w,0,id);return w;};
const flow=w=>kidFlow(w.players[0].activity,state(w,0));
const hit=(w,id)=>{const f=flow(w),b=id?[f.primary,...f.extras].find(b=>b?.id===id):f.primary;assert.ok(b,`Missing ${id||'primary'} action`);assert.ok(performKidAction(w,0,b,action));return flow(w);};
const advance=(w,t)=>{for(let i=0;i<t*60;i++)step(w,1/60);};

test('all 15 starters have one primary, at most three picture actions, and short recorded help',()=>{
 for(const id of IDS){const w=setup(id),f=flow(w);assert.ok(f.primary);assert.ok(f.extras.length<=2);assert.ok(HINTS[f.hint]);for(const b of [f.primary,...f.extras]){assert.ok(b.label.length<=12);assert.match(picture(b.icon),/<svg/);assert.ok(b.commands.length);}}
});
test('slime progresses Pour → Mix → Stretch → tear → Squish using actual material state',()=>{
 const w=setup('slime');assert.equal(hit(w).hint,'slimeMix');assert.equal(hit(w).hint,'slimePlay');assert.ok(slimeReady(state(w,0)));hit(w);assert.ok(state(w,0).stretch>.8);assert.equal(hit(w,'pull').hint,'slimeTorn');assert.equal(hit(w).hint,'slimePlay');assert.equal(state(w,0).stretch,0);
});
test('rocket pump and launch lead to a passive flight and a return action',()=>{
 const w=setup('rocket');assert.equal(hit(w).hint,'rocketGo');assert.equal(state(w,0).air,3);assert.equal(hit(w).hint,'rocketWatch');assert.equal(flow(w).primary,null);advance(w,20);assert.equal(flow(w).hint,'rocketBack');assert.ok(state(w,0).x>400);assert.equal(hit(w).hint,'rocketPump');assert.equal(state(w,0).air,0);
});
test('foam requires all three real ingredients and finishes without another tap',()=>{
 const w=setup('foam');assert.equal(hit(w).hint,'foamSoap');advance(w,2);assert.equal(state(w,0).foam,0);assert.equal(hit(w).hint,'foamYeast');assert.equal(hit(w).hint,'foamWatch');advance(w,2);assert.ok(state(w,0).foam>0);advance(w,35);assert.equal(flow(w).hint,'foamAgain');assert.equal(hit(w).hint,'foamPour');
});
test('ice melt remains finite and toy selection disappears once melting starts',()=>{
 const w=setup('ice');hit(w,'dinosaur');hit(w,'tool');hit(w);assert.equal(flow(w).extras.length,1);advance(w,3);assert.ok(iceAmount(state(w,0))>0);for(let n=0;n<8&&!state(w,0).freed;n++){hit(w);advance(w,3);}assert.equal(flow(w).hint,'iceFree');assert.equal(flow(w).primary,null);
});
test('milk places color before soap, soap moves existing drops',()=>{
 const w=setup('milk');assert.equal(hit(w).hint,'milkSoap');const old=clone(state(w,0).drops);hit(w);advance(w,1);assert.equal(state(w,0).drops.length,1);assert.notDeepEqual(state(w,0).drops,old);
});
test('circuit connects a loop before switching and can recover edited incomplete wires',()=>{
 const w=setup('circuits');assert.equal(hit(w).hint,'switch');assert.ok(!powered(state(w,0)));assert.equal(hit(w).hint,'circuitPlay');hit(w,'output');assert.equal(state(w,0).load,'fan');action(w,0,'wire',[0,5]);assert.equal(flow(w).hint,'wire');hit(w);assert.ok(powered(state(w,0)));
});
test('bubble wand dips, blows finite film, and returns to Dip',()=>{
 const w=setup('bubbles');hit(w);hit(w);hit(w);assert.equal(hit(w).hint,'blow');hit(w);assert.equal(state(w,0).bubbles.length,3);for(let i=0;i<3;i++)hit(w);assert.equal(flow(w).hint,'dip');
});
test('mirror controls follow actual beam intersection and torch power',()=>{
 const w=setup('light');hit(w);assert.ok(lightPath(state(w,0)).hit);hit(w);assert.ok(!lightPath(state(w,0)).hit);hit(w);assert.ok(lightPath(state(w,0)).hit);assert.equal(hit(w,'off').hint,'lightOn');hit(w);assert.ok(state(w,0).on);
});
test('chain repairs a gap, waits during motion, and leaves the design intact',()=>{
 const w=setup('chain');action(w,0,'piece','gap');action(w,0,'place',2);assert.equal(flow(w).hint,'chainGap');hit(w);const slots=clone(state(w,0).slots);assert.equal(hit(w).hint,'chainWatch');advance(w,6);assert.equal(flow(w).hint,'chain');assert.deepEqual(state(w,0).slots,slots);
});
test('chain picture tools place a visible piece without an unexplained second step',()=>{
 const w=setup('chain'),before=clone(state(w,0).slots);hit(w,'bell');assert.notDeepEqual(state(w,0).slots,before);const after=clone(state(w,0).slots);hit(w,'ramp');assert.notDeepEqual(state(w,0).slots,after);assert.equal(state(w,0).slots[0],'domino');
});
test('lava, marble, robot, wind, weather actions change their own physical controls',()=>{
 for(const id of ['lava','marble','robot','wind','weather']){const w=setup(id),before=clone(state(w,0));hit(w);advance(w,2);assert.notDeepEqual(state(w,0),before);assert.ok(valid(w));}
});
test('every flow stays bounded for repeated input and version-2 save round trips',()=>{
 for(const id of IDS){const w=setup(id);for(let i=0;i<35;i++){const f=flow(w);assert.ok(f.extras.length+(f.primary?1:0)<=3);assert.ok(HINTS[f.hint]);if(f.primary)hit(w);advance(w,.5);assert.ok(valid(w),id);assert.deepEqual(kidFlow(id,state(clone(w),0)),flow(w));}}
});
test('four independently chosen flows never mutate another player through a button',()=>{
 const w=fresh();for(let p=0;p<4;p++)choose(w,p,'slime');for(let p=0;p<4;p++){const others=w.players.filter((_,i)=>i!==p).map(clone);performKidAction(w,p,kidFlow('slime',state(w,p)).primary,action);assert.deepEqual(w.players.filter((_,i)=>i!==p),others);}assert.ok(w.players.every(p=>p.states.slime.activator===.5));
});
test('all local hint clips match scripts and contain non-silent 22.05kHz mono PCM',()=>{
 const base=new URL('../../docs/implementation/science-playground/hints/',import.meta.url),manifest=JSON.parse(readFileSync(new URL('manifest.json',base)));assert.equal(manifest.hints.length,Object.keys(HINTS).length);
 for(const hint of manifest.hints){assert.equal(hint.text,HINTS[hint.id]);const b=readFileSync(new URL(hint.file,base));assert.equal(b.toString('ascii',0,4),'RIFF');assert.equal(b.toString('ascii',8,12),'WAVE');let fmt,data;for(let at=12;at+8<=b.length;){const kind=b.toString('ascii',at,at+4),n=b.readUInt32LE(at+4),chunk=b.subarray(at+8,at+8+n);if(kind==='fmt ')fmt=chunk;if(kind==='data')data=chunk;at+=8+n+(n%2);}assert.ok(fmt&&data);assert.equal(fmt.readUInt16LE(0),1);assert.equal(fmt.readUInt16LE(2),1);assert.equal(fmt.readUInt32LE(4),22050);assert.equal(fmt.readUInt16LE(14),16);let peak=0;for(let i=0;i<data.length;i+=2)peak=Math.max(peak,Math.abs(data.readInt16LE(i)));const duration=data.length/44100;assert.ok(peak>500);assert.ok(duration>1&&duration<12);clips.push({id:hint.id,seconds:Number(duration.toFixed(2)),peak});}
});
const report={date:'2026-09-28',scope:'Browser prototype flow and generated hint files; no Unity, device, network, or child-usability qualification',total:results.length,passed:results.filter(r=>r.passed).length,results,clips};
if(process.argv[2]){const out=resolve(process.argv[2]);mkdirSync(dirname(out),{recursive:true});writeFileSync(out,JSON.stringify(report,null,2)+'\n');}
console.log(JSON.stringify(report,null,2));process.exitCode=report.passed===report.total?0:1;
