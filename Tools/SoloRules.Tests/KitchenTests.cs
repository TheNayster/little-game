using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using LittleWeeps.Core;

static partial class Program
{
    static SoloWorld KitchenWorld()=>SoloWorld.WithKitchen(PlayWorld());
    static void Cook(SoloWorld w,string operation,string item="",string target="",string actor="first")=>Good(w,SoloAction.Kitchen,item,target:target,value:operation,actor:actor);
    static void OpenKitchen(SoloWorld w){Cook(w,"door",target:"fridge");for(var i=0;i<4;i++)Cook(w,"door",target:i.ToString());Cook(w,"door",target:"oven");Cook(w,"door",target:"water");}
    static void KitchenDrop(SoloWorld w,string id,string group,int slot,string actor="first")
    {Good(w,SoloAction.Grab,id,actor:actor);Good(w,SoloAction.Drop,id,target:Kitchen.Support(group,slot),x:Kitchen.X(group,slot),y:Kitchen.Y(group,slot),actor:actor);}
    static void FinishDish(SoloWorld w,string id,string actor="first")
    {
        while(Kitchen.Next(Toy(w,id).kitchen.dish)!="serve"){
            var op=Kitchen.Next(Toy(w,id).kitchen.dish);
            if(op.StartsWith("add:")){Cook(w,"add","ingredient-"+op.Substring(4),id,actor);continue;}
            if(op=="heat"){
                var slot=Enumerable.Range(0,4).First(i=>!w.ReadToys().Any(t=>t.container==Kitchen.Support("oven",i)));
                KitchenDrop(w,id,"oven",slot,actor);for(var i=0;i<9;i++)w.AdvanceIdle(1,out _);
                Good(w,SoloAction.Grab,id,actor:actor);Good(w,SoloAction.Drop,id,x:-1550,y:200,actor:actor);
            }else Cook(w,op,id,actor:actor);
        }
    }
    static void KitchenTests()
    {
        Test("recipe ingredients are required before heating and physical tools return to their rack",()=>{
            var w=KitchenWorld();OpenKitchen(w);Cook(w,"start","cookware-0","PIZ-01");
            Good(w,SoloAction.Grab,"utensil-roller-0");Good(w,SoloAction.Drop,"utensil-roller-0",target:"cookware-0",x:-2040,y:430);
            Check(Toy(w,"utensil-roller-0").container==Kitchen.Support("tools",2));Cook(w,"spread","cookware-0");KitchenDrop(w,"cookware-0","oven",0);
            for(var i=0;i<10;i++)w.AdvanceIdle(1,out _);Check(!Toy(w,"cookware-0").kitchen.dish.heated && Kitchen.Next(Toy(w,"cookware-0").kitchen.dish)=="add:sauce");
            Cook(w,"add","ingredient-sauce","cookware-0");Cook(w,"add","ingredient-cheese","cookware-0");for(var i=0;i<9;i++)w.AdvanceIdle(1,out _);Check(Toy(w,"cookware-0").kitchen.dish.heated);SoloWorld.Validate(w.Snapshot());
        });
        Test("Unity inline empty food metadata normalizes without accepting malformed nonempty data",()=>{
            var s=KitchenWorld().Snapshot();foreach(var t in s.toys){if(t.kitchen==null)t.kitchen=new KitchenItem();t.kitchen.dish=new FoodDish();}SoloWorld.Validate(s);Check(s.toys.Where(t=>!Kitchen.Kind(t.kind)).All(t=>t.kitchen==null) && s.toys.All(t=>t.kitchen?.dish==null));
            var old=PlayWorld().Snapshot();old.kitchen=new KitchenState();SoloWorld.Validate(old);Check(old.kitchen==null);
            s.toys[0].kitchen=new KitchenItem{definition="forged"};Throws(()=>SoloWorld.Validate(s));
        });
        Test("kitchen migration retains every old room object and receipt and adds bounded inventory once",()=>{
            var old=PlayWorld();var before=old.Snapshot();var w=SoloWorld.WithKitchen(old);
            Check(w.Schema==13 && w.ReadToys().Length==before.toys.Length+53);
            Check(Encode(w.ReadToys().Where(t=>!Kitchen.Kind(t.kind)).ToArray())==Encode(before.toys));
            Check(Encode(w.Snapshot().bedrooms)==Encode(before.bedrooms) && Encode(w.Snapshot().receipts)==Encode(before.receipts));
            Check(ReferenceEquals(w,SoloWorld.WithKitchen(w)));SoloWorld.Validate(w.Snapshot());
        });
        Test("closed kitchen interiors reject pickup and competing support drops preserve both identities",()=>{
            var w=KitchenWorld();Check(!w.Apply(Command(w,SoloAction.Grab,"ingredient-dough")).Accepted);
            OpenKitchen(w);Good(w,SoloAction.Grab,"ingredient-dough");Cook(w,"door",target:"fridge");
            var c=Command(w,SoloAction.Drop,"ingredient-dough",target:Kitchen.Support("fridge",0),x:Kitchen.X("fridge",0),y:Kitchen.Y("fridge",0));
            var before=Encode(w.Snapshot());Check(!w.Apply(c).Accepted && Encode(w.Snapshot())==before);
            Good(w,SoloAction.CancelGrab,"ingredient-dough");SoloWorld.Validate(w.Snapshot());
        });
        Test("four independent dining seats preserve siblings when one leaves",()=>{
            var w=KitchenWorld();var actors=w.Snapshot().players.Select(p=>p.id).ToArray();
            for(var i=0;i<4;i++)Good(w,SoloAction.UseFixture,target:Kitchen.Seat(i),actor:actors[i]);
            var before=Encode(w.Snapshot().players.Skip(1).ToArray());Good(w,SoloAction.Move,x:-1200,y:200);
            Check(w.ReadPlayer("first").fixture=="" && Encode(w.Snapshot().players.Skip(1).ToArray())==before);SoloWorld.Validate(w.Snapshot());
        });
        Test("ingredient transfer consumes one durable unit and retry cannot duplicate it",()=>{
            var w=KitchenWorld();OpenKitchen(w);Cook(w,"start","cookware-0","PIZ-01");
            var c=Command(w,SoloAction.Kitchen,"ingredient-cheese",target:"cookware-0",value:"add");Check(w.Apply(c).Accepted);
            var before=Encode(w.Snapshot());Check(w.Apply(c).Duplicate && Encode(w.Snapshot())==before);
            Check(Toy(w,"ingredient-dough").kitchen.amount==15 && Toy(w,"ingredient-cheese").kitchen.amount==15 && Toy(w,"cookware-0").kitchen.dish.ingredients.Length==2);
            Good(w,SoloAction.Grab,"ingredient-cheese",actor:"second");Check(!w.Apply(Command(w,SoloAction.Kitchen,"ingredient-cheese",target:"cookware-0",value:"add")).Accepted);SoloWorld.Validate(w.Snapshot());
        });
        Test("all fifteen distinct recipes complete prepare heat serve taste and wash without duplicating portions",()=>{
            foreach(var recipe in Kitchen.Recipes){
                var w=KitchenWorld();OpenKitchen(w);Cook(w,"start","cookware-0",recipe.id);
                foreach(var ingredient in recipe.toppings)Cook(w,"add","ingredient-"+ingredient,"cookware-0");
                FinishDish(w,"cookware-0");Check(Toy(w,"cookware-0").kitchen.dish.heated);
                for(var i=0;i<4;i++)Cook(w,"serve","cookware-0","plate-"+i);
                Check(Toy(w,"cookware-0").kitchen.dish.portions==0);
                Check(w.ReadToys().Where(t=>t.kind==ToyKind.Plate && t.kitchen.dish!=null).Sum(t=>t.kitchen.dish.portions)==15);
                Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",target:"plate-4",value:"serve")).Accepted);
                for(var i=0;i<4;i++){Cook(w,"taste","plate-"+i);KitchenDrop(w,"plate-"+i,"sink",i);Cook(w,"wash","plate-"+i);Check(Toy(w,"plate-"+i).kitchen.dish==null);}
                SoloWorld.Validate(w.Snapshot());
            }
        });
        Test("four cooks heat independently and walking away or removing one dish never resets siblings",()=>{
            var w=KitchenWorld();OpenKitchen(w);var actors=w.Snapshot().players.Select(p=>p.id).ToArray();
            for(var i=0;i<4;i++){Cook(w,"start","cookware-"+i,"PIZ-01",actors[i]);Cook(w,"add","ingredient-sauce","cookware-"+i,actors[i]);Cook(w,"add","ingredient-cheese","cookware-"+i,actors[i]);Cook(w,"shape","cookware-"+i,actor:actors[i]);Cook(w,"spread","cookware-"+i,actor:actors[i]);KitchenDrop(w,"cookware-"+i,"oven",i,actors[i]);}
            for(var i=0;i<3;i++)w.AdvanceIdle(1,out _);
            Good(w,SoloAction.Grab,"cookware-0");Good(w,SoloAction.Drop,"cookware-0",x:-1500,y:200);
            Good(w,SoloAction.Travel,value:"park");var partial=Toy(w,"cookware-0").kitchen.dish.heat;
            for(var i=0;i<20;i++)w.AdvanceIdle(1,out _);
            Check(Toy(w,"cookware-0").kitchen.dish.heat==partial);
            for(var i=1;i<4;i++)Check(Kitchen.Next(Toy(w,"cookware-"+i).kitchen.dish)=="cut" && Toy(w,"cookware-"+i).kitchen.dish.heat==8);
            SoloWorld.Validate(w.Snapshot());
        });
        Test("food snapshots are deep copies and invalid duplicate portions or malformed ingredient coordinates refuse",()=>{
            var w=KitchenWorld();OpenKitchen(w);Cook(w,"readybase","cookware-0","PIZ-01");Cook(w,"cut","cookware-0");Cook(w,"serve","cookware-0","plate-0");
            var snap=w.Snapshot();snap.toys.First(t=>t.id=="plate-0").kitchen.dish.ingredients[0].ingredient="carrot";
            Check(Toy(w,"plate-0").kitchen.dish.ingredients[0].ingredient=="dough");
            snap=w.Snapshot();snap.toys.First(t=>t.id=="plate-1").kitchen.dish=Toy(w,"plate-0").kitchen.dish;Throws(()=>SoloWorld.Validate(snap));
            snap=w.Snapshot();snap.toys.First(t=>t.id=="plate-0").kitchen.dish.ingredients[0].x=float.NaN;Throws(()=>SoloWorld.Validate(snap));
            var restored=SoloWorld.Restore(w.Snapshot());Check(Encode(restored.Snapshot())==Encode(w.Snapshot()));
        });
        Test("kitchen maximum stock and food remain inside reliable-view and recovery bounds",()=>{
            var w=KitchenWorld();var actors=w.Snapshot().players;
            for(var i=0;i<4;i++)MakeSecret(w,i,actors[i].id);
            var s=w.Snapshot();foreach(var p in s.players){p.zone="garden";p.x=-1500;p.y=200;p.fixture="";p.useSeconds=0;p.stairs=0;}w=SoloWorld.Restore(s);
            OpenKitchen(w);
            for(var i=0;i<4;i++){Cook(w,"start","cookware-"+i,"PIZ-01");Cook(w,"add","ingredient-sauce","cookware-"+i);for(var j=0;j<22;j++){if(Toy(w,"ingredient-cheese").kitchen.amount==0)Cook(w,"restock","ingredient-cheese");Cook(w,"add","ingredient-cheese","cookware-"+i);}FinishDish(w,"cookware-"+i);Cook(w,"serve","cookware-"+i,"plate-"+(i*2));Cook(w,"serve","cookware-"+i,"plate-"+(i*2+1));}
            var view=JsonSerializer.Serialize(new FamilySession(w).View(),new JsonSerializerOptions{IncludeFields=true});
            if(Encoding.UTF8.GetByteCount(view)>=60000)throw new Exception("oversized view "+Encoding.UTF8.GetByteCount(view));
            var recovery=RecoveryFixture();recovery.snapshot=w.Snapshot();recovery.content=14;recovery.Validate(recovery.family,recovery.authority,recovery.world);Check(RecoveryBytes(recovery).Length<RecoveryTransfer.MaxBytes);SoloWorld.Validate(w.Snapshot());
        });
    }
}
