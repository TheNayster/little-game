using System;
using UnityEngine;

namespace LittleWeeps.Client
{
    // Story decisions and route targets belong to the shared authority. This
    // clock only presents that route smoothly between its sparse updates.
    public sealed class NpcPresentationClock
    {
        private bool initialized;
        private string continuity;
        private double sample, received, displayed;
        public double Step(double authority, string key, bool connected, double now, float dt)
        {
            if (!initialized || continuity != key)
            {
                initialized = true; continuity = key;
                sample = displayed = authority; received = now;
            }
            if (authority > sample) { sample = authority; received = now; }
            if (!connected) return displayed = authority;
            // Resume from the current story after a long pause/reconnect rather
            // than spending minutes replaying missed route time. Locomotion
            // still eases toward this target; its root cannot teleport.
            if (sample-displayed>.5) displayed=sample-.25;
            // Ordinary packets never change displayed time directly. Correct
            // pace, and stop predicting after 250 ms without a fresh sample.
            var desired = sample + Math.Min(.2, Math.Max(0, now - received));
            var pace = Math.Max(.9, Math.Min(1.1, 1 + (desired - displayed) * 2));
            displayed = Math.Max(displayed, Math.Min(sample + .25,
                displayed + Math.Min(Math.Max(0, dt), .1f) * pace));
            return displayed;
        }
        public void Reset() { initialized = false; }
    }

    // One owner of position; animation observes the resulting world movement.
    // Camera transforms and server snapshots never become animation velocity.
    public sealed class NpcLocomotion
    {
        private readonly CharacterMotion motion = new CharacterMotion();
        private string continuity;
        private bool initialized, walking;
        private Vector2 velocity;
        public Vector2 Point { get; private set; }
        public CharacterFrame Step(Vector2 target, string key, bool carrying, float maxSpeed, float dt)
        {
            if (!initialized || key != continuity)
            {
                initialized = true; continuity = key; Point = target;
                velocity = Vector2.zero; walking = false;
            }
            else if (dt > 0)
            {
                Point = Vector2.SmoothDamp(Point, target, ref velocity, .12f, maxSpeed, Mathf.Min(dt, .1f));
                if ((Point-target).sqrMagnitude < .0625f && velocity.sqrMagnitude < 4)
                { Point = target; velocity = Vector2.zero; }
            }
            var frame = motion.Observe(Point, key, false, false, dt);
            // Different start/stop thresholds prevent correction-sized travel
            // from repeatedly swapping the idle and walk drawings near arrival.
            walking = !frame.ResetMotion && frame.Speed > (walking ? .6f : 2.5f);
            return new CharacterFrame(carrying ? CharacterPose.Carry : walking ? CharacterPose.Walk : CharacterPose.Idle,
                walking ? frame.Speed : 0, frame.FaceLeft, travel: walking ? frame.Travel : Vector2.zero,
                resetMotion: frame.ResetMotion);
        }
        public void Reset() { initialized = false; }
    }
}
