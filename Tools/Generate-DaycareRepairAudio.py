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
entries={
 'Adventure':{'step-1':"Our royal picnic is across the river, but the bridge is broken! Three friends are frozen in a pretend spell. Bring food, build a crossing, then distract the playful queen and borrow her wand. Together, we can wake our friends!",'step-2':"First we need food for the feast. Tap a fruit to pick it up, then tap the glowing basket to pack it. Our picnic helper is bringing extra food!",'step-3':"The river blocks our way. Bring the three planks to our bridge builders. They'll help us make a safe crossing!",'step-4':"The queen is guarding her magic wand. She loves a bouncy ball! Toss the ball, then get her wand while she watches it.",'step-5':"The wand can break the pretend spell! Tap each frozen friend to wake them. Let's bring everyone back to our feast.",'step-6':"You brought food, built the bridge and woke our friends! Now everyone can share the royal picnic. Thank you, adventurers!"},
 'Daycare':{'help':"Take a plate from the stack marked Plates. Carry it to a green place at the table and tap to give it to a friend. We'll count together! You can also use the big arrow.",'invite':"Four friends need plates for their picnic. Tap the plate stack to take one, then tap a green place to give it to a friend. Let's count four plates together!"}}
for folder,lines in entries.items():
    records_path=ROOT/'SourceAudio'/folder/'manifest.json'
    records_path.parent.mkdir(parents=True,exist_ok=True)
    records=json.loads(records_path.read_text(encoding='utf-8')) if records_path.exists() else []
    for name,text in lines.items():
        torch.manual_seed(218)
        print('Rendering '+folder+'/'+name,flush=True)
        waves,sr=model.generate_voice_design(text=text,language='English',instruct=prompt,max_new_tokens=1800)
        wave=np.asarray(waves[0],dtype=np.float32).squeeze()
        assert np.isfinite(wave).all() and .15<len(wave)/sr<30
        raw=WORK/'raw'/(folder.lower()+'-repair-'+name+'.wav');sf.write(raw,wave,sr)
        dest=ROOT/'Unity/FamilyPlayset/Assets/FamilyPlayset/Resources'/folder/(name+'.wav')
        subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(raw),'-af','loudnorm=I=-20:TP=-3:LRA=7','-ac','1','-ar','24000',str(dest)],check=True)
        records=[r for r in records if r['id']!=name]
        records.append(dict(id=name,text=text,model=model_id,prompt=prompt,seed=218,sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),duration=len(wave)/sr,status='generated candidate; family listening pending'))
        print('Done '+name+' '+str(round(len(wave)/sr,2))+'s',flush=True)
    records_path.write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
