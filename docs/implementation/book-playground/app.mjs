import {picButton,labelButton} from '../science-playground/controls.mjs';
import {picture} from '../science-playground/pictures.mjs';
import {TITLES,fresh,valid,bookmark,turn,PlaybackIntent} from './model.mjs';

const ROOT=new URL('../../../Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Books/',import.meta.url);
const KEY='little-weeps-book-controls-v1',colors=['#629bad','#d9a164','#ab8dbe','#8bab70'];
const $=s=>document.querySelector(s),library=$('#library'),reader=$('#reader'),canvas=$('#page-art'),ctx=canvas.getContext('2d');
const saveLabel=$('#save'),status=$('#audio-status'),intent=new PlaybackIntent();
document.querySelectorAll('.preview-nav a').forEach((link,i)=>link.insertAdjacentHTML('afterbegin',picture(i?'book':'games')));
let world=fresh(),catalog=[],book=null,art=[],artGeneration=0,audio=$('#book-audio'),blocked=false,storageReady=false,loading=false,mediaError=false,lastSaved=0,selectedAnimal=0;
const asset=(id,path)=>new URL(id+'/'+path,ROOT).href;
// A tab can hide while its catalog is loading; never replace unread bookmarks.
function save(){if(blocked||!storageReady)return;try{localStorage.setItem(KEY,JSON.stringify(world));saveLabel.textContent='Your pages are saved in this browser';}catch{saveLabel.textContent='Could not save here. Keep this tab open.';saveLabel.classList.add('bad');}}
function mark(){return book?bookmark(world,world.focus,book):null;}
function capture(){if(book&&intent.kind==='voice'&&!loading&&Number.isFinite(audio.currentTime))mark().seconds=audio.ended?0:audio.currentTime;}
function halt(){capture();intent.stop();audio.pause();loading=false;status.textContent=mark()?.seconds>0?'Paused':'';updatePlayback();save();}
function image(src){return new Promise((resolve,reject)=>{const img=new Image();img.onload=()=>resolve(img);img.onerror=()=>reject(Error('Picture unavailable'));img.src=src;});}
function fit(c,img,r,x,y,w,h){const sw=r.width*img.width,sh=r.height*img.height,scale=Math.min(w/sw,h/sh);c.drawImage(img,r.x*img.width,r.y*img.height,sw,sh,x+(w-sw*scale)/2,y+(h-sh*scale)/2,sw*scale,sh*scale);}
function dinosaur(c,index,x,y,w,h){if(art.length===2)fit(c,art[Math.floor(index/6)],book.artRects[index],x,y,w,h);}
function paint(){
 if(!book||reader.hidden)return;
 const w=reader.clientWidth,h=reader.clientHeight,dpr=Math.min(devicePixelRatio||1,2);canvas.width=Math.round(w*dpr);canvas.height=Math.round(h*dpr);ctx.setTransform(dpr,0,0,dpr,0,0);
 const gradient=ctx.createLinearGradient(0,0,0,h);gradient.addColorStop(0,'#e2efdd');gradient.addColorStop(1,'#f8e5bd');ctx.fillStyle=gradient;ctx.fillRect(0,0,w,h);
 const page=book.pages[mark().page];
 if(book.kind==='dinosaurs'&&page.species>=0){const top=$('.reader-top').getBoundingClientRect().bottom+6,bottom=$('.reader-bottom').getBoundingClientRect().top-10;dinosaur(ctx,page.species,58,top,w-116,Math.max(80,bottom-top));}
 else if(book.kind!=='dinosaurs'&&art[0])fit(ctx,art[0],book.artRects[mark().page],0,0,w,h);
}
function refreshPlayers(){
 $('#players').replaceChildren();
 for(let i=0;i<4;i++){const button=document.createElement('button');button.className='player';button.style.setProperty('--player',colors[i]);button.innerHTML=`<span class="player-dot">${i+1}</span><span>Player ${i+1}</span>`;button.setAttribute('aria-label',`Player ${i+1}`);button.setAttribute('aria-pressed',i===world.focus);button.onclick=()=>{halt();world.focus=i;save();refreshPlayers();refreshShelf();};$('#players').append(button);}
}
const back=picButton('Books','book',closeBook,'small-picture');$('#book-back').append(back);
const previous=picButton('Back','previous',()=>changePage(-1),'');previous.setAttribute('aria-label','Previous page');$('#page-back').append(previous);
const next=picButton('Next','next',()=>changePage(1),'');next.setAttribute('aria-label','Next page');$('#page-next').append(next);
const read=picButton('Read to me','play',()=>{if(intent.kind==='voice')halt();else play('voice');},'picture-tool primary');
const replay=picButton('Read again','restart',()=>{halt();mark().seconds=0;play('voice');},'picture-tool');
const sound=picButton('Hear sound','listen',()=>play('effect'),'picture-tool');$('#reading-tools').append(read,replay,sound);
$('#book-options>summary').innerHTML=picture('toolbox');
$('#auto-pages').onchange=e=>{world.players[world.focus].auto=e.target.checked;save();};
$('#show-words').onchange=e=>{world.players[world.focus].words=e.target.checked;$('#story-words').hidden=!e.target.checked;save();paint();};
function updatePlayback(){
 const active=intent.kind==='voice';labelButton(read,active?(loading?'Cancel':'Pause'):(mediaError?'Try reading':mark()?.seconds>0?'Keep reading':'Read to me'),active?'pause':'play');read.setAttribute('aria-pressed',active);
 sound.setAttribute('aria-pressed',intent.kind==='effect');
}
async function play(kind,index){
 if(!book)return;halt();mediaError=false;
 const page=book.pages[mark().page];if(kind==='name')selectedAnimal=index;const selected=index??(page.species>=0?page.species:selectedAnimal),token=intent.start(kind),seconds=kind==='voice'?mark().seconds:0;
 // A request owns its element, so a late rejected play() can only stop itself.
 const local=document.createElement('audio');local.id='book-audio';local.preload='none';local.setAttribute('aria-label','Book narration or selected sound');audio.replaceWith(local);audio=local;
 local.src=asset(book.id,`audio/${kind==='voice'?'page-'+mark().page:kind==='name'?'name-'+selected:'effect-'+selected}.wav`);local.volume=.65;
 if(book.kind!=='dinosaurs'&&kind==='effect')local.src=asset(book.id,'audio/effect-0.wav');
 loading=true;status.textContent='Loading sound…';updatePlayback();
 const fail=()=>{if(!intent.accepts(token))return;intent.stop();local.pause();loading=false;mediaError=true;status.textContent='Sound did not start. Tap to try again.';updatePlayback();};
 local.onloadedmetadata=()=>{if(intent.accepts(token)&&seconds>0)local.currentTime=Math.min(seconds,Math.max(0,local.duration-.05));};
 local.onplaying=()=>{if(!intent.accepts(token)){local.pause();return;}loading=false;status.textContent=kind==='voice'?'Reading…':kind==='name'?'Animal name':'Story sound';updatePlayback();};
 local.ontimeupdate=()=>{if(intent.accepts(token)&&kind==='voice'){capture();if(Date.now()-lastSaved>1500){save();lastSaved=Date.now();}}};
 local.onended=()=>{if(!intent.accepts(token))return;intent.stop();loading=false;if(kind==='voice')mark().seconds=0;status.textContent='';save();updatePlayback();if(kind==='voice'&&world.players[world.focus].auto&&mark().page<book.pages.length-1){changePage(1);play('voice');}};
 local.onerror=fail;
 try{await local.play();if(!intent.accepts(token))local.pause();}catch{fail();}
}
function showPage(){
 const page=book.pages[mark().page],group=book.kind==='dinosaurs'&&page.species<0;
 $('#page-title').textContent=page.title;$('#reader-owner').textContent=`Player ${world.focus+1} · ${book.title}`;
 $('#page-count').textContent=`${mark().page+1} / ${book.pages.length}`;$('#story-words').textContent=page.speech;$('#story-words').hidden=!world.players[world.focus].words;
 $('#page-art').setAttribute('aria-label',`${book.title}. Page ${mark().page+1}: ${page.title}. ${page.caption}`);
 previous.disabled=mark().page===0;next.disabled=mark().page===book.pages.length-1;
 const choices=$('#animal-choices');choices.hidden=!group;choices.replaceChildren();
 if(group)book.names.forEach((name,i)=>{const b=document.createElement('button');b.setAttribute('aria-label','Hear '+name);b.innerHTML='<canvas width="300" height="180" aria-hidden="true"></canvas><span></span>';b.querySelector('span').textContent=name;b.onclick=()=>play('name',i);dinosaur(b.querySelector('canvas').getContext('2d'),i,0,0,300,180);choices.append(b);});
 mediaError=false;status.textContent='';updatePlayback();paint();save();
}
function changePage(delta){if(!book)return;halt();turn(world,world.focus,book,delta);showPage();}
async function openBook(id){
 halt();book=catalog.find(b=>b.id===id);world.players[world.focus].last=id;bookmark(world,world.focus,book);art=[];selectedAnimal=0;const generation=++artGeneration;
 library.hidden=true;reader.hidden=false;$('#book-options').open=false;$('#auto-pages').checked=world.players[world.focus].auto;$('#show-words').checked=world.players[world.focus].words;showPage();read.focus();
 try{const loaded=await Promise.all((book.kind==='dinosaurs'?['dinosaurs.png','dinosaurs-extra.png']:['pages.png']).map(name=>image(asset(id,name))));if(generation!==artGeneration)return;art=loaded;showPage();}catch{if(generation===artGeneration)status.textContent='Pictures unavailable. You can still read and turn pages.';}
}
function closeBook(){halt();artGeneration++;reader.hidden=true;library.hidden=false;art=[];book=null;refreshShelf();$('#shelf button[aria-current=true]')?.focus();}
function refreshShelf(){for(const b of document.querySelectorAll('.book-card')){const item=catalog.find(c=>c.id===b.dataset.book),saved=world.players[world.focus].books[item.id];b.setAttribute('aria-current',world.players[world.focus].last===item.id);b.querySelector('small').textContent=saved?`Keep my page · ${saved.page+1} / ${item.pages.length}`:`${item.pages.length} pages`;}}
window.addEventListener('resize',paint);new ResizeObserver(paint).observe($('.reader-bottom'));
document.addEventListener('visibilitychange',()=>{if(document.hidden)halt();});window.addEventListener('pagehide',halt);
document.addEventListener('keydown',e=>{if(reader.hidden||e.target.closest('input,summary,details')||e.altKey||e.ctrlKey||e.metaKey)return;if(e.key==='ArrowRight'||e.key==='ArrowLeft'){e.preventDefault();changePage(e.key==='ArrowRight'?1:-1);}else if(e.key==='Escape')closeBook();});
try{
 catalog=await Promise.all(TITLES.map(async id=>{const response=await fetch(asset(id,'content.json'));if(!response.ok)throw Error('Missing book');return response.json();}));
 try{const raw=localStorage.getItem(KEY);if(raw){const saved=JSON.parse(raw);if(!valid(saved,catalog))throw Error('Invalid saved pages');world=saved;}}catch{blocked=true;saveLabel.textContent='Saved pages could not be loaded. The original record is kept.';saveLabel.classList.add('bad');}
 storageReady=true;
 const covers=await image(new URL('covers.png',ROOT).href);
 for(const [i,item]of catalog.entries()){
  const card=document.createElement('button');card.className='book-card';card.dataset.book=item.id;card.setAttribute('aria-label',item.title);card.innerHTML='<canvas width="320" height="240" aria-hidden="true"></canvas><span class="book-title"></span><small></small>';card.querySelector('.book-title').textContent=item.title;
  fit(card.querySelector('canvas').getContext('2d'),covers,{x:(i%3)/3,y:Math.floor(i/3)/2,width:1/3,height:1/2},0,0,320,240);card.onclick=()=>openBook(item.id);$('#shelf').append(card);
 }
 refreshPlayers();refreshShelf();save();
}catch{saveLabel.textContent='Books could not load. Reload this preview to try again.';saveLabel.classList.add('bad');}
