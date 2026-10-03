using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace LittleWeeps.Runtime
{
    // Opt-in native-player integration checks. This fixture never runs in ordinary play.
    public sealed class FoundationVerification : MonoBehaviour
    {
        private string runtimeError;
        private void OnEnable() => Application.logMessageReceived += Log;
        private void OnDisable() => Application.logMessageReceived -= Log;
        private void Log(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) runtimeError = message; }

        private async void Start()
        {
            var result = new Result { mode = FoundationRun.Mode, build = Application.version, runId = FoundationRun.RunId };
            Directory.CreateDirectory(FoundationRun.EvidenceDirectory);
            try
            {
                var screen = GetComponent<FoundationScreen>();
                await Wait(() => screen.Video.Ready && !screen.Video.Seeking, "Prepare local video");
                if (FoundationRun.Mode == "bookmark" || FoundationRun.Mode == "bookmark-resume")
                {
                    await VerifyPausedBookmark(screen, result);
                    result.passed = true;
                }
                else
                {
                var initialBookmark = screen.Video.SavedPosition;
                if (FoundationRun.Mode == "seed")
                {
                    Require(screen.SavedCount == 0, "Verification run must start in a fresh namespace.");
                    for (var i = 0; i < 3; i++) screen.TapButton.onClick.Invoke();
                    Require(screen.SavedCount == 3, "Button event did not persist three taps.");
                }
                else
                {
                    var seed = JsonUtility.FromJson<Result>(File.ReadAllText(Path.Combine(FoundationRun.EvidenceDirectory, "seed.json")));
                    Require(seed.passed, "Seed check did not pass.");
                    Require(screen.ProfileId == seed.profileId, "Profile changed after relaunch/update.");
                    Require(screen.SavedCount == seed.taps, "Saved tap count changed after relaunch/update.");
                    Require(Math.Abs(initialBookmark - seed.bookmark) < 0.25, "Video bookmark changed after relaunch/update.");
                    Require(Math.Abs(screen.Video.Player.time - seed.bookmark) < 0.3, "Video did not seek to its saved position.");
                    Require(FoundationRun.Mode != "update" || seed.build != Application.version, "Update test must use a different build.");
                }

                var start = screen.Video.Player.time;
                screen.Video.Play();
                await Wait(() => screen.Video.DecodedFrames >= 5 && screen.Video.Player.time > start + 0.4, "Decode moving local video");
                screen.Video.Pause();
                await Task.Delay(100);
                var pausedAt = screen.Video.Player.time;
                await Task.Delay(400);
                Require(!screen.Video.Player.isPlaying && Math.Abs(screen.Video.Player.time - pausedAt) < 0.12, "Pause did not hold position.");
                screen.Video.Seek(4);
                await Wait(() => !screen.Video.Seeking && Math.Abs(screen.Video.Player.time - 4) < 0.25, "Seek local video");
                screen.Video.Checkpoint();
                var saved = screen.Video.SavedPosition;
                var frames = screen.Video.DecodedFrames;
                screen.CloseVideo();
                await Task.Delay(100);
                screen.OpenVideo();
                await Wait(() => screen.Video.Ready && !screen.Video.Seeking && Math.Abs(screen.Video.Player.time - saved) < 0.3,
                    "Reopen local video at saved position");
                Require(!screen.Video.Player.isPlaying, "Returning should wait for Play.");
                Require(runtimeError == null, runtimeError ?? "Unexpected player error.");
                result.profileId = screen.ProfileId; result.taps = screen.SavedCount;
                result.bookmark = screen.Video.SavedPosition; result.decodedFrames = frames;
                result.videoUrl = screen.Video.Player.url; result.graphics = SystemInfo.graphicsDeviceName;
                result.passed = true;
                ScreenCapture.CaptureScreenshot(Path.Combine(FoundationRun.EvidenceDirectory, FoundationRun.Mode + ".png"));
                await Task.Delay(800);
                }
            }
            catch (Exception e) { result.passed = false; result.error = e.Message; }
            result.utc = DateTime.UtcNow.ToString("O");
            File.WriteAllText(Path.Combine(FoundationRun.EvidenceDirectory, FoundationRun.Mode + ".json"), JsonUtility.ToJson(result, true));
            Debug.Log("LITTLE_WEEPS_VERIFY " + JsonUtility.ToJson(result));
            Application.Quit(result.passed ? 0 : 1);
        }
        private async Task VerifyPausedBookmark(FoundationScreen screen, Result result)
        {
            // Use an off-frame time to expose accidental decoder rounding writes.
            if (FoundationRun.Mode == "bookmark")
            {
                Require(screen.SavedCount == 0, "Bookmark run must use a fresh namespace.");
                for (var i = 0; i < 3; i++) screen.TapButton.onClick.Invoke();
                screen.Video.Seek(4.013);
                await Wait(() => screen.Video.Ready && !screen.Video.Seeking, "Seed paused bookmark");
                result.bookmark = screen.Video.SavedPosition;
                Require(result.bookmark == (double)(float)4.013, "Seek did not save the requested time.");
            }
            else
            {
                var seed = JsonUtility.FromJson<Result>(File.ReadAllText(Path.Combine(FoundationRun.EvidenceDirectory, "bookmark.json")));
                Require(seed.passed, "Bookmark seed must pass before relaunch.");
                Require(screen.ProfileId == seed.profileId && screen.SavedCount == seed.taps, "Relaunch changed profile/taps.");
                result.bookmark = seed.bookmark;
            }
            result.profileId = screen.ProfileId; result.taps = screen.SavedCount;
            result.graphics = SystemInfo.graphicsDeviceName;
            for (var i = 0; i < 30; i++)
            {
                await Task.Delay(100);
                screen.Video.Pause();
                screen.Video.Checkpoint();
                result.observedBookmark = screen.Video.SavedPosition;
                result.maxBookmarkDrift = Math.Max(result.maxBookmarkDrift, Math.Abs(result.observedBookmark - result.bookmark));
                Require(result.observedBookmark == result.bookmark, "Paused checkpoint changed the bookmark without playback.");
                screen.CloseVideo();
                await Task.Delay(50);
                screen.OpenVideo();
                await Wait(() => screen.Video.Ready && !screen.Video.Seeking, "Repeated paused reopen");
                Require(!screen.Video.Player.isPlaying, "Reopen started playback without Play.");
                Require(Math.Abs(screen.Video.Player.time - result.bookmark) < 0.3, "Reopen did not display the saved frame.");
                result.reopenCycles++;
                result.observedBookmark = screen.Video.SavedPosition;
                Require(result.observedBookmark == result.bookmark, "Reopening rewrote the paused bookmark.");
            }
            // Pause and lifecycle suspension must cancel autoplay while a seek
            // is decoding, without stranding the controller in its Seeking state.
            for (var i = 0; i < 2; i++)
            {
                screen.Video.Play();
                var start = screen.Video.Player.time;
                await Wait(() => screen.Video.Player.time > start + 0.2, "Playback before interrupted seek");
                screen.Video.Seek(7.013);
                if (i == 0) screen.Video.Pause();
                else screen.Video.SendMessage("OnApplicationPause", true);
                await Wait(() => screen.Video.Ready && !screen.Video.Seeking, "Complete interrupted seek");
                await Task.Delay(150);
                Require(!screen.Video.Player.isPlaying, "Pause during seek resumed playback.");
                Require(screen.Video.SavedPosition == (double)(float)7.013, "Interrupted seek saved an intermediate frame.");
                result.interruptedSeekChecks++;
            }
            screen.Video.Restart();
            await Wait(() => screen.Video.Ready && !screen.Video.Seeking, "Start over after seek interruption");
            Require(screen.Video.SavedPosition == 0, "Start over did not save zero.");
            screen.Video.Seek(result.bookmark);
            await Wait(() => screen.Video.Ready && !screen.Video.Seeking, "Restore relaunch baseline");
            screen.Video.Pause();
            Require(screen.Video.SavedPosition == result.bookmark, "Final paused checkpoint changed baseline.");
            Require(runtimeError == null, runtimeError ?? "Unexpected video error.");
        }
        private async Task Wait(Func<bool> condition, string check)
        {
            var until = Time.realtimeSinceStartup + 25;
            while (!condition())
            {
                Require(runtimeError == null, runtimeError ?? check);
                Require(Time.realtimeSinceStartup < until, check + " timed out.");
                await Task.Delay(50);
            }
        }
        private static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
        [Serializable] private sealed class Result
        {
            public bool passed;
            public string mode, build, runId, profileId, videoUrl, graphics, utc, error;
            public int taps, decodedFrames, reopenCycles, interruptedSeekChecks;
            public double bookmark, observedBookmark, maxBookmarkDrift;
        }
    }
}
