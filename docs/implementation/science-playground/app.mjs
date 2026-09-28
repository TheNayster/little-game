import {fresh,upgrade,VERSION,clone,initial,ACTIVITIES,state,choose,action,step,startFamily,slimeReady,CIRCUIT_NODES,observe} from './model.mjs';
import {draw,handles} from './draw.mjs';
import {kidFlow,performKidAction} from './kids-flow.mjs';
import {picture} from './pictures.mjs';
import {renderAdvanced} from './advanced.mjs';
import {picButton} from './controls.mjs';

const KEY='little-weeps-science-playground-v1';
document.querySelectorAll('.preview-nav a').forEach((link,i)=>link.insertAdjacentHTML('afterbegin',picture(i?'book':'games')));
let world=fresh(),storageBlocked=false,hadStored=false,dirty=false,lastSave=0;
const saveLabel=document.querySelector('#save');
try{const raw=localStorage.getItem(KEY);hadStored=!!raw;if(raw){const parsed=JSON.parse(raw),next=upgrade(parsed);if(parsed.version<VERSION){const backup=KEY+'-before-v2';if(!localStorage.getItem(backup))localStorage.setItem(backup,raw);dirty=true;}world=next;world.paused=false;}}
catch{storageBlocked=true;saveLabel.textContent='Previous data preserved. This preview is temporary.';saveLabel.classList.add('bad');}
if(!hadStored)world.calm=matchMedia('(prefers-reduced-motion: reduce)').matches;
function save(){if(!dirty||storageBlocked)return;try{localStorage.setItem(KEY,JSON.stringify(world));saveLabel.textContent='Saved on this browser';saveLabel.classList.remove('bad');dirty=false;}catch{saveLabel.textContent='Saving unavailable. Keep this page open.';saveLabel.classList.add('bad');}}
const playerColors=['#6599ad','#d49d70','#a58cc2','#8caa75'];
const panels=[],pointers=new Map(),selectedHandles=Array(4).fill(-1);
let audioContext=null,voices=0;
function sound(hz=480){
 if(!world.sound||voices>=3)return;
 try{audioContext??=new(window.AudioContext||window.webkitAudioContext)();if(audioContext.state==='suspended')audioContext.resume();const o=audioContext.createOscillator(),g=audioContext.createGain();o.type='sine';o.frequency.setValueAtTime(hz,audioContext.currentTime);g.gain.setValueAtTime(.025,audioContext.currentTime);g.gain.exponentialRampToValueAtTime(.0001,audioContext.currentTime+.13);o.connect(g).connect(audioContext.destination);o.start();o.stop(audioContext.currentTime+.15);voices++;o.onended=()=>voices--;}catch{}
}
function stopHint(panel){panel.audio.pause();panel.audio.currentTime=0;panel.listen.setAttribute('aria-pressed','false');panel.listen.innerHTML=picture('listen')+'<span>Listen</span>';}
async function hear(panel){
 if(!panel.audio.paused){stopHint(panel);return;}
 const key=panel.choosing?'choose':kidFlow(world.players[panel.i].activity,state(world,panel.i)).hint;
 panel.audio.src=new URL(`./hints/${key}.wav`,import.meta.url).href;
 try{await panel.audio.play();panel.listen.setAttribute('aria-pressed','true');panel.listen.innerHTML=picture('stop')+'<span>Stop</span>';}
 catch{panel.listen.innerHTML=picture('listen')+'<span>Try sound</span>';panel.listen.setAttribute('aria-pressed','false');}
}
for(let i=0;i<4;i++){
 const player=document.createElement('button');player.className='player';player.style.setProperty('--player',playerColors[i]);player.innerHTML=`<span class="player-dot">${i+1}</span><span>Player ${i+1}</span>`;player.setAttribute('aria-label',`Player ${i+1}`);
 player.onclick=()=>{panels.forEach(p=>cancelPanel(p.i));world.focus=i;world.quad=false;dirty=true;layout();save();};document.querySelector('#players').append(player);
 const section=document.createElement('section');section.className='workspace';section.dataset.player=i;section.style.setProperty('--player',playerColors[i]);section.setAttribute('aria-label',`Player ${i+1} workspace`);
 section.innerHTML=`<div class="work-head"><div class="heading"><span class="owner">Player ${i+1}</span><h2></h2></div><div class="head-actions"></div></div><div class="picker" hidden><p>Pick a picture</p><div class="picture-grid"></div></div><div class="experiment"><div class="play-body"><div class="canvas-wrap"><canvas width="1000" height="600" tabindex="0" role="application"></canvas></div><div class="kids-tools" aria-label="Play tools"></div></div></div><div class="hint-bar"><div class="hint"></div></div><div class="extra-bar"><details class="extra"><summary>${picture('toolbox')}<span>More to try</span></summary><div class="extra-tools"></div></details><button class="undo" hidden>${picture('undo')}<span>Undo restart</span></button></div><audio preload="none" aria-label="Player ${i+1} spoken hint"></audio>`;
 const canvas=section.querySelector('canvas');
 const panel={i,section,canvas,ctx:canvas.getContext('2d'),title:section.querySelector('h2'),hint:section.querySelector('.hint'),tools:section.querySelector('.kids-tools'),picker:section.querySelector('.picker'),experiment:section.querySelector('.experiment'),extra:section.querySelector('.extra'),extraTools:section.querySelector('.extra-tools'),undoButton:section.querySelector('.undo'),audio:section.querySelector('audio'),id:null,key:null,flow:null,choosing:!hadStored,undo:null};
 panel.games=picButton('Games','games',()=>{cancelPanel(i);stopHint(panel);panel.choosing=!panel.choosing;panel.extra.open=false;build(panel);},'small-picture');panel.games.setAttribute('aria-label',`Choose experiment for player ${i+1}`);
 const reset=picButton('Again','restart',()=>restart(panel),'small-picture reset');reset.setAttribute('aria-label',`Restart player ${i+1} experiment`);section.querySelector('.head-actions').append(panel.games,reset);
 panel.listen=picButton('Listen','listen',()=>hear(panel),'listen small-picture');panel.listen.setAttribute('aria-label',`Hear hint for player ${i+1}`);panel.listen.setAttribute('aria-pressed','false');section.querySelector('.hint-bar').prepend(panel.listen);
 panel.audio.onended=()=>stopHint(panel);panel.audio.onerror=()=>panel.listen.setAttribute('aria-pressed','false');
 panel.undoButton.onclick=()=>{if(!panel.undo)return;cancelPanel(i);stopHint(panel);world.players[i].states[panel.undo.id]=clone(panel.undo.value);panel.undo=null;dirty=true;build(panel);save();};
 panel.extra.ontoggle=()=>{if(panel.extra.open)refreshAdvanced(panel);};
 for(const a of ACTIVITIES){
  const card=document.createElement('button');card.className='picture-card';card.dataset.activity=a.id;card.setAttribute('aria-label',a.title);card.innerHTML=`<canvas width="260" height="156" aria-hidden="true"></canvas><span>${a.title}</span>`;
  const c=card.querySelector('canvas').getContext('2d');c.scale(.26,.26);const preview=initial(a.id);
  if(a.id==='slime'){preview.activator=1;preview.mixed=1;preview.stretch=.4;}
  if(a.id==='lava')preview.blobs=[{x:480,y:230,r:22,up:true},{x:560,y:330,r:17,up:false}];
  if(a.id==='foam')preview.foam=3;if(a.id==='light'){preview.angle=0;preview.prismY=250;}if(a.id==='bubbles'){preview.water=4;preview.soap=1;preview.mixed=1;preview.bubbles=[{x:580,y:170,r:60},{x:720,y:120,r:38}];}if(a.id==='colors')preview.parts=[2,2,0];draw(c,a.id,preview,{calm:true});
  card.onclick=()=>{cancelPanel(i);stopHint(panel);choose(world,i,a.id);panel.choosing=false;panel.undo=null;panel.extra.open=false;dirty=true;build(panel);save();panel.canvas.focus({preventScroll:true});};panel.picker.querySelector('.picture-grid').append(card);
 }
 canvas.addEventListener('pointerdown',e=>pointerDown(panel,e));canvas.addEventListener('pointermove',e=>pointerMove(panel,e));canvas.addEventListener('pointerup',e=>pointerEnd(panel,e));canvas.addEventListener('pointercancel',e=>pointerEnd(panel,e,true));canvas.addEventListener('lostpointercapture',e=>pointers.delete(e.pointerId));
 canvas.addEventListener('keydown',e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();runKid(panel,panel.flow?.primary);}if(e.key==='Escape'){cancelPanel(i);panel.choosing=true;build(panel);}});
 panels.push(panel);document.querySelector('#workspaces').append(section);
}
function restart(panel){cancelPanel(panel.i);stopHint(panel);panel.undo={id:world.players[panel.i].activity,value:clone(state(world,panel.i))};action(world,panel.i,'reset');dirty=true;build(panel);save();}
function dispatch(i,kind,value,{rebuild=false,tone=false}={}){
 const panel=panels[i];if(kind==='reset'){restart(panel);return true;}
 if(kind==='paper')panel.undo={id:'robot',value:clone(state(world,i))};else panel.undo=null;
 const ok=action(world,i,kind,value);dirty=true;if(tone)sound(ok?480:250);update(panel);if(rebuild)refreshAdvanced(panel);return ok;
}
function runKid(panel,descriptor){
 if(!descriptor||panel.choosing)return;stopHint(panel);if(descriptor.commands[0][0]==='reset'){restart(panel);return;}
 performKidAction(world,panel.i,descriptor,(w,i,k,v)=>dispatch(i,k,v));sound(450+panel.i*55);update(panel);if(panel.extra.open)refreshAdvanced(panel);save();
}
function refreshAdvanced(panel){renderAdvanced(world,panel.i,panel.extraTools,dispatch,()=>{startFamily(world);dirty=true;panels.forEach(update);save();});}
function build(panel){
 const id=world.players[panel.i].activity,a=ACTIVITIES.find(a=>a.id===id);panel.id=id;panel.key=null;panel.title.textContent=panel.choosing?'Choose & play':a.title;panel.picker.hidden=!panel.choosing;panel.experiment.hidden=panel.choosing;panel.extra.hidden=panel.choosing;panel.section.querySelector('.reset').hidden=panel.choosing;panel.games.setAttribute('aria-pressed',panel.choosing);
 panel.canvas.setAttribute('aria-label',`${a.title}. Tap the highlighted object or use the picture tools.`);panel.picker.querySelectorAll('.picture-card').forEach(c=>c.setAttribute('aria-pressed',c.dataset.activity===id));if(panel.extra.open)refreshAdvanced(panel);update(panel);
}
function update(panel){
 const flow=kidFlow(world.players[panel.i].activity,state(world,panel.i));panel.flow=flow;panel.undoButton.hidden=!panel.undo||panel.choosing;
 const description=`${ACTIVITIES.find(a=>a.id===world.players[panel.i].activity).title}. ${observe(world.players[panel.i].activity,state(world,panel.i))} Use the picture tools or tap the objects.`;if(panel.canvas.getAttribute('aria-label')!==description)panel.canvas.setAttribute('aria-label',description);
 panel.hint.textContent=panel.choosing?'Pick a picture. Your play stays here.':selectedHandles[panel.i]>=0?'Tap where this ramp end should go.':flow.text;if(panel.choosing)return;
 const key=JSON.stringify([flow.hint,flow.primary&&[flow.primary.id,flow.primary.label],flow.extras.map(b=>[b.id,b.label])]);if(panel.key===key)return;panel.key=key;
 const focusId=panel.tools.contains(document.activeElement)?document.activeElement.dataset.kidAction:null;panel.tools.replaceChildren();
 for(const[n,descriptor]of [flow.primary,...flow.extras].entries()){
  if(!descriptor)continue;const b=picButton(descriptor.label,descriptor.icon,()=>{const current=kidFlow(world.players[panel.i].activity,state(world,panel.i));runKid(panel,[current.primary,...current.extras].find(d=>d?.id===descriptor.id));},`picture-tool${n===0&&!flow.equalChoices?' primary':''}`);b.dataset.kidAction=descriptor.id;b.setAttribute('aria-label',descriptor.label);if(n===0&&!flow.equalChoices)b.setAttribute('aria-description','Try this next');panel.tools.append(b);
 }
 if(!flow.primary&&!flow.extras.length){const watch=document.createElement('div');watch.className='watch';watch.innerHTML=picture('play')+'<span>Watch!</span>';panel.tools.append(watch);}
 if(focusId){const next=[...panel.tools.children].find(b=>b.dataset.kidAction===focusId)||panel.tools.querySelector('button');next?.focus({preventScroll:true});}
}
function layout(){document.body.classList.toggle('quad',world.quad);document.body.classList.toggle('gentle',world.calm);document.querySelector('#quad').setAttribute('aria-pressed',world.quad);document.querySelector('#quad').textContent=world.quad?'Focus one':'Four together';document.querySelectorAll('#players button').forEach((b,i)=>b.setAttribute('aria-pressed',i===world.focus));for(const p of panels){p.section.hidden=!world.quad&&p.i!==world.focus;if(p.section.hidden)stopHint(p);if(p.id!==world.players[p.i].activity)build(p);}}
function pos(panel,e){const r=panel.canvas.getBoundingClientRect();return{x:(e.clientX-r.left)/r.width*1000,y:(e.clientY-r.top)/r.height*600};}
function nearest(list,p,r=40){let best=-1,d=r;list.forEach((q,i)=>{const n=Math.hypot(p.x-q[0],p.y-q[1]);if(n<d){d=n;best=i;}});return best;}
function inside(p,target){return target&&Math.hypot(p.x-target[0],p.y-target[1])<=target[2];}
function cancelPanel(i){selectedHandles[i]=-1;for(const[id,g]of pointers)if(g.i===i){pointers.delete(id);try{panels[i].canvas.releasePointerCapture(id);}catch{}}}
function pointerDown(panel,e){
 if(e.button!==0||panel.choosing)return;e.preventDefault();if([...pointers.values()].some(g=>g.i===panel.i))return;stopHint(panel);
 const p=pos(panel,e),s=state(world,panel.i),id=world.players[panel.i].activity,flow=kidFlow(id,s),g={i:panel.i,id,x:p.x,y:p.y,time:e.timeStamp,downX:p.x,downY:p.y,handle:-1,moved:false,lastDrop:e.timeStamp,handled:false,primary:inside(p,flow.primary?.target)};
 if(id==='marble'&&!g.primary)g.handle=nearest(handles(id,s),p,45);if(id==='circuits')g.handle=nearest(CIRCUIT_NODES,p,43);pointers.set(e.pointerId,g);panel.canvas.setPointerCapture(e.pointerId);
 if(id==='ice'&&(s.freed||p.x>280&&p.x<720&&p.y>150&&p.y<430)){dispatch(panel.i,s.freed?'move':s.tool==='hammer'?'chip':'drop',p,{tone:s.tool==='hammer'});g.handled=true;}
 if(id==='ice'&&!s.freed&&!g.primary){if(Math.hypot(p.x-155,p.y-265)<80){dispatch(panel.i,'iceTool','hammer');g.handled=true;}else if(Math.hypot(p.x-850,p.y-265)<75){dispatch(panel.i,'iceTool','water');g.handled=true;}}
 if(id==='colors'){const n=nearest([[165,220],[500,90],[835,220]],p,85);if(n>=0){dispatch(panel.i,'pourColor',n,{tone:true});g.handled=true;}}
 if(id==='milk'&&Math.hypot((p.x-500)/355,(p.y-295)/200)<=1){if(!panel.extra.open)dispatch(panel.i,'tool',s.drops.length?'soap':'color');dispatch(panel.i,'touch',p);g.handled=true;}
 if(id==='bubbles'&&s.bubbles.some(b=>Math.hypot(b.x-p.x,b.y-p.y)<b.r+12)){dispatch(panel.i,'pop',p);g.handled=true;}
 if(id==='chain'&&!g.primary){const n=Math.round((p.x-192)/124);if(p.y>190&&p.y<440&&n>=0&&n<6){dispatch(panel.i,'place',n);g.handled=true;}}
 if(id==='light'&&Math.hypot(p.x-s.prismX,p.y-s.prismY)<85)g.handle=0;if(id==='slime'&&slimeReady(s)&&!s.torn)dispatch(panel.i,'pull',{...p,speed:0});
}
function pointerMove(panel,e){
 const g=pointers.get(e.pointerId);if(!g||g.i!==panel.i)return;const p=pos(panel,e),dt=Math.max(.008,(e.timeStamp-g.time)/1000),speed=Math.hypot(p.x-g.x,p.y-g.y)/dt;if(Math.hypot(p.x-g.downX,p.y-g.downY)>10)g.moved=true;const s=state(world,panel.i);
 if(g.id==='marble'&&g.handle>=0)dispatch(panel.i,'handle',{...p,index:g.handle});
 if(g.id==='ice'&&(s.freed||p.x>280&&p.x<720&&p.y>150&&p.y<430)&&e.timeStamp-g.lastDrop>(s.tool==='hammer'?180:80)){dispatch(panel.i,s.freed?'move':s.tool==='hammer'?'chip':'drop',p,{tone:s.tool==='hammer'});g.lastDrop=e.timeStamp;}
 if(g.id==='slime')dispatch(panel.i,'pull',{...p,speed});if(g.id==='light'&&g.handle===0)dispatch(panel.i,'prism',p);g.x=p.x;g.y=p.y;g.time=e.timeStamp;
}
function pointerEnd(panel,e,cancelled=false){
 const g=pointers.get(e.pointerId);if(!g)return;const p=pos(panel,e);pointers.delete(e.pointerId);
 if(!cancelled){
  if(g.id==='circuits'&&g.handle>=0){const n=nearest(CIRCUIT_NODES,p,43);if(g.moved&&n>=0)dispatch(panel.i,'wire',[g.handle,n]);else if(!g.moved)dispatch(panel.i,'terminal',g.handle);g.handled=true;}
  if(g.id==='marble'&&!g.moved&&!g.primary){if(g.handle>=0)selectedHandles[panel.i]=g.handle;else if(selectedHandles[panel.i]>=0){dispatch(panel.i,'handle',{...p,index:selectedHandles[panel.i]});selectedHandles[panel.i]=-1;}g.handled=true;}
  if(!g.moved&&!g.handled&&g.primary&&inside(p,panel.flow.primary?.target))runKid(panel,panel.flow.primary);
 }
 try{panel.canvas.releasePointerCapture(e.pointerId);}catch{}update(panel);save();
}
function drawCue(panel){if(panel.choosing)return;const targets=panel.flow?.equalChoices?[panel.flow.primary,...panel.flow.extras].map(b=>b.target):[panel.flow?.primary?.target];const c=panel.ctx;for(const target of targets){if(!target)continue;c.save();c.beginPath();c.arc(target[0],target[1],target[2],0,Math.PI*2);c.lineWidth=7;c.strokeStyle='#fff6ce';c.stroke();c.lineWidth=3;c.strokeStyle='#27827e';c.setLineDash([10,8]);c.stroke();c.restore();}}
document.querySelector('#quad').onclick=()=>{panels.forEach(p=>cancelPanel(p.i));world.quad=!world.quad;dirty=true;layout();save();};
document.querySelector('#pause').onclick=()=>{world.paused=!world.paused;document.querySelector('#pause').textContent=world.paused?'Resume':'Pause';document.querySelector('#pause').setAttribute('aria-pressed',world.paused);dirty=true;save();};
document.querySelector('#sound').checked=world.sound;document.querySelector('#sound').onchange=e=>{world.sound=e.target.checked;dirty=true;sound();save();};
document.querySelector('#calm').checked=world.calm;document.querySelector('#calm').onchange=e=>{world.calm=e.target.checked;dirty=true;layout();save();};
document.addEventListener('visibilitychange',()=>{if(document.hidden){panels.forEach(p=>{cancelPanel(p.i);stopHint(p);});save();}last=performance.now();});window.addEventListener('pagehide',save);
let last=performance.now(),acc=0,lastObservation=0;
function frame(now){const elapsed=Math.min(.15,(now-last)/1000);last=now;if(!document.hidden){acc+=elapsed;while(acc>=1/60){step(world,1/60);acc-=1/60;}if(!world.paused)dirty=true;if(now-lastObservation>160){panels.forEach(update);lastObservation=now;}for(const panel of panels)if(!panel.section.hidden&&!panel.choosing){draw(panel.ctx,world.players[panel.i].activity,state(world,panel.i),world);drawCue(panel);}if(now-lastSave>1800){save();lastSave=now;}}requestAnimationFrame(frame);}
panels.forEach(build);layout();if(!storageBlocked)saveLabel.textContent='Your play stays in this browser';requestAnimationFrame(frame);
