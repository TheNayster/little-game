using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static void KitchenEasyTests()
    {
        Test("friendly kitchen migrates occupied furniture while preserving old food and loose objects",()=>{
            var old=GameWorld.WithKitchen(PlayWorld());OpenKitchen(old);Cook(old,"start","cookware-0","PIZ-01");Cook(old,"add","ingredient-cheese","cookware-0");
            Good(old,SoloAction.UseFixture,target:Kitchen.Seat(0));Good(old,SoloAction.Grab,"plate-1",actor:"second");
            var before=old.Snapshot();var w=GameWorld.WithFriendlyKitchen(old);Check(w.Schema==14 && w.ReadToys().Length==before.toys.Length);
            Check(Encode(new[]{Toy(w,"cookware-0").kitchen})==Encode(new[]{Toy(old,"cookware-0").kitchen}));
            Check(Encode(new[]{Toy(w,"plate-1")})==Encode(new[]{Toy(old,"plate-1")}));Check(w.ReadPlayer("first").x==Kitchen.SeatX(Kitchen.Seat(0)));
            Check(Encode(w.Snapshot().receipts)==Encode(before.receipts) && Encode(w.Snapshot().bedrooms)==Encode(before.bedrooms));
            Check(ReferenceEquals(w,GameWorld.WithFriendlyKitchen(w)));GameWorld.Validate(w.Snapshot());
        });
        Test("four assisted cooks choose distinct trays with all doors shut and physical storage remains closed",()=>{
            var w=KitchenWorld();var actors=w.Snapshot().players.Select(p=>p.id).ToArray();
            for(var i=0;i<4;i++)Cook(w,"easy:start",target:"PIZ-01",actor:actors[i]);
            Check(w.ReadToys().Count(t=>t.kind==ToyKind.Cookware && t.kitchen.dish!=null)==4);
            Check(w.ReadToys().Where(t=>t.kind==ToyKind.Cookware).Select(t=>t.kitchen.cook).Distinct().Count()==4);
            Check(!w.ReadKitchen().fridgeOpen && w.ReadKitchen().cupboards.All(v=>!v));
            Check(!w.Apply(Command(w,SoloAction.Grab,"ingredient-cheese")).Accepted);
            var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Kitchen,value:"easy:start",target:"PIZ-01")).Accepted && Encode(w.Snapshot())==before);
        });
        Test("assisted addition cannot take a held last ingredient and retry cannot duplicate food",()=>{
            var w=KitchenWorld();Cook(w,"easy:start",target:"PIZ-01");var s=w.Snapshot();var ingredient=s.toys.First(t=>t.id=="ingredient-cheese");ingredient.kitchen.amount=1;w=GameWorld.Restore(s);
            Cook(w,"door",target:"fridge");Good(w,SoloAction.Grab,"ingredient-cheese",actor:"second");
            Check(!w.Apply(Command(w,SoloAction.Kitchen,"ingredient-cheese",target:"cookware-0",value:"easy:add")).Accepted);
            Good(w,SoloAction.CancelGrab,"ingredient-cheese",actor:"second");var c=Command(w,SoloAction.Kitchen,"ingredient-cheese",target:"cookware-0",value:"easy:add",x:Kitchen.X("counter",0)+42,y:430);
            Check(w.Apply(c).Accepted);var before=Encode(w.Snapshot());Check(w.Apply(c).Duplicate && Encode(w.Snapshot())==before);
            Check(Toy(w,"ingredient-cheese").kitchen.amount==0 && !w.Apply(Command(w,SoloAction.Kitchen,"ingredient-cheese",target:"cookware-0",value:"easy:add")).Accepted);
            GameWorld.Validate(w.Snapshot());
        });
        Test("assisted fifteen recipes finish with closed doors and preserve decorations through serving",()=>{
            foreach(var r in Kitchen.Recipes){
                var w=KitchenWorld();Cook(w,"easy:start",target:r.id);
                foreach(var ingredient in r.toppings)Cook(w,"easy:add","ingredient-"+ingredient,"cookware-0");
                for(var limit=0;limit<20 && Kitchen.Next(Toy(w,"cookware-0").kitchen.dish)!="serve";limit++){
                    var next=Kitchen.Next(Toy(w,"cookware-0").kitchen.dish);Cook(w,"easy:"+(next=="heat"?"bake":next),"cookware-0");
                    if(next=="heat")for(var i=0;i<9;i++)w.AdvanceIdle(1,out _);
                }
                var before=Encode(Toy(w,"cookware-0").kitchen.dish.ingredients);Cook(w,"easy:serve","cookware-0","plate-4");
                Check(before==Encode(Toy(w,"plate-4").kitchen.dish.ingredients));Cook(w,"easy:taste","plate-4");Cook(w,"easy:wash","plate-4");
                Check(Toy(w,"plate-4").kitchen.dish==null && !w.ReadKitchen().ovenOpen && !w.ReadKitchen().waterOn);GameWorld.Validate(w.Snapshot());
            }
        });
        Test("undo cannot rewind sibling additions or refunded ingredient batches",()=>{
            var w=KitchenWorld();Cook(w,"easy:start",target:"PIZ-01");Cook(w,"easy:add","ingredient-cheese","cookware-0");Cook(w,"easy:add","ingredient-tomato","cookware-0","second");
            Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",value:"easy:undo")).Accepted);
            Cook(w,"easy:undo","cookware-0",actor:"second");Check(Toy(w,"ingredient-tomato").kitchen.amount==15 && Toy(w,"cookware-0").kitchen.dish.ingredients.Length==2);
            Cook(w,"easy:undo","cookware-0");Check(Toy(w,"ingredient-cheese").kitchen.amount==15);GameWorld.Validate(w.Snapshot());
        });
        Test("four assisted bakes survive a departing cook closed doors and cold restore",()=>{
            var w=KitchenWorld();var actors=w.Snapshot().players.Select(p=>p.id).ToArray();
            for(var i=0;i<4;i++){var id="cookware-"+i;Cook(w,"easy:start",target:"PIZ-01",actor:actors[i]);Cook(w,"easy:add","ingredient-sauce",id,actors[i]);Cook(w,"easy:add","ingredient-cheese",id,actors[i]);Cook(w,"easy:shape",id,actor:actors[i]);Cook(w,"easy:spread",id,actor:actors[i]);Cook(w,"easy:bake",id,actor:actors[i]);}
            w.AdvanceIdle(1,out _);w.AdvanceIdle(1,out _);Good(w,SoloAction.Travel,value:"park");w=GameWorld.Restore(w.Snapshot());
            for(var i=0;i<9;i++)w.AdvanceIdle(1,out _);Check(w.ReadToys().Where(t=>t.kind==ToyKind.Cookware).All(t=>t.kitchen.dish.heated));GameWorld.Validate(w.Snapshot());
        });
    }
}
