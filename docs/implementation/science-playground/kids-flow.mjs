import {slimeReady,powered,lightPath} from './model.mjs';

// Guidance follows the experiment state, including a restored or freely edited state.
// Commands always use the original model; this layer cannot invent a completed result.
export const HINTS={
 lava:'Tap the fizzy tablet. Watch the blobs go up and down.',
 ice:'Tap the dropper, or rub warm water over the ice.',
 iceFree:'You rescued your dinosaur! Drag it around the tray.',
 slimeAdd:'Tip the blue bottle into the glue.',
 slimeMix:'Mix it with the spoon. Your slime is almost ready.',
 slimePlay:'Pull slowly to stretch. Pull quickly to tear!',
 slimeTorn:'Squish the pieces back together.',
 milkColor:'Put a color drop in the milk.',
 milkSoap:'Touch the color with soap. Watch it swirl!',
 marble:'Tap the ball to roll it. Try moving a ramp.',
 robotStart:'Tap the robot to make it draw.',
 robotStop:'Tap the robot to stop. Try a new pen color.',
 robotPaper:'Your drawing is full. Tap the paper for a new one.',
 rocketPump:'Pump some air into your balloon.',
 rocketGo:'Let go! Watch your balloon rocket fly.',
 rocketWatch:'Watch it fly! The air pushes it along.',
 rocketBack:'Bring your balloon back for another go.',
 foamPour:'Pour the starter liquid into the bottle.',
 foamSoap:'Add soap to catch the bubbles.',
 foamYeast:'Tip in the yeast mix. Watch the foam grow!',
 foamWatch:'Look at the foam! The reaction will finish by itself.',
 foamAgain:'Ready for a fresh bottle? Tap start again.',
 windOn:'Turn on the fan. Can your parachute float?',
 windPlay:'Try a bigger parachute, or add a weight.',
 wire:'Tap the battery to connect the ready wires.',
 switch:'Tap the switch to turn on your invention.',
 circuitPlay:'It works! Turn it off, or try the fan.',
 rain:'Tap the cloud. Watch water turn into rain.',
 weatherPlay:'Try the sun, rain, or snow.',
 dip:'Dip the wand into the bubble mixture.',
 blow:'Blow bubbles! Tap a bubble to pop it.',
 lightOn:'Turn on your torch.',
 prism:'Move the triangle into the light.',
 rainbow:'A rainbow! Turn the mirror and try again.',
 chain:'Give the first domino a nudge.',
 chainWatch:'Watch one piece set off the next!',
 chainGap:'There is a gap. Add a piece to join it up.',
 choose:'Pick a picture to play. Your other experiments will stay here.'
};
const button=(id,label,icon,commands,target)=>({id,label,icon,commands,target});
const make=(hint,primary,extras=[])=>({hint,text:HINTS[hint],primary,extras});
const color=s=>button('color','Color','palette',[['color',((s.color||0)+1)%5]]);
const again=()=>button('again','Again','restart',[['reset']]);

export function kidFlow(id,s){
 switch(id){
 case 'lava':return make('lava',button('fizz','Fizz!','tablet',[['tablet']],[793,300,75]),[color(s)]);
 case 'ice':return s.freed?make('iceFree',null,[again()]):make('ice',button('melt','Melt','dropper',[['heat',1],['pour']],[145,200,75]),s.drops===0?[button('dinosaur','Dinosaur','dinosaur',[['toy']])]:[]);
 case 'marble':return make('marble',button('roll','Roll','ball',[['release']],s.ball?.done?[s.ball.x,s.ball.y,55]:[s.tracks[0][0]+18,s.tracks[0][1]-24,45]),[button('slope','Ramp','ramp',[['slope']])]);
 case 'slime':
  if(!s.activator)return make('slimeAdd',button('add','Pour','bottle',[['activator']],[790,190,85]));
  if(!slimeReady(s))return make('slimeMix',button('mix','Mix','spoon',[['stir'],['stir']],[490,345,120]));
  if(s.torn)return make('slimeTorn',button('squish','Squish','slime',[['squish']],[500,345,140]));
  return make('slimePlay',button('stretch','Stretch','stretch',[['slow']],[500+s.stretch*300,350-s.stretch*60,65]),[button('pull','Quick pull','tear',[['quick']]),color(s)]);
 case 'milk':{
  const pos=s.drops.length?{x:s.drops[0].x,y:s.drops[0].y}:{x:500,y:295};
  const drop=button('drop','Color','dropper',[['tool','color'],['color',s.drops.length%5],['touch',{x:410+(s.drops.length%4)*65,y:250+(s.drops.length%3)*45}]]);
  return s.drops.length?make('milkSoap',button('soap','Soap','soap',[['tool','soap'],['touch',pos]],[pos.x,pos.y,80]),[drop]):make('milkColor',drop);
 }
 case 'robot':return s.paths.length>=1200?make('robotPaper',button('paper','New paper','paper',[['paper']],[500,300,150])):make(s.running?'robotStop':'robotStart',button('motor',s.running?'Stop':'Go!',s.running?'stop':'play',[['run']],[s.x,s.y,80]),[color(s)]);
 case 'rocket':
  if(s.phase==='flying')return make('rocketWatch',null);
  if(s.phase==='rest')return make('rocketBack',button('back','Bring back','restart',[['return']],[s.x,270,85]));
  return s.air?make('rocketGo',button('launch','Go!','rocket',[['release']],[145,245,100]),[button('pump','More air','pump',[['pump']])]):make('rocketPump',button('pump','Pump','pump',[['pump'],['pump'],['pump']],[145,245,85]));
 case 'foam':
  if(s.reactant<.001&&s.used)return make('foamAgain',again());
  if(!s.reactant)return make('foamPour',button('pour','Pour','bottle',[['peroxide'],['peroxide']],[190,265,80]));
  if(!s.soap)return make('foamSoap',button('soap','Soap','soap',[['soap']],[810,265,80]));
  if(!s.catalyst)return make('foamYeast',button('yeast','Fizz!','mix',[['yeast'],['yeast']],[810,265,80]));
  return make('foamWatch',null,[color(s)]);
 case 'wind':return make(s.fan?'windPlay':'windOn',button('fan',s.fan?'Fan off':'Fan on','fan',[['fan',s.fan?0:2.5]],[500,540,75]),[button('canopy',s.area===1?'Bigger':'Smaller','parachute',[['area']]),button('weight',s.mass>1?'Lighter':'Heavier','weight',[[s.mass>1?'unweight':'weight']])]);
 case 'circuits':
  if((!powered(s)&&s.closed)||s.wires.length<3)return make('wire',button('wire','Connect','plug',[['loop']],[190,300,65]));
  return make(powered(s)?'circuitPlay':'switch',button('switch',s.closed?'Off':'On','switch',[['switch']],[490,210,80]),[button('output',s.load==='fan'?'Lamp':'Fan',s.load==='fan'?'bulb':'fan',[['load',s.load==='fan'?'lamp':'fan']])]);
 case 'weather':return make(s.rain||s.snow?'weatherPlay':'rain',button('rain','Rain','rain',[['cool',1.5]],[s.x,190,95]),[button('sun','Sun','sun',[['cool',0],['sun',2]]),button('snow','Snow','snow',[['cool',2]])]);
 case 'bubbles':return s.film>.1?make('blow',button('blow','Blow','bubbles',[['blow']],[267,290,65])):make('dip',button('dip','Dip','wand',[['dip']],[240,479,80]));
 case 'light':
  if(!s.on)return make('lightOn',button('on','Light on','torch',[['light']],[140,250,65]));
  if(!lightPath(s).hit)return make('prism',button('prism','Move','prism',[['align']],[s.prismX,s.prismY,75]));
  return make('rainbow',button('mirror','Turn','mirror',[['angle',s.angle>0?-15:20]],[410,250,75]),[button('off','Light off','torch',[['light']])]);
 case 'chain':
  if(s.phase==='running')return make('chainWatch',null);
  if(s.slots.includes('gap'))return make('chainGap',button('bridge','Add piece','domino',[['piece','domino'],['place',s.slots.indexOf('gap')]],[192+s.slots.indexOf('gap')*124,315,55]));
  return make('chain',button('nudge','Go!','domino',[['run']],[192,320,55]),['bell','ramp'].map(piece=>button(piece,piece==='bell'?'Bell':'Ramp',piece,[['piece',piece],['place',Math.max(1,s.slots.findIndex((value,n)=>n>0&&value!==piece))]])));
 }
 throw Error(`Unknown activity: ${id}`);
}

export function performKidAction(world,p,descriptor,act){
 if(!descriptor)return false;
 for(const [kind,value] of descriptor.commands)if(!act(world,p,kind,value))return false;
 return true;
}
