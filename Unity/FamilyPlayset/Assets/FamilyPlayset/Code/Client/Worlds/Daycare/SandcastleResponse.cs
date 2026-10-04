using System;
namespace LittleWeeps.Client
{
    // Optional, bounded presentation metadata. Never persisted or used by rules.
    [Serializable] public sealed class SandcastleResponse
    {public long revision;public int round;public string actor,operation;}
    public interface ISandcastleResponses
    {bool SandcastleFeedbackAvailable {get;} SandcastleResponse[] SandcastleResponses {get;}}
}
