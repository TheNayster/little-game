using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

static partial class Program
{
    static SoloWorld LiquidWorld()=>SoloWorld.WithLiquidColors(DiscoveryWorld());
    static LiquidColorTray LiquidTray(SoloWorld w,string actor="first")=>Workspace(w,actor).liquid[0];
    static SoloResult Liquid(SoloWorld w,string op,string actor="first")=>w.Apply(Command(w,SoloAction.Discovery,actor,LiquidColorLab.Token(LiquidTray(w,actor)),"liquid:"+op,actor:actor));
    static void LiquidColorTests()
    {
        Test("liquid migration appends four empty trays without changing older saved content",()=>{
            var old=BubbleWorld();PrepareBubbles(old);Check(Bubbles(old,"blow").Accepted && Discover(old,"fill:0:5").Accepted && Ice(old,"chip").Accepted);var before=old.Snapshot();var w=SoloWorld.WithLiquidColors(old);var s=w.Snapshot();Check(s.schema==21 && s.discovery.All(v=>v.liquid.Length==1 && LiquidColorLab.Volume(v.liquid[0].current)==0));s.schema=20;s.revision--;foreach(var d in s.discovery)d.liquid=Array.Empty<LiquidColorTray>();Check(Encode(before)==Encode(s));Check(ReferenceEquals(w,SoloWorld.WithLiquidColors(w)));
        });
        Test("liquid mixtures match researched orange green purple and earthy brown anchors",()=>{
            foreach(var item in new[]{(new[]{1,1,0},new[]{242,137,52},"Orange"),(new[]{0,1,1},new[]{60,172,113},"Green"),(new[]{1,0,1},new[]{153,90,193},"Purple"),(new[]{1,1,1},new[]{125,101,80},"Earthy brown")}){var s=new LiquidColorState{parts=item.Item1};Check(LiquidColorLab.Color(s).SequenceEqual(item.Item2) && LiquidColorLab.Name(s)==item.Item3);}
            Check(LiquidColorLab.Name(new LiquidColorState{parts=new[]{8,1,1}})=="Mixed colors");Check(LiquidColorLab.Name(new LiquidColorState())=="Choose two colors" && LiquidColorLab.Name(new LiquidColorState{water=1})=="Clear water");
        });
        Test("color ratios change shades and water dilutes without losing source quantities",()=>{
            var s=new LiquidColorState{parts=new[]{1,1,0}};var orange=LiquidColorLab.Color(s);s.parts[0]=3;var redder=LiquidColorLab.Color(s);Check(!redder.SequenceEqual(orange));s.water=4;var lighter=LiquidColorLab.Color(s);var white=new[]{238,247,247};double distance(int[] c)=>c.Select((v,i)=>(double)(v-white[i])*(v-white[i])).Sum();Check(distance(lighter)<distance(redder) && s.parts.SequenceEqual(new[]{3,1,0}) && LiquidColorLab.Volume(s)==8);
            var w=LiquidWorld();foreach(var op in new[]{"blue","red","yellow"})Check(Liquid(w,op).Accepted);var first=LiquidColorLab.Color(LiquidTray(w).current);Check(Liquid(w,"again").Accepted);foreach(var op in new[]{"yellow","blue","red"})Check(Liquid(w,op).Accepted);Check(LiquidColorLab.Color(LiquidTray(w).current).SequenceEqual(first));
        });
        Test("twelve portions cap contents and reset undo restores full state exactly",()=>{
            var w=LiquidWorld();Check(!Liquid(w,"again").Accepted);for(var i=0;i<12;i++)Check(Liquid(w,new[]{"red","yellow","blue","water"}[i%4]).Accepted);var prior=JsonSerializer.Serialize(LiquidTray(w).current,Json);var before=Encode(w.Snapshot());foreach(var op in new[]{"red","yellow","blue","water"})Check(!Liquid(w,op).Accepted);Check(Encode(w.Snapshot())==before);Check(Liquid(w,"again").Accepted && Liquid(w,"undo").Accepted);Check(JsonSerializer.Serialize(LiquidTray(w).current,Json)==prior && !Liquid(w,"undo").Accepted);
        });
        Test("four liquid trays remain independent through changes travel disconnect and reopen",()=>{
            var w=LiquidWorld();var actors=w.Snapshot().players.Select(p=>p.id).ToArray();for(var i=0;i<4;i++)Check(Liquid(w,new[]{"red","yellow","blue","water"}[i],actors[i]).Accepted);var other=Encode(w.ReadDiscovery().Skip(1).ToArray());Check(Liquid(w,"blue").Accepted && Liquid(w,"again").Accepted && Liquid(w,"undo").Accepted);Good(w,SoloAction.Travel,value:"park");Check(!Liquid(w,"red").Accepted);var session=new FamilySession(w);Check(session.Attach(7,"third",out _) && session.Detach(7));Check(Encode(w.ReadDiscovery().Skip(1).ToArray())==other);Check(Encode(SoloWorld.Restore(Decode(Encode(w.Snapshot()))).Snapshot())==Encode(w.Snapshot()));
        });
        Test("liquid commands reject stale foreign malformed input and duplicate pours",()=>{
            var w=LiquidWorld();var command=Command(w,SoloAction.Discovery,"first","colors@0","liquid:red");Check(w.Apply(command).Accepted && w.Apply(command).Duplicate);var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Discovery,"first","colors@0","liquid:blue")).Accepted);Check(!w.Apply(Command(w,SoloAction.Discovery,"second","colors@0","liquid:yellow")).Accepted);foreach(var op in new[]{"red:9","green","pour:100",""})Check(!Liquid(w,op).Accepted);Check(Encode(w.Snapshot())==before);
        });
        Test("liquid copying and validation protect current and undo ingredient arrays",()=>{
            var w=LiquidWorld();Check(Liquid(w,"red").Accepted && Liquid(w,"blue").Accepted);var s=w.Snapshot();var copy=SoloWorld.CopySnapshot(s);copy.discovery[0].liquid[0].current.parts[0]=9;copy.discovery[0].liquid[0].previous[0].parts[0]=9;Check(LiquidTray(w).current.parts[0]==1 && LiquidTray(w).previous[0].parts[0]==1);
            void Reject(Action<LiquidColorTray> edit){var v=Decode(Encode(s));edit(v.discovery[0].liquid[0]);Throws(()=>SoloWorld.Validate(v));}
            Reject(t=>t.current.parts=null);Reject(t=>t.current.parts=new[]{1,1});Reject(t=>t.current.parts[0]=-1);Reject(t=>t.current.water=12);Reject(t=>t.current.parts[0]=int.MaxValue);Reject(t=>t.previous[0].water=-1);Reject(t=>t.previous=new[]{t.current.Copy(),t.current.Copy()});Reject(t=>t.revision=long.MaxValue);
        });
    }
}
