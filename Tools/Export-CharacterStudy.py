"""Export the existing vector study as cropped Unity sprites (no art regeneration).

Requires Node + sharp. Pass --node and --node-modules for a bundled runtime.
The SVG remains the editable source; the manifest preserves crop origins so
Unity can reconstruct the source's joints without full-canvas transparent sprites.
"""
import argparse
import copy
import hashlib
import json
import os
from pathlib import Path
import subprocess
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "SourceArt/Characters"
OUTPUT = ROOT / "Unity/FamilyPlayset/Assets/FamilyPlayset/Art/Characters"
NS = "{http://www.w3.org/2000/svg}"
ET.register_namespace("", NS[1:-1])


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--node", default="node")
    parser.add_argument("--node-modules")
    args = parser.parse_args()
    contract = json.loads((SOURCE / "character-contract.json").read_text())
    for character in contract["characters"]:
        export_character(args, contract, character)
        export_character(args, contract, character, profile=True)


def export_character(args, contract, character, profile=False):
    source = SOURCE / character["profileSource" if profile else "source"]
    output = OUTPUT / character["id"]
    if profile:
        output = output / "profile"
    pivots = contract["profilePivots" if profile else "pivots"]
    hand = contract["profileHandAnchor" if profile else "handAnchor"]
    original = ET.parse(source).getroot()
    nodes = {node.get("id"): node for node in original.iter() if node.get("id")}
    temporary = ROOT / "LocalData/CharacterWorkshop/export" / character["id"]
    if profile:
        temporary = temporary / "profile"
    temporary.mkdir(parents=True, exist_ok=True)
    output.mkdir(parents=True, exist_ok=True)
    names = ["ground-shadow", "tail", "arm-far", "foot-far", "foot-near",
             "body", "head", "eyes-open", "eyes-closed", "muzzle", "arm-near", "held-prop"]
    jobs = []
    for name in names:
        svg = ET.Element(NS + "svg", {"viewBox": "0 0 440 500", "width": "440", "height": "500"})
        definitions = original.find(NS + "defs")
        if definitions is not None:
            svg.append(copy.deepcopy(definitions))
        node = copy.deepcopy(nodes[name])
        node.attrib.pop("display", None)
        if name == "head":
            for child in list(node):
                if child.get("id") in ("eyes-open", "eyes-closed", "muzzle"):
                    node.remove(child)
        if name == "arm-near":
            node.remove(next(child for child in node if child.get("id") == "hand-anchor"))
        if name == "held-prop":
            node.set("transform", "translate(%s %s)" % tuple(hand["position"]))
        svg.append(node)
        path = temporary / (name + ".svg")
        ET.ElementTree(svg).write(path, encoding="utf-8", xml_declaration=True)
        jobs.append({"name": name, "input": str(path), "output": str(output / (name + ".png"))})

    # Alpha scanning gives exact crop coordinates, including antialiased edges.
    script = r"""
const sharp=require('sharp'), fs=require('fs');
(async()=>{const rows=[];for(const job of JSON.parse(fs.readFileSync(0,'utf8'))){
 const {data,info}=await sharp(job.input).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 let l=info.width,t=info.height,r=-1,b=-1;
 for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++)if(data[(y*info.width+x)*4+3]){
  l=Math.min(l,x);r=Math.max(r,x);t=Math.min(t,y);b=Math.max(b,y);
 }
 if(r<0)throw Error('Empty layer: '+job.name);
 await sharp(data,{raw:info}).extract({left:l,top:t,width:r-l+1,height:b-t+1}).png().toFile(job.output);
 rows.push({name:job.name,left:l,top:t,width:r-l+1,height:b-t+1});
}process.stdout.write(JSON.stringify(rows));})().catch(e=>{console.error(e);process.exit(1)});
"""
    env = os.environ.copy()
    if args.node_modules:
        env["NODE_PATH"] = args.node_modules
    layers = json.loads(subprocess.check_output([args.node, "-e", script], input=json.dumps(jobs).encode(), env=env))
    for layer in layers:
        layer["sha256"] = hashlib.sha256((output / (layer["name"] + ".png")).read_bytes()).hexdigest()
        name = layer["name"]
        pivot = pivots.get(name, contract["coordinates"]["rootGround"])
        if name in ("eyes-open", "eyes-closed", "muzzle"):
            pivot = pivots["head"]
        if name == "held-prop":
            pivot = hand["position"]
        layer["pivotX"], layer["pivotY"] = pivot
    manifest = {"schema": 2, "assetId": character["id"], "displayName": character["name"], "scale": character["scale"], "source": source.relative_to(ROOT).as_posix(),
                "sourceSha256": hashlib.sha256(source.read_bytes()).hexdigest(), "pixelsPerUnit": 100,
                "contract": "SourceArt/Characters/character-contract.json", "contractSha256": hashlib.sha256((SOURCE / "character-contract.json").read_bytes()).hexdigest(),
                "width": 440, "height": 500, "groundX": 220, "groundY": 454, "layers": layers}
    (output / "import-manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    print(json.dumps({"layers": len(layers), "rgbaBytes": sum(x["width"] * x["height"] * 4 for x in layers),
                      "output": str(output)}))


if __name__ == "__main__":
    main()
