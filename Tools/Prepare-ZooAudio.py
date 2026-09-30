"""Prepare licensed Zoo calls and original quiet activity Foley on the PC.

Downloaded previews keep their source-page CC0 credit and checksum. Quiet
animal effects are designed activity sounds, never labelled field recordings.
The four dinosaur voices reuse the separately user-approved book clips.
"""
from pathlib import Path
import hashlib,json,math,random,re,shutil,struct,subprocess,urllib.request,uuid,wave

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'SourceAudio/Zoo'
RUNTIME=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/ZooAudio'
SOURCES=[
 ('elephant','vataaa','148873',3.0),
 ('lion','stratcat322','270383',2.2),
 ('zebra','TheKingOfGeeks360','850661',3.0),
 ('penguin','Breviceps','705839',0.6)]

def checksum(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def foley(name,water=False):
    rate=22050;duration=.85;values=[0.0]*int(rate*duration);rng=random.Random(name)
    for pulse in range(4):
        start=int((.05+pulse*.18)*rate);length=int(.12*rate);smooth=0.0
        for i in range(length):
            t=i/rate;envelope=math.sin(math.pi*i/length)**2
            if water:
                v=math.sin(2*math.pi*(530*t+700*t*t))*math.exp(-t*28)
            else:
                smooth=.76*smooth+.24*rng.uniform(-1,1);v=smooth
            values[start+i]+=v*envelope*.17
    path=OUT/(name+'.wav')
    with wave.open(str(path),'wb') as w:
        w.setparams((1,2,rate,len(values),'NONE','not compressed'))
        w.writeframes(b''.join(struct.pack('<h',round(max(-1,min(1,v))*32767)) for v in values))
    return path

def main():
    (OUT/'sources').mkdir(parents=True,exist_ok=True);RUNTIME.mkdir(parents=True,exist_ok=True);records=[]
    for name,author,sound_id,duration in SOURCES:
        url=f'https://freesound.org/people/{author}/sounds/{sound_id}/'
        html=urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'Mozilla/5.0'}),timeout=30).read().decode()
        if 'creativecommons.org/publicdomain/zero' not in html:raise ValueError('CC0 license missing: '+url)
        media=next(iter(re.findall(r'https://cdn\.freesound\.org/previews/[^"<>\s]+-hq\.mp3',html)))
        source=OUT/'sources'/(name+'.mp3')
        source.write_bytes(urllib.request.urlopen(media,timeout=30).read())
        dest=OUT/(name+'.wav')
        subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(source),'-t',str(duration),'-af','loudnorm=I=-25:TP=-8:LRA=5,afade=t=in:d=0.03,afade=t=out:st='+str(max(.05,duration-.12))+':d=0.12','-ar','22050','-ac','1',str(dest)],check=True)
        records.append(dict(id=name,type='cartoon call' if name=='penguin' else 'recorded animal call',author=author,sourcePage=url,media=media,license='CC0 1.0',licenseUrl='https://creativecommons.org/publicdomain/zero/1.0/',sourceSha256=checksum(source),runtimeSha256=checksum(dest),edit='trim, mono 22050 Hz, loudness target -25 LUFS, peak -8 dB, fades',seconds=duration))
    for name in ['feeding','water','giraffe','tortoise','gecko','iguana','crocodile']:
        path=foley(name,name in ['water','crocodile'])
        records.append(dict(id=name,type='original PC-designed activity Foley',source='Tools/Prepare-ZooAudio.py',license='original project asset',runtimeSha256=checksum(path),description='water bubbles and soft splashes' if name in ['water','crocodile'] else 'quiet rustle/feeding movement; not an animal vocal field recording'))
    for name,index in [('tyrannosaurus',0),('triceratops',1),('stegosaurus',2),('brachiosaurus',3)]:
        path=ROOT/f'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Books/hello-dinosaurs/audio/effect-{index}.wav'
        records.append(dict(id=name,type='user-approved reconstructed dinosaur book call',runtimeResource=f'Books/hello-dinosaurs/audio/effect-{index}',runtimeSha256=checksum(path),approvalManifest='SourceAudio/Books/ApprovedDinosaurCalls/2026-09-30/manifest.json'))
    for path in OUT.glob('*.wav'):
        dest=RUNTIME/path.name;shutil.copyfile(path,dest)
        meta=dest.with_suffix('.wav.meta')
        if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n',encoding='utf-8')
    meta=RUNTIME.with_suffix('.meta')
    if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nfolderAsset: yes\n',encoding='utf-8')
    (OUT/'manifest.json').write_text(json.dumps(dict(records=records,simultaneousSources=2,quietSpecies='Fish and quiet reptiles use activity Foley, without invented field calls.'),indent=2)+'\n',encoding='utf-8')
    print('Prepared four CC0 calls, seven quiet activity clips and four approved dinosaur resource mappings.')

if __name__=='__main__':main()
