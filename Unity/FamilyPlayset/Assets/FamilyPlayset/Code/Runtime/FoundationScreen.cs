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
        private const string ProfileKey = "foundation.profile-id";
        private const string CountKey = "foundation.tap-count";
        private TapCount count;
        private Text countLabel;
        private Font font;

        private void Start()
        {
            Application.targetFrameRate = 30;
            count = new TapCount(PlayerPrefs.GetInt(CountKey, 0));
            if (!PlayerPrefs.HasKey(ProfileKey))
            {
                PlayerPrefs.SetString(ProfileKey, Guid.NewGuid().ToString("N"));
                PlayerPrefs.Save();
            }
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Foundation Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1200, 800);
            scaler.matchWidthOrHeight = 0.5f;

            var safe = new GameObject("Safe Area", typeof(RectTransform)).GetComponent<RectTransform>();
            safe.SetParent(canvas.transform, false);
            var area = Screen.safeArea;
            safe.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safe.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            safe.offsetMin = safe.offsetMax = Vector2.zero;

            Label(safe, "Little Weeps", 58, 220, new Vector2(950, 90));
            Label(safe, "Foundation check", 30, 145, new Vector2(950, 55));
            countLabel = Label(safe, "", 42, 55, new Vector2(950, 70));
            var buttonObject = new GameObject("Tap and Save", typeof(RectTransform), typeof(Image), typeof(Button));
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.SetParent(safe, false);
            buttonRect.sizeDelta = new Vector2(460, 130);
            buttonRect.anchoredPosition = new Vector2(0, -65);
            buttonObject.GetComponent<Image>().color = new Color(0.97f, 0.71f, 0.28f);
            var button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(AddAndSave);
            var caption = Label(buttonRect, "TAP & SAVE", 38, 0, new Vector2(440, 120));
            caption.color = new Color(0.10f, 0.19f, 0.23f);
            Label(safe, "Close and reopen: your tap count should stay.", 25, -190, new Vector2(1000, 60));
            Label(safe, "Build " + Application.version, 22, -260, new Vector2(900, 45));
            Refresh();

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            Debug.Log($"LITTLE_WEEPS_FOUNDATION ready build={Application.version} taps={count.Value}");
        }

        private Text Label(Transform parent, string value, int size, float y, Vector2 dimensions)
        {
            var obj = new GameObject(value.Length == 0 ? "Saved Count" : value, typeof(RectTransform), typeof(Text));
            var rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = dimensions;
            rect.anchoredPosition = new Vector2(0, y);
            var label = obj.GetComponent<Text>();
            label.text = value;
            label.font = font;
            label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.1f, 0.2f, 0.25f);
            label.raycastTarget = false;
            return label;
        }

        private void AddAndSave()
        {
            count.Add();
            PlayerPrefs.SetInt(CountKey, count.Value);
            PlayerPrefs.Save();
            Refresh();
            Debug.Log($"LITTLE_WEEPS_FOUNDATION saved taps={count.Value}");
        }
        private void Refresh() => countLabel.text = "Saved taps: " + count.Value;
    }
}
