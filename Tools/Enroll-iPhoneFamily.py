"""Stage a family identity over an already trusted Windows USB connection.

Run with: uv run --with pymobiledevice3==11.19.1 python Tools/Enroll-iPhoneFamily.py ...
The app consumes the inbox into Keychain on its next launch. This tool never
uninstalls, resets pairing, replaces an existing identity, or prints credentials.
"""
import argparse
import asyncio
import hashlib
import json
import plistlib
import re
import uuid
from pathlib import Path

from pymobiledevice3.lockdown import create_using_usbmux
from pymobiledevice3.services.house_arrest import HouseArrestService
from pymobiledevice3.services.installation_proxy import InstallationProxyService
from family_pairing import ROOT, read_record

BUNDLE = "com.littleweeps.familyplayset"


async def enroll(args):
    backup = json.loads(args.backup.read_text(encoding="utf-8"))
    if backup["device"] != args.serial or backup["bundle"] != BUNDLE:
        raise ValueError("Backup belongs to another device or app")
    before = Path(backup["folder"])
    # Missing enrollment status is allowed only for the known foundation update.
    # An established family app must explicitly report unpaired before enrollment.
    foundation = backup["installed"]["CFBundleVersion"] == "20"
    for item in backup["files"]:
        file = (before / item["path"].lstrip("/")).resolve()
        if not file.is_relative_to(before.resolve()):
            raise ValueError("Backup path escapes its folder")
        if hashlib.sha256(file.read_bytes()).hexdigest() != item["sha256"]:
            raise ValueError("Backup no longer matches its manifest")
    pref_path = "/Library/Preferences/" + BUNDLE + ".plist"
    previous_prefs = plistlib.loads((before / pref_path.lstrip("/")).read_bytes())
    pair = read_record(ROOT / "LocalData/FamilyLAN" / args.family / f"player-{args.player}.pairing")
    if pair.get("role") != "client" or pair.get("worldId") != args.family or pair.get("privateKey") or pair.get("members"):
        raise ValueError("Expected one client enrollment")
    lock = await create_using_usbmux(serial=args.serial, autopair=False, connection_type="USB")
    try:
        async with InstallationProxyService(lock) as proxy:
            apps = await proxy.lookup({"BundleIDs": [BUNDLE], "ReturnAttributes": [
                "CFBundleIdentifier", "CFBundleVersion", "CFBundleShortVersionString"]})
        app = (apps or {}).get(BUNDLE, {})
        if app.get("CFBundleVersion") != str(args.build) or app.get("CFBundleShortVersionString") != f"0.0.{args.build}":
            raise ValueError("Requested family build is not installed")
        async with await HouseArrestService.create(lock, BUNDLE) as afc:
            current_prefs = plistlib.loads(await afc.get_file_contents(pref_path))
            if any(current_prefs.get(k) != v for k, v in previous_prefs.items()):
                raise ValueError("Existing preferences changed; review before enrollment")
            for item in backup["files"]:
                if not item["path"].startswith("/Documents/"):
                    continue
                if hashlib.sha256(await afc.get_file_contents(item["path"])).hexdigest() != item["sha256"]:
                    raise ValueError("Existing document changed; review before enrollment")
            folder = "/Documents/FamilyLAN"
            status_path = folder + "/enrollment-status.txt"
            status = (await afc.get_file_contents(status_path)).decode().strip() if await afc.exists(status_path) else ""
            if status != "unpaired" and not (foundation and not status):
                raise ValueError("Enrollment is established or unresolved; nothing replaced")
            for name in ("enrollment.json", "enrollment.pending"):
                if await afc.exists(folder + "/" + name):
                    raise ValueError("An enrollment inbox already exists; review it first")
            await afc.makedirs(folder)
            payload = json.dumps(pair, separators=(",", ":")).encode()
            pending = folder + "/enrollment.pending"
            await afc.set_file_contents(pending, payload)
            if await afc.get_file_contents(pending) != payload:
                raise ValueError("Enrollment transfer verification failed; pending file retained")
            await afc.rename(pending, folder + "/enrollment.json")
        print(json.dumps({"staged": True, "build": args.build, "player": args.player,
            "existingPreferenceValuesRetained": True, "existingDocumentsRetained": True,
            "next": "Open the app; verify paired status, consumed inbox and shared admission. Staging is not completion."}))
    finally:
        await lock.close()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--serial", required=True)
    parser.add_argument("--family", required=True)
    parser.add_argument("--player", type=int, choices=range(1, 5), required=True)
    parser.add_argument("--build", type=int, required=True)
    parser.add_argument("--backup", type=Path, required=True)
    args = parser.parse_args()
    if not re.fullmatch(r"[A-Za-z0-9-]+", args.serial) or uuid.UUID(args.family).hex != args.family or args.build < 79:
        raise ValueError("Invalid target")
    asyncio.run(asyncio.wait_for(enroll(args), 45))


if __name__ == "__main__":
    main()
