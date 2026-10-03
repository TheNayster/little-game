using System;
using System.IO;
using System.Linq;

namespace LittleWeeps.Core
{
    public sealed class MealProgress
    {
        public float mix,pour,water,stir,drain,coat,plate,combine;
        public int chop,flip,stack,heat,started;
        public float Amount(string phase)=>phase=="pan" || phase=="pour"?pour:phase=="mix"?mix:phase=="water"?water:phase=="stir"?stir:phase=="drain"?drain:phase=="coat"?coat:phase=="combine"?combine:plate;
        public void Add(string phase,float amount)
        {
            float More(float n)=>Math.Min(1,n+amount);
            switch(phase){case "pan":case "pour":pour=More(pour);break;case "mix":mix=More(mix);break;case "water":water=More(water);break;case "stir":stir=More(stir);break;case "drain":drain=More(drain);break;case "coat":coat=More(coat);break;case "combine":combine=More(combine);break;default:plate=More(plate);break;}
        }
    }
    public static class MealFlow
    {
        public const int Schema=26,Version=4;
        private static readonly string[][] Paths={
            new[]{"pan","cook-first","flip","cheese","cook-second","assembly","stack","serve"},
            new[]{"water","stir","boil","drain","sauce","coat","plate","cheese","serve"},
            new[]{"vegetables","chop","pour","stir","simmer","ladle","serve"},
            new[]{"ingredients","mix","pour","cook-first","flip","cook-second","stack","fruit","serve"},
            new[]{"water","boil","vegetables","chop","stir","pan-cook","combine","scoop","serve"}
        };
        public static readonly string[] Continuous={"pan","mix","pour","water","stir","drain","coat","plate","ladle","scoop","combine"};
        public static bool Active(FoodDish d)=>d!=null && d.recipeVersion==Version;
        private static int RecipeIndex(FoodDish d)=>Array.FindIndex(Kitchen.Recipes.Skip(10).ToArray(),r=>r.id==d.recipe);
        public static string[] Stages(FoodDish d)=>Paths[RecipeIndex(d)];
        public static bool HeatStage(string stage)=>stage=="cook-first" || stage=="cook-second" || stage=="boil" || stage=="simmer" || stage=="pan-cook";
        public static bool Has(FoodDish d,string name)=>d.ingredients.Any(a=>a.ingredient==name);
        public static string Base(string recipe)=>recipe=="MEAL-01"?"patty":Kitchen.Recipe(recipe).ingredient;
        public static string[] Vegetables(FoodDish d)=>d.recipe=="MEAL-03"?new[]{"carrot","tomato","capsicum"}:new[]{"carrot","capsicum","mushroom"};
        public static string[] Required(FoodDish d)=>d.recipe=="MEAL-01"?new[]{"bun","lettuce"}:d.recipe=="MEAL-02"?new[]{"sauce"}:d.recipe=="MEAL-04"?new[]{"egg","milk","banana"}:Vegetables(d);
        public static string[] ReadyInputs(FoodDish d)=>d.recipe=="MEAL-01" || d.recipe=="MEAL-02"?Array.Empty<string>():d.recipe=="MEAL-04"?new[]{"egg","milk"}:Vegetables(d);
        public static string[] Palette(FoodDish d)
        {
            var finishing=d.stage=="cheese" || d.stage=="assembly" || d.stage=="fruit";
            if(finishing && d.experiment)return Kitchen.Ingredients;
            return d.stage=="ingredients"?new[]{"egg","milk"}.Where(n=>!Has(d,n)).ToArray():d.stage=="vegetables"?Vegetables(d).Where(n=>!Has(d,n)).ToArray():d.stage=="sauce"?new[]{"sauce"}:d.stage=="cheese"?new[]{"cheese"}:d.stage=="assembly"?new[]{"bun","lettuce","tomato","cheese"}:d.stage=="fruit"?new[]{"banana","strawberry"}:Array.Empty<string>();
        }
        // One compact, explicitly versioned record keeps meal fields out of the
        // many empty inline FoodDish records in Unity's frequent world JSON.
        public static MealProgress Read(FoodDish d)
        {
            if(d.preparation==null || d.preparation.Length!=72)throw new InvalidDataException("Invalid meal preparation.");
            using(var stream=new MemoryStream(Convert.FromBase64String(d.preparation)))using(var r=new BinaryReader(stream)){
                if(stream.Length!=53 || r.ReadByte()!=1)throw new InvalidDataException("Unknown meal preparation.");
                return new MealProgress{mix=r.ReadSingle(),pour=r.ReadSingle(),water=r.ReadSingle(),stir=r.ReadSingle(),drain=r.ReadSingle(),coat=r.ReadSingle(),plate=r.ReadSingle(),combine=r.ReadSingle(),chop=r.ReadInt32(),flip=r.ReadInt32(),stack=r.ReadInt32(),heat=r.ReadInt32(),started=r.ReadInt32()};
            }
        }
        public static void Save(FoodDish d,MealProgress p)
        {
            using(var stream=new MemoryStream())using(var w=new BinaryWriter(stream)){
                w.Write((byte)1);foreach(var f in new[]{p.mix,p.pour,p.water,p.stir,p.drain,p.coat,p.plate,p.combine})w.Write(f);
                foreach(var n in new[]{p.chop,p.flip,p.stack,p.heat,p.started})w.Write(n);d.preparation=Convert.ToBase64String(stream.ToArray());
            }
        }
        public static int StackCount(FoodDish d)=>d.recipe=="MEAL-01"?4:3;
        public static bool Complete(FoodDish d)=>Complete(d,d.stage,Read(d));
        private static bool Complete(FoodDish d,string stage,MealProgress p)
        {
            if(Continuous.Contains(stage))return p.Amount(stage)==1;
            switch(stage){case "ingredients":return Has(d,"egg") && Has(d,"milk");case "vegetables":return Vegetables(d).All(n=>Has(d,n));case "sauce":return Has(d,"sauce");case "chop":return p.chop==3;case "flip":return p.flip==1;case "stack":return p.stack==StackCount(d);case "assembly":return Has(d,"bun") && Has(d,"lettuce");case "fruit":return Has(d,"banana");case "cheese":return true;default:return false;}
        }
        public static string Token(FoodDish d)=>d.id+"@"+d.stage+(d.stage=="stack"?"@"+Read(d).stack:"");
        public static void Finish(FoodDish d)=>d.stage=Stages(d)[Array.IndexOf(Stages(d),d.stage)+1];
        public static void Added(FoodDish d,FoodAddition a){a.phase=d.stage;if((d.stage=="ingredients" || d.stage=="vegetables" || d.stage=="sauce") && Complete(d))Finish(d);}
        public static void Begin(FoodDish d,bool ready)
        {
            d.recipeVersion=Version;d.step=0;d.mixed=d.poured=0;d.layers=d.icingMask=d.cutMask=0;d.heated=false;d.heat=0;
            var p=new MealProgress();var stages=Stages(d);d.stage=stages[0];
            if(ready){
                var destination=d.recipe=="MEAL-01"?"assembly":d.recipe=="MEAL-02"?"sauce":d.recipe=="MEAL-03"?"ladle":d.recipe=="MEAL-04"?"stack":"combine";
                foreach(var phase in stages.TakeWhile(s=>s!=destination)){
                    if(Continuous.Contains(phase))p.Add(phase,1);if(phase=="chop")p.chop=3;if(phase=="flip")p.flip=1;if(HeatStage(phase))p.heat++;
                }
                p.started=p.heat;d.heated=true;d.heat=Kitchen.HeatSeconds;d.stage=destination;
            }
            Save(d,p);
        }
        public static bool Heating(FoodDish d){if(!HeatStage(d.stage))return false;var p=Read(d);return p.started>p.heat;}
        public static void StartHeat(FoodDish d){var p=Read(d);if(p.started==p.heat){p.started++;d.heat=0;Save(d,p);}}
        public static void Heated(FoodDish d){var p=Read(d);p.heat++;p.started=p.heat;d.heated=p.heat==Stages(d).Count(HeatStage);Save(d,p);Finish(d);}
        public static bool Valid(FoodDish d,int schema)
        {
            if(schema<Schema || RecipeIndex(d)<0 || d.ingredients==null || d.ingredients.Any(a=>a==null) || !Has(d,Base(d.recipe)) || d.step!=0 || d.mixed!=0 || d.poured!=0 || d.layers!=0 || d.icingMask!=0 || d.cutMask!=0)return false;
            try{
                var p=Read(d);var stages=Stages(d);var at=Array.IndexOf(stages,d.stage);if(at<0)return false;
                if(new[]{p.mix,p.pour,p.water,p.stir,p.drain,p.coat,p.plate,p.combine}.Any(f=>float.IsNaN(f) || float.IsInfinity(f) || f<0 || f>1) || p.chop<0 || p.chop>3 || p.flip<0 || p.flip>1 || p.stack<0 || p.stack>StackCount(d))return false;
                var heats=stages.Take(at).Count(HeatStage);if(p.heat!=heats || p.started<heats || p.started>heats+(HeatStage(d.stage)?1:0) || d.heated!=(heats==stages.Count(HeatStage)))return false;
                if(p.started==p.heat && d.heat!=(p.heat==0?0:Kitchen.HeatSeconds))return false;
                foreach(var phase in stages.Take(at))if(!HeatStage(phase) && !Complete(d,phase,p))return false;
                foreach(var phase in stages.Skip(at+1))if(Continuous.Contains(phase) && p.Amount(phase)!=0 || phase=="chop" && p.chop!=0 || phase=="flip" && p.flip!=0 || phase=="stack" && p.stack!=0)return false;
                return true;
            }catch(Exception e) when(e is IOException || e is FormatException || e is ArgumentException){return false;}
        }
    }
    public sealed partial class GameWorld
    {
        public static GameWorld WithMealPreparation(GameWorld world)
        {world=WithPizzaPreparation(world);if(world.Schema>=MealFlow.Schema)return world;var s=world.Snapshot();s.schema=MealFlow.Schema;s.revision++;Validate(s);return new GameWorld(s);}
        private string MealOperation(SoloCommand c,SoloPlayer p,SoloToy item)
        {
            var d=item?.kitchen?.dish;
            if(state.schema<MealFlow.Schema || p.zone!="garden" || !PrepAvailable(item,p) || item.kind!=ToyKind.Cookware || !MealFlow.Active(d) || d.portions!=15)return "need-whole-dish";
            if(c.target!=MealFlow.Token(d))return "next-step-changed";var op=c.value.Substring(5);
            if(op=="finish"){if(!MealFlow.Complete(d))return "preparation-incomplete";MealFlow.Finish(d);Touch(item);return null;}
            if(op!=d.stage)return "next-step-changed";var progress=MealFlow.Read(d);
            if(MealFlow.Continuous.Contains(op)){
                if(float.IsNaN(c.x) || float.IsInfinity(c.x) || c.x<=0 || c.x>.2f)return "invalid-preparation";progress.Add(op,c.x);
            }else if(op=="chop"){
                if(c.x!=0 && c.x!=1)return "invalid-preparation";progress.chop|=1<<(int)c.x;
            }else if(op=="flip")progress.flip=1;
            else if(op=="stack"){if(progress.stack>=MealFlow.StackCount(d))return "preparation-complete";progress.stack++;}
            else return "next-step-changed";
            MealFlow.Save(d,progress);Touch(item);return null;
        }
    }
}
