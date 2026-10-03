using System;
using System.Linq;

namespace LittleWeeps.Core
{
    // Version 2 reuses the existing food record. 'step' is recipe-specific:
    // carrot preparation bits, duck icing features, or three base-7 color choices.
    // Legacy version 0/1 dishes keep their exact original preparation contract.
    public static class CakeFamilies
    {
        public const int Schema=24, Version=2;
        public static readonly string[] Recipes={"CAK-01","CAK-03","CAK-04","CAK-05"};
        private static readonly string[] Duck={"ingredients","mix","pour","bake","stack","ice","features","decorate","cut","serve"};
        private static readonly string[] Heart={"ingredients","mix","pour","bake","ice","decorate","cut","serve"};
        private static readonly string[] Rainbow={"ingredients","mix","colors","pour","bake","stack","ice","decorate","cut","serve"};
        private static readonly string[] Carrot={"ingredients","chop","mix","pour","bake","ice","decorate","cut","serve"};
        public static bool Active(FoodDish d)=>d!=null && d.recipeVersion==Version;
        public static string[] Stages(FoodDish d)=>d.recipe=="CAK-01"?Duck:d.recipe=="CAK-03"?Heart:d.recipe=="CAK-04"?Rainbow:Carrot;
        public static int Index(FoodDish d)=>Array.IndexOf(Stages(d),d.stage);
        public static int LayerCount(FoodDish d)=>d.recipe=="CAK-01"?2:d.recipe=="CAK-04"?3:1;
        public static string[] Batter(string recipe)=>recipe=="CAK-05"?new[]{"egg","milk","carrot"}:new[]{"egg","milk"};
        public static string[] Decorations(FoodDish d)=>d.recipe=="CAK-01"?new[]{"popcorn","sprinkles"}:d.recipe=="CAK-03"?new[]{"strawberry","sprinkles"}:d.recipe=="CAK-05"?new[]{"carrot","icing"}:new[]{"sprinkles","strawberry"};
        public static string[] Palette(FoodDish d)
        {
            if(d.experiment && (d.stage=="ingredients" || d.stage=="decorate"))return Kitchen.Ingredients;
            if(d.stage=="ingredients")return Batter(d.recipe).Where(id=>!d.ingredients.Any(a=>a.ingredient==id)).ToArray();
            if(d.stage=="ice" && !d.ingredients.Any(a=>a.ingredient=="icing"))return new[]{"icing"};
            return d.stage=="decorate"?Decorations(d):Array.Empty<string>();
        }
        public static int[] Colors(FoodDish d)
        {
            if(d.recipe!="CAK-04")return Array.Empty<int>();
            var n=d.step;var a=new System.Collections.Generic.List<int>();
            while(n>0 && a.Count<4){a.Insert(0,n%7-1);n/=7;}return a.ToArray();
        }
        public static string Token(FoodDish d)=>d.id+"@"+d.stage+(d.stage=="stack"?"@"+d.layers:d.stage=="colors"?"@"+d.step:"");
        public static void Begin(FoodDish d,bool ready)
        {
            d.recipeVersion=Version;d.step=ready && d.recipe=="CAK-05"?3:ready && d.recipe=="CAK-04"?94:0;
            d.stage=ready?(LayerCount(d)>1?"stack":"ice"):"ingredients";
            d.heated=ready;d.mixed=d.poured=ready?1:0;d.heat=ready?Kitchen.HeatSeconds:0;d.layers=ready?1:0;
        }
        public static void Added(FoodDish d,FoodAddition a)
        {
            a.phase=d.stage;
            if(d.stage=="ingredients" && Batter(d.recipe).All(id=>d.ingredients.Any(i=>i.ingredient==id)))d.stage=d.recipe=="CAK-05"?"chop":"mix";
        }
        public static bool Complete(FoodDish d)=>d.stage=="chop"?d.step==3:d.stage=="colors"?Colors(d).Length==3:d.stage=="features"?d.step==3:
            d.stage=="mix"?d.mixed==1:d.stage=="pour"?d.poured==1:d.stage=="stack"?d.layers==LayerCount(d):d.stage=="ice"?d.icingMask==511:d.stage=="cut"?d.cutMask==3:d.stage=="decorate";
        public static void Finish(FoodDish d)=>d.stage=Stages(d)[Index(d)+1];
        public static void Heated(FoodDish d){d.layers=1;Finish(d);}
        public static bool Valid(FoodDish d,int schema)
        {
            if(schema<Schema || !Recipes.Contains(d.recipe) || d.ingredients==null || d.ingredients.Any(a=>a==null) || Index(d)<0 || !Unit(d.mixed) || !Unit(d.poured) || d.layers<0 || d.layers>LayerCount(d) || d.icingMask<0 || d.icingMask>511 || d.cutMask<0 || d.cutMask>3)return false;
            var stages=Stages(d);var at=Index(d);bool Past(string stage)=>at>Array.IndexOf(stages,stage);
            var hasBatter=Batter(d.recipe).All(id=>d.ingredients.Any(a=>a.ingredient==id));
            if(!d.ingredients.Any(a=>a.ingredient=="cake-mix") || (at==0?hasBatter:!hasBatter) || Past("mix") && d.mixed!=1 || Past("pour") && d.poured!=1 || Past("bake")!=d.heated || !Past("pour") && d.heat!=0 || Past("bake") && d.heat!=Kitchen.HeatSeconds || (!Past("bake") && d.layers!=0) || Past("bake") && d.layers<1)return false;
            if(Past("ice") && d.icingMask!=511 || d.icingMask>0 && !d.ingredients.Any(a=>a.ingredient=="icing") || Past("cut") && d.cutMask!=3)return false;
            if((d.recipe=="CAK-01" || d.recipe=="CAK-04") && Past("stack") && d.layers!=LayerCount(d))return false;
            if(d.recipe=="CAK-01")return d.step>=0 && d.step<=3 && (!Past("features") || d.step==3) && (at>=Array.IndexOf(stages,"features") || d.step==0);
            if(d.recipe=="CAK-03")return d.step==0;
            if(d.recipe=="CAK-05")return d.step>=0 && d.step<=3 && (!Past("chop") || d.step==3) && (at>=1 || d.step==0);
            var colors=Colors(d);return d.step>=0 && colors.Length<=3 && colors.All(n=>n>=0 && n<6) && (!Past("colors") || colors.Length==3) && (at>=Array.IndexOf(stages,"colors") || d.step==0);
        }
        private static bool Unit(float n)=>!float.IsNaN(n) && !float.IsInfinity(n) && n>=0 && n<=1;
    }

    public sealed partial class GameWorld
    {
        public static GameWorld WithCakeFamilies(GameWorld world)
        {
            world=WithHomeCreations(world);if(world.Schema>=CakeFamilies.Schema)return world;
            var s=world.Snapshot();s.schema=CakeFamilies.Schema;s.revision++;Validate(s);return new GameWorld(s);
        }
    }
}
