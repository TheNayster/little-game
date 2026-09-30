"""Author an original seamless soft-water loop and a short float plop."""
from pathlib import Path
import math, random, wave, struct

root=Path(__file__).resolve().parents[1]
source=root/'SourceAudio/Pond'
runtime=root/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/PondArt'
source.mkdir(parents=True,exist_ok=True);runtime.mkdir(parents=True,exist_ok=True)
rate=22050
def save(name,samples):
    data=b''.join(struct.pack('<h',int(max(-1,min(1,v))*32767)) for v in samples)
    for target in (source,runtime):
        with wave.open(str(target/(name+'.wav')),'wb') as out:out.setnchannels(1);out.setsampwidth(2);out.setframerate(rate);out.writeframes(data)

rng=random.Random(32032);count=rate*8;noise=[rng.uniform(-1,1) for _ in range(count)]
# Circular moving averages make a continuous filtered water bed, including
# across the loop boundary. Soft droplets add texture without loud chirps.
def smooth(span):
    total=sum(noise[-span:]);result=[]
    for i in range(count):total+=noise[i]-noise[(i-span)%count];result.append(total/span)
    return result
low=smooth(30);mid=smooth(7)
water=[.45*mid[i]+.23*low[i] for i in range(count)]
for _ in range(130):
    start=rng.randrange(count);duration=rng.uniform(.025,.095);freq=rng.uniform(380,1350);gain=rng.uniform(.006,.024)
    for j in range(int(duration*rate)):
        t=j/rate;envelope=math.sin(math.pi*t/duration)**2*math.exp(-24*t)
        water[(start+j)%count]+=gain*envelope*math.sin(2*math.pi*(freq*t-500*t*t))
save('water',water)
save('bite',[.21*math.sin(math.pi*i/(rate*.13))**2*math.exp(-i/rate*22)*math.sin(2*math.pi*(470*i/rate-1200*(i/rate)**2)) for i in range(int(rate*.13))])
print('Original water and bite audio authored in SourceAudio/Pond and runtime PondArt.')
