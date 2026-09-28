"""Small original bell cues; no speech, samples, or runtime synthesis."""
from pathlib import Path
import math, wave, struct
ROOT=Path(__file__).resolve().parents[1]
dest=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/HideAndSeek'
dest.mkdir(parents=True,exist_ok=True)
for name,notes in [('tick',[(0,659.25)]),('reveal',[(0,523.25),(.14,659.25),(.28,783.99)])]:
    rate=24000;duration=notes[-1][0]+.65
    with wave.open(str(dest/(name+'.wav')),'wb') as f:
        f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate)
        values=[]
        for n in range(int(duration*rate)):
            t=n/rate;sample=0
            for start,hz in notes:
                age=t-start
                if age>=0:
                    envelope=min(1,age/.012)*math.exp(-age*9)
                    sample+=.23*envelope*(math.sin(2*math.pi*hz*age)+.15*math.sin(4*math.pi*hz*age))
            values.append(struct.pack('<h',int(max(-1,min(1,sample))*32760)))
        f.writeframes(b''.join(values))
