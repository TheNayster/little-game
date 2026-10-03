"""Original, deterministic quiet Sandcastle Foley. No downloaded recordings.
Editable synthesis source; emits four mono 22.05 kHz PCM production clips.
"""
import math, random, struct, wave
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Worlds/Daycare/SandcastleClub'
RATE=22050
def make(name,duration):
    rng=random.Random(52);low=0;samples=[]
    for i in range(int(RATE*duration)):
        t=i/RATE;p=t/duration;noise=rng.uniform(-1,1);low=.85*low+.15*noise
        envelope=math.sin(math.pi*p)**1.5
        if name=='scoop':value=(low*.5+noise*.035)*envelope*(.6+.4*math.sin(31*t)**2)
        elif name=='pour':value=(low*.4+math.sin(2*math.pi*(780-200*p)*t)*.025)*envelope
        elif name=='reveal':
            value=sum(math.sin(2*math.pi*f*t)*math.exp(-8*max(0,t-j*.08))*(t>=j*.08) for j,f in enumerate((523.25,659.25,783.99)))*.09*envelope
        else:value=(math.sin(2*math.pi*880*t)*math.exp(-18*t)+math.sin(2*math.pi*1320*t)*math.exp(-24*t))*.12*envelope
        samples.append(max(-.24,min(.24,value)))
    with wave.open(str(OUT/('fx-'+name+'.wav')),'wb') as f:
        f.setparams((1,2,RATE,0,'NONE','not compressed'));f.writeframes(b''.join(struct.pack('<h',int(v*32767)) for v in samples))
    print(name,'seconds',duration,'peak',round(max(map(abs,samples)),3))
if __name__=='__main__':
    for name,duration in (('scoop',.38),('pour',.72),('reveal',.65),('decorate',.23)):make(name,duration)
