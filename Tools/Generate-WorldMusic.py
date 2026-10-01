"""Render six original chamber-pop loops with sampled acoustic instruments.

No game-time synth or network service. Install no global tools: pass the local
FluidSynth 2.6.1 executable and GeneralUser GS 2.0.3 font explicitly.
The MIDI score is the editable source; Unity imports the lossless loop masters.
"""
import argparse, hashlib, json, math, random, struct, subprocess, wave
from pathlib import Path
from array import array

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'SourceAudio/WorldMusic'
OUTPUT = ROOT / 'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/WorldMusic'
FONT_HASH = '9575028c7a1f589f5770fccc8cff2734566af40cd26ed836944e9a5152688cfe'
PPQ = 480
# Degrees of C major, shifted as a whole to each track's key. Each bar has room
# to breathe; the second half answers the first instead of looping four notes.
TUNES = {
    'home': (100, 0, 0, 71, 'Little Everyday Adventures', [
        '2:1 4:.5 5:.5 4:1 r:1', '2:1 1:.5 0:.5 1:1 r:1',
        '0:.5 2:.5 3:1 5:1 4:1', '3:1 1:1 4:1 r:1',
        '2:1 4:.5 6:.5 5:1 4:1', '2:.5 1:.5 0:1 2:1 r:1',
        '3:1 2:.5 1:.5 0:1 1:1', '1:1 4:1 0:1 r:1']),
    'yard': (112, 5, 24, 11, 'Sunshine in the Grass', [
        '0:.5 2:.5 4:1 r:.5 2:.5 5:1', '4:1 2:.5 1:.5 2:1 r:1',
        '3:.5 5:.5 7:1 5:1 3:1', '4:.5 3:.5 1:1 r:1 1:1',
        '2:.5 4:.5 5:1 7:1 r:1', '6:1 5:.5 4:.5 2:1 r:1',
        '3:.5 2:.5 0:1 3:.5 2:.5 1:1', '1:1 4:1 0:1 r:1']),
    'park': (116, 2, 71, 45, 'Follow the Little Path', [
        '4:.5 2:.5 0:1 2:.5 4:.5 5:1', '4:1 2:1 r:1 1:1',
        '0:.5 3:.5 5:1 3:.5 2:.5 0:1', '1:1 3:.5 4:.5 1:1 r:1',
        '4:1 5:.5 7:.5 6:1 5:1', '4:.5 2:.5 0:1 2:1 r:1',
        '3:1 5:.5 3:.5 2:1 0:1', '1:1 4:.5 1:.5 0:1 r:1']),
    'creek': (86, 7, 73, 0, 'Pebbles and Ripples', [
        '2:1.5 4:.5 5:1 r:1', '4:2 2:1 r:1',
        '3:1 5:1 7:1 5:1', '4:1.5 3:.5 1:1 r:1',
        '2:1 4:1 7:1 r:1', '5:1.5 4:.5 2:1 r:1',
        '3:1 2:1 0:1 r:1', '1:1 4:1 0:1 r:1']),
    'beach': (104, 0, 24, 73, 'Seashell Skipping', [
        '4:1 r:.5 2:.5 0:1 2:1', '4:.5 5:.5 4:1 2:1 r:1',
        '3:1 r:.5 5:.5 7:1 5:1', '4:.5 3:.5 1:1 4:1 r:1',
        '2:.5 4:.5 7:1 5:.5 4:.5 2:1', '4:1 2:.5 1:.5 0:1 r:1',
        '3:1 5:1 3:.5 2:.5 0:1', '1:1 4:1 0:1 r:1']),
    'daycare': (116, 0, 11, 73, 'Sunny Daycare Play', [
        '0:.5 2:.5 4:.5 r:.5 5:.5 4:.5 2:.5 r:.5', '2:.5 1:.5 0:1 2:.5 4:.5 r:1',
        '3:.5 5:.5 7:.5 r:.5 5:1 3:1', '4:.5 6:.5 8:.5 6:.5 4:1 r:1',
        '2:.5 4:.5 5:.5 r:.5 7:.5 5:.5 4:1', '4:.5 2:.5 0:1 2:1 r:1',
        '3:.5 5:.5 7:1 5:.5 3:.5 r:1', '4:.5 2:.5 1:.5 r:.5 0:2']),
    'treasure': (122, 5, 11, 45, 'The Windblown Map', [
        '0:.5 2:.5 4:1 5:.5 4:.5 r:1', '2:.5 4:.5 7:1 4:.5 2:.5 r:1',
        '3:.5 5:.5 7:.5 5:.5 3:1 r:1', '4:.5 6:.5 8:1 6:.5 4:.5 r:1',
        '0:.5 2:.5 4:.5 r:.5 7:1 5:1', '4:.5 2:.5 0:.5 2:.5 4:1 r:1',
        '3:.5 5:.5 7:1 5:.5 3:.5 r:1', '4:.5 2:.5 1:.5 r:.5 0:2'])}

def vlq(n):
    data = [n & 127]
    while (n := n >> 7): data.insert(0, (n & 127) | 128)
    return bytes(data)

def midi(path, bpm, transpose, lead, answer, bars, seed, bright=False):
    rng = random.Random(seed); events = []
    def event(beat, data): events.append((round(beat * PPQ), data))
    def note(ch, key, beat, length, velocity):
        # Fixed-seed touch variation repeats each complete score, keeping the
        # middle rendered cycle exactly loopable, including reverb release.
        beat = max(0, beat + rng.uniform(-.012, .012))
        event(beat, bytes([0x90 | ch, key, max(1, min(127, velocity + rng.randrange(-3, 4)))]))
        event(beat + length, bytes([0x80 | ch, key, 0]))
    event(0, b'\xff\x51\x03' + int(60000000 / bpm).to_bytes(3, 'big'))
    for ch, program, volume, pan in [(0,lead,89,57),(1,answer,70,72),(2,24,68,43),(3,45,62,84),(4,32,71,64),(5,48,43,58)]:
        event(0, bytes([0xc0 | ch, program]))
        for controller, value in [(7,volume),(10,pan),(91,24),(93,5)]:event(0,bytes([0xb0|ch,controller,value]))
    event(0, bytes([0xc9, 0]))
    degrees = [0,2,4,5,7,9,11]
    def pitch(d): return 60 + transpose + degrees[d % 7] + 12*(d//7)
    progression = [(0,2,4),(5,7,9),(3,5,7),(4,6,8),(0,2,4),(5,7,9),(3,5,7),(4,6,8)]
    if bright:progression=[(0,2,4),(0,2,4),(3,5,7),(4,6,8),(0,2,4),(0,2,4),(3,5,7),(0,2,4)]
    score_start = rng.getstate()
    for cycle in range(3):
        rng.setstate(score_start)
        for bar in range(32):
            start = cycle*128 + bar*4; section = bar//8; chord=progression[bar%8]
            soft = -7 if section==2 else 0
            # A, answering A, quieter B, and a returning A with a soft countermelody.
            source_bar=(bar%8+4)%8 if section==2 else bar%8
            pos=0
            for token in bars[source_bar].split():
                degree, length=token.split(':');length=float(length)
                if degree!='r':
                    key=pitch(int(degree))
                    ch=1 if section==1 else 0
                    note(ch,key,start+pos,length*(.64 if bright else .84),67+soft)
                pos+=length
            assert abs(pos-4)<.001
            bass=pitch(chord[0])-24
            while bass<36:bass+=12
            note(4,bass,start,1.65,58+soft);note(4,bass+7,start+2,1.55,48+soft)
            for beat, d in [(0,chord[0]),(.75,chord[1]),(1.5,chord[2]),(2.5,chord[1]),(3.25,chord[2])]:
                key=pitch(d)
                while key>72:key-=12
                note(2,key,start+beat,.66,49+soft)
            if section!=2:
                for beat in [1,3]:
                    for d in chord:note(3,pitch(d),start+beat,.4,40)
            if section in [2,3] and not bright:
                for d in chord:note(5,pitch(d),start+.08,3.8,37)
            if section==3 and bar%2==1:
                for beat,d in [(2,chord[2]),(2.5,chord[1]),(3,chord[0])]:note(1,pitch(d)+12,start+beat,.4,38)
            if seed!=3:
                for beat in [0,1,2,3]:note(9,82,start+beat,.12,30 if beat%2 else 24)
                for beat in [1,3]:note(9,37,start+beat,.1,26)
            else:
                note(9,54,start+3,.1,19)
    event(384,b'\xff\x2f\x00')
    data=bytearray();last=0
    for tick, msg in sorted(events, key=lambda e:(e[0],e[1])):
        data.extend(vlq(tick-last));data.extend(msg);last=tick
    path.write_bytes(b'MThd'+struct.pack('>IHHH',6,0,1,PPQ)+b'MTrk'+struct.pack('>I',len(data))+data)

def main():
    p=argparse.ArgumentParser();p.add_argument('--synth',type=Path,required=True);p.add_argument('--font',type=Path,required=True);p.add_argument('--tracks',default='');args=p.parse_args()
    assert hashlib.sha256(args.font.read_bytes()).hexdigest()==FONT_HASH
    SOURCE.mkdir(parents=True,exist_ok=True);OUTPUT.mkdir(parents=True,exist_ok=True)
    temp=ROOT/'LocalData/MusicStudio/renders';temp.mkdir(parents=True,exist_ok=True);manifest_path=SOURCE/'manifest.json';tracks=json.loads(manifest_path.read_text(encoding='utf-8'))['tracks'] if args.tracks and manifest_path.exists() else []
    for seed,(key,(bpm,transpose,lead,answer,title,bars)) in enumerate(TUNES.items()):
        if args.tracks and key not in args.tracks.split(','):continue
        score=SOURCE/(key+'.mid');raw=temp/(key+'.wav');out=OUTPUT/(key+'.wav')
        midi(score,bpm,transpose,lead,answer,bars,seed,bright=key in ('daycare','treasure'))
        subprocess.run([str(args.synth),'-ni','-q','-r','44100','-g','.6','-o','synth.reverb.room-size=.32','-o','synth.reverb.damp=.4','-o','synth.reverb.level=.18','-o','synth.chorus.active=0','-F',str(raw),str(args.font),str(score)],check=True,capture_output=True)
        with wave.open(str(raw),'rb') as w:
            assert w.getsampwidth()==2 and w.getnchannels()==2
            frames=round(128*60/bpm*44100);w.setpos(frames);samples=array('h',w.readframes(frames))
        rms=math.sqrt(sum(float(s)*s for s in samples)/len(samples));peak=max(abs(s) for s in samples)
        gain=min((32767*10**(-22/20))/rms,(32767*.79)/peak)
        samples=array('h',(round(s*gain) for s in samples))
        # Sampled instruments have free-running modulation. A 5 ms boundary
        # ramp removes phase discontinuities without shortening the bar grid.
        ramp=221
        for i in range(ramp):
            weight=math.sin(i/(ramp-1)*math.pi/2)
            for ch in range(2):
                samples[2*i+ch]=round(samples[2*i+ch]*weight)
                samples[-2*(i+1)+ch]=round(samples[-2*(i+1)+ch]*weight)
        with wave.open(str(out),'wb') as w:w.setparams((2,2,44100,0,'NONE','not compressed'));w.writeframes(samples.tobytes())
        seam=max(abs(samples[n]-samples[-2+n]) for n in range(2))/32768
        assert seam<.02,'Loop discontinuity '+key
        track=dict(id=key,title=title,bpm=bpm,bars=32,seconds=frames/44100,sha256=hashlib.sha256(out.read_bytes()).hexdigest(),peak=max(abs(s) for s in samples)/32768,rms_db=20*math.log10(rms*gain/32768),seam=seam)
        tracks=[t for t in tracks if t['id']!=key];tracks.append(track);print(json.dumps(track),flush=True)
    (SOURCE/'manifest.json').write_text(json.dumps(dict(renderer='FluidSynth 2.6.1',font='GeneralUser GS 2.0.3',font_sha256=FONT_HASH,tracks=tracks),indent=2)+'\n')

if __name__=='__main__':main()
