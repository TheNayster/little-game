using System;
using System.Linq;
namespace LittleWeeps.Core
{
    public sealed partial class GameWorld
    {
        public FunfairState ReadFunfair()=>state.funfair?.Copy();
        static void NormalizeFunfair(SoloSnapshot s){if(s!=null&&s.schema<PeppaFunfair.Schema&&s.funfair!=null&&(s.funfair.members==null||s.funfair.members.Length==0)&&s.funfair.clock==0)s.funfair=null;var h=s?.funfair;if(h!=null&&string.IsNullOrEmpty(h.photoRole))h.photoRole=h.members?.FirstOrDefault(m=>m.actor==h.photoActor)?.role??"";}
        static void ValidateFunfair(SoloSnapshot s){if(s.funfair==null&&s.schema<PeppaFunfair.Schema)return;PeppaFunfair.Validate(s.funfair,s.players.Select(p=>p.id).ToArray());}
        static void RestoreFunfair(SoloSnapshot s)=>PeppaFunfair.Restore(s.funfair);
        public void ReleaseFunfair(string actor)=>PeppaFunfair.Release(state.funfair,actor,state.players);
        bool LeaveFunfairStation(string actor){var m=state.funfair?.members.FirstOrDefault(a=>a.actor==actor);if(m==null)return false;bool changed=m.station!="";PeppaFunfair.Leave(state.funfair,actor);return changed;}
        bool AdvanceFunfair(double seconds,string[] connected,out bool visible){bool changed=PeppaFunfair.Advance(state.funfair,state.players,seconds,connected,out visible);AmbientChanged|=changed;return changed;}
        string FunfairOperation(SoloCommand c,string[] connected)=>PeppaFunfair.Apply(state.funfair,state.players,c,connected);
    }
}
