using System;
using System.Linq;
using System.Collections.Generic;

namespace LittleWeeps.Core
{
    [Serializable] public sealed class KitchenState
    {
        public bool fridgeOpen,ovenOpen,waterOn;
        public bool[] cupboards=new bool[4];
        public KitchenState Copy()=>new KitchenState{fridgeOpen=fridgeOpen,ovenOpen=ovenOpen,waterOn=waterOn,cupboards=(bool[])cupboards.Clone()};
    }
    [Serializable] public sealed class FoodAddition
    {
        public string unit,ingredient;
        public float x,y;
        public FoodAddition Copy()=>(FoodAddition)MemberwiseClone();
    }
    [Serializable] public sealed class FoodDish
    {
        public string id,recipe;
        public int step,portions;
        public bool heated,assisted;
        public double heat;
        public FoodAddition[] ingredients=Array.Empty<FoodAddition>();
        public static bool Empty(FoodDish d)=>d==null || string.IsNullOrEmpty(d.id) && string.IsNullOrEmpty(d.recipe) && d.step==0 && d.portions==0 && !d.heated && !d.assisted && d.heat==0 && (d.ingredients==null || d.ingredients.Length==0);
        public FoodDish Copy(){var d=(FoodDish)MemberwiseClone();d.ingredients=ingredients.Select(i=>i.Copy()).ToArray();return d;}
    }
    [Serializable] public sealed class KitchenItem
    {
        public string definition;
        public int amount,batch;
        public bool dirty;
        public FoodDish dish;
        public static bool Empty(KitchenItem k)=>k==null || string.IsNullOrEmpty(k.definition) && k.amount==0 && k.batch==0 && !k.dirty && FoodDish.Empty(k.dish);
        public KitchenItem Copy(){var k=(KitchenItem)MemberwiseClone();k.dish=dish?.Copy();return k;}
    }
    public sealed class KitchenRecipe
    {
        public readonly string id,title,ingredient;
        public readonly string[] steps,toppings;
        public readonly int art;
        public KitchenRecipe(string id,string title,string ingredient,string steps,string toppings,int art)
        {this.id=id;this.title=title;this.ingredient=ingredient;this.steps=steps.Split(',');this.toppings=toppings.Split(',');this.art=art;}
    }
    public static class Kitchen
    {
        public const int Schema=13,StockCount=53;
        public const double HeatSeconds=8;
        public static readonly string[] Ingredients={"dough","sauce","cheese","pepperoni","capsicum","mushroom","tomato","ham","pineapple","flour","egg","milk","icing","chocolate","strawberry","sprinkles","carrot","bun","patty","lettuce","pasta","broth","rice","banana","popcorn"};
        public static readonly string[] Tools={"spoon","knife","roller","spatula"};
        public static readonly KitchenRecipe[] Recipes={
            new KitchenRecipe("PIZ-01","Cheese pizza","dough","shape,spread,heat,cut","sauce,cheese",0),
            new KitchenRecipe("PIZ-02","Pepperoni pizza","dough","shape,spread,heat,cut","sauce,cheese,pepperoni",1),
            new KitchenRecipe("PIZ-03","Vegetable pizza","dough","shape,chop,spread,heat,cut","capsicum,mushroom,tomato",2),
            new KitchenRecipe("PIZ-04","Ham & pineapple pizza","dough","shape,spread,heat,cut","ham,pineapple,cheese",3),
            new KitchenRecipe("PIZ-05","Silly-face pizza","dough","shape,spread,decorate,heat,cut","tomato,cheese,capsicum",4),
            new KitchenRecipe("CAK-01","Duck cake","flour","mix,shape-duck,heat,decorate,cut","egg,milk,icing,popcorn",5),
            new KitchenRecipe("CAK-02","Chocolate layer cake","flour","mix,heat,stack,ice,cut","egg,chocolate,icing",6),
            new KitchenRecipe("CAK-03","Strawberry heart cake","flour","pour,shape-heart,heat,ice,cut","milk,icing,strawberry",7),
            new KitchenRecipe("CAK-04","Rainbow cake","flour","mix,layer,heat,decorate,cut","milk,sprinkles,icing",8),
            new KitchenRecipe("CAK-05","Carrot cake","flour","chop,mix,heat,ice,cut","carrot,egg,icing",9),
            new KitchenRecipe("MEAL-01","Burger plate","bun","heat,stack,plate","patty,cheese,lettuce",10),
            new KitchenRecipe("MEAL-02","Spaghetti & sauce","pasta","pour,stir,heat,scoop","sauce,cheese",11),
            new KitchenRecipe("MEAL-03","Vegetable soup","broth","chop,pour,stir,heat,ladle","carrot,tomato,capsicum",12),
            new KitchenRecipe("MEAL-04","Pancake breakfast","flour","mix,pour,heat,flip,stack","egg,milk,banana",13),
            new KitchenRecipe("MEAL-05","Rice & vegetables","rice","chop,stir,heat,scoop","carrot,capsicum,mushroom",14)
        };
        public static KitchenRecipe Recipe(string id)=>Recipes.FirstOrDefault(r=>r.id==id);
        public static bool Kind(ToyKind kind)=>kind>=ToyKind.Ingredient && kind<=ToyKind.Plate;
        public static bool Dish(ToyKind kind)=>kind==ToyKind.Cookware || kind==ToyKind.Plate;
        public static string Next(FoodDish dish)
        {
            if(dish==null)return "choose";var recipe=Recipe(dish.recipe);var step=dish.step<recipe.steps.Length?recipe.steps[dish.step]:"serve";
            if(step=="heat"){var missing=recipe.toppings.FirstOrDefault(id=>!dish.ingredients.Any(i=>i.ingredient==id));if(missing!=null)return "add:"+missing;}
            return step;
        }
        public static string Tool(string step)=>step=="shape"?"roller":step=="chop" || step=="cut"?"knife":step=="flip" || step=="stack" || step=="plate" || step.StartsWith("shape-")?"spatula":"spoon";
        public static string Seat(int index)=>"kitchen-seat-"+index;
        public static bool Seat(string id)=>Enumerable.Range(0,4).Any(i=>Seat(i)==id);
        public static float SeatX(string id)=>-1020+Array.FindIndex(Enumerable.Range(0,4).Select(Seat).ToArray(),s=>s==id)*235;
        public static string Support(string group,int i)=>"kitchen/"+group+"/"+i;
        public static int Count(string group)=>group=="fridge"?12:group=="cupboard"?24:group=="tools"?16:group=="counter" || group=="oven" || group=="dining" || group=="sink"?4:0;
        public static bool Slot(string id,out string group,out int slot)
        {
            var parts=(id??"").Split('/');group=parts.Length==3?parts[1]:"";slot=-1;
            return parts.Length==3 && parts[0]=="kitchen" && int.TryParse(parts[2],out slot) && slot>=0 && slot<Count(group);
        }
        public static float X(string group,int i)=>group=="fridge"?-1050+(i%3)*72:group=="cupboard"?-2270+(i%12)*86:group=="tools"?-2230+i*62:group=="counter"?-2040+i*230:group=="oven"?-610+(i%2)*95:group=="dining"?-1020+i*235:-2270+i*44;
        public static float Y(string group,int i)=>group=="fridge"?450-(i/3)*100:group=="cupboard"?400-(i/12)*180:group=="counter"?430:group=="oven"?340-(i/2)*160:group=="dining"?180:430;
        public static bool Open(KitchenState state,string group,int slot)=>state!=null && (group=="fridge"?state.fridgeOpen:group=="cupboard"?state.cupboards[(slot%12)/3]:group=="oven"?state.ovenOpen:true);
        private static SoloToy[] CreateStock()
        {
            var list=new List<SoloToy>();
            void Add(string id,ToyKind kind,string definition,string group,int slot,int amount=0)=>list.Add(new SoloToy{id=id,kind=kind,x=X(group,slot),y=Y(group,slot),container=Support(group,slot),kitchen=new KitchenItem{definition=definition,amount=amount}});
            for(var i=0;i<Ingredients.Length;i++)Add("ingredient-"+Ingredients[i],ToyKind.Ingredient,Ingredients[i],i<12?"fridge":"cupboard",i<12?i:i-12,16);
            // Four exclusive sets hang on a persistent rack above the worktops.
            for(var i=0;i<4;i++){
                Add("cookware-"+i,ToyKind.Cookware,"tray","counter",i);
                Add("plate-"+i,ToyKind.Plate,"plate","dining",i);
                Add("plate-"+(i+4),ToyKind.Plate,"plate","cupboard",13+i);
                for(var j=0;j<4;j++)Add("utensil-"+Tools[j]+"-"+i,ToyKind.KitchenTool,Tools[j],"tools",i*4+j);
            }
            return list.ToArray();
        }
        private static readonly SoloToy[] originals=CreateStock();
        public static SoloToy[] Stock()=>originals.Select(t=>t.Copy()).ToArray();
        public static bool Identity(SoloToy t)=>originals.Any(v=>v.id==t.id && v.kind==t.kind);
    }
    public sealed partial class SoloWorld
    {
        public KitchenState ReadKitchen()=>state.kitchen?.Copy();
        public static SoloWorld WithKitchen(SoloWorld world)
        {
            world=WithRoomPlay(world);if(world.Schema>=Kitchen.Schema)return world;
            var s=world.Snapshot();s.schema=Kitchen.Schema;s.kitchen=new KitchenState();s.toys=s.toys.Concat(Kitchen.Stock()).ToArray();s.revision++;
            Validate(s);return new SoloWorld(s);
        }
        // Unity JsonUtility expands null inline classes into empty records.
        // Canonicalize only wholly empty records; nonempty malformed state still
        // reaches validation and is refused rather than silently repaired.
        private static void NormalizeKitchenInline(SoloSnapshot s)
        {
            if(s==null)return;
            if(s.schema<Kitchen.Schema && s.kitchen!=null && !s.kitchen.fridgeOpen && !s.kitchen.ovenOpen && !s.kitchen.waterOn && (s.kitchen.cupboards==null || s.kitchen.cupboards.All(v=>!v)))s.kitchen=null;
            foreach(var t in s.toys??Array.Empty<SoloToy>()){if(t==null)continue;if(KitchenItem.Empty(t.kitchen))t.kitchen=null;else if(FoodDish.Empty(t.kitchen.dish))t.kitchen.dish=null;}
        }
        private static void ValidateKitchen(SoloSnapshot s)
        {
            if(s.schema<Kitchen.Schema){if(s.kitchen!=null || s.toys.Any(t=>t.kitchen!=null || Kitchen.Kind(t.kind)))throw new InvalidOperationException("Kitchen requires schema 13.");return;}
            if(s.kitchen==null || s.kitchen.cupboards==null || s.kitchen.cupboards.Length!=4)throw new InvalidOperationException("Invalid kitchen fixtures.");
            foreach(var original in Kitchen.Stock())
            {
                var t=s.toys.SingleOrDefault(v=>v.id==original.id);var k=t?.kitchen;
                if(t==null || t.kind!=original.kind || k==null || k.definition!=original.kitchen.definition || k.batch<0 || k.batch>1000000000 || k.amount<0 || k.amount>16 ||
                   (t.kind!=ToyKind.Ingredient && k.amount!=0) || (t.kind==ToyKind.Ingredient && k.dirty) || t.personalRoom!="" || t.water!=0 || t.wet || t.resetPending)
                    throw new InvalidOperationException("Invalid kitchen inventory.");
            }
            foreach(var t in s.toys)
            {
                if(!Kitchen.Kind(t.kind)){if(t.kitchen!=null)throw new InvalidOperationException("Unexpected food state.");continue;}
                var d=t.kitchen.dish;if(d==null)continue;var r=Kitchen.Recipe(d.recipe);
                if(!Kitchen.Dish(t.kind) || !Id(d.id) || r==null || d.step<0 || d.step>r.steps.Length || d.portions<0 || d.portions>15 ||
                   double.IsNaN(d.heat) || double.IsInfinity(d.heat) || d.heat<0 || d.heat>Kitchen.HeatSeconds || d.ingredients==null || d.ingredients.Length<1 || d.ingredients.Length>24 ||
                   d.ingredients.Any(i=>i==null || !Id(i.unit) || !Kitchen.Ingredients.Contains(i.ingredient) || float.IsNaN(i.x) || float.IsNaN(i.y) || Math.Abs(i.x)>1 || Math.Abs(i.y)>1) ||
                   d.ingredients.Select(i=>i.unit).Distinct().Count()!=d.ingredients.Length || t.kind==ToyKind.Plate && Kitchen.Next(d)!="serve")throw new InvalidOperationException("Invalid persistent dish.");
            }
            // Portions may live on different plates, but their origin/bit is unique.
            var portions=new HashSet<string>();
            foreach(var d in s.toys.Where(t=>t.kitchen?.dish!=null).Select(t=>t.kitchen.dish))for(var i=0;i<4;i++)
                if((d.portions&(1<<i))!=0 && !portions.Add(d.id+"/"+i))throw new InvalidOperationException("Duplicated food portion.");
        }
        private bool KitchenAvailable(SoloToy t,SoloPlayer p)=>t!=null && t.zone==p.zone && (t.holder=="" || t.holder==p.id) && StorageOpen(t);
        private void ReturnKitchenTool(SoloToy item,SoloPlayer player)
        {
            var origin=Kitchen.Stock().First(t=>t.id==item.id);item.holder="";
            if(player.zone=="garden" && !state.toys.Any(t=>t.container==origin.container)){item.container=origin.container;item.x=origin.x;item.y=origin.y;}
            else {item.container="";item.x=player.x;item.y=Math.Max(35,player.y-65);}
            Touch(item);
        }
        private FoodAddition TakeIngredient(SoloToy ingredient,float x=0,float y=0)
        {
            var k=ingredient.kitchen;var unit=ingredient.id+"/"+k.batch+"/"+(16-k.amount);k.amount--;Touch(ingredient);
            return new FoodAddition{unit=unit,ingredient=k.definition,x=Math.Max(-1,Math.Min(1,x)),y=Math.Max(-1,Math.Min(1,y))};
        }
        private string KitchenOperation(SoloCommand c,SoloPlayer p,SoloToy item)
        {
            if(state.schema<Kitchen.Schema)return "kitchen-unavailable";
            if(c.value=="door"){
                if(p.zone!="garden")return "wrong-area";
                if(c.target=="fridge")state.kitchen.fridgeOpen=!state.kitchen.fridgeOpen;
                else if(c.target=="oven")state.kitchen.ovenOpen=!state.kitchen.ovenOpen;
                else if(c.target=="water")state.kitchen.waterOn=!state.kitchen.waterOn;
                else if(int.TryParse(c.target,out var cupboard) && cupboard>=0 && cupboard<4)state.kitchen.cupboards[cupboard]=!state.kitchen.cupboards[cupboard];else return "invalid-fixture";
                return null;
            }
            if(!KitchenAvailable(item,p) || item.kitchen==null)return "item-busy-or-closed";
            var k=item.kitchen;var d=k.dish;
            if(c.value=="restock"){
                if(item.kind!=ToyKind.Ingredient || k.amount!=0 || k.batch>=1000000000)return "not-empty";
                k.batch++;k.amount=16;return null;
            }
            if(c.value=="start" || c.value=="readybase"){
                if(p.zone!="garden" || item.kind!=ToyKind.Cookware || d!=null || k.dirty || k.batch>=1000000000)return "need-empty-clean-tray";
                var r=Kitchen.Recipe(c.target);if(r==null)return "unknown-recipe";
                var source=state.toys.First(t=>t.id=="ingredient-"+r.ingredient);
                if(!KitchenAvailable(source,p) || source.kitchen.amount<1)return "open-ingredient-storage";
                var extras=c.value=="readybase"?r.toppings.Select(id=>state.toys.First(t=>t.id=="ingredient-"+id)).ToArray():Array.Empty<SoloToy>();
                if(extras.Any(t=>!KitchenAvailable(t,p) || t.kitchen.amount<1))return "open-ingredient-storage";
                var addition=TakeIngredient(source);k.batch++;
                k.dish=new FoodDish{id=item.id+"/"+k.batch,recipe=r.id,portions=15,ingredients=new[]{addition}};
                if(c.value=="readybase"){k.dish.ingredients=k.dish.ingredients.Concat(extras.Select(t=>TakeIngredient(t))).ToArray();k.dish.assisted=true;k.dish.heated=true;k.dish.heat=Kitchen.HeatSeconds;k.dish.step=Array.IndexOf(r.steps,"heat")+1;}
                return null;
            }
            if(c.value=="add"){
                var target=state.toys.FirstOrDefault(t=>t.id==c.target);
                if(item.kind!=ToyKind.Ingredient || k.amount<1 || !KitchenAvailable(target,p) || target.kind!=ToyKind.Cookware || target.kitchen.dish==null)return "need-ingredient-and-tray";
                var food=target.kitchen.dish;var missing=Kitchen.Recipe(food.recipe).toppings.Count(id=>id!=k.definition && !food.ingredients.Any(i=>i.ingredient==id));if(food.ingredients.Length>=24-missing || food.portions!=15)return "dish-full-or-served";
                // Decorations use local coordinates; the command's board point is
                // mapped into a bounded surface, independent of camera or avatar.
                var add=TakeIngredient(item,(c.x-target.x)/100,(c.y-target.y)/100);food.ingredients=food.ingredients.Concat(new[]{add}).ToArray();return null;
            }
            if(c.value=="taste"){
                if(d==null || Kitchen.Next(d)!="serve" || d.portions==0)return "food-not-ready";
                d.portions&=d.portions-1;k.dirty=true;return null;
            }
            if(c.value=="wash"){
                if(p.zone!="garden" || !state.kitchen.waterOn || !Kitchen.Slot(item.container,out var group,out _) || group!="sink")return "use-running-sink";
                if(d!=null && d.portions!=0)return "food-still-here";
                k.dish=null;k.dirty=false;return null;
            }
            if(c.value=="serve"){
                var plate=state.toys.FirstOrDefault(t=>t.id==c.target);
                if(d==null || Kitchen.Next(d)!="serve" || d.portions==0 || !KitchenAvailable(plate,p) || plate.kind!=ToyKind.Plate || plate.kitchen.dish!=null || plate.kitchen.dirty)return "need-ready-food-and-clean-plate";
                var bit=d.portions&-d.portions;var portion=d.Copy();portion.portions=bit;plate.kitchen.dish=portion;d.portions&=~bit;k.dirty=true;return null;
            }
            if(d==null || d.portions!=15 || item.kind!=ToyKind.Cookware || p.zone!="garden")return "need-whole-dish";
            if(c.value!=Kitchen.Next(d) || c.value=="heat" || c.value=="serve")return "next-step-changed";
            var tool=Kitchen.Tool(c.value);
            if(!state.toys.Any(t=>t.kind==ToyKind.KitchenTool && t.kitchen.definition==tool && KitchenAvailable(t,p)))return "tool-busy";
            d.step++;return null;
        }
        private string StoreKitchen(SoloCommand c,SoloPlayer p,SoloToy item,string group,int slot)
        {
            if(state.schema<Kitchen.Schema || p.zone!="garden" || !Kitchen.Open(state.kitchen,group,slot))return "storage-closed";
            if(state.toys.Any(t=>t.container==c.target))return "storage-full";
            if((group=="oven" && item.kind!=ToyKind.Cookware) || (group=="tools" && item.kind!=ToyKind.KitchenTool) || (group=="dining" && item.kind!=ToyKind.Plate) || (group=="sink" && !Kitchen.Kind(item.kind)))return "wrong-support";
            if(Math.Abs(c.x-Kitchen.X(group,slot))>80 || Math.Abs(c.y-Kitchen.Y(group,slot))>80)return "target-too-far";
            item.container=c.target;item.holder="";item.x=Kitchen.X(group,slot);item.y=Kitchen.Y(group,slot);Touch(item);return null;
        }
        private bool AdvanceKitchen(double seconds,out bool visible)
        {
            visible=false;if(state.kitchen==null)return false;var changed=false;
            foreach(var t in state.toys.Where(t=>t.kitchen?.dish!=null)){
                var d=t.kitchen.dish;if(Kitchen.Next(d)!="heat" || !Kitchen.Slot(t.container,out var group,out _) || group!="oven")continue;
                d.heat=Math.Min(Kitchen.HeatSeconds,d.heat+seconds);changed=true;
                if(d.heat>=Kitchen.HeatSeconds){d.heated=true;d.step++;visible=true;}
            }
            return changed;
        }
    }
}
