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
entries={'Adventure':{
 'step-1':"Welcome to our pretend kingdom! Three friends are frozen beyond the river. The Kindly Queen needs our help. Meet our orchard friend and river helpers, then find out why the other queen took the wand. How will you save the kingdom?",
 'step-2':"Our journey starts in the orchard. Pack some food for the kingdom, or talk to our friend and ask for help. You can explore together!",
 'step-3':"We've reached the river! Our helpers know two ways across. Shall we build a bridge, or hop over the big stepping stones? Talk to a helper and choose.",
 'step-4':"Here is the Greedy Queen! Why did she freeze our friends? Talk to her. You can play ball to borrow the wand, or invite her to our feast.",
 'step-5':"We've found the wand! Our frozen friends need us. Talk to them, then try a sparkle spell or a silly dancing spell to wake them.",
 'talk-castle':"I'm the Kindly Queen in our pretend kingdom! The other queen froze three of our friends. Can you bring them home? Our orchard friend and river helpers know the way.",
 'talk-food':"The kingdom needs food for the feast. You can carry fruit to our basket. Would you like me to help pack while you explore?",
 'talk-river':"The bridge washed away! We can build it with these planks, or hop over the big stepping stones. Which way shall we go?",
 'talk-queen':"I'm the Greedy Queen! I froze everyone because I wasn't invited. Are you here to take my wand? Hmm! I do love playing ball.",
 'talk-rescue':"Brrr! I'm stuck in a pretend spell. You've found the wand! Shall we try a sparkle spell or a silly dancing spell?",
 'talk-frozen':"I'm frozen in a pretend spell! Find the queen's wand beyond the river. Please come back for me!",
 'talk-invited':"You invited me? Thank you! I'll meet everyone at the feast. You may borrow my wand to wake our friends.",
 'talk-helping':"We're helping the kingdom together. Talk to our next friend, or explore and see what you can find!",
 'talk-thanks':"I'm awake! Thank you for coming back for me. Let's take all our friends to the feast!",
 'ending-kind':"You brought food, crossed the river, and invited the lonely queen! Now our friends are awake, and everyone belongs at the feast. You changed our kingdom's story!",
 'ending-ball':"You brought food, crossed the river, and played ball to find the wand! Now our friends are awake, and the kingdom can celebrate. You saved the day!"
}}
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
        subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(raw),'-af','loudnorm=I=-20:TP=-3:LRA=7','-ac','1','-ar','24000',str(dest)],check=True)
        records=[r for r in records if r['id']!=name]
        records.append(dict(id=name,text=text,model=model_id,prompt=prompt,seed=218,sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),duration=len(wave)/sr,status='generated candidate; family listening pending'))
        print('Done '+name+' '+str(round(len(wave)/sr,2))+'s',flush=True)
    records_path.write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
