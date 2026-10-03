# Children's narrator research and local voice auditions

**Current implementation:** see the [original six-book phone preview and limits]( home-books-2026-09-26.md). Later user feedback requires full-screen pages, translucent controls, all spoken story words and Read to me; original research details below remain scoped to their earlier production stage.
September 26, 2026. Scope: research appealing children's narrator voices and generate them on the user's PC as part of the complete dinosaur-book task. The user clarified that the voice guidance does not cancel dinosaur effects or book implementation. This report does not mark the reader, books or Home complete.

## What the evidence supports

There is no defensible universal list of AI voices that children like best in the sources reviewed. Model popularity, vendor quality grades and adult listening benchmarks are different from children's preferences. Use evidence to choose delivery qualities, then compare recordings with this family.

A study of 45 preschoolers (average age five) compared expressive and flat robot storytelling while keeping robot movement constant. Expressive delivery produced stronger engagement measures and some richer story retelling. Children reported similar liking in both conditions, so the finding must not be rewritten as proof that children preferred a particular narrator. This supports auditioning natural intonation and emotion, with clear delivery, rather than claiming a winning voice. [Original study, Frontiers in Human Neuroscience](https://www.frontiersin.org/journals/human-neuroscience/articles/10.3389/fnhum.2017.00295/full).

A 2025 conference study involved 14 children aged 5–12 comparing natural and synthetic voices, including matched and unmatched voices. Its reported differences were not statistically significant; the small sample and communication-device context limit generalization to books. It reinforces the need for actual listening choices instead of assuming that a child-like voice, particular gender or matched age will be best. [Flinders University research abstract](https://researchnow.flinders.edu.au/en/publications/childrens-perceptions-of-natural-and-synthetic-voices/).

**Applied production direction:** clear words, gentle expression, a warm and curious delivery, audible pauses between ideas, and enough energy to keep a dinosaur book interesting. Avoid monotonous reading, hurried name lists, exaggerated baby talk and shouting. These are project design choices informed by the studies, not measured universal thresholds.

## Free local models worth comparing

| Candidate | Why it is in the audition | Evidence and limits |
|---|---|---|
| Qwen3-TTS 1.7B VoiceDesign — Warm Storyteller | An original adult female American English narrator, described as warm, curious and relaxed | Description-based voice design and style control; Apache-2.0. The prompt describes the intended result, not a guaranteed listener reaction. [Official model documentation](https://github.com/QwenLM/Qwen3-TTS) |
| Qwen3-TTS 1.7B VoiceDesign — Friendly Explorer | An original adult male American English guide, with playful curiosity and clear emphasis | Same model and excerpt, different voice description. No reference recording or impersonation of a named performer. Use a retained generated reference if later extending the selected identity consistently. [Official voice-design workflow](https://github.com/QwenLM/Qwen3-TTS#voice-design) |
| Kokoro-82M — Heart (`af_heart`) | A readily repeatable stock narrator for comparison | Apache-2.0. Its catalog assigns an A grade based on training/reference quality, not a vote by children. [Official voice catalog](https://huggingface.co/hexgrad/Kokoro-82M/blob/main/VOICES.md) |
| Kokoro-82M — Bella (`af_bella`) | A second stock voice, using identical wording and speed | The catalog gives A− and warns that short utterances can be less reliable. Evaluate standalone dinosaur names separately before producing game clips. [Official voice catalog](https://huggingface.co/hexgrad/Kokoro-82M/blob/main/VOICES.md) |

Chatterbox remains a credible alternative: upstream documents expressive English generation and a multilingual family. It is not established as a children's favorite, and its documented development environment is Python 3.11 on Debian with pinned dependencies. The first comparison uses the two engines above rather than changing the user's working ComfyUI installation to add a third engine. [Chatterbox upstream](https://github.com/resemble-ai/chatterbox).

## How the research is applied

The auditions use the same original passage: a friendly introduction, counting three horns, a long-neck description, Tyrannosaurus rex, Brontosaurus, Triceratops, Spinosaurus and Pteranodon. This exposes warmth, pacing and name clarity instead of judging a voice on a generic greeting. The name list is a pronunciation stress test; the finished book should introduce animals individually.

Kokoro uses speed 0.92, an audition setting rather than a scientifically prescribed reading speed. Qwen receives written direction for relaxed pacing, curiosity, natural emphasis and no whispering/shouting. Both Qwen voices are newly designed, without importing anyone's voice recording. The full text, prompts, seed, model and output hashes are retained beside each WAV.

All generation runs on the Windows PC's verified RTX 5090. A project-local Python environment reuses the existing CUDA PyTorch runtime; speech dependencies and model caches remain under ignored `LocalData/AudioStudio`. ComfyUI's installed packages and workflows are not modified. Generation uses official Python APIs rather than a running ComfyUI graph. No paid speech API is used. Downloaded models need network initially; rendered WAVs play offline.

Compare at equal playback volume. Each audition is exported as 24 kHz mono PCM with a target of −20 LUFS and a −3 dB true-peak ceiling. These are comparison/mastering choices, not guarantees about listening volume at the ears. Do not reward a candidate merely for being louder.

The [local listening page](../../SourceAudio/Home/Auditions/2026-09-26/index.html) presents one voice at a time. Listen for complete words, correct dinosaur names, natural sentence endings, gentle energy and comfortable pauses. Automated decoding, duration and peak checks establish file integrity only; family listening acceptance remains open. No candidate is silently treated as selected or installed in the game.

## Production handoff

The temporary Microsoft Zira book recordings are not the chosen production narrator. Keep them out of a delivered book build. After choosing an audition, retain its exact voice configuration/reference and make per-page narration and deliberate name cues. Listen to every line, correct pronunciations, match levels, and check page changes, pause/replay and offline playback. Game integration must retain independent audio and bookmarks for all four players.

The downstairs shared book location and expanded requested cast remain requirements. T. rex, Brontosaurus, Triceratops, Spinosaurus and pterosaurs must appear; pterosaurs should be identified as flying reptiles. The earlier fixed six-species/eight-page proposal must be revised rather than used to omit these additions. Continue effects, illustrated pages, interactions and qualification as part of the full book process. Effects should be labeled as interpretations of extinct-animal calls, not authenticated recordings.
