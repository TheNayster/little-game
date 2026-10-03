using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static void BookTests()
    {
        Test("installed 147 four-book save upgrades without moving or replacing its copies",()=>{
            var s=GameWorld.WithBooks(SecretWorld()).Snapshot();s.schema=10;
            s.toys=s.toys.Where(t=>t.kind!=ToyKind.Book || HomeBooks.Available(t.id,10)).ToArray();
            var prior=s.toys.Where(t=>t.kind==ToyKind.Book).ToArray();
            for(var i=0;i<prior.Length;i++){prior[i].container=HomeBooks.Support(i);prior[i].x=HomeBooks.X(i);prior[i].y=HomeBooks.Y(i);}
            prior[2].container="";prior[2].x=-4000;prior[2].y=200;
            var before=Encode(s.toys);var w=GameWorld.WithBooks(GameWorld.Restore(s));var after=w.Snapshot();
            Check(after.schema==11 && after.toys.Length==s.toys.Length+2);
            Check(Encode(after.toys.Where(t=>s.toys.Any(old=>old.id==t.id)).ToArray())==before);
            Check(after.toys.Count(t=>t.kind==ToyKind.Book)==6);Check(ReferenceEquals(w,GameWorld.WithBooks(w)));
        });
        Test("book migration adds exactly the preview copies preserving existing bedrooms secrets and inventory",()=>{
            var old=SecretWorld();MakeSecret(old);EnterSecret(old);var before=old.Snapshot();var w=GameWorld.WithBooks(old);var after=w.Snapshot();
            Check(after.schema==HomeBooks.Schema && after.toys.Length==before.toys.Length+HomeBooks.Titles.Length);
            Check(after.toys.Count(t=>t.kind==ToyKind.Book)==HomeBooks.Titles.Length && HomeBooks.Titles.Distinct().Count()==HomeBooks.Titles.Length);
            after.schema=before.schema;after.revision=before.revision;after.toys=after.toys.Where(t=>t.kind!=ToyKind.Book).ToArray();Check(Encode(after)==Encode(before));
            Check(ReferenceEquals(w,GameWorld.WithBooks(w)));GameWorld.Validate(w.Snapshot());
        });
        Test("four readers have independent local cursors without changing shared objects",()=>{
            var w=GameWorld.WithBooks(SecretWorld());var before=Encode(w.Snapshot());var cursors=Enumerable.Range(0,4).Select(_=>new BookCursor()).ToArray();
            for(var i=0;i<4;i++){cursors[i].Begin(i,100*i,2);cursors[i].Play();}
            cursors[0].Turn(12);cursors[1].Close();Check(cursors[2].Playing && cursors[2].Page==2 && cursors[3].Playing && cursors[3].Sample==300);
            Check(Encode(w.Snapshot())==before);
        });
        Test("reader resume rejects stale name load after pause turn close and revision change",()=>{
            var c=new BookCursor();c.Begin(4,1000,2);c.Play();var old=c.Generation;Check(c.CanResume(old));c.Pause();Check(!c.CanResume(old) && c.Sample==1000);
            c.Play();old=c.Generation;c.Turn(6);Check(!c.Current(old) && c.Sample==0 && !c.Playing);c.Play();old=c.Generation;c.Close();Check(!c.Current(old));
            c.Begin(40,9000,1,8,2);Check(c.Page==0 && c.Sample==0);c.Begin(40,9000,2,8,2);c.ClampSamples(50);Check(c.Page==7 && c.Sample==49 && !c.Playing);
        });
        Test("book inventory rejects duplicated or renamed copies and survives idle without refill",()=>{
            var w=GameWorld.WithBooks(SecretWorld());var s=w.Snapshot();s.toys.Last().id=HomeBooks.Copy(0);Throws(()=>GameWorld.Restore(s));
            s=w.Snapshot();s.toys.Last().water=1;Throws(()=>GameWorld.Restore(s));s=w.Snapshot();s.toys=s.toys.Where(t=>t.id!=HomeBooks.Copy(0)).ToArray();Throws(()=>GameWorld.Restore(s));
            var before=Encode(w.ReadToys().Where(t=>t.kind==ToyKind.Book).ToArray());Advance(w,900);Check(Encode(w.ReadToys().Where(t=>t.kind==ToyKind.Book).ToArray())==before);
        });
        Test("book has one holder and real occupied rack supports reject a second copy",()=>{
            var w=GameWorld.WithBooks(SecretWorld());Good(w,SoloAction.Move,x:HomeBooks.X(0),y:HomeBooks.Y(0));Good(w,SoloAction.Grab,HomeBooks.Copy(0));
            Check(!w.Apply(Command(w,SoloAction.Grab,HomeBooks.Copy(0),actor:"second")).Accepted);
            Check(!w.Apply(Command(w,SoloAction.Drop,HomeBooks.Copy(0),target:HomeBooks.Support(1),x:HomeBooks.X(1),y:HomeBooks.Y(1))).Accepted);
            Good(w,SoloAction.Drop,HomeBooks.Copy(0),target:HomeBooks.Support(0),x:HomeBooks.X(0),y:HomeBooks.Y(0));w=GameWorld.Restore(w.Snapshot());Check(Toy(w,HomeBooks.Copy(0)).container==HomeBooks.Support(0));
        });
    }
}
