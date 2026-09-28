using System;
using System.Linq;
using LittleWeeps.Core;

static partial class Program
{
    static void FamilyCakeNext(SoloWorld w,string id="cookware-0",string actor="first")
    {
        var d=Cake(w,id);var stage=d.stage;
        if(stage=="ingredients"){foreach(var name in CakeFamilies.Batter(d.recipe))Cook(w,"easy:add","ingredient-"+name,id,actor);}
        else if(stage=="mix" || stage=="pour"){for(var i=0;i<5;i++)CakeAction(w,stage,id,.2f,actor:actor);CakeAction(w,"finish",id,actor:actor);}
        else if(stage=="chop" || stage=="features" || stage=="cut"){CakeAction(w,stage,id,0,actor:actor);CakeAction(w,stage,id,1,actor:actor);CakeAction(w,"finish",id,actor:actor);}
        else if(stage=="colors"){foreach(var color in new[]{5,1,4})CakeAction(w,stage,id,color,actor:actor);}
        else if(stage=="bake"){Cook(w,"easy:bake",id,actor:actor);Advance(w,10);}
        else if(stage=="stack")CakeAction(w,stage,id,actor:actor);
        else if(stage=="ice"){Cook(w,"easy:add","ingredient-icing",id,actor);CakeIce(w,id,actor);}
        else if(stage=="decorate"){Cook(w,"easy:add","ingredient-"+CakeFamilies.Decorations(d)[0],id,actor);CakeAction(w,"finish",id,actor:actor);}
        else throw new Exception("Unexpected cake phase "+stage);
    }
    static void CakeFamilyTests()
    {
        Test("four distinctive cakes retain every intermediate preparation and counted ingredient through cold restore",()=>{
            var w=SoloWorld.WithCakeFamilies(KitchenWorld());var actors=w.Snapshot().players.Select(p=>p.id).ToArray();
            for(var i=0;i<4;i++)Cook(w,"easy:start","cookware-"+i,CakeFamilies.Recipes[i],actors[i]);
            for(var turn=0;turn<15;turn++)for(var i=0;i<4;i++){
                var id="cookware-"+i;if(Cake(w,id).stage=="serve")continue;
                var others=CreationJson(w.ReadToys().Where(t=>t.kind==ToyKind.Cookware && t.id!=id).Select(t=>t.kitchen.dish).ToArray());
                FamilyCakeNext(w,id,actors[i]);Check(others==CreationJson(w.ReadToys().Where(t=>t.kind==ToyKind.Cookware && t.id!=id).Select(t=>t.kitchen.dish).ToArray()));
                var food=CreationJson(Cake(w,id));w=SoloWorld.Restore(w.Snapshot());Check(food==CreationJson(Cake(w,id)));SoloWorld.Validate(w.Snapshot());
            }
            for(var i=0;i<4;i++){var id="cookware-"+i;var d=Cake(w,id);Check(d.stage=="serve" && d.recipeVersion==2 && d.cutMask==3 && d.icingMask==511 && d.heated);var before=CreationJson(d);PackFood(w,id,actors[i]);var saved=w.ReadCreations().foods.Single(f=>f.owner==actors[i]);Check(before==CreationJson(saved.dish));}
            var data=w.ReadCreations();Check(CakeFamilies.Colors(data.foods.Single(f=>f.dish.recipe=="CAK-04").dish).SequenceEqual(new[]{5,1,4}));Check(data.foods.Single(f=>f.dish.recipe=="CAK-01").dish.step==3 && data.foods.Single(f=>f.dish.recipe=="CAK-05").dish.step==3);
        });
        Test("family cake choices reject stale taps, wrong phases and unrelated ingredients without changing stock",()=>{
            var w=SoloWorld.WithCakeFamilies(KitchenWorld());Cook(w,"easy:start",target:"CAK-04");
            foreach(var name in new[]{"patty","cheese","icing","sprinkles"}){var before=CreationJson(w.Snapshot());Check(!w.Apply(Command(w,SoloAction.Kitchen,"ingredient-"+name,target:"cookware-0",value:"easy:add")).Accepted && before==CreationJson(w.Snapshot()));}
            FamilyCakeNext(w);FamilyCakeNext(w);Check(Cake(w).stage=="colors");var token=CakeFlow.Token(Cake(w));var c=Command(w,SoloAction.Kitchen,"cookware-0",target:token,value:"cake:colors",x:5);Check(w.Apply(c).Accepted);var after=CreationJson(w.Snapshot());Check(w.Apply(c).Duplicate && after==CreationJson(w.Snapshot()));
            Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",target:token,value:"cake:colors",x:2)).Accepted && after==CreationJson(w.Snapshot()));
            foreach(var x in new[]{-1f,6f,.5f,float.NaN})Check(!w.Apply(Command(w,SoloAction.Kitchen,"cookware-0",target:CakeFlow.Token(Cake(w)),value:"cake:colors",x:x)).Accepted);
            CakeAction(w,"colors",x:2);CakeAction(w,"colors",x:4);Check(Cake(w).stage=="pour" && CakeFamilies.Colors(Cake(w)).SequenceEqual(new[]{5,2,4}));
        });
        Test("all family ready bases keep post-bake assembly icing and serving; four portions stay unique",()=>{
            foreach(var recipe in CakeFamilies.Recipes){var w=SoloWorld.WithCakeFamilies(KitchenWorld());Cook(w,"easy:readybase",target:recipe);Check(Cake(w).heated && Cake(w).icingMask==0);for(var i=0;i<9 && Cake(w).stage!="serve";i++)FamilyCakeNext(w);Check(Cake(w).stage=="serve");var ingredients=CreationJson(Cake(w).ingredients);for(var i=0;i<4;i++){Cook(w,"easy:serve","cookware-0","plate-"+i);Check(Cake(w,"plate-"+i).portions==1<<i && CreationJson(Cake(w,"plate-"+i).ingredients)==ingredients);}Check(Cake(w).portions==0);SoloWorld.Validate(w.Snapshot());}
        });
        Test("family cake migration leaves legacy dishes and stored creations byte-equivalent",()=>{
            var old=SoloWorld.WithHomeCreations(KitchenWorld());Cook(old,"easy:start",target:"CAK-03");var before=old.Snapshot();var w=SoloWorld.WithCakeFamilies(old);var after=w.Snapshot();Check(after.schema==24);after.schema=23;after.revision--;Check(CreationJson(before)==CreationJson(after));Check(ReferenceEquals(w,SoloWorld.WithCakeFamilies(w)));
            var chocolate=SoloWorld.WithHomeCreations(KitchenWorld());CakeBatter(chocolate);CakeAction(chocolate,"mix",x:.2f);var exact=CreationJson(Cake(chocolate));Check(CreationJson(Cake(SoloWorld.WithCakeFamilies(chocolate)))==exact);
        });
        Test("family cake validation refuses impossible shapes colors and skipped preparation",()=>{
            foreach(var recipe in CakeFamilies.Recipes){var w=SoloWorld.WithCakeFamilies(KitchenWorld());Cook(w,"easy:readybase",target:recipe);
                foreach(var change in new Action<FoodDish>[] {d=>d.layers=4,d=>d.mixed=.3f,d=>d.poured=0,d=>d.heat=0,d=>d.stage="serve",d=>d.step=999999,d=>d.ingredients=null}){var s=w.Snapshot();change(s.toys.First(t=>t.id=="cookware-0").kitchen.dish);Throws(()=>SoloWorld.Validate(s));}}
        });
    }
}
