import assert from 'node:assert/strict';
import {writeFileSync,mkdirSync} from 'node:fs';
import {dirname,resolve} from 'node:path';
import {fresh,state,choose,action,step,clone,valid,IDS,iceAmount,slimeReady,powered,lightPath,startFamily} from '../docs/implementation/science-playground/model.mjs';

const results=[];
function test(name,fn){try{fn();results.push({name,passed:true});}catch(e){results.push({name,passed:false,error:e.message});}}
const advance=(w,seconds)=>{for(let i=0;i<Math.round(seconds*60);i++)step(w,1/60);};
const setup=id=>{const w=fresh();choose(w,0,id);return [w,state(w,0)];};
const act=(w,k,v)=>action(w,0,k,v);
const near=(a,b,epsilon=1e-6)=>assert.ok(Math.abs(a-b)<epsilon,`${a} != ${b}`);

test('14 complete workspaces per player, no shared references',()=>{
 const w=fresh();assert.equal(IDS.length,14);assert.ok(valid(w));
 for(const p of w.players){assert.equal(Object.keys(p.states).length,14);for(const id of IDS)assert.notEqual(p.states[id],w.players[(p.id+1)%4].states[id]);}
});
test('warm lava reacts faster but cannot create extra reserve',()=>{
 const [warm,ws]=setup('lava'),[cool,cs]=setup('lava');act(warm,'tablet');act(cool,'tablet');act(cool,'heat',0);advance(warm,4);advance(cool,4);assert.ok(ws.used>cs.used);advance(warm,35);advance(cool,35);near(ws.used,1);near(cs.used,1);assert.equal(ws.blobs.length,0);assert.equal(cs.blobs.length,0);
});
test('ice responds to finite local heat; warm drops melt more',()=>{
 const [w,s]=setup('ice'),[c,t]=setup('ice');act(c,'heat',0);act(w,'drop',{x:500,y:300});act(c,'drop',{x:500,y:300});advance(w,8);advance(c,8);assert.ok(iceAmount(s)>iceAmount(t)*2);assert.ok(s.cells.some(v=>v===1));const before=iceAmount(s);advance(w,60);assert.ok(iceAmount(s)-before<.001);
 for(let i=0;i<8;i++){act(w,'pour');advance(w,3);}assert.ok(s.freed);act(w,'move',{x:650,y:400});assert.equal(s.x,650);
});
test('marble changes path with geometry and rough surface slows it',()=>{
 const [w,s]=setup('marble'),[r,t]=setup('marble');act(w,'release');act(r,'rough');act(r,'release');advance(w,3);advance(r,3);assert.ok(s.ball.x>t.ball.x+10);
 const [m,u]=setup('marble');act(m,'handle',{index:1,x:620,y:235});act(m,'release');advance(m,3);assert.notEqual(Math.round(s.ball.x),Math.round(u.ball.x));advance(w,60);assert.ok(s.ball.done);assert.ok(s.last>650&&s.last<850,'ready-made course must reach its basket');assert.ok(s.trace.length<=220);
});
test('slime formation, slow stretch, fast tear and reunion',()=>{
 const [w,s]=setup('slime');act(w,'slow');assert.equal(s.stretch,0);act(w,'stir');assert.ok(!slimeReady(s));act(w,'activator');act(w,'stir');act(w,'stir');assert.ok(slimeReady(s));act(w,'pull',{x:780,y:340,speed:100});assert.ok(s.stretch>.7&&!s.torn);act(w,'pull',{x:790,y:340,speed:2000});assert.ok(s.torn);act(w,'squish');assert.ok(!s.torn&&s.stretch===0);
});
test('soap advects existing dye even at exact-center tap and decays',()=>{
 const [w,s]=setup('milk');act(w,'tool','soap');act(w,'touch',{x:500,y:295});advance(w,1);assert.equal(s.drops.length,0);act(w,'tool','color');act(w,'touch',{x:500,y:295});act(w,'tool','soap');act(w,'touch',{x:500,y:295});advance(w,1);assert.ok(Math.hypot(s.drops[0].x-500,s.drops[0].y-295)>1);advance(w,8);assert.equal(s.bursts.length,0);assert.ok(s.drops.every(d=>Math.hypot((d.x-500)/345,(d.y-295)/193)<=1.00001));
});
test('robot needs power; imbalance changes trace; history bounded',()=>{
 const [w,s]=setup('robot'),[v,t]=setup('robot');advance(w,2);assert.equal(s.paths.length,0);act(w,'run');act(v,'balance',.2);act(v,'run');advance(w,4);advance(v,4);assert.notDeepEqual(s.paths,t.paths);act(w,'run');const n=s.paths.length;advance(w,2);assert.equal(s.paths.length,n);advance(v,100);assert.equal(t.paths.length,1200);
});
test('rocket requires gas; larger reserve travels farther; added mass slows',()=>{
 const [w,s]=setup('rocket');act(w,'release');assert.equal(s.phase,'idle');act(w,'pump');act(w,'release');advance(w,20);const small=s.x;assert.equal(s.phase,'rest');assert.equal(s.air,0);act(w,'return');act(w,'pump');act(w,'pump');act(w,'release');advance(w,20);assert.ok(s.x>small+50);
 const [a,x]=setup('rocket'),[b,y]=setup('rocket');for(const v of[a,b]){act(v,'pump');act(v,'pump');}act(b,'mass');act(a,'release');act(b,'release');advance(a,1);advance(b,1);assert.ok(x.x>y.x);
});
test('foam catalyst changes rate, cannot replace reactant; soap traps gas',()=>{
 const [w,s]=setup('foam'),[v,t]=setup('foam');for(const z of[w,v]){act(z,'peroxide');act(z,'yeast');}act(w,'soap');advance(w,3);advance(v,3);near(s.used,t.used);assert.ok(s.foam>0);assert.equal(t.foam,0);advance(w,20);near(s.used,1);assert.equal(s.reactant,0);const catalyst=s.catalyst;act(w,'yeast');advance(w,2);near(s.used,1);assert.ok(s.catalyst>catalyst);
});
test('wind lift responds to canopy and mass, fan off returns object',()=>{
 const [a,x]=setup('wind'),[b,y]=setup('wind');act(a,'fan',2);act(b,'fan',2);act(b,'area');advance(a,6);advance(b,6);assert.ok(y.height>x.height);act(b,'weight');act(b,'weight');advance(b,15);assert.ok(y.height<x.height);act(a,'fan',0);advance(a,10);assert.equal(x.height,0);
});
test('circuit requires a closed load loop and rejects shorts',()=>{
 const [w,s]=setup('circuits');act(w,'wire',[0,5]);act(w,'switch');assert.ok(!powered(s));act(w,'loop');assert.ok(powered(s));act(w,'switch');assert.ok(!powered(s));const before=clone(s.wires);assert.equal(act(w,'wire',[0,1]),false);assert.deepEqual(s.wires,before);act(w,'switch');for(const load of ['lamp','fan','buzzer']){act(w,'load',load);assert.ok(powered(s));}act(w,'wire',[0,5]);assert.ok(!powered(s));
});
test('water is conserved; condensation required before precipitation',()=>{
 const [w,s]=setup('weather');advance(w,25);assert.equal(s.rain,0);assert.ok(s.vapor>25);act(w,'cool',1.5);advance(w,20);assert.ok(s.rain>0);near(s.water+s.vapor+s.cloud+s.fall,100);act(w,'cool',2);advance(w,5);assert.ok(s.snow>0&&s.rain===0);advance(w,300);near(s.water+s.vapor+s.cloud+s.fall,100);
});
test('bubbles require film; pop affects only selected bubble',()=>{
 const [w,s]=setup('bubbles');act(w,'blow');assert.equal(s.bubbles.length,0);act(w,'dip');act(w,'shape');act(w,'blow');assert.equal(s.shape,'square');assert.equal(s.bubbles.length,3);act(w,'pop',{x:s.bubbles[0].x,y:s.bubbles[0].y});assert.equal(s.bubbles.length,2);for(let i=0;i<10;i++)act(w,'blow');assert.ok(s.bubbles.length<=14);advance(w,20);assert.equal(s.bubbles.length,0);
});
test('reflection changes beam; prism must intersect; source off stops rainbow',()=>{
 const [w,s]=setup('light');act(w,'align');assert.ok(lightPath(s).hit);const dx=lightPath(s).dx;act(w,'angle',-20);assert.notEqual(lightPath(s).dx,dx);assert.ok(!lightPath(s).hit);act(w,'align');assert.ok(lightPath(s).hit);act(w,'light');assert.ok(!lightPath(s).hit);act(w,'light');act(w,'angle',65);act(w,'align');assert.ok(lightPath(s).hit);
});
test('chain gap blocks; completed individual run preserves design',()=>{
 const [w,s]=setup('chain');act(w,'piece','gap');act(w,'place',2);act(w,'run');advance(w,5);assert.equal(s.phase,'blocked');act(w,'piece','bell');act(w,'place',2);const slots=clone(s.slots);act(w,'run');advance(w,5);assert.equal(s.phase,'done');assert.deepEqual(s.slots,slots);
});
test('four joined chain sections complete; leaving player does not block others',()=>{
 const w=fresh();for(let i=0;i<4;i++){choose(w,i,'chain');action(w,i,'join');}startFamily(w);advance(w,20);assert.ok(!w.family.running);for(const p of w.players)assert.equal(p.states.chain.runs,1);startFamily(w);action(w,0,'join');advance(w,18);assert.ok(!w.family.running);for(let i=1;i<4;i++)assert.equal(state(w,i).runs,2);
});
test('same activity on four players: reset and navigation never clear sibling',()=>{
 const w=fresh();for(let i=0;i<4;i++){choose(w,i,'lava');for(let j=0;j<=i;j++)action(w,i,'tablet');}advance(w,1);const siblings=w.players.slice(1).map(p=>clone(p.states));action(w,0,'reset');choose(w,0,'ice');assert.deepEqual(w.players.slice(1).map(p=>p.states),siblings);advance(w,1);assert.ok(state(w,3).used>siblings[2].lava.used);
});
test('pause freezes simulations, resume continues; save restores all 56 states',()=>{
 const w=fresh();for(let i=0;i<4;i++){choose(w,i,'robot');action(w,i,'run');}advance(w,1);w.paused=true;const snapshot=clone(w);advance(w,2);assert.deepEqual(w,snapshot);w.paused=false;advance(w,1);assert.notDeepEqual(w,snapshot);const restored=clone(w);assert.ok(valid(restored));advance(w,1);advance(restored,1);assert.deepEqual(restored,w);
});
test('corrupt saves, wrong types, invalid running queues rejected',()=>{
 const mutations=[w=>w.version=99,w=>w.players[0].states.lava.fuel='3',w=>w.players[0].states.ice.cells[0]='0',w=>w.players[0].states.marble.ball={},w=>w.players[0].states.robot.paths.push([1]),w=>w.players[0].states.milk.drops.push({x:1}),w=>w.players[0].states.chain.phase='no',w=>w.family={running:true,queue:[],at:0},w=>w.family={running:true,queue:[0,0],at:0},w=>w.players[0].states.weather.water=200];for(const mutate of mutations){const w=fresh();mutate(w);assert.ok(!valid(w));}
});
test('stress inputs stay bounded and complete saved model remains valid',()=>{
 const w=fresh();for(let p=0;p<4;p++)for(const id of IDS){choose(w,p,id);for(let n=0;n<100;n++){const actions={lava:['tablet'],ice:['pour'],marble:['release'],slime:['activator','stir'],milk:['touch'],robot:['run'],rocket:['pump'],foam:['peroxide','soap','yeast'],wind:['weight'],circuits:['loop'],weather:['cool'],bubbles:['dip','blow'],light:['align'],chain:['run']}[id];for(const k of actions)action(w,p,k,k==='touch'?{x:500,y:295}:1);}}advance(w,10);assert.ok(valid(w));assert.ok(JSON.stringify(w).length<900000);assert.ok(valid(clone(w)));
});

const report={date:'2026-09-27',scope:'Pure browser-prototype model; no Unity, network, or physical-device qualification',total:results.length,passed:results.filter(r=>r.passed).length,results};
if(process.argv[2]){const path=resolve(process.argv[2]);mkdirSync(dirname(path),{recursive:true});writeFileSync(path,JSON.stringify(report,null,2)+'\n');}
console.log(JSON.stringify(report,null,2));process.exitCode=report.passed===report.total?0:1;
