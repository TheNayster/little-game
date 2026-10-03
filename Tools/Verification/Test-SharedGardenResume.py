"""Verify the preview launcher's saved session route using an existing test run."""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0,str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name=='Tools')))
from project_paths import ROOT as PROJECT_ROOT
import argparse
from pathlib import Path
from shared_garden_runtime import ROOT, Run, read, write, wait, require


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('build',type=int);parser.add_argument('run_id');parser.add_argument('output',type=Path)
    args=parser.parse_args();require(not args.output.exists(),'Use new evidence')
    original=ROOT/'LocalData/SharedGarden'/args.run_id
    result=read(original/'result.json');require(result and result['passed'],'Requires a completed isolated garden test')
    states=[read(p) for p in original.glob('*/view.json')]
    states=[s for s in states if s];expected=max(states,key=lambda s:s['view']['revision'])['view']
    require(all(not t['holder'] for t in expected['toys']),'Resume fixture must have settled props')
    run=Run(args.build,resume=args.run_id)
    try:
        server=run.start('server');first=run.start('client','player-1');second=run.start('client','player-2')
        wait(lambda: first.input('inspect')['visiblePlayers']==2 and second.input('inspect')['visiblePlayers']==2,'both returning profiles drawn')
        require(server.state()['view']==expected and first.state()['view']==expected and second.state()['view']==expected,'Saved preview world changed on reopen')
        args.output.parent.mkdir(parents=True,exist_ok=True)
        write(args.output,dict(passed=True,build=args.build,runId=run.run_id,worldId=expected['worldId'],revision=expected['revision'],sameCompleteWorld=True,twoRenderedProfiles=True,normalSoloSaveAccessed=False))
        print('PASS preview session reopen preserves complete world; '+str(args.output),flush=True)
    finally:run.close()


if __name__=='__main__':main()
