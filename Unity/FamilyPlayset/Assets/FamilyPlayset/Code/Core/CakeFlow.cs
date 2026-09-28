using System;
using System.Linq;

namespace LittleWeeps.Core
{
    // Versioned preparation belongs to the food, not an open screen or avatar.
    public static class CakeFlow
    {
        public const int Schema=15, Version=1, FullCoverage=511;
        public static readonly string[] Stages={"ingredients","mix","pour","bake","filling","stack","ice","decorate","cut","serve"};
        public static readonly string[] Batter={"egg","milk","chocolate"};
        public static bool Active(FoodDish d)=>d!=null && (d.recipeVersion==Version || CakeFamilies.Active(d));
        public static string Token(FoodDish d)=>CakeFamilies.Active(d)?CakeFamilies.Token(d):d.id+"@"+d.stage;
        public static int Index(FoodDish d)=>CakeFamilies.Active(d)?CakeFamilies.Index(d):Array.IndexOf(Stages,d.stage);
        public static string[] StageList(FoodDish d)=>CakeFamilies.Active(d)?CakeFamilies.Stages(d):Stages;
        public static string Next(FoodDish d)=>d.stage=="ingredients"?"add:"+(CakeFamilies.Active(d)?CakeFamilies.Batter(d.recipe):Batter).First(id=>!d.ingredients.Any(a=>a.ingredient==id)):d.stage=="bake"?"heat":d.stage;
        public static string[] Palette(FoodDish d)
        {
            if(CakeFamilies.Active(d))return CakeFamilies.Palette(d);
            if(d.experiment && (d.stage=="ingredients" || d.stage=="decorate"))return Kitchen.Ingredients;
            if(d.stage=="ingredients")return Batter.Where(id=>!d.ingredients.Any(a=>a.ingredient==id)).ToArray();
            if(d.stage=="filling" && !d.ingredients.Any(a=>a.ingredient=="icing"))return new[]{"icing"};
            return d.stage=="decorate"?new[]{"chocolate","strawberry","sprinkles"}:Array.Empty<string>();
        }
        public static bool CanAdd(FoodDish d,string ingredient)=>Palette(d).Contains(ingredient);
        public static bool HasBatter(FoodDish d)=>Batter.All(id=>d.ingredients.Any(a=>a.ingredient==id));
        public static void Begin(FoodDish d,bool ready)
        {
            d.recipeVersion=Version;d.stage=ready?"filling":"ingredients";
            d.mixed=ready?1:0;d.poured=ready?1:0;d.layers=ready?1:0;
            d.heated=ready;d.heat=ready?Kitchen.HeatSeconds:0;
        }
        public static void Added(FoodDish d,FoodAddition addition)
        {
            if(CakeFamilies.Active(d)){CakeFamilies.Added(d,addition);return;}
            addition.phase=d.stage;
            if(d.stage=="ingredients" && HasBatter(d))d.stage="mix";
        }
        public static bool Complete(FoodDish d)=>CakeFamilies.Active(d)?CakeFamilies.Complete(d):d.stage=="mix"?d.mixed==1:d.stage=="pour"?d.poured==1:
            d.stage=="filling" || d.stage=="ice"?d.icingMask==FullCoverage:d.stage=="stack"?d.layers==2:d.stage=="cut"?d.cutMask==3:d.stage=="decorate";
        public static void Finish(FoodDish d)
        {
            if(CakeFamilies.Active(d)){CakeFamilies.Finish(d);return;}
            var prior=d.stage;d.stage=Stages[Index(d)+1];
            if(prior=="stack")d.icingMask=0;
        }
        public static SoloToy MixStock()=>new SoloToy{id="ingredient-cake-mix",kind=ToyKind.Ingredient,x=Kitchen.X("cupboard",17),y=Kitchen.Y("cupboard",17),container=Kitchen.Support("cupboard",17),kitchen=new KitchenItem{definition="cake-mix",amount=16}};
        public static bool Valid(FoodDish d,int schema)
        {
            if(CakeFamilies.Active(d))return CakeFamilies.Valid(d,schema);
            if(d.recipeVersion==0)return string.IsNullOrEmpty(d.stage) && d.mixed==0 && d.poured==0 && d.layers==0 && d.icingMask==0 && d.cutMask==0;
            if(schema<Schema || !Active(d) || d.recipe!="CAK-02" || Index(d)<0 || !FiniteUnit(d.mixed) || !FiniteUnit(d.poured) || d.layers<0 || d.layers>2 || d.icingMask<0 || d.icingMask>FullCoverage || d.cutMask<0 || d.cutMask>3)return false;
            var i=Index(d);
            return (i<2 || d.mixed==1) && (i<3 || d.poured==1) && (i<4 || d.heated && d.layers>=1) &&
                (i<6 || d.layers==2) && (i<7 || d.icingMask==FullCoverage) && (i<9 || d.cutMask==3) &&
                (d.stage!="ingredients" || !HasBatter(d)) && (i==0 || HasBatter(d)) && (i>=4 || !d.heated) && (i>=3 || d.heat==0);
        }
        private static bool FiniteUnit(float n)=>!float.IsNaN(n) && !float.IsInfinity(n) && n>=0 && n<=1;
    }

    public sealed partial class SoloWorld
    {
        public static SoloWorld WithCakeFlow(SoloWorld world)
        {
            world=WithFriendlyKitchen(world);if(world.Schema>=CakeFlow.Schema)return world;
            var s=world.Snapshot();s.schema=CakeFlow.Schema;s.toys=s.toys.Concat(new[]{CakeFlow.MixStock()}).ToArray();
            // Legacy dishes retain their original recipe version and exact content.
            // They can finish using the old rules; only newly chosen chocolate cakes
            // use the new process. Never charge old batter twice or erase odd toppings.
            s.revision++;Validate(s);return new SoloWorld(s);
        }
        private string CakeOperation(SoloCommand c,SoloPlayer p,SoloToy item)
        {
            var d=item?.kitchen?.dish;
            if(state.schema<CakeFlow.Schema || p.zone!="garden" || !PrepAvailable(item,p) || item.kind!=ToyKind.Cookware || !CakeFlow.Active(d) || d.portions!=15)return "need-whole-dish";
            if(c.target!=CakeFlow.Token(d))return "next-step-changed";
            var op=c.value.Substring(5);
            if(op=="finish"){
                if(!CakeFlow.Complete(d))return "preparation-incomplete";
                CakeFlow.Finish(d);Touch(item);return null;
            }
            if(op!=d.stage)return "next-step-changed";
            if(CakeFamilies.Active(d) && (op=="chop" || op=="features")){
                if(c.x!=0 && c.x!=1)return "invalid-preparation";d.step|=1<<(int)c.x;
            }else if(CakeFamilies.Active(d) && op=="colors"){
                if(c.x<0 || c.x>5 || c.x!=(int)c.x || CakeFamilies.Colors(d).Length>=3)return "invalid-preparation";
                d.step=d.step*7+(int)c.x+1;
                if(CakeFamilies.Complete(d))CakeFamilies.Finish(d);
            }else if(op=="mix" || op=="pour"){
                if(c.x<=0 || c.x>.2f || float.IsNaN(c.x) || float.IsInfinity(c.x))return "invalid-preparation";
                if(op=="mix")d.mixed=Math.Min(1,d.mixed+c.x);else d.poured=Math.Min(1,d.poured+c.x);
            }else if(op=="filling" || op=="ice"){
                if(!d.ingredients.Any(a=>a.ingredient=="icing"))return "need-icing";
                var y=c.y-2; // command coordinates retain the world's nonnegative Y envelope
                if(!CakePoint(c.x,y))return "invalid-preparation";
                var column=Math.Min(2,(int)((c.x+1)*1.5f));var row=Math.Min(2,(int)((y+1)*1.5f));d.icingMask|=1<<(row*3+column);
            }else if(op=="stack"){
                if(!CakePoint(c.x,c.y-2) || Math.Abs(c.x)>.8f || Math.Abs(c.y-2)>.8f)return "place-layer-on-cake";
                d.layers=CakeFamilies.Active(d)?Math.Min(CakeFamilies.LayerCount(d),d.layers+1):2;
                if(CakeFamilies.Active(d) && CakeFamilies.Complete(d))CakeFamilies.Finish(d);
            }else if(op=="cut"){
                if(c.x!=0 && c.x!=1)return "invalid-preparation";d.cutMask|=1<<(int)c.x;
            }else return "next-step-changed";
            Touch(item);return null;
        }
        private static bool CakePoint(float x,float y)=>!float.IsNaN(x) && !float.IsInfinity(x) && !float.IsNaN(y) && !float.IsInfinity(y) && Math.Abs(x)<=1 && Math.Abs(y)<=1;
    }
}
