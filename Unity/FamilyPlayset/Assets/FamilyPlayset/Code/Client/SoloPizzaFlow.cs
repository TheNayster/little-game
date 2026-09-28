using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private static string PizzaInstruction(FoodDish d)
        {
            switch(d.stage){
                case "knead":return "Press and move the dough to knead it";
                case "roll":return "Roll back and forth — make a big pizza base";
                case "vegetables":return "Choose pepper, mushroom and tomato for your chopping board";
                case "chop":return "Cut across the vegetables, then chop them smaller";
                case "sauce":return "Add tomato sauce before spreading it";
                case "spread":return "Spread sauce over the base with your spoon";
                case "toppings":var missing=PizzaFlow.Missing(d);return missing.Length>0?"Add "+missing[0]+" from the bowls":d.recipe=="PIZ-05"?"Place a silly face — then tap Ready to bake":"Add more toppings if you like — then tap Ready to bake";
                case "bake":return "Bake your pizza and watch the crust and cheese change";
                case "cut":return "Slice across, then up and down — four pieces to share";
                default:return "Your pizza is ready! Tap a plate to share";
            }
        }
        private static string PizzaAction(FoodDish d)=>d.stage=="knead"?"Knead for me":d.stage=="roll"?"Roll for me":d.stage=="chop"?"Chop for me":d.stage=="spread"?"Spread sauce":d.stage=="toppings"?"Ready to bake":d.stage=="bake"?"Bake":d.stage=="cut"?"Slice pizza":"";
        private Sprite PizzaSauceCell(int cell)
        {
            var key="pizza-sauce-"+cell;if(kitchenSprites.TryGetValue(key,out var sprite))return sprite;
            var source=CookingLayer(2);var r=source.rect;var w=r.width/3;var h=r.height/3;
            sprite=Sprite.Create(source.texture,new Rect(r.x+cell%3*w,r.y+cell/3*h,w,h),new Vector2(.5f,.5f));homeSprites.Add(sprite);kitchenSprites[key]=sprite;return sprite;
        }
        private void PaintPizzaPreparation(FoodDish d)
        {
            if(cakeParts==null || !cakeSurface.gameObject.activeSelf)return;
            foreach(var p in cakeParts)p.gameObject.SetActive(false);cakeTool.gameObject.SetActive(false);var at=0;
            Image Part(Sprite s,float x,float y,float w,float h,float alpha=1){var p=cakeParts[at++];p.gameObject.SetActive(true);p.sprite=s;p.material=null;p.type=Image.Type.Simple;p.preserveAspect=false;p.color=new Color(1,1,1,alpha);p.rectTransform.anchoredPosition=new Vector2(x,y);p.rectTransform.sizeDelta=new Vector2(w,h);p.rectTransform.localEulerAngles=Vector3.zero;return p;}
            Part(CookingLayer(4),0,0,510,440);
            if(d.stage=="knead"){
                var pressing=cakeHelping || cakeMoveTime>Time.unscaledTime;var squeeze=pressing?Mathf.Sin(Time.unscaledTime*9)*12:0;
                Part(IngredientSprite("dough"),0,0,240+d.mixed*45+squeeze,190+d.mixed*25-squeeze);
            }else if(d.stage=="roll")Part(CookingLayer(0),0,0,200+d.poured*195,160+d.poured*235);
            else if(d.stage=="vegetables" || d.stage=="chop"){
                for(var i=0;i<3;i++)if(PizzaFlow.Has(d,PizzaFlow.Vegetables[i])){
                    var chopped=d.step==3 || d.step!=0 && i<2;Part(chopped?KitchenSprite("toppings",Array.IndexOf(Kitchen.Ingredients,PizzaFlow.Vegetables[i]),5,5):IngredientSprite(PizzaFlow.Vegetables[i]),-150+i*150,0,140,160);
                }
            }else{
                var heat=d.stage=="bake"?(float)(d.heat/Kitchen.HeatSeconds):d.heated?1:0;
                Part(CookingLayer(0),0,0,402,402);if(heat>0)Part(CookingLayer(1),0,0,402,402,heat);
                for(var cell=0;cell<9;cell++)if((d.icingMask&(1<<cell))!=0)Part(PizzaSauceCell(cell),-106.67f+cell%3*106.67f,-106.67f+cell/3*106.67f,108,108);
                if(d.stage=="spread" && d.icingMask==0)Part(KitchenSprite("toppings",1,5,5),0,0,90,90);
                var toppings=d.stage=="toppings" || d.stage=="bake" || d.stage=="cut" || d.stage=="serve";
                if(toppings){
                    if(PizzaFlow.Has(d,"cheese")){
                        if(heat<1)for(var i=0;i<9;i++){var angle=i*2.4f;Part(KitchenSprite("toppings",2,5,5),Mathf.Sin(angle)*100,Mathf.Cos(angle)*100,66,66,1-heat);}
                        if(heat>0)Part(CookingLayer(3),0,0,317,317,heat);
                    }
                    foreach(var a in d.ingredients.Where(a=>a.ingredient!="dough" && a.ingredient!="sauce" && a.ingredient!="cheese")){
                        Part(KitchenSprite("toppings",Array.IndexOf(Kitchen.Ingredients,a.ingredient),5,5),a.x*165,a.y*165,72,72);
                    }
                }
                for(var bit=0;bit<2;bit++)if((d.cutMask&(1<<bit))!=0){var p=Part(null,0,0,bit==0?330:4,bit==0?4:330);p.color=new Color(.34f,.15f,.08f,.7f);}
                foreach(var p in cakeParts.Where(p=>p.gameObject.activeSelf).Skip(1))FoodPortionClip.Apply(p,d.portions,Vector2.zero);
            }
            if(d.stage=="roll" || d.stage=="spread" || d.stage=="chop" || d.stage=="cut"){
                cakeTool.gameObject.SetActive(true);cakeTool.sprite=KitchenSprite("utensils",d.stage=="roll"?2:d.stage=="spread"?0:1,2,2);
                var moving=cakeHelping || Time.unscaledTime<cakeDemoUntil;
                cakeTool.rectTransform.anchoredPosition=moving?new Vector2(Mathf.Sin(Time.unscaledTime*3)*115,Mathf.Cos(Time.unscaledTime*3)*55):cakeMoveTime>Time.unscaledTime?cakeToolPoint:new Vector2(130,65);
                cakeTool.rectTransform.localEulerAngles=new Vector3(0,0,d.stage=="roll"?25:0);
            }
        }
        private void PaintSmallPizza(FoodDish d,Image food,Image plate,Image[] extras)
        {
            foreach(var p in extras)p.gameObject.SetActive(false);plate.gameObject.SetActive(true);food.gameObject.SetActive(d.portions!=0);if(d.portions==0)return;
            food.material=null;food.type=Image.Type.Simple;food.color=Color.white;food.preserveAspect=true;food.rectTransform.anchoredPosition=new Vector2(0,18);food.sprite=d.stage=="knead"?IngredientSprite("dough"):CookingLayer(d.heated?1:0);
            var diameter=Mathf.Min(food.rectTransform.sizeDelta.x,food.rectTransform.sizeDelta.y);var factor=d.stage=="knead"?.62f:d.stage=="roll"?.55f+d.poured*.45f:1;
            food.rectTransform.localScale=Vector3.one*factor;var n=0;
            Image Part(Sprite sprite,float x,float y,float w,float h){var p=extras[n++];p.gameObject.SetActive(true);p.sprite=sprite;p.material=null;p.type=Image.Type.Simple;p.color=Color.white;p.preserveAspect=false;p.rectTransform.anchoredPosition=new Vector2(x,y+18);p.rectTransform.sizeDelta=new Vector2(w,h);return p;}
            var s=diameter*.8f;
            for(var cell=0;cell<9;cell++)if((d.icingMask&(1<<cell))!=0)Part(PizzaSauceCell(cell),-s/3+cell%3*s/3,-s/3+cell/3*s/3,s/3+1,s/3+1);
            var toppingStage=d.stage=="toppings" || d.stage=="bake" || d.stage=="cut" || d.stage=="serve";
            if(toppingStage){
                if(PizzaFlow.Has(d,"cheese"))Part(d.heated?CookingLayer(3):KitchenSprite("toppings",2,5,5),0,0,s,s);
                foreach(var a in d.ingredients.Where(a=>a.ingredient!="dough" && a.ingredient!="sauce" && a.ingredient!="cheese")){
                    if(n>=extras.Length)break;
                    Part(KitchenSprite("toppings",Array.IndexOf(Kitchen.Ingredients,a.ingredient),5,5),a.x*diameter*.4f,a.y*diameter*.4f,diameter*.2f,diameter*.2f);
                }
            }
            foreach(var p in new[]{food}.Concat(extras.Take(n)))FoodPortionClip.Apply(p,d.portions,new Vector2(0,18));
        }
    }
}
