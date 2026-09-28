using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public sealed partial class SoloWorld
    {
        public static SoloWorld WithFriendlyKitchen(SoloWorld world)
        {
            world=WithKitchen(world);if(world.Schema>=Kitchen.EasySchema)return world;
            var s=world.Snapshot();s.schema=Kitchen.EasySchema;
            // Move only anchored furniture contents/occupants. Loose creations,
            // held objects, receipts and enrollment retain their original identity.
            foreach(var t in s.toys)if(Kitchen.Slot(t.container,out var group,out var slot))t.x=Kitchen.X(group,slot);
            foreach(var p in s.players)if(Kitchen.Seat(p.fixture))p.x=Kitchen.SeatX(p.fixture);
            s.revision++;Validate(s);return new SoloWorld(s);
        }

        private bool PrepAvailable(SoloToy t,SoloPlayer p)
        {
            if(t==null || t.zone!=p.zone || t.holder!="" && t.holder!=p.id)return false;
            // Assisted preparation can draw from communal kitchen storage; this
            // does not expose closed contents to ordinary pickup or private storage.
            return StorageOpen(t) || p.zone=="garden" && Kitchen.Slot(t.container,out var g,out _) && (g=="fridge" || g=="cupboard" || g=="oven");
        }

        private string KitchenAssist(SoloCommand c,SoloPlayer p,SoloToy item)
        {
            if(state.schema<Kitchen.EasySchema || p.zone!="garden")return "kitchen-unavailable";
            var op=c.value.Substring(5);var experiment=op=="experiment" || op=="readyexperiment";
            if(experiment)op=op=="experiment"?"start":"readybase";
            if(op=="start" || op=="readybase")
            {
                var recipe=Kitchen.Recipe(c.target);if(recipe==null)return "unknown-recipe";
                // Selection and consumption happen in one authority operation.
                // Concurrent cooks cannot claim the same previously empty tray.
                if(item==null)item=state.toys.FirstOrDefault(t=>t.kind==ToyKind.Cookware && PrepAvailable(t,p) && t.kitchen.dish==null && !t.kitchen.dirty && (string.IsNullOrEmpty(t.kitchen.cook) || t.kitchen.cook==p.id));
                if(!PrepAvailable(item,p) || item.kind!=ToyKind.Cookware || item.kitchen.dish!=null || item.kitchen.dirty || !string.IsNullOrEmpty(item.kitchen.cook) && item.kitchen.cook!=p.id || item.kitchen.batch>=1000000000)return "need-empty-clean-tray";
                // A stored creation leaves an empty tray behind. Reusing a tray
                // still in the oven must begin on a free worktop, or its next
                // recipe would heat automatically as soon as preparation ends.
                var prepSlot=-1;
                if(state.schema>=PizzaFlow.Schema && Kitchen.Slot(item.container,out var oldPlace,out _) && (oldPlace=="oven" || oldPlace=="hob")){
                    prepSlot=Enumerable.Range(0,4).Where(i=>!state.toys.Any(t=>t!=item && t.container==Kitchen.Support("counter",i))).DefaultIfEmpty(-1).First();
                    if(prepSlot<0)return "storage-full";
                }
                var meal=state.schema>=MealFlow.Schema && recipe.id.StartsWith("MEAL-");
                var pizza=state.schema>=PizzaFlow.Schema && recipe.id.StartsWith("PIZ-");
                var family=state.schema>=CakeFamilies.Schema && CakeFamilies.Recipes.Contains(recipe.id);
                var staged=state.schema>=CakeFlow.Schema && recipe.id=="CAK-02";
                var ids=new[]{meal?MealFlow.Base(recipe.id):staged || family?"cake-mix":recipe.ingredient}.Concat(op=="readybase"?(meal?MealFlow.ReadyInputs(new FoodDish{recipe=recipe.id}):pizza?PizzaFlow.Inputs(new FoodDish{recipe=recipe.id}):family?CakeFamilies.Batter(recipe.id):staged?CakeFlow.Batter:recipe.toppings):Array.Empty<string>()).Distinct().ToArray();
                var stock=ids.Select(id=>state.toys.First(t=>t.id=="ingredient-"+id)).ToArray();
                if(stock.Any(t=>!PrepAvailable(t,p) || t.kitchen.amount==0 && t.kitchen.batch>=1000000000))return "ingredient-unavailable";
                // Choosing a recipe deliberately restocks an exhausted base in
                // place. Validate all sources before changing any counted stock.
                foreach(var t in stock)if(t.kitchen.amount==0){t.kitchen.batch++;t.kitchen.amount=16;}
                var additions=stock.Select(t=>{var a=TakeIngredient(t);a.by=p.id;return a;}).ToArray();
                var k=item.kitchen;k.batch++;k.cook=p.id;k.dish=new FoodDish{id=item.id+"/"+k.batch,recipe=recipe.id,portions=15,ingredients=additions,assisted=true};
                k.dish.guided=state.schema>=CakeFlow.Schema;k.dish.experiment=experiment;
                if(op=="readybase"){k.dish.heated=true;k.dish.heat=Kitchen.HeatSeconds;k.dish.step=Array.IndexOf(recipe.steps,"heat")+1;}
                if(staged)CakeFlow.Begin(k.dish,op=="readybase");
                if(family)CakeFamilies.Begin(k.dish,op=="readybase");
                if(pizza)PizzaFlow.Begin(k.dish,op=="readybase");
                if(meal)MealFlow.Begin(k.dish,op=="readybase");
                if(prepSlot>=0)PlacePrepared(item,"counter",prepSlot);
                Touch(item);return null;
            }
            if(!PrepAvailable(item,p) || item.kitchen==null)return "item-busy-or-closed";
            var dish=item.kitchen.dish;
            if(op=="taste"){
                if(dish==null || Kitchen.Next(dish)!="serve" || dish.portions==0)return "food-not-ready";
                dish.portions&=dish.portions-1;item.kitchen.dirty=true;Touch(item);return null;
            }
            if(op=="restock"){
                if(item.kind!=ToyKind.Ingredient || item.kitchen.amount!=0 || item.kitchen.batch>=1000000000)return "not-empty";
                item.kitchen.batch++;item.kitchen.amount=16;Touch(item);return null;
            }
            if(op=="add"){
                var tray=state.toys.FirstOrDefault(t=>t.id==c.target);var food=tray?.kitchen?.dish;
                if(item.kind!=ToyKind.Ingredient || item.kitchen.amount<1 || !PrepAvailable(tray,p) || tray.kind!=ToyKind.Cookware || food==null || food.portions!=15)return "need-ingredient-and-tray";
                if(!Kitchen.CanAdd(food,item.kitchen.definition))return "ingredient-not-in-stage";
                var missing=Kitchen.ReservedInputs(food,item.kitchen.definition);
                if(food.ingredients.Length>=24-missing)return "dish-full-or-served";
                var a=TakeIngredient(item,(c.x-tray.x)/100,(c.y-tray.y)/100);a.by=p.id;food.ingredients=food.ingredients.Concat(new[]{a}).ToArray();if(PreparationFlow.Active(food))PreparationFlow.Added(food,a);Touch(tray);return null;
            }
            if(op=="wash"){
                if(!Kitchen.Dish(item.kind) || dish!=null && dish.portions!=0)return "food-still-here";
                // A wash returns this empty dish to a free appropriate support;
                // it cannot clear a sibling's food or move another held object.
                var group=item.kind==ToyKind.Cookware?"counter":"dining";
                var slot=Enumerable.Range(0,4).Where(i=>!state.toys.Any(t=>t!=item && t.container==Kitchen.Support(group,i))).DefaultIfEmpty(-1).First();
                if(slot<0){group="sink";slot=Enumerable.Range(0,4).Where(i=>!state.toys.Any(t=>t!=item && t.container==Kitchen.Support(group,i))).DefaultIfEmpty(-1).First();}
                if(slot<0)return "storage-full";
                item.kitchen.dish=null;item.kitchen.dirty=false;item.kitchen.cook="";PlacePrepared(item,group,slot);return null;
            }
            if(op=="serve"){
                var plate=state.toys.FirstOrDefault(t=>t.id==c.target);
                if(item.kind!=ToyKind.Cookware || dish==null || Kitchen.Next(dish)!="serve" || dish.portions==0 || !PrepAvailable(plate,p) || plate.kind!=ToyKind.Plate || plate.kitchen.dish!=null || plate.kitchen.dirty)return "need-ready-food-and-clean-plate";
                var bit=dish.portions&-dish.portions;var portion=dish.Copy();portion.portions=bit;plate.kitchen.dish=portion;dish.portions&=~bit;item.kitchen.dirty=true;Touch(item);Touch(plate);return null;
            }
            if(dish==null || item.kind!=ToyKind.Cookware || dish.portions!=15)return "need-whole-dish";
            if(op=="undo"){
                if(PreparationFlow.Active(dish))return "preparation-already-incorporated";
                if(dish.heated || dish.heat>0 || dish.ingredients.Length<2 || dish.ingredients.Last().by!=p.id)return "nothing-to-undo";
                // Removing an unbaked addition discards that consumed portion.
                // Never rewind stock batches or refund already shared/served food.
                dish.ingredients=dish.ingredients.Take(dish.ingredients.Length-1).ToArray();Touch(item);return null;
            }
            if(op=="bake"){
                if(Kitchen.Next(dish)!="heat")return "next-step-changed";
                var group=MealFlow.Active(dish)?"hob":"oven";
                if(Kitchen.Slot(item.container,out var current,out _) && current==group){if(MealFlow.Active(dish)){MealFlow.StartHeat(dish);Touch(item);}return null;}
                var slot=Enumerable.Range(0,4).Where(i=>!state.toys.Any(t=>t.container==Kitchen.Support(group,i))).DefaultIfEmpty(-1).First();
                if(slot<0)return "storage-full";if(MealFlow.Active(dish))MealFlow.StartHeat(dish);PlacePrepared(item,group,slot);return null;
            }
            if(op!=Kitchen.Next(dish) || op=="heat" || op=="serve")return "next-step-changed";
            if(PreparationFlow.Active(dish))return "use-preparation-activity";
            var tool=Kitchen.Tool(op);
            if(!state.toys.Any(t=>t.kind==ToyKind.KitchenTool && t.kitchen.definition==tool && PrepAvailable(t,p)))return "tool-busy";
            dish.step++;Touch(item);return null;
        }

        private void PlacePrepared(SoloToy item,string group,int slot)
        {item.holder="";item.container=Kitchen.Support(group,slot);item.x=Kitchen.X(group,slot);item.y=Kitchen.Y(group,slot);Touch(item);}
    }
}
