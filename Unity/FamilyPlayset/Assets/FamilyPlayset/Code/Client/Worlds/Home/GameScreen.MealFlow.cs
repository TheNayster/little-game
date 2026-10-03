using System;
using System.Collections;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private Sprite MealSprite(int i)=>KitchenSprite("meal-stages",i,4,3);
        private static string MealInstruction(FoodDish d)
        {
            switch(d.stage){
                case "pan":return "Slide the patty into the pan";
                case "water":return "Pour water into your pot";
                case "ingredients":return "Add egg and milk to your flour";
                case "vegetables":return "Choose the vegetables for your chopping board";
                case "chop":return "Chop across, then up and down";
                case "mix":return "Stir the batter until it is smooth";
                case "pour":return d.recipe=="MEAL-03"?"Pour your broth into the pot":"Pour the batter into three pancake circles";
                case "stir":return "Move your spoon through the food";
                case "cook-first":return "Cook the first side, then turn it over";
                case "flip":return "Slide your spatula to turn the food over";
                case "cook-second":return "Cook the other side — it stops safely when ready";
                case "boil":return d.recipe=="MEAL-05"?"Cook the rice — watch it swell as it takes up the water":"Cook your pasta in the bubbling pot";
                case "simmer":return "Simmer your soup — watch the vegetables move";
                case "pan-cook":return "Cook your chopped vegetables in the pan";
                case "drain":return "Tip the pasta into the colander and drain the water";
                case "sauce":return "Add tomato sauce to your cooked pasta";
                case "coat":return "Stir the sauce through the pasta";
                case "cheese":return "Add cheese if you like, then tap Ready";
                case "assembly":return "Choose a bun and lettuce — add more if you like";
                case "stack":return d.recipe=="MEAL-01"?"Place the next burger layer":"Place the next pancake on the stack";
                case "combine":return "Stir the cooked vegetables into your rice";
                case "fruit":return "Add banana — strawberries are welcome too";
                case "ladle":return "Ladle your soup into the serving bowl";
                case "plate":case "scoop":return "Scoop your food into the serving dish";
                default:return "Your meal is ready! Tap a plate to share";
            }
        }
        private static string MealActionLabel(FoodDish d)=>MealFlow.HeatStage(d.stage)?"Cook":d.stage=="pan"?"Into the pan":d.stage=="water"?"Pour water":d.stage=="chop"?"Chop for me":d.stage=="mix" || d.stage=="stir" || d.stage=="coat" || d.stage=="combine"?"Stir for me":d.stage=="pour"?"Pour for me":d.stage=="flip"?"Turn over":d.stage=="drain"?"Drain pasta":d.stage=="stack"?"Place layer":d.stage=="ladle"?"Ladle soup":d.stage=="plate" || d.stage=="scoop"?"Scoop food":d.stage=="assembly"?"Build burger":"Ready";
        private void PaintMealControls(FoodDish d,SoloToy current)
        {
            var choosing=d.stage=="ingredients" || d.stage=="vegetables" || d.stage=="sauce";
            kitchenStep.transform.parent.gameObject.SetActive(!choosing && d.stage!="serve");
            var finish=d.stage=="cheese" || d.stage=="assembly" || d.stage=="fruit";
            kitchenStep.transform.parent.GetComponent<Button>().interactable=!cakeHelping && !cakeSending && !Kitchen.Heating(current) && (!finish || MealFlow.Complete(d));
            kitchenActionIcon.sprite=MealFlow.HeatStage(d.stage)?MealSprite(d.recipe=="MEAL-01" || d.recipe=="MEAL-04" || d.stage=="pan-cook"?0:1):d.stage=="water"?MealSprite(3):d.stage=="drain"?MealSprite(2):d.stage=="chop"?KitchenSprite("utensils",1,2,2):d.stage=="pan" || d.stage=="flip" || d.stage=="stack"?KitchenSprite("utensils",3,2,2):d.stage=="ladle"?MealSprite(11):CakeSprite(5);
            kitchenHeat.text=Kitchen.Heating(current)?"Cooking… "+Mathf.CeilToInt((float)(Kitchen.HeatSeconds-d.heat)):"";
        }
        private void MealTap(FoodDish d)
        {
            if(d.stage=="serve")return;
            if(d.stage=="ingredients" || d.stage=="vegetables" || d.stage=="sauce" || (d.stage=="assembly" || d.stage=="fruit") && !MealFlow.Complete(d)){kitchenHint.text=MealInstruction(d);return;}
            if(MealFlow.HeatStage(d.stage)){KitchenCommand("easy:bake",cookingItem);return;}
            StartCoroutine(AssistMeal(PreparationFlow.Token(d),d.stage));
        }
        private IEnumerator AssistMeal(string token,string stage)
        {
            cakeHelping=true;cakeAssistStart=Time.unscaledTime;cakeReleaseToken=null;
            try{
                if(stage=="cheese" || stage=="assembly" || stage=="fruit"){CakeSend("finish",token,0,0);yield break;}
                var count=MealFlow.Continuous.Contains(stage)?10:stage=="chop"?2:1;
                for(var i=0;i<count && CakeToken==token;i++){
                    yield return new WaitForSecondsRealtime(stage=="flip" || stage=="stack"?.6f:.2f);
                    if(CakeToken!=token || !KitchenOpen || applicationPaused || !Ready)yield break;
                    CakeSend(stage,token,stage=="chop"?i:.12f,0);while(cakeSending)yield return null;
                }
                var current=CurrentCake;if(MealFlow.Active(current) && current.stage==stage && MealFlow.Complete(current))CakeSend("finish",PreparationFlow.Token(current),0,0);
            }finally{cakeHelping=false;if(KitchenOpen)PresentKitchen();}
        }
        private void MealStroke(string token,Vector2 a,Vector2 b,bool end,Vector2 origin)
        {
            var d=CurrentCake;var phase=d.stage;
            if(MealFlow.Continuous.Contains(phase)){
                var amount=Mathf.Min(.18f,Vector2.Distance(a,b)/700);
                if(amount>0)CakeSend(phase,token,amount,0);
                else if(end && MealFlow.Complete(d))CakeSend("finish",token,0,0);
            }else if(phase=="chop" && end && Vector2.Distance(origin,b)>100)CakeSend(phase,token,Mathf.Abs(b.x-origin.x)>Mathf.Abs(b.y-origin.y)?0:1,0);
            else if(phase=="flip" && end && Vector2.Distance(origin,b)>60)CakeSend(phase,token,0,0);
            else if(phase=="stack" && end && b.magnitude<180)CakeSend(phase,token,0,0,r=>{if(r.Accepted && MealFlow.Active(CurrentCake) && CurrentCake.stage=="stack" && MealFlow.Complete(CurrentCake))CakeSend("finish",PreparationFlow.Token(CurrentCake),0,0);});
        }
        private void PaintMeal(FoodDish d)
        {
            foreach(var p in cakeParts)p.gameObject.SetActive(false);cakeTool.gameObject.SetActive(false);var at=0;
            void Part(Sprite sprite,float x,float y,float w,float h,Color tint){if(at>=cakeParts.Length)return;var p=cakeParts[at++];p.gameObject.SetActive(true);p.sprite=sprite;p.material=null;p.type=Image.Type.Simple;p.preserveAspect=false;p.color=tint;p.rectTransform.anchoredPosition=new Vector2(x,y);p.rectTransform.sizeDelta=new Vector2(w,h);p.rectTransform.localEulerAngles=Vector3.zero;}
            DrawMeal(d,Part,true);
            if(MealSolidPortion(d))foreach(var p in cakeParts.Take(at).Skip(1))FoodPortionClip.Apply(p,d.portions,Vector2.zero);
            var stage=d.stage;
            if(MealFlow.Continuous.Contains(stage) || stage=="chop" || stage=="flip"){
                cakeTool.gameObject.SetActive(true);cakeTool.sprite=stage=="water"?MealSprite(3):stage=="chop"?KitchenSprite("utensils",1,2,2):stage=="pan" || stage=="flip"?KitchenSprite("utensils",3,2,2):stage=="ladle"?MealSprite(11):CakeSprite(5);
                cakeTool.rectTransform.localEulerAngles=new Vector3(0,0,stage=="water"?25:0);
                cakeTool.rectTransform.anchoredPosition=cakeHelping?new Vector2(Mathf.Sin(Time.unscaledTime*3)*100,Mathf.Cos(Time.unscaledTime*3)*40+45):cakeMoveTime>Time.unscaledTime?cakeToolPoint:new Vector2(155,100);
            }
        }
        private static bool MealSolidPortion(FoodDish d)=>(d.recipe=="MEAL-01" || d.recipe=="MEAL-04") && (d.stage=="serve" || d.stage=="fruit");
        private void PaintSmallMeal(FoodDish d,Image image,Image baseImage,Image[] extras,float displayScale)
        {
            foreach(var p in extras)p.gameObject.SetActive(false);baseImage.gameObject.SetActive(false);image.gameObject.SetActive(false);if(d.portions==0)return;var at=0;
            // Derive scale from the view, not a component resized on the previous paint.
            var scale=.25f*displayScale;var origin=new Vector2(0,18*displayScale);
            DrawMeal(d,(sprite,x,y,w,h,tint)=>{
                if(at>=extras.Length)return;var p=extras[at++];p.gameObject.SetActive(true);p.sprite=sprite;p.material=null;p.type=Image.Type.Simple;p.preserveAspect=false;p.color=tint;p.rectTransform.anchoredPosition=new Vector2(x*scale,y*scale)+origin;p.rectTransform.sizeDelta=new Vector2(w*scale,h*scale);p.rectTransform.localEulerAngles=Vector3.zero;
            },false);
            if(MealSolidPortion(d))foreach(var p in extras.Take(at).Skip(1))FoodPortionClip.Apply(p,d.portions,origin);
        }
        private void DrawMeal(FoodDish d,Action<Sprite,float,float,float,float,Color> add,bool animate)
        {
            var p=MealFlow.Read(d);var stage=d.stage;var white=Color.white;
            void Art(int n,float x,float y,float w,float h,float alpha=1)=>add(MealSprite(n),x,y,w,h,new Color(1,1,1,alpha));
            void Food(string name,float x,float y,float w,float h,float alpha=1)=>add(KitchenSprite("toppings",Array.IndexOf(Kitchen.Ingredients,name),5,5),x,y,w,h,new Color(1,1,1,alpha));
            void Liquid(float x,float y,float w,float h,Color color)=>add(circle,x,y,w,h,color);
            var motion=animate && (cakeHelping || cakeMoveTime>Time.unscaledTime);var clock=Time.unscaledTime;
            var cooking=MealFlow.HeatStage(stage) && MealFlow.Heating(d);var fraction=(float)(d.heat/Kitchen.HeatSeconds);
            var ready=stage=="serve" || stage=="fruit" || stage=="cheese" && d.recipe=="MEAL-02";
            var portions=Enumerable.Range(0,4).Count(i=>(d.portions&(1<<i))!=0)/4f;
            Vector2 StackPoint(Vector2 target)=>!animate?new Vector2(135,135):cakeHelping?Vector2.Lerp(new Vector2(135,135),target,Mathf.Clamp01((clock-cakeAssistStart)/.6f)):cakeMoveTime>clock?cakeToolPoint:new Vector2(135,135);
            void Plate(float x=0,float y=-70)=>add(CookingLayer(5),x,y,445,230,white);
            void Veg(float x,float y,float scale,float progress=1){var names=MealFlow.Vegetables(d);for(var i=0;i<9;i++){var angle=i*2.4f+(motion?clock*.8f:0);Food(names[i%3],x+Mathf.Sin(angle)*100*scale,y+Mathf.Cos(angle)*30*scale,50*scale,45*scale,progress);}}
            void Pot(float water,Color tint){Art(1,0,-30,405,295);if(water>0)Liquid(0,40,285,100*Mathf.Max(.15f,water),tint);}
            void Pancakes(float cooked,float amount){Art(0,15,-5,500,300);for(var i=0;i<3;i++){var x=-140+i*80;var y=i%2==0?-25:45;var flip=stage=="flip" && motion?Mathf.Clamp01((clock-cakeAssistStart)/.6f):0;var h=88*Mathf.Max(.12f,Mathf.Abs(Mathf.Cos(flip*Mathf.PI)));Art(8,x,y+Mathf.Sin(flip*Mathf.PI)*40,125*Mathf.Sqrt(amount),h*Mathf.Sqrt(amount),1-cooked);if(cooked>0)Art(9,x,y,125*Mathf.Sqrt(amount),88*Mathf.Sqrt(amount),cooked);}}
            void Finished(float quantity){Plate();
                if(d.recipe=="MEAL-01"){
                    Art(6,0,-78,320,162);Art(5,0,-34,294,165);if(MealFlow.Has(d,"cheese"))Food("cheese",0,-5,290,100);Food("lettuce",0,14,300,108);foreach(var a in d.ingredients.Where(a=>a.ingredient=="tomato"))Food("tomato",a.x*130,30+a.y*32,64,45);Art(7,0,85,323,230);
                }else if(d.recipe=="MEAL-04"){
                    for(var i=0;i<3;i++)Art(9,0,-52+i*33,330,200);foreach(var a in d.ingredients.Where(a=>a.phase=="fruit"))Food(a.ingredient,a.x*132,40+a.y*44,72,55);
                }else if(d.recipe=="MEAL-02"){add(KitchenSprite("toppings",20,5,5),0,0,340*Mathf.Sqrt(quantity),180*Mathf.Sqrt(quantity),new Color(1,.63f,.4f));if(MealFlow.Has(d,"cheese"))Food("cheese",0,20,270,110);}
                else if(d.recipe=="MEAL-03"){
                    var depth=Mathf.Lerp(-82,-54,quantity);var radius=Mathf.Sqrt(quantity);
                    Liquid(0,depth,288*radius,88*radius,new Color(.83f,.49f,.16f));Liquid(-4,depth+4,277*radius,77*radius,new Color(.95f,.65f,.28f));Liquid(-28,depth+15,145*radius,27*radius,new Color(1,.86f,.52f,.35f));Veg(0,depth+6,.68f*radius);
                }
                else {Art(10,0,-4,320*Mathf.Sqrt(quantity),220*Mathf.Sqrt(quantity));Veg(0,30,.86f,quantity);}
            }
            if(ready){Finished(portions);return;}
            if(stage=="vegetables" || stage=="chop"){
                add(CookingLayer(4),0,0,500,390,white);var names=MealFlow.Vegetables(d);
                for(var i=0;i<3;i++)if(MealFlow.Has(d,names[i])){var chopped=p.chop==3 || p.chop!=0 && i<2;add(chopped?KitchenSprite("toppings",Array.IndexOf(Kitchen.Ingredients,names[i]),5,5):IngredientSprite(names[i]),-145+i*145,0,135,155,white);}return;
            }
            if(d.recipe=="MEAL-01"){
                if(stage=="assembly" || stage=="stack"){
                    Plate();var n=stage=="assembly"?0:p.stack;
                    if(n>0)Art(6,0,-80,320,165);if(n>1){Art(5,0,-35,292,160);if(MealFlow.Has(d,"cheese"))Food("cheese",0,-5,275,100);}if(n>2)Food("lettuce",0,16,300,110);if(n>3)Art(7,0,85,323,230);
                    var target=StackPoint(new Vector2(0,n==0?-80:n==1?-35:n==2?16:85));
                    if(n<4 && n!=2)Art(n==0?6:n==1?5:7,target.x,target.y,170,110);if(n==2)Food("lettuce",target.x,target.y,175,105);
                }else{
                    Art(0,10,-5,500,300);var progress=stage=="pan"?p.pour:1;var done=p.heat>0?1:stage=="cook-first"?fraction*.65f:0;
                    var flip=stage=="flip" && motion?Mathf.Clamp01((clock-cakeAssistStart)/.6f):0;var height=160*Mathf.Max(.12f,Mathf.Abs(Mathf.Cos(flip*Mathf.PI)));var lift=Mathf.Sin(flip*Mathf.PI)*65;
                    Art(4,Mathf.Lerp(160,-55,progress),Mathf.Lerp(130,0,progress)+lift,240,height,1-done);if(done>0)Art(5,-55,lift,240,height,done);
                    if(MealFlow.Has(d,"cheese"))Food("cheese",-55,14,215,90);
                }
            }else if(d.recipe=="MEAL-04"){
                if(stage=="ingredients" || stage=="mix"){
                    add(CakeSprite(0),0,-40,425,290,white);add(FamilyCakeSprite(4),0,20,340*Mathf.Sqrt(Mathf.Max(.05f,p.mix)),120*Mathf.Sqrt(Mathf.Max(.05f,p.mix)),white);
                    var inputs=d.ingredients.Take(3).ToArray();for(var i=0;i<inputs.Length;i++)Food(inputs[i].ingredient,-90+i*90,35,90,70,1-p.mix);
                }else if(stage=="stack"){
                    Plate();for(var i=0;i<p.stack;i++)Art(9,0,-50+i*34,330,200);var target=StackPoint(new Vector2(0,-50+p.stack*34));if(p.stack<3)Art(9,target.x,target.y,210,135);
                }else{
                    Pancakes(p.flip==1?1:p.heat>0?.7f:fraction*.7f,stage=="pour"?Mathf.Max(.015f,p.pour):1);
                    if(stage=="pour"){add(CakeSprite(0),-140,150,185,112,white);if(p.pour<1)add(FamilyCakeSprite(4),-140,167,150*Mathf.Sqrt(1-p.pour),48,white);}
                }
            }else if(stage=="drain"){
                Art(1,-140,70,240,175);Art(2,125,-50,245,180);Food("pasta",Mathf.Lerp(-140,125,p.drain),Mathf.Lerp(120,0,p.drain),180,100);if(p.drain>0 && p.drain<1)Liquid(125,-120,14,85,new Color(.5f,.83f,.94f,.7f));
            }else if(stage=="plate" || stage=="ladle" || stage=="scoop"){
                Art(1,-140,50,240,175);add(CookingLayer(5),135,-45,265,145,white);var amount=p.plate;
                if(d.recipe=="MEAL-02"){Food("pasta",-140,93,170*(1-amount),95);Food("pasta",135,-10,210*amount,120*amount);}
                else if(d.recipe=="MEAL-03"){Liquid(-140,93,170,55,new Color(.93f,.65f,.28f,1-amount));Liquid(135,-20,193,68,new Color(.93f,.65f,.28f,amount));Veg(135,-15,.45f,amount);}
                else {Art(10,-140,94,175*(1-amount),105);Art(10,135,-10,205*amount,125*amount);Veg(135,10,.45f,amount);}
            }else if(stage=="sauce" || stage=="coat"){
                Plate();Food("pasta",0,0,365,200);if(MealFlow.Has(d,"sauce"))add(KitchenSprite("toppings",1,5,5),0,25,100+p.coat*220,75+p.coat*75,white);
            }else if(stage=="combine"){
                Plate();Art(10,0,0,340,240);Veg(Mathf.Lerp(180,0,p.combine),Mathf.Lerp(110,25,p.combine),.85f);
            }else if(d.recipe=="MEAL-05" && (stage=="stir" || stage=="pan-cook")){
                Art(0,15,-15,500,300);Veg(-55,0,1);
            }else{
                var water=stage=="water"?p.water:d.recipe=="MEAL-03"?p.pour:1;
                if(d.recipe=="MEAL-05" && stage=="boil")water=1-fraction;
                Pot(water,d.recipe=="MEAL-03"?new Color(.93f,.65f,.28f):new Color(.5f,.81f,.93f,.72f));
                if(d.recipe=="MEAL-03"){Veg(0,45,.8f);if(stage=="pour" && p.pour<1)add(IngredientSprite("broth"),-170,155,105,100,new Color(1,1,1,1-p.pour));}
                else if(d.recipe=="MEAL-05"){
                    var swelling=stage=="boil"?fraction:0;add(IngredientSprite("rice"),0,42,205,110,new Color(1,1,1,1-swelling));if(swelling>0)Art(10,0,42,210+swelling*70,90+swelling*25,swelling);
                }else{
                    var softened=stage=="boil"?fraction:0;add(IngredientSprite("pasta"),0,55,235,100,new Color(1,1,1,1-softened));if(softened>0)Food("pasta",0,55,240,95,softened);
                }
            }
            if(cooking)for(var i=0;i<4;i++)Liquid(-85+i*50,100+(animate?Mathf.Repeat(clock*30+i*19,100):i*12),12+i*2,20+i*3,new Color(1,1,1,.3f));
        }
    }
}
