using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public sealed partial class GameWorld
    {
        private string SpideyBuildOperation(SoloCommand c,SoloPlayer player,SpideyMember member)
        {
            var h=state.spidey;if(Math.Abs(player.x-SpideyHQ.X("build"))>350)return "come-to-building";
            var brick=h.bricks.FirstOrDefault(b=>b.id==c.item);
            if(c.value=="brick-new"){
                if(!int.TryParse(c.item,out var variant) || variant<0 || variant>=26 || h.bricks.Length>=96 || h.brickSerial==int.MaxValue)return "building-full";
                if(h.bricks.Any(b=>b.owner==player.id))return "finish-your-brick-first";
                var b=new SpideyBrick{id="hq-brick-"+(h.brickSerial++),variant=variant,owner=player.id,x=SpideyHQ.X("build"),y=150};h.bricks=h.bricks.Concat(new[]{b}).ToArray();return null;
            }
            if(c.value=="brick-claim"){
                if(brick==null || brick.owner!="" || h.bricks.Any(b=>b.owner==player.id))return "brick-in-use";
                brick.owner=player.id;brick.version++;return null;
            }
            if(brick==null || brick.owner!=player.id)return "brick-in-use";
            if(!int.TryParse(c.target,out var version) || version!=brick.version)return "brick-changed";
            if(c.value=="brick-place"){
                if(!SpideyHQ.Point(c.x,c.y) || Math.Abs(c.x-SpideyHQ.X("build"))>500)return "place-on-the-building-mat";
                float x=(float)Math.Round(c.x/40)*40,y=(float)Math.Round(c.y/40)*40;
                if(h.bricks.Any(b=>b!=brick && b.owner=="" && Math.Abs(b.x-x)<30 && Math.Abs(b.y-y)<30))return "space-in-use";
                brick.x=x;brick.y=y;brick.owner="";brick.version++;return null;
            }
            if(c.value=="brick-turn"){brick.turn=(brick.turn+1)%4;brick.version++;return null;}
            if(c.value=="brick-release"){brick.owner="";brick.version++;return null;}
            if(c.value=="brick-remove"){h.bricks=h.bricks.Where(b=>b!=brick).ToArray();return null;}
            return "unknown-building-action";
        }
        private string SpideyPuzzleOperation(SoloCommand c,SoloPlayer player,SpideyMember member)
        {
            var h=state.spidey;if(Math.Abs(player.x-SpideyHQ.X("puzzle"))>350)return "come-to-the-puzzle";
            if(c.value=="puzzle-join"){LeaveSpideyStation(player.id);SpideyPose(member,"idle","puzzle",99999);return null;}
            if(member.station!="puzzle")return "join-the-shared-puzzle";
            if(c.value=="puzzle-select"){
                if(h.puzzleHolders.Any(a=>a!="") || !int.TryParse(c.item,out var image) || !int.TryParse(c.target,out var size) || image<0 || image>4 || size<2 || size>5)return "finish-the-piece-first";
                // Only the first participant starts a new unfinished board. A
                // completed board may be replayed by any remaining participant.
                if(h.members.Any(m=>m.station=="puzzle" && m.actor!=player.id) && h.puzzlePlaced.Any(v=>v==1) && !h.puzzlePlaced.All(v=>v==1))return "finish-the-shared-puzzle-first";
                h.puzzleImage=image;h.puzzleSize=size;h.puzzlePlaced=new int[size*size];h.puzzleHolders=Enumerable.Repeat("",size*size).ToArray();h.puzzleDoneAt=-1;h.puzzleRevision++;return null;
            }
            if(!int.TryParse(c.item,out var piece) || piece<0 || piece>=h.puzzlePlaced.Length)return "choose-a-piece";
            if(c.value=="puzzle-claim"){
                if(h.puzzlePlaced[piece]!=0 || h.puzzleHolders[piece]!="" || h.puzzleHolders.Contains(player.id))return "piece-in-use";
                h.puzzleHolders[piece]=player.id;h.puzzleRevision++;return null;
            }
            if(h.puzzleHolders[piece]!=player.id)return "piece-in-use";
            if(c.value=="puzzle-release"){h.puzzleHolders[piece]="";h.puzzleRevision++;return null;}
            if(c.value=="puzzle-place"){
                if(!int.TryParse(c.target,out var revision) || revision!=h.puzzleRevision)return "puzzle-changed";
                int column=piece%h.puzzleSize,row=piece/h.puzzleSize;
                if(!KeepyRules.Finite(c.x) || !KeepyRules.Finite(c.y) || Math.Abs(c.x-column)>.35 || Math.Abs(c.y-row)>.35)return "try-the-matching-place";
                h.puzzleHolders[piece]="";h.puzzlePlaced[piece]=1;h.puzzleRevision++;
                if(h.puzzlePlaced.All(v=>v==1)){h.puzzleDoneAt=h.clock;h.puzzles++;foreach(var m in h.members.Where(m=>m.station=="puzzle")){m.pose="happy";m.at=h.clock;}}
                return null;
            }
            return "unknown-puzzle-action";
        }
    }
}
