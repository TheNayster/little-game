using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace LittleWeeps.Server
{
    // G1 process/lifecycle probe only. No network listener or shared-world state yet.
    public sealed class FoundationServer : MonoBehaviour
    {
        private string evidenceDirectory, runId;
        private float nextHeartbeat;
        private int heartbeats;
        private bool stopped;

        private void Start()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 20;
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-serverEvidence") evidenceDirectory = Path.GetFullPath(args[++i]);
                else if (args[i] == "-serverRun") runId = args[++i];
            }
#if !UNITY_SERVER
            Debug.LogError("The server fixture requires the Dedicated Server build target.");
            Application.Quit(2);
            return;
#else
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null || Camera.allCamerasCount != 0)
            {
                Debug.LogError("Unexpected graphics device or camera in server fixture.");
                Application.Quit(3);
                return;
            }
            if (evidenceDirectory != null)
            {
                if (!Guid.TryParseExact(runId, "N", out _)) throw new ArgumentException("Use a unique server run GUID.");
                Directory.CreateDirectory(evidenceDirectory);
                if (File.Exists(Path.Combine(evidenceDirectory, "ready.json")))
                    throw new IOException("Evidence already exists; use a fresh run directory.");
                WriteRecord("ready");
            }
            nextHeartbeat = Time.realtimeSinceStartup + 1;
            Debug.Log("LITTLE_WEEPS_SERVER_READY (G1 fixture; networking not implemented)");
#endif
        }

        private void Update()
        {
#if UNITY_SERVER
            if (stopped) return;
            if (Time.realtimeSinceStartup >= nextHeartbeat)
            {
                heartbeats++;
                nextHeartbeat = Time.realtimeSinceStartup + 1;
                if (evidenceDirectory != null) WriteRecord("heartbeat");
            }
            if (evidenceDirectory != null && File.Exists(Path.Combine(evidenceDirectory, "stop.request")))
            {
                stopped = true;
                WriteRecord("stopped");
                Debug.Log("LITTLE_WEEPS_SERVER_STOPPED");
                Application.Quit(0);
            }
#endif
        }

        private void WriteRecord(string phase)
        {
            var record = new Record
            {
                phase = phase, runId = runId, build = Application.version, unity = Application.unityVersion,
                utc = DateTime.UtcNow.ToString("O"), graphics = SystemInfo.graphicsDeviceType.ToString(),
                heartbeats = heartbeats, cameras = Camera.allCamerasCount,
                batchMode = Application.isBatchMode, dedicatedServer = true, networkingImplemented = false
            };
            var path = Path.Combine(evidenceDirectory, phase + ".json");
            var pending = path + ".tmp";
            File.WriteAllText(pending, JsonUtility.ToJson(record, true));
            if (File.Exists(path)) File.Replace(pending, path, null);
            else File.Move(pending, path);
        }

        [Serializable] private sealed class Record
        {
            public string phase, runId, build, unity, utc, graphics;
            public int heartbeats, cameras;
            public bool batchMode, dedicatedServer, networkingImplemented;
        }
    }
}
