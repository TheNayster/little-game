using UnityEngine;

namespace LittleWeeps.Client
{
    // Distance-driven two-step cycle. The first half of each foot's phase is
    // stance: offset retreats at root speed, holding the paw in floor space.
    public static class CharacterWalk
    {
        // Full two-step distance in floor units, deliberately NOT divided by
        // Bingo's art scale. Build 116 made her run six steps/second at walk speed.
        public const float Stride=120;
        public static float HipHeight(float phase)
        {
            // Contact, down, passing, up, contact. Explicit authored values
            // retain a quiet torso instead of a continuous side-to-side wobble.
            var p=Mathf.Repeat(phase*2,1);
            if(p<.18f)return Mathf.Lerp(-.035f,-.08f,Mathf.SmoothStep(0,1,p/.18f));
            if(p<.5f)return Mathf.Lerp(-.08f,-.015f,Mathf.SmoothStep(0,1,(p-.18f)/.32f));
            if(p<.78f)return Mathf.Lerp(-.015f,.012f,Mathf.SmoothStep(0,1,(p-.5f)/.28f));
            return Mathf.Lerp(.012f,-.035f,Mathf.SmoothStep(0,1,(p-.78f)/.22f));
        }
        public readonly struct Foot
        {
            public readonly float Along,Lift,Bend;
            public readonly bool Planted;
            public Foot(float along,float lift,float bend,bool planted)
            {Along=along;Lift=lift;Bend=bend;Planted=planted;}
        }
        public static Foot Sample(float phase)
        {
            phase=Mathf.Repeat(phase,1);
            if(phase<.5f)return new Foot(.25f-phase,0,.02f+Mathf.Sin(phase*Mathf.PI*2)*.02f,true);
            var t=(phase-.5f)*2;
            // Match the stance's rearward velocity at both contacts; the free
            // foot then accelerates forward instead of snapping at lift-off.
            var along=-.25f-.5f*t+3*t*t-2*t*t*t;
            var lift=Mathf.Pow(Mathf.Sin(Mathf.PI*t),2);
            return new Foot(along,.20f*lift,.055f*lift,false);
        }
    }
}
