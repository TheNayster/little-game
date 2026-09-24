using System;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LittleWeeps.Runtime
{
    // G1 device fixture. This is not the final game menu or world-save format.
    public sealed class FoundationScreen : MonoBehaviour
    {
        public int SavedCount => count.Value;
        public string ProfileId => PlayerPrefs.GetString(FoundationRun.Key("profile-id"));
        public Button TapButton { get; private set; }
        public FoundationVideo Video { get; private set; }
        private TapCount count;
        private Text countLabel, videoStatus;
        private Font font;
        private RectTransform safe;
        private RawImage videoImage;
        private Rect lastSafeArea;

        private void Start()
        {
            Application.targetFrameRate = 30;
            Application.runInBackground = true;
            count = new TapCount(PlayerPrefs.GetInt(FoundationRun.Key("tap-count"), 0));
            if (!PlayerPrefs.HasKey(FoundationRun.Key("profile-id")))
            {
                PlayerPrefs.SetString(FoundationRun.Key("profile-id"), Guid.NewGuid().ToString("N"));
                PlayerPrefs.Save();
            }
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Foundation Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1200, 800);
            // Keep the whole reference layout visible on wide phones and squarer tablets.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            safe = MakeRect(canvasObject.transform, "Safe Area", Vector2.zero, Vector2.zero);
            UpdateSafeArea();
            Label(safe, "Little Weeps", 52, new Vector2(0, 325), new Vector2(1100, 75));
            Label(safe, "Foundation check  /  Build " + Application.version, 26, new Vector2(0, 260), new Vector2(1100, 48));
            countLabel = Label(safe, "", 35, new Vector2(-350, 115), new Vector2(340, 70));
            TapButton = MakeButton(safe, "TAP & SAVE", new Vector2(-350, 10), new Vector2(330, 100), AddAndSave);
            Label(safe, "Your tap count stays when\nyou close and reopen.", 24, new Vector2(-350, -100), new Vector2(340, 90));
            Label(safe, "Local video check", 28, new Vector2(220, 200), new Vector2(620, 50));
            var screen = MakeRect(safe, "Local Video", new Vector2(220, -5), new Vector2(620, 349));
            videoImage = screen.gameObject.AddComponent<RawImage>();
            videoImage.raycastTarget = false;
            videoStatus = Label(safe, "Preparing video...", 20, new Vector2(220, -202), new Vector2(640, 40));
            MakeButton(safe, "PLAY / PAUSE", new Vector2(-15, -255), new Vector2(220, 70), () => { if (Video.Player.isPlaying) Video.Pause(); else Video.Play(); });
            MakeButton(safe, "+2 SECONDS", new Vector2(220, -255), new Vector2(220, 70), () => Video.Seek(Video.Player.time + 2));
            MakeButton(safe, "START OVER", new Vector2(455, -255), new Vector2(220, 70), () => Video.Restart());
            Label(safe, "Technical test scene - game worlds come later", 21, new Vector2(0, -350), new Vector2(1100, 45));
            OpenVideo(); Refresh();
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            if (FoundationRun.Automated) gameObject.AddComponent<FoundationVerification>();
            Debug.Log($"LITTLE_WEEPS_FOUNDATION ready build={Application.version} taps={count.Value}");
        }
        public void OpenVideo()
        {
            Video = new GameObject("Local Video Probe").AddComponent<FoundationVideo>();
            Video.transform.SetParent(transform, false); Video.Initialize(); videoImage.texture = Video.Texture;
        }
        public void CloseVideo() { Video.Pause(); Destroy(Video.gameObject); Video = null; videoImage.texture = null; }
        private void Update()
        {
            if (safe != null && lastSafeArea != Screen.safeArea) UpdateSafeArea();
            if (Video == null) return;
            videoStatus.text = Video.Failure ?? (!Video.Ready ? "Preparing video..." :
                $"{(Video.Player.isPlaying ? "Playing" : "Paused")}  {Video.Player.time:0.0}s / {Video.Player.length:0.0}s  |  Saved {Video.SavedPosition:0.0}s");
        }
        private void UpdateSafeArea()
        {
            lastSafeArea = Screen.safeArea;
            safe.anchorMin = new Vector2(lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height);
            safe.anchorMax = new Vector2(lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height);
            safe.offsetMin = safe.offsetMax = Vector2.zero;
        }
        private RectTransform MakeRect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.sizeDelta = size; rect.anchoredPosition = position; return rect;
        }
        private Text Label(Transform parent, string value, int size, Vector2 position, Vector2 dimensions)
        {
            var label = MakeRect(parent, value.Length == 0 ? "Saved Count" : value, position, dimensions).gameObject.AddComponent<Text>();
            label.text = value; label.font = font; label.fontSize = size; label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.1f, 0.2f, 0.25f); label.raycastTarget = false; return label;
        }
        private Button MakeButton(Transform parent, string title, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction clicked)
        {
            var rect = MakeRect(parent, title, position, size);
            rect.gameObject.AddComponent<Image>().color = new Color(0.97f, 0.71f, 0.28f);
            var button = rect.gameObject.AddComponent<Button>(); button.onClick.AddListener(clicked);
            Label(rect, title, 25, Vector2.zero, size); return button;
        }
        private void AddAndSave()
        {
            count.Add(); PlayerPrefs.SetInt(FoundationRun.Key("tap-count"), count.Value); PlayerPrefs.Save(); Refresh();
            Debug.Log($"LITTLE_WEEPS_FOUNDATION saved taps={count.Value}");
        }
        private void Refresh() => countLabel.text = "Saved taps: " + count.Value;
    }
}
