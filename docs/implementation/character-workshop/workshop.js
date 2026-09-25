(async function () {
  'use strict';
  const box=document.getElementById('character');
  try {const response=await fetch('blue-pup-rig.svg');if(!response.ok)throw Error('Art unavailable');box.innerHTML=await response.text();}
  catch {document.getElementById('load-error').hidden=false;return;}
  const part=id=>box.querySelector('#'+id);
  const reduced=matchMedia('(prefers-reduced-motion: reduce)').matches;
  let motion='walk',facing='right',pace=1,paused=reduced,time=0,last=null;
  const pause=document.getElementById('pause');
  const refreshPause=()=>{pause.textContent=paused?'Resume motion':'Pause motion';pause.setAttribute('aria-pressed',String(paused));};refreshPause();
  function paint(){
    const moving=motion==='walk'||motion==='carry',cycle=time*2*Math.PI*1.35*pace;
    const swing=moving?Math.sin(cycle)*19:0,bob=moving?-Math.abs(Math.sin(cycle))*3:Math.sin(time*2)*1.2;
    part('facing-root').setAttribute('transform',facing==='left'?'translate(440 0) scale(-1 1)':'');
    part('foot-far').setAttribute('transform',`rotate(${swing} 183 395)`);
    part('foot-near').setAttribute('transform',`rotate(${-swing} 249 395)`);
    part('body').setAttribute('transform',`translate(0 ${bob})`);
    part('head').setAttribute('transform',`translate(0 ${bob}) rotate(${motion==='wave'?Math.sin(time*2)*2:Math.sin(time*1.4)*.6} 220 252)`);
    part('tail').setAttribute('transform',`rotate(${Math.sin(time*(motion==='wave'?9:4))*5} 280 364)`);
    part('arm-far').setAttribute('transform',`translate(0 ${bob}) rotate(${-swing*.65} 170 278)`);
    const angle=motion==='wave'?-132+Math.sin(time*7)*14:motion==='carry'?-12:swing*.7;
    part('arm-near').setAttribute('transform',`translate(0 ${bob}) rotate(${angle} 272 280)`);
    // The bucket is a child of the hand anchor. Counter-rotation keeps its
    // opening level while the arm moves; its pivot follows the held handle.
    part('held-prop').setAttribute('transform',`rotate(${-angle} 0 0)`);
    part('held-prop').style.display=motion==='carry'?'inline':'none';
    const blink=time%4.3>4.14;part('eyes-open').style.display=blink?'none':'';part('eyes-closed').style.display=blink?'inline':'none';
    const guides=part('rig-guides'),radians=angle*Math.PI/180;
    const hand=[272+27*Math.cos(radians)-55*Math.sin(radians),280+bob+27*Math.sin(radians)+55*Math.cos(radians)];
    const joints=[[220,239+bob],[272,280+bob],[170,278+bob],[183,395],[249,395],hand];
    guides.querySelectorAll('circle').forEach((point,i)=>{point.setAttribute('cx',joints[i][0]);point.setAttribute('cy',joints[i][1]);});
    guides.querySelector('path').setAttribute('d',`M${joints[0]} L220,${267+bob} L${joints[1]} L${hand} M220,${267+bob} L${joints[2]} M220,${267+bob} L220,395 L${joints[3]} M220,395 L${joints[4]}`);
    guides.style.display=document.getElementById('show-rig').checked?'inline':'none';
    document.getElementById('state-label').textContent={idle:'A quiet moment',walk:'Walking',wave:'Hello, friend!',carry:'Carry & walk'}[motion];
    box.dataset.motion=motion;box.dataset.facing=facing;box.dataset.paused=String(paused);
  }
  document.querySelectorAll('button[data-motion]').forEach(button=>button.addEventListener('click',()=>{
    motion=button.dataset.motion;document.querySelectorAll('button[data-motion]').forEach(b=>b.setAttribute('aria-pressed',String(b===button)));paint();
  }));
  document.querySelectorAll('button[data-facing]').forEach(button=>button.addEventListener('click',()=>{
    facing=button.dataset.facing;document.querySelectorAll('button[data-facing]').forEach(b=>b.setAttribute('aria-pressed',String(b===button)));paint();
  }));
  document.getElementById('pace').addEventListener('input',event=>{pace=Number(event.target.value);document.getElementById('pace-label').value=pace.toFixed(1)+'×';paint();});
  document.getElementById('show-rig').addEventListener('change',paint);
  pause.addEventListener('click',()=>{paused=!paused;refreshPause();paint();});
  document.addEventListener('visibilitychange',()=>{last=null;});
  function tick(now){if(last!==null&&!paused&&!document.hidden)time+=Math.min((now-last)/1000,.05);last=now;paint();requestAnimationFrame(tick);}
  paint();requestAnimationFrame(tick);
})();
