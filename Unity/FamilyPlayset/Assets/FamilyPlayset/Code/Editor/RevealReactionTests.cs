using System;
using LittleWeeps.Client;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public static class RevealReactionTests
    {
        private static void Check(bool pass,string reason){if(!pass)throw new InvalidOperationException(reason);}
        public static void Run()
        {
            var r=new RevealReactions();
            var hidden=new[]{new RevealReactions.Occupant("p2",1,false),new RevealReactions.Occupant("p1",1,false),new RevealReactions.Occupant("npc",1,false)};
            var found=new[]{new RevealReactions.Occupant("p1",-1,true),new RevealReactions.Occupant("p2",-1,true),new RevealReactions.Occupant("npc",-1,true)};
            Check(!r.Observe("world/1",1,hidden,0,true),"First snapshot must be silent");
            Check(r.Observe("world/1",1,found,.1f,true) && r.Events==3,"One event per simultaneous discovery");
            Check(!r.Observe("world/1",1,found,.2f,true) && r.Events==3,"Snapshot must not replay");
            var reactions=r.Active(.2f);Check(reactions.Length==3 && reactions[0].index==0 && reactions[2].index==2 && reactions[1].count==3,"Authority identities determine positions");
            var peer=new RevealReactions();peer.Observe("world/1",1,hidden,0,true);peer.Observe("world/1",1,found,.1f,true);
            Check(peer.Active(.2f)[1].variant==reactions[1].variant,"Observers must agree on variant");
            Check(r.Active(2).Length==0,"Reaction is bounded");
            r.Observe("world/1",1,found,2,false);Check(!r.Observe("world/1",1,found,3,true) && r.Active(3).Length==0,"Reconnect must baseline existing discoveries");
            Check(!r.Observe("world/2",2,found,4,true),"New-round baseline must be silent");
            r.Observe("world/3",3,hidden,5,true);r.Observe("world/3",3,new[]{new RevealReactions.Occupant("p1",-1,false)},6,true);Check(r.Active(6).Length==0,"Departure is not a reveal");
            Debug.Log("RevealReactionTests PASS: simultaneous identities, deterministic variants, snapshot/reconnect suppression, bounded duration and departure");
        }
    }
}
