"""Derive exact scene actor and attachment IDs from the read-only REA inventory."""
import argparse
import json
from pathlib import Path


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--inspection',type=Path,required=True)
    parser.add_argument('--output',type=Path,required=True)
    args=parser.parse_args()
    actors=json.loads((args.inspection/'actors.json').read_text(encoding='utf-8'))
    actions=json.loads((args.inspection/'actor-action-map.json').read_text(encoding='utf-8'))
    by_id={a['id']:a for a in actions}
    result=[]
    for actor in actors:
        action=by_id[actor['id']]
        prefab=next((b['configuration'] for b in action['behaviors'] if b['type']=='PrefabReferences'),{})
        anchors=next((b['configuration'] for b in action['behaviors'] if b['type']=='AnchoringComponent'),{})
        hold=next((entry['value']['pegTransform'].get('fileID',0) for entry in anchors.get('positions',{}).get('entries',[]) if entry['name']=='hold'),0)
        anchor_ids={entry['name']:entry['value']['pegTransform'].get('fileID',0) for entry in anchors.get('positions',{}).get('entries',[])}
        result.append({'name':actor['name'],'game_object_id':actor['game_object_id'],
            'leftId':prefab.get('LeftHandAnchor',{}).get('fileID',0) or anchor_ids.get('lefthand',0),
            'rightId':prefab.get('RightHandAnchor',{}).get('fileID',0) or anchor_ids.get('rightHand',0),
            'holdId':hold,'baseId':anchor_ids.get('base',0),
            'faceId':prefab.get('FaceAnchor',{}).get('fileID',0) or anchor_ids.get('face',0)})
    args.output.parent.mkdir(parents=True,exist_ok=True)
    args.output.write_text(json.dumps({'actors':result},indent=2),encoding='utf-8')
    print(f'Wrote {len(result)} exact actor bindings; no source asset changed.')


if __name__=='__main__':
    main()
