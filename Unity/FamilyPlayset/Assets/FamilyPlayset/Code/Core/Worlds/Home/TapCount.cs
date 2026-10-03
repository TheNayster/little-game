using System;

namespace LittleWeeps.Core
{
    // The first tiny rule can run without Unity, on a client or a future server.
    public sealed class TapCount
    {
        public int Value { get; private set; }
        public TapCount(int initialValue) => Value = Math.Max(0, initialValue);
        public void Add() { if (Value < int.MaxValue) Value++; }
    }
}
