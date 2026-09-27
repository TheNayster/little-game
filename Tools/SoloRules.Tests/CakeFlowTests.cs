using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static FoodDish Cake(SoloWorld w,string id="cookware-0")=>Toy(w,id).kitchen.dish;
    static void CakeAction(SoloWorld w,string op,string id="cookware-0",float x=0,float y=0,string actor="first")=>Good(w,SoloAction.Kitchen,id,target:CakeFlow.Token(Cake(w,id)),value:"cake:"+op,x:x,y:y+2,actor:actor);
    static SoloWorld CakeWorld()=>SoloWorld.WithCakeFlow(PlayWorld());
    static void CakeBatter(SoloWorld w,string id="cookware-0",string actor="first"){
        Cook(w,"easy:start",id,"CAK-02",actor);foreach(var name in CakeFlow.Batter)Cook(w,"easy:add","ingredient-"+name,id,actor);
    }
    static void CakePrepare(SoloWorld w,string id="cookware-0",string actor="first"){
        foreach(var stage in new[]{"mix","pour"}){for(var i=0;i<10;i++)CakeAction(w,stage,id,.12f,actor:actor);CakeAction(w,"finish",id,actor:actor);}
    }
    static void CakeIce(SoloWorld w,string id="cookware-0",string actor="first"){
        var stage=Cake(w,id).stage;for(var i=0;i<9;i++)CakeAction(w,stage,id,-.67f+i%3*.67f,-.67f+i/3*.67f,actor);CakeAction(w,"finish",id,actor:actor);
    }
    static void CakeFlowTests()
    {
        Test("a full experimental batter reserves milk and later icing so optional additions cannot trap the recipe",()=>{
            var w=CakeWorld();Cook(w,"easy:experiment",target:"CAK-02");
            for(var i=0;i<19;i++){
                if(Toy(w,"ingredient-patty").kitchen.amount==0)Cook(w,"easy:restock","ingredient-patty");
                Cook(w,"easy:add","ingredient-patty","cookware-0");
            }
            Check(Cake(w).ingredients.Length==20);
            Check(!w.Apply(Command(w,SoloAction.Kitchen,"ingredient-patty",target:"cookware-0",value:"easy:add")).Accepted);
            foreach(var name in CakeFlow.Batter)Cook(w,"easy:add","ingredient-"+name,"cookware-0");
            CakePrepare(w);Cook(w,"easy:bake","cookware-0");Advance(w,10);Cook(w,"easy:add","ingredient-icing","cookware-0");
            Check(Cake(w).ingredients.Length==24 && Cake(w).stage=="filling");SoloWorld.Validate(w.Snapshot());
        });
        Test("every new named recipe filters unrelated stock and unusual combinations require deliberate experiment mode",()=>{
            foreach(var r in Kitchen.Recipes){
                var w=CakeWorld();Cook(w,"easy:start",target:r.id);var d=Cake(w);Check(d.guided && !d.experiment);
                if(r.id.StartsWith("CAK"))foreach(var name in new[]{"patty","cheese","broth"})Check(!Kitchen.Palette(d).Contains(name) && !w.Apply(Command(w,SoloAction.Kitchen,"ingredient-"+name,target:"cookware-0",value:"easy:add")).Accepted);
            }
            var e=CakeWorld();Cook(e,"easy:experiment",target:"CAK-02");Cook(e,"easy:add","ingredient-patty","cookware-0");
            Check(Cake(e).experiment && Cake(e).ingredients.Any(a=>a.ingredient=="patty"));e=SoloWorld.Restore(e.Snapshot());Check(Cake(e).experiment);
        });
        Test("cake migration preserves every legacy dish step without double-consuming stock",()=>{
            foreach(var r in Kitchen.Recipes)for(var step=0;step<=r.steps.Length;step++){
                var old=KitchenWorld();Cook(old,"easy:start","cookware-0",r.id);var s=old.Snapshot();var d=s.toys.First(t=>t.id=="cookware-0").kitchen.dish;d.step=step;
                old=SoloWorld.Restore(s);var before=old.Snapshot();var w=SoloWorld.WithCakeFlow(old);
                Check(w.Schema==15 && w.ReadToys().Length==before.toys.Length+1 && Encode(w.Snapshot().toys.Take(before.toys.Length).ToArray())==Encode(before.toys));
                Check(Encode(w.Snapshot().players)==Encode(before.players) && Encode(w.Snapshot().receipts)==Encode(before.receipts));
                Check(Cake(w).recipeVersion==0 && ReferenceEquals(w,SoloWorld.WithCakeFlow(w)));SoloWorld.Validate(w.Snapshot());
            }
        });
        Test("cake stage palette rejects patties cheese and early icing without consuming stock",()=>{
            var w=CakeWorld();Cook(w,"easy:start",target:"CAK-02");Check(Cake(w).ingredients.Single().ingredient=="cake-mix");
            Check(Toy(w,"ingredient-cake-mix").kitchen.amount==15);
            foreach(var name in new[]{"patty","cheese","icing","sprinkles"}){
                var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Kitchen,"ingredient-"+name,target:"cookware-0",value:"easy:add")).Accepted && Encode(w.Snapshot())==before);
            }
            foreach(var name in CakeFlow.Batter)Cook(w,"easy:add","ingredient-"+name,"cookware-0");
            Check(Cake(w).stage=="mix" && CakeFlow.Palette(Cake(w)).Length==0);
            Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",value:"easy:mix")).Accepted);
            Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",target:CakeFlow.Token(Cake(w)),value:"cake:finish")).Accepted);
        });
        Test("partial batter and transfer survive cold restore and stale gestures never operate next stage",()=>{
            var w=CakeWorld();CakeBatter(w);var stock=Encode(w.ReadToys().Where(t=>t.kind==ToyKind.Ingredient).ToArray());
            CakeAction(w,"mix",x:.2f);w=SoloWorld.Restore(w.Snapshot());Check(Math.Abs(Cake(w).mixed-.2f)<.001);
            var token=CakeFlow.Token(Cake(w));for(var i=0;i<5;i++)CakeAction(w,"mix",x:.2f);Check(Cake(w).stage=="mix");CakeAction(w,"finish");
            var before=Encode(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",target:token,value:"cake:mix",x:.2f)).Accepted && Encode(w.Snapshot())==before);
            CakeAction(w,"pour",x:.2f);w=SoloWorld.Restore(w.Snapshot());Check(Math.Abs(Cake(w).poured-.2f)<.001 && Cake(w).mixed==1);
            Check(Encode(w.ReadToys().Where(t=>t.kind==ToyKind.Ingredient).ToArray())==stock && w.ReadToys().Count(t=>t.kitchen?.dish?.id==Cake(w).id)==1);
            var c=Command(w,SoloAction.Kitchen,"cookware-0",target:CakeFlow.Token(Cake(w)),value:"cake:pour",x:.2f);Check(w.Apply(c).Accepted);before=Encode(w.Snapshot());Check(w.Apply(c).Duplicate && before==Encode(w.Snapshot()));
        });
        Test("full cake visibly staged contract bakes without icing then conserves layers decoration and four portions",()=>{
            var w=CakeWorld();CakeBatter(w);CakePrepare(w);Cook(w,"easy:bake","cookware-0");Advance(w,10);
            Check(Cake(w).stage=="filling" && Cake(w).heated && !Cake(w).ingredients.Any(a=>a.ingredient=="icing"));
            Cook(w,"easy:add","ingredient-icing","cookware-0");CakeIce(w);CakeAction(w,"stack");CakeAction(w,"finish");CakeIce(w);
            Cook(w,"easy:add","ingredient-strawberry","cookware-0");Check(Cake(w).ingredients.Last().phase=="decorate");CakeAction(w,"finish");
            CakeAction(w,"cut",x:0);CakeAction(w,"cut",x:1);CakeAction(w,"finish");var ingredients=Encode(Cake(w).ingredients);
            for(var i=0;i<4;i++){Cook(w,"easy:serve","cookware-0","plate-"+i);var d=Cake(w,"plate-"+i);Check(d.layers==2 && d.icingMask==511 && d.portions==1<<i && Encode(d.ingredients)==ingredients);}
            Check(Cake(w).portions==0);SoloWorld.Validate(w.Snapshot());
        });
        Test("four cakes stay independent through mixing pouring baking decoration and departing cook",()=>{
            var w=CakeWorld();var actors=w.Snapshot().players.Select(p=>p.id).ToArray();
            for(var i=0;i<4;i++)CakeBatter(w,"cookware-"+i,actors[i]);
            CakeAction(w,"mix","cookware-0",.2f,actor:actors[0]);CakePrepare(w,"cookware-1",actors[1]);
            CakePrepare(w,"cookware-2",actors[2]);Cook(w,"easy:bake","cookware-2",actor:actors[2]);
            var others=Encode(w.ReadToys().Where(t=>t.id=="cookware-0" || t.id=="cookware-1").ToArray());Good(w,SoloAction.Travel,value:"park",actor:actors[2]);Advance(w,10);
            Check(Cake(w,"cookware-2").heated && Cake(w,"cookware-3").mixed==0 && others==Encode(w.ReadToys().Where(t=>t.id=="cookware-0" || t.id=="cookware-1").ToArray()));
            w=SoloWorld.Restore(w.Snapshot());SoloWorld.Validate(w.Snapshot());
        });
        Test("two cake cooks cannot both consume the last egg; cooperation shares a single mixture",()=>{
            var w=CakeWorld();Cook(w,"easy:start","cookware-0","CAK-02");Cook(w,"easy:start","cookware-1","CAK-02","second");
            var s=w.Snapshot();s.toys.First(t=>t.id=="ingredient-egg").kitchen.amount=1;w=SoloWorld.Restore(s);
            Cook(w,"easy:add","ingredient-egg","cookware-0");Check(!w.Apply(Command(w,SoloAction.Kitchen,"ingredient-egg",target:"cookware-1",value:"easy:add",actor:"second")).Accepted);
            Cook(w,"easy:add","ingredient-milk","cookware-0","second");Cook(w,"easy:add","ingredient-chocolate","cookware-0");
            CakeAction(w,"mix",x:.2f);CakeAction(w,"mix",x:.2f,actor:"second");Check(Math.Abs(Cake(w).mixed-.4f)<.001);SoloWorld.Validate(w.Snapshot());
        });
        Test("cake rejects malformed partial records and ready base explicitly skips only preparation",()=>{
            var w=CakeWorld();Cook(w,"easy:readybase",target:"CAK-02");Check(Cake(w).stage=="filling" && Cake(w).mixed==1 && Cake(w).poured==1 && Cake(w).layers==1);
            foreach(var invalid in new[]{float.NaN,float.PositiveInfinity,-1f,1.1f}){var s=w.Snapshot();s.toys.First(t=>t.id=="cookware-0").kitchen.dish.mixed=invalid;Throws(()=>SoloWorld.Validate(s));}
            var state=w.Snapshot();state.toys.First(t=>t.id=="cookware-0").kitchen.dish.stage="serve";Throws(()=>SoloWorld.Validate(state));
        });
    }
}
