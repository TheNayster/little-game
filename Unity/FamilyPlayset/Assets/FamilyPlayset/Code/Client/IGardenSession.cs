using System;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    // Presentation knows only an admitted profile, a confirmed view and commands.
    // Transport and the authority's disk checkpoint stay outside this assembly.
    public interface IGardenSession
    {
        string Actor { get; }
        string PreferenceScope { get; }
        bool Connected { get; }
        bool Busy { get; }
        bool MutedTest { get; }
        string Status { get; }
        SoloSnapshot View { get; }
        string[] Players { get; }
        long ViewSequence { get; }
        bool Submit(SoloCommand command,Action<SoloResult> complete);
        void Preview(string item,Vector2 point);
        bool TryPreview(string item,out Vector2 point);
        void Walk(WalkMode mode,float x=0,float y=0);
        Vector2 VisualPosition(string actor);
    }
}
