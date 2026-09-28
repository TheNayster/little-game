import assert from 'node:assert/strict';
import {writeFileSync,mkdirSync} from 'node:fs';
import {resolve,dirname} from 'node:path';
import {fresh,state,choose,action,step,clone,valid,upgrade,VERSION,IDS,nextIce,iceAmount,liquidVolume,liquidColor,liquidName} from '../docs/implementation/science-playground/model.mjs';
import {kidFlow,performKidAction} from '../docs/implementation/science-playground/kids-flow.mjs';
const results=[];
const test=(name,fn)=>{try{fn();results.push({name,passed:true});}catch(e){results.push({name,passed:false,error:e.message});}};
const setup=id=>{const w=fresh();choose(w,0,id);return w;};
const advance=(w,t)=>{for(let n=0;n<t*60;n++)step(w,1/60);};
const hit=w=>{const b=kidFlow(w.players[0].activity,state(w,0)).primary;assert.ok(b);assert.ok(performKidAction(w,0,b,action));};
const bubbleMix=w=>{for(const k of ['water','soap','stir'])action(w,0,k);};
const legacy=w=>{const old=clone(w);old.version=1;for(const p of old.players){delete p.states.colors;for(const k of ['tool','hits','chips','strike'])delete p.states.ice[k];for(const k of ['water','soap','mixed','solution','big','pops'])delete p.states.bubbles[k];}return old;};

test('hammer cracks the struck ice locally, breaks on later taps, and never creates heat',()=>{
 const w=setup('ice'),s=state(w,0);assert.equal(s.tool,'hammer');action(w,0,'chip',{x:390,y:255});assert.ok(s.cells.some(v=>v>0&&v<1));assert.equal(s.cells[23],1);assert.ok(s.energy.every(v=>v===0));action(w,0,'chip',{x:390,y:255});assert.ok(s.cells.includes(0));assert.ok(s.chips.length>0);assert.equal(s.hits,2);assert.ok(!s.freed);const before=clone(s);action(w,0,'chip',{x:0,y:0});assert.deepEqual(s,before);
});
test('simple hammer flow fully rescues a draggable dinosaur and keeps particle effects bounded',()=>{
 const w=setup('ice');let taps=0;while(!state(w,0).freed&&taps++<60){hit(w);advance(w,.2);}const s=state(w,0);assert.ok(s.freed);assert.ok(taps<40);assert.ok(s.cells.every(v=>v===0));assert.ok(s.chips.length<=48);action(w,0,'move',{x:690,y:420});assert.equal(s.x,690);assert.equal(s.y,420);advance(w,2);assert.equal(s.chips.length,0);assert.ok(valid(w));
});
test('water melting and hammer chipping share the same retained ice',()=>{
 const w=setup('ice');action(w,0,'pour');advance(w,2);const before=iceAmount(state(w,0));action(w,0,'chip',nextIce(state(w,0)));assert.ok(iceAmount(state(w,0))>before);action(w,0,'iceTool','water');assert.equal(kidFlow('ice',state(w,0)).hint,'ice');const saved=clone(w);assert.ok(valid(saved));assert.deepEqual(saved,w);
});
test('bubble film requires real water, soap and mixing before dipping',()=>{
 const w=setup('bubbles'),s=state(w,0);action(w,0,'dip');action(w,0,'blow');assert.equal(s.bubbles.length,0);hit(w);assert.equal(kidFlow('bubbles',s).hint,'bubbleSoap');action(w,0,'dip');assert.equal(s.film,0);hit(w);assert.equal(kidFlow('bubbles',s).hint,'bubbleStir');hit(w);assert.equal(s.solution,4);hit(w);assert.ok(s.film>0);hit(w);assert.equal(s.bubbles.length,3);
});
test('big bubbles spend more film, square wands still release round bubbles, pop is local',()=>{
 const small=setup('bubbles'),big=setup('bubbles');for(const w of [small,big]){bubbleMix(w);action(w,0,'dip');}action(big,0,'big');action(big,0,'shape');action(small,0,'blow');action(big,0,'blow');const a=state(small,0),b=state(big,0);assert.equal(a.bubbles.length,3);assert.equal(b.bubbles.length,1);assert.ok(b.bubbles[0].r>a.bubbles[0].r*1.8);assert.ok(b.film<a.film);assert.equal(b.shape,'square');assert.deepEqual(Object.keys(b.bubbles[0]).sort(),['age','id','r','vx','vy','x','y']);const target=clone(a.bubbles[0]);action(small,0,'pop',target);assert.equal(a.bubbles.length,2);assert.equal(a.pops.length,1);advance(small,1);assert.equal(a.pops.length,0);
});
test('exhausted mixture cannot be refilled by repeated stirring; a fresh batch preserves flying bubbles',()=>{
 const w=setup('bubbles');bubbleMix(w);for(let i=0;i<16;i++)action(w,0,'dip');const s=state(w,0);assert.equal(s.solution,0);action(w,0,'blow');const bubbles=clone(s.bubbles);s.film=0;action(w,0,'stir');action(w,0,'dip');assert.equal(s.film,0);assert.equal(s.solution,0);hit(w);assert.equal(s.water,0);assert.deepEqual(s.bubbles,bubbles);assert.equal(kidFlow('bubbles',s).hint,'bubbleWater');
});
test('liquid pairs produce orange, green and purple independently of pour order',()=>{
 for(const [a,b,name,rgb] of [[0,1,'Orange',[242,137,52]],[1,2,'Green',[60,172,113]],[0,2,'Purple',[153,90,193]]]){const w=setup('colors'),v=setup('colors');for(const n of [a,b])action(w,0,'pourColor',n);for(const n of [b,a])action(v,0,'pourColor',n);assert.equal(liquidName(state(w,0)),name);assert.deepEqual(liquidColor(state(w,0)),rgb);assert.deepEqual(liquidColor(state(w,0)),liquidColor(state(v,0)));assert.equal(liquidVolume(state(w,0)),2);}
});
test('ratios change shade, water dilutes, equal three colors become earthy instead of white',()=>{
 const w=setup('colors'),s=state(w,0);action(w,0,'pourColor',0);action(w,0,'pourColor',1);const orange=liquidColor(s);action(w,0,'pourColor',0);assert.notDeepEqual(liquidColor(s),orange);action(w,0,'pourColor',1);assert.deepEqual(liquidColor(s),orange);action(w,0,'water');const diluted=liquidColor(s);const luminance=rgb=>rgb[0]*.2126+rgb[1]*.7152+rgb[2]*.0722;assert.ok(luminance(diluted)>luminance(orange));assert.ok(Math.max(...diluted)-Math.min(...diluted)<Math.max(...orange)-Math.min(...orange));action(w,0,'pourColor',2);action(w,0,'pourColor',2);assert.equal(liquidName(s),'Earthy brown');assert.ok(liquidColor(s).every(v=>v<190));
});
test('liquid fill is bounded, settles without further taps, and resets only the chosen player',()=>{
 const w=setup('colors');choose(w,1,'colors');action(w,1,'pourColor',2);const other=clone(state(w,1));for(let n=0;n<80;n++)action(w,0,'pourColor',n%3);assert.equal(liquidVolume(state(w,0)),12);assert.equal(kidFlow('colors',state(w,0)).hint,'colorsFull');advance(w,3);assert.equal(state(w,0).mix,1);const otherAfterTime=clone(state(w,1));hit(w);assert.deepEqual(state(w,1),otherAfterTime);assert.deepEqual(state(w,1).parts,other.parts);assert.equal(liquidVolume(state(w,0)),0);assert.ok(valid(w));
});
test('version-1 migration preserves all 56 earlier workspaces and adds four separate color labs',()=>{
 const w=fresh();w.players[0].activity='ice';w.players[0].states.ice.cells[5]=.38;w.players[0].states.ice.energy[5]=.2;w.players[0].states.ice.drops=3;w.players[1].states.bubbles.film=.5;w.players[2].states.robot.paths=[[500,300,0,2]];w.players[3].states.chain.joined=true;
 const old=legacy(w),before=JSON.stringify(old),next=upgrade(old);assert.equal(JSON.stringify(old),before);assert.equal(next.version,VERSION);assert.ok(valid(next));for(let p=0;p<4;p++){for(const [id,s] of Object.entries(old.players[p].states)){for(const k of Object.keys(s))assert.deepEqual(next.players[p].states[id][k],s[k],`${p}/${id}/${k}`);}assert.equal(Object.keys(next.players[p].states).length,15);assert.equal(next.players[p].states.bubbles.solution,4);}assert.notEqual(next.players[0].states.colors,next.players[1].states.colors);assert.deepEqual(upgrade(next),next);
});
test('damaged/future saves are rejected and new records validate bounded shapes',()=>{
 const old=legacy(fresh());old.players[0].states.ice.cells[0]='bad';assert.throws(()=>upgrade(old));const future=fresh();future.version=3;assert.throws(()=>upgrade(future));for(const mutate of [w=>w.players[0].states.colors.parts[0]=-1,w=>w.players[0].states.colors.water=99,w=>w.players[0].states.ice.chips.push({x:1}),w=>w.players[0].states.bubbles.solution=50]){const w=fresh();mutate(w);assert.ok(!valid(w));assert.throws(()=>upgrade(w));}
});
test('four players can hammer, mix bubbles and mix colors without cross-player command mutation',()=>{
 const w=fresh();for(const id of ['ice','bubbles','colors'])for(let p=0;p<4;p++){choose(w,p,id);const others=w.players.filter((_,i)=>i!==p).map(clone);performKidAction(w,p,kidFlow(id,state(w,p)).primary,action);assert.deepEqual(w.players.filter((_,i)=>i!==p),others);}assert.ok(valid(w));assert.equal(IDS.length,15);
});
const report={date:'2026-09-28',scope:'Browser prototype hammer, bubble lab, liquid pigments and version-1 migration; no Unity/device/network qualification',total:results.length,passed:results.filter(r=>r.passed).length,results};
if(process.argv[2]){const out=resolve(process.argv[2]);mkdirSync(dirname(out),{recursive:true});writeFileSync(out,JSON.stringify(report,null,2)+'\n');}console.log(JSON.stringify(report,null,2));process.exitCode=report.passed===report.total?0:1;
