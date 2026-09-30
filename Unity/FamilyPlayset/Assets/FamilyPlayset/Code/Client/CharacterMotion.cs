using UnityEngine;

namespace LittleWeeps.Client
{
    public enum CharacterPose { Idle, Walk, Wave, Carry, Sit, Bounce, Dance, BalloonTap, Rest, Roar }

    public readonly struct CharacterFrame
    {
        public readonly CharacterPose Pose;
        public readonly float Speed, UseSeconds;
        public readonly bool FaceLeft;
        public readonly Vector2 Travel;
        public readonly bool ResetMotion;
        public CharacterFrame(CharacterPose pose, float speed, bool faceLeft, float useSeconds=0,
            Vector2 travel=default, bool resetMotion=false)
        { Pose = pose; Speed = speed; FaceLeft = faceLeft; UseSeconds=useSeconds; Travel=travel; ResetMotion=resetMotion; }
    }

    // Read-only presentation input. Feed existing displayed movement in game
    // pixels; a new player/area/visit key resets motion history, never game state.
    public sealed class CharacterMotion
    {
        private string continuity;
        private Vector2 previous;
        private bool sampled, faceLeft;
        private float turnDistance;

        public CharacterFrame Observe(Vector2 position, string key, bool held, bool wave, float dt)
        {
            var speed = 0f;
            var travel=Vector2.zero;
            var reset=!sampled || key!=continuity || dt<=0 || dt>.1f;
            if (sampled && key == continuity && dt > 0 && dt <= .1f)
            {
                var delta = position - previous;
                // A resync/teleport must not produce a sprint or change facing.
                if (delta.magnitude <= Mathf.Max(15, Core.Walking.Speed * dt * 2))
                {
                    speed = Mathf.Clamp(delta.magnitude / dt, 0, Core.Walking.Speed);
                    travel=Vector2.ClampMagnitude(delta,Core.Walking.Speed*dt);
                    // Ignore tiny alternating render corrections near rest, but
                    // allow a deliberate reversal within a normal walking frame.
                    if(Mathf.Abs(delta.x)>.01f)
                    {
                        turnDistance=Mathf.Sign(delta.x)==Mathf.Sign(turnDistance)?turnDistance+delta.x:delta.x;
                        if(Mathf.Abs(turnDistance)>.8f)faceLeft=turnDistance<0;
                    }
                }
                else reset=true;
            }
            if(reset)turnDistance=0;
            previous = position;
            continuity = key;
            sampled = true;
            var pose = held ? CharacterPose.Carry : wave ? CharacterPose.Wave :
                speed > 1 ? CharacterPose.Walk : CharacterPose.Idle;
            return new CharacterFrame(pose, speed, faceLeft,travel:travel,resetMotion:reset);
        }
    }
}
