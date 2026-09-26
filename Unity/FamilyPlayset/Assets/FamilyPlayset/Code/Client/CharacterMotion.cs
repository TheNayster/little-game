using UnityEngine;

namespace LittleWeeps.Client
{
    public enum CharacterPose { Idle, Walk, Wave, Carry }

    public readonly struct CharacterFrame
    {
        public readonly CharacterPose Pose;
        public readonly float Speed;
        public readonly bool FaceLeft;
        public CharacterFrame(CharacterPose pose, float speed, bool faceLeft)
        { Pose = pose; Speed = speed; FaceLeft = faceLeft; }
    }

    // Read-only presentation input. Feed existing displayed movement in game
    // pixels; a new player/area/visit key resets motion history, never game state.
    public sealed class CharacterMotion
    {
        private string continuity;
        private Vector2 previous;
        private bool sampled, faceLeft;

        public CharacterFrame Observe(Vector2 position, string key, bool held, bool wave, float dt)
        {
            var speed = 0f;
            if (sampled && key == continuity && dt > 0 && dt <= .1f)
            {
                var delta = position - previous;
                // A resync/teleport must not produce a sprint or change facing.
                if (delta.magnitude <= Mathf.Max(15, Core.Walking.Speed * dt * 2))
                {
                    speed = Mathf.Clamp(delta.magnitude / dt, 0, Core.Walking.Speed);
                    if (Mathf.Abs(delta.x) > .01f) faceLeft = delta.x < 0;
                }
            }
            previous = position;
            continuity = key;
            sampled = true;
            var pose = held ? CharacterPose.Carry : wave ? CharacterPose.Wave :
                speed > 1 ? CharacterPose.Walk : CharacterPose.Idle;
            return new CharacterFrame(pose, speed, faceLeft);
        }
    }
}
