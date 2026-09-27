"""Render original book-voice auditions locally; never changes Unity assets.

Run with the isolated LocalData/AudioStudio/venv interpreter, selecting kokoro
or qwen. Models are cached locally; no family recordings are uploaded.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / 'LocalData/AudioStudio'
os.environ.setdefault('HF_HOME', str(WORK / 'models'))
os.environ.setdefault('HF_HUB_DISABLE_SYMLINKS_WARNING', '1')
TEXT = (
    "Hello, dinosaur explorers! Look, a Triceratops! It has three horns. "
    "One, two, three! Here comes Brontosaurus, with a very long neck. "
    "Tyrannosaurus rex. Spinosaurus. Pteranodon. "
    "Let's listen. What might these amazing animals have sounded like?"
)


def save(name, samples, rate, settings):
    import numpy as np
    import soundfile as sf
    out = ROOT / 'SourceAudio/HomeAuditions/2026-09-26'
    raw = WORK / 'raw'
    out.mkdir(parents=True, exist_ok=True)
    raw.mkdir(parents=True, exist_ok=True)
    wav = np.asarray(samples, dtype=np.float32).squeeze()
    if not np.isfinite(wav).all() or len(wav) < rate:
        raise ValueError('Invalid or unexpectedly short speech')
    sf.write(raw / (name + '.wav'), wav, rate, subtype='FLOAT')
    dest = out / (name + '.wav')
    # A consistent preview level makes voice comparisons fair. This is a digital
    # mastering target, not a guarantee about loudness at a child's ears.
    subprocess.run(['ffmpeg', '-hide_banner', '-loglevel', 'error', '-y', '-i',
        str(raw / (name + '.wav')), '-af', 'loudnorm=I=-20:TP=-3:LRA=7',
        '-ar', '24000', '-ac', '1', '-c:a', 'pcm_s16le', str(dest)], check=True)
    data, sr = sf.read(dest)
    record = dict(id=name, text=TEXT, settings=settings, file=dest.name,
        duration_seconds=round(len(data) / sr, 3), sample_rate=sr,
        sample_peak_dbfs=round(float(20 * np.log10(max(np.abs(data).max(), 1e-12))), 2),
        sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),
        status='audition; family listening acceptance pending')
    revision = WORK / 'models/hub' / ('models--' + settings['model'].replace('/', '--')) / 'refs/main'
    if revision.exists():
        record['settings']['model_revision'] = revision.read_text().strip()
    (out / (name + '.json')).write_text(json.dumps(record, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(record), flush=True)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('engine', choices=['kokoro', 'qwen'])
    args = parser.parse_args()
    import torch
    torch.set_num_threads(4)
    torch.manual_seed(426)
    print(f'{args.engine}: torch={torch.__version__}, cuda={torch.cuda.is_available()}', flush=True)
    if not torch.cuda.is_available():
        raise RuntimeError('Expected the verified RTX 5090; do not silently render on CPU')
    if args.engine == 'kokoro':
        from kokoro import KPipeline
        print('Kokoro imported; loading model', flush=True)
        import numpy as np
        pipeline = KPipeline(lang_code='a', repo_id='hexgrad/Kokoro-82M', device='cuda')
        for voice in ['af_heart', 'af_bella']:
            pieces = [audio.detach().cpu().numpy() for _, _, audio in
                pipeline(TEXT, voice=voice, speed=0.92)]
            save('kokoro-' + voice, np.concatenate(pieces), 24000,
                dict(model='hexgrad/Kokoro-82M', voice=voice, speed=0.92,
                     model_license='Apache-2.0'))
    else:
        from qwen_tts import Qwen3TTSModel
        print('Qwen imported; loading model', flush=True)
        model_id = 'Qwen/Qwen3-TTS-12Hz-1.7B-VoiceDesign'
        model = Qwen3TTSModel.from_pretrained(model_id, device_map='cuda:0',
            dtype=torch.bfloat16, attn_implementation='sdpa')
        designs = {
            'warm-storyteller': 'A warm, clear adult female American English storyteller, '
                'reading a picture book to young children. Friendly, curious, gently expressive. '
                'Natural conversational voice with a smile, distinct words and relaxed pauses. '
                'Medium pitch and unhurried pace. No whispering, shouting or exaggerated baby voice.',
            'friendly-explorer': 'A warm adult male American English museum guide reading a '
                'picture book to young children. Playful curiosity, reassuring and clear, '
                'a light smile and expressive emphasis on dinosaur names. Medium pitch, '
                'gentle steady energy and unhurried pauses. No shouting or theatrical growling.'
        }
        for name, prompt in designs.items():
            torch.manual_seed(426)
            wavs, sr = model.generate_voice_design(text=TEXT, language='English',
                instruct=prompt, max_new_tokens=1800)
            save('qwen-' + name, wavs[0], sr, dict(model=model_id,
                prompt=prompt, seed=426, attention='sdpa', model_license='Apache-2.0'))


if __name__ == '__main__':
    main()
