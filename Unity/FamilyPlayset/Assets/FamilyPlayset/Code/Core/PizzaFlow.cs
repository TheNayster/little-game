using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public static class PizzaFlow
    {
        public const int Schema=25,Version=3;
        private static readonly string[] Plain={"knead","roll","sauce","spread","toppings","bake","cut","serve"};
        private static readonly string[] Vegetable={"knead","roll","vegetables","chop","sauce","spread","toppings","bake","cut","serve"};
        public static readonly string[] Vegetables={"capsicum","mushroom","tomato"};
        public static bool Active(FoodDish d)=>d!=null && d.recipeVersion==Version;
        public static string[] Stages(FoodDish d)=>d.recipe=="PIZ-03"?Vegetable:Plain;
        public static string[] Toppings(FoodDish d)=>d.recipe=="PIZ-01"?new[]{"cheese"}:d.recipe=="PIZ-02"?new[]{"cheese","pepperoni"}:d.recipe=="PIZ-03"?Vegetables:d.recipe=="PIZ-04"?new[]{"cheese","ham","pineapple"}:new[]{"cheese","tomato","capsicum"};
        public static string[] Inputs(FoodDish d)=>new[]{"sauce"}.Concat(Toppings(d)).ToArray();
        public static bool Has(FoodDish d,string ingredient)=>d.ingredients.Any(a=>a.ingredient==ingredient);
        public static string[] Missing(FoodDish d)=>Toppings(d).Where(id=>!Has(d,id)).ToArray();
        public static string[] Palette(FoodDish d)
        {
            if(d.stage=="sauce")return new[]{"sauce"};
            if(d.stage=="vegetables")return Vegetables.Where(id=>!Has(d,id)).ToArray();
            if(d.stage!="toppings")return Array.Empty<string>();
            return d.experiment?Kitchen.Ingredients:Toppings(d).Concat(d.recipe=="PIZ-03"?new[]{"cheese"}:Array.Empty<string>()).ToArray();
        }
        public static void Begin(FoodDish d,bool ready)
        {
            d.recipeVersion=Version;d.stage=ready?"cut":"knead";d.step=ready && d.recipe=="PIZ-03"?3:0;
            d.mixed=d.poured=ready?1:0;d.heated=ready;d.heat=ready?Kitchen.HeatSeconds:0;d.icingMask=ready?511:0;
        }
        public static void Added(FoodDish d,FoodAddition a)
        {
            a.phase=d.stage;
            if(d.stage=="sauce")d.stage="spread";
            if(d.stage=="vegetables" && Vegetables.All(id=>Has(d,id)))d.stage="chop";
        }
        public static bool Complete(FoodDish d)=>d.stage=="knead"?d.mixed==1:d.stage=="roll"?d.poured==1:d.stage=="chop"?d.step==3:d.stage=="spread"?d.icingMask==511:d.stage=="toppings"?Missing(d).Length==0:d.stage=="cut"?d.cutMask==3:false;
        public static void Finish(FoodDish d)=>d.stage=Stages(d)[Array.IndexOf(Stages(d),d.stage)+1];
        public static bool Valid(FoodDish d,int schema)
        {
            if(schema<Schema || !Kitchen.Recipes.Take(5).Any(r=>r.id==d.recipe) || d.ingredients==null || d.ingredients.Any(a=>a==null) || !Unit(d.mixed) || !Unit(d.poured) || d.layers!=0 || d.icingMask<0 || d.icingMask>511 || d.cutMask<0 || d.cutMask>3 || d.step<0 || d.step>3 || d.recipe!="PIZ-03" && d.step!=0)return false;
            var stages=Stages(d);var at=Array.IndexOf(stages,d.stage);if(at<0 || !Has(d,"dough"))return false;
            bool Past(string stage)=>at>Array.IndexOf(stages,stage);
            if(Past("knead") && d.mixed!=1 || Past("roll") && d.poured!=1 || Past("sauce") && !Has(d,"sauce") || Past("spread") && d.icingMask!=511 || Past("toppings") && Missing(d).Length>0 || Past("bake")!=d.heated || !Past("toppings") && d.heat!=0 || Past("bake") && d.heat!=Kitchen.HeatSeconds || Past("cut") && d.cutMask!=3)return false;
            return d.recipe!="PIZ-03" || ((!Past("vegetables") || Vegetables.All(id=>Has(d,id))) && (!Past("chop") || d.step==3));
        }
        private static bool Unit(float n)=>!float.IsNaN(n) && !float.IsInfinity(n) && n>=0 && n<=1;
    }
    public sealed partial class SoloWorld
    {
        public static SoloWorld WithPizzaPreparation(SoloWorld world)
        {
            world=WithCakeFamilies(world);if(world.Schema>=PizzaFlow.Schema)return world;
            var s=world.Snapshot();s.schema=PizzaFlow.Schema;s.revision++;Validate(s);return new SoloWorld(s);
        }
        private string PizzaOperation(SoloCommand c,SoloPlayer p,SoloToy item)
        {
            var d=item?.kitchen?.dish;
            if(state.schema<PizzaFlow.Schema || p.zone!="garden" || !PrepAvailable(item,p) || item.kind!=ToyKind.Cookware || !PizzaFlow.Active(d) || d.portions!=15)return "need-whole-dish";
            if(c.target!=PreparationFlow.Token(d))return "next-step-changed";var op=c.value.Substring(8);
            if(op=="finish"){if(!PizzaFlow.Complete(d))return "preparation-incomplete";PizzaFlow.Finish(d);Touch(item);return null;}
            if(op!=d.stage)return "next-step-changed";
            if(op=="knead" || op=="roll"){
                if(c.x<=0 || c.x>.2f || float.IsNaN(c.x) || float.IsInfinity(c.x))return "invalid-preparation";
                if(op=="knead")d.mixed=Math.Min(1,d.mixed+c.x);else d.poured=Math.Min(1,d.poured+c.x);
            }else if(op=="spread"){
                var y=c.y-2;if(!CakePoint(c.x,y) || !PizzaFlow.Has(d,"sauce"))return "invalid-preparation";
                var column=Math.Min(2,(int)((c.x+1)*1.5f));var row=Math.Min(2,(int)((y+1)*1.5f));d.icingMask|=1<<(row*3+column);
            }else if(op=="chop" || op=="cut"){
                if(c.x!=0 && c.x!=1)return "invalid-preparation";if(op=="chop")d.step|=1<<(int)c.x;else d.cutMask|=1<<(int)c.x;
            }else return "next-step-changed";
            Touch(item);return null;
        }
    }
}
