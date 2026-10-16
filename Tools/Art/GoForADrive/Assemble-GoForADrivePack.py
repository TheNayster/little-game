"""Assemble source-bound Go for a Drive media in a new ignored output; BLU-02."""
from pathlib import Path
import json,shutil,hashlib,wave
from PIL import Image
import argparse
p=argparse.ArgumentParser();p.add_argument('--research',type=Path,required=True);p.add_argument('--sprites',type=Path,required=True);p.add_argument('--scenes',type=Path,required=True);p.add_argument('--output',type=Path,required=True);p.add_argument('--layers',type=Path);a=p.parse_args();root=a.research.resolve();base=a.sprites.resolve();out=a.output.resolve();out.mkdir(exist_ok=False);m=json.loads((base/'drive-content.json').read_text());pictures=m['pictures'];assert len({x['key'] for x in pictures})==len(pictures)
for row in pictures:shutil.copyfile(base/row['file'],out/row['file'])
for row in m['actions']:shutil.copyfile(base/row['texture'],out/row['texture'])
for row in json.loads((Path(__file__).resolve().parent/'PictureAliases.json').read_text()):
 original=next(x for x in pictures if Path(x['source']).stem==row['name']);pictures.append(dict(original,key=row['key'],originalKey=original['key'],sourcePathID=str(row['sourcePathID'])))
for i,key in enumerate(['packing-background','driving-background','window-background','building-background'],1):
 file=key+'.png';shutil.copyfile(a.scenes/f'scene{i}-background.png',out/file);pictures.append(dict(key=key,file=file,source=f'Assets/LDBMain/AddressableAssets/Embedded/Playpacks/GoForADrive/Scenes/SUB_SCN_0{i}_GoForADrive_Scene{i}.unity'))
media=json.loads((root/'ContentSources/receipt.json').read_text());audio=[];seen=set();(out/'Audio').mkdir()
for row in media:
 if row['type']!='AudioClip' or row['name'] in seen:continue
 seen.add(row['name']);source=root/'ContentSources'/row['file'];file='Audio/'+row['name']+'.wav';shutil.copyfile(source,out/file)
 with wave.open(str(source)) as w:
  assert w.getsampwidth()==2 and w.getcomptype()=='NONE';duration=w.getnframes()/w.getframerate()
 audio.append(dict(key=row['name'],file=file,sourceBundle=row['bundle'],sourcePathID=str(row['pathID']),duration=duration))
aliases={'music-packing':'mus_gfd_carpacking_loop','music-driving':'mus_gfd_drivetime_loop','music-window':'mus_gfd_windowgames_loop','music-building':'mus_gfd_giantpeanut_loop','music-nomads':'mus_gfd_greynomads_loop','music-layby':'mus_gfd_drivetime_layby_loop','horn':'sfx_gfd_car_horn_01','nomad-horn':'sfx_gfd_drivetime_greynomad_horn_01','star':'sfx_gfd_windowgames_star_collect','jump':'sfx_gfd_windowgames_jump','double-jump':'sfx_gfd_windowgames_double_jump','packing-success':'sfx_gfd_imperfectpacking_confetti','peanut-smash':'sfx_gfd_buildbigthing_peanut_smashed','engine':'sfx_gfd_drivetime_heeler_car_engine_loop','petrol-start':'sfx_gfd_drivetime_petrol_filling_start','petrol-full':'sfx_gfd_drivetime_petrol_gauge_full','cow':'sfx_gfd_drivetime_cow_moo_01'}
for key,original in aliases.items():audio.append(dict(next(x for x in audio if x['key']==original),key=key,originalKey=original))

# Exact source UI palette order and requested functional aliases.
for row in json.loads((Path(__file__).resolve().parent/'Palette.json').read_text()):
 file='palette-'+row['key']+'.png';shutil.copyfile(root/'ContentSources'/row['file'],out/file);pictures.append(dict(key=row['key'],file=file,sourceBundle=row['bundle'],sourcePathID=str(row['pathID']),sourceSprite=row['name']))
for i in range(12):pictures.append(dict(next(x for x in pictures if x['key']=='statue-brick-'+str(i)),key='brick-'+str(i)))
ui={'window-frame':'tex_gfd_Window_Frame','hand-up':'tex_gfd_bluey_hand_up','hand-down':'tex_gfd_bluey_hand','fuel-icon':'tex_gfd_fuel_icon','traffic-red':'tex_gfd_TrafficLight_Red','traffic-green':'tex_gfd_TrafficLight_Green'}
for key,name in ui.items():
 row=next(x for x in media if x['type']=='Sprite' and x['name']==name);file=key+'.png';shutil.copyfile(root/'ContentSources'/row['file'],out/file);pictures.append(dict(key=key,file=file,sourceBundle=row['bundle'],sourcePathID=str(row['pathID'])))
row=next(x for x in m['actions'] if x['actor']=='bluey' and x['action']=='wave');im=Image.open(out/row['texture']);im.crop((0,0,256,256)).save(out/'bluey-wave-icon.png');pictures.append(dict(key='bluey-wave',file='bluey-wave-icon.png',source=row['clip']))
for row in json.loads((root/'ExtraAudio/receipt.json').read_text()):
 file='Audio/'+row['file'];shutil.copyfile(root/'ExtraAudio'/row['file'],out/file)
 with wave.open(str(out/file)) as w:duration=w.getnframes()/w.getframerate()
 audio.append(dict(row,file=file,duration=duration))
requested={'music':'mus_gfd_carpacking_loop','start':'sfx_gfd_imperfectpacking_car_driving_on','travel':'sfx_gfd_drivetime_heeler_car_engine_loop','pack-place':'sfx_emb_brick_click_01','pack-drop':'sfx_emb_brick_drop_01','balance':'sfx_gfd_imperfectpacking_car_leaning_moderately_01','boost':'sfx_gfd_drivetime_heeler_car_prompt_movement_01','stop':'sfx_gfd_drivetime_heeler_car_engine_stop','continue':'sfx_gfd_imperfectpacking_car_driving_off','toilet':'sfx_gfd_drivetime_bluey_urinating','fuel':'sfx_gfd_drivetime_petrol_filling_start','filled':'sfx_gfd_drivetime_petrol_gauge_full','light':'sfx_gfd_drivetime_traffic_light_green','rubble':'sfx_emb_brick_drop_01','window-bump':'sfx_gfd_windowgames_bricks','statue':'sfx_emb_item_connect','smash':'sfx_gfd_buildbigthing_peanut_smashed','replay':'sfx_gfd_buildbigthing_car_reversing','door':'sfx_emb_door_open','wave':'vo_emb_bluey_hi_01'}
for key,original in requested.items():
 if not any(x['key']==key for x in audio):audio.append(dict(next(x for x in audio if x['key']==original),key=key,originalKey=original))
layout=json.loads((a.scenes/'statue-layout.json').read_text());shutil.copyfile(a.scenes/'statue-layout.json',out/'statue-layout.json')
m['statueLayout']=layout;m['paletteCount']=48
m['poseRecipes']={'carry':'Source Walk plus both source HandAngle Mid layers; family carry presentation.','throw':'Source Wave used as a family release gesture; no standalone throw clip recovered.','balance':'Source FreakOut response.','fuel':'Source HoseWashing Adult or HandAngle Right Mid Child.','toilet':'Source GFD child toilet or source adult Sitting.','build':'Source BendFWD for placement presentation.','smash':'Source FreakOut reaction to CAR smash; not a character strike.','driving':'Source GFD Adult DrivingLoop or source child Sitting.'}
m['audioAdaptations']={'boost':'Recovered movement prompt sound as boost feedback.','rubble':'Recovered common brick drop used for moving rubble.','wave':'Recovered Bluey greeting.','music':'Default packing music; activity-specific music aliases retained.'}
assert len({x['key'] for x in pictures})==len(pictures)
assert len({x['key'] for x in audio})==len(audio)

if a.layers:
 layers=json.loads((a.layers/'statue-layout.json').read_text())
 for row in layers['rows']:
  shutil.copyfile(a.layers/row['file'],out/row['file'])
  for key in [row['key'],'brick-'+str(row['index'])]:
   target=next(x for x in pictures if x['key']==key);target.update(file=row['file'],pivot=row['pivot'],sourceGameObjectId=row['sourceGameObjectId'],sourceCameraLayer=True)
  original=next(x for x in layout['rows'] if x['index']==row['index']);original.update(row)
 m['statueLayout']=layout;(out/'statue-layout.json').write_text(json.dumps(layout,indent=2))
for row in pictures:
 image=Image.open(out/row['file']).convert('RGBA');bounds=image.getchannel('A').getbbox()
 if bounds:
  left,top,right,bottom=bounds;row['holdPivot']={'x':(left+right)/2,'y':image.height-(top+bottom)/2};row['gripEvidence']='Family center-at-hand adaptation using alpha bounds; original 3D hole/peg grip not projected.'
m['gripAdaptation']='Picture holdPivot uses the alpha-bounds center in pixels, bottom-origin. These are explicit family presentation anchors; exact original hole/peg connectivity is not restored.'
m.update(pictures=pictures,audio=audio,limits=['Source asset-only reconstruction: original native app was not executed.','Source scenes loaded with main environment additively. Driving/window use five original native scrolling chunks at source11.8-unit spans because original controllers place them at runtime.','Handwritten material recovery approximates original lighting. Original geometry/textures retained; final game appearance and subjective audio need separate checks.','Source clips sampled12fps with CPU skinned meshes. Four recognizable sourceDuplo cast; clip sampling is not the original animation controller.','Hold track uses original right hand anchor; hand and head tracks resolve original rig transform names.'])
(out/'drive-content.json').write_text(json.dumps(m,indent=2)); receipt=[dict(path=f.relative_to(out).as_posix(),sha256=hashlib.sha256(f.read_bytes()).hexdigest(),bytes=f.stat().st_size) for f in sorted(out.rglob('*')) if f.is_file()];(out.parent/(out.name+'-receipt.json')).write_text(json.dumps(receipt,indent=2));print(dict(pictures=len(pictures),uniquePictures=len({x['file'] for x in pictures}),actions=len(m['actions']),audio=len(audio),audioFiles=len(seen),files=len(receipt),bytes=sum(x['bytes'] for x in receipt)))
