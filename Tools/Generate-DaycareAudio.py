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
entries={'hello':"Hello! You can play here, hear a story, or help our friends get ready for a picnic.",'story':"A little bird found a seed. Her friends brought water and sunshine. Together they grew a tree, with room for every friend.",'help':"Let's give one plate to each friend. Tap the next empty place, or use the big arrow. You can help at your own pace.",'invite':"Let's set the picnic table together. Give each of our four friends a plate.",'one':"One plate. One friend is ready.",'two':"Two plates. Two friends are ready.",'three':"Three plates. Three friends are ready.",'four':"Four plates. Every friend is ready for our picnic!",'done':"Our picnic is ready! Four plates for four friends. You can play again or go and explore."}
out=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Daycare'
out.mkdir(parents=True,exist_ok=True)
records=[]
for index,(name,text) in enumerate(entries.items()):
    torch.manual_seed(218)
    print('Rendering '+name,flush=True)
    waves,sr=model.generate_voice_design(text=text,language='English',instruct=prompt,max_new_tokens=500)
    wave=np.asarray(waves[0],dtype=np.float32).squeeze()
    assert np.isfinite(wave).all() and .15<len(wave)/sr<20
    raw=WORK/'raw'/('daycare-'+name+'.wav');sf.write(raw,wave,sr)
    dest=out/(name+'.wav')
    subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(raw),'-af','loudnorm=I=-20:TP=-3:LRA=7','-ac','1','-ar','24000',str(dest)],check=True)
    records.append(dict(id=name,text=text,model=model_id,prompt=prompt,seed=218,sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),duration=len(wave)/sr,status='generated candidate; family listening pending'))
source=ROOT/'SourceAudio/Daycare';source.mkdir(parents=True,exist_ok=True)
(source/'manifest.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
