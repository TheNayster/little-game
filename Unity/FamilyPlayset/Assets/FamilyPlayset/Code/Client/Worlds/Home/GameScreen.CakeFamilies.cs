using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform cakeColorChoices,cakeFeatureChoices;
        private Material cakeColorMaterial;
        private Material CakeColorMaterial=>cakeColorMaterial!=null?cakeColorMaterial:(cakeColorMaterial=new Material(WorldResources.Load<Shader>("Worlds/Home/Kitchen/CakeColor")));
        private static readonly Color[] CakeColors={new Color(1,.35f,.43f),new Color(1,.63f,.25f),new Color(1,.91f,.35f),new Color(.47f,.85f,.51f),new Color(.39f,.72f,1),new Color(.76f,.48f,.95f)};
        private Sprite FamilyCakeSprite(int i)=>KitchenSprite("cake-families",i,4,3);
        private void BuildCakeFamilyChoices()
        {
            cakeColorChoices=Rect(kitchenCooking,"Rainbow layer colors",Vector2.zero,Vector2.zero);
            var names=new[]{"Red","Orange","Yellow","Green","Blue","Purple"};
            for(var i=0;i<6;i++){
                var pick=i;var label=Button(cakeColorChoices,"Color "+names[i],new Vector2(i<3?-448:448,190-i%3*166),new Vector2(178,143),()=>CakeDetail("colors",pick),Color.white);
                var picture=HomePicture(label.transform.parent,"Colored batter",new Vector2(0,20),new Vector2(120,70),FamilyCakeSprite(4));picture.color=CakeColors[i];picture.material=CakeColorMaterial;
                label.text=names[i];label.fontSize=22;label.rectTransform.anchoredPosition=new Vector2(0,-48);label.rectTransform.sizeDelta=new Vector2(175,36);
            }
            cakeFeatureChoices=Rect(kitchenCooking,"Duck icing shapes",Vector2.zero,Vector2.zero);
            for(var i=0;i<2;i++){
                var pick=i;var label=Button(cakeFeatureChoices,i==0?"Add duck beak":"Add duck eyes",new Vector2(i==0?-448:448,0),new Vector2(178,153),()=>CakeDetail("features",pick),Color.white);
                HomePicture(label.transform.parent,"Icing shape",new Vector2(0,15),new Vector2(124,95),FamilyCakeSprite(8+i)).preserveAspect=true;
                label.text=i==0?"Beak":"Eyes";label.fontSize=24;label.rectTransform.anchoredPosition=new Vector2(0,-55);label.rectTransform.sizeDelta=new Vector2(175,35);
            }
        }
        private void PaintCakeFamilyChoices(FoodDish d)
        {
            if(cakeColorChoices==null)return;
            cakeColorChoices.gameObject.SetActive(CakeFamilies.Active(d) && d.stage=="colors");
            cakeFeatureChoices.gameObject.SetActive(CakeFamilies.Active(d) && d.stage=="features");
        }
        private void CakeDetail(string stage,int choice)
        {
            var d=CurrentCake;if(!CakeFamilies.Active(d) || d.stage!=stage || cakeHelping)return;
            var token=CakeFlow.Token(d);CakeSend(stage,token,choice,0,r=>{if(r.Accepted && CakeToken==token && CakeFlow.Complete(CurrentCake))CakeSend("finish",token,0,0);});
        }
        private static string CakeFamilyInstruction(FoodDish d)
        {
            switch(d.stage){
                case "ingredients":return d.recipe=="CAK-05"?"Add egg, milk and carrot to your cake mix":"Add egg and milk to your cake mix";
                case "chop":return "Slice across the carrot, then chop the pieces smaller";
                case "mix":return "Stir inside the bowl — watch your batter become smooth";
                case "colors":return "Choose a color for bowl "+(CakeFamilies.Colors(d).Length+1)+" — the cake mix includes colors";
                case "pour":return d.recipe=="CAK-03"?"Pour your batter into the heart mold":d.recipe=="CAK-04"?"Pour the three colored bowls into their tins":d.recipe=="CAK-01"?"Fill the big body tin and the little head tin":"Pour your carrot batter into the tin";
                case "bake":return "Bake and watch your cake rise — it stops safely when ready";
                case "stack":return d.recipe=="CAK-01"?"Place the little cake head on the body":"Place the next colored layer onto your cake";
                case "ice":return d.ingredients.Any(a=>a.ingredient=="icing")?"Spread icing across your cake":"Add icing after baking";
                case "features":return "Add the beak and eyes from your icing shapes";
                case "decorate":return d.recipe=="CAK-01"?"Give your duck popcorn feathers":d.recipe=="CAK-03"?"Place strawberries around your heart":"Choose decorations and place them where you like";
                case "cut":return "Cut across, then up and down — four pieces to share";
                default:return "Your cake is ready! Tap a plate to share";
            }
        }
        private static string CakeFamilyAction(FoodDish d)=>d.stage=="chop"?"Chop for me":d.stage=="mix"?"Stir for me":d.stage=="pour"?"Pour for me":d.stage=="bake"?"Bake":d.stage=="stack"?d.recipe=="CAK-01"?"Place head":"Place layer":d.stage=="ice"?"Spread icing":d.stage=="features"?"Add face":d.stage=="decorate"?"Finish decorating":d.stage=="cut"?"Slice cake":"";
        private Sprite FamilyFrostingCell(int spriteIndex,int cell)
        {
            var key="family-icing-"+spriteIndex+"-"+cell;if(kitchenSprites.TryGetValue(key,out var sprite))return sprite;
            var source=FamilyCakeSprite(spriteIndex);var r=source.rect;var w=r.width/3;var h=r.height/3;
            sprite=Sprite.Create(source.texture,new Rect(r.x+cell%3*w,r.y+cell/3*h,w,h),new Vector2(.5f,.5f));homeSprites.Add(sprite);kitchenSprites[key]=sprite;return sprite;
        }
        private void PaintFamilyCake(FoodDish d)
        {
            if(cakeParts==null || !cakeSurface.gameObject.activeSelf)return;
            foreach(var p in cakeParts)p.gameObject.SetActive(false);cakeTool.gameObject.SetActive(false);var at=0;
            Image Part(Sprite s,float x,float y,float w,float h,Color? tint=null){var p=cakeParts[at++];p.gameObject.SetActive(true);p.sprite=s;p.material=tint.HasValue && tint.Value!=Color.white?CakeColorMaterial:null;p.type=Image.Type.Simple;p.preserveAspect=false;p.color=tint??Color.white;p.rectTransform.anchoredPosition=new Vector2(x,y);p.rectTransform.sizeDelta=new Vector2(w,h);p.rectTransform.localEulerAngles=Vector3.zero;return p;}
            var duck=d.recipe=="CAK-01";var heart=d.recipe=="CAK-03";var rainbow=d.recipe=="CAK-04";var carrot=d.recipe=="CAK-05";var colors=CakeFamilies.Colors(d);
            Color Tint(int n)=>rainbow && n<colors.Length?CakeColors[colors[n]]:carrot?new Color(1,.79f,.56f):Color.white;
            void Batter(float x,float y,float w,float h,float amount,int n=0){if(amount>0)Part(FamilyCakeSprite(4),x,y,w*Mathf.Sqrt(amount),h*Mathf.Sqrt(amount),Tint(n));}
            if(d.stage=="ingredients" || d.stage=="mix"){
                Part(CakeSprite(0),0,-12,415,280);Batter(0,45,344,118,.05f+d.mixed*.95f);
                var raw=d.ingredients.Where(a=>a.phase!="decorate").Take(6).ToArray();
                for(var i=0;i<raw.Length;i++){var p=Part(raw[i].ingredient=="carrot" && d.step==3?FamilyCakeSprite(11):KitchenSprite("toppings",raw[i].ingredient=="cake-mix"?9:Array.IndexOf(Kitchen.Ingredients,raw[i].ingredient),5,5),-100+i*45,45+i%2*17,70,64);p.color=new Color(1,1,1,1-d.mixed);}
            }else if(d.stage=="chop"){
                Part(CookingLayer(4),0,-10,450,330);Part(FamilyCakeSprite(d.step==0?10:11),0,25,d.step==0?285:330,200);
                if(d.step==1 || d.step==2)Part(FamilyCakeSprite(10),-95,75,130,100);
            }else if(d.stage=="colors"){
                for(var i=0;i<3;i++){Part(CakeSprite(0),-165+i*165,-18,165,120);Batter(-165+i*165,13,138,52,1,i);}
            }else if(d.stage=="pour" || d.stage=="bake"){
                var count=CakeFamilies.LayerCount(d);var width=count==3?160:count==2?215:310;
                if(d.stage=="pour"){
                    for(var i=0;i<(rainbow?3:1);i++){var x=rainbow?-165+i*165:-90;var w=rainbow?155:200;var bowl=Part(CakeSprite(0),x,135,w,w*.66f);bowl.rectTransform.localEulerAngles=new Vector3(0,0,-d.poured*38);Batter(x,135+w*.13f,w*.78f,w*.265f,1-d.poured,i);}
                    if(d.poured>0 && d.poured<1)Part(FamilyCakeSprite(4),18,30,23,130);
                }else Part(KitchenSprite("oven-closed"),0,5,510,420);
                for(var i=0;i<count;i++){
                    var x=(i-(count-1)/2f)*(width+6);var w=duck && i==1?width*.68f:width;var y=d.stage=="bake"?-40:-115;
                    Part(heart?FamilyCakeSprite(0):CakeSprite(2),x,y,w,heart?210:120);
                    if(d.stage=="pour")Batter(x,y+24,w*.82f,heart?115:48,d.poured,i);
                    else {var rise=(float)(d.heat/Kitchen.HeatSeconds);Part(FamilyCakeSprite(heart?1:duck?i==0?6:7:3),x,y+25+rise*12,w*.86f,35+rise*(heart?120:65),Tint(i));}
                }
            }else{
                Part(CookingLayer(5),0,-120,465,178);
                var count=d.layers;var bodyY=duck?-40:heart?-15:-70;var topY=heart?35:duck?0:bodyY+(count-1)*62+45;
                if(duck){Part(FamilyCakeSprite(6),-35,bodyY,310,187);if(count==2)Part(FamilyCakeSprite(7),94,94,148,139);}
                else for(var i=0;i<count;i++)Part(FamilyCakeSprite(heart?1:3),0,bodyY+i*62,heart?340:340,heart?265:155,Tint(i));
                if(d.stage=="stack"){
                    var pos=cakeHelping?Vector2.Lerp(new Vector2(135,150),duck?new Vector2(94,94):new Vector2(0,bodyY+count*62),Mathf.Clamp01((Time.unscaledTime-cakeAssistStart)/.6f)):cakeMoveTime>Time.unscaledTime?cakeToolPoint:new Vector2(135,150);
                    Part(FamilyCakeSprite(duck?7:3),pos.x,pos.y,duck?140:200,duck?133:95,Tint(count));
                }
                if(d.icingMask>0){
                    var frost=heart?2:5;var fw=heart?330:duck?285:332;var fh=heart?225:duck?124:115;var fx=duck?-35:0;var tint=duck?new Color(1,.89f,.43f):Color.white;
                    for(var cell=0;cell<9;cell++)if((d.icingMask&(1<<cell))!=0)Part(FamilyFrostingCell(frost,cell),fx-fw/3+cell%3*fw/3,topY-fh/3+cell/3*fh/3,fw/3+1,fh/3+1,tint);
                    if(duck && count==2){var head=Part(FamilyCakeSprite(5),94,129,145,85,tint);head.color=new Color(tint.r,tint.g,tint.b,Enumerable.Range(0,9).Count(i=>(d.icingMask&(1<<i))!=0)/9f);}
                }
                if(duck){if((d.step&1)!=0)Part(FamilyCakeSprite(8),169,78,115,75);if((d.step&2)!=0)Part(FamilyCakeSprite(9),110,138,100,50);}
                foreach(var a in d.ingredients.Where(a=>a.phase=="decorate").Take(20))Part(KitchenSprite("toppings",Array.IndexOf(Kitchen.Ingredients,a.ingredient),5,5),a.x*130+(duck?-35:0),topY+a.y*(heart?75:42),53,45);
                if(d.stage=="cut" || d.stage=="serve")for(var bit=0;bit<2;bit++)if((d.cutMask&(1<<bit))!=0)Part(null,0,topY,bit==0?290:4,bit==0?4:heart?180:95,new Color(.37f,.22f,.14f,.65f));
                if(d.portions!=15)foreach(var p in cakeParts.Where(p=>p.gameObject.activeSelf).Skip(1))CakePortion(d,p);
            }
            if(d.stage=="mix" || d.stage=="chop" || d.stage=="ice" || d.stage=="cut"){
                cakeTool.gameObject.SetActive(true);cakeTool.sprite=d.stage=="chop" || d.stage=="cut"?KitchenSprite("utensils",1,2,2):CakeSprite(5);
                var moving=cakeHelping || Time.unscaledTime<cakeDemoUntil;
                cakeTool.rectTransform.anchoredPosition=moving?new Vector2(Mathf.Sin(Time.unscaledTime*3)*110,Mathf.Cos(Time.unscaledTime*3)*38+55):cakeMoveTime>Time.unscaledTime?cakeToolPoint:new Vector2(115,90);
                cakeTool.rectTransform.localEulerAngles=new Vector3(0,0,moving?Mathf.Sin(Time.unscaledTime*3)*18:0);
            }
        }
        private static void CakePortion(FoodDish d,Image p)
        {
            p.type=Image.Type.Filled;p.fillMethod=Image.FillMethod.Radial360;p.fillClockwise=true;p.fillAmount=Enumerable.Range(0,4).Count(i=>(d.portions&(1<<i))!=0)/4f;
            p.fillOrigin=(6-Enumerable.Range(0,4).First(i=>(d.portions&(1<<i))!=0))%4;
        }
        private void PaintSmallFamilyCake(FoodDish d,Image food,Image plate,Image[] extras)
        {
            foreach(var p in extras)p.gameObject.SetActive(false);plate.gameObject.SetActive(true);food.gameObject.SetActive(d.portions!=0);if(d.portions==0)return;
            var heart=d.recipe=="CAK-03";var duck=d.recipe=="CAK-01";var rainbow=d.recipe=="CAK-04";var colors=CakeFamilies.Colors(d);var n=0;
            food.type=Image.Type.Simple;food.material=rainbow && d.heated?CakeColorMaterial:null;food.color=Color.white;food.rectTransform.anchoredPosition=new Vector2(duck?-12:0,22);food.sprite=d.heated?FamilyCakeSprite(heart?1:duck?6:3):CakeSprite(d.stage=="pour" || d.stage=="bake"?2:0);
            Image Layer(Sprite sprite,float x,float y,float w,float h,Color? tint=null){var p=extras[n++];p.gameObject.SetActive(true);p.sprite=sprite;p.material=tint.HasValue && tint.Value!=Color.white?CakeColorMaterial:null;p.type=Image.Type.Simple;p.color=tint??Color.white;p.rectTransform.anchoredPosition=new Vector2(x,y);p.rectTransform.sizeDelta=new Vector2(w,h);p.preserveAspect=false;return p;}
            if(!d.heated){Layer(FamilyCakeSprite(4),0,40,84*Mathf.Sqrt(.1f+d.mixed*.9f),30);return;}
            if(rainbow){food.color=CakeColors[colors[0]];for(var i=1;i<d.layers;i++)Layer(FamilyCakeSprite(3),0,22+i*23,126,60,CakeColors[colors[i]]);}
            if(duck && d.layers==2)Layer(FamilyCakeSprite(7),35,70,54,52);
            var top=heart?39:duck?40:42+(d.layers-1)*23;
            if(d.icingMask>0){var width=heart?119:duck?107:124;var height=heart?81:43;var x=duck?-12:0;var tint=duck?new Color(1,.89f,.43f):Color.white;
                for(var cell=0;cell<9 && n<extras.Length;cell++)if((d.icingMask&(1<<cell))!=0)Layer(FamilyFrostingCell(heart?2:5,cell),x-width/3f+cell%3*width/3f,top-height/3f+cell/3*height/3f,width/3f+1,height/3f+1,tint);
                if(duck && d.layers==2)Layer(FamilyCakeSprite(5),35,83,52,32,tint);
            }
            if(duck){if((d.step&1)!=0)Layer(FamilyCakeSprite(8),62,65,43,29);if((d.step&2)!=0)Layer(FamilyCakeSprite(9),41,85,36,19);}
            foreach(var a in d.ingredients.Where(a=>a.phase=="decorate")){if(n>=extras.Length)break;Layer(KitchenSprite("toppings",Array.IndexOf(Kitchen.Ingredients,a.ingredient),5,5),a.x*48,top+a.y*(heart?26:16),19,16);}
            if(d.portions!=15)foreach(var p in new[]{food}.Concat(extras.Take(n)))CakePortion(d,p);
        }
    }
}
