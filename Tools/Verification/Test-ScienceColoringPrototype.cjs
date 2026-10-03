// Tests the research prototype's model, not the Unity game or live family server.
const assert=require('node:assert/strict');
const fs=require('node:fs');
const path=require('node:path');
const m=require('../docs/implementation/home-science-coloring-prototype.js');
const checks=[];
function test(name,body){body();checks.push({name,passed:true});console.log('PASS '+name);}
test('same cargo can sink a narrow hull and float in a wider hull; removing cargo restores buoyancy',()=>{
  const b=m.profile().science.boat;b.cargo=3;assert(m.boatResult(b).sinks);b.wide=true;assert(!m.boatResult(b).sinks);b.wide=false;b.cargo=2;assert(!m.boatResult(b).sinks);
  for(const wide of [false,true]){let previous=0;for(let cargo=0;cargo<=7;cargo++){const result=m.boatResult({wide,cargo});assert(result.ratio>=previous);previous=result.ratio;}}
});
test('magnet moves iron only, preserves other sample positions and remains inside its tray',()=>{
  const tray=m.profile().science.magnet,others=JSON.stringify(tray.items.slice(1));m.moveMagnet(tray,140,250);assert.equal(tray.items[0].y,290);assert.equal(JSON.stringify(tray.items.slice(1)),others);
  for(const item of tray.items.slice(1))m.moveMagnet(tray,item.x,item.y-60);
  assert.equal(JSON.stringify(tray.items.slice(1)),others);m.moveMagnet(tray,-100,2000);assert.equal(tray.x,55);assert.equal(tray.y,370);
});
test('RGB combinations identify additive secondary colors and white',()=>{
  assert.equal(m.LIGHTS[1|2],'Yellow');assert.equal(m.LIGHTS[1|4],'Magenta');assert.equal(m.LIGHTS[2|4],'Cyan');assert.equal(m.LIGHTS[1|2|4],'White');assert.equal(m.LIGHTS[0],'Dark');assert.equal(new Set(m.LIGHTS).size,8);
});
test('four owners can color the same template independently; undo and page switching preserve siblings',()=>{
  const data=m.fresh();data.players.forEach((p,i)=>m.fill(p,'body',m.COLORS[i][1]));const siblings=JSON.stringify(data.players.slice(1));m.undo(data.players[0]);assert.equal(m.paper(data.players[0]).fills.body,'#ffffff');assert.equal(JSON.stringify(data.players.slice(1)),siblings);m.redo(data.players[0]);assert.equal(m.paper(data.players[0]).fills.body,m.COLORS[0][1]);
  const first=data.players[0];first.page='truck';assert.deepEqual(m.paper(first).fills,{});first.page='dinosaur';assert.equal(m.paper(first).fills.body,m.COLORS[0][1]);assert(m.valid(JSON.parse(JSON.stringify(data))));
});
test('a freehand gesture is one undo step, redo restores it, a new stroke clears abandoned redo',()=>{
  const p=m.profile();p.page='blank';const points=[[1,5],[50,30],[80,20]];m.stroke(p,points);assert.equal(m.paper(p).strokes.length,1);m.undo(p);assert.equal(m.paper(p).strokes.length,0);m.redo(p);assert.deepEqual(m.paper(p).strokes[0].points,points);m.undo(p);m.stroke(p,[[2,3],[4,5]]);assert.equal(m.paper(p).redo.length,0);
});
test('science state and all six page records survive serialization with four independent owners',()=>{
  const data=m.fresh();for(const [i,p] of data.players.entries()){p.science.boat.cargo=i;p.science.lights=i;for(const page of m.PAGES){p.page=page;if(page==='blank')m.stroke(p,[[12,30],[200,300]]);else m.fill(p,'body',m.COLORS[i][1]);}}
  const restored=JSON.parse(JSON.stringify(data));assert(m.valid(restored));assert.deepEqual(restored,data);restored.players[0].science.boat.cargo=7;assert.equal(restored.players[1].science.boat.cargo,1);
});
test('corrupt colors, undo operations, out-of-paper coordinates and excessive strokes are rejected',()=>{
  const data=m.fresh(),p=data.players[0];p.page='blank';m.stroke(p,[[2,3],[5,7]]);assert(m.valid(data));const corrupt=mutate=>{const v=JSON.parse(JSON.stringify(data));mutate(v.players[0]);assert(!m.valid(v));};
  corrupt(v=>v.pages.blank.strokes[0].points[0][0]=801);corrupt(v=>v.pages.blank.undo[0].value.color='invalid');corrupt(v=>v.science.lights=8);corrupt(v=>v.pages.blank.undo=[{type:'erase-other-player'}]);corrupt(v=>v.science.magnet.items=null);
});
test('bounded prototype drawing/history refuses extra strokes without deleting existing pictures',()=>{
  const data=m.fresh();data.players.forEach(p=>{p.page='blank';for(let i=0;i<100;i++)assert(m.stroke(p,Array.from({length:256},(_,j)=>[j*3,Math.round(230+100*Math.sin(j))])));const before=JSON.stringify(m.paper(p));assert(!m.stroke(p,[[1,1],[3,3]]));assert.equal(JSON.stringify(m.paper(p)),before);assert.equal(m.paper(p).undo.length,40);});
  assert(m.valid(data));assert(Buffer.byteLength(JSON.stringify(data))<2*1024*1024);
});
const result={passed:true,checks,scope:'Browser research model only',unityGameplayTested:false,networkMultiplayerTested:false,liveFamilyTouched:false};
if(process.argv[2]){const out=path.resolve(process.argv[2]);fs.mkdirSync(path.dirname(out),{recursive:true});fs.writeFileSync(out,JSON.stringify(result,null,2)+'\n');}
console.log(JSON.stringify({passed:true,groups:checks.length}));
