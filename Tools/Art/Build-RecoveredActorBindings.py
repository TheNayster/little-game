"""Derive exact scene actor and attachment IDs from the read-only REA inventory."""
import argparse
import json
import importlib.util
from pathlib import Path


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--inspection',type=Path,required=True)
    parser.add_argument('--output',type=Path,required=True)
    parser.add_argument('--guest-wiring',type=Path,help='Optional exact spawner closure; requires --project.')
    parser.add_argument('--project',type=Path)
    parser.add_argument('--prop-strategy',help='Optional exact scene strategy ID; requires --project and dependency inventory.')
    parser.add_argument('--extra-prefab',action='append',default=[],help='Explicit recovered prefab addition; not claimed as a strategy member.')
    args=parser.parse_args()
    actors=json.loads((args.inspection/'actors.json').read_text(encoding='utf-8'))
    actions=json.loads((args.inspection/'actor-action-map.json').read_text(encoding='utf-8'))
    by_id={a['id']:a for a in actions}
    result=[]
    if args.guest_wiring or args.prop_strategy:
        if not args.project:parser.error('Prefab binding requires --project')
        spec=importlib.util.spec_from_file_location('world_inventory',Path(__file__).parents[1]/'Verification/Inspect-RecoveredUnityWorld.py')
        module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
        world=module.World(args.project.resolve())
        owners={}
        if args.guest_wiring:
            wiring=json.loads(args.guest_wiring.read_text(encoding='utf-8'))
            paths=sorted({r['path'] for r in wiring.values() if r['path'].endswith('.prefab') and r['script_type']=='MonoActor'})
        else:
            dependencies=json.loads((args.inspection/'dependencies.json').read_text(encoding='utf-8'))
            for r in dependencies:
                if r['from_id']==args.prop_strategy and r.get('target') and r['target'][0].endswith('.prefab'):owners[r['target'][0]]=r['target'][1]
            paths=sorted(owners)
            if not paths:raise ValueError('No exact prefab dependency found for strategy')
        paths=sorted(set(paths+args.extra_prefab))
        for path in paths:
            rows=list(world.load(path).values())
            owner=next(r for r in rows if r['id']==owners[path])['game_object_id'] if path in owners else next(r for r in rows if r['script_type']=='MonoActor')['game_object_id']
            root=next(r for r in rows if r['id']==owner)
            prefab=next((r['fields'] for r in rows if r['script_type']=='PrefabReferences'),{})
            anchors=next(r['fields'] for r in rows if r['script_type']=='AnchoringComponent')
            ids={e['name']:e['value']['pegTransform'].get('fileID',0) for e in anchors['positions']['entries']}
            result.append({'name':Path(path).stem if args.prop_strategy else root['name'],'prefab':path,'game_object_id':owner,
                'leftId':prefab.get('LeftHandAnchor',{}).get('fileID',0),'rightId':prefab.get('RightHandAnchor',{}).get('fileID',0),
                'faceId':prefab.get('FaceAnchor',{}).get('fileID',0),'holdId':ids.get('hold',0),'baseId':ids.get('base',0)})
        if world.errors:raise ValueError(world.errors)
        args.output.parent.mkdir(parents=True,exist_ok=True)
        args.output.write_text(json.dumps({'actors':result},indent=2),encoding='utf-8')
        print(f'Wrote {len(result)} exact source prefab bindings; no source asset changed.')
        return
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
