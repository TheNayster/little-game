import assert from 'node:assert/strict';
import {readFileSync,existsSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve,dirname} from 'node:path';
import {fileURLToPath} from 'node:url';
import {TITLES,fresh,valid,bookmark,turn,PlaybackIntent} from '../docs/implementation/book-playground/model.mjs';
const root=fileURLToPath(new URL('../../',import.meta.url));
const books=resolve(root,'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Worlds/Home/Books');
const catalog=TITLES.map(id=>JSON.parse(readFileSync(resolve(books,id,'content.json'),'utf8')));
const results=[];const test=(name,run)=>{try{run();results.push({name,passed:true});}catch(e){results.push({name,passed:false,error:e.message});}};
test('four readers keep distinct pages and narration positions in the same title',()=>{
 const s=fresh(),b=catalog[0];for(let p=0;p<4;p++){turn(s,p,b,p+1);bookmark(s,p,b).seconds=p+.5;}
 assert.deepEqual(s.players.map(p=>p.books[b.id].page),[1,2,3,4]);assert.ok(valid(s,catalog));assert.deepEqual(JSON.parse(JSON.stringify(s)),s);
});
test('every title retains its own bookmark when another title is opened',()=>{
 const s=fresh();for(const b of catalog){turn(s,0,b,3);bookmark(s,0,b).seconds=1.25;}bookmark(s,0,catalog[0]);assert.ok(catalog.every(b=>s.players[0].books[b.id].page===3));assert.deepEqual(s.players[1].books,{});
});
test('first and last page boundaries cannot wrap or erase progress',()=>{
 const s=fresh(),b=catalog[0];bookmark(s,0,b).seconds=3;assert.equal(turn(s,0,b,-1),false);assert.equal(bookmark(s,0,b).seconds,3);turn(s,0,b,99);assert.equal(bookmark(s,0,b).page,13);assert.equal(bookmark(s,0,b).seconds,0);assert.equal(turn(s,0,b,1),false);
});
test('changed content resets only its own bookmark; player settings survive',()=>{
 const s=fresh();s.players[0].auto=true;turn(s,0,catalog[0],2);turn(s,0,catalog[1],3);bookmark(s,0,{...catalog[0],revision:99});assert.equal(s.players[0].books[TITLES[0]].page,0);assert.equal(s.players[0].books[TITLES[1]].page,3);assert.equal(s.players[0].auto,true);
});
test('malformed and future saves are rejected without changing their input',()=>{
 const candidates=[{...fresh(),version:2},{...fresh(),focus:4}];for(const field of ['page','seconds','revision']){const s=fresh();bookmark(s,0,catalog[0])[field]=-1;candidates.push(s);}for(const s of candidates){const before=JSON.stringify(s);assert.equal(valid(s,catalog),false);assert.equal(JSON.stringify(s),before);}assert.ok(valid(fresh(),catalog));
});
test('pause and navigation invalidate delayed starts; new playback cannot be stopped by an old token',()=>{
 const intent=new PlaybackIntent(),old=intent.start('voice');intent.stop();assert.equal(intent.accepts(old),false);const current=intent.start('voice');assert.equal(intent.accepts(old),false);assert.equal(intent.accepts(current),true);const effect=intent.start('effect');assert.equal(intent.accepts(current),false);assert.equal(intent.accepts(effect),true);intent.stop();assert.equal(intent.kind,null);
});
test('all six existing books and 54 page recordings are present without changing story selection',()=>{
 assert.equal(catalog.reduce((n,b)=>n+b.pages.length,0),54);assert.ok(existsSync(resolve(books,'covers.png')));
 for(const b of catalog){assert.equal(b.id,TITLES[catalog.indexOf(b)]);for(let p=0;p<b.pages.length;p++){assert.ok(b.pages[p].speech);assert.ok(existsSync(resolve(books,b.id,`audio/page-${p}.wav`)));}assert.ok(existsSync(resolve(books,b.id,'audio/effect-0.wav')));}
});
test('all story and dinosaur crop rectangles fit their existing atlases',()=>{
 for(const b of catalog){assert.equal(b.artRects.length,b.kind==='dinosaurs'?12:b.pages.length);for(const r of b.artRects){assert.ok(r.x>=0&&r.y>=0&&r.width>0&&r.height>0&&r.x+r.width<=1&&r.y+r.height<=1);}for(const name of b.kind==='dinosaurs'?['dinosaurs','dinosaurs-extra']:['pages'])assert.ok(existsSync(resolve(books,b.id,name+'.png')));}
});
const report={date:'2026-09-28',scope:'Browser reader controls, saved-page isolation and existing content integrity; not Unity or physical audio qualification',total:results.length,passed:results.filter(x=>x.passed).length,results};
if(process.argv[2]){const path=resolve(process.argv[2]);mkdirSync(dirname(path),{recursive:true});writeFileSync(path,JSON.stringify(report,null,2)+'\n');}
console.log(JSON.stringify(report,null,2));process.exitCode=report.passed===report.total?0:1;
