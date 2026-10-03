using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static void MealAction(GameWorld w,string op,string id="cookware-0",float x=0,string actor="first")=>Good(w,SoloAction.Kitchen,id,target:PreparationFlow.Token(Cake(w,id)),value:"meal:"+op,x:x,actor:actor);
    static void MealNext(GameWorld w,string id="cookware-0",string actor="first")
    {
        var d=Cake(w,id);var stage=d.stage;
        if(MealFlow.HeatStage(stage)){Cook(w,"easy:bake",id,actor:actor);Advance(w,10);return;}
        if(stage=="ingredients" || stage=="vegetables" || stage=="sauce"){
            foreach(var ingredient in MealFlow.Palette(d))Cook(w,"easy:add","ingredient-"+ingredient,id,actor);return;
        }
        if(MealFlow.Continuous.Contains(stage))for(var i=0;i<5;i++)MealAction(w,stage,id,.2f,actor);
        else if(stage=="chop"){MealAction(w,stage,id,0,actor);MealAction(w,stage,id,1,actor);}
        else if(stage=="flip")MealAction(w,stage,id,0,actor);
        else if(stage=="stack")while(MealFlow.Read(Cake(w,id)).stack<MealFlow.StackCount(d))MealAction(w,stage,id,0,actor);
        else if(stage=="assembly")foreach(var ingredient in new[]{"bun","lettuce"})Cook(w,"easy:add","ingredient-"+ingredient,id,actor);
        else if(stage=="fruit")Cook(w,"easy:add","ingredient-banana",id,actor);
        MealAction(w,"finish",id,actor:actor);
    }
    static void MealFlowTests()
    {
        Test("five meals transform distinct pan pot and assembly stages with cold restore at every phase",()=>{
            foreach(var recipe in Kitchen.Recipes.Skip(10)){
                var w=GameWorld.WithMealPreparation(KitchenWorld());Cook(w,"easy:start",target:recipe.id);Check(Cake(w).recipeVersion==4);
                for(var i=0;i<20 && Cake(w).stage!="serve";i++){MealNext(w);var exact=CreationJson(Cake(w));w=GameWorld.Restore(w.Snapshot());Check(exact==CreationJson(Cake(w)));}
                var d=Cake(w);Check(d.stage=="serve" && d.heated && MealFlow.Read(d).heat==(recipe.id=="MEAL-01" || recipe.id=="MEAL-04" || recipe.id=="MEAL-05"?2:1));
                var contents=CreationJson(d.ingredients);for(var i=0;i<4;i++){Cook(w,"easy:serve","cookware-0","plate-"+i);Check(Cake(w,"plate-"+i).portions==1<<i && contents==CreationJson(Cake(w,"plate-"+i).ingredients));}Check(Cake(w).portions==0);
            }
        });
        Test("each pan side waits for deliberate heating and an interrupted flip can be stored",()=>{
            var w=GameWorld.WithMealPreparation(KitchenWorld());Cook(w,"easy:start",target:"MEAL-01");MealNext(w);Advance(w,10);Check(Cake(w).heat==0);
            Cook(w,"easy:bake","cookware-0");Advance(w,3);var partial=Cake(w).heat;Cook(w,"easy:bake","cookware-0");Check(Cake(w).heat==partial);Advance(w,10);Check(Cake(w).stage=="flip" && !Cake(w).heated);
            PackFood(w,"cookware-0","first");w=GameWorld.Restore(w.Snapshot());var record=w.ReadCreations().foods.Single();Good(w,SoloAction.Kitchen,target:record.key.ToString(),value:"restore-food");Check(Cake(w).stage=="flip" && MealFlow.Read(Cake(w)).heat==1);
            MealNext(w);Check(Cake(w).stage=="cheese");Cook(w,"easy:add","ingredient-cheese","cookware-0");MealNext(w);Advance(w,10);Check(Cake(w).stage=="cook-second" && MealFlow.Read(Cake(w)).heat==1);
            Cook(w,"easy:bake","cookware-0");Check(Cake(w).heat==0);Advance(w,2);Check(Cake(w).heat>0 && Cake(w).heat<4);Advance(w,10);Check(Cake(w).stage=="assembly" && Cake(w).heated);
        });
        Test("meal phase palettes keep fruit and fresh salad out of batter and cooking pots",()=>{
            foreach(var recipe in Kitchen.Recipes.Skip(10)){
                var w=GameWorld.WithMealPreparation(KitchenWorld());Cook(w,"easy:start",target:recipe.id);
                Check(!Kitchen.CanAdd(Cake(w),"icing") && !Kitchen.CanAdd(Cake(w),"chocolate"));
                if(recipe.id=="MEAL-04")Check(!Kitchen.CanAdd(Cake(w),"banana") && Kitchen.CanAdd(Cake(w),"egg"));
                if(recipe.id=="MEAL-01")Check(!Kitchen.CanAdd(Cake(w),"lettuce") && !Kitchen.CanAdd(Cake(w),"bun"));
                if(recipe.id=="MEAL-02")Check(!Kitchen.CanAdd(Cake(w),"sauce"));
            }
        });
        Test("four hobs run independently and departure stops safely at each needed flip",()=>{
            var w=GameWorld.WithMealPreparation(KitchenWorld());var actors=w.Snapshot().players.Select(p=>p.id).ToArray();
            for(var i=0;i<4;i++){var id="cookware-"+i;Cook(w,"easy:start",id,"MEAL-04",actors[i]);while(Cake(w,id).stage!="cook-first")MealNext(w,id,actors[i]);Cook(w,"easy:bake",id,actor:actors[i]);}
            Check(w.Snapshot().toys.Count(t=>Kitchen.Heating(t))==4);Good(w,SoloAction.Travel,value:"park",actor:actors[3]);Advance(w,10);
            Check(Enumerable.Range(0,4).All(i=>Cake(w,"cookware-"+i).stage=="flip") && w.Snapshot().toys.All(t=>!Kitchen.Heating(t)));w=GameWorld.Restore(w.Snapshot());
            var other=CreationJson(Cake(w,"cookware-3"));MealNext(w,"cookware-0",actors[0]);Cook(w,"easy:bake","cookware-0",actor:actors[0]);Advance(w,10);Check(Cake(w).stage=="stack" && other==CreationJson(Cake(w,"cookware-3")));
        });
        Test("meal water and stirring retain partial progress while stale touches cannot enter the next phase",()=>{
            var w=GameWorld.WithMealPreparation(KitchenWorld());Cook(w,"easy:start",target:"MEAL-02");MealAction(w,"water",x:.2f);w=GameWorld.Restore(w.Snapshot());Check(MealFlow.Read(Cake(w)).water==.2f);
            var old=PreparationFlow.Token(Cake(w));MealNext(w);var before=CreationJson(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",target:old,value:"meal:water",x:.2f)).Accepted && before==CreationJson(w.Snapshot()));
            MealAction(w,"stir",x:.2f);PackFood(w,"cookware-0","first");Check(MealFlow.Read(w.ReadCreations().foods.Single().dish).stir==.2f);w=GameWorld.Restore(w.Snapshot());Check(w.ReadCreations().foods.Single().dish.recipeVersion==4);
        });
        Test("ready meals disclose their skip and still require finishing while malformed progress is refused",()=>{
            foreach(var recipe in Kitchen.Recipes.Skip(10)){
                var w=GameWorld.WithMealPreparation(KitchenWorld());Cook(w,"easy:readybase",target:recipe.id);Check(Cake(w).heated && Cake(w).stage!="serve");
                foreach(var change in new Action<FoodDish>[] {d=>d.preparation="bad",d=>{var p=MealFlow.Read(d);p.started=10;MealFlow.Save(d,p);},d=>{var p=MealFlow.Read(d);p.mix=float.NaN;MealFlow.Save(d,p);},d=>d.stage="serve",d=>d.heated=false}){
                    var s=w.Snapshot();change(s.toys.First(t=>t.id=="cookware-0").kitchen.dish);Throws(()=>GameWorld.Validate(s));
                }
                for(var i=0;i<12 && Cake(w).stage!="serve";i++)MealNext(w);Check(Cake(w).stage=="serve");
            }
        });
        Test("build 210 format-one archive reads exactly and upgrades without losing food",()=>{
            // Captured from disposable native pizza qualification, never a family save.
            const string oldArchive="Y2RlYGBgBBEcDAU5iZWpRbqGPAzJ+fnZ5YlFqboG+kZsDAGeUboGpkAlDPwgxYyMDMwMEPCfkQHMZmUoTi0qS2VgaLCHYBBQcGATYcjMSy9KTclMzSvRTckvTc/QN9A3YWUAMxEW/v8PNY8BRUNxYmlyKkQDmInQABUwPv3Ablb6132iyLqSM1JTi0HajNkYIGyEPg6GkvyCAqDiYq7Dqvvlv2bYomgtyc9NLMkHajVkY4CwsWl9KHVq35uIRjvsWo0IaLUDaRVHcXBiQXFmcmkuyF4OBhgPm3ZQCK1buGcfAwMA";
            var data=HomeCreations.Decode(oldArchive);Check(data.foods.Length>0 && data.foods.All(f=>f.dish.recipeVersion==3 && f.dish.preparation==null));
            var upgraded=HomeCreations.Decode(HomeCreations.Encode(data));Check(CreationJson(data)==CreationJson(upgraded));
        });
        Test("meal migration preserves legacy food and stored creation bytes",()=>{
            var old=GameWorld.WithPizzaPreparation(KitchenWorld());Cook(old,"easy:readybase",target:"PIZ-02");PackFood(old,"cookware-0","first");Cook(old,"easy:start",target:"MEAL-01");var before=old.Snapshot();var upgraded=GameWorld.WithMealPreparation(old).Snapshot();Check(upgraded.schema==26);upgraded.schema=25;upgraded.revision--;Check(CreationJson(before)==CreationJson(upgraded));
        });
    }
}
