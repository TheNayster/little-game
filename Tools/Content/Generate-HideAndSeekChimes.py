"""Small original bell cues; no speech, samples, or runtime synthesis."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
from pathlib import Path
import math, wave, struct
ROOT=PROJECT_ROOT
dest=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Worlds/Home/HideAndSeek'
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
