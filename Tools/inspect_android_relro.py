"""Diagnose ELF LOAD/RELRO geometry; never certifies Android compatibility.

Keeps the stricter Test-AndroidArtifact.ps1 gate unchanged. Uses declared memory
ranges, including BSS, to look for writable bytes inside page-rounded RELRO.
Does not model the native bridge, dynamic relocations, or every linker version.
"""

import argparse
import hashlib
import json
import struct
import zipfile
from datetime import datetime, timezone
from pathlib import Path

PAGE = 16384
LOAD = 1
RELRO = 0x6474E552
WRITE = 2
LIMIT = 1 << 64


def read_segments(data):
    if len(data) < 64 or data[:7] != b"\x7fELF\x02\x01\x01":
        raise ValueError("Expected a complete little-endian ELF64 header")
    kind, machine, version = struct.unpack_from("<HHI", data, 16)
    if (kind, machine, version) != (3, 183, 1):
        raise ValueError("Expected an AArch64 shared object")
    table = struct.unpack_from("<Q", data, 32)[0]
    header_size, entry_size, count = struct.unpack_from("<HHH", data, 52)
    if header_size != 64 or entry_size != 56 or count in (0, 65535):
        raise ValueError("Unsupported ELF program-header layout")
    if table < 64 or table + count * entry_size > len(data):
        raise ValueError("Truncated ELF program-header table")
    segments = []
    for index in range(count):
        kind, flags, offset, addr, _, filesz, memsz, align = struct.unpack_from(
            "<IIQQQQQQ", data, table + index * entry_size
        )
        if kind not in (LOAD, RELRO):
            continue
        if filesz > memsz or offset + filesz > len(data) or addr + memsz >= LIMIT:
            raise ValueError(f"Invalid segment bounds at program header {index}")
        segments.append(dict(index=index, type=kind, flags=flags, offset=offset,
                             start=addr, end=addr + memsz, alignment=align))
    if not any(s["type"] == LOAD for s in segments):
        raise ValueError("Missing LOAD segments")
    return segments


def outside_ranges(start, end, covered):
    """Return pieces of [start,end) outside the union of covered intervals."""
    cursor = start
    pieces = []
    for left, right in sorted(covered):
        if right <= cursor or left >= end:
            continue
        if left > cursor:
            pieces.append((cursor, left))
        cursor = max(cursor, min(right, end))
    if cursor < end:
        pieces.append((cursor, end))
    return pieces


def analyze(data):
    segments = read_segments(data)
    loads = [s for s in segments if s["type"] == LOAD]
    relros = [s for s in segments if s["type"] == RELRO and s["end"] > s["start"]]
    declared = [(s["start"], s["end"]) for s in relros]
    load_issues = []
    for s in loads:
        a = s["alignment"]
        if a < PAGE or a & (a - 1):
            load_issues.append(f"LOAD {s['index']}: alignment is not a power of two >= 16384")
        elif (s["start"] - s["offset"]) % a:
            load_issues.append(f"LOAD {s['index']}: offset/address incongruence")
    reports = []
    for s in relros:
        start = s["start"] // PAGE * PAGE
        end = (s["end"] + PAGE - 1) // PAGE * PAGE
        overlaps = []
        for load in loads:
            if not load["flags"] & WRITE:
                continue
            left, right = max(start, load["start"]), min(end, load["end"])
            if left >= right:
                continue
            for a, b in outside_ranges(left, right, declared):
                overlaps.append(dict(loadIndex=load["index"], start=hex(a), end=hex(b)))
        covered = not outside_ranges(s["start"], s["end"],
                                     [(l["start"], l["end"]) for l in loads])
        reports.append(dict(start=hex(s["start"]), end=hex(s["end"]),
                            rawEnd16KbAligned=s["end"] % PAGE == 0,
                            roundedStart=hex(start), roundedEnd=hex(end),
                            containedInLoads=covered, writableBytesOutsideRelro=overlaps))
    return dict(loadIssues=load_issues, relro=reports,
                rawRelroEnds16KbAligned=all(r["rawEnd16KbAligned"] for r in reports),
                declaredGeometryIssuesFound=bool(load_issues) or any(
                    r["writableBytesOutsideRelro"] or not r["containedInLoads"] for r in reports),
                relroPresent=bool(reports))


def inspect_apk(path, expected_sha):
    digest = hashlib.sha256(path.read_bytes()).hexdigest()
    if digest != expected_sha.lower():
        raise ValueError("APK hash does not match the selected build evidence")
    libraries = []
    with zipfile.ZipFile(path) as apk:
        names = [n for n in apk.namelist() if n.startswith("lib/arm64-v8a/") and n.endswith(".so")]
        if not names or len(names) != len(set(names)):
            raise ValueError("Missing or duplicate ARM64 libraries")
        for name in sorted(names):
            data = apk.read(name)
            libraries.append(dict(name=name, sha256=hashlib.sha256(data).hexdigest(), **analyze(data)))
    return dict(utc=datetime.now(timezone.utc).isoformat(), apkSha256=digest,
                pageBytes=PAGE, scope="Declared ELF64 LOAD/RELRO geometry only",
                androidCompatibilityQualified=False, strictArtifactGateUnchanged=True,
                libraries=libraries)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("apk", type=Path)
    parser.add_argument("--expected-sha256", required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    report = inspect_apk(args.apk, args.expected_sha256)
    # Preserve earlier evidence rather than overwriting another inspection.
    with args.output.open("x", encoding="utf-8") as stream:
        json.dump(report, stream, indent=2)
        stream.write("\n")
    issues = sum(x["declaredGeometryIssuesFound"] for x in report["libraries"])
    print(f"Inspected {len(report['libraries'])} libraries; {issues} with geometry issues. "
          "Diagnostic only; strict APK and actual-device gates remain separate.")
    return 1 if issues else 0


if __name__ == "__main__":
    raise SystemExit(main())
