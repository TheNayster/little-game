"""Original locally designed parent voice; no actor or family voice reference."""
import os, json, hashlib, subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
WORK=Path(os.environ.get('FAMILY_AUDIO_CACHE',str(ROOT/'LocalData/AudioStudio')))
(WORK/'raw').mkdir(parents=True,exist_ok=True)
os.environ['HF_HOME']=str(WORK/'models')
os.environ['HF_HUB_DISABLE_SYMLINKS_WARNING']='1'
import numpy as np
import soundfile as sf
import torch
from qwen_tts import Qwen3TTSModel
torch.set_num_threads(4)
assert torch.cuda.is_available()
model_id='Qwen/Qwen3-TTS-12Hz-1.7B-VoiceDesign'
model=Qwen3TTSModel.from_pretrained(model_id,device_map='cuda:0',dtype=torch.bfloat16,attn_implementation='sdpa')
prompt='An original friendly adult female Australian English storyteller, warm, clear and gentle, inviting preschool children into pretend play. Natural medium pitch and an encouraging smile. No imitation of an actor or character, no music, no shouting.'
entries={'step-1':"Our kingdom needs a picnic! Let's help together.",'step-2':"Tap the fruit to gather our picnic supplies.",'step-3':"Tap the planks to open our bridge.",'step-4':"Toss the magic ball. While the queen watches it, take her wand.",'step-5':"Tap our frozen friends to wake them with the wand.",'step-6':"You saved the kingdom! Let's have a picnic together."}
out=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Adventure'
out.mkdir(parents=True,exist_ok=True)
records=[]
for index,(name,text) in enumerate(entries.items()):
    torch.manual_seed(218)
    print('Rendering '+name,flush=True)
    waves,sr=model.generate_voice_design(text=text,language='English',instruct=prompt,max_new_tokens=500)
    wave=np.asarray(waves[0],dtype=np.float32).squeeze()
    assert np.isfinite(wave).all() and .15<len(wave)/sr<20
    raw=WORK/'raw'/('adventure-'+name+'.wav');sf.write(raw,wave,sr)
    dest=out/(name+'.wav')
    subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(raw),'-af','loudnorm=I=-20:TP=-3:LRA=7','-ac','1','-ar','24000',str(dest)],check=True)
    records.append(dict(id=name,text=text,model=model_id,prompt=prompt,seed=218,sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),duration=len(wave)/sr,status='generated candidate; family listening pending'))
source=ROOT/'SourceAudio/Adventure';source.mkdir(parents=True,exist_ok=True)
(source/'manifest.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
