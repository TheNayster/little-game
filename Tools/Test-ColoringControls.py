"""Native coloring controls: touch targets, retained pages and four independent artists."""
import argparse, importlib.util, time, json, collections
from pathlib import Path
from copy import deepcopy
from shared_garden_runtime import Run, wait, require, write
spec=importlib.util.spec_from_file_location("home",Path(__file__).with_name("Test-HomeWorld.py"))
home=importlib.util.module_from_spec(spec);spec.loader.exec_module(home)

def main():
    p=argparse.ArgumentParser();p.add_argument("build",type=int);p.add_argument("--previous",type=int,default=200);args=p.parse_args()
    old=Run(args.previous);run=None;out=old.path/"coloring-controls";out.mkdir();checks=[];passed=False
    def record(s):checks.append(s);print("PASS "+s,flush=True)
    def button(c,name):c.input("touchButton",text=name);time.sleep(.12);home.ready(c)
    def move(c):require(home.command(c,0,x=-5460,y=200)["accepted"],"move to art");time.sleep(.3)
    def info(c):return c.input("inspect")
    def choose(c,page):
        button(c,"Choose picture")
        for _ in range(3):
            controls={v["name"] for v in info(c)["controls"]}
            if "Coloring page "+str(page) in controls:break
            visible=[int(n.split()[-1]) for n in controls if n.startswith("Coloring page ")]
            button(c,"More pictures" if page>max(visible) else "Earlier pictures")
        button(c,"Coloring page "+str(page))
    def tap(c,x,y):
        c.input("touch-begin",role="discovery",x=x,y=y,finger=49);c.input("touch-end",role="discovery",x=x,y=y,finger=49);time.sleep(.12);home.ready(c)
    print("EVIDENCE "+str(out),flush=True)
    try:
        old.start("server");a=old.start("client",old.slots[0]["profile"]);home.ready(a);move(a)
        require(home.command(a,19,item=a.profile,target="0@0",value="fill:0:5")["accepted"],"old fill")
        old.close();before=json.loads((old.path/"server-world/world.save").read_bytes().split(b"\n",2)[2])["discovery"]
        run=Run(args.build,resume=old.run_id);server=run.start("server");clients=[run.start("client",s["profile"]) for s in run.slots];a,b,c,d=clients
        def world():return wait(lambda:server.state(),"authority view")["view"]
        def work(c):return next(w for w in world()["discovery"] if w["owner"]==c.profile)
        require(world()["discovery"]==before,"controls changed saved discovery data")
        for v in clients:home.ready(v);move(v);button(v,"Coloring table")
        record("200 saved pages colors histories and science data reopen unchanged")
        root=Path(__file__).resolve().parents[1]/"Unity/FamilyPlayset/Assets/FamilyPlayset/Resources/Discovery/Coloring"
        catalog=json.loads((root/"catalog.json").read_text())
        for page in range(18):
            region=0;x,y=280,250
            if page>=6:
                item=catalog["pages"][page-6];raw=(root/(item["id"]+".bytes")).read_bytes();w=int.from_bytes(raw[:2],"little");h=int.from_bytes(raw[2:4],"little")
                counts=collections.Counter(raw[4+w*h//4:4+w*h*3//4]);counts.pop(0,None);reg=counts.most_common(1)[0][0]
                pixels=[i for i,n in enumerate(raw[4:]) if n==reg];pt=pixels[len(pixels)//2];region=reg-1;x=(pt%w+.5)/w*800;y=(pt//w+.5)/h*460
            # Only the dinosaur legacy shape uses the known tap point. Other
            # legacy pages are selected/rendered; official masks test exact taps.
            for v,color in zip(clients,["Red","Green","Purple","Orange"]):
                choose(v,page)
                if page==0 or page>=6:button(v,"Discovery crayon "+color);tap(v,x,y)
            if page==0 or page>=6:require([work(v)["pages"][page]["colors"][region] for v in clients]==[1,4,6,2],"independent fill "+str(page))
            if page in [0,6,17]:home.capture(a,out,"page-"+str(page))
        record("all eighteen pages accessible through the paged picture chooser; thirteen real mask fills remain independent across four players")
        prior=deepcopy(work(b));button(a,"Undo");require(work(a)["pages"][17]["colors"][region]==0 and work(b)==prior,"undo scope");button(a,"Redo");require(work(a)["pages"][17]["colors"][region]==1,"redo scope")
        # Disabled boundaries must not wrap or edit any page.
        button(a,"Page >");require(any("18 / 18" in t for t in info(a)["visibleText"]),"last page wraps")
        choose(a,0);button(a,"< Page");require(any("1 / 18" in t for t in info(a)["visibleText"]),"first page wraps")
        record("undo redo and disabled page boundaries cannot modify another artist or wrap")
        choose(a,6)
        for width,height,label in [(1024,768,"tablet"),(1280,591,"phone")]:
            a.input("resize",x=width,y=height);time.sleep(.5);s=info(a);controls={v["name"]:v for v in s["controls"]}
            for name in ["Back to Home","Choose picture","Undo","Redo","< Page","Page >"]+["Discovery crayon "+c for c in ["White","Red","Orange","Yellow","Green","Blue","Purple","Brown","Black"]]:
                r=controls[name]["bounds"];require(r["width"]>=44 and r["height"]>=44,"small target "+name);require(r["x"]>=0 and r["y"]>=0 and r["x"]+r["width"]<=s["screenWidth"]+1 and r["y"]+r["height"]<=s["screenHeight"]+1,"offscreen target "+name)
            home.capture(a,out,"coloring-"+label);button(a,"Choose picture");home.capture(a,out,"pictures-"+label)
            visible=[v for v in info(a)["controls"] if v["name"].startswith("Coloring page ")];require(len(visible)==6,"chooser crowded");require(all(v["bounds"]["width"]>100 and v["bounds"]["height"]>100 for v in visible),"tiny previews")
            button(a,"Close picture collection")
        record("phone/tablet touch bounds and six large previews verified with native captures")
        button(d,"Back to Home");require(home.command(d,7,value="park")["accepted"],"sibling travel");require(info(b)["discoveryOpen"],"sibling activity closed")
        button(b,"Discovery crayon Blue");tap(b,x,y);require(work(b)["pages"][17]["colors"][region]==5,"remaining artist cannot paint")
        expected=deepcopy(world()["discovery"]);run.close();run=Run(args.build,resume=old.run_id);server=run.start("server");a=run.start("client",run.slots[0]["profile"]);home.ready(a)
        require(world()["discovery"]==expected,"restart lost work");move(a);button(a,"Coloring table");choose(a,17)
        record("independent departure and authority restart/rejoin retain all four artists' pages and histories")
        run.close()
        for log in old.path.rglob("player.log"):require("Exception:" not in log.read_text(errors="replace"),"native exception "+str(log))
        record("native clients and authority close without exceptions")
        passed=True
    finally:
        if run:run.close()
        old.close();write(out/"results.json",dict(passed=passed,build=args.build,previous=args.previous,checks=checks,liveFamilyTouched=False,physicalDevicesTested=False));print("RESULT "+str(out/"results.json"),flush=True)
if __name__=="__main__":main()
