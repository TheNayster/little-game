import {state,COLORS} from './model.mjs';

export function renderAdvanced(world,i,element,dispatch,runFamily){
 const id=world.players[i].activity,s=state(world,i);element.replaceChildren();
 const b=(label,k,v)=>{const e=document.createElement('button');e.textContent=label;e.onclick=()=>dispatch(i,k,v,{rebuild:true});element.append(e);};
 const r=(label,k,value,min,max,step=.1)=>{const e=document.createElement('label');e.className='range';const name=document.createElement('span');name.textContent=label;const input=document.createElement('input');input.type='range';input.min=min;input.max=max;input.step=step;input.value=value;input.setAttribute('aria-label',label);input.oninput=()=>dispatch(i,k,Number(input.value));e.append(name,input);element.append(e);};
 if('color'in s){COLORS.forEach((c,n)=>{const e=document.createElement('button');e.className='swatch';e.style.background=c;e.setAttribute('aria-label',['Rose','Blue','Purple','Gold','Mint'][n]);e.textContent=s.color===n?'✓':'';e.onclick=()=>dispatch(i,'color',n,{rebuild:true});element.append(e);});}
 switch(id){
 case 'lava':b(s.heat?'Cool water':'Warm water','heat',s.heat?0:1);break;
 case 'ice':b('Warm dropper','heat',1);b('Cool dropper','heat',0);b('Change dinosaur','toy');break;
 case 'marble':b(s.rough?'Use smooth surface':'Use felt surface','rough');break;
 case 'slime':b('Add activator','activator');b('Stir','stir');b('Squish together','squish');break;
 case 'milk':b('Color dropper','tool','color');b('Soap wand','tool','soap');break;
 case 'robot':r('Motor speed','speed',s.speed,.3,2.5);r('Off-center weight','balance',s.balance,.2,2);b('New paper','paper');break;
 case 'rocket':b(s.mass===1?'Add weight':'Remove weight','mass');b('Add air','pump');break;
 case 'foam':b('Peroxide','peroxide');b('Soap','soap');b('Yeast mixture','yeast');b(s.wide?'Use narrow vessel':'Use wide vessel','wide');break;
 case 'wind':r('Fan strength','fan',s.fan,0,3);b('Add weight','weight');b('Remove weight','unweight');break;
 case 'circuits':['lamp','fan','buzzer'].forEach(v=>b(v,'load',v));break;
 case 'weather':r('Sun warmth','sun',s.sun,0,2);r('Cool the air','cool',s.cool,0,2);r('Wind','wind',s.wind,-2,2);break;
 case 'bubbles':b(s.shape==='round'?'Square wand':'Round wand','shape');r('Blowing strength','air',s.air,.5,2);r('Wind','wind',s.wind,-2,2);break;
 case 'light':r('Mirror angle','angle',s.angle,-35,65,1);break;
 case 'chain':['domino','ramp','bell','gap'].forEach(v=>b(v,'piece',v));b(s.joined?'Leave family chain':'Join family chain','join');const e=document.createElement('button');e.textContent='Run joined sections';e.onclick=runFamily;element.append(e);break;
 }
}
