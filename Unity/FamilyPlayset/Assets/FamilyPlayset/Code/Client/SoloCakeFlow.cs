using System;
using System.Collections;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform cakeSurface;
        private Image[] cakeParts;
        private Image cakeTool;
        private bool cakeHelping,cakeSending;
        private string cakeDisplayToken,cakeReleaseToken;
        private Vector2 cakeToolPoint;
        private float cakeMoveTime,cakeDemoUntil,cakeAssistStart;
        private float cakeNextPaint;
        private FoodDish cakeDrawn;
        private FoodDish CurrentCake=>ReadToys().FirstOrDefault(t=>t.id==cookingItem)?.kitchen?.dish;
        public string CakeToken=>KitchenOpen && PreparationFlow.Active(CurrentCake)?PreparationFlow.Token(CurrentCake):null;
        private Sprite CakeSprite(int index)=>KitchenSprite("cake-stages",index,3,2);

        private void BuildCakeSurface()
        {
            cakeSurface=Panel(kitchenCooking,"Cake activity",new Vector2(0,25),new Vector2(550,450),new Color(1,.98f,.93f),true).rectTransform;
            cakeSurface.gameObject.AddComponent<Button>().targetGraphic=cakeSurface.GetComponent<Image>();
            cakeSurface.gameObject.AddComponent<CakeGesture>().Screen=this;
            cakeParts=Enumerable.Range(0,48).Select(i=>HomePicture(cakeSurface,"Cake component "+i,Vector2.zero,new Vector2(100,100),CakeSprite(0))).ToArray();
            cakeTool=HomePicture(cakeSurface,"Your cooking tool",Vector2.zero,new Vector2(150,150),CakeSprite(5));cakeTool.preserveAspect=true;
            cakeSurface.gameObject.SetActive(false);
            BuildCakeFamilyChoices();
        }
        private static string CakeInstruction(FoodDish d)
        {
            if(PizzaFlow.Active(d))return PizzaInstruction(d);
            if(CakeFamilies.Active(d))return CakeFamilyInstruction(d);
            switch(d.stage){
                case "ingredients":return "Add egg, milk and chocolate to the cake mix";
                case "mix":return "Stir inside the bowl — watch the batter become smooth";
                case "pour":return "Slide the bowl toward the two cake tins";
                case "bake":return "Bake the two layers and watch them rise";
                case "filling":return d.ingredients.Any(a=>a.ingredient=="icing")?"Spread filling across the first layer":"Add the icing for your filling";
                case "stack":return "Slide the top layer onto the filling";
                case "ice":return "Spread icing across the top of your cake";
                case "decorate":return "Place chocolate, berries or sprinkles wherever you like";
                case "cut":return "Cut across the cake, then up and down";
                default:return "Your cake is ready! Tap a plate to share";
            }
        }
        private void PaintCakeControls(FoodDish d,SoloToy current)
        {
            cakeDrawn=d;
            var active=PreparationFlow.Active(d) && d.portions!=0;
            PaintCakeFamilyChoices(d);
            cakeSurface.gameObject.SetActive(active);kitchenPreview.gameObject.SetActive(!active);
            if(!active)return;
            if(cakeDisplayToken!=PreparationFlow.Token(d)){cakeDisplayToken=PreparationFlow.Token(d);cakeToolPoint=Vector2.zero;cakeMoveTime=0;kitchenHint.text=CakeInstruction(d);}
            kitchenStage.text="Make  •  Decorate  •  Serve     |     "+(PreparationFlow.Index(d)+1)+" / "+PreparationFlow.StageList(d).Length;
            if(string.IsNullOrEmpty(kitchenHint.text))kitchenHint.text=CakeInstruction(d);
            kitchenUndo.transform.parent.gameObject.SetActive(false);
            kitchenIngredients.gameObject.SetActive(PreparationFlow.Palette(d).Length>0);
            kitchenMore.transform.parent.gameObject.SetActive(PreparationFlow.Palette(d).Length>6);kitchenPrevious.transform.parent.gameObject.SetActive(PreparationFlow.Palette(d).Length>6 && kitchenPage>0);
            kitchenServing.gameObject.SetActive(d.stage=="serve" && current.kind==ToyKind.Cookware && CurrentArea=="garden");
            kitchenStep.transform.parent.gameObject.SetActive(d.stage!="serve" && d.stage!="ingredients" && !(d.stage=="filling" && !d.ingredients.Any(a=>a.ingredient=="icing")));
            var labels=new[]{"","Stir for me","Pour for me","Bake","Spread filling","Stack layer","Spread icing","Finish decorating","Slice cake",""};
            kitchenStep.text=PizzaFlow.Active(d)?PizzaAction(d):CakeFamilies.Active(d)?CakeFamilyAction(d):labels[PreparationFlow.Index(d)];
            if(CakeFamilies.Active(d))kitchenStep.transform.parent.gameObject.SetActive(d.stage!="ingredients" && d.stage!="colors" && d.stage!="serve" && !(d.stage=="ice" && !d.ingredients.Any(a=>a.ingredient=="icing")));
            if(PizzaFlow.Active(d))kitchenStep.transform.parent.gameObject.SetActive(d.stage!="sauce" && d.stage!="vegetables" && d.stage!="serve");
            kitchenStep.transform.parent.GetComponent<Button>().interactable=!cakeHelping && !cakeSending && !(d.stage=="bake" && Kitchen.Slot(current.container,out var g,out _) && g=="oven");
            kitchenActionIcon.sprite=d.stage=="bake"?KitchenSprite("oven-closed"):d.stage=="pour"?CakeSprite(0):d.stage=="stack"?CakeSprite(3):d.stage=="cut"?KitchenSprite("utensils",1,2,2):CakeSprite(5);
            if(PizzaFlow.Active(d)){
                kitchenActionIcon.sprite=d.stage=="knead"?IngredientSprite("dough"):d.stage=="roll"?KitchenSprite("utensils",2,2,2):d.stage=="chop" || d.stage=="cut"?KitchenSprite("utensils",1,2,2):d.stage=="toppings"?CookingLayer(0):kitchenActionIcon.sprite;
                if(d.stage=="toppings")kitchenStep.transform.parent.GetComponent<Button>().interactable=PizzaFlow.Complete(d) && !cakeHelping && !cakeSending;
            }
            PaintCake(d);
        }
        private void PaintCake(FoodDish d)
        {
            if(cakeParts!=null)foreach(var p in cakeParts)FoodPortionClip.Apply(p,15,Vector2.zero);
            if(PizzaFlow.Active(d)){PaintPizzaPreparation(d);return;}
            if(CakeFamilies.Active(d)){PaintFamilyCake(d);return;}
            if(cakeParts==null || !cakeSurface.gameObject.activeSelf)return;
            foreach(var part in cakeParts)part.gameObject.SetActive(false);cakeTool.gameObject.SetActive(false);var at=0;
            Image Part(Sprite sprite,float x,float y,float w,float h,float alpha=1){var p=cakeParts[at++];p.gameObject.SetActive(true);p.sprite=sprite;p.material=null;p.type=Image.Type.Simple;p.preserveAspect=false;p.color=new Color(1,1,1,alpha);p.rectTransform.anchoredPosition=new Vector2(x,y);p.rectTransform.sizeDelta=new Vector2(w,h);p.rectTransform.localEulerAngles=Vector3.zero;return p;}
            void Batter(float x,float y,float w,float h,float amount){if(amount<=0)return;Part(CakeSprite(1),x,y,w*Mathf.Sqrt(amount),h*Mathf.Sqrt(amount));}
            var stage=d.stage;var prep=stage=="ingredients" || stage=="mix";
            if(prep){
                Part(CakeSprite(0),0,-12,415,280);
                // Ingredients remain visible until their mixture is smooth.
                if(stage=="mix")Batter(0,45,344,118,.05f+d.mixed*.95f);
                var raw=d.ingredients.Where(a=>a.phase!="decorate").Take(4).ToArray();
                for(var i=0;i<raw.Length;i++)Part(KitchenSprite("toppings",raw[i].ingredient=="cake-mix"?9:Array.IndexOf(Kitchen.Ingredients,raw[i].ingredient),5,5),-92+i*59,40+(i%2)*17,83,72,1-d.mixed);
            }else if(stage=="pour"){
                var bowl=Part(CakeSprite(0),-105,110,230,156);bowl.rectTransform.localEulerAngles=new Vector3(0,0,-d.poured*38);
                Batter(-105,140,180,65,1-d.poured);
                for(var i=0;i<2;i++){Part(CakeSprite(2),-110+i*220,-105,222,135);Batter(-110+i*220,-84,186,65,d.poured);}
                if(d.poured>0 && d.poured<1){var stream=Part(CakeSprite(1),5,18,25,150);stream.color=new Color(.65f,.42f,.28f);}
            }else if(stage=="bake"){
                Part(KitchenSprite("oven-closed"),0,5,500,410);
                var risen=(float)(d.heat/Kitchen.HeatSeconds);
                for(var i=0;i<2;i++){Part(CakeSprite(2),-105+i*210,-50,180,105);Part(CakeSprite(3),-105+i*210,-18+risen*18,158,45+risen*45);}
            }else{
                Part(CookingLayer(5),0,-83,450,215);
                var single=stage=="filling" || stage=="stack" && d.layers<2;
                Part(CakeSprite(3),0,-55,350,192);
                if(!single){Part(CakeSprite(4),0,-10,344,116);Part(CakeSprite(3),0,37,350,192);}
                var topY=single?-5:90;
                if(stage=="filling" || stage=="ice"){
                    // Each touched region reveals its corresponding piece of one
                    // frosting surface. Coverage is persistent, not a generic meter.
                    for(var cell=0;cell<9;cell++)if((d.icingMask&(1<<cell))!=0){var p=Part(CakeIcingCell(cell),-115+(cell%3)*115,topY-39+(cell/3)*39,116,40);}
                }else if(stage=="stack" || PreparationFlow.Index(d)>=7)Part(CakeSprite(4),0,topY,345,118);
                if(stage=="stack" && d.layers<2){
                    var destination=new Vector2(0,37);var moving=cakeHelping?Vector2.Lerp(new Vector2(120,147),destination,Mathf.Clamp01((Time.unscaledTime-cakeAssistStart)/.6f)):cakeMoveTime>Time.unscaledTime?cakeToolPoint:new Vector2(120,147);
                    Part(CakeSprite(3),moving.x,moving.y,220,120);
                }
                foreach(var add in d.ingredients.Where(a=>a.phase=="decorate").Take(20)){
                    var quadrant=add.x>=0?(add.y>=0?0:1):(add.y<0?2:3);if((d.portions&(1<<quadrant))==0)continue;
                    Part(KitchenSprite("toppings",Array.IndexOf(Kitchen.Ingredients,add.ingredient),5,5),add.x*150,topY+add.y*48,57,47);
                }
                // Portion visibility is shared with the world view; original
                // portions are removed on serving rather than copied whole.
                if(d.portions!=15)foreach(var p in cakeParts.Where(p=>p.gameObject.activeSelf).Skip(1)){
                    p.type=Image.Type.Filled;p.fillMethod=Image.FillMethod.Radial360;p.fillClockwise=true;p.fillAmount=Enumerable.Range(0,4).Count(i=>(d.portions&(1<<i))!=0)/4f;
                    p.fillOrigin=(6-Enumerable.Range(0,4).First(i=>(d.portions&(1<<i))!=0))%4;
                }
                if(stage=="cut" || stage=="serve"){
                    for(var bit=0;bit<2;bit++)if((d.cutMask&(1<<bit))!=0){var p=Part(CakeSprite(5),0,topY,bit==0?320:4,bit==0?4:95);p.sprite=null;p.color=new Color(.29f,.14f,.09f,.65f);}
                }
            }
            if(stage=="mix" || stage=="filling" || stage=="ice" || stage=="cut"){
                cakeTool.gameObject.SetActive(true);cakeTool.sprite=stage=="cut"?KitchenSprite("utensils",1,2,2):CakeSprite(5);
                var automatic=cakeHelping || Time.unscaledTime<cakeDemoUntil;
                cakeTool.rectTransform.anchoredPosition=automatic?new Vector2(Mathf.Sin(Time.unscaledTime*3)*110,Mathf.Cos(Time.unscaledTime*3)*38+(stage=="mix"?45:70)):cakeMoveTime>Time.unscaledTime?cakeToolPoint:new Vector2(115,90);
                cakeTool.rectTransform.localEulerAngles=new Vector3(0,0,automatic?Mathf.Sin(Time.unscaledTime*3)*18:0);
            }
        }
        private Sprite CakeIcingCell(int index)
        {
            var key="cake-icing-"+index;if(kitchenSprites.TryGetValue(key,out var sprite))return sprite;
            var source=CakeSprite(4);var r=source.rect;var w=r.width/3;var h=r.height/3;
            sprite=Sprite.Create(source.texture,new Rect(r.x+index%3*w,r.y+index/3*h,w,h),new Vector2(.5f,.5f));homeSprites.Add(sprite);kitchenSprites[key]=sprite;return sprite;
        }
        public void CakeTap()
        {
            var d=CurrentCake;if(!PreparationFlow.Active(d) || cakeHelping || cakeSending)return;
            if(PizzaFlow.Active(d) && (d.stage=="sauce" || d.stage=="vegetables" || d.stage=="toppings" && !PizzaFlow.Complete(d))){kitchenHint.text=PizzaInstruction(d);return;}
            if(CakeFamilies.Active(d) && (d.stage=="colors" || d.stage=="ice" && !d.ingredients.Any(a=>a.ingredient=="icing"))){kitchenHint.text=CakeInstruction(d);return;}
            if(d.stage=="ingredients" || d.stage=="filling" && !d.ingredients.Any(a=>a.ingredient=="icing")){kitchenHint.text=CakeInstruction(d);return;}
            if(d.stage=="bake"){KitchenCommand("easy:bake",cookingItem);return;}
            if(d.stage=="serve")return;
            StartCoroutine(AssistCake(PreparationFlow.Token(d),d.stage,cookingItem));
        }
        private IEnumerator AssistCake(string token,string stage,string item)
        {
            cakeHelping=true;cakeAssistStart=Time.unscaledTime;cakeReleaseToken=null;
            try{
                if(stage=="decorate" || stage=="toppings"){CakeSend("finish",token,0,0);yield break;}
                var progress=stage=="mix" || stage=="pour" || stage=="knead" || stage=="roll";var spreading=stage=="ice" || stage=="filling" || stage=="spread";
                var count=progress?10:spreading?9:stage=="cut" || stage=="chop" || stage=="features"?2:1;
                for(var i=0;i<count && CakeToken==token;i++){
                    if(!KitchenOpen || applicationPaused || !Ready)yield break;
                    yield return new WaitForSecondsRealtime(stage=="stack"?.6f:stage=="cut"?.45f:.2f);
                    if(CakeToken!=token || applicationPaused || !Ready)yield break;
                    var x=spreading?-.67f+i%3*.67f:stage=="cut" || stage=="chop" || stage=="features"?i:stage=="stack"?0:.12f;
                    var y=spreading?-.67f+i/3*.67f:0;
                    CakeSend(stage,token,x,y);
                    while(cakeSending && CakeToken==token)yield return null;
                }
                if(CakeToken==token && PreparationFlow.Complete(CurrentCake))CakeSend("finish",token,0,0);
            }finally{cakeHelping=false;if(KitchenOpen)PresentKitchen();}
        }
        private void CakeSend(string op,string token,float x,float y,Action<SoloResult> done=null)
        {
            if(cakeSending || CakeToken!=token || !Ready)return;
            cakeSending=true;KitchenCommand((PizzaFlow.Active(CurrentCake)?"prepare:":"cake:")+op,cookingItem,token,x,y+2,r=>{cakeSending=false;done?.Invoke(r);if(r.Accepted && op!="finish" && CakeToken==token && cakeReleaseToken==token && PreparationFlow.Complete(CurrentCake))CakeSend("finish",token,0,0);});
        }
        public void CakeStroke(string token,Vector2 from,Vector2 to,bool end,Vector2 start)
        {
            if(token==null || token!=CakeToken || cakeHelping)return;
            cakeReleaseToken=end?token:null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(cakeSurface,from,null,out var a);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(cakeSurface,to,null,out var b);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(cakeSurface,start,null,out var origin);
            if(Mathf.Abs(b.x)>280 || Mathf.Abs(b.y)>230)return;
            cakeToolPoint=b;cakeMoveTime=Time.unscaledTime+.3f;var d=CurrentCake;var stage=d.stage;
            void Finish(SoloResult result){if(end && result.Accepted && CakeToken==token && PreparationFlow.Complete(CurrentCake))CakeSend("finish",token,0,0);}
            if(stage=="mix" || stage=="pour" || stage=="knead" || stage=="roll"){
                var amount=Mathf.Min(.18f,Vector2.Distance(a,b)/(stage=="mix"?1100:650));
                if(amount>0)CakeSend(stage,token,amount,0,Finish);
                else if(end && PreparationFlow.Complete(d))CakeSend("finish",token,0,0);
            }else if(stage=="ice" || stage=="filling")CakeSend(stage,token,Mathf.Clamp(b.x/175,-.99f,.99f),Mathf.Clamp((b.y-(stage=="ice"?90:-5))/75,-.99f,.99f),Finish);
            else if(stage=="spread")CakeSend(stage,token,Mathf.Clamp(b.x/175,-.99f,.99f),Mathf.Clamp(b.y/175,-.99f,.99f),Finish);
            else if(stage=="stack" && end)CakeSend(stage,token,b.x/250,(b.y+30)/250,Finish);
            else if((stage=="cut" || stage=="chop") && end && Vector2.Distance(origin,b)>110)CakeSend(stage,token,Mathf.Abs(b.x-origin.x)>Mathf.Abs(b.y-origin.y)?0:1,0,Finish);
        }
        private void TickCake()
        {
            if(!KitchenOpen || cakeSurface==null || !cakeSurface.gameObject.activeSelf || !HasWorld)return;
            if(Time.unscaledTime<cakeNextPaint || cakeDrawn==null)return;
            if(!cakeHelping && cakeMoveTime<Time.unscaledTime && cakeDemoUntil<Time.unscaledTime && cakeDrawn.stage!="bake")return;
            cakeNextPaint=Time.unscaledTime+.04f;
            var d=cakeDrawn.stage=="bake"?CurrentCake:cakeDrawn;if(PreparationFlow.Active(d))PaintCake(d);
        }
        private void PaintSmallCake(FoodDish d,Image food,Image baseImage,Image[] extras)
        {
            if(PizzaFlow.Active(d)){PaintSmallPizza(d,food,baseImage,extras);return;}
            if(CakeFamilies.Active(d)){PaintSmallFamilyCake(d,food,baseImage,extras);return;}
            foreach(var p in extras)p.gameObject.SetActive(false);baseImage.gameObject.SetActive(true);
            food.gameObject.SetActive(d.portions!=0);if(d.portions==0)return;
            food.type=Image.Type.Simple;food.color=Color.white;food.rectTransform.anchoredPosition=new Vector2(0,22);food.sprite=CakeSprite(d.heated?3:d.stage=="pour" || d.stage=="bake"?2:0);
            if(!d.heated){
                var batter=extras[0];batter.gameObject.SetActive(true);batter.sprite=CakeSprite(1);batter.type=Image.Type.Simple;batter.color=Color.white;batter.rectTransform.anchoredPosition=new Vector2(0,38);batter.rectTransform.sizeDelta=new Vector2(84,30)*Mathf.Sqrt(d.stage=="pour"?Mathf.Max(.1f,d.poured):.25f+d.mixed*.75f);return;
            }
            var n=0;
            Image Layer(Sprite sprite,Vector2 point,Vector2 size){var p=extras[n++];p.gameObject.SetActive(true);p.sprite=sprite;p.material=null;p.type=Image.Type.Simple;p.color=Color.white;p.rectTransform.anchoredPosition=point;p.rectTransform.sizeDelta=size;p.preserveAspect=false;return p;}
            if(d.layers==2){Layer(CakeSprite(4),new Vector2(0,35),new Vector2(126,43));Layer(CakeSprite(3),new Vector2(0,53),new Vector2(126,73));}
            var top=d.layers==2?73:42;
            if(d.stage=="stack" || PreparationFlow.Index(d)>=7)Layer(CakeSprite(4),new Vector2(0,top),new Vector2(126,43));
            else if(d.stage=="ice" || d.stage=="filling")for(var cell=0;cell<9;cell++)if((d.icingMask&(1<<cell))!=0)Layer(CakeIcingCell(cell),new Vector2(-42+cell%3*42,top-14+cell/3*14),new Vector2(43,15));
            foreach(var add in d.ingredients.Where(a=>a.phase=="decorate")){
                var bit=add.x>=0?(add.y>=0?0:1):(add.y<0?2:3);if((d.portions&(1<<bit))==0 || n>=extras.Length)continue;
                Layer(KitchenSprite("toppings",Array.IndexOf(Kitchen.Ingredients,add.ingredient),5,5),new Vector2(add.x*52,top+add.y*17),new Vector2(19,16));
            }
            if(d.portions!=15)foreach(var p in new[]{food}.Concat(extras.Take(n))){p.type=Image.Type.Filled;p.fillMethod=Image.FillMethod.Radial360;p.fillClockwise=true;p.fillAmount=Enumerable.Range(0,4).Count(i=>(d.portions&(1<<i))!=0)/4f;p.fillOrigin=(6-Enumerable.Range(0,4).First(i=>(d.portions&(1<<i))!=0))%4;}
        }
    }
}
