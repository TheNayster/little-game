using System;
using System.IO;
using System.Threading.Tasks;
using MCPForUnity.Editor.Services;
using MCPForUnity.Editor.Services.Transport;
using UnityEditor;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    [InitializeOnLoad]
    internal static class LittleWeepsReconnect
    {
        private static string lastRequest;
        private static bool busy;
        private static double nextPoll;
        private static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        private static readonly string State = Path.Combine(Root, "Library", "LittleWeepsTools");

        static LittleWeepsReconnect() { EditorApplication.update += Poll; }

        private static async void Poll()
        {
            if (Application.isBatchMode || busy || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || EditorApplication.timeSinceStartup < nextPoll) return;
            nextPoll = EditorApplication.timeSinceStartup + 1;
            string request;
            try
            {
                var file = Path.Combine(State, "connect.request");
                if (!File.Exists(file) || DateTime.UtcNow - File.GetLastWriteTimeUtc(file) > TimeSpan.FromMinutes(10)) return;
                request = File.ReadAllText(file).Trim();
                if (!Guid.TryParse(request, out _) || request == lastRequest) return;
            }
            catch (IOException) { return; }
            busy = true;
            try
            {
                // Use the registered loopback server, never an arbitrary request URL.
                var transport = MCPServiceLocator.TransportManager;
                var ok = await transport.VerifyAsync(TransportMode.Http);
                if (!ok) ok = await transport.StartAsync(TransportMode.Http);
                // StartAsync can return while the server is still assigning a session.
                for (var attempt = 0; ok && attempt < 20; attempt++)
                {
                    var current = transport.GetState(TransportMode.Http);
                    if (!string.IsNullOrEmpty(current.SessionId) && current.SessionId != "pending") break;
                    await Task.Delay(250);
                    ok = await transport.VerifyAsync(TransportMode.Http);
                }
                var status = transport.GetState(TransportMode.Http);
                var ready = ok && status.IsConnected && !string.IsNullOrEmpty(status.SessionId) && status.SessionId != "pending";
                WriteStatus(request, ready, status.Error ?? (ready ? "Connected" : "Server handshake did not finish"), status.SessionId);
                lastRequest = request;
            }
            catch (Exception e) { WriteStatus(request, false, e.Message, null); lastRequest = request; }
            finally { busy = false; }
        }

        private static void WriteStatus(string request, bool connected, string message, string session)
        {
            Directory.CreateDirectory(State);
            File.WriteAllText(Path.Combine(State, "status.json"), JsonUtility.ToJson(new Status
            { request = request, project = Root, connected = connected, message = message, session = session, utc = DateTime.UtcNow.ToString("O") }));
        }

        [Serializable] private sealed class Status
        { public string request, project, message, session, utc; public bool connected; }
    }
}
