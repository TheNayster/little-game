using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public static partial class BirthdayPlay
    {
        public const float BubbleX=1850,BubbleY=150;
        public static int LastBubble(BirthdayState b)=>(int)Math.Max(-1,Math.Floor((b.clock-b.bubblesAt)/.35));
        public static bool BubbleVisible(BirthdayState b,int id)=>b.bubblesOn && id>=0 && id<=LastBubble(b) && id>LastBubble(b)-12 && !b.poppedBubbles.Contains(id);
        public static float BubbleXAt(int id,float age)=>BubbleX+(id%5-2)*24+(float)Math.Sin(age*1.7+id)*35;
        public static float BubbleYAt(int id,float age)=>BubbleY+age*115+id%3*18;
        public static int BrickLevel(BirthdayState b,BirthdayProp prop){int level=0;while(prop.support!="" && level<8){prop=b.props.Single(p=>p.id==prop.support);level++;}return level;}
    }
    public sealed partial class GameWorld
    {
        private static void ValidateBirthdayPlay(BirthdayState b)
        {
            if(!KeepyRules.Finite(b.bubblesAt) || b.bubblesAt< -1 || b.bubblesAt>b.clock || b.bubblesPopped<0 || b.poppedBubbles==null || b.poppedBubbles.Length>12 || b.poppedBubbles.Distinct().Count()!=b.poppedBubbles.Length || b.poppedBubbles.Any(i=>i<0 || i>BirthdayPlay.LastBubble(b)))throw new InvalidOperationException("Invalid shared bubbles.");
            foreach(var prop in b.props){
                if(prop.support==null || prop.turns<0 || prop.turns>3 || prop.tint<0 || prop.tint>3 || prop.support!="" && (prop.kind!="brick" || prop.holder!="" || !b.props.Any(p=>p.id==prop.support && p.kind=="brick" && p.holder=="")))throw new InvalidOperationException("Invalid birthday brick support.");
                var visited=new System.Collections.Generic.HashSet<string>();var cursor=prop;
                while(cursor.support!=""){if(!visited.Add(cursor.id))throw new InvalidOperationException("Cyclic birthday bricks.");cursor=b.props.Single(p=>p.id==cursor.support);}
            }
        }
        private bool BirthdayPlayOperation(SoloCommand c,SoloPlayer player,BirthdayMember member,BirthdayProp held,out string outcome)
        {
            var b=state.birthday;outcome=null;
            if(c.value=="bubbles"){
                if(Math.Abs(player.x-BirthdayPlay.BubbleX)>InteractionRadius || Math.Abs(player.y-BirthdayPlay.BubbleY)>InteractionRadius){outcome="come-to-the-bubble-machine";return true;}
                b.bubblesOn=!b.bubblesOn;b.bubblesAt=b.clock;b.poppedBubbles=Array.Empty<int>();return true;
            }
            if(c.value=="bubble-pop"){
                if(!int.TryParse(c.item,out int id) || !BirthdayPlay.BubbleVisible(b,id)){outcome="bubble-already-popped";return true;}
                b.poppedBubbles=b.poppedBubbles.Append(id).ToArray();b.bubblesPopped++;return true;
            }
            if(c.value=="brick-turn" || c.value=="brick-color"){
                if(held?.kind!="brick"){outcome="hold-a-building-brick";return true;}
                if(c.value=="brick-turn")held.turns=(held.turns+1)%4;else held.tint=(held.tint+1)%4;
                held.version++;return true;
            }
            if(c.value!="stack")return false;
            var target=b.props.SingleOrDefault(p=>p.id==c.item);
            if(held?.kind!="brick" || target?.kind!="brick" || target==held || target.holder!="" || b.props.Any(p=>p.support==target.id)){outcome="choose-a-free-brick-top";return true;}
            if(Math.Abs(player.x-target.x)>InteractionRadius || Math.Abs(player.y-target.y)>InteractionRadius){outcome="come-closer";return true;}
            held.holder="";held.location="ground";held.support=target.id;held.x=target.x;held.y=target.y;held.version++;BirthdayPose(member,"Idle");return true;
        }
        private void AdvanceBirthdayPlay()
        {
            var b=state.birthday;int last=BirthdayPlay.LastBubble(b);
            b.poppedBubbles=b.poppedBubbles.Where(i=>i>last-12).ToArray();
            if(b.bubblesOn)AmbientChanged=true;
        }
    }
}
