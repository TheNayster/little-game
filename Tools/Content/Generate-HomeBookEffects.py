"""Render original, gentle story effects locally with MMAudio (CC BY-NC 4.0 weights)."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT, authoring_path
import argparse, hashlib, json, os, subprocess
from pathlib import Path

ROOT=PROJECT_ROOT
WORK=ROOT/'LocalData/AudioStudio'
os.environ.setdefault('HF_HOME',str(WORK/'models'))
os.environ.setdefault('HF_HUB_DISABLE_SYMLINKS_WARNING','1')

def main():
    p=argparse.ArgumentParser();p.add_argument('manifest',type=Path);args=p.parse_args()
    entries=json.loads(args.manifest.resolve().read_text())
    # Listening-approved downloads must never be replaced by the old generated
    # prompts when the book's jobs are rebuilt. A missing/corrupt approved asset
    # is an explicit repair, not permission to synthesize a different voice.
    pending=[]
    for entry in entries:
        dest=authoring_path(entry['output']).resolve();meta=dest.with_suffix('.json')
        if not dest.is_relative_to(ROOT) or dest.suffix!='.wav':raise ValueError('Invalid output')
        old=json.loads(meta.read_text()) if meta.exists() else {}
        if old.get('user_approved'):
            if not dest.exists() or hashlib.sha256(dest.read_bytes()).hexdigest()!=old['sha256']:
                raise ValueError('Approved audio needs restoration: '+str(dest))
            print('Keeping approved call: '+dest.name,flush=True)
        else:pending.append(entry)
    entries=pending
    if not entries:return
    import torch, soundfile as sf, numpy as np
    from mmaudio.eval_utils import all_model_cfg,generate
    from mmaudio.model.networks import get_my_mmaudio
    from mmaudio.model.flow_matching import FlowMatching
    from mmaudio.model.utils.features_utils import FeaturesUtils
    torch.set_num_threads(4)
    os.chdir(WORK/'MMAudio')
    config=all_model_cfg['large_44k_v2'];print('Loading local effects model',flush=True);config.download_if_needed()
    net=get_my_mmaudio(config.model_name).to('cuda',torch.bfloat16).eval()
    net.load_weights(torch.load(config.model_path,map_location='cpu',weights_only=True))
    features=FeaturesUtils(tod_vae_ckpt=config.vae_path,synchformer_ckpt=config.synchformer_ckpt,
        enable_conditions=True,mode=config.mode,bigvgan_vocoder_ckpt=config.bigvgan_16k_path,
        need_vae_encoder=False).to('cuda',torch.bfloat16).eval()
    fm=FlowMatching(min_sigma=0,inference_mode='euler',num_steps=25)
    seq=config.seq_cfg;seq.duration=8;net.update_seq_lengths(seq.latent_seq_len,seq.clip_seq_len,seq.sync_seq_len)
    model_hash=hashlib.sha256(config.model_path.read_bytes()).hexdigest()
    for i,e in enumerate(entries):
        dest=authoring_path(e['output']).resolve()
        if not dest.is_relative_to(ROOT) or dest.suffix!='.wav':raise ValueError('Invalid output')
        seed=e.get('seed',9626+i);meta=dest.with_suffix('.json')
        seconds=float(e.get('seconds',4));loudness=float(e.get('loudness',-24));highpass=float(e.get('highpass',0))
        if not 1<=seconds<=8 or not -30<=loudness<=-16 or not 0<=highpass<=200:raise ValueError('Invalid mastering settings')
        if dest.exists() and meta.exists():
            old=json.loads(meta.read_text())
            if (old.get('prompt')==e['prompt'] and old.get('model_sha256')==model_hash and old.get('seed')==seed
                and old.get('seconds',4)==seconds and old.get('loudness',-24)==loudness and old.get('highpass',0)==highpass):continue
        print(f'Rendering {i+1}/{len(entries)}: {dest.name}',flush=True)
        negative='Speech, words, talking, narration, screaming, scary horror, loud harsh distortion, music, singing'
        with torch.inference_mode():
            wave=generate(None,None,[e['prompt']],negative_text=[negative],feature_utils=features,net=net,
                fm=fm,rng=torch.Generator(device='cuda').manual_seed(seed),cfg_strength=4.5)[0].float().cpu().numpy()
        if wave.ndim==2:wave=wave.T
        if not np.isfinite(wave).all():raise ValueError('Invalid audio')
        raw=WORK/'raw'/('effect-'+dest.stem+'.wav');raw.parent.mkdir(exist_ok=True,parents=True);sf.write(raw,wave,44100,subtype='FLOAT')
        dest.parent.mkdir(exist_ok=True,parents=True)
        filters=(f'highpass=f={highpass},' if highpass else '')+f'afade=t=in:d=0.06,afade=t=out:st={seconds-.3}:d=0.3,loudnorm=I={loudness}:TP=-3:LRA=7'
        subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(raw),'-t',str(seconds),
            '-af',filters,
            '-ac','1','-ar','32000','-c:a','pcm_s16le',str(dest)],check=True)
        meta.write_text(json.dumps(dict(prompt=e['prompt'],negative_prompt=negative,seed=seed,model='MMAudio large_44k_v2',
            model_sha256=model_hash,model_license='CC-BY-NC-4.0',source='https://github.com/hkchengrex/MMAudio',
            generated=True,kind='Imaginative sound design; not an authentic extinct-animal recording',
            seconds=seconds,loudness=loudness,highpass=highpass,
            sha256=hashlib.sha256(dest.read_bytes()).hexdigest()),indent=2)+'\n')
        print('Saved '+str(dest),flush=True)

if __name__=='__main__':main()
