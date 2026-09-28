using System;
using System.Linq;

namespace LittleWeeps.Core
{
    // Dispatch on the food's saved contract, never the currently selected menu.
    public static class PreparationFlow
    {
        public static bool Active(FoodDish d)=>CakeFlow.Active(d) || PizzaFlow.Active(d);
        public static string Token(FoodDish d)=>PizzaFlow.Active(d)?d.id+"@"+d.stage:CakeFlow.Token(d);
        public static string[] StageList(FoodDish d)=>PizzaFlow.Active(d)?PizzaFlow.Stages(d):CakeFlow.StageList(d);
        public static int Index(FoodDish d)=>Array.IndexOf(StageList(d),d.stage);
        public static string[] Palette(FoodDish d)=>PizzaFlow.Active(d)?PizzaFlow.Palette(d):CakeFlow.Palette(d);
        public static bool CanAdd(FoodDish d,string ingredient)=>Palette(d).Contains(ingredient);
        public static string[] Required(FoodDish d)=>PizzaFlow.Active(d)?PizzaFlow.Inputs(d):CakeFamilies.Active(d)?CakeFamilies.Batter(d.recipe).Concat(new[]{"icing"}).ToArray():CakeFlow.Batter.Concat(new[]{"icing"}).ToArray();
        public static string Next(FoodDish d)=>PizzaFlow.Active(d)?d.stage=="bake"?"heat":d.stage:CakeFlow.Next(d);
        public static void Added(FoodDish d,FoodAddition a){if(PizzaFlow.Active(d))PizzaFlow.Added(d,a);else CakeFlow.Added(d,a);}
        public static bool Complete(FoodDish d)=>PizzaFlow.Active(d)?PizzaFlow.Complete(d):CakeFlow.Complete(d);
        public static void Finish(FoodDish d){if(PizzaFlow.Active(d))PizzaFlow.Finish(d);else CakeFlow.Finish(d);}
        public static bool Valid(FoodDish d,int schema)=>PizzaFlow.Active(d)?PizzaFlow.Valid(d,schema):CakeFlow.Valid(d,schema);
    }
}
