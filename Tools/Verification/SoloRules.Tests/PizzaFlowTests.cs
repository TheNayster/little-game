using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static void PizzaAction(GameWorld w,string op,string id="cookware-0",float x=0,float y=0,string actor="first")=>Good(w,SoloAction.Kitchen,id,target:PreparationFlow.Token(Cake(w,id)),value:"prepare:"+op,x:x,y:y+2,actor:actor);
    static void PizzaNext(GameWorld w,string id="cookware-0",string actor="first")
    {
        var d=Cake(w,id);var stage=d.stage;
        if(stage=="knead" || stage=="roll"){for(var i=0;i<5;i++)PizzaAction(w,stage,id,.2f,actor:actor);PizzaAction(w,"finish",id,actor:actor);}
        else if(stage=="vegetables" || stage=="sauce"){foreach(var name in PizzaFlow.Palette(d))Cook(w,"easy:add","ingredient-"+name,id,actor);}
        else if(stage=="spread"){for(var i=0;i<9;i++)PizzaAction(w,stage,id,-.67f+i%3*.67f,-.67f+i/3*.67f,actor);PizzaAction(w,"finish",id,actor:actor);}
        else if(stage=="toppings"){foreach(var name in PizzaFlow.Missing(d))Cook(w,"easy:add","ingredient-"+name,id,actor);PizzaAction(w,"finish",id,actor:actor);}
        else if(stage=="chop" || stage=="cut"){PizzaAction(w,stage,id,0,actor:actor);PizzaAction(w,stage,id,1,actor:actor);PizzaAction(w,"finish",id,actor:actor);}
        else if(stage=="bake"){Cook(w,"easy:bake",id,actor:actor);Advance(w,10);}
        else throw new Exception("Unexpected pizza phase "+stage);
    }
    static void PizzaFlowTests()
    {
        Test("reusing an archived oven tray waits on the worktop for an explicit bake",()=>{
            var w=GameWorld.WithPizzaPreparation(KitchenWorld());Cook(w,"easy:readybase",target:"PIZ-01");
            var s=w.Snapshot();var tray=s.toys.First(t=>t.id=="cookware-0");tray.container=Kitchen.Support("oven",0);tray.x=Kitchen.X("oven",0);tray.y=Kitchen.Y("oven",0);w=GameWorld.Restore(s);
            PackFood(w,"cookware-0","first");Cook(w,"easy:start",target:"PIZ-05");Check(Toy(w,"cookware-0").container.StartsWith("kitchen/counter/"));
            while(Cake(w).stage!="bake")PizzaNext(w);Advance(w,10);Check(Cake(w).stage=="bake" && Cake(w).heat==0);PizzaNext(w);Check(Cake(w).stage=="cut");
            PackFood(w,"cookware-0","first");s=w.Snapshot();
            var plate=s.toys.First(t=>t.id=="plate-0");plate.container=Kitchen.Support("counter",0);plate.x=Kitchen.X("counter",0);plate.y=Kitchen.Y("counter",0);w=GameWorld.Restore(s);
            var before=CreationJson(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",target:"PIZ-01",value:"easy:start")).Accepted && before==CreationJson(w.Snapshot()));
        });
        Test("five pizza styles preserve real dough sauce chopped vegetables and placements through every saved stage",()=>{
            foreach(var recipe in Kitchen.Recipes.Take(5)){
                var w=GameWorld.WithPizzaPreparation(KitchenWorld());Cook(w,"easy:start",target:recipe.id);Check(Cake(w).recipeVersion==3 && Cake(w).stage=="knead");
                for(var i=0;i<15 && Cake(w).stage!="serve";i++){PizzaNext(w);var food=CreationJson(Cake(w));w=GameWorld.Restore(w.Snapshot());Check(food==CreationJson(Cake(w)));}
                var d=Cake(w);Check(d.stage=="serve" && d.icingMask==511 && d.cutMask==3 && d.heated && d.mixed==1 && d.poured==1);Check(recipe.id!="PIZ-03" || d.step==3 && !PizzaFlow.Has(d,"cheese"));
                var exact=CreationJson(d.ingredients);for(var i=0;i<4;i++){Cook(w,"easy:serve","cookware-0","plate-"+i);Check(Cake(w,"plate-"+i).portions==1<<i && exact==CreationJson(Cake(w,"plate-"+i).ingredients));}Check(Cake(w).portions==0);GameWorld.Validate(w.Snapshot());
            }
        });
        Test("pizza dough and sauce retain partial touch progress and reject stale commands without consuming more stock",()=>{
            var w=GameWorld.WithPizzaPreparation(KitchenWorld());Cook(w,"easy:start",target:"PIZ-05");PizzaAction(w,"knead",x:.2f);Check(Cake(w).mixed==.2f);w=GameWorld.Restore(w.Snapshot());var token=PreparationFlow.Token(Cake(w));PizzaNext(w);var before=CreationJson(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",target:token,value:"prepare:knead",x:.2f)).Accepted && before==CreationJson(w.Snapshot()));
            PizzaNext(w);PizzaNext(w);Check(Cake(w).stage=="spread");PizzaAction(w,"spread",x:-.67f,y:-.67f);w=GameWorld.Restore(w.Snapshot());Check(Cake(w).icingMask==1);Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",target:PreparationFlow.Token(Cake(w)),value:"prepare:finish")).Accepted);PizzaNext(w);
            var t=Toy(w,"cookware-0");Good(w,SoloAction.Kitchen,"ingredient-tomato",target:t.id,value:"easy:add",x:t.x+55,y:t.y-40);Check(Cake(w).ingredients.Last().x==.55f && Cake(w).ingredients.Last().y==-.4f);var placement=CreationJson(Cake(w).ingredients.Last());PizzaNext(w);PizzaNext(w);Check(placement==CreationJson(Cake(w).ingredients.Single(a=>a.ingredient=="tomato")));
        });
        Test("pizza recipe palettes reject cake ingredients and reserve required toppings against experiment clutter",()=>{
            var w=GameWorld.WithPizzaPreparation(KitchenWorld());Cook(w,"easy:start",target:"PIZ-02");
            foreach(var name in new[]{"icing","chocolate","cheese","sauce"})Check(!w.Apply(Command(w,SoloAction.Kitchen,"ingredient-"+name,target:"cookware-0",value:"easy:add")).Accepted);
            while(Cake(w).stage!="toppings")PizzaNext(w);foreach(var name in new[]{"icing","patty","carrot"})Check(!Kitchen.Palette(Cake(w)).Contains(name));
            var e=GameWorld.WithPizzaPreparation(KitchenWorld());Cook(e,"easy:experiment",target:"PIZ-05");while(Cake(e).stage!="toppings")PizzaNext(e);
            for(var i=0;i<19;i++){if(Toy(e,"ingredient-banana").kitchen.amount==0)Cook(e,"easy:restock","ingredient-banana");Cook(e,"easy:add","ingredient-banana","cookware-0");}
            Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",target:PreparationFlow.Token(Cake(w)),value:"prepare:finish")).Accepted);
            Check(!e.Apply(Command(e,SoloAction.Kitchen,"ingredient-banana",target:"cookware-0",value:"easy:add")).Accepted);PizzaNext(e);Check(Cake(e).ingredients.Length==24 && Cake(e).stage=="bake");GameWorld.Validate(e.Snapshot());
        });
        Test("four pizza cooks can leave independently and preserve exact archived leftovers",()=>{
            var w=GameWorld.WithPizzaPreparation(KitchenWorld());var actors=w.Snapshot().players.Select(p=>p.id).ToArray();
            for(var i=0;i<4;i++)Cook(w,"easy:start","cookware-"+i,"PIZ-0"+(i+1),actors[i]);
            PizzaAction(w,"knead","cookware-0",.2f,actor:actors[0]);PizzaNext(w,"cookware-1",actors[1]);while(Cake(w,"cookware-2").stage!="bake")PizzaNext(w,"cookware-2",actors[2]);
            var others=CreationJson(new[]{Cake(w),Cake(w,"cookware-1"),Cake(w,"cookware-3")});Cook(w,"easy:bake","cookware-2",actor:actors[2]);Good(w,SoloAction.Travel,value:"park",actor:actors[2]);Advance(w,10);Check(Cake(w,"cookware-2").stage=="cut" && others==CreationJson(new[]{Cake(w),Cake(w,"cookware-1"),Cake(w,"cookware-3")}));
            PackFood(w,"cookware-0",actors[0]);Check(w.ReadCreations().foods.Single().dish.mixed==.2f);w=GameWorld.Restore(w.Snapshot());Check(w.ReadCreations().foods.Single().dish.recipeVersion==3);
        });
        Test("pizza migration leaves all legacy dishes untouched and refuses impossible staged saves",()=>{
            var old=GameWorld.WithCakeFamilies(KitchenWorld());Cook(old,"easy:start",target:"PIZ-03");var before=old.Snapshot();var w=GameWorld.WithPizzaPreparation(old);var after=w.Snapshot();Check(after.schema==25);after.schema=24;after.revision--;Check(CreationJson(before)==CreationJson(after));
            foreach(var recipe in Kitchen.Recipes.Take(5)){w=GameWorld.WithPizzaPreparation(KitchenWorld());Cook(w,"easy:readybase",target:recipe.id);Check(Cake(w).stage=="cut" && Cake(w).heated);foreach(var change in new Action<FoodDish>[] {d=>d.mixed=.1f,d=>d.poured=0,d=>d.icingMask=0,d=>d.layers=1,d=>d.step=4,d=>d.stage="serve",d=>d.ingredients=null}){var s=w.Snapshot();change(s.toys.First(t=>t.id=="cookware-0").kitchen.dish);Throws(()=>GameWorld.Validate(s));}}
        });
    }
}
