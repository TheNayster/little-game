using System;
using System.IO;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static string CreationJson(object value)=>System.Text.Json.JsonSerializer.Serialize(value,Json);
    static GameWorld CreationWorld()=>GameWorld.WithHomeCreations(KitchenWorld());
    static SoloResult Creation(GameWorld w,string op,string item="",string target="",string actor="first")=>w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=actor,action=SoloAction.Kitchen,item=item,target=target,value=op,expectedRevision=w.Revision,zone=w.ReadPlayer(actor).zone,visit=w.ReadPlayer(actor).visit});
    static void PackFood(GameWorld w,string item,string actor="first")=>Check(Creation(w,"store-food",item,HomeCreations.FoodToken(Toy(w,item).kitchen.dish),actor).Accepted);
    static void HomeCreationTests()
    {
        Test("creation storage migrates additively and preserves every prior record",()=>{
            var old=GameWorld.WithHomeTidying(KitchenWorld());var before=old.Snapshot();var w=GameWorld.WithHomeCreations(old);var after=w.Snapshot();Check(after.schema==23 && w.ReadCreations().foods.Length==0);after.schema=22;after.revision--;Check(CreationJson(after)==CreationJson(before));Check(ReferenceEquals(w,GameWorld.WithHomeCreations(w)));
        });
        Test("four cooks store independent creations and free the same reusable trays",()=>{
            var w=CreationWorld();var actors=w.Snapshot().players;
            for(var i=0;i<4;i++){
                Cook(w,"easy:start","cookware-"+i,"PIZ-02",actors[i].id);Cook(w,"easy:add","ingredient-sauce","cookware-"+i,actors[i].id);
                var dish=CreationJson(Toy(w,"cookware-"+i).kitchen.dish);PackFood(w,"cookware-"+i,actors[i].id);
                Check(Toy(w,"cookware-"+i).kitchen.dish==null && !Toy(w,"cookware-"+i).kitchen.dirty && Toy(w,"cookware-"+i).kitchen.cook=="");
                Check(CreationJson(w.ReadCreations().foods.Single(f=>f.owner==actors[i].id).dish)==dish);
            }
            Check(w.ReadCreations().foods.Length==4);var stock=CreationJson(w.ReadToys().Where(t=>t.kind==ToyKind.Ingredient).ToArray());
            foreach(var f in w.ReadCreations().foods)Check(Creation(w,"restore-food",target:f.key.ToString(),actor:f.owner).Accepted);
            Check(w.ReadCreations().foods.Length==0 && stock==CreationJson(w.ReadToys().Where(t=>t.kind==ToyKind.Ingredient).ToArray()));GameWorld.Validate(w.Snapshot());
        });
        Test("storage refuses other cooks held food and heating; full slots lose nothing",()=>{
            var w=CreationWorld();Cook(w,"easy:start","cookware-0","PIZ-01");
            var token=HomeCreations.FoodToken(Toy(w,"cookware-0").kitchen.dish);
            Check(Creation(w,"store-food","cookware-0",token,"second").Outcome=="owner-only");
            Good(w,SoloAction.Grab,"cookware-0");Check(!Creation(w,"store-food","cookware-0",token).Accepted);Good(w,SoloAction.CancelGrab,"cookware-0");
            PackFood(w,"cookware-0");Cook(w,"easy:start","cookware-0","PIZ-01");PackFood(w,"cookware-0");Cook(w,"easy:start","cookware-0","PIZ-01");
            var before=CreationJson(Toy(w,"cookware-0"));Check(Creation(w,"store-food","cookware-0",HomeCreations.FoodToken(Toy(w,"cookware-0").kitchen.dish)).Outcome=="food-storage-full");Check(CreationJson(Toy(w,"cookware-0"))==before);
            var f=w.ReadCreations().foods[0];Check(!Creation(w,"restore-food",target:f.key.ToString(),actor:"second").Accepted);Check(!Creation(w,"restore-food","cookware-0",f.key.ToString()).Accepted);
            w=CreationWorld();Cook(w,"easy:readybase","cookware-0","CAK-02");var s=w.Snapshot();var d=s.toys.First(t=>t.id=="cookware-0").kitchen.dish;d.stage="bake";d.heated=false;d.heat=1;d.layers=0;w=GameWorld.Restore(s);Check(Creation(w,"store-food","cookware-0",HomeCreations.FoodToken(d)).Outcome=="food-still-cooking");
        });
        Test("stored portions cannot duplicate served food and retain exact ingredients through recovery",()=>{
            var w=CreationWorld();Cook(w,"easy:readybase","cookware-0","PIZ-01");Cook(w,"easy:cut","cookware-0");Cook(w,"easy:serve","cookware-0","plate-0");
            var remaining=Toy(w,"cookware-0").kitchen.dish.Copy();PackFood(w,"cookware-0");PackFood(w,"plate-0");
            Check(w.ReadCreations().foods.Select(f=>f.dish.portions).Sum()==15);var restored=GameWorld.Restore(w.Snapshot());Check(CreationJson(restored.ReadCreations())==CreationJson(w.ReadCreations()));
            var snapshot=w.Snapshot();var data=w.ReadCreations();data.foods[0].dish.portions=15;snapshot.homeCreations=HomeCreations.Encode(data);Throws(()=>GameWorld.Validate(snapshot));
            Check(Creation(w,"restore-food",target:w.ReadCreations().foods[0].key.ToString()).Accepted);Check(CreationJson(w.ReadToys().First(t=>t.kitchen?.dish?.id==remaining.id).kitchen.dish)==CreationJson(remaining));GameWorld.Validate(w.Snapshot());
        });
        Test("retrying a stored-food command never creates a second archived copy",()=>{
            var w=CreationWorld();Cook(w,"easy:start","cookware-0","PIZ-01");var p=w.ReadPlayer("first");var command=new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=p.id,action=SoloAction.Kitchen,item="cookware-0",target=HomeCreations.FoodToken(Toy(w,"cookware-0").kitchen.dish),value="store-food",expectedRevision=w.Revision,zone=p.zone,visit=p.visit};
            Check(w.Apply(command).Accepted);var again=w.Apply(command);Check(again.Accepted && again.Duplicate && w.ReadCreations().foods.Length==1);
        });
        Test("picture folders snapshot colors without overwriting work; four owners display independently",()=>{
            var w=CreationWorld();foreach(var p in w.Snapshot().players){Good(w,SoloAction.Move,x:Discovery.ArtX,y:200,actor:p.id);Check(Discover(w,"fill:0:5",p.id).Accepted);Check(Discover(w,"save-picture",p.id).Accepted);Check(Discover(w,"save-picture",p.id).Accepted);}
            Check(w.ReadCreations().pictures.Length==4);var original=w.ReadCreations().pictures.First();Check(Discover(w,"fill:0:1").Accepted);Check(w.ReadCreations().pictures.First().drawing.colors[0]==5);
            SoloResult Picture(string op,int key,string actor)=>w.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=actor,item=actor,action=SoloAction.Discovery,value=op,target=key.ToString(),expectedRevision=w.Revision,zone=w.ReadPlayer(actor).zone,visit=w.ReadPlayer(actor).visit});
            Check(!Picture("display-picture",original.key,"second").Accepted);foreach(var q in w.ReadCreations().pictures)Check(Picture("display-picture",q.key,q.owner).Accepted);Check(w.ReadCreations().pictures.Count(q=>q.displayed)==4);
            Check(Picture("remove-picture",original.key,"first").Accepted);Check(w.ReadCreations().removed.Single().key==original.key);Check(Picture("undo-picture-remove",original.key,"first").Accepted);Check(w.ReadCreations().pictures.Single(q=>q.key==original.key).displayed);
            var encoded=w.Snapshot().homeCreations;Advance(w,(int)(HomeTidying.IdleSeconds+HomeTidying.CueSeconds+5));Check(w.Snapshot().homeCreations==encoded);Check(CreationJson(GameWorld.Restore(w.Snapshot()).ReadCreations())==CreationJson(w.ReadCreations()));
        });
        Test("bounded archive format rejects corrupt oversized and trailing content without replacing saves",()=>{
            var w=CreationWorld();foreach(var invalid in new[]{"bad!",new string('A',HomeCreations.MaxEncoded+1),Convert.ToBase64String(new byte[]{1,2,3})}){var s=w.Snapshot();s.homeCreations=invalid;Throws(()=>GameWorld.Validate(s));}
            Check(w.Snapshot().homeCreations=="");
            var data=new CreationCollection{serial=1,pictures=new[]{new StoredPicture{key=1,page=0,owner="first",drawing=new ColoringPage{colors=new int[]{9,0,0,0}}}}};var snapshot=w.Snapshot();snapshot.homeCreations=HomeCreations.Encode(data);Throws(()=>GameWorld.Validate(snapshot));
            var blob=HomeCreations.Encode(new CreationCollection());Check(HomeCreations.Decode(blob).foods.Length==0);Check(blob.Length<=HomeCreations.MaxEncoded);
        });
    }
}
