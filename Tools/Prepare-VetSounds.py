"""Prepare three credited CC0 living-pet calls; reuse approved dinosaur voices.

Rabbit uses quiet rustle Foley, not a fabricated vocal field recording.
"""
from pathlib import Path
import hashlib,json,re,subprocess,urllib.request,uuid
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'SourceAudio/Vet'; RUNTIME=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Vet'
def main():
    records=[];(OUT/'sources').mkdir(parents=True,exist_ok=True)
    for name,author,sound_id,seconds in [('puppy','ipears1','118072',1.4),('kitten','tuberatanka','110011',1.544),('guinea-pig','RICHERlandTV','435748',1.548)]:
        page=f'https://freesound.org/people/{author}/sounds/{sound_id}/'
        html=urllib.request.urlopen(urllib.request.Request(page,headers={'User-Agent':'Mozilla/5.0'}),timeout=30).read().decode()
        assert 'creativecommons.org/publicdomain/zero' in html, page
        url=re.findall(r'https://cdn\.freesound\.org/previews/[^"<>\s]+-hq\.mp3',html)[0]
        source=OUT/'sources'/(name+'.mp3');source.write_bytes(urllib.request.urlopen(url,timeout=30).read())
        dest=RUNTIME/(name+'.wav')
        subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(source),'-t',str(seconds),'-af',f'loudnorm=I=-25:TP=-8:LRA=5,afade=t=in:d=0.025,afade=t=out:st={seconds-.1}:d=0.1','-ar','22050','-ac','1',str(dest)],check=True)
        Path(str(dest)+'.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n')
        records.append(dict(id=name,author=author,sourcePage=page,license='CC0 1.0',licenseUrl='https://creativecommons.org/publicdomain/zero/1.0/',sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(),runtimeSha256=hashlib.sha256(dest.read_bytes()).hexdigest(),seconds=seconds,edit='mono22.05kHz, quiet loudness normalization, trim and edge fades'))
    (OUT/'pet-calls.json').write_text(json.dumps(dict(records=records,rabbit='Reuses original ZooAudio/tortoise rustle Foley; no invented rabbit vocalization.',dinosaurs='Reuses all four DinosaurWorldAudio assets, without regenerating approved calls.'),indent=2)+'\n')
    print('Prepared dog bark, cat meow and guinea-pig greeting; CC0 sources recorded.')
if __name__=='__main__':main()
