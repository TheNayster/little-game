"""Stage 2: one isolated authority/four native clients, plus real picture taps.
Requires a freshly built schema50/content68 artifact matching current task source.
Never launches the installed family server or opens a real save.
"""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0, str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name == "Tools")))
from project_paths import ROOT as PROJECT_ROOT
import argparse, copy, hashlib, importlib.util, json, time
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
from shared_garden_runtime import Run, wait, require, read, write
spec = importlib.util.spec_from_file_location("home", Path(__file__).with_name("Test-HomeWorld.py"))
home = importlib.util.module_from_spec(spec); spec.loader.exec_module(home)

def main():
    parser = argparse.ArgumentParser(); parser.add_argument("build", type=int)
    parser.add_argument("--remaining-only", action="store_true", help="Reuse organization departure/reconnect evidence; check missing construction/input/migrated visuals")
    parser.add_argument("--cap-capture", metavar="OWN_RUN_UUID", help="Only check full-pit feedback in this script's completed synthetic run")
    parser.add_argument("--different-pieces", metavar="OWN_RUN_UUID", help="Only check concurrent contributions to distinct pieces in the completed synthetic run")
    args = parser.parse_args()
    require(not (args.cap_capture and args.different_pieces), "choose one targeted fixture check")
    run = Run(args.build, extended_test_lifetime=True, resume=args.cap_capture or args.different_pieces)
    require(run.content == 68 and read(run.folder / "build-summary.json")["schema"] == 50, "Stage 2 source required; never substitute an older build")
    manifest = read(run.folder / "source-manifest.json")
    files = {f["path"]: f["sha256"] for f in manifest["files"]}
    for name in ("Core/Worlds/Daycare/DaycareSandpit.cs", "Core/Worlds/Dinosaur/DinosaurWorld.cs", "Core/Shared/Layout/WorldLayout.cs", "Client/Shared/Sessions/GameScreen.cs", *["Client/Worlds/Daycare/"+n+".cs" for n in ("GameScreen.SandcastleClub", "GameScreen.SandcastleWater", "GameScreen.SandcastleTip", "GameScreen.SandcastleDecoration", "SandShape", "SandScoopFeedback", "SandWaterFeedback", "SandTipFeedback", "SandDecorationFeedback")]):
        relative = "Unity/FamilyPlayset/Assets/FamilyPlayset/Code/" + name
        require(files.get(relative) == hashlib.sha256((PROJECT_ROOT / relative).read_bytes()).hexdigest(), "Build does not contain current " + name)
    out = run.path / ("sandpit-parallel" if args.different_pieces else "sandpit-cap" if args.cap_capture else "sandpit-stage-two"); out.mkdir(); checks = []; clients = []; passed = False
    print("EVIDENCE " + str(out), flush=True)
    def state(): return server.state()["view"]["sandpit"]
    def member(v): return next(m for m in state()["members"] if m["actor"] == v.profile)
    def piece(id): return next(m for m in state()["moulds"] if m["id"] == id)
    def record(name): checks.append(name); print("PASS " + name, flush=True)
    def button(v, name):
        # Daycare's scroll list has more cards than fit on a phone screen.
        for _ in range(8):
            info = v.input("inspect")
            if any(c["name"] == name and c["enabled"] for c in info["controls"]): break
            if not info["menuOpen"]: break
            x,y = info["screenWidth"]*.55,info["screenHeight"]*.37
            v.input("touch-begin", role="screen", x=x, y=y, finger=97)
            for dy in (30,65,110,160): v.input("touch-move", role="screen", x=x, y=y+dy, finger=97)
            v.input("touch-end", role="screen", x=x, y=y+160, finger=97)
            time.sleep(.15)
        wait(lambda: any(c["name"] == name and c["enabled"] for c in v.input("inspect")["controls"]), "enabled " + name, 20)
        v.input("touchButton", text=name)
    def command(v, action, **kw): return home.command(v, action, **kw)
    def move(v, id):
        m = piece(id); require(command(v, 0, x=m["x"]-60, y=m["y"]-55)["accepted"], "work position")
    def tool(v, id, op):
        return command(v, 32, target=id+"@"+str(state()["round"]), value=op)
    def place_ui(v, cell):
        old = {m["id"] for m in state()["moulds"]}
        button(v, "Build"); button(v, "Sand spot "+str(cell))
        home.capture(v, out, "preview-"+v.profile+"-"+str(cell))
        button(v, "Confirm sand placement")
        wait(lambda: len(state()["moulds"]) > len(old), "placed round mould")
        id = next(m["id"] for m in state()["moulds"] if m["id"] not in old)
        wait(lambda: (not (info:=v.input("inspect"))["pending"] and any(m["id"]==id for m in info["sandpit"]["moulds"]) and not any(c["name"]=="Confirm sand placement" for c in info["controls"])), "client placement confirmation settled")
        return id
    def layout(v, name):
        info = home.ready(v)
        require(not info["pending"], "capture must show settled gameplay")
        require({"Build","Scoop","Water","Tip","Leave sandpit","Optional teacher help"} <= {c["name"] for c in info["controls"]}, "missing settled essential controls")
        for control in info["controls"]:
            if control["name"] in ("Build", "Scoop", "Water", "Tip", "Leave sandpit"):
                require(control["bounds"]["width"] >= 50 and control["bounds"]["height"] >= 44, "small essential control "+control["name"])
                r, safe = control["bounds"], info["safeArea"]
                require(r["x"] >= safe["x"]-2 and r["y"] >= safe["y"]-2 and r["x"]+r["width"] <= safe["x"]+safe["width"]+2 and r["y"]+r["height"] <= safe["y"]+safe["height"]+2, "essential control leaves safe area")
        home.capture(v, out, name)
    def legacy_captures():
        # Only this Run's synthetic checkpoint is edited, with all writers stopped.
        run.close()
        checkpoint = run.path / "server-world/world.save"
        header, digest, body = checkpoint.read_bytes().split(b"\n", 2)
        require(header == b"LITTLEWEEPS-SOLO-1" and hashlib.sha256(body).hexdigest().encode() == digest, "synthetic checkpoint digest")
        snapshot = json.loads(body); fixture = copy.deepcopy(snapshot)
        fixture["schema"] = 49; g = fixture["sandpit"]
        g.update(format=0, pieceLimit=0, scoopCapacity=0, phase=2, round=7)
        for m in g["members"]: m.update(attending=False, declined=False)
        g["moulds"] = [dict(scoops=2,wet=True,built=True,decoration=1),dict(scoops=3,wet=True,built=True,decoration=2),dict(scoops=1,wet=False,built=False,decoration=0),dict(scoops=2,wet=True,built=False,decoration=0)]
        body = json.dumps(fixture, separators=(",", ":")).encode()
        checkpoint.write_bytes(header+b"\n"+hashlib.sha256(body).hexdigest().encode()+b"\n"+body)
        reopened = run.start("server")
        migrated = reopened.state()["view"]["sandpit"]
        for i,m in enumerate(migrated["moulds"]):
            require(m["id"] == "legacy-"+str(i) and m["x"] == 4210+160*i and m["y"] == 440 and m["capacity"] == 2+i%2, "native legacy placement/capacity")
            require(all(m[k] == g["moulds"][i][k] for k in ("scoops","wet","built","decoration")), "native legacy progress/decorations")
        # Capture the actual rendered saved flag/shell, not generated diagrams.
        for profile,size,label in ((run.slots[0]["profile"],(1280,591),"phone"),(run.slots[1]["profile"],(1024,768),"tablet")):
            v = run.start("client", profile); clients.append(v); home.ready(v)
            v.input("resize", x=size[0], y=size[1]); home.ready(v)
            wait(lambda: v.input("inspect")["sandpitDecorationVisuals"][:2] == [1,2], "migrated decorations rendered")
            layout(v, label+"-migrated-flags-shells")
        record("native synthetic schema49 reopening retains positions, capacities, partial/full progress and rendered flags/shells on phone/tablet")
    def cap_capture():
        prior = read(run.path / "sandpit-stage-two/result.json")
        require(prior and prior["gameplayPassed"] and prior["build"] == args.build, "only reuse this script's completed synthetic fixture")
        checkpoint = run.path / "server-world/world.save"
        header, digest, body = checkpoint.read_bytes().split(b"\n", 2)
        require(header == b"LITTLEWEEPS-SOLO-1" and hashlib.sha256(body).hexdigest().encode() == digest, "cap checkpoint digest")
        snapshot = json.loads(body); fixture = copy.deepcopy(snapshot); g = fixture["sandpit"]
        require(g["format"] == 1 and len(g["moulds"]) == 4, "expected migrated synthetic source")
        for i in range(12):
            g["moulds"].append(dict(id="cap-"+str(i),creator="",shape="round",x=4150+85*(i%8),y=130+110*(i//8),width=72,depth=80,capacity=3,scoops=0,wet=False,built=False,decoration=0,version=0))
        body = json.dumps(fixture, separators=(",", ":")).encode()
        checkpoint.write_bytes(header+b"\n"+hashlib.sha256(body).hexdigest().encode()+b"\n"+body)
        nonlocal server
        server = run.start("server")
        before = copy.deepcopy(state()["moulds"])
        for profile,size,label in ((run.slots[0]["profile"],(1280,591),"phone"),(run.slots[1]["profile"],(1024,768),"tablet")):
            v = run.start("client", profile); clients.append(v); home.ready(v)
            v.input("resize", x=size[0], y=size[1]); home.ready(v); time.sleep(1.2)
            info = v.input("inspect")
            require(len(info["sandpit"]["moulds"]) == 16 and not next(c["enabled"] for c in info["controls"] if c["name"] == "Build"), "full pit must disable Build")
            rejected = command(v,32,value="place",target="place@"+str(state()["round"]),item="round",x=4745,y=460)
            require(not rejected["accepted"] and rejected["outcome"] == "sandpit-full" and state()["moulds"] == before, "native cap rejection preserves all work")
            layout(v, label+"-full-pit-feedback")
        record("native 16-piece cap rejects additional placement without mutation; Build disabled; Full count and existing flags/shells visible on phone/tablet")
    try:
        if args.different_pieces:
            prior = read(run.path / "sandpit-cap/result.json")
            require(prior and prior["gameplayPassed"] and prior["build"] == args.build, "only reuse owned completed cap fixture")
            server = run.start("server"); clients = [run.start("client", s["profile"]) for s in run.slots]
            wait(lambda: len(server.state()["connected"]) == 4, "four current admitted native clients")
            for v in clients: home.ready(v)
            a,b = clients[:2]; ids = ("cap-0","cap-1")
            move(a, ids[0]); move(b, ids[1]); before = copy.deepcopy(state()["moulds"])
            with ThreadPoolExecutor(max_workers=2) as pool:
                replies = list(pool.map(lambda pair: tool(pair[0],pair[1],"scoop"), zip((a,b),ids)))
            require(all(r["accepted"] for r in replies), "distinct-piece concurrent contributions accepted")
            for old in before:
                expected = copy.deepcopy(old)
                if old["id"] in ids: expected["scoops"] += 1; expected["version"] += 1
                require(piece(old["id"]) == expected, "parallel contributions change only their own pieces")
            record("four admitted native clients; concurrent contributions to two different pieces each add exactly one scoop without changing the other fourteen")
            passed = True
            return
        if args.cap_capture:
            cap_capture(); passed = True
            return
        server = run.start("server"); clients = [run.start("client", s["profile"]) for s in run.slots]; a,b,c,d = clients
        for v in clients:
            home.ready(v); require(command(v, 7, value="park" if v == d else "daycare")["accepted"], "travel")
            v.input("resize", x=1024 if v == c else 1280, y=768 if v == c else 591); home.ready(v)
        button(a, "Games"); button(a, "Sandcastle club"); wait(lambda: sum(m["attending"] for m in state()["members"]) == 3, "common start")
        first = place_ui(a, 31); second = place_ui(b, 0)
        # Cancel a genuinely retained approach before reaching the far bucket.
        a.input("touchButton", text="Scoop"); a.input("touchButton", text="Scoop")
        require(a.input("inspect")["sandpitApproach"] >= 0, "fixture must exercise a retained scoop approach")
        button(a, "Sand piece 2"); time.sleep(1.5)
        require(piece(second)["scoops"] == 0 and piece(first)["scoops"] <= 1, "selection change failed to cancel retained scoop taps")
        button(a, "Sand piece 1")
        # Inject actual rapid picture taps, including extra taps after capacity.
        for _ in range(8): a.input("touchButton", text="Scoop")
        wait(lambda: piece(first)["scoops"] == 3, "bounded rapid scoops")
        button(a, "Sand piece 2"); time.sleep(.5)
        require(piece(first)["scoops"] == 3 and piece(second)["scoops"] == 0, "rapid scoops spill onto newly selected piece")
        button(a, "Sand piece 1"); button(a, "Tip")
        wait(lambda: not a.input("inspect")["pending"], "dry-tip reply")
        require(piece(first)["scoops"] == 3 and not piece(first)["built"], "dry-tip loses fill")
        button(a, "Water"); wait(lambda: piece(first)["wet"], "water"); button(a, "Tip"); wait(lambda: piece(first)["built"], "confirmed reveal")
        button(b, "Water"); wait(lambda: piece(second)["wet"], "water before scoops")
        third = place_ui(a, 16); require(piece(first)["built"], "new placement erases first")
        button(a, "Scoop"); wait(lambda: piece(third)["scoops"] == 1, "partial fill")
        button(a, "Tip"); wait(lambda: not a.input("inspect")["pending"], "underfilled tip reply")
        require(piece(third)["scoops"] == 1 and not piece(third)["built"], "underfilled tip loses progress")
        button(a, "Water"); wait(lambda: piece(third)["wet"], "water partial bucket")
        button(a, "Tip"); wait(lambda: not a.input("inspect")["pending"], "wet underfilled tip reply")
        require(piece(third)["scoops"] == 1 and not piece(third)["built"], "wet underfilled bucket bypasses remaining scoops")
        layout(a, "phone-underfilled-retained")
        move(a, second); move(b, second)
        with ThreadPoolExecutor(max_workers=2) as pool:
            replies = list(pool.map(lambda v: tool(v, second, "scoop"), (a,b)))
        require(all(r["accepted"] for r in replies) and piece(second)["scoops"] == 2, "shared scoop race")
        require(tool(c, third, "scoop")["outcome"] == "walk-closer", "reach bypass")
        move(c, third); require(tool(c, third, "scoop")["accepted"] and piece(third)["scoops"] == 2, "parallel different-piece work")
        require(tool(a, second, "scoop")["accepted"], "third shared scoop")
        with ThreadPoolExecutor(max_workers=2) as pool:
            tips = list(pool.map(lambda v: tool(v, second, "tip"), (a,b)))
        require(sum(r["accepted"] for r in tips) == 1 and piece(second)["built"], "competing tips")
        button(a, "Scoop"); wait(lambda: piece(third)["scoops"] == 3, "remaining partial scoop")
        button(a, "Tip"); wait(lambda: piece(third)["built"], "partial bucket completes only after remaining scoops")
        place_ui(a, 20); require(len(state()["moulds"]) == 4 and all(piece(id)["built"] for id in (first,second,third)), "continued construction preserves completed pieces")
        layout(a, "phone-built-and-unfinished")
        fourth = state()["moulds"][-1]["id"]
        button(c, "Sand piece 4")
        for _ in range(3): button(c, "Scoop")
        wait(lambda: piece(fourth)["scoops"] == 3, "tablet touch scoops")
        button(c, "Water"); wait(lambda: piece(fourth)["wet"], "tablet touch water")
        button(c, "Tip"); wait(lambda: piece(fourth)["built"], "tablet touch tip")
        place_ui(c, 21); layout(c, "tablet-continued-construction")
        require(command(a, 32, value="replay")["accepted"] and len(state()["moulds"]) == 5, "old replay clears construction")
        record("native shared and different-piece work, receipt revision retries, exactly one competing tip, rapid picture taps, early water and dry Water-Tip recovery")
        if args.remaining_only:
            record("underfilled dry/wet Tip retains one scoop and requires remaining two; continued tap construction; phone/tablet safe controls; no replay clearing")
            legacy_captures(); passed = True
            return
        require(command(d, 7, value="daycare")["accepted"], "late travel"); wait(lambda: member(d)["attending"], "late join")
        before = state()["moulds"]; epoch = state()["round"]
        button(b, "Leave sandpit"); wait(lambda: not member(b)["attending"], "independent departure")
        require(state()["moulds"] == before and sum(m["attending"] for m in state()["members"]) == 3, "departure loss")
        c.close(); wait(lambda: not member(c)["attending"], "disconnect")
        c = run.start("client", c.profile); clients[2] = c; home.ready(c); wait(lambda: member(c)["attending"], "reconnect")
        require(state()["round"] == epoch and state()["moulds"] == before, "reconnect imports another world")
        info = c.input("inspect"); require(not any(info["sandpitTipEffects"]), "reconnect replays reveal")
        require(command(a, 32, value="replay")["accepted"] and state()["moulds"] == before, "replay clears work")
        layout(a, "phone-built-and-unfinished"); c.input("resize", x=1024, y=768); layout(c, "tablet-built-and-unfinished")
        record("independent departure/disconnect, late join/reconnect, current reconstruction without transient replay, no replay clear, phone/tablet safe controls")
        passed = True
    finally:
        if not passed:
            for i,v in enumerate(clients):
                if v.process.poll() is None:
                    try: home.capture(v, out, "failure-"+str(i))
                    except Exception: pass
        run.close()
        write(out / "result.json", dict(build=args.build, runId=run.run_id, gameplayPassed=passed, checks=checks, remainingOnly=args.remaining_only, capOnly=bool(args.cap_capture), peakNativeClients=2 if args.cap_capture else 4, scope="isolated native Stage 2; no real saved world/device/live server"))
    print("PASS ALL", flush=True)
if __name__ == "__main__": main()
