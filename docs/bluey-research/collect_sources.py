"""Collect public reference metadata; never install or execute template code."""
import concurrent.futures
import datetime
import html
import json
import re
import urllib.error
import urllib.request
from pathlib import Path

OUT = Path(__file__).parent
HEADERS = {"User-Agent": "Meeps-Game-Research", "Accept": "application/vnd.github+json"}


def fetch(url):
    request = urllib.request.Request(url, headers=HEADERS)
    with urllib.request.urlopen(request, timeout=25) as response:
        return response.read().decode("utf-8-sig"), response.geturl()


def character(row):
    name, slug, group = row
    url = f"https://www.bluey.tv/characters/{slug}/"
    try:
        source, resolved = fetch(url)
        title = re.search(r"<title>(.*?)</title>", source, re.S)
        portraits = [tag for tag in re.findall(r"<img\b[^>]*>", source) if "character-pose" in tag]
        image = re.search(r'src=["\']([^"\']+)', portraits[0]).group(1) if portraits else None
        if image and image.startswith("/"):
            image = "https://www.bluey.tv" + image
        return {"name": name, "group": group, "source": url, "resolved": resolved,
                "title": html.unescape(title.group(1)) if title else None, "portrait": image,
                "verified": bool(image), "checked": "2026-09-23"}
    except Exception as exc:
        return {"name": name, "group": group, "source": url, "portrait": None, "verified": False, "error": str(exc)}


def repo(name):
    base = f"https://api.github.com/repos/{name}"
    try:
        meta = json.loads(fetch(base)[0])
        branch = meta["default_branch"]
        commits = json.loads(fetch(base + "/commits?per_page=1")[0])
        latest = commits[0]
        tree = json.loads(fetch(base + f"/git/trees/{branch}?recursive=1")[0])
        paths = [x["path"] for x in tree.get("tree", []) if x["type"] == "blob"]
        evidence_paths = [p for p in paths if p.lower() in ("readme.md", "license", "license.md", "license.txt", "package.json")]
        evidence_paths += [p for p in paths if p.endswith("ProjectSettings/ProjectVersion.txt")][:3]
        evidence_paths += [p for p in paths if p.endswith(".cs") and re.search(r"(Drag|RadialLayout|QuestManager|SaveManager|PlayerController|ObjectControl|Touch|SaveLoad)", p)][0:8]
        files = []
        for path in evidence_paths:
            raw = f"https://raw.githubusercontent.com/{name}/{branch}/{urllib.parse.quote(path)}"
            try:
                content = fetch(raw)[0]
                files.append({"path": path, "url": f"https://github.com/{name}/blob/{branch}/{urllib.parse.quote(path)}", "content": content[:25000]})
            except Exception as exc:
                files.append({"path": path, "error": str(exc)})
        return {"name": name, "url": meta["html_url"], "archived": meta["archived"],
                "default_branch": branch, "license": meta.get("license"), "last_push": meta["pushed_at"],
                "head_commit": latest["sha"], "head_commit_date": latest["commit"]["committer"]["date"],
                "head_commit_message": latest["commit"]["message"].splitlines()[0],
                "tree_truncated": tree.get("truncated"), "relevant_files": files,
                "script_paths": [p for p in paths if p.endswith(".cs")], "checked": "2026-09-23"}
    except Exception as exc:
        return {"name": name, "error": str(exc)}


CHARACTERS = [
    ("Bluey", "bluey", "Heeler kids"), ("Bingo", "bingo", "Heeler kids"),
    ("Muffin", "muffin", "Heeler kids"), ("Socks", "socks", "Heeler kids"),
    ("Chloe", "chloe", "School friends"), ("Coco", "coco", "School friends"),
    ("Honey", "honey", "School friends"), ("Indy", "indy", "School friends"),
    ("Mackenzie", "mackenzie", "School friends"), ("Rusty", "rusty", "School friends"),
    ("Jack", "jack", "School friends"), ("Snickers", "snickers", "School friends"),
    ("Winton", "winton", "School friends"), ("The Terriers", "the-terriers", "School friends"),
    ("Lucky", "lucky", "Neighbours and friends"), ("Chucky", "chucky", "Neighbours and friends"),
    ("Judo", "judo", "Neighbours and friends"), ("Pom Pom", "pom-pom", "Neighbours and friends"),
    ("Lila", "lila", "Kindy friends"), ("Missy", "missy", "Kindy friends"),
    ("Buddy", "buddy", "Kindy friends"), ("Bentley", "bentley", "Kindy friends"),
    ("Juniper", "juniper", "Kindy friends"), ("Winnie", "winnie", "Neighbours and friends"),
    ("Jean-Luc", "jean-luc", "Neighbours and friends"), ("Lulu", "lulu", "Younger siblings and friends"),
    ("Dusty", "dusty", "Younger siblings and friends"), ("Dougie", "dougie", "Younger siblings and friends"),
    ("Hercules", "hercules", "More child characters"), ("Pretzel", "pretzel", "School friends"),
    ("Digger", "digger", "Older child characters"), ("Mia", "mia", "Older child characters"),
    ("Captain", "captain", "Older child characters"),
]
REPOS = ["Unity-Technologies/UnityPlayground", "Unity-UI-Extensions/com.unity.uiextensions",
         "YarnSpinnerTool/YarnSpinner-Unity", "UnityTechnologies/open-project-1",
         "lluispalerm/QuestSystem", "FelixBole/quest-system",
         "hsadler/unity-2d-topdown-template", "Unity-Technologies/PhysicsExamples2D",
         "Team-on/UnityGameTemplate"]

if __name__ == "__main__":
    with concurrent.futures.ThreadPoolExecutor(max_workers=5) as pool:
        characters = list(pool.map(character, CHARACTERS))
    (OUT / "character-references.json").write_text(json.dumps(characters, indent=2), encoding="utf-8")
    print("Characters:", [(c["name"], c["verified"]) for c in characters], flush=True)
    with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
        repos = list(pool.map(repo, REPOS))
    (OUT / "github-evidence.json").write_text(json.dumps(repos, indent=2), encoding="utf-8")
    print("Repositories:", [(r["name"], r.get("head_commit_date"), r.get("error")) for r in repos], flush=True)
