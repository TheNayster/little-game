"""Inspect a Windows-produced G2 Xcode export; never signs or contacts a Mac."""
import argparse
import hashlib
import json
import plistlib
from datetime import datetime, timezone
from pathlib import Path


def sha256(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def require(condition, message):
    if not condition:
        raise RuntimeError(message)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("build", type=int)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    require(23 <= args.build <= 9999, "Invalid solo build number")
    require(not args.output.exists(), "Use a new evidence file")
    root = Path(__file__).resolve().parent.parent
    folder = root / "Builds" / "iOSSolo" / f"G2-0.0.{args.build}"
    summary = json.loads((folder / "build-summary.json").read_text(encoding="utf-8-sig"))
    require(summary["result"] == "Succeeded" and not summary["development"], "Export failed or is development")
    require(summary["version"] == f"0.0.{args.build}" and summary["platform"] == "iOS", "Wrong build identity")
    require(summary["profile"] == "Assets/BuildProfiles/iPad Solo Prototype.asset", "Wrong profile")
    manifest = json.loads((folder / "artifact-manifest.json").read_text(encoding="utf-8-sig"))
    require(len(manifest) > 0, "No export files recorded")
    for entry in manifest:
        path = (folder / entry["path"]).resolve()
        require(path.is_relative_to(folder.resolve()), "Artifact path escaped export")
        require(sha256(path) == entry["sha256"], f"Artifact changed: {entry['path']}")
    xcode = folder / "Xcode"
    with (xcode / "Info.plist").open("rb") as stream:
        info = plistlib.load(stream)
    require(info["CFBundleShortVersionString"] == summary["version"] and info["CFBundleVersion"] == str(args.build), "Plist version mismatch")
    require("arm64" in info["UIRequiredDeviceCapabilities"], "Expected ARM64 device export")
    require(all("Landscape" in value for value in info["UISupportedInterfaceOrientations"]), "Unexpected orientation")
    project = (xcode / "Unity-iPhone.xcodeproj" / "project.pbxproj").read_text(encoding="utf-8")
    require("PRODUCT_BUNDLE_IDENTIFIER = com.littleweeps.familyplayset;" in project, "App identity changed")
    require('TARGETED_DEVICE_FAMILY = "1,2";' in project and "SUPPORTED_PLATFORMS = iphoneos;" in project, "Expected iPad/iPhone device target")
    require("IPHONEOS_DEPLOYMENT_TARGET = 15.0;" in project, "Minimum deployment target changed")
    generated = xcode / "Il2CppOutputProject" / "Source" / "il2cppOutput"
    for name, symbol in (("LittleWeeps.Core.cpp", "SoloWorld_ReadPlayer"), ("LittleWeeps.Client.cpp", "SoloScreen_BeginPointer"), ("LittleWeeps.Adapters.cpp", "CheckpointStore_Save")):
        require(symbol in (generated / name).read_text(encoding="utf-8-sig"), f"Missing current generated code: {name}")
    result = dict(utc=datetime.now(timezone.utc).isoformat(), build=args.build, unity=summary["unity"],
                  exportInspectionPassed=True, artifactFilesVerified=len(manifest),
                  identity="com.littleweeps.familyplayset", minimumIOS="15.0", deviceFamilies=["iPhone", "iPad"],
                  orientations=info["UISupportedInterfaceOrientations"], il2cppCodeGenerated=True,
                  nativeXcodeBuildPassed=False, signed=False, installed=False, macAccessed=False,
                  limitation="Windows export inspection only; Mac compilation, signing and real-device tests remain pending.")
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
