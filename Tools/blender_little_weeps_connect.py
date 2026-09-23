"""Local reconnect request; never opens or saves a .blend file."""
from pathlib import Path
import time
import bpy

bl_info = {
    "name": "Little Weeps Tools Reconnect", "author": "Little Weeps",
    "version": (1, 0, 0), "blender": (5, 0, 0), "category": "System",
}
_last_request = None

def _poll():
    global _last_request
    try:
        request = Path.home() / ".little-weeps-tools" / "blender-connect.request"
        if request.exists() and 0 <= time.time() - request.stat().st_mtime < 180:
            token = request.read_text(encoding="utf-8-sig").strip()
            if token and token != _last_request:
                import addon_utils
                if not addon_utils.check("blender_mcp")[1]:
                    addon_utils.enable("blender_mcp", default_set=True, persistent=True)
                server = getattr(bpy.types, "blendermcp_server", None)
                if server is None or not server.running:
                    bpy.context.scene.blendermcp_port = 9876
                    bpy.ops.blendermcp.start_server()
                _last_request = token
    except Exception as exc:
        print(f"Little Weeps reconnect: {exc}")
    return 1.0

def register():
    if not bpy.app.background and not bpy.app.timers.is_registered(_poll):
        bpy.app.timers.register(_poll, first_interval=1.0, persistent=True)

def unregister():
    if bpy.app.timers.is_registered(_poll):
        bpy.app.timers.unregister(_poll)
