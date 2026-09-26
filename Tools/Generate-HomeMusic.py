"""Reproduce the original 12-second home radio loop; no sampled/commercial audio."""
from pathlib import Path
import math
import struct
import wave

rate=22050
beat=.375
melody=[60,64,67,69,67,64,62,64,60,64,67,72,69,67,64,62,65,69,72,74,72,69,67,64,67,64,62,59,60,64,67,64]
samples=[0.0]*int(rate*beat*len(melody))
for index,note in enumerate(melody):
    for pitch,gain,duration in [(note,.16,.31),(48+(0 if index<16 else 5 if index<24 else 7),.06,.36)]:
        start=int(index*beat*rate)
        frequency=440*2**((pitch-69)/12)
        for frame in range(int(duration*rate)):
            time=frame/rate
            envelope=min(1,time/.008)*math.exp(-time*11)
            samples[(start+frame)%len(samples)]+=gain*envelope*(math.sin(2*math.pi*frequency*time)+.25*math.sin(2*math.pi*frequency*2*time))
path=Path(__file__).resolve().parents[1]/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/HomeArt/home-music.wav'
with wave.open(str(path),'wb') as output:
    output.setparams((1,2,rate,0,'NONE','not compressed'))
    output.writeframes(b''.join(struct.pack('<h',int(max(-1,min(1,sample))*32767)) for sample in samples))
print(path)
