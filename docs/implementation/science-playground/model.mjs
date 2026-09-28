// Research models only. Values are illustrative, not laboratory predictions.
export const VERSION=1, W=1000, H=600;
export const COLORS=['#dc638c','#588ed5','#af70c7','#e5ae3b','#4cb899'];
export const ACTIVITIES=[
 ['lava','Lava jars','Add a fizz tablet. Watch the colored blobs rise, then fall.','Try warm and cool water. Does the same portion last as long?','◉'],
 ['ice','Dinosaur ice rescue','Choose a dropper, then tap or brush over the ice.','Use equal warm and cool drops. Which melts more ice?','❄'],
 ['marble','Marble playground','Release the ball. Drag the gold ramp handles to change its route.','Can you reach the basket with a different ramp arrangement?','●'],
 ['slime','Stretchy slime','Add activator and stir. Pull the slime handle slowly or quickly.','Does a quick pull behave like a slow stretch?','≈'],
 ['milk','Magic milk','Place colored drops, then touch them with the soap wand.','Touch a different spot. Watch where the colors travel.','✺'],
 ['robot','Drawing robot','Start the motor. Try different speed, balance and pen colors.','Can you make tight loops and wide wandering lines?','⚙'],
 ['rocket','Balloon rocket','Pump the balloon, then release it along the string.','Compare a small balloon with a fuller one.','➜'],
 ['foam','Foam fountain','Add peroxide and soap. Add yeast mixture to start the reaction.','Try a wide vessel or leave out the soap.','♧'],
 ['wind','Wind tube','Turn up the fan. Change the canopy or add a weight.','Can you make your invention hover halfway up?','↟'],
 ['circuits','Light-up inventions','Tap two terminals to connect a wire. Close the switch.','Make a complete loop, then try a different output.','ϟ'],
 ['weather','Mini weather world','Warm the pond, cool the air and move the wind.','Follow water from the pond into a cloud and back again.','☁'],
 ['bubbles','Bubble garden','Dip your wand, blow, then tap bubbles to pop them.','Does a square wand make a square flying bubble?','○'],
 ['light','Rainbow mirrors','Rotate the mirror. Drag the prism into the reflected beam.','Move the prism away. Where does the rainbow go?','◇'],
 ['chain','Family chain reaction','Choose a piece and tap a place. Set off your little machine.','Try a gap. Join the family chain to link your sections.','⇢']
].map(([id,title,hint,invitation,icon])=>({id,title,hint,invitation,icon}));
export const IDS=ACTIVITIES.map(a=>a.id);
export const clamp=(v,min=0,max=1)=>Math.max(min,Math.min(max,Number.isFinite(v)?v:min));
export const clone=v=>JSON.parse(JSON.stringify(v));
const norm=(x,y)=>{const d=Math.hypot(x,y)||1;return [x/d,y/d];};
export const CIRCUIT_NODES=[[190,385],[190,210],[425,210],[555,210],[785,245],[785,395]];
export function initial(id){
 const all={
  lava:{color:0,heat:1,fuel:0,used:0,clock:0,spawn:0,blobs:[],serial:0},
  ice:{heat:1,cells:Array(24).fill(1),energy:Array(24).fill(0),toy:0,drops:0,freed:false,x:500,y:355},
  marble:{tracks:[[210,150,760,220],[860,280,230,345],[140,410,700,470]],rough:false,ball:null,trace:[],last:0},
  slime:{color:2,glue:1,activator:0,mixed:0,stretch:0,torn:false,x:620,y:340},
  milk:{color:0,tool:'color',drops:[],bursts:[],saturation:0},
  robot:{running:false,speed:1,balance:1,color:0,x:500,y:300,angle:0,clock:0,paths:[],sample:0},
  rocket:{air:0,startAir:0,x:145,v:0,mass:1,phase:'idle',last:0},
  foam:{color:0,reactant:0,soap:0,catalyst:0,foam:0,gas:0,used:0,wide:false,clock:0},
  wind:{fan:0,area:1,mass:1,height:0,v:0,spin:0},
  circuits:{wires:[],closed:false,load:'lamp',selected:-1,phase:0},
  weather:{sun:1,cool:0,wind:0,water:70,vapor:25,cloud:5,fall:0,rain:0,snow:0,x:470,clock:0},
  bubbles:{film:0,shape:'round',air:1,wind:0,bubbles:[],serial:0,popped:0,clock:0},
  light:{on:true,angle:15,prismX:675,prismY:330},
  chain:{slots:['domino','ramp','domino','bell','ramp','bell'],piece:'domino',joined:false,phase:'idle',at:-1,clock:0,runs:0}
 };return all[id];
}
export function fresh(){return {version:VERSION,focus:0,quad:false,sound:false,calm:false,paused:false,players:Array.from({length:4},(_,i)=>({id:i,activity:IDS[i],states:Object.fromEntries(IDS.map(id=>[id,initial(id)]))})),family:{running:false,queue:[],at:0}};}
export function choose(world,player,id){if(IDS.includes(id)&&world.players[player])world.players[player].activity=id;}
export function state(world,p,id=world.players[p].activity){return world.players[p].states[id];}
export function iceAmount(s){return 1-s.cells.reduce((a,b)=>a+b,0)/24;}
export function slimeReady(s){return s.glue>0&&s.activator>0&&s.mixed>=1;}
function circuitPath(s,load=true){
 const edges=s.wires.slice();if(s.closed)edges.push([2,3]);if(load)edges.push([4,5]);
 const seen=new Set([0]),q=[0];while(q.length){const n=q.shift();for(const [a,b] of edges){const m=a===n?b:b===n?a:-1;if(m>=0&&!seen.has(m)){seen.add(m);q.push(m);}}}return seen.has(1);
}
export function powered(s){return !circuitPath(s,false)&&circuitPath(s,true);}
export function lightPath(s){
 const a=s.angle*Math.PI/180,dx=Math.cos(2*a),dy=Math.sin(2*a),x=410,y=250;
 const t=(s.prismX-x)*dx+(s.prismY-y)*dy,d=Math.abs((s.prismX-x)*dy-(s.prismY-y)*dx);
 return {x,y,dx,dy,hit:s.on&&t>0&&d<53,px:x+t*dx,py:y+t*dy};
}
function addWire(s,a,b){
 a=Math.round(a);b=Math.round(b);if(a===b||a<0||b<0||a>5||b>5)return false;
 const old=s.wires.findIndex(e=>e.includes(a)&&e.includes(b));if(old>=0){s.wires.splice(old,1);return true;}
 s.wires.push([Math.min(a,b),Math.max(a,b)]);
 // Reject a direct short even with the switch temporarily closed.
 if(circuitPath({...s,closed:true},false)){s.wires.pop();return false;}return true;
}
export function action(world,p,kind,v){
 if(!world.players[p])return false;
 const id=world.players[p].activity,s=state(world,p);
 if(kind==='reset'){world.players[p].states[id]=initial(id);return true;}
 if(kind==='color'){if('color' in s)s.color=Math.round(clamp(v,0,4));return true;}
 switch(id){
 case 'lava':if(kind==='tablet')s.fuel=clamp(s.fuel+1,0,5);if(kind==='heat')s.heat=clamp(v,0,2);break;
 case 'ice':
  if(kind==='heat')s.heat=v?1:0;
  if(kind==='drop'){
   const x=clamp(v.x,280,720),y=clamp(v.y,150,430);s.drops++;
   for(let i=0;i<24;i++){const cx=316+(i%6)*74,cy=185+Math.floor(i/6)*70;const d=Math.hypot(cx-x,cy-y);s.energy[i]=clamp(s.energy[i]+Math.max(0,1-d/150)*(s.heat?.28:.07),0,1.5);}
  }
  if(kind==='pour'){s.drops+=4;for(let i=0;i<24;i++)s.energy[i]=clamp(s.energy[i]+(s.heat?.22:.055),0,1.5);}
  if(kind==='toy')s.toy=(s.toy+1)%2;
  if(kind==='move'&&s.freed){s.x=clamp(v.x,180,820);s.y=clamp(v.y,260,490);}break;
 case 'marble':
  if(kind==='release'){s.ball={x:s.tracks[0][0]+18,y:s.tracks[0][1]-24,vx:0,vy:0,done:false};s.trace=[];}
  if(kind==='rough')s.rough=!s.rough;
  if(kind==='handle'){const i=Math.floor(v.index/2),end=v.index%2;if(s.tracks[i]){s.tracks[i][end*2]=clamp(v.x,120,880);s.tracks[i][end*2+1]=clamp(v.y,90+i*120,240+i*120);}}
  if(kind==='slope'){const t=s.tracks[0];t[3]=t[3]>210?180:235;}break;
 case 'slime':
  if(kind==='activator')s.activator=clamp(s.activator+.5,0,1);
  if(kind==='stir'&&s.activator)s.mixed=clamp(s.mixed+.5);
  if(kind==='pull'&&slimeReady(s)&&!s.torn){s.x=clamp(v.x,340,880);s.y=clamp(v.y,170,480);s.stretch=clamp(Math.hypot(s.x-425,s.y-345)/400);if((v.speed||0)>1200&&s.stretch>.3)s.torn=true;}
  if(kind==='slow'&&slimeReady(s)){s.stretch=.85;s.x=780;s.y=270;}
  if(kind==='quick'&&slimeReady(s)){s.stretch=.8;s.torn=true;s.x=785;}
  if(kind==='squish'){s.stretch=0;s.torn=false;s.x=620;s.y=340;}break;
 case 'milk':
  if(kind==='tool')s.tool=v==='soap'?'soap':'color';
  if(kind==='touch'){
   let [x,y]=[v.x-500,v.y-295];const d=Math.hypot(x/355,y/200);if(d>1){x/=d;y/=d;}x+=500;y+=295;
   if(s.tool==='color'&&s.drops.length<90)s.drops.push({x,y,c:s.color,r:10});
   if(s.tool==='soap'&&s.bursts.length<8){s.bursts.push({x,y,power:Math.max(.08,1-s.saturation),age:0});s.saturation=clamp(s.saturation+.14);}
  }break;
 case 'robot':
  if(kind==='run')s.running=!s.running;
  if(kind==='speed')s.speed=clamp(v,.3,2.5);
  if(kind==='balance')s.balance=clamp(v,.2,2);
  if(kind==='paper')s.paths=[];break;
 case 'rocket':
  if(kind==='pump'&&s.phase==='idle')s.air=clamp(s.air+1,0,6);
  if(kind==='release'&&s.phase==='idle'&&s.air>0){s.phase='flying';s.startAir=s.air;s.v=0;}
  if(kind==='return'){s.last=s.x;s.phase='idle';s.x=145;s.v=0;s.air=0;}
  if(kind==='mass'&&s.phase==='idle')s.mass=s.mass===1?2:1;break;
 case 'foam':
  if(kind==='peroxide')s.reactant=clamp(s.reactant+1,0,5);
  if(kind==='soap')s.soap=clamp(s.soap+1,0,3);
  if(kind==='yeast')s.catalyst=clamp(s.catalyst+.5,0,2);
  if(kind==='wide')s.wide=!s.wide;break;
 case 'wind':
  if(kind==='fan')s.fan=clamp(v,0,3);
  if(kind==='area')s.area=s.area===1?1.8:1;
  if(kind==='weight')s.mass=clamp(s.mass+1,1,4);
  if(kind==='unweight')s.mass=clamp(s.mass-1,1,4);break;
 case 'circuits':
  if(kind==='terminal'){const n=Math.round(clamp(v,0,5));if(s.selected===-1)s.selected=n;else{const ok=addWire(s,s.selected,n);s.selected=-1;return ok;}}
  if(kind==='wire')return addWire(s,v[0],v[1]);
  if(kind==='switch')s.closed=!s.closed;
  if(kind==='load')s.load=['lamp','fan','buzzer'].includes(v)?v:'lamp';
  if(kind==='loop')s.wires=[[0,5],[1,2],[3,4]];break;
 case 'weather':
  if(kind==='sun')s.sun=clamp(v,0,2);
  if(kind==='cool')s.cool=clamp(v,0,2);
  if(kind==='wind')s.wind=clamp(v,-2,2);break;
 case 'bubbles':
  if(kind==='dip')s.film=1;
  if(kind==='shape')s.shape=s.shape==='round'?'square':'round';
  if(kind==='air')s.air=clamp(v,.5,2);
  if(kind==='wind')s.wind=clamp(v,-2,2);
  if(kind==='blow'&&s.film>.1&&s.bubbles.length<28){s.film=Math.max(0,s.film-.24);for(let i=0;i<3;i++)s.bubbles.push({id:++s.serial,x:250+i*22,y:420-i*10,r:20+9*s.air+(i*7)%15,vx:45*s.air,vy:-30-12*i,age:0});}
  if(kind==='pop'){const i=s.bubbles.findIndex(b=>Math.hypot(b.x-v.x,b.y-v.y)<b.r+12);if(i>=0){s.bubbles.splice(i,1);s.popped++;}}
  break;
 case 'light':
  if(kind==='light')s.on=!s.on;
  if(kind==='angle')s.angle=clamp(v,-35,65);
  if(kind==='prism'){s.prismX=clamp(v.x,490,860);s.prismY=clamp(v.y,100,470);}
  if(kind==='align'){let r=lightPath(s);if(r.dx<.5||r.y+r.dy*310<100||r.y+r.dy*310>470){s.angle=15;r=lightPath(s);}s.prismX=r.x+r.dx*310;s.prismY=r.y+r.dy*310;}break;
 case 'chain':
  if(kind==='piece')s.piece=['domino','ramp','bell','gap'].includes(v)?v:'domino';
  if(kind==='place'&&s.phase!=='running')s.slots[Math.round(clamp(v,0,5))]=s.piece;
  if(kind==='run'){s.phase='running';s.at=0;s.clock=0;}
  if(kind==='join')s.joined=!s.joined;break;
 }
 return true;
}
export function startFamily(world){
 const queue=world.players.filter(p=>p.states.chain.joined).map(p=>p.id);
 world.family={running:queue.length>0,queue,at:0};
 if(queue.length){const s=state(world,queue[0],'chain');s.phase='running';s.at=0;s.clock=0;}
}
export function step(world,dt){
 dt=clamp(dt,0,.04);if(world.paused||!dt)return;
 // All retained experiments advance, even when a sibling changes the focused view.
 for(const p of world.players)for(const id of IDS)advance(id,p.states[id],dt);
 const f=world.family;
 if(f.running){
  const id=f.queue[f.at],s=state(world,id,'chain');
  if(!s.joined||s.phase==='done'){
   do{f.at++;}while(f.at<f.queue.length&&!state(world,f.queue[f.at],'chain').joined);
   if(f.at>=f.queue.length)f.running=false;
   else{const next=state(world,f.queue[f.at],'chain');next.phase='running';next.at=0;next.clock=0;}
  }else if(s.phase==='blocked'||s.phase==='idle')f.running=false;
 }
}
function advance(id,s,dt){
 switch(id){
 case 'lava':{
  s.clock+=dt;const spent=Math.min(s.fuel,dt*(.08+s.heat*.06));s.fuel-=spent;s.used+=spent;s.spawn+=spent*14;
  while(s.spawn>=1&&s.blobs.length<32){s.spawn--;s.serial++;s.blobs.push({x:380+(s.serial*53)%240,y:435,r:10+(s.serial*7)%17,up:true});}
  for(const b of s.blobs){b.y+=(b.up?-1:1)*(42+s.heat*12)*dt;if(b.y<160){b.y=160;b.up=false;}}
  s.blobs=s.blobs.filter(b=>b.y<443);break;
 }
 case 'ice':
  for(let i=0;i<24;i++){const melt=Math.min(s.cells[i],s.energy[i]*dt*2);s.cells[i]-=melt;s.energy[i]=Math.max(0,s.energy[i]-melt-dt*.003);}
  s.freed=iceAmount(s)>.96;if(s.freed)s.cells.fill(0);break;
 case 'marble':{
  const b=s.ball;if(!b||b.done)break;const oldY=b.y,oldX=b.x;b.vy+=420*dt;b.x+=b.vx*dt;b.y+=b.vy*dt;
  // Visible catch rails at the upper ends reverse an arriving ball, like a gutter stop.
  for(let i=1;i<3;i++){const [x,y]=s.tracks[i],sign=i===1?1:-1,edge=x-sign*12;if(b.y>y-110&&b.y<y+20&&(oldX-edge)*sign<=0&&(b.x-edge)*sign>0){b.x=edge;b.vx=-sign*Math.abs(b.vx)*.55;}}
  for(const t of s.tracks){const [x1,y1,x2,y2]=t;if(Math.abs(x2-x1)<20)continue;const u=(b.x-x1)/(x2-x1);if(u<0||u>1)continue;const y=y1+u*(y2-y1)-12;
   if(b.y>=y&&oldY<=y+20&&b.vy>=-20){b.y=y;const [dx,dy]=norm(x2-x1,y2-y1),a=420*dy;b.vx+=(a*dx-b.vx*(s.rough?2.4:.12))*dt;b.vy=b.vx*(y2-y1)/(x2-x1);}
  }
  if(b.y>=542){b.y=542;b.done=true;s.last=b.x;}if(b.x<55||b.x>945){b.x=clamp(b.x,55,945);b.vx=-b.vx*.4;}
  s.trace.push([b.x,b.y]);if(s.trace.length>220)s.trace.shift();break;
 }
 case 'slime':if(!s.torn&&slimeReady(s))s.stretch=Math.max(0,s.stretch-dt*.025);break;
 case 'milk':
  for(const b of s.bursts){b.age+=dt;for(const [i,d] of s.drops.entries()){let dx=d.x-b.x,dy=d.y-b.y;if(Math.hypot(dx,dy)<1){dx=Math.cos(i*2.4)*20;dy=Math.sin(i*2.4)*20;}const dist=Math.max(20,Math.hypot(dx,dy));const f=b.power*Math.exp(-b.age*.9)*65/(1+dist/90);d.x+=(dx/dist*.9-dy/dist*.5)*f*dt;d.y+=(dy/dist*.9+dx/dist*.5)*f*dt;d.r=Math.min(25,d.r+dt*f*.035);const u=(d.x-500)/345,v=(d.y-295)/193,k=Math.hypot(u,v);if(k>1){d.x=500+(d.x-500)/k;d.y=295+(d.y-295)/k;}}}
  s.bursts=s.bursts.filter(b=>b.age<5);break;
 case 'robot':
  if(s.running&&s.paths.length<1200){s.clock+=dt*s.speed;s.angle+=dt*(.7+s.balance*2.2+Math.sin(s.clock*3)*s.balance);s.x+=Math.cos(s.angle)*dt*s.speed*65;s.y+=Math.sin(s.angle)*dt*s.speed*65;if(s.x<200||s.x>800||s.y<130||s.y>460){s.angle+=Math.PI*.75;s.x=clamp(s.x,200,800);s.y=clamp(s.y,130,460);}s.sample+=dt;if(s.sample>.05){s.sample=0;s.paths.push([s.x,s.y,s.angle,s.color]);}}break;
 case 'rocket':
  if(s.phase==='flying'){const gas=Math.min(s.air,dt*1.15);s.air-=gas;s.v+=gas*125/s.mass;s.v=Math.max(0,s.v-dt*24);s.x+=s.v*dt;if(s.x>=860||(s.air<=0&&s.v<=.01)){s.x=Math.min(s.x,860);s.phase='rest';s.last=s.x;}}
  break;
 case 'foam':{
  s.clock+=dt;const gas=Math.min(s.reactant,dt*s.catalyst*.28);s.reactant-=gas;s.used+=gas;s.gas=gas/dt;s.foam=clamp(s.foam+gas*s.soap*1.8-dt*s.foam*.06,0,10);break;
 }
 case 'wind':{
  const air=s.fan*1.4*(1-s.height/700),rel=air-s.v/100;
  const force=rel*Math.abs(rel)*s.area*.18-s.mass*.65;
  s.v=clamp(s.v+force/s.mass*dt*100,-140,140);s.height=clamp(s.height+s.v*dt,0,380);if((s.height===0&&s.v<0)||(s.height===380&&s.v>0))s.v=0;s.spin+=dt*s.fan;break;
 }
 case 'circuits':if(powered(s))s.phase+=dt*4;break;
 case 'weather':{
  s.clock+=dt;const evap=Math.min(s.water,dt*(.1+s.sun*.7)),cond=Math.min(s.vapor,dt*s.cool*1.4),rain=Math.min(Math.max(0,s.cloud-8),dt*2);
  s.water-=evap;s.vapor+=evap-cond;s.cloud+=cond-rain;s.fall+=rain;
  const landed=Math.min(s.fall,dt*2);s.fall-=landed;s.water+=landed;s.rain=s.cool<1.7?rain/dt:0;s.snow=s.cool>=1.7?rain/dt:0;
  s.x=clamp(s.x+s.wind*dt*13,230,770);break;
 }
 case 'bubbles':
  s.clock+=dt;for(const b of s.bubbles){b.age+=dt;b.vx+=(s.wind*30-b.vx*.2)*dt;b.x+=b.vx*dt;b.y+=b.vy*dt;b.vy-=dt*2;}s.bubbles=s.bubbles.filter(b=>b.age<14&&b.x>-80&&b.x<1080&&b.y>-70);break;
 case 'chain':
  if(s.phase==='running'){if(s.slots[s.at]==='gap'){s.phase='blocked';break;}s.clock+=dt;if(s.clock>=.65){s.clock=0;s.at++;if(s.at>=6){s.at=5;s.phase='done';s.runs++;}}}break;
 }
}
export function observe(id,s){
 switch(id){
 case 'lava':return s.fuel>.001?'Gas carries the colored water up. It falls when the gas escapes.':s.blobs.length?'The last blobs are settling.':'Ready for a fizz tablet.';
 case 'ice':return s.freed?'Your dinosaur is free! Move it around the tray.':`${Math.round(iceAmount(s)*100)}% melted · ${s.heat?'Warm':'Cool'} dropper`;
 case 'marble':return !s.ball?'The course is ready.':s.ball.done?(s.last>650&&s.last<850?'In the basket! Try changing the ramps.':'The ball landed on the mat. Adjust a ramp and try again.'):'Watch the ball follow your ramps.';
 case 'slime':return !s.activator?'Add activator to the glue mixture.':!slimeReady(s)?'Stir the two portions together.':s.torn?'That quick pull tore the slime. Squish it together.':'Ready to stretch. Pull slowly, then try quickly.';
 case 'milk':return s.bursts.length?'The soap pushes the colors away from where you touched.':s.drops.length?'Touch a colored spot with the soap wand.':'Add some colored drops to the milk.';
 case 'robot':return s.paths.length>=1200?'Your paper is full. Keep it, or start fresh paper.':s.running?'The off-center motor makes the robot wobble and draw.':'Motor stopped. Your drawing stays here.';
 case 'rocket':return s.phase==='idle'?`Air portions: ${s.air} · Weight: ${s.mass}`:s.phase==='flying'?'Air goes backward. Your rocket moves forward.':'Flight finished. Return the rocket to try again.';
 case 'foam':return s.gas>.001?(s.soap?'Oxygen bubbles are caught in the soap as foam.':'Gas bubbles escape. Try soap to hold them as foam.'):s.reactant?'Add yeast mixture to start the reaction.':s.used?'The gas supply is used up. Foam will settle.':'Add peroxide, soap and yeast mixture.';
 case 'wind':return !s.fan?'Fan off. Gravity brings your invention down.':Math.abs(s.v)<7&&s.height>20?'Hovering! Try another weight or canopy.':'Air pushes up; weight pulls down.';
 case 'circuits':return powered(s)?`A complete loop powers the ${s.load}.`:s.selected>=0?'Now choose another terminal.':'Complete the wires and close the switch.';
 case 'weather':return s.snow?'Cloud water returns as snow in cold air.':s.rain?'Droplets have grown: rain returns water to the pond.':s.cool?'Cooling helps vapor condense into cloud droplets.':'Sun warms the pond. Some water evaporates.';
 case 'bubbles':return s.bubbles.length?'The flying bubbles are round, whatever the wand shape.':s.film?'The film is ready. Blow through your wand.':'Dip the wand to pick up a soap film.';
 case 'light':return !s.on?'Light off.':lightPath(s).hit?'The prism separates the white light into colors.':'Move the prism into the reflected beam.';
 case 'chain':return s.phase==='blocked'?'The gap stopped the chain. Replace it and try again.':s.phase==='done'?'Your whole section worked!':s.phase==='running'?'One action sets off the next.':'Build your section, then give it a nudge.';
 }
}

// Reject malformed or oversized saved prototypes; never coerce unknown data into a new save.
export function valid(world){
 try{
  const shape=(value,template)=>{
   if(template===null)return value===null;
   if(Array.isArray(template))return Array.isArray(value)&&(template.length===0||value.length===template.length&&value.every((v,i)=>shape(v,template[i])));
   if(typeof template==='object')return value!==null&&!Array.isArray(value)&&Object.keys(value).sort().join()===Object.keys(template).sort().join()&&Object.keys(template).every(k=>shape(value[k],template[k]));
   return typeof value===typeof template&&(typeof value!=='number'||Number.isFinite(value));
  };
  const records=(values,template,max)=>Array.isArray(values)&&values.length<=max&&values.every(v=>shape(v,template));
  if(world.version!==VERSION||!Array.isArray(world.players)||world.players.length!==4||!Number.isInteger(world.focus)||world.focus<0||world.focus>3)return false;
  for(const k of ['quad','sound','calm','paused'])if(typeof world[k]!=='boolean')return false;
  for(let i=0;i<4;i++){
   const p=world.players[i];if(p.id!==i||!IDS.includes(p.activity)||Object.keys(p.states).length!==14)return false;
   for(const id of IDS){const s=p.states[id],base=initial(id);if(id==='marble'&&s?.ball!==null)base.ball={x:0,y:0,vx:0,vy:0,done:false};if(!s||!shape(s,base))return false;}
   const s=p.states;if(s.ice.cells.length!==24||s.ice.energy.length!==24||s.marble.tracks.length!==3||s.robot.paths.length>1200||s.milk.drops.length>90||s.lava.blobs.length>32||s.bubbles.bubbles.length>30||s.chain.slots.length!==6)return false;
   if(s.circuits.wires.length>15||s.circuits.wires.some(e=>e.length!==2||e.some(n=>!Number.isInteger(n)||n<0||n>5)))return false;
   if(s.chain.slots.some(v=>!['domino','ramp','bell','gap'].includes(v)))return false;
   if(!records(s.lava.blobs,{x:0,y:0,r:0,up:true},32)||!records(s.marble.trace,[0,0],220)||!records(s.milk.drops,{x:0,y:0,c:0,r:0},90)||!records(s.milk.bursts,{x:0,y:0,power:0,age:0},8)||!records(s.robot.paths,[0,0,0,0],1200)||!records(s.bubbles.bubbles,{id:0,x:0,y:0,r:0,vx:0,vy:0,age:0},30))return false;
   if(!['idle','flying','rest'].includes(s.rocket.phase)||!['idle','running','blocked','done'].includes(s.chain.phase)||!['domino','ramp','bell','gap'].includes(s.chain.piece)||!['lamp','fan','buzzer'].includes(s.circuits.load)||!['round','square'].includes(s.bubbles.shape)||!['soap','color'].includes(s.milk.tool))return false;
   if(!Number.isInteger(s.chain.at)||s.chain.at< -1||s.chain.at>5||s.chain.phase==='running'&&s.chain.at<0||s.ice.energy.some(v=>v<0||v>1.5))return false;
   if(s.ice.cells.some(v=>v<0||v>1)||Math.abs(s.weather.water+s.weather.vapor+s.weather.cloud+s.weather.fall-100)>.001)return false;
  }
  const scan=(v,depth=0)=>{if(depth>10)return false;if(typeof v==='number')return Number.isFinite(v)&&Math.abs(v)<1e9;if(typeof v==='string')return v.length<40;if(v===null||typeof v==='boolean')return true;if(Array.isArray(v))return v.length<=1200&&v.every(x=>scan(x,depth+1));if(typeof v==='object')return Object.values(v).every(x=>scan(x,depth+1));return false;};
  const f=world.family;
  return shape(f,{running:false,queue:[],at:0})&&f.queue.length<=4&&new Set(f.queue).size===f.queue.length&&f.queue.every(i=>Number.isInteger(i)&&i>=0&&i<4)&&Number.isInteger(f.at)&&f.at>=0&&f.at<=f.queue.length&&(!f.running||f.at<f.queue.length)&&scan(world)&&JSON.stringify(world).length<900000;
 }catch{return false;}
}
