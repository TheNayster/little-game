"""Project-checked local Unity MCP client.

Run: uv run --python 3.11 --with mcp==2.2.0 python Tools/unity_mcp.py request.json
The request is {"tool": "name", "arguments": {...}} or {"resource": "uri"}.
With no request file, print the verified project information.
"""
import asyncio
import json
from pathlib import Path
import sys

from mcp import ClientSession
from mcp.client.streamable_http import streamable_http_client


def resource_json(result):
    return json.loads(result.contents[0].text)


async def main():
    expected = (Path(__file__).resolve().parents[1] / "Unity" / "FamilyPlayset").resolve()
    async with streamable_http_client("http://127.0.0.1:8080/mcp") as streams:
        async with ClientSession(streams[0], streams[1]) as session:
            await session.initialize()
            instances = resource_json(await session.read_resource("mcpforunity://instances"))
            candidates = [i for i in instances.get("instances", []) if i.get("name") == "FamilyPlayset"]
            if len(candidates) != 1:
                raise RuntimeError("Expected exactly one FamilyPlayset editor. Run Connect-GameTools.ps1.")
            await session.call_tool("set_active_instance", {"instance": candidates[0]["id"]})
            info = resource_json(await session.read_resource("mcpforunity://project/info"))
            if not info.get("success") or Path(info["data"]["projectRoot"]).resolve() != expected:
                raise RuntimeError("Unity reports a different project. No requested action was run.")
            if len(sys.argv) == 1:
                print(json.dumps(info, indent=2))
                return
            request = json.loads(Path(sys.argv[1]).read_text(encoding="utf-8-sig"))
            if "resource" in request:
                result = await session.read_resource(request["resource"])
            else:
                result = await session.call_tool(request["tool"], request.get("arguments", {}))
            print(result.model_dump_json())
            structured = getattr(result, "structured_content", None)
            if getattr(result, "is_error", False) or (isinstance(structured, dict) and structured.get("success") is False):
                raise RuntimeError("Unity tool returned an error; see the response above.")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    asyncio.run(main())
