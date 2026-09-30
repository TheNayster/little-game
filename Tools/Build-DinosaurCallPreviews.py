"""Make standalone dinosaur call auditions. Never writes Unity or installed book audio."""
import base64, hashlib, html, json, subprocess, wave
from pathlib import Path
import numpy as np

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'SourceAudio/Books/DinosaurPreviews/2026-09-30'
RATE=32000

def decode(path):
    data=subprocess.check_output(['ffmpeg','-v','error','-i',str(path),'-ac','1','-ar',str(RATE),'-f','f32le','-'])
    return np.frombuffer(data,dtype='<f4').copy()

def fragment(source,start,end,pitch=1):
    x=decode(OUT/'sources'/source)[int(start*RATE):int(end*RATE)]
    # Resampling preserves the source vocal articulation while changing size/pitch.
    x=np.interp(np.arange(0,len(x)-1,pitch),np.arange(len(x)),x)
    # Remove sub-bass and hiss without generating noise or replacing the animal voice.
    spectrum=np.fft.rfft(x);freq=np.fft.rfftfreq(len(x),1/RATE)
    spectrum*=np.minimum(1,(freq/80)**2)*np.minimum(1,(5000/np.maximum(freq,1))**4)
    x=np.fft.irfft(spectrum,n=len(x))
    fade=min(int(.045*RATE),len(x)//3);x[:fade]*=np.linspace(0,1,fade);x[-fade:]*=np.linspace(1,0,fade)
    return x/max(float(np.sqrt(np.mean(x*x))),.001)*.12

def render(job):
    x=np.zeros(int(job['seconds']*RATE))
    for layer in job['layers']:
        signal=fragment(layer['source'],layer['start'],layer['end'],layer.get('pitch',1))*layer['gain']
        offset=int(layer.get('at',0)*RATE);size=min(len(signal),len(x)-offset)
        x[offset:offset+size]+=signal[:size]
    if job.get('horn'):
        # A stylized audible horn, inspired by crest resonance; not a fossil simulation.
        n=int(2.55*RATE);t=np.arange(n)/RATE
        pitch=130+22*np.sin(np.pi*np.minimum(t/2.2,1));phase=2*np.pi*np.cumsum(pitch)/RATE
        envelope=np.minimum(t/.18,1)*np.minimum(np.maximum(2.55-t,0)/.35,1)
        signal=sum(np.sin(phase*k)*gain for k,gain in [(1,.16),(2,.10),(3,.05),(5,.018)])*envelope
        x[:n]+=signal
    # Gentle peak control; level matching is separate from subjective acceptance.
    x=np.tanh(x*1.25)
    rms=float(np.sqrt(np.mean(x*x)));x*=min(.12/max(rms,.001),.76/max(float(np.max(np.abs(x))),.001))
    tail=min(int(.12*RATE),len(x));x[-tail:]*=np.linspace(1,0,tail)
    assert np.isfinite(x).all() and np.max(np.abs(x))<1 and np.sqrt(np.mean(x*x))>.02
    path=OUT/(job['id']+'.wav')
    with wave.open(str(path),'wb') as wav:
        wav.setnchannels(1);wav.setsampwidth(2);wav.setframerate(RATE);wav.writeframes((x*32767).astype('<i2').tobytes())
    subprocess.run(['ffmpeg','-v','error','-y','-i',str(path),'-codec:a','libmp3lame','-b:a','128k',str(path.with_suffix('.mp3'))],check=True)
    return dict(id=job['id'],species=job['species'],seconds=len(x)/RATE,peak=float(np.max(np.abs(x))),rms=float(np.sqrt(np.mean(x*x))),sha256=hashlib.sha256(path.read_bytes()).hexdigest())

def main():
    jobs=json.loads((OUT/'design.json').read_text(encoding='utf-8'))
    results=[render(job) for job in jobs]
    (OUT/'asset-checks.json').write_text(json.dumps(dict(previewOnly=True,installed=False,listeningAccepted=False,checks=results),indent=2)+'\n')
    cards=[]
    for job in jobs:
        encoded=base64.b64encode((OUT/(job['id']+'.mp3')).read_bytes()).decode()
        cards.append('<article><h2>'+html.escape(job['species'])+'</h2><p>'+html.escape(job['direction'])+'</p><audio controls preload="none" src="data:audio/mpeg;base64,'+encoded+'"></audio></article>')
    page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Dinosaur sound previews</title><style>body{font:18px system-ui;background:#f4f7ef;color:#18372c;margin:0;padding:24px}main{max-width:960px;margin:auto}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(270px,1fr));gap:16px}article{background:white;border-radius:18px;padding:20px;box-shadow:0 4px 16px #18372c10}h2{font-size:22px}audio{width:100%}p{line-height:1.5}a{color:#245c46}</style><main><h1>Dinosaur sound previews</h1><p>Twelve audition drafts made from free recorded animal calls and artist-made effects. These are imagined dinosaur voices, not authentic extinct-animal recordings. Nothing in this preview has been added to the app.</p><div class="grid">'''+''.join(cards)+'''</div><p>Source credits, licenses and design choices: <a href="research-and-sources.json">research-and-sources.json</a>. Listening approval is pending.</p></main><script>document.addEventListener('play',e=>{for(const a of document.querySelectorAll('audio'))if(a!==e.target)a.pause()},true)</script></html>'''
    (OUT/'listen.html').write_text(page,encoding='utf-8')
    print('Made '+str(len(results))+' preview calls: '+str(OUT/'listen.html'))

if __name__=='__main__':main()
