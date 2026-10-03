using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LittleWeeps.Client
{
    // Isolated art fixture: Walking.Step supplies the same movement as the game,
    // while this scene has no session, enrollment, persistence or networking.
    public sealed class CharacterWorkshop : MonoBehaviour
    {
        public CharacterView character, companion;
        public Camera viewCamera;
        private readonly CharacterMotion motion = new CharacterMotion();
        private Vector2 point = new Vector2(360, 140);
        private string selected = "idle";
        private bool paused, left, guides;
        private bool verifying;
        private float speed = 1;
        private GUIStyle title, subtitle, button, selectedButton, small;
        private Texture2D cardTexture, buttonTexture, selectedTexture;
        private int frameCount;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
                if (args[i] == "-characterEvidence") { verifying = true; StartCoroutine(Verify(args[i + 1])); }
        }

        private void Update()
        {
            if (verifying) return;
            if (paused)
            {
                // Allow visual choices while the animation clock is paused.
                var pose = selected == "carry" ? CharacterPose.Carry : selected == "wave" ? CharacterPose.Wave :
                    selected == "walk" ? CharacterPose.Walk : CharacterPose.Idle;
                character.Present(new CharacterFrame(pose, character.Frame.Speed, left), 0);
                return;
            }
            var dt = Mathf.Min(Time.deltaTime, .05f);
            var direction = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) direction.x--;
                if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) direction.x++;
                if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) direction.y++;
                if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) direction.y--;
            }
            if (direction == Vector2.zero && (selected == "walk" || selected == "carry"))
            {
                if (point.x < 260) left = false;
                if (point.x > 650) left = true;
                direction.x = left ? -speed : speed;
            }
            var next = Walking.Step(point.x, point.y,
                new WalkInput { mode = WalkMode.Direction, x = direction.x, y = direction.y }, dt);
            point = new Vector2(next.X, next.Y);
            var frame = motion.Observe(point, "workshop/player/garden/1", selected == "carry", selected == "wave", dt);
            if (frame.Speed > 1) left = frame.FaceLeft;
            frame = new CharacterFrame(frame.Pose, frame.Speed, left);
            character.transform.parent.position = Floor(point);
            character.Present(frame, dt);
            character.SetFloorDepth(character.transform.parent.position.y);
            companion.Present(new CharacterFrame(CharacterPose.Idle, 0, true), dt);
            companion.SetFloorDepth(companion.transform.parent.position.y);
        }

        private static Vector3 Floor(Vector2 value) => new Vector3((value.x - 500) / 100, (value.y - 300) / 100, 0);

        private void SelectCharacter(string id)
        {
            if (character.characterId == id) return;
            var frame = character.Frame;
            var actorRoot = character.transform.parent;
            var otherRoot = companion.transform.parent;
            character.transform.SetParent(otherRoot, false);
            companion.transform.SetParent(actorRoot, false);
            (character, companion) = (companion, character);
            character.Present(frame, 0);
            character.SetFloorDepth(actorRoot.position.y);
            companion.Present(new CharacterFrame(CharacterPose.Idle, 0, true), 0);
            companion.SetFloorDepth(otherRoot.position.y);
        }

        private void OnGUI()
        {
            EnsureStyles();
            var scale = Mathf.Min(Screen.width / 1280f, Screen.height / 800f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            GUI.Label(new Rect(42, 28, 750, 48), "Bluey & Bingo", title);
            GUI.Label(new Rect(45, 83, 810, 34), "The character workshop", subtitle);
            GUI.DrawTexture(new Rect(944, 28, 294, 742), cardTexture);
            GUI.Label(new Rect(969, 51, 246, 30), "CHOOSE A CHARACTER", small);
            if (GUI.Button(new Rect(968, 94, 116, 44), "Bluey", character.characterId == "bluey" ? selectedButton : button)) SelectCharacter("bluey");
            if (GUI.Button(new Rect(1098, 94, 116, 44), "Bingo", character.characterId == "bingo" ? selectedButton : button)) SelectCharacter("bingo");
            GUI.Label(new Rect(969, 160, 246, 30), "TRY A MOVEMENT", small);
            var names = new[] { "idle", "walk", "wave", "carry" };
            var labels = new[] { "A quiet moment", "Go for a walk", "Wave hello", "Carry & walk" };
            for (var i = 0; i < names.Length; i++)
                if (GUI.Button(new Rect(968, 201 + i * 51, 246, 42), labels[i], selected == names[i] ? selectedButton : button)) selected = names[i];
            GUI.Label(new Rect(969, 418, 245, 28), "FACING", small);
            if (GUI.Button(new Rect(968, 457, 116, 42), "Left", left ? selectedButton : button)) left = true;
            if (GUI.Button(new Rect(1098, 457, 116, 42), "Right", !left ? selectedButton : button)) left = false;
            GUI.Label(new Rect(969, 521, 245, 28), "WALKING PACE", small);
            speed = GUI.HorizontalSlider(new Rect(971, 562, 240, 24), speed, .3f, 1);
            if (GUI.Button(new Rect(968, 605, 246, 42), paused ? "Resume" : "Pause", button)) paused = !paused;
            if (GUI.Button(new Rect(968, 665, 246, 42), guides ? "Hide hand anchor" : "Show hand anchor", button)) guides = !guides;
            GUI.Label(new Rect(969, 729, 246, 32), "Arrow keys / WASD to explore", small);
            GUI.Label(new Rect(45, 691, 835, 32), "Idle  /  walk  /  wave  /  carry", subtitle);
            GUI.Label(new Rect(45, 732, 835, 32), "Character study · game integration and iPad testing are still ahead.", small);
            if (guides)
            {
                var p = viewCamera.WorldToScreenPoint(character.handAnchor.position);
                GUI.Label(new Rect(p.x / scale - 10, (Screen.height - p.y) / scale - 16, 180, 32), "+  hand anchor", subtitle);
            }
        }

        private void EnsureStyles()
        {
            if (title != null) return;
            cardTexture = Texture(new Color(.99f, .98f, .94f));
            buttonTexture = Texture(new Color(.91f, .94f, .89f));
            selectedTexture = Texture(new Color(.20f, .38f, .32f));
            title = new GUIStyle(GUI.skin.label) { fontSize = 42, fontStyle = FontStyle.Bold };
            title.normal.textColor = new Color(.16f, .29f, .26f);
            subtitle = new GUIStyle(title) { fontSize = 21, fontStyle = FontStyle.Normal };
            small = new GUIStyle(subtitle) { fontSize = 15, wordWrap = true };
            button = new GUIStyle(GUI.skin.button) { fontSize = 19, alignment = TextAnchor.MiddleCenter };
            button.normal.background = buttonTexture; button.normal.textColor = title.normal.textColor;
            button.hover.background = buttonTexture; button.hover.textColor = title.normal.textColor;
            selectedButton = new GUIStyle(button);
            selectedButton.normal.background = selectedTexture; selectedButton.normal.textColor = Color.white;
            selectedButton.hover.background = selectedTexture; selectedButton.hover.textColor = Color.white;
        }

        private static Texture2D Texture(Color color)
        { var result = new Texture2D(1, 1); result.SetPixel(0, 0, color); result.Apply(); return result; }

        private void OnDestroy()
        {
            if (cardTexture != null) Destroy(cardTexture);
            if (buttonTexture != null) Destroy(buttonTexture);
            if (selectedTexture != null) Destroy(selectedTexture);
        }

        private IEnumerator Verify(string folder)
        {
            Directory.CreateDirectory(folder);
            yield return null;
            var checks = new List<string>();
            string failure = null;
            try { RunChecks(checks); }
            catch (Exception error) { failure = error.ToString(); Debug.LogException(error); }
            if (failure == null)
            {
                foreach (var id in new[] { "bluey", "bingo" })
                {
                SelectCharacter(id);
                foreach (var pose in new[] { CharacterPose.Idle, CharacterPose.Walk, CharacterPose.Wave, CharacterPose.Carry })
                {
                    selected = pose.ToString().ToLowerInvariant();
                    character.transform.parent.position = new Vector3(-1.4f, -1.7f, 0);
                    character.SetFloorDepth(-1.7f);
                    for (var i = 0; i < 30; i++) { character.Present(new CharacterFrame(pose, pose == CharacterPose.Walk || pose == CharacterPose.Carry ? Walking.Speed : 0, false), 1f / 60); yield return null; }
                    yield return new WaitForEndOfFrame();
                    CaptureScene(Path.Combine(folder, id + "-" + selected + ".png"));
                    yield return null;
                }
                left = true; character.Present(new CharacterFrame(CharacterPose.Carry, Walking.Speed, true), .016f);
                yield return new WaitForEndOfFrame();
                CaptureScene(Path.Combine(folder, id + "-carry-left.png"));
                yield return null;
                }
            }
            File.WriteAllText(Path.Combine(folder, "runtime-checks.json"), JsonUtility.ToJson(new Evidence {
                passed = failure == null, checks = checks.ToArray(), error = failure, unity = Application.unityVersion,
                product = Application.productName, framesChecked = frameCount, scope = "Isolated native Windows character workshop; no game session, device qualification or production likeness claim."
            }, true));
            yield return new WaitForSeconds(.3f);
            Application.Quit(failure == null ? 0 : 1);
        }

        private void CaptureScene(string path)
        {
            // Hidden verification windows have no readable display backbuffer.
            // Render the actual camera explicitly; this proves scene pixels,
            // while the visible workshop controls receive a separate UI check.
            var target = new RenderTexture(1280, 800, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(1280, 800, TextureFormat.RGB24, false);
            var priorTarget = viewCamera.targetTexture;
            var priorActive = RenderTexture.active;
            try
            {
                viewCamera.targetTexture = target; viewCamera.Render(); RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0); texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                viewCamera.targetTexture = priorTarget; RenderTexture.active = priorActive;
                target.Release(); Destroy(target); Destroy(texture);
            }
        }

        private void RunChecks(List<string> checks)
        {
            void Check(bool condition, string description) { if (!condition) throw new Exception(description); checks.Add(description); }
            var source = new CharacterMotion();
            var first = source.Observe(new Vector2(500, 200), "player/garden/1", false, false, .02f);
            Check(first.Pose == CharacterPose.Idle, "First position sample stays idle");
            var next = Walking.Step(500, 200, new WalkInput { mode = WalkMode.Direction, x = -1 }, .02f);
            var walking = source.Observe(new Vector2(next.X, next.Y), "player/garden/1", false, false, .02f);
            Check(walking.Pose == CharacterPose.Walk && walking.FaceLeft && Mathf.Abs(walking.Speed - Walking.Speed) < .1f, "Existing Walking.Step drives speed and facing");
            var stop = source.Observe(new Vector2(next.X, next.Y), "player/garden/1", false, false, .02f);
            Check(stop.Pose == CharacterPose.Idle && stop.FaceLeft, "Stopping preserves facing and stops stride");
            Check(source.Observe(new Vector2(next.X, next.Y), "player/garden/1", false, true, .02f).Pose == CharacterPose.Wave, "Wave poses a stationary character");
            Check(source.Observe(new Vector2(next.X, next.Y), "player/garden/1", true, true, .02f).Pose == CharacterPose.Carry, "Confirmed carry takes precedence over waving");
            Check(source.Observe(new Vector2(900, 400), "player/creek/2", false, false, .02f).Speed == 0, "Area/visit change resets velocity history");
            Check(source.Observe(new Vector2(100, 200), "player/creek/2", false, false, .02f).Speed == 0, "Position correction does not animate a sprint");
            Check(source.Observe(new Vector2(101, 200), "player/creek/2", false, false, 2).Speed == 0, "Resume after a long frame resets observed movement");

            foreach (var id in new[] { "bluey", "bingo" })
            {
            SelectCharacter(id);
            var root = character.transform.parent;
            var position = root.position; var rotation = root.rotation; var scale = root.localScale;
            var visualPosition = character.transform.localPosition;
            var originalLocal = character.handAnchor.localPosition;
            var maximumGripError = 0f; var maximumTilt = 0f;
            foreach (var faceLeft in new[] { false, true })
                foreach (var pose in new[] { CharacterPose.Idle, CharacterPose.Walk, CharacterPose.Wave, CharacterPose.Carry })
                    for (var i = 0; i < 180; i++)
                    {
                        character.Present(new CharacterFrame(pose, pose == CharacterPose.Walk || pose == CharacterPose.Carry ? Walking.Speed : 0, faceLeft), 1f / 60);
                        frameCount++;
                        if (pose == CharacterPose.Carry)
                        {
                            maximumGripError = Mathf.Max(maximumGripError, Vector3.Distance(character.handAnchor.position, character.heldProp.position));
                            maximumTilt = Mathf.Max(maximumTilt, Vector3.Angle(character.heldProp.TransformVector(Vector3.up), Vector3.up));
                        }
                        if (character.heldProp.gameObject.activeSelf != (pose == CharacterPose.Carry)) throw new Exception("Prop visibility does not match carried state");
                    }
            Check(root.position == position && root.rotation == rotation && root.localScale == scale && character.transform.localPosition == visualPosition,
                "1440 pose frames preserve gameplay root and visual floor pivot");
            Check(character.handAnchor.localPosition == originalLocal && maximumGripError < .00001f, "Bucket grip remains attached to the animated hand in both facings");
            Check(maximumTilt < .05f, "Carried bucket stays upright in both facings");
            character.Present(new CharacterFrame(CharacterPose.Carry, 0, true), 0);
            Check(Quaternion.Angle(character.footNear.localRotation, Quaternion.identity) < .001f &&
                Quaternion.Angle(character.footFar.localRotation, Quaternion.identity) < .001f, "Stationary carry plants both feet");
            character.Present(new CharacterFrame(CharacterPose.Idle, 0, false), .01f);
            Check(!character.heldProp.gameObject.activeSelf, "Releasing carry removes the prop presentation");
            character.SetFloorDepth(-2); companion.SetFloorDepth(-1);
            Check(character.sorting.sortingOrder > companion.sorting.sortingOrder, "Lower floor position sorts the entire character and prop in front");
            character.SetFloorDepth(0);
            Check(character.sorting.sortingOrder < companion.sorting.sortingOrder, "Crossing behind reverses character depth order");
            Check(character.heldProp.GetComponentInChildren<SpriteRenderer>().sortingOrder > character.armNear.GetComponentInChildren<SpriteRenderer>().sortingOrder,
                "Held prop renders within the character above its near arm");
            Check(character.GetComponentsInChildren<SpriteRenderer>(true).Length == 12, id + ": all twelve imported sprite layers are present");
            }
            SelectCharacter("bluey");
            var actorRoot = character.transform.parent;
            var actorPosition = actorRoot.position;
            character.Present(new CharacterFrame(CharacterPose.Carry, Walking.Speed, true), .01f);
            var pointBefore = point;
            SelectCharacter("bingo");
            Check(character.characterId == "bingo" && character.transform.parent == actorRoot && actorRoot.position == actorPosition && point == pointBefore &&
                character.Frame.Pose == CharacterPose.Carry && character.Frame.FaceLeft && character.heldProp.gameObject.activeSelf,
                "Bluey to Bingo switch preserves movement root, position, facing and carried state");
            SelectCharacter("bluey");
            Check(character.Frame.Pose == CharacterPose.Carry && character.heldProp.gameObject.activeSelf && !companion.heldProp.gameObject.activeSelf,
                "Switch back transfers the carried presentation without duplicating it on the other visual");
            Check(FindObjectsByType<GameScreen>(FindObjectsSortMode.None).Length == 0, "Workshop contains no game session or save screen");
        }

        [Serializable] private sealed class Evidence
        { public bool passed; public string[] checks; public string error, unity, product, scope; public int framesChecked; }
    }
}
