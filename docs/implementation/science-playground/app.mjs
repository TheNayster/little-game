// A separate local review playground; it never reads or writes game saves.
import {fresh,valid,ACTIVITIES,COLORS,state,choose,action,step,startFamily,observe,slimeReady,CIRCUIT_NODES} from './model.mjs';
import {draw,handles} from './draw.mjs';
const KEY='little-weeps-science-playground-v1';
let world=fresh(),storageBlocked=false,hadStored=false,dirty=false,lastSave=0;
const saveLabel=document.querySelector('#save');
try{const raw=localStorage.getItem(KEY);hadStored=!!raw;if(raw){const parsed=JSON.parse(raw);if(!valid(parsed))throw Error('Saved prototype format was not recognized.');world=parsed;world.paused=false;}}catch(e){storageBlocked=true;saveLabel.textContent='Previous browser data was preserved. This preview is temporary.';saveLabel.classList.add('bad');}
if(!hadStored)world.calm=matchMedia('(prefers-reduced-motion: reduce)').matches;
function save(){if(!dirty||storageBlocked)return;try{localStorage.setItem(KEY,JSON.stringify(world));saveLabel.textContent='Your four workspaces are saved in this browser';saveLabel.classList.remove('bad');dirty=false;}catch{saveLabel.textContent='Browser storage is unavailable. Keep this preview open to retain your work.';saveLabel.classList.add('bad');}}
const playerColors=['#6599ad','#d49d70','#a58cc2','#8caa75'];
let audio=null,voices=0;
function sound(hz=480){if(!world.sound||voices>=3)return;try{audio??=new (window.AudioContext||window.webkitAudioContext)();if(audio.state==='suspended')audio.resume();const o=audio.createOscillator(),g=audio.createGain();o.type='sine';o.frequency.setValueAtTime(hz,audio.currentTime);o.frequency.exponentialRampToValueAtTime(hz*.7,audio.currentTime+.12);g.gain.setValueAtTime(.035,audio.currentTime);g.gain.exponentialRampToValueAtTime(.0001,audio.currentTime+.16);o.connect(g).connect(audio.destination);o.start();o.stop(audio.currentTime+.17);voices++;o.onended=()=>voices--;}catch{/* Audio is optional; visual play remains usable. */}}
const panels=[];const pointers=new Map();const selectedHandles=Array(4).fill(-1);
for(let i=0;i<4;i++){
 const button=document.createElement('button');button.innerHTML=`<span class="dot" style="--player:${playerColors[i]}"></span>Player ${i+1}`;button.onclick=()=>{world.focus=i;world.quad=false;dirty=true;layout();};document.querySelector('#players').append(button);
 const section=document.createElement('section');section.className='workspace';section.dataset.player=i;section.style.setProperty('--player',playerColors[i]);section.setAttribute('aria-label',`Player ${i+1} workspace`);
 section.innerHTML=`<div class="work-head"><div><span class="owner">Player ${i+1} · Own workspace</span><h2></h2></div><div class="head-actions"><select aria-label="Player ${i+1} experiment">${ACTIVITIES.map(a=>`<option value="${a.id}">${a.title}</option>`).join('')}</select><button class="reset" aria-label="Reset player ${i+1} experiment">↺ Start fresh</button></div></div><p class="hint"></p><div class="canvas-wrap"><canvas width="1000" height="600" tabindex="0" role="application" aria-label="Interactive experiment"></canvas></div><div class="tools"></div><div class="observation" role="status" aria-live="polite"></div><p class="invitation"></p>`;
 const canvas=section.querySelector('canvas');const panel={i,section,canvas,ctx:canvas.getContext('2d'),title:section.querySelector('h2'),hint:section.querySelector('.hint'),tools:section.querySelector('.tools'),observation:section.querySelector('.observation'),invitation:section.querySelector('.invitation'),select:section.querySelector('select'),id:null};
 section.querySelector('.reset').onclick=()=>{cancelPanel(i);dispatch(i,'reset');build(panel);};
 panel.select.onchange=e=>{choose(world,i,e.target.value);dirty=true;cancelPanel(i);build(panel);layout();};
 canvas.addEventListener('pointerdown',e=>pointerDown(panel,e));canvas.addEventListener('pointermove',e=>pointerMove(panel,e));canvas.addEventListener('pointerup',e=>pointerEnd(panel,e));canvas.addEventListener('pointercancel',e=>pointerEnd(panel,e,true));canvas.addEventListener('lostpointercapture',e=>pointers.delete(e.pointerId));
 canvas.addEventListener('keydown',e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();primary(i);}if(e.key==='Escape')cancelPanel(i);});
 panels.push(panel);document.querySelector('#workspaces').append(section);
}
for(const a of ACTIVITIES){const b=document.createElement('button');b.dataset.activity=a.id;b.innerHTML=`<span class="activity-icon" aria-hidden="true">${a.icon}</span><span>${a.title}</span>`;b.onclick=()=>{cancelPanel(world.focus);choose(world,world.focus,a.id);dirty=true;build(panels[world.focus]);layout();};document.querySelector('#activities').append(b);}
function dispatch(i,k,v,{tone=true,rebuild=false}={}){const ok=action(world,i,k,v);dirty=true;if(tone)sound(ok?420+i*70:210);if(rebuild)build(panels[i]);update(panels[i]);return ok;}
function btn(panel,label,kind,value,{primary=false,selected=false,rebuild=true}={}){const b=document.createElement('button');b.textContent=label;b.dataset.action=kind;b.className=(primary?'primary ':'')+(selected?'selected':'');b.onclick=()=>dispatch(panel.i,kind,value,{rebuild});panel.tools.append(b);return b;}
function range(panel,label,kind,value,min,max,step=.1){const l=document.createElement('label');l.className='range';l.innerHTML=`<span>${label}</span><output>${Number(value).toFixed(1)}</output><input aria-label="${label}" type="range" min="${min}" max="${max}" step="${step}" value="${value}">`;const input=l.querySelector('input');input.oninput=()=>{l.querySelector('output').value=Number(input.value).toFixed(1);dispatch(panel.i,kind,Number(input.value),{tone:false});};panel.tools.append(l);}
function colors(panel,s){const box=document.createElement('div');box.className='colors';box.setAttribute('aria-label','Choose a color');COLORS.forEach((color,j)=>{const b=document.createElement('button');b.className='color'+(s.color===j?' selected':'');b.style.setProperty('--swatch',color);b.setAttribute('aria-label',['Rose','Blue','Purple','Gold','Mint'][j]);b.setAttribute('aria-pressed',s.color===j);b.textContent=s.color===j?'✓':'';b.onclick=()=>dispatch(panel.i,'color',j,{rebuild:true});box.append(b);});panel.tools.append(box);}
function build(panel){
 const p=world.players[panel.i],id=p.activity,s=state(world,p.id),a=ACTIVITIES.find(a=>a.id===id);panel.id=id;panel.title.textContent=a.title;panel.hint.textContent=a.hint;panel.invitation.textContent='Try this: '+a.invitation;panel.select.value=id;panel.canvas.setAttribute('aria-label',`${a.title}. ${a.hint} Core actions are also available in the buttons below.`);panel.tools.replaceChildren();
 switch(id){
 case 'lava':btn(panel,'＋ Fizz tablet','tablet',null,{primary:true});btn(panel,s.heat?'❄ Cool water':'☀ Warm water','heat',s.heat?0:1);colors(panel,s);break;
 case 'ice':btn(panel,'Warm dropper','heat',1,{selected:!!s.heat});btn(panel,'Cool dropper','heat',0,{selected:!s.heat});btn(panel,'Pour over ice','pour',null,{primary:true});btn(panel,'Change dinosaur','toy');break;
 case 'marble':btn(panel,'● Release ball','release',null,{primary:true});btn(panel,s.rough?'Felt surface':'Smooth surface','rough');btn(panel,'Change first slope','slope');break;
 case 'slime':btn(panel,'＋ Activator','activator');btn(panel,'↻ Stir','stir',null,{primary:!slimeReady(s)});btn(panel,'Slow stretch','slow',null,{primary:slimeReady(s)});btn(panel,'Quick pull','quick');btn(panel,'Squish together','squish');colors(panel,s);break;
 case 'milk':btn(panel,'Color dropper','tool','color',{selected:s.tool==='color'});btn(panel,'Soap wand','tool','soap',{selected:s.tool==='soap'});btn(panel,'Touch the center','touch',{x:500,y:295},{primary:true});colors(panel,s);break;
 case 'robot':btn(panel,s.running?'■ Stop motor':'▶ Start motor','run',null,{primary:true});range(panel,'Motor speed','speed',s.speed,.3,2.5);range(panel,'Off-center weight','balance',s.balance,.2,2);colors(panel,s);btn(panel,'Fresh paper','paper');break;
 case 'rocket':btn(panel,'＋ Pump air','pump',null,{primary:s.phase==='idle'});btn(panel,'➜ Release','release',null,{primary:s.phase==='idle'&&s.air>0});btn(panel,'Return rocket','return');btn(panel,s.mass===1?'Add a weight':'Remove weight','mass');break;
 case 'foam':btn(panel,'＋ Peroxide','peroxide');btn(panel,'＋ Soap','soap');btn(panel,'＋ Yeast mixture','yeast',null,{primary:true});btn(panel,s.wide?'Wide vessel':'Narrow vessel','wide');colors(panel,s);break;
 case 'wind':range(panel,'Fan strength','fan',s.fan,0,3);btn(panel,s.area===1?'Larger canopy':'Smaller canopy','area');btn(panel,'＋ Weight','weight');btn(panel,'− Weight','unweight');break;
 case 'circuits':btn(panel,'Connect ready loop','loop',null,{primary:true});btn(panel,s.closed?'Open switch':'Close switch','switch');['lamp','fan','buzzer'].forEach(l=>btn(panel,l[0].toUpperCase()+l.slice(1),'load',l,{selected:s.load===l}));break;
 case 'weather':range(panel,'Sun warmth','sun',s.sun,0,2);range(panel,'Cool the air','cool',s.cool,0,2);range(panel,'Wind','wind',s.wind,-2,2);break;
 case 'bubbles':btn(panel,'Dip wand','dip');btn(panel,'Blow bubbles','blow',null,{primary:true});btn(panel,s.shape==='round'?'Round wand':'Square wand','shape');range(panel,'Blowing strength','air',s.air,.5,2);range(panel,'Wind','wind',s.wind,-2,2);break;
 case 'light':btn(panel,s.on?'Light off':'Light on','light');range(panel,'Mirror angle','angle',s.angle,-35,65,1);btn(panel,'Put prism in beam','align',null,{primary:true});break;
 case 'chain':['domino','ramp','bell','gap'].forEach(t=>btn(panel,t[0].toUpperCase()+t.slice(1),'piece',t,{selected:s.piece===t}));btn(panel,'▶ Nudge my section','run',null,{primary:true});btn(panel,s.joined?'Leave family chain':'Join family chain','join');const b=document.createElement('button');b.textContent='▶ Run joined sections';b.onclick=()=>{startFamily(world);dirty=true;panels.forEach(update);sound(660);};panel.tools.append(b);break;
 }
 update(panel);
}
function update(panel){const id=world.players[panel.i].activity,s=state(world,panel.i);const msg=observe(id,s);if(panel.observation.textContent!==msg)panel.observation.textContent=msg;}
function layout(){document.body.classList.toggle('quad',world.quad);document.querySelector('#quad').setAttribute('aria-pressed',world.quad);document.querySelector('#quad').textContent=world.quad?'Focus one':'Four together';document.querySelectorAll('#players button').forEach((b,i)=>b.setAttribute('aria-pressed',i===world.focus));document.querySelectorAll('#activities button').forEach(b=>b.setAttribute('aria-pressed',b.dataset.activity===world.players[world.focus].activity));for(const panel of panels){panel.section.hidden=!world.quad&&panel.i!==world.focus;if(panel.id!==world.players[panel.i].activity)build(panel);}}
function pos(panel,e){const r=panel.canvas.getBoundingClientRect();return {x:(e.clientX-r.left)/r.width*1000,y:(e.clientY-r.top)/r.height*600};}
function nearest(list,p,r=40){let best=-1,d=r;list.forEach((q,i)=>{const n=Math.hypot(p.x-q[0],p.y-q[1]);if(n<d){d=n;best=i;}});return best;}
function cancelPanel(i){selectedHandles[i]=-1;for(const [id,g]of pointers)if(g.i===i){pointers.delete(id);try{panels[i].canvas.releasePointerCapture(id);}catch{}}}
function pointerDown(panel,e){
 if(e.button!==0)return;e.preventDefault();if([...pointers.values()].some(g=>g.i===panel.i))return;
 const p=pos(panel,e),s=state(world,panel.i),id=world.players[panel.i].activity,g={i:panel.i,id,x:p.x,y:p.y,time:e.timeStamp,downX:p.x,downY:p.y,handle:-1,moved:false,lastDrop:0};
 if(id==='marble')g.handle=nearest(handles(id,s),p,45);
 if(id==='circuits')g.handle=nearest(CIRCUIT_NODES,p,43);
 pointers.set(e.pointerId,g);panel.canvas.setPointerCapture(e.pointerId);
 if(id==='ice')dispatch(panel.i,s.freed?'move':'drop',p,{tone:false});
 if(id==='milk')dispatch(panel.i,'touch',p,{tone:false});
 if(id==='bubbles')dispatch(panel.i,'pop',p);
 if(id==='chain'){const n=Math.round((p.x-192)/124);if(p.y>190&&p.y<440&&n>=0&&n<6)dispatch(panel.i,'place',n);}
 if(id==='light'&&Math.hypot(p.x-s.prismX,p.y-s.prismY)<100)g.handle=0;
 if(id==='slime'&&slimeReady(s))dispatch(panel.i,'pull',{...p,speed:0},{tone:false});
}
function pointerMove(panel,e){
 const g=pointers.get(e.pointerId);if(!g||g.i!==panel.i)return;const p=pos(panel,e),dt=Math.max(.008,(e.timeStamp-g.time)/1000),speed=Math.hypot(p.x-g.x,p.y-g.y)/dt;
 if(Math.hypot(p.x-g.downX,p.y-g.downY)>10)g.moved=true;
 const s=state(world,panel.i);
 if(g.id==='marble'&&g.handle>=0)dispatch(panel.i,'handle',{...p,index:g.handle},{tone:false});
 if(g.id==='ice'&&(s.freed||e.timeStamp-g.lastDrop>80)){dispatch(panel.i,s.freed?'move':'drop',p,{tone:false});g.lastDrop=e.timeStamp;}
 if(g.id==='slime')dispatch(panel.i,'pull',{...p,speed},{tone:false});
 if(g.id==='light'&&g.handle===0)dispatch(panel.i,'prism',p,{tone:false});
 g.x=p.x;g.y=p.y;g.time=e.timeStamp;
}
function pointerEnd(panel,e,cancelled=false){const g=pointers.get(e.pointerId);if(!g)return;const p=pos(panel,e);pointers.delete(e.pointerId);if(!cancelled){
 if(g.id==='circuits'){const n=nearest(CIRCUIT_NODES,p,43);if(g.moved&&g.handle>=0&&n>=0)dispatch(panel.i,'wire',[g.handle,n]);else if(g.handle>=0)dispatch(panel.i,'terminal',g.handle);}
 if(g.id==='marble'&&!g.moved){if(g.handle>=0){selectedHandles[panel.i]=g.handle;panel.observation.textContent='Ramp handle selected. Tap its new position.';}else if(selectedHandles[panel.i]>=0){dispatch(panel.i,'handle',{...p,index:selectedHandles[panel.i]});selectedHandles[panel.i]=-1;}else dispatch(panel.i,'release');}
 if(g.id==='lava'&&!g.moved)dispatch(panel.i,'tablet');
 }try{panel.canvas.releasePointerCapture(e.pointerId);}catch{}save();}
function primary(i){const id=world.players[i].activity,s=state(world,i);const a={lava:['tablet'],ice:['pour'],marble:['release'],slime:[slimeReady(s)?'slow':s.activator?'stir':'activator'],milk:['touch',{x:500,y:295}],robot:['run'],rocket:[s.phase==='idle'&&s.air?'release':'pump'],foam:['yeast'],wind:['fan',s.fan?0:2],circuits:['switch'],weather:['cool',s.cool?0:1.5],bubbles:['blow'],light:['align'],chain:['run']}[id];dispatch(i,a[0],a[1],{rebuild:true});}
document.querySelector('#quad').onclick=()=>{world.quad=!world.quad;dirty=true;layout();};
document.querySelector('#pause').onclick=()=>{world.paused=!world.paused;document.querySelector('#pause').textContent=world.paused?'Resume':'Pause';document.querySelector('#pause').setAttribute('aria-pressed',world.paused);dirty=true;save();};
document.querySelector('#sound').checked=world.sound;document.querySelector('#sound').onchange=e=>{world.sound=e.target.checked;dirty=true;sound();save();};
document.querySelector('#calm').checked=world.calm;document.querySelector('#calm').onchange=e=>{world.calm=e.target.checked;dirty=true;save();};
document.addEventListener('visibilitychange',()=>{if(document.hidden){for(let i=0;i<4;i++)cancelPanel(i);save();}last=performance.now();});window.addEventListener('pagehide',save);
let last=performance.now(),acc=0,lastObservation=0;
function frame(now){const elapsed=Math.min(.15,(now-last)/1000);last=now;if(!document.hidden){acc+=elapsed;while(acc>=1/60){step(world,1/60);acc-=1/60;}if(!world.paused)dirty=true;
 for(const panel of panels)if(!panel.section.hidden)draw(panel.ctx,world.players[panel.i].activity,state(world,panel.i),world);
 if(now-lastObservation>350){panels.forEach(update);lastObservation=now;}if(now-lastSave>1800){save();lastSave=now;}}
 requestAnimationFrame(frame);
}
panels.forEach(build);layout();if(!storageBlocked)saveLabel.textContent='Your experiments stay in this browser';requestAnimationFrame(frame);
