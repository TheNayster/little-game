"""Original editable vector reference and soft procedural Zoo visitor Foley."""
from pathlib import Path
import math,random,struct,wave
ROOT=Path(__file__).resolve().parents[2]
source=ROOT/'SourceArt/Zoo/Playable'
source.mkdir(exist_ok=True)
svg='''<svg xmlns="http://www.w3.org/2000/svg" width="520" height="230" viewBox="0 0 520 230">
<g stroke="#425d43" stroke-width="4"><ellipse cx="110" cy="140" rx="26" ry="24" fill="#76bdbc"/><ellipse cx="107" cy="148" rx="13" ry="9" fill="#fae5a8"/><circle cx="119" cy="130" r="3" fill="#26342e"/>
<path d="M132 138l13 5-13 6" fill="#fab858"/></g>
'''
for i in range(5):
 x=60+i*25;y=168-(i%2)*13
 svg+=f'<ellipse cx="{x}" cy="{y}" rx="23" ry="35" transform="rotate({(i-2)*9} {x} {y})" fill="#7aaa52" stroke="#426e42" stroke-width="4"/><path d="M{x} {y-24}v48" stroke="#b8c970" stroke-width="3"/>'
for i in range(5):
 x=310+i*27;y=166-(i%2)*15
 svg+=f'<path d="M{x} 202V{y}" stroke="#52874c" stroke-width="4"/>'
 for j in range(5):
  svg+=f'<ellipse cx="{x+math.cos(j*1.2566)*11}" cy="{y+math.sin(j*1.2566)*11}" rx="9.5" ry="11" fill="{("#e0abbc" if i%2==0 else "#f7d693")}"/>'
 svg+=f'<circle cx="{x}" cy="{y}" r="6" fill="#b87d3b"/>'
for i,c in enumerate(['#f7c973','#b8b0e0','#e8a0ab']):
 x=330+i*47;y=86+i*11
 svg+=f'<g fill="{c}" stroke="#665451" stroke-width="3"><ellipse cx="{x-12}" cy="{y}" rx="12" ry="15"/><ellipse cx="{x+12}" cy="{y}" rx="12" ry="15"/><path d="M{x} {y-14}v28" stroke-width="6"/></g>'
(source/'elephant-surprises.svg').write_text(svg+'</svg>')
audio=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Worlds/Zoo/Audio'
random.seed(7)
for name,duration in [('visitor-chirp',.38),('flower-rustle',.3)]:
 samples=[];phase=0
 for n in range(int(24000*duration)):
  t=n/24000;env=math.sin(math.pi*t/duration)**2
  phase+=2*math.pi*(1750+500*math.sin(t*18))/24000
  value=math.sin(phase)*.16*env if name=='visitor-chirp' else random.uniform(-1,1)*.055*env
  samples.append(struct.pack('<h',int(value*32767)))
 with wave.open(str(audio/(name+'.wav')),'wb') as f:
  f.setparams((1,2,24000,0,'NONE','not compressed'));f.writeframes(b''.join(samples))
