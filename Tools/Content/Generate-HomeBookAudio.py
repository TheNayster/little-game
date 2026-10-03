"""Generate the requested book narrator locally using a short speaking reference.

Reference media and the derived speaker prompt remain in ignored LocalData.
Only newly spoken original project scripts are exported as game candidates.
"""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT, authoring_path
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess

ROOT = PROJECT_ROOT
WORK = ROOT / 'LocalData/AudioStudio'
os.environ.setdefault('HF_HOME', str(WORK / 'models'))
os.environ.setdefault('HF_HUB_DISABLE_SYMLINKS_WARNING', '1')
REFERENCE_TEXT = 'And... I have something for you too. I should have given it to you before, but I was just scared.'
REFERENCE_URL = 'https://video.disney.com/watch/tangled-side-by-side-disney-59e8be2ba9091ebd6b4f2a72'
AUDITION = 'Hello, story explorers! Today we can discover dinosaurs, build a little bridge, and visit the moon. Then we can step into a fairy garden, help a princess find a star, and dive into a mermaid adventure. Which book shall we open first?'


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--manifest', type=Path, help='List of original text/output entries; omit for audition')
    args = parser.parse_args()
    import numpy as np
    import soundfile as sf
    import torch
    from qwen_tts import Qwen3TTSModel
    torch.set_num_threads(4)
    if not torch.cuda.is_available():
        raise RuntimeError('Verified NVIDIA GPU is required for this production workflow')
    model_id = 'Qwen/Qwen3-TTS-12Hz-1.7B-Base'
    print('Loading local narrator model', flush=True)
    model = Qwen3TTSModel.from_pretrained(model_id, device_map='cuda:0',
        dtype=torch.bfloat16, attn_implementation='sdpa')
    reference = WORK / 'reference/rapunzel-speaking.wav'
    prompt = model.create_voice_clone_prompt(ref_audio=str(reference),
        ref_text=REFERENCE_TEXT, x_vector_only_mode=False)
    if args.manifest:
        entries = json.loads(args.manifest.read_text(encoding='utf-8'))
    else:
        entries = [dict(text=AUDITION, output='SourceAudio/Home/Auditions/2026-09-26/rapunzel-speaking-candidate.wav')]
    revision = (WORK / 'models/hub' / ('models--' + model_id.replace('/', '--')) / 'refs/main').read_text().strip()
    for i, entry in enumerate(entries):
        dest = authoring_path(entry['output']).resolve()
        if not dest.is_relative_to(ROOT) or dest.suffix != '.wav':
            raise ValueError('Output must be a WAV inside this project')
        meta = dest.with_suffix('.json')
        reference_hash=hashlib.sha256(reference.read_bytes()).hexdigest()
        seed=int(entry.get('seed',426+i))
        tempo=float(entry.get('tempo',.88))
        if dest.exists() and meta.exists():
            previous = json.loads(meta.read_text())
            if (previous.get('text') == entry['text'] and previous.get('model_revision') == revision
                and previous.get('reference_sha256')==reference_hash and previous.get('seed')==seed
                and previous.get('tempo')==tempo and previous.get('reference_text')==REFERENCE_TEXT):
                print('Already rendered: ' + dest.name, flush=True)
                continue
        torch.manual_seed(seed)
        print(f'Rendering {i+1}/{len(entries)}: {dest.name}', flush=True)
        waves, sr = model.generate_voice_clone(text=entry['text'], language='English',
            voice_clone_prompt=prompt, max_new_tokens=1800)
        wav = np.asarray(waves[0], dtype=np.float32).squeeze()
        if not np.isfinite(wav).all() or not 0.25 < len(wav)/sr < 100:
            raise ValueError('Invalid voice render')
        dest.parent.mkdir(parents=True, exist_ok=True)
        raw = WORK / 'raw' / (dest.parent.name + '-' + dest.name)
        raw.parent.mkdir(parents=True, exist_ok=True)
        sf.write(raw, wav, sr, subtype='FLOAT')
        subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(raw),
            '-af',f'atempo={tempo},loudnorm=I=-20:TP=-3:LRA=7','-ac','1','-ar','24000','-c:a','pcm_s16le',str(dest)],check=True)
        data, rate = sf.read(dest)
        record = dict(text=entry['text'], model=model_id, model_revision=revision,
            seed=seed, tempo=tempo, reference_text=REFERENCE_TEXT, reference_url=REFERENCE_URL, reference_start=94.95,
            reference_duration=8.35, reference_sha256=hashlib.sha256(reference.read_bytes()).hexdigest(),
            generated=True, model_license='Apache-2.0', sample_rate=rate,
            duration_seconds=round(len(data)/rate,3),
            peak_dbfs=round(float(20*np.log10(max(np.abs(data).max(),1e-12))),2),
            sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),
            status='Generated candidate; similarity and final listening acceptance are not established by generation')
        meta.write_text(json.dumps(record,indent=2)+'\n',encoding='utf-8')
        print(json.dumps({k:record[k] for k in ['duration_seconds','peak_dbfs','sha256']}),flush=True)


if __name__ == '__main__':
    main()
