"""Original locally designed parent voice; no actor or family voice reference."""
import os, sys, json, hashlib, subprocess
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
entries={'Sandpit': {'count': 'One scoop. Two scoops! Our small bucket is full.', 'water': 'A little water helps the sand stick together.', 'tip': 'Press, turn, lift! A sand tower!', 'play': 'Your turn! The small bucket takes two scoops. The big bucket takes three. Add a little water, then turn it over. Let us build one castle together!', 'crumble': 'The dry sand crumbled! That is okay. Fill it again and add a little water before you turn it over.', 'done': 'One, two, three, four towers! You built our castle together. Which bucket held more sand? Now you can add flags and shells, or make another castle.'}}
for folder,lines in entries.items():
    records_path=ROOT/'SourceAudio'/folder/'manifest.json'
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
        dest=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources'/folder/(name+'.wav')
        dest.parent.mkdir(parents=True,exist_ok=True)
        subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(raw),'-af','loudnorm=I=-20:TP=-3:LRA=7','-ac','1','-ar','24000',str(dest)],check=True)
        records=[r for r in records if r['id']!=name]
        records.append(dict(id=name,text=text,model=model_id,prompt=prompt,seed=218,sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),duration=len(wave)/sr,status='generated candidate; family listening pending'))
        print('Done '+name+' '+str(round(len(wave)/sr,2))+'s',flush=True)
    records_path.write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
