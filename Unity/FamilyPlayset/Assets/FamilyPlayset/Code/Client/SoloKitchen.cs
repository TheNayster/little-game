using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private readonly Dictionary<string,Sprite> kitchenSprites=new Dictionary<string,Sprite>();
        private readonly Dictionary<string,(RectTransform root,float x,float y)> kitchenFixtures=new Dictionary<string,(RectTransform,float,float)>();
        private readonly Dictionary<string,(Image image,Text count,Image food,Image[] additions)> kitchenItems=new Dictionary<string,(Image,Text,Image,Image[])>();
        private readonly List<GameObject> kitchenDoors=new List<GameObject>();
        private readonly List<(string id,Image image,Text count)> kitchenIngredientButtons=new List<(string,Image,Text)>();
        private RectTransform kitchenPanel,kitchenRecipes,kitchenCooking,kitchenPreview,kitchenDiningFront,kitchenIngredients,kitchenServing;
        private Image kitchenFridge,kitchenOven,kitchenWater,kitchenPreviewFood,kitchenPreviewBase;
        private Image[] kitchenPreviewToppings;
        private Text kitchenTitle,kitchenHint,kitchenStep,kitchenHeat;
        private string cookingItem,kitchenArea;
        private long kitchenPaintRevision=-1;
        private string kitchenMode="Make";
        private bool kitchenReadyBase;
        private Text kitchenReadyBaseText,kitchenWash;
        public bool KitchenOpen=>kitchenPanel!=null && kitchenPanel.gameObject.activeSelf;
        private KitchenState KitchenState=>HasWorld?(Shared?shared.View.kitchen:World.ReadKitchen()):null;
        private Sprite KitchenSprite(string id,int index=-1,int columns=1,int rows=1)
        {
            var key=id+":"+index;if(kitchenSprites.TryGetValue(key,out var sprite))return sprite;
            var texture=homeTextures.FirstOrDefault(t=>t!=null && t.name=="kitchen-"+id);
            if(texture==null){texture=Resources.Load<Texture2D>("Kitchen/"+id);if(texture==null)throw new InvalidOperationException("Missing kitchen art: "+id);texture.name="kitchen-"+id;homeTextures.Add(texture);}
            var rect=index<0?new Rect(0,0,texture.width,texture.height):new Rect(index%columns*(texture.width/(float)columns),(rows-1-index/columns)*(texture.height/(float)rows),texture.width/(float)columns,texture.height/(float)rows);
            // Generated atlas spacing is inspected, not assumed to be uniform.
            // These bounds preserve each full silhouette and exclude neighbours.
            if(index>=0 && id=="recipes"){
                var boxes=new[]{new Rect(10,48,303,254),new Rect(327,48,313,258),new Rect(652,48,312,256),new Rect(976,49,314,256),new Rect(1303,48,306,255),new Rect(14,351,309,267),new Rect(344,336,303,294),new Rect(668,365,294,256),new Rect(980,357,305,270),new Rect(1307,368,298,253),new Rect(11,671,342,253),new Rect(352,687,299,229),new Rect(671,692,290,227),new Rect(969,671,332,250),new Rect(1312,687,288,233)};
                var b=boxes[index];rect=new Rect(b.x/1619*texture.width,(971-b.y-b.height)/971*texture.height,b.width/1619*texture.width,b.height/971*texture.height);
            }else if(index>=0 && id=="toppings"){
                var xs=new[]{25,270,505,762,1005};var xe=new[]{260,495,746,988,1232};var ys=new[]{95,315,540,755,980};var ye=new[]{295,520,738,965,1190};var c=index%5;var r=index/5;
                rect=new Rect(xs[c]/1254f*texture.width,(1254-ye[r])/1254f*texture.height,(xe[c]-xs[c])/1254f*texture.width,(ye[r]-ys[r])/1254f*texture.height);
            }else if(index>=0)rect=new Rect(rect.x+3,rect.y+3,rect.width-6,rect.height-6);
            sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f));homeSprites.Add(sprite);kitchenSprites[key]=sprite;return sprite;
        }
        private Sprite IngredientSprite(string id)=>KitchenSprite("ingredients",Array.IndexOf(Kitchen.Ingredients,id),5,5);
        private RectTransform KitchenFixture(string id,float x,float y,string art,Vector2 offset,Vector2 size)
        {
            var root=Rect(Board,"Kitchen "+id,Vector2.zero,Vector2.zero);kitchenFixtures[id]=(root,x,y);HomePicture(root,id,offset,size,KitchenSprite(art));return root;
        }
        private void KitchenCommand(string operation,string item="",string target="",float x=0,float y=0,Action<SoloResult> complete=null)
        {
            if(!Ready || MenuOpen || TravelPending)return;
            void Done(SoloResult result){
                if(!result.Accepted)RoomPlayFeedback(result.Outcome=="open-ingredient-storage"?"Open the fridge or cupboard for your ingredients.":result.Outcome=="tool-busy"?"That tool is busy. Try another one.":result.Outcome=="food-still-here"?"Enjoy or serve the food before washing.":result.Outcome=="use-running-sink"?"Turn on the tap, then wash at the sink.":result.Outcome=="need-empty-clean-tray"?"Use an empty, clean tray.":"That spot or item is busy. Try again!");
                if(!result.Accepted && KitchenOpen)kitchenHint.text=result.Outcome=="ingredient-unavailable"?"That ingredient is being held. Try a different food.":result.Outcome=="need-empty-clean-tray"?"Every tray has a creation. Go back to continue one.":result.Outcome=="dish-full-or-served"?"This dish is full. You can bake or share it.":result.Outcome=="tool-busy"?"That tool is being used. Try another step.":"That item is being used. Your food is safe.";
                kitchenPaintRevision=-1;if(result.Accepted && kitchenHint!=null)kitchenHint.text="";complete?.Invoke(result);Render();
            }
            if(Shared)SubmitShared(SoloAction.Kitchen,item,target,operation,x,y,Done);else Done(Command(SoloAction.Kitchen,item,target,operation,x,y));
        }
        public void KitchenStepGesture()
        {
            var t=ReadToys().FirstOrDefault(v=>v.id==cookingItem);var d=t?.kitchen?.dish;
            if(d==null || t.kind==ToyKind.Plate){OpenEasyCook();return;}
            var step=Kitchen.Next(d);
            if(step.StartsWith("add:")){AddKitchenIngredient("ingredient-"+step.Substring(4));return;}
            if(step=="serve"){kitchenMode=kitchenMode=="Decorate"?"Serve":"Decorate";PresentKitchen();return;}
            KitchenCommand("easy:"+(step=="heat"?"bake":step),t.id);
        }
        public void AddKitchenIngredient(string id,Vector2? screen=null)
        {
            var all=ReadToys();var t=all.FirstOrDefault(v=>v.id==cookingItem);if(t?.kitchen?.dish==null)return;
            var ingredient=all.FirstOrDefault(v=>v.id==id);if(ingredient==null)return;
            if(ingredient.kitchen.amount==0){KitchenCommand("easy:restock",id);return;}
            var n=t.kitchen.dish.ingredients.Length;var local=new Vector2(Mathf.Sin(n*2.4f)*.65f,Mathf.Cos(n*2.4f)*.65f);
            if(screen.HasValue){RectTransformUtility.ScreenPointToLocalPointInRectangle(kitchenPreview,screen.Value,null,out var point);if(point.magnitude>245)return;local=Vector2.ClampMagnitude(point/210,.78f);}
            KitchenCommand("easy:add",id,t.id,t.x+local.x*100,Mathf.Clamp(t.y+local.y*100,0,500));
        }
        private void OpenKitchenItem(string id)
        {
            var t=ReadToys().FirstOrDefault(v=>v.id==id);if(t?.kitchen==null)return;
            if(t.kind==ToyKind.Ingredient){if(t.kitchen.amount==0)KitchenCommand("easy:restock",id);else RoomPlayFeedback("Drag an ingredient to a tray, or tap a tray to cook.");return;}
            if(t.kind==ToyKind.KitchenTool){RoomPlayFeedback("Drag this tool, or use the large tool button at a worktop.");return;}
            CancelPointers();cookingItem=id;kitchenChoosing=t.kind==ToyKind.Cookware && t.kitchen.dish==null && !t.kitchen.dirty;kitchenArea=CurrentArea;kitchenMode=t.kind==ToyKind.Plate?"Serve":"Make";kitchenPaintRevision=-1;kitchenPage=0;kitchenHint.text="";kitchenPanel.gameObject.SetActive(true);kitchenPanel.SetAsLastSibling();PresentKitchen();
        }
        private void CloseKitchen(){if(kitchenPanel!=null)kitchenPanel.gameObject.SetActive(false);cookingItem=null;}
        private void BuildKitchen()
        {
            if(SceneSchema<Kitchen.Schema)return;
            var counter=KitchenFixture("counter",Kitchen.CounterX,350,"counter",new Vector2(0,120),new Vector2(900,300));
            var closed=KitchenSprite("counter-closed");
            for(var i=0;i<4;i++){
                var index=i;var door=Rect(Board,"Cupboard door "+i,Vector2.zero,Vector2.zero);kitchenFixtures["door-"+i]=(door,Kitchen.CounterX,350);
                var part=Rect(door,"Cream door",new Vector2(0,120),new Vector2(900,300)).gameObject.AddComponent<HomeArtPart>();
                part.Configure(closed.texture,new[]{new HomeArtPart.Polygon{points=Patch(.025f+i*.24f,.35f,.267f+i*.24f,.94f)}});kitchenDoors.Add(door.gameObject);
                HomeHit(counter,"Cupboard handle "+(i+1),new Vector2(-235+i*205,86),new Vector2(76,76),()=>KitchenCommand("door",target:index.ToString()),false);
                HomeHit(counter,"Cook at worktop "+(i+1),new Vector2(Kitchen.X("counter",i)-Kitchen.CounterX,185),new Vector2(160,76),OpenEasyCook,false);
            }
            HomeHit(counter,"Kitchen tap",new Vector2(-315,220),new Vector2(125,85),()=>KitchenCommand("door",target:"water"),false);
            kitchenWater=Panel(counter,"Running water",new Vector2(-307,218),new Vector2(12,65),new Color(.34f,.77f,.95f,.75f));
            Panel(counter,"Utensil rail",new Vector2(-30,310),new Vector2(840,12),new Color(.58f,.39f,.22f));
            var fridge=KitchenFixture("fridge",Kitchen.FridgeX,350,"fridge",new Vector2(0,210),new Vector2(310,465));kitchenFridge=fridge.Find("fridge").GetComponent<Image>();
            HomeHit(fridge,"Fridge handle",new Vector2(117,265),new Vector2(60,180),()=>KitchenCommand("door",target:"fridge"),false);
            var oven=KitchenFixture("oven",Kitchen.OvenX,350,"oven",new Vector2(0,125),new Vector2(320,308));kitchenOven=oven.Find("oven").GetComponent<Image>();
            HomeHit(oven,"Oven handle",new Vector2(0,150),new Vector2(190,55),()=>KitchenCommand("door",target:"oven"),false);
            var table=KitchenFixture("dining",Kitchen.DiningX,130,"dining",new Vector2(0,70),new Vector2(900,300));
            var front=Rect(Board,"Dining table front",Vector2.zero,Vector2.zero);kitchenDiningFront=front;
            var tablePart=Rect(front,"Table top and legs",new Vector2(0,70),new Vector2(900,300)).gameObject.AddComponent<HomeArtPart>();tablePart.Configure(KitchenSprite("dining").texture,new[]{new HomeArtPart.Polygon{points=Patch(0,.27f,1,1)}});
            for(var i=0;i<4;i++){var index=i;HomeHit(table,"Dining seat "+(i+1),new Vector2(Kitchen.SeatX(Kitchen.Seat(i))-Kitchen.DiningX,-20),new Vector2(155,75),()=>UseHome(Kitchen.Seat(index)),false);}
            var cook=Button(counter,"Cook",new Vector2(0,395),new Vector2(180,90),OpenEasyCook,new Color(.81f,.94f,.82f));
            HomePicture(cook.transform.parent,"Pizza invitation",new Vector2(-55,0),new Vector2(55,55),KitchenSprite("recipes",0,5,3)).preserveAspect=true;
            cook.rectTransform.anchoredPosition=new Vector2(28,0);cook.rectTransform.sizeDelta=new Vector2(105,70);
            BuildEasyKitchenPanel();
        }
        private Vector2 KitchenPoint(string group,int slot)
        {
            if(group=="fridge")return ToBoard(Kitchen.X(group,slot),350)+new Vector2(0,320-slot/3*86)*sceneScale;
            if(group=="cupboard")return ToBoard(Kitchen.X(group,slot),350)+new Vector2(0,113-slot/12*99)*sceneScale;
            if(group=="counter")return ToBoard(Kitchen.X(group,slot),350)+new Vector2(0,185)*sceneScale;
            if(group=="oven")return ToBoard(Kitchen.X(group,slot),350)+new Vector2(0,134-slot/2*56)*sceneScale;
            if(group=="dining")return ToBoard(Kitchen.X(group,slot),130)+new Vector2(0,115)*sceneScale;
            if(group=="tools")return ToBoard(Kitchen.X(group,slot),350)+new Vector2(0,280)*sceneScale;
            return ToBoard(Kitchen.X(group,slot),350)+new Vector2(0,235)*sceneScale;
        }
        private bool KitchenToyPoint(SoloToy t,out Vector2 point)
        {point=Vector2.zero;if(!Kitchen.Slot(t.container,out var group,out var slot))return false;point=KitchenPoint(group,slot);return true;}
        private bool KitchenDropPoint(Vector2 raw,out Vector2 point)
        {
            point=raw;if(SceneSchema<Kitchen.Schema || CurrentArea!="garden" || dragging==null)return false;
            var all=ReadToys();var t=all.FirstOrDefault(v=>v.id==dragging);if(t==null)return false;var board=ToBoard(raw.x,raw.y);var distance=70f;var target="";var result=raw;
            void Candidate(string id,Vector2 visual,float x,float y){var d=Vector2.Distance(board,visual)/sceneScale;if(d>=distance)return;distance=d;target=id;result=new Vector2(x,y);}
            foreach(var group in new[]{"fridge","cupboard","counter","oven","dining","sink","tools"})for(var i=0;i<Kitchen.Count(group);i++){
                if(all.Any(v=>v.container==Kitchen.Support(group,i)) || !Kitchen.Open(KitchenState,group,i) || group=="oven" && t.kind!=ToyKind.Cookware || group=="tools" && t.kind!=ToyKind.KitchenTool || group=="dining" && t.kind!=ToyKind.Plate || group=="sink" && !Kitchen.Kind(t.kind))continue;
                Candidate(Kitchen.Support(group,i),KitchenPoint(group,i),Kitchen.X(group,i),Kitchen.Y(group,i));
            }
            foreach(var item in all.Where(v=>v.id!=t.id && v.zone==CurrentArea && VisibleToy(v)))
                if((t.kind==ToyKind.Ingredient || t.kind==ToyKind.KitchenTool) && item.kind==ToyKind.Cookware || t.kind==ToyKind.Cookware && item.kind==ToyKind.Plate)Candidate(item.id,FurnitureToyPoint(item),item.x,item.y);
            if(target=="")return false;bedroomDragTarget=target;point=result;return true;
        }
        private void DrawKitchenItem(SoloToy t,RectTransform root)
        {
            if(t.kitchen==null)return;Sprite sprite=null;
            if(t.kind==ToyKind.Ingredient)sprite=IngredientSprite(t.kitchen.definition);
            if(t.kind==ToyKind.KitchenTool)sprite=KitchenSprite("utensils",Array.IndexOf(Kitchen.Tools,t.kitchen.definition),2,2);
            var baseImage=sprite==null?Panel(root,"Reusable dish",Vector2.zero,new Vector2(130,65),t.kind==ToyKind.Plate?Cream:new Color(.65f,.75f,.81f),false,true):HomePicture(root,"Kitchen item",Vector2.zero,new Vector2(83,83),sprite);
            baseImage.preserveAspect=sprite!=null;
            if(sprite==null)Panel(baseImage.transform,"Dish interior",Vector2.zero,new Vector2(110,48),t.kind==ToyKind.Plate?Color.white:new Color(.8f,.87f,.9f),false,true).preserveAspect=false;
            var food=HomePicture(root,"Real food",new Vector2(0,22),new Vector2(126,93),KitchenSprite("recipes",0,5,3));food.preserveAspect=true;
            var additions=Kitchen.Dish(t.kind)?Enumerable.Range(0,36).Select(i=>HomePicture(root,"Topping "+i,Vector2.zero,new Vector2(19,19),IngredientSprite("cheese"))).ToArray():Array.Empty<Image>();
            var count=Label(root,"",16,new Vector2(0,-42),new Vector2(124,30));kitchenItems[t.id]=(baseImage,count,food,additions);
        }
        private void PaintDish(FoodDish dish,Image image,Image baseImage,Image[] additions,float scale=1)
        {
            var visible=dish!=null && dish.portions!=0;var pizza=visible && dish.recipe.StartsWith("PIZ");
            image.gameObject.SetActive(visible && (pizza || dish.heated));baseImage.gameObject.SetActive(true);
            if(pizza && scale>1)baseImage.gameObject.SetActive(false);
            image.rectTransform.anchoredPosition=scale>1?Vector2.zero:new Vector2(0,10);
            if(visible && (dish.portions&(dish.portions-1))==0){var bit=Enumerable.Range(0,4).First(i=>(dish.portions&(1<<i))!=0);var r=Mathf.Min(image.rectTransform.sizeDelta.x,image.rectTransform.sizeDelta.y)*.21f;image.rectTransform.anchoredPosition-=new Vector2(bit<2?r:-r,bit==0 || bit==3?r:-r);}
            // Unity's origins run Bottom, Right, Top, Left; portion bits run clockwise from top-right.
            void Portion(Image picture){picture.type=Image.Type.Filled;picture.fillMethod=Image.FillMethod.Radial360;picture.fillClockwise=true;picture.fillAmount=Enumerable.Range(0,4).Count(i=>(dish.portions&(1<<i))!=0)/4f;picture.fillOrigin=(6-Enumerable.Range(0,4).First(i=>(dish.portions&(1<<i))!=0))%4;}
            if(visible){image.sprite=pizza?CookingLayer(dish.heated?1:0):KitchenSprite("recipes",Kitchen.Recipe(dish.recipe).art,5,3);image.color=Color.white;image.type=Image.Type.Simple;if(pizza || dish.recipe.StartsWith("CAK"))Portion(image);}
            foreach(var a in additions)a.gameObject.SetActive(false);if(!visible)return;
            var at=0;
            if(pizza)foreach(var name in new[]{"sauce","cheese"}){
                if(!dish.ingredients.Any(a=>a.ingredient==name))continue;
                if(name=="cheese" && !dish.heated){
                    for(var n=0;n<9;n++){var shreds=additions[at++];shreds.gameObject.SetActive(true);shreds.type=Image.Type.Simple;shreds.sprite=KitchenSprite("toppings",2,5,5);shreds.preserveAspect=true;var radius=Mathf.Min(image.rectTransform.sizeDelta.x,image.rectTransform.sizeDelta.y)*.25f;var angle=n*2.4f;shreds.rectTransform.anchoredPosition=image.rectTransform.anchoredPosition+new Vector2(Mathf.Sin(angle),Mathf.Cos(angle))*radius*(n==0?0:.55f+n*.045f);shreds.rectTransform.sizeDelta=new Vector2(28,28)*scale;}
                    continue;
                }
                var layer=additions[at++];layer.gameObject.SetActive(true);layer.sprite=CookingLayer(name=="sauce"?2:3);layer.preserveAspect=true;
                layer.rectTransform.anchoredPosition=image.rectTransform.anchoredPosition;layer.rectTransform.sizeDelta=image.rectTransform.sizeDelta*.79f;Portion(layer);
            }
            var seenSauce=false;var seenCheese=false;
            for(var i=1;i<dish.ingredients.Length && at<additions.Length;i++){
                var add=dish.ingredients[i];if(pizza && add.ingredient=="sauce" && !seenSauce){seenSauce=true;continue;}if(pizza && add.ingredient=="cheese" && !seenCheese){seenCheese=true;continue;}
                var quadrant=add.x>=0?(add.y>=0?0:1):(add.y<0?2:3);if((dish.portions&(1<<quadrant))==0)continue;
                var part=additions[at++];part.gameObject.SetActive(true);part.type=Image.Type.Simple;part.sprite=KitchenSprite("toppings",Array.IndexOf(Kitchen.Ingredients,add.ingredient),5,5);part.preserveAspect=true;
                var radius=Mathf.Min(image.rectTransform.sizeDelta.x,image.rectTransform.sizeDelta.y)*.37f;
                part.rectTransform.anchoredPosition=image.rectTransform.anchoredPosition+Vector2.ClampMagnitude(new Vector2(add.x,add.y),.82f)*radius;
                part.rectTransform.sizeDelta=new Vector2(26,26)*scale;
            }
        }
        private void PresentKitchen()
        {
            if(kitchenPanel==null)return;var all=AllToys();var state=KitchenState;var visible=CurrentArea=="garden";
            foreach(var pair in kitchenFixtures){var f=pair.Value;f.root.gameObject.SetActive(visible);f.root.anchoredPosition=ToBoard(f.x,f.y);f.root.localScale=Vector3.one*sceneScale;}
            for(var i=0;i<4;i++)kitchenDoors[i].SetActive(visible && !state.cupboards[i]);
            kitchenFridge.sprite=KitchenSprite(state.fridgeOpen?"fridge":"fridge-closed");kitchenOven.sprite=KitchenSprite(state.ovenOpen?"oven":"oven-closed");kitchenWater.gameObject.SetActive(state.waterOn);
            kitchenFridge.rectTransform.sizeDelta=state.fridgeOpen?new Vector2(310,465):new Vector2(260,440);kitchenFridge.rectTransform.anchoredPosition=state.fridgeOpen?new Vector2(0,210):new Vector2(-40,218);
            kitchenDiningFront.gameObject.SetActive(visible);kitchenDiningFront.anchoredPosition=ToBoard(Kitchen.DiningX,130);kitchenDiningFront.localScale=Vector3.one*sceneScale;
            foreach(var t in all)if(t.id!=dragging && t.zone==CurrentArea && KitchenToyPoint(t,out var point))toys[t.id].anchoredPosition=point;
            // The chair seat is elevated above its floor anchor. Keep bodies
            // behind the table front, with faces clearly above their plates.
            void Seat(SoloPlayer p,RectTransform root){if(Kitchen.Seat(p.fixture))root.anchoredPosition=ToBoard(p.x,p.y)+new Vector2(0,95)*sceneScale;}
            Seat(ReadPlayer(Actor),avatar);foreach(var friend in friends)if(friend.Value.root.gameObject.activeSelf)Seat(ReadPlayer(friend.Key),friend.Value.root);
            SortDepth();
            if(KitchenOpen && (kitchenArea!=CurrentArea || MenuOpen || BookOpen || TravelPending || applicationPaused))CloseKitchen();
            var revision=Shared?shared.View.revision:World.Revision;if(kitchenPaintRevision==revision && !KitchenOpen)return;kitchenPaintRevision=revision;
            foreach(var t in all.Where(t=>t.kitchen!=null)){
                var view=kitchenItems[t.id];if(Kitchen.Dish(t.kind)){PaintDish(t.kitchen.dish,view.food,view.image,view.additions);view.count.text=t.kitchen.dish==null?(t.kitchen.dirty?"Wash me":""):t.kitchen.dish.portions==0?"Empty":Kitchen.Next(t.kitchen.dish)=="heat"?"Cooking…":"";}
                else {view.food.gameObject.SetActive(false);view.count.text=t.kind==ToyKind.Ingredient?t.kitchen.amount.ToString():"";}
            }
            PresentEasyKitchen(all,visible);
        }
        private void AddKitchenDepth(Action<RectTransform,float,int,string> add)
        {
            foreach(var pair in kitchenFixtures){var f=pair.Value;add(f.root,ToBoard(f.x,f.y).y,pair.Key.StartsWith("door-")?2:0,"kitchen-"+pair.Key);}
            if(kitchenDiningFront!=null)add(kitchenDiningFront,ToBoard(Kitchen.DiningX,130).y,2,"kitchen-dining-front");
        }
        private bool KitchenDepth(SoloToy t,RectTransform rect,Action<RectTransform,float,int,string> add)
        {
            if(!Kitchen.Slot(t.container,out var group,out var slot))return false;
            add(rect,ToBoard(t.x,group=="dining"?130:350).y,group=="counter" || group=="dining" || group=="sink" || group=="tools"?3:1,t.id);return true;
        }
        private void ResetKitchen(){kitchenSprites.Clear();kitchenFixtures.Clear();kitchenItems.Clear();kitchenDoors.Clear();kitchenIngredientButtons.Clear();recipeCards.Clear();plateCards.Clear();occupiedCards.Clear();kitchenPanel=null;kitchenDiningFront=null;cookingItem=null;kitchenPaintRevision=-1;}
    }
}
