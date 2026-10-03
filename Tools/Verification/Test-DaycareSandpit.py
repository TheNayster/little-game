"""Stage 2: one isolated authority/four native clients, plus real picture taps.
Requires a freshly built schema50/content68 artifact matching current task source.
Never launches the installed family server or opens a real save.
"""
import sys as _path_sys
from pathlib import Path as _ProjectPath
_path_sys.path.insert(0, str(next(p for p in _ProjectPath(__file__).resolve().parents if p.name == "Tools")))
from project_paths import ROOT as PROJECT_ROOT
import argparse, hashlib, importlib.util, time
from concurrent.futures import ThreadPoolExecutor
from shared_garden_runtime import Run, wait, require, read, write
spec = importlib.util.spec_from_file_location("home", Path(__file__).with_name("Test-HomeWorld.py"))
home = importlib.util.module_from_spec(spec); spec.loader.exec_module(home)

def main():
    parser = argparse.ArgumentParser(); parser.add_argument("build", type=int); args = parser.parse_args()
    run = Run(args.build, extended_test_lifetime=True)
    require(run.content == 68 and read(run.folder / "build-summary.json")["schema"] == 50, "Stage 2 source required; never substitute an older build")
    manifest = read(run.folder / "source-manifest.json")
    files = {f["path"]: f["sha256"] for f in manifest["files"]}
    for name in ("Core/Worlds/Daycare/DaycareSandpit.cs", "Core/Worlds/Dinosaur/DinosaurWorld.cs", "Core/Shared/Layout/WorldLayout.cs", "Client/Shared/Sessions/GameScreen.cs", *["Client/Worlds/Daycare/"+n+".cs" for n in ("GameScreen.SandcastleClub", "GameScreen.SandcastleWater", "GameScreen.SandcastleTip", "GameScreen.SandcastleDecoration", "SandShape", "SandScoopFeedback", "SandWaterFeedback", "SandTipFeedback", "SandDecorationFeedback")]):
        relative = "Unity/FamilyPlayset/Assets/FamilyPlayset/Code/" + name
        require(files.get(relative) == hashlib.sha256((PROJECT_ROOT / relative).read_bytes()).hexdigest(), "Build does not contain current " + name)
    out = run.path / "sandpit-stage-two"; out.mkdir(); checks = []; clients = []; passed = False
    print("EVIDENCE " + str(out), flush=True)
    def state(): return server.state()["view"]["sandpit"]
    def member(v): return next(m for m in state()["members"] if m["actor"] == v.profile)
    def piece(id): return next(m for m in state()["moulds"] if m["id"] == id)
    def record(name): checks.append(name); print("PASS " + name, flush=True)
    def button(v, name):
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
        return next(m["id"] for m in state()["moulds"] if m["id"] not in old)
    def layout(v, name):
        info = v.input("inspect")
        for control in info["controls"]:
            if control["name"] in ("Build", "Scoop", "Water", "Tip", "Leave sandpit"):
                require(control["bounds"]["width"] >= 50 and control["bounds"]["height"] >= 44, "small essential control "+control["name"])
                r, safe = control["bounds"], info["safeArea"]
                require(r["x"] >= safe["x"]-2 and r["y"] >= safe["y"]-2 and r["x"]+r["width"] <= safe["x"]+safe["width"]+2 and r["y"]+r["height"] <= safe["y"]+safe["height"]+2, "essential control leaves safe area")
        home.capture(v, out, name)
    try:
        server = run.start("server"); clients = [run.start("client", s["profile"]) for s in run.slots]; a,b,c,d = clients
        for v in clients:
            home.ready(v); require(command(v, 7, value="park" if v == d else "daycare")["accepted"], "travel")
            v.input("resize", x=1024 if v == c else 1280, y=768 if v == c else 591); home.ready(v)
        button(a, "Games"); button(a, "Sandcastle club"); wait(lambda: sum(m["attending"] for m in state()["members"]) == 3, "common start")
        first = place_ui(a, 0); second = place_ui(b, 3)
        for _ in range(3): button(a, "Scoop")
        wait(lambda: piece(first)["scoops"] == 3, "three rapid scoops"); button(a, "Tip")
        wait(lambda: not a.input("inspect")["pending"], "dry-tip reply")
        require(piece(first)["scoops"] == 3 and not piece(first)["built"], "dry-tip loses fill")
        button(a, "Water"); wait(lambda: piece(first)["wet"], "water"); button(a, "Tip"); wait(lambda: piece(first)["built"], "confirmed reveal")
        button(b, "Water"); wait(lambda: piece(second)["wet"], "water before scoops")
        third = place_ui(a, 16); require(piece(first)["built"], "new placement erases first")
        move(a, second); move(b, second)
        with ThreadPoolExecutor(max_workers=2) as pool:
            replies = list(pool.map(lambda v: tool(v, second, "scoop"), (a,b)))
        require(all(r["accepted"] for r in replies) and piece(second)["scoops"] == 2, "shared scoop race")
        require(tool(c, third, "scoop")["outcome"] == "walk-closer", "reach bypass")
        move(c, third); require(tool(c, third, "scoop")["accepted"], "parallel different-piece work")
        require(tool(a, second, "scoop")["accepted"], "third shared scoop")
        with ThreadPoolExecutor(max_workers=2) as pool:
            tips = list(pool.map(lambda v: tool(v, second, "tip"), (a,b)))
        require(sum(r["accepted"] for r in tips) == 1 and piece(second)["built"], "competing tips")
        record("native shared and different-piece work, receipt revision retries, exactly one competing tip, rapid picture taps, early water and dry Water-Tip recovery")
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
        write(out / "result.json", dict(build=args.build, runId=run.run_id, gameplayPassed=passed, checks=checks, scope="isolated native Stage 2; no real saved world/device/live server"))
    print("PASS ALL", flush=True)
if __name__ == "__main__": main()
