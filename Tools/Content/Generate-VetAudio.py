"""Original locally designed parent voice; no actor or family voice reference."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT, resource_path, authoring_path
import os, sys, json, hashlib, subprocess
from pathlib import Path
ROOT=PROJECT_ROOT
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
entries={'Vet': {'welcome': 'Welcome to our animal care clinic! Little dinosaurs and pets are waiting. Choose a friend. Look at the pictures to see what they need. Everyone can help!', 'wash': 'Choose the sponge. Gently rub the muddy spots. See the bubbles? Keep rubbing until each spot is clean.', 'brush': 'Choose the brush. Brush the little tufts or dusty patches gently until they disappear.', 'bandage': 'Clean the mud first. Then drag a bandage onto the little peach patch. There we go!', 'cuddle': 'Choose the heart. Gently stroke or tap your friend for a cuddle. One, two, three!', 'ready': 'Our friend feels lovely! Send them home. Now you can choose another waiting animal for the empty bed.', 'done': 'All eight animals feel cared for! The pets and little dinosaurs are ready to play. You all helped. Would you like another clinic day?'}}
for folder,lines in entries.items():
    records_path=authoring_path('SourceAudio/'+folder)/'manifest.json'
    records_path.parent.mkdir(parents=True,exist_ok=True)
    records=json.loads(records_path.read_text(encoding='utf-8')) if records_path.exists() else []
    for name,text in lines.items():
        if len(sys.argv)>1 and name not in sys.argv[1].split(','):continue
        torch.manual_seed(218)
        print('Rendering '+folder+'/'+name,flush=True)
        waves,sr=model.generate_voice_design(text=text,language='English',instruct=prompt,max_new_tokens=1800)
        wave=np.asarray(waves[0],dtype=np.float32).squeeze()
        assert np.isfinite(wave).all() and .15<len(wave)/sr<30
        raw=WORK/'raw'/(folder.lower()+'-story-'+name+'.wav');sf.write(raw,wave,sr)
        dest=resource_path(folder)/(name+'.wav')
        dest.parent.mkdir(parents=True,exist_ok=True)
        subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(raw),'-af','loudnorm=I=-20:TP=-3:LRA=7','-ac','1','-ar','24000',str(dest)],check=True)
        records=[r for r in records if r['id']!=name]
        records.append(dict(id=name,text=text,model=model_id,prompt=prompt,seed=218,sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),duration=len(wave)/sr,status='generated candidate; family listening pending'))
        print('Done '+name+' '+str(round(len(wave)/sr,2))+'s',flush=True)
    records_path.write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
