import {COLORS,iceAmount,slimeReady,powered,lightPath,CIRCUIT_NODES} from './model.mjs';
const INK='#35565a', GOLD='#eeb959', BLUE='#79bdd3';
function ellipse(c,x,y,rx,ry,fill,stroke){c.beginPath();c.ellipse(x,y,Math.max(.1,rx),Math.max(.1,ry),0,0,7);if(fill){c.fillStyle=fill;c.fill();}if(stroke){c.strokeStyle=stroke;c.lineWidth=3;c.stroke();}}
function rect(c,x,y,w,h,r,fill,stroke){c.beginPath();c.roundRect(x,y,w,h,r);if(fill){c.fillStyle=fill;c.fill();}if(stroke){c.strokeStyle=stroke;c.lineWidth=3;c.stroke();}}
function line(c,points,color,width=5){c.beginPath();points.forEach((p,i)=>i?c.lineTo(...p):c.moveTo(...p));c.strokeStyle=color;c.lineWidth=width;c.lineCap='round';c.lineJoin='round';c.stroke();}
function text(c,s,x,y,size=25,color=INK){c.font=`600 ${size}px 'Segoe UI',sans-serif`;c.fillStyle=color;c.textAlign='center';c.fillText(s,x,y);}
function glass(c,x,y,w,h){const g=c.createLinearGradient(x,0,x+w,0);g.addColorStop(0,'#e3f8ffbb');g.addColorStop(.25,'#faffff11');g.addColorStop(.75,'#dbf8ff22');g.addColorStop(1,'#a0d3e699');rect(c,x,y,w,h,24,g,'#477b8c');line(c,[[x+15,y+20],[x+15,y+h-32]],'#ffffffbb',7);ellipse(c,x+w/2,y,w/2,13,'#edfaff99','#477b8c');}
function bottle(c,x,y,color,label){rect(c,x-25,y-62,50,80,13,color,'#56747c');rect(c,x-17,y-82,34,24,6,'#f6eed7','#56747c');rect(c,x-17,y-38,34,32,6,'#fffaf0');text(c,label,x,y+53,22);}
function mat(c,color='#e5eedd'){ellipse(c,500,490,380,56,'#52674833');rect(c,120,112,760,398,40,color,'#ffffffbb');}
function dish(c){ellipse(c,500,310,382,222,'#8aa7a144');ellipse(c,500,295,375,214,'#faf5e3','#a6bdb8');ellipse(c,500,295,354,199,'#fffdf3','#dedaca');}
function drop(c,x,y,r,color){c.save();c.translate(x,y);c.beginPath();c.moveTo(0,-r*1.6);c.bezierCurveTo(-r*1.8,.2*r,-r,r*1.4,0,r*1.4);c.bezierCurveTo(r,r*1.4,r*1.8,.2*r,0,-r*1.6);c.fillStyle=color;c.fill();c.restore();}
function toy(c,x,y,type=0,scale=1){c.save();c.translate(x,y);c.scale(scale,scale);const col=type?'#dfae62':'#70ae80';ellipse(c,-13,5,70,45,col,INK);line(c,[[-59,7],[-110,-21],[-73,29]],col,24);rect(c,-60,32,29,47,12,col,INK);rect(c,24,32,29,47,12,col,INK);if(type){line(c,[[34,-8],[44,-87],[62,-112]],col,30);ellipse(c,72,-111,36,22,col,INK);}else{rect(c,23,-76,83,55,17,col,INK);line(c,[[36,-30],[53,-48]],col,33);line(c,[[48,-6],[70,9]],col,12);ellipse(c,91,-58,5,5,INK);}ellipse(c,type?86:86,type?-116:-57,5,5,INK);c.restore();}
function arrow(c,x,y,angle,color=GOLD){c.save();c.translate(x,y);c.rotate(angle);line(c,[[-25,0],[22,0]],color,6);line(c,[[8,-12],[22,0],[8,12]],color,6);c.restore();}
function gear(c,x,y,a){c.save();c.translate(x,y);c.rotate(a);for(let i=0;i<8;i++){c.rotate(Math.PI/4);rect(c,28,-8,17,16,3,'#dbb878',INK);}ellipse(c,0,0,32,32,'#dfc997',INK);ellipse(c,0,0,10,10,'#fff6df',INK);c.restore();}
function robot(c,s){c.save();c.translate(s.x,s.y);c.rotate(s.angle);rect(c,-40,-34,80,68,20,'#7fbeb2',INK);ellipse(c,17,-15,8,8,'white');ellipse(c,17,15,8,8,'white');ellipse(c,20,-15,4,4,INK);ellipse(c,20,15,4,4,INK);gear(c,-8,0,s.clock*6);for(let i=0;i<3;i++){const a=i*2.094;line(c,[[Math.cos(a)*30,Math.sin(a)*30],[Math.cos(a)*55,Math.sin(a)*55]],COLORS[(s.color+i)%5],9);}c.restore();}
export function handles(id,s){if(id==='marble')return s.tracks.flatMap(t=>[[t[0],t[1]],[t[2],t[3]]]);if(id==='circuits')return CIRCUIT_NODES;return [];}
export function draw(c,id,s,world){
 c.clearRect(0,0,1000,600);c.save();c.fillStyle='#fff6dfc4';c.fillRect(0,0,1000,600);
 c.fillStyle='#b7854633';c.fillRect(0,485,1000,115);line(c,[[0,487],[1000,487]],'#b58c5a33',3);
 switch(id){
 case 'lava':{
  ellipse(c,500,518,192,24,'#60878333');rect(c,335,93,330,402,25,'#fffbee','#668b93');
  rect(c,348,142,304,286,10,'#eed97070');rect(c,348,426,304,55,8,COLORS[s.color]+'ba');
  for(const b of s.blobs){ellipse(c,b.x+(world.calm?0:Math.sin(s.clock+b.x)*5),b.y,b.r,b.r*1.35,COLORS[s.color]+'d9');if(b.up)ellipse(c,b.x+3,b.y-b.r,6,6,'#ffffffc9');}
  glass(c,335,93,330,402);bottle(c,200,300,COLORS[s.color],'Color');ellipse(c,793,300,33,18,'#fffaf4','#b5aba0');line(c,[[774,293],[809,305]],'#d8cec0',3);text(c,'Fizz tablet',793,352,25);text(c,['Cool','Warm','Warmer'][s.heat],500,555,25);break;
 }
 case 'ice':{
  ellipse(c,500,464,310,45,'#8bbdce80');toy(c,s.freed?s.x:500,s.freed?s.y:345,s.toy,1.28);
  for(let i=0;i<24;i++){const a=s.cells[i];if(a<.01)continue;const x=279+(i%6)*74,y=148+Math.floor(i/6)*70;c.globalAlpha=a*.8;rect(c,x,y,77,74,12,'#99d9ed','#d3f5ff');line(c,[[x+12,y+10],[x+54,y+6]],'#fff',3);}c.globalAlpha=1;
  bottle(c,145,230,s.heat?'#e69f74':'#85badc',s.heat?'Warm':'Cool');if(!s.freed){for(let i=0;i<24;i++)if(s.energy[i]>.08)drop(c,316+(i%6)*74,185+Math.floor(i/6)*70,7,'#edfaffaa');}text(c,s.freed?'Free to play!':'Touch the ice with your dropper',500,555,26);break;
 }
 case 'marble':{
  rect(c,95,58,810,500,24,'#f6e5c3','#ccb88a');for(let x=135;x<880;x+=50)for(let y=90;y<550;y+=50)ellipse(c,x,y,3,3,'#d5c39e');
  for(const t of s.tracks){line(c,[[t[0],t[1]+9],[t[2],t[3]+9]],'#96715d',18);line(c,[[t[0],t[1]],[t[2],t[3]]],s.rough?'#9cb196':'#dfb16c',15);}
  for(let i=1;i<3;i++){const [x,y]=s.tracks[i];line(c,[[x,y-105],[x,y+10]],'#85a99b',13);}
  for(const h of handles(id,s)){ellipse(c,...h,18,18,GOLD,INK);ellipse(c,...h,5,5,'#fff9d9');}
  rect(c,650,513,200,52,12,'#86b7a5',INK);for(let x=665;x<850;x+=27)line(c,[[x,521],[x,558]],'#568d7d',3);
  if(s.trace.length>1)line(c,s.trace,'#599cbe44',4);if(s.ball)ellipse(c,s.ball.x,s.ball.y,12,12,'#678cd9',INK);else ellipse(c,s.tracks[0][0]+18,s.tracks[0][1]-24,12,12,'#678cd9',INK);break;
 }
 case 'slime':{
  mat(c,'#dbeade');const color=COLORS[s.color];
  if(!slimeReady(s)){ellipse(c,490,345,198,70,color+'99','#a899a7');bottle(c,220,220,'#fffaf2','Glue');bottle(c,790,220,'#a8deea','Activator');text(c,s.activator?'Stir to form your slime':'Your glue mixture is ready',500,170,27);}
  else{const right=500+s.stretch*300,top=350-s.stretch*60;c.beginPath();c.moveTo(330,350);c.bezierCurveTo(305,230,490,270,right,top-35);c.bezierCurveTo(right+45,top-50,right+65,top+28,right,top+36);c.bezierCurveTo(510,420,395,435,330,350);c.fillStyle=color;c.fill();c.strokeStyle=INK;c.lineWidth=4;c.stroke();line(c,[[380,305],[465,298],[right-20,top-10]],'#ffffff66',7);
   if(s.torn){rect(c,600,220,48,225,8,'#dbeade');text(c,'Torn — squish to rejoin',500,175,27);}ellipse(c,right,top,21,21,'#f6e7b0',INK);}
  break;
 }
 case 'milk':{
  dish(c);c.save();c.beginPath();c.ellipse(500,295,352,196,0,0,7);c.clip();for(const d of s.drops)ellipse(c,d.x,d.y,d.r*1.8,d.r,COLORS[d.c]+'ce');for(const b of s.bursts){c.globalAlpha=Math.max(0,1-b.age/5)*.4;ellipse(c,b.x,b.y,25+b.age*25,25+b.age*15,null,'#cbe6e0');}c.restore();text(c,s.tool==='soap'?'Soap wand selected':'Color dropper selected',500,557,26);break;
 }
 case 'robot':{
  rect(c,140,67,720,461,10,'#fffdfa','#b9c8c0');c.save();c.beginPath();c.rect(151,78,698,440);c.clip();
  for(let i=1;i<s.paths.length;i++){const a=s.paths[i-1],b=s.paths[i];for(let pen=0;pen<3;pen++){const angle=pen*2.094;line(c,[[a[0]+Math.cos(a[2]+angle)*55,a[1]+Math.sin(a[2]+angle)*55],[b[0]+Math.cos(b[2]+angle)*55,b[1]+Math.sin(b[2]+angle)*55]],COLORS[(b[3]+pen)%5]+'aa',3);}}c.restore();robot(c,s);break;
 }
 case 'rocket':{
  rect(c,75,85,850,420,26,'#eff1da');line(c,[[105,296],[890,296]],'#8b7e65',4);rect(c,93,240,17,260,5,'#b99261');rect(c,888,240,17,260,5,'#b99261');
  const r=30+s.air*9;ellipse(c,s.x,245,r*1.12,r,COLORS[1],INK);line(c,[[s.x,265],[s.x,295]],'#b4bfb9',13);rect(c,s.x-46,297,90,37,12,'#edb660',INK);line(c,[[s.x-35,297],[s.x+25,297]],'#fff3c5',5);if(s.phase==='flying'&&s.air>0){arrow(c,s.x-110,246,Math.PI,'#fffdf0');arrow(c,s.x+125,330,0,INK);}for(let i=0;i<s.mass;i++)rect(c,s.x-15+i*24,337,20,20,4,'#8a8584');text(c,'AIR OUT',s.x-110,207,20);if(s.phase==='rest')text(c,'Ready for another flight?',500,460,28);break;
 }
 case 'foam':{
  ellipse(c,500,520,345,42,'#cedcc2',INK);const width=s.wide?250:145,x=500-width/2;
  const height=Math.min(360,s.foam*(s.wide?42:75));const col=COLORS[s.color];
  rect(c,x,200,width,293,20,'#edfaff44','#6597a2');rect(c,x+8,450-s.reactant*13,width-16,35+s.reactant*13,10,'#9acde0aa');
  if(height>2){for(let i=0;i<38;i++){const bx=500+Math.sin(i*8.81)*(width*.5+Math.max(0,height-265)*.9),by=480-(i/38)*height;ellipse(c,bx,by,19+(i%4)*4,15+(i%4)*4,col+'d9','#fff9f488');}}
  if(s.gas>.001&&!s.soap)for(let i=0;i<12;i++)ellipse(c,500+Math.sin(i*13)*width*.32,470-(s.clock*55+i*24)%220,5+(i%3)*3,7+(i%3)*3,'#efffff99');glass(c,x,200,width,293);bottle(c,190,300,'#bddce1','Peroxide');bottle(c,810,300,'#edc573','Soap + yeast');break;
 }
 case 'wind':{
  const y=460-s.height;rect(c,310,75,380,443,28,'#e7f8fa50','#81a5aa');
  if(s.fan&&!world.calm)for(let i=0;i<12;i++){const x=340+(i*59)%310,yy=490-(s.spin*80+i*31)%380;arrow(c,x,yy,-Math.PI/2,'#a1caca66');}
  const canopy=65*s.area;ellipse(c,500,y,canopy,canopy*.45,'#f0be71',INK);line(c,[[500-canopy,y],[476,y+65],[524,y+65],[500+canopy,y]],INK,3);rect(c,470,y+55,60,30+s.mass*5,8,'#8bb8a8',INK);for(let i=0;i<s.mass;i++)ellipse(c,480+i*12,y+71,5,5,'#405b61');rect(c,290,514,420,50,16,'#84b4a8',INK);for(let i=0;i<4;i++)gear(c,360+i*95,540,s.spin*(world.calm?0:1));glass(c,310,75,380,443);break;
 }
 case 'circuits':{
  mat(c,'#e5ecda');for(const [a,b] of s.wires){const p=CIRCUIT_NODES[a],q=CIRCUIT_NODES[b];c.beginPath();c.moveTo(...p);c.bezierCurveTo(p[0],p[1]+90,q[0],q[1]+90,...q);c.strokeStyle=powered(s)?'#dba647':'#7da6ae';c.lineWidth=8;c.stroke();}
  rect(c,152,257,78,83,12,'#b78760',INK);text(c,'+',190,280,30,'#fff');text(c,'−',190,326,30,'#fff');line(c,[[190,255],CIRCUIT_NODES[1]],INK,5);line(c,[[190,340],CIRCUIT_NODES[0]],INK,5);
  line(c,[[425,210],s.closed?[555,210]:[530,155]],'#b5894a',12);rect(c,385,227,210,38,10,'#c3ceae');
  const on=powered(s);if(s.load==='lamp'){if(on){c.shadowBlur=45;c.shadowColor='#fff088';}ellipse(c,785,315,53,59,on?'#ffe998':'#d5e3da',INK);c.shadowBlur=0;rect(c,760,367,50,28,9,'#b49d80');}
  if(s.load==='fan'){gear(c,785,320,world.calm?0:s.phase);ellipse(c,785,320,68,68,null,INK);}
  if(s.load==='buzzer'){rect(c,739,278,92,75,20,'#bfaaa0',INK);if(on)for(let i=0;i<3;i++)ellipse(c,785,315,55+i*16,48+i*14,null,'#eeb95977');}
  for(let i=0;i<6;i++){ellipse(c,...CIRCUIT_NODES[i],19,19,s.selected===i?GOLD:'#f4eee0',INK);text(c,String(i+1),CIRCUIT_NODES[i][0],CIRCUIT_NODES[i][1]+7,20);}break;
 }
 case 'weather':{
  const g=c.createLinearGradient(0,70,0,480);g.addColorStop(0,s.cool>1.6?'#b3c6dc':'#a2d9e8');g.addColorStop(1,'#e4f4df');rect(c,90,60,820,480,25,g);ellipse(c,230,140,42+s.sun*13,42+s.sun*13,'#ffe394');
  c.beginPath();c.moveTo(90,460);c.quadraticCurveTo(320,305,470,455);c.quadraticCurveTo(690,310,910,430);c.lineTo(910,540);c.lineTo(90,540);c.fillStyle='#93bc87';c.fill();ellipse(c,400,474,200,30+s.water*.35,'#64b9d0');
  if(s.sun>0)for(let i=0;i<7;i++){const y=440-(s.clock*(12+s.sun*9)+i*23)%250;drop(c,330+i*22,y,3,'#e2f9fa88');}
  if(s.cloud>1){for(let i=0;i<6;i++)ellipse(c,s.x-80+i*30,190+Math.sin(i*2)*14,34+s.cloud*.5,27+s.cloud*.35,s.cool>1.6?'#f4f7fb':'#dcecf2');}
  if(s.rain||s.snow){for(let i=0;i<35;i++){const x=s.x-105+(i*47)%210,y=225+(s.clock*110+i*31)%225;if(s.snow)text(c,'✧',x,y,19,'#fff');else line(c,[[x,y],[x+s.wind*4,y+14]],'#568bb7',3);}}text(c,'Water keeps moving around the world',500,580,25);break;
 }
 case 'bubbles':{
  mat(c,'#dcecd555');ellipse(c,240,479,110,30,'#96bac4',INK);line(c,[[220,450],[250,335]],'#be9f6e',13);
  if(s.shape==='round')ellipse(c,267,290,55,55,s.film?'#badaf977':null,'#f2ca75');else rect(c,215,235,108,108,12,s.film?'#badaf977':null,'#f2ca75');
  for(const b of s.bubbles){const g=c.createRadialGradient(b.x-b.r*.35,b.y-b.r*.3,2,b.x,b.y,b.r);g.addColorStop(0,'#ffffff66');g.addColorStop(.8,'#a2d9ee22');g.addColorStop(1,'#db83b888');ellipse(c,b.x,b.y,b.r,b.r,g,'#ffffffb0');c.beginPath();c.arc(b.x,b.y,b.r*.8,3.7,4.8);c.strokeStyle='#fff';c.lineWidth=3;c.stroke();}break;
 }
 case 'light':{
  rect(c,90,65,820,470,28,'#344d60');const r=lightPath(s);if(s.on){line(c,[[160,250],[410,250]],'#fff6c5',8);line(c,[[410,250],[410+r.dx*900,250+r.dy*900]],'#fff6c5',6);}
  c.save();c.translate(410,250);c.rotate(s.angle*Math.PI/180);rect(c,-68,-7,136,14,7,'#c6e7f2','#88adb8');c.restore();
  c.beginPath();c.moveTo(s.prismX,s.prismY-54);c.lineTo(s.prismX+54,s.prismY+40);c.lineTo(s.prismX-54,s.prismY+40);c.closePath();c.fillStyle='#c8e9f36b';c.fill();c.strokeStyle='#e5f8ff';c.lineWidth=4;c.stroke();
  if(r.hit){const col=['#ef707a','#f2a14f','#efe567','#9ecf73','#6baedf','#9f85d9'];for(let i=0;i<6;i++){const a=Math.atan2(r.dy,r.dx)+(i-2.5)*.047;line(c,[[r.px,r.py],[r.px+Math.cos(a)*650,r.py+Math.sin(a)*650]],col[i],6);}}
  rect(c,110,222,58,55,12,'#bf9671',INK);ellipse(c,167,250,10,28,s.on?'#fff5cb':'#aebcae');text(c,'Mirror',405,370,22,'#d4e6df');text(c,'Prism',s.prismX,s.prismY+90,22,'#d4e6df');break;
 }
 case 'chain':{
  mat(c,'#eef0dd');for(let i=0;i<6;i++){const x=192+i*124,active=s.at===i&&s.phase==='running',done=s.phase==='done'||i<s.at;rect(c,x-49,225,98,185,16,active?'#fff2b7':'#d9e6d4','#b0c7b0');const piece=s.slots[i];
   if(piece==='domino'){c.save();c.translate(x,373);c.rotate(done?1.1:active?s.clock*.9:0);rect(c,-15,-101,30,101,7,'#bd87b4',INK);for(let j=0;j<3;j++)ellipse(c,0,-80+j*25,4,4,'#fff9eb');c.restore();}
   if(piece==='ramp'){line(c,[[x-31,289],[x+33,364]],'#b99261',14);ellipse(c,done?x+30:x-31,done?350:274,14,14,'#72a5c7',INK);}
   if(piece==='bell'){ellipse(c,x,336,32,40,'#e7be67',INK);line(c,[[x-37,370],[x+37,370]],INK,7);if(active||done)text(c,'♪',x,280,35,'#bc8050');}
   if(piece==='gap')text(c,'?',x,330,60,'#9bad99');text(c,String(i+1),x,446,24);if(i<5)arrow(c,x+63,435,0,'#b2c3a8');}
  text(c,s.joined?'Joined to the family machine':'Your own little machine',500,165,27);break;
 }
 }
 c.restore();
}
