# /// script
# dependencies = ["PyYAML>=6,<7"]
# ///
"""Read an exported Unity world without modifying its assets.

Owner: Hulk world Phase 0. Inputs are an AssetRipper-style project and original
scene. Outputs are research data, not game assets or recovered executable code.
Keep outputs in ignored LocalData; source artifacts must remain unchanged.
"""
import argparse
from collections import Counter, deque
import hashlib
import json
from pathlib import Path
import re

import yaml

HEADER = re.compile(r"^--- !u!(\d+) &(-?\d+)(?: stripped)?\r?\n", re.M)
GUID = re.compile(r"^guid: ([0-9a-f]+)", re.M)
STRUCTURAL = {
    "m_GameObject", "m_Component", "m_Father", "m_Children", "m_PrefabInstance",
    "m_CorrespondingSourceObject", "m_PrefabAsset", "m_Script",
}
SERIALIZED = {".unity", ".prefab", ".asset", ".mat", ".controller", ".anim", ".overrideController"}
LOADER = getattr(yaml, "CSafeLoader", yaml.SafeLoader)


def sha(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest() if hasattr(hashlib, "file_digest") else hashlib.sha256(stream.read()).hexdigest()


def refs(value, trail=()):
    if isinstance(value, dict):
        if "fileID" in value:
            yield {"field": ".".join(map(str, trail)), "id": str(value["fileID"]), "guid": str(value.get("guid", ""))}
        else:
            for key, item in value.items():
                yield from refs(item, trail + (key,))
    elif isinstance(value, list):
        for index, item in enumerate(value):
            yield from refs(item, trail + (index,))


class World:
    def __init__(self, project):
        self.project = project
        self.guid_paths = {}
        self.duplicates = []
        self.documents = {}
        self.hashes = {}
        self.errors = []
        for meta in sorted((project / "Assets").rglob("*.meta")):
            match = GUID.search(meta.read_text(encoding="utf-8-sig"))
            if not match:
                continue
            path = meta.with_suffix("").relative_to(project).as_posix()
            if match[1] in self.guid_paths:
                self.duplicates.append([match[1], self.guid_paths[match[1]], path])
            self.guid_paths[match[1]] = path

    def load(self, path):
        if path in self.documents:
            return self.documents[path]
        self.documents[path] = {}
        absolute = self.project / path
        if not absolute.is_file():
            self.errors.append({"path": path, "error": "missing file"})
            return self.documents[path]
        self.hashes[path] = sha(absolute)
        if absolute.suffix not in SERIALIZED:
            return self.documents[path]
        text = absolute.read_text(encoding="utf-8-sig")
        headers = list(HEADER.finditer(text))
        for index, header in enumerate(headers):
            body = text[header.end():headers[index + 1].start() if index + 1 < len(headers) else len(text)]
            try:
                parsed = yaml.load(body, Loader=LOADER)
                if not isinstance(parsed, dict) or len(parsed) != 1:
                    raise ValueError("expected one Unity object mapping")
                typename, fields = next(iter(parsed.items()))
                if not isinstance(fields, dict):
                    raise ValueError("expected object fields")
            except (yaml.YAMLError, ValueError) as error:
                self.errors.append({"path": path, "id": header[2], "error": str(error)[:300]})
                continue
            if header[2] in self.documents[path]:
                raise ValueError("Duplicate object ID in " + path + ": " + header[2])
            script = fields.get("m_Script", {})
            script_path = self.guid_paths.get(str(script.get("guid", "")), "")
            script_type = Path(script_path).stem if Path(script_path).suffix == ".cs" else ("ASSEMBLY:" + Path(script_path).stem + "#" + str(script.get("fileID")) if script_path else "")
            node = {
                "path": path, "id": header[2], "class_id": int(header[1]), "type": typename,
                "name": fields.get("m_Name", ""), "script": script_path,
                "script_type": script_type,
                "game_object_id": str(fields.get("m_GameObject", {}).get("fileID", "0")),
                "refs": list(refs(fields)), "fields": fields,
            }
            self.documents[path][node["id"]] = node
        return self.documents[path]

    def resolve(self, path, reference):
        guid, target = reference["guid"], reference["id"]
        if guid and "deadbeef" in guid.lower():
            return "exporter-placeholder", None
        if not guid:
            if target == "0":
                return "null", None
            return ("local", (path, target)) if target in self.load(path) else ("missing-local", None)
        if guid.startswith("0000000000000000"):
            return "builtin", None
        other = self.guid_paths.get(guid)
        if not other:
            return "missing-guid", None
        objects = self.load(other)
        if objects and target not in objects:
            return "missing-object", (other, target)
        if not objects:
            return "file-present-object-unverified", (other, target)
        return "external", (other, target)

    def asset_label(self, reference):
        status, target = self.resolve("", reference)
        if not target:
            return {"status": status, "guid": reference["guid"], "id": reference["id"]}
        node = self.load(target[0]).get(target[1], {})
        return {"status": status, "path": target[0], "id": target[1], "name": node.get("name", Path(target[0]).stem)}

    def closure(self, scene, seeds, limit):
        queue, visited, links = deque(seeds), set(), []
        while queue and len(visited) < limit:
            location = queue.popleft()
            if location in visited:
                continue
            visited.add(location)
            node = self.load(location[0]).get(location[1])
            if not node:
                continue
            # ActivityActionObject and similar references point at GameObjects;
            # their components are executable graph nodes, not structural noise.
            if node["class_id"] == 1:
                for component in node["fields"].get("m_Component", []):
                    ref = component.get("component", {})
                    key = (location[0], str(ref.get("fileID", "0")))
                    child = self.load(location[0]).get(key[1])
                    if child and child["class_id"] == 114:
                        queue.append(key)
            elif node["script_type"] == "MonoActor":
                # MonoActor discovers its actor components at runtime, so its
                # serialized fields alone omit inventory and animation services.
                queue.append((location[0], node["game_object_id"]))
            for reference in node["refs"]:
                if reference["field"].split(".")[0] in STRUCTURAL:
                    continue
                status, target = self.resolve(location[0], reference)
                if status != "null":
                    links.append({"from_path": location[0], "from_id": location[1], **reference,
                                  "status": status, "target": target})
                if target and status in {"local", "external"}:
                    queue.append(target)
        return {"coverage": "bounded" if queue else "complete-static-closure",
                "remaining_queue": len(queue), "nodes": sorted(visited), "links": links}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project", required=True, type=Path)
    parser.add_argument("--scene", required=True)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--closure-limit", type=int, default=20000)
    args = parser.parse_args()
    project, output = args.project.resolve(), args.output.resolve()
    scene = Path(args.scene).as_posix()
    if not (project / scene).resolve().is_relative_to(project):
        parser.error("scene must remain inside project")
    if output.is_relative_to(project):
        parser.error("output must be outside the recovered project")
    if args.closure_limit < 1:
        parser.error("closure-limit must be positive")
    world = World(project)
    objects = world.load(scene)
    before = world.hashes[scene]
    game_objects = {k: n for k, n in objects.items() if n["class_id"] == 1}
    components = [n for n in objects.values() if n["class_id"] == 114]
    actor_nodes = [n for n in components if n["script_type"] == "MonoActor"]
    activities = [n for n in components if re.search(r"Activity$|HBP.*Controller$|Hbp.*Activity$|CakeStickerMapController$", n["script_type"])]
    actor_rows = []
    for actor in actor_nodes:
        go = game_objects.get(actor["game_object_id"], {})
        component_ids = [str(c.get("component", {}).get("fileID", "0")) for c in go.get("fields", {}).get("m_Component", [])]
        attached = [objects[c] for c in component_ids if c in objects]
        row = {"id": actor["id"], "game_object_id": actor["game_object_id"], "name": go.get("name"),
               "active": go.get("fields", {}).get("m_IsActive"),
               "components": [{"id": n["id"], "type": n["script_type"] or n["type"], "fields": n["fields"]} for n in attached if n["class_id"] == 114]}
        actor_rows.append(row)
    all_refs = []
    pending, seen = deque([scene]), set()
    while pending:
        path = pending.popleft()
        if path in seen:
            continue
        seen.add(path)
        for node in list(world.load(path).values()):
            for reference in node["refs"]:
                status, target = world.resolve(path, reference)
                if status != "null":
                    all_refs.append({"from_path": path, "from_id": node["id"], **reference, "status": status, "target": target})
                if target and reference["guid"] and status in {"external", "file-present-object-unverified", "missing-object"} and target[0] not in seen:
                    pending.append(target[0])
    roots = []
    for node in activities:
        go = game_objects.get(node["game_object_id"], {})
        roots.append({"id": node["id"], "object": go.get("name"), "script": node["script_type"]})
    selected = {"HulkOpenPresentActivity", "GoToTableWithPresentActivity", "HulkReactsToGivenPresents", "PutPresentOnTableActivityAction_1", "BouncyCastle"}
    closures = {}
    all_closures = {}
    for root in roots:
        key = root["object"] if root["object"] not in all_closures else root["object"] + ":" + root["id"]
        all_closures[key] = world.closure(scene, [(scene, root["id"])], args.closure_limit)
        if root["object"] in selected:
            closures[root["object"]] = all_closures[key]
    action_rows = []
    for actor in actor_rows:
        row = {"name": actor["name"], "id": actor["id"], "behaviors": [], "inventory": [], "animators": []}
        for component in actor["components"]:
            fields = component["fields"]
            row["behaviors"].append({"type": component["type"], "id": component["id"],
                                     "configuration": {k: v for k, v in fields.items() if not k.startswith("m_")}})
            if component["type"] == "NewActorInventoryActivity":
                for slot in fields.get("holdSlots", []):
                    row["inventory"].append({**slot, "slot_asset": world.asset_label(next(refs(slot["holdSlotId"])))})
            if component["type"] == "PrefabReferences":
                target_id = str(fields.get("Animator", {}).get("fileID", "0"))
                animator = objects.get(target_id)
                if animator:
                    controller = animator["fields"].get("m_Controller", {})
                    binding = world.asset_label(next(refs(controller))) if "fileID" in controller else {"status": "not-configured"}
                    row["animators"].append({"animator_id": target_id, "controller": binding})
        action_rows.append(row)
    nonrenderer = []
    brick_renderer_types = Counter()
    for node in components:
        if node["script_type"] != "LegoBrickMaterial":
            continue
        go = game_objects.get(node["game_object_id"], {})
        ids = [str(c.get("component", {}).get("fileID", "0")) for c in go.get("fields", {}).get("m_Component", [])]
        for i in ids:
            if objects.get(i, {}).get("class_id") in {23, 25, 26, 96, 120, 137, 199, 212}:
                brick_renderer_types[objects[i]["type"]] += 1
        if not any(objects.get(i, {}).get("class_id") in {23, 25, 26, 96, 120, 137, 199, 212} for i in ids):
            nonrenderer.append({"component": node["id"], "game_object": node["game_object_id"], "name": go.get("name"), "component_ids": ids})
    missing = [r for r in all_refs if r["status"] in {"missing-local", "missing-guid", "missing-object", "exporter-placeholder"}]
    brick_owners = Counter(n["game_object_id"] for n in components if n["script_type"] == "LegoBrickMaterial")
    duplicate_bricks = [{"game_object_id": key, "name": game_objects.get(key, {}).get("name"), "components": count}
                        for key, count in brick_owners.items() if count > 1]
    report = {
        "schema": 1, "purpose": "Hulk world Phase 0 static evidence; not runtime acceptance",
        "project": str(project), "scene": scene, "scene_sha256": before,
        "objects": len(objects), "game_objects": len(game_objects), "script_components": len(components),
        "actor_components": len(actor_rows), "activity_components": len(roots), "reachable_files": len(seen),
        "reference_statuses": dict(Counter(r["status"] for r in all_refs)),
        "unresolved_script_guids": dict(Counter(str(n["fields"].get("m_Script", {}).get("guid")) for n in components if not n["script"])),
        "assembly_script_bindings": dict(Counter(n["script_type"] for n in components if n["script_type"].startswith("ASSEMBLY:"))),
        "brick_renderer_types": dict(brick_renderer_types),
        "brick_owner_count": len(brick_owners), "extra_brick_components_on_same_owner": sum(n - 1 for n in brick_owners.values()),
        "activity_closure_counts": {"total": len(all_closures), "bounded": sum(v["coverage"] == "bounded" for v in all_closures.values())},
        "brick_components_without_renderer": len(nonrenderer), "duplicate_meta_guids": world.duplicates,
        "parse_errors": world.errors,
        "limits": ["Static serialized references only; no execution or activity semantics implied.",
                   "Actor components include props; names alone do not establish a playable roster.",
                   "Exporter placeholder GUIDs can collapse many unrelated missing assets into one token.",
                   "Image/audio/script file presence does not verify imported subobject identity.",
                   "Activity closures exclude transform hierarchy and expand action GameObject and MonoActor components.",
                   "Dependency injection, dynamic addressable loads and runtime-selected services need separate traces.",
                   "Assembly-bound MonoScripts retain assembly and fileID; their exact types are not inferred."]}
    assert before == sha(project / scene), "source scene changed during read-only inspection"
    output.mkdir(parents=True, exist_ok=True)
    outputs = {"summary.json": report, "actors.json": actor_rows, "activities.json": roots,
               "dependencies.json": all_refs, "missing-dependencies.json": missing,
               "gift-action-closures.json": closures, "brick-components-without-renderer.json": nonrenderer,
               "activity-closures.json": all_closures, "actor-action-map.json": action_rows,
               "duplicate-brick-components.json": duplicate_bricks,
               "input-hashes.json": world.hashes}
    for name, payload in outputs.items():
        (output / name).write_text(json.dumps(payload, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps(report, indent=2))
    return 1 if world.errors or world.duplicates else 0


if __name__ == "__main__":
    raise SystemExit(main())
