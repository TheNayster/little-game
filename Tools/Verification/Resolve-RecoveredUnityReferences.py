# /// script
# dependencies = ["UnityPy"]
# ///
"""Recover original PPtr identities lost by an exported Unity scene.

Hulk Phase 0 research utility. Inputs are read-only. It correlates exported
object IDs and field paths with the original bundle, then inventories the named
bundle directory. Identity/presence is not reconstruction or runtime validation.
"""
import argparse
from collections import Counter
import gc
import hashlib
import json
from pathlib import Path

import UnityPy


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def serialized_files(environment):
    pending = list(environment.files.values())
    while pending:
        item = pending.pop()
        if hasattr(item, "externals") and hasattr(item, "objects"):
            yield item
        elif hasattr(item, "files"):
            pending.extend(item.files.values())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--bundle", type=Path, required=True)
    parser.add_argument("--bundle-directory", type=Path, required=True)
    parser.add_argument("--unity-version", required=True)
    parser.add_argument("--scene", required=True, help="Exact relative scene path in dependency manifest")
    parser.add_argument("--manifest", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    bundle, directory, output = args.bundle.resolve(), args.bundle_directory.resolve(), args.output.resolve()
    if output.is_relative_to(directory) or output == bundle:
        parser.error("research output must remain outside source bundles")
    UnityPy.config.FALLBACK_UNITY_VERSION = args.unity_version
    original_hash = digest(bundle)
    manifest = json.loads(args.manifest.read_text(encoding="utf-8"))
    selected = [ref for ref in manifest if ref["from_path"] == args.scene]
    identifiers = {int(ref["from_id"]) for ref in selected}
    if not identifiers:
        parser.error("manifest contains no references from selected scene")
    environment = UnityPy.load(str(bundle))
    candidates = sorted(serialized_files(environment), key=lambda f: len(identifiers.intersection(f.objects)), reverse=True)
    source = candidates[0] if candidates else None
    if source is None or not identifiers.issubset(source.objects):
        raise ValueError("Original bundle must contain every manifest consumer ID; do not guess remapped IDs")
    trees, crosswalk = {}, []
    for ref in selected:
        identifier = int(ref["from_id"])
        row = {"consumer_id": ref["from_id"], "field": ref["field"], "status": "unresolved"}
        try:
            if identifier not in trees:
                trees[identifier] = source.objects[identifier].read_typetree()
            value = trees[identifier]
            for part in ref["field"].split("."):
                value = value[int(part)] if isinstance(value, list) else value[part]
            file_id, path_id = value["m_FileID"], value["m_PathID"]
            row.update(file_id=file_id, path_id=path_id, source_cab=source.name,
                       target_cab=source.externals[file_id-1].path if file_id else source.name,
                       status="original-pointer-recovered")
        except (KeyError, IndexError, TypeError, ValueError) as error:
            row["reason"] = str(error)[:200]
        crosswalk.append(row)
    externals = [{"source": source.name, "file_id": i+1, "path": ext.path,
                  "guid": ext.guid.hex() if ext.guid else None} for i, ext in enumerate(source.externals)]
    available, errors = {}, []
    files_examined = 0
    for candidate in sorted(directory.glob("*.bundle")):
        try:
            content = environment if candidate.resolve() == bundle else UnityPy.load(str(candidate))
            for serialized in serialized_files(content):
                available.setdefault(serialized.name, []).append(candidate.name)
            files_examined += 1
        except Exception as error:
            errors.append({"bundle": candidate.name, "error": str(error)[:200]})
        if files_examined % 20 == 0:
            gc.collect()
    groups = Counter()
    for row in crosswalk:
        if row["status"] != "original-pointer-recovered":
            continue
        name = row["target_cab"].split("/")[-1]
        row["candidate_bundles"] = available.get(name, [])
        # Exact object membership is checked only when that target is materialized.
        row["asset_status"] = "bundle-present-object-unverified" if row["candidate_bundles"] else "bundle-absent-from-input-directory"
        groups[row["asset_status"]] += 1
    if digest(bundle) != original_hash:
        raise ValueError("Original bundle changed during inspection")
    report = {"schema": 1, "source_bundle": bundle.name, "source_sha256": original_hash,
              "unity_fallback": args.unity_version, "bundle_files_examined": files_examined,
              "serialized_files": len(available), "scene_missing_usages": len(selected),
              "pointers_recovered": sum(r["status"] == "original-pointer-recovered" for r in crosswalk),
              "target_cabs": len({r["target_cab"] for r in crosswalk if "target_cab" in r}),
              "asset_statuses": dict(groups), "errors": errors,
              "limits": ["Only manifest usages in the selected original scene are correlated.",
                         "Directory absence does not establish absence from every possible download or cache.",
                         "A bundle name match does not prove the referenced object or its dependencies are usable."]}
    output.mkdir(parents=True, exist_ok=True)
    for name, value in {"original-pointer-summary.json": report, "original-pointer-crosswalk.json": crosswalk,
                        "original-externals.json": externals, "installed-bundle-index.json": available}.items():
        (output/name).write_text(json.dumps(value, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))
    return 1 if errors or report["pointers_recovered"] != len(selected) else 0


if __name__ == "__main__":
    raise SystemExit(main())
