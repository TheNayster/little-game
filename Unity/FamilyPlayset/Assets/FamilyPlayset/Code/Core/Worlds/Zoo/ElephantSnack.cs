using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public sealed partial class GameWorld
    {
        private string LeaseZooFood(ZooFood food,string species)
        {
            var z=state.zoo;
            var slot=Enumerable.Range(0,4).FirstOrDefault(i=>!z.food.Any(f=>f.species==species && f.slot==i));
            if(z.food.Any(f=>f.species==species && f.slot==slot))return "all-feed-spots-busy";
            if(z.nextTicket>=long.MaxValue-1)return "portion-limit";
            food.species=species;food.slot=slot;food.ticket=++z.nextTicket;food.preparing=false;return null;
        }
        private string ElephantSnackOperation(SoloCommand c,SoloPlayer p,ZooFood f)
        {
            if(!AtExhibit(p,c.target))return "come-to-exhibit";
            if(c.value=="snack-begin"){
                if(FossilHeld(p.id) || f.species!="" || state.toys.Any(t=>t.holder==p.id))return "hands-full";
                if(f.preparing)return f.prepSpecies==c.target?null:"snack-stale";
                if(Math.Abs(p.x-ZooCatalog.Get(c.target).SnackX)>200 || p.y>160)return "walk-to-snack-station";
                if(state.zoo.nextPrep>=long.MaxValue-1)return "portion-limit";
                LeaveElephantCare(p.id);
                f.preparing=true;f.prepSpecies=c.target;f.prepEpoch=++state.zoo.nextPrep;return null;
            }
            // The epoch protects a new bowl from delayed messages from an old
            // panel. Edit numbers make retries idempotent even with new receipts.
            var bits=(c.item??"").Split('/');
            if(!f.preparing || f.prepSpecies!=c.target || f.species!="" || bits.Length!=3 || !long.TryParse(bits[0],out var epoch) || epoch!=f.prepEpoch ||
                !long.TryParse(bits[1],out var edit) || edit!=f.edit || f.edit>=long.MaxValue-1 || !int.TryParse(bits[2],out var piece))return "snack-stale";
            if(c.value=="snack-add"){
                if(!ZooCatalog.Get(c.target).AcceptsSnack(piece))return "snack-ingredient";
                if(f.pieces.Length>=3)return "snack-full";
                f.pieces=f.pieces.Concat(new[]{piece}).ToArray();
            }else if(c.value=="snack-remove"){
                if(piece<0 || piece>=f.pieces.Length)return "snack-stale";
                f.pieces=f.pieces.Where((n,i)=>i!=piece).ToArray();
            }else if(c.value=="snack-clear")f.pieces=Array.Empty<int>();
            else if(c.value=="snack-cancel"){f.Clear();return null;}
            else if(c.value=="snack-serve"){
                if(f.pieces.Length==0)return "snack-empty";
                if(FossilHeld(p.id) || state.toys.Any(t=>t.holder==p.id))return "hands-full";
                return LeaseZooFood(f,c.target);
            }else return "unknown-zoo-action";
            f.edit++;return null;
        }
    }
}
