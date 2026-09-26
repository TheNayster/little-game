using UnityEngine;
using UnityEngine.Rendering;

namespace LittleWeeps.Client
{
    // Replaceable visual child of a gameplay root. Owns neither position,
    // inventory nor saves; a supplied frame only poses the imported joints.
    public sealed class CharacterView : MonoBehaviour
    {
        public string characterId, displayName;
        public Transform facing, body, head, tail, armFar, armNear, footFar, footNear, handAnchor, heldProp;
        public SpriteRenderer eyesOpen, eyesClosed;
        public SortingGroup sorting;
        private Vector3 bodyRest, headRest, farRest, nearRest;
        private bool initialized;
        private float time, cycle;
        public CharacterFrame Frame { get; private set; }

        private void Initialize()
        {
            if (initialized) return;
            bodyRest = body.localPosition; headRest = head.localPosition;
            farRest = armFar.localPosition; nearRest = armNear.localPosition;
            initialized = true;
        }

        public void Present(CharacterFrame frame, float deltaTime)
        {
            Initialize(); Frame = frame;
            var dt = Mathf.Clamp(deltaTime, 0, .1f);
            time += dt;
            var stride = Mathf.Clamp01(frame.Speed / Core.Walking.Speed);
            cycle = Mathf.Repeat(cycle + dt * Mathf.PI * 2 * 1.35f * stride, Mathf.PI * 2);
            var walking = stride > .005f;
            var swing = walking ? Mathf.Sin(cycle) * 19 * stride : 0;
            var bob = walking ? Mathf.Abs(Mathf.Sin(cycle)) * .03f * stride : -Mathf.Sin(time * 2) * .012f;
            facing.localScale = new Vector3(frame.FaceLeft ? -1 : 1, 1, 1);
            body.localPosition = bodyRest + Vector3.up * bob;
            head.localPosition = headRest + Vector3.up * bob;
            // The face markings belong to the continuous box-shaped torso.
            // Do not rotate them as a detached round head over the body outline.
            head.localRotation = Quaternion.identity;
            tail.localRotation = Rotation(-Mathf.Sin(time * (frame.Pose == CharacterPose.Wave ? 9 : 4)) * 5);
            footFar.localRotation = Rotation(-swing);
            footNear.localRotation = Rotation(swing);
            armFar.localPosition = farRest + Vector3.up * bob;
            armNear.localPosition = nearRest + Vector3.up * bob;
            armFar.localRotation = Rotation(swing * .65f);
            var armAngle = frame.Pose == CharacterPose.Wave ? 132 - Mathf.Sin(time * 7) * 14 :
                frame.Pose == CharacterPose.Carry ? 12 : -swing * .7f;
            armNear.localRotation = Rotation(armAngle);
            heldProp.gameObject.SetActive(frame.Pose == CharacterPose.Carry);
            // Counter-rotate inside the mirrored hierarchy; the grip stays on
            // the hand and the bucket stays upright in both facing directions.
            heldProp.localRotation = Rotation(-armAngle);
            var blink = time % 4.3f > 4.14f;
            eyesOpen.enabled = !blink; eyesClosed.enabled = blink;
        }

        public void SetFloorDepth(float worldY)
        {
            sorting.sortingOrder = Mathf.Clamp(-Mathf.RoundToInt(worldY * 100), -30000, 30000);
        }

        private static Quaternion Rotation(float degrees) => Quaternion.Euler(0, 0, degrees);
    }
}
