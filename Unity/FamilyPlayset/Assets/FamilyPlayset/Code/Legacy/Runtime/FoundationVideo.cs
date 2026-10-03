using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace LittleWeeps.Runtime
{
    // A bounded G1 local-media probe, not the final TV library/controller.
    public sealed class FoundationVideo : MonoBehaviour
    {
        public VideoPlayer Player { get; private set; }
        public RenderTexture Texture { get; private set; }
        public bool Ready { get; private set; }
        public bool Seeking { get; private set; }
        public string Failure { get; private set; }
        public int DecodedFrames { get; private set; }
        public double RestoredPosition { get; private set; }
        public double SavedPosition => PlayerPrefs.GetFloat(BookmarkKey, 0);
        private string BookmarkKey => FoundationRun.Key("video." + PlayerPrefs.GetString(FoundationRun.Key("profile-id")) + ".g1-clip-v1");
        private float deadline, nextCheckpoint;
        private double seekTarget;
        // Decoder playback used to present a seek frame is not a child's Play request.
        private bool playRequested, presentedFrame;

        public void Initialize()
        {
            Texture = new RenderTexture(640, 360, 0, RenderTextureFormat.ARGB32);
            Player = gameObject.AddComponent<VideoPlayer>();
            Player.playOnAwake = false; Player.isLooping = false; Player.waitForFirstFrame = true;
            Player.renderMode = VideoRenderMode.RenderTexture; Player.targetTexture = Texture;
            Player.audioOutputMode = VideoAudioOutputMode.Direct;
            Player.SetDirectAudioVolume(0, FoundationRun.Automated ? 0 : 0.12f);
            Player.sendFrameReadyEvents = true;
            Player.frameReady += FrameReady;
            Player.errorReceived += (_, error) => { Failure = error; Ready = false; Debug.LogError("Local video: " + error); };
            Player.prepareCompleted += Prepared;
            Player.loopPointReached += _ => Pause();
            Player.url = Path.Combine(Application.streamingAssetsPath, "Foundation", "test-clip.mp4");
            deadline = Time.realtimeSinceStartup + 20;
            Player.Prepare();
        }
        private void Prepared(VideoPlayer player)
        {
            Ready = true;
            RestoredPosition = Math.Min(SavedPosition, Math.Max(0, player.length - 0.1));
            if (RestoredPosition > 0.05) Seek(RestoredPosition);
            nextCheckpoint = Time.realtimeSinceStartup + 5;
        }
        public void Play() { if (Ready && !Seeking) { playRequested = true; Player.Play(); } }
        public void Pause()
        {
            if (Player == null) return;
            if (Seeking)
            {
                // Finish presenting the requested frame, then stay paused even if
                // the seek began during playback. Do not save an intermediate frame.
                playRequested = false;
                return;
            }
            Player.Pause(); Checkpoint(); playRequested = false;
        }
        public void Seek(double seconds)
        {
            if (!Ready || Seeking || !Player.canSetTime) return;
            Seeking = true; Ready = false; deadline = Time.realtimeSinceStartup + 20;
            seekTarget = Math.Max(0, Math.Min(seconds, Math.Max(0, Player.length - 0.1)));
            Player.time = seekTarget;
            // A freshly prepared, paused Windows decoder may complete a seek
            // before it presents that frame. Decode it before exposing readiness.
            Player.Play();
        }
        private void FrameReady(VideoPlayer player, long frame)
        {
            DecodedFrames++; presentedFrame = true;
            if (!Seeking || Math.Abs(frame / player.frameRate - seekTarget) > 0.25) return;
            Seeking = false; Ready = true;
            if (!playRequested) player.Pause();
            PlayerPrefs.SetFloat(BookmarkKey, (float)seekTarget); PlayerPrefs.Save();
        }
        public void Restart() { Pause(); Seek(0); }
        public void Checkpoint()
        {
            if (!playRequested || Player == null || !Player.isPrepared || !Ready || !presentedFrame || Seeking || Failure != null) return;
            PlayerPrefs.SetFloat(BookmarkKey, (float)Player.time); PlayerPrefs.Save();
        }
        private void Update()
        {
            if (Failure != null || Player == null) return;
            if ((!Ready || Seeking) && Time.realtimeSinceStartup > deadline)
            { Failure = "Local video preparation or seeking timed out."; Ready = false; Debug.LogError(Failure); }
            if (Ready && Player.isPlaying && Time.realtimeSinceStartup >= nextCheckpoint)
            { Checkpoint(); nextCheckpoint = Time.realtimeSinceStartup + 5; }
        }
        private void OnApplicationPause(bool paused) { if (paused) Pause(); }
        private void OnApplicationQuit() => Checkpoint();
        private void OnDestroy()
        {
            Checkpoint();
            if (Player != null) Player.Stop();
            if (Texture != null) { Texture.Release(); Destroy(Texture); }
        }
    }
}
