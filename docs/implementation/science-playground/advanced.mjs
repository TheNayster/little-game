import {state,COLORS} from './model.mjs';
import {picButton} from './controls.mjs';
import {picture} from './pictures.mjs';

export function renderAdvanced(world,i,element,dispatch,runFamily){
 const id=world.players[i].activity,s=state(world,i);element.replaceChildren();
 const b=(label,icon,k,v,selected)=>{const e=picButton(label,icon,()=>dispatch(i,k,v,{rebuild:true}),'extra-picture');if(selected!==undefined)e.setAttribute('aria-pressed',selected);element.append(e);};
 const r=(label,icon,k,value,min,max,step=.1)=>{const e=document.createElement('label');e.className='range';const name=document.createElement('span');name.innerHTML=picture(icon);name.append(document.createTextNode(label));const input=document.createElement('input');input.type='range';input.min=min;input.max=max;input.step=step;input.value=value;input.setAttribute('aria-label',label);input.oninput=()=>dispatch(i,k,Number(input.value));e.append(name,input);element.append(e);};
 if('color'in s){COLORS.forEach((c,n)=>{const e=document.createElement('button');e.className='swatch';e.style.background=c;e.setAttribute('aria-label',['Rose','Blue','Purple','Gold','Mint'][n]);e.setAttribute('aria-pressed',s.color===n);e.textContent=s.color===n?'✓':'';e.onclick=()=>dispatch(i,'color',n,{rebuild:true});element.append(e);});}
 switch(id){
 case 'lava':b(s.heat?'Cool water':'Warm water',s.heat?'snow':'sun','heat',s.heat?0:1);break;
 case 'ice':b('Little hammer','hammer','iceTool','hammer',s.tool==='hammer');b('Water dropper','dropper','iceTool','water',s.tool==='water');b('Warm water','sun','heat',1,s.heat===1);b('Cool water','snow','heat',0,s.heat===0);b('Change dinosaur','dinosaur','toy');break;
 case 'marble':b(s.rough?'Smooth surface':'Felt surface',s.rough?'smooth':'felt','rough');break;
 case 'slime':b('Add activator','bottle','activator');b('Stir','spoon','stir');b('Squish together','slime','squish');break;
 case 'milk':b('Color dropper','dropper','tool','color',s.tool==='color');b('Soap wand','soap','tool','soap',s.tool==='soap');break;
 case 'robot':r('Motor speed','fan','speed',s.speed,.3,2.5);r('Off-center weight','weight','balance',s.balance,.2,2);b('New paper','paper','paper');break;
 case 'rocket':b(s.mass===1?'Add weight':'Remove weight',s.mass===1?'weight':'removeWeight','mass');b('Add air','pump','pump');break;
 case 'foam':b('Starter liquid','bottle','peroxide');b('Soap','soap','soap');b('Yeast mixture','mix','yeast');b(s.wide?'Narrow bottle':'Wide bowl',s.wide?'narrow':'wide','wide');break;
 case 'wind':r('Fan strength','fan','fan',s.fan,0,3);b('Add weight','weight','weight');b('Remove weight','removeWeight','unweight');break;
 case 'circuits':[['Lamp','bulb','lamp'],['Fan','fan','fan'],['Buzzer','bell','buzzer']].forEach(([label,icon,v])=>b(label,icon,'load',v,s.load===v));break;
 case 'weather':r('Sun warmth','sun','sun',s.sun,0,2);r('Cool the air','snow','cool',s.cool,0,2);r('Wind','wind','wind',s.wind,-2,2);break;
 case 'bubbles':b(s.shape==='round'?'Square wand':'Round wand','wand','shape');r('Blowing strength','bubbles','air',s.air,.5,2);r('Wind','wind','wind',s.wind,-2,2);break;
 case 'colors':b('Clear water','water','water');b('Stir together','spoon','stir');break;
 case 'light':r('Mirror angle','mirror','angle',s.angle,-35,65,1);break;
 case 'chain':['domino','ramp','bell','gap'].forEach(v=>b(v[0].toUpperCase()+v.slice(1),v,'piece',v,s.piece===v));b(s.joined?'Leave chain':'Join chain','family','join');element.append(picButton('Run together','play',runFamily,'extra-picture'));break;
 }
}
