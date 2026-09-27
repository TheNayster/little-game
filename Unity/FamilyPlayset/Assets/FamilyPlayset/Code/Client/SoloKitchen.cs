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
        private RectTransform kitchenPanel,kitchenRecipes,kitchenCooking,kitchenPreview,kitchenDiningFront,kitchenIngredients,kitchenServing,kitchenAppliances;
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
                kitchenPaintRevision=-1;complete?.Invoke(result);Render();
            }
            if(Shared)SubmitShared(SoloAction.Kitchen,item,target,operation,x,y,Done);else Done(Command(SoloAction.Kitchen,item,target,operation,x,y));
        }
        private void MoveKitchenDish(string group,Action after=null)
        {
            var t=ReadToys().FirstOrDefault(v=>v.id==cookingItem);if(t==null)return;
            var slot=Enumerable.Range(0,Kitchen.Count(group)).Where(i=>!ReadToys().Any(v=>v.container==Kitchen.Support(group,i))).DefaultIfEmpty(-1).First();
            if(slot<0){RoomPlayFeedback("Those places are full. Your dish is safe.");return;}
            void Dropped(SoloResult result){if(result.Accepted)after?.Invoke();else {RoomPlayFeedback("Open the oven or choose a free place.");if(Shared)SubmitShared(SoloAction.CancelGrab,t.id,"","",0,0,_=>Render());else Command(SoloAction.CancelGrab,t.id);}kitchenPaintRevision=-1;Render();}
            void Grabbed(SoloResult result){
                if(!result.Accepted){RoomPlayFeedback("That dish is being used.");return;}
                if(Shared)SubmitShared(SoloAction.Drop,t.id,Kitchen.Support(group,slot),"",Kitchen.X(group,slot),Kitchen.Y(group,slot),Dropped);
                else Dropped(Command(SoloAction.Drop,t.id,Kitchen.Support(group,slot),x:Kitchen.X(group,slot),y:Kitchen.Y(group,slot)));
            }
            if(t.holder==Actor)Grabbed(new SoloResult(true,"held",0));
            else if(Shared)SubmitShared(SoloAction.Grab,t.id,"","",0,0,Grabbed);else Grabbed(Command(SoloAction.Grab,t.id));
        }
        public void KitchenStepGesture()
        {
            var t=ReadToys().FirstOrDefault(v=>v.id==cookingItem);if(t?.kitchen?.dish==null)return;
            var step=Kitchen.Next(t.kitchen.dish);
            if(step.StartsWith("add:")){AddKitchenIngredient("ingredient-"+step.Substring(4));return;}
            if(step=="heat"){MoveKitchenDish("oven");return;}
            if(step!="serve")KitchenCommand(step,t.id);
        }
        public void AddKitchenIngredient(string id,Vector2? screen=null)
        {
            var t=ReadToys().FirstOrDefault(v=>v.id==cookingItem);if(t?.kitchen?.dish==null)return;
            var n=t.kitchen.dish.ingredients.Length;var local=new Vector2(Mathf.Sin(n*2.4f)*.7f,Mathf.Cos(n*2.4f)*.6f);
            if(screen.HasValue){RectTransformUtility.ScreenPointToLocalPointInRectangle(kitchenPreview,screen.Value,null,out var p);if(Mathf.Abs(p.x)>180 || Mathf.Abs(p.y)>130)return;local=new Vector2(p.x/180,p.y/130);}
            KitchenCommand("add",id,t.id,t.x+local.x*70,Mathf.Clamp(t.y+local.y*50,0,500));
        }
        private void OpenKitchenItem(string id)
        {
            var t=ReadToys().FirstOrDefault(v=>v.id==id);if(t?.kitchen==null)return;
            if(t.kind==ToyKind.Ingredient){if(t.kitchen.amount==0)KitchenCommand("restock",id);else RoomPlayFeedback("Drag an ingredient to a tray, or tap a tray to cook.");return;}
            if(t.kind==ToyKind.KitchenTool){RoomPlayFeedback("Drag this tool, or use the large tool button at a worktop.");return;}
            CancelPointers();cookingItem=id;kitchenArea=CurrentArea;kitchenMode=t.kind==ToyKind.Plate?"Serve":"Make";kitchenPaintRevision=-1;kitchenPanel.gameObject.SetActive(true);kitchenPanel.SetAsLastSibling();PresentKitchen();
        }
        private void CloseKitchen(){if(kitchenPanel!=null)kitchenPanel.gameObject.SetActive(false);cookingItem=null;}
        private void BuildKitchen()
        {
            if(SceneSchema<Kitchen.Schema)return;
            var counter=KitchenFixture("counter",-1780,350,"counter",new Vector2(0,120),new Vector2(1250,417));
            var closed=KitchenSprite("counter-closed");
            for(var i=0;i<4;i++){
                var index=i;var door=Rect(Board,"Cupboard door "+i,Vector2.zero,Vector2.zero);kitchenFixtures["door-"+i]=(door,-1780,350);
                var part=Rect(door,"Cream door",new Vector2(0,120),new Vector2(1250,417)).gameObject.AddComponent<HomeArtPart>();
                part.Configure(closed.texture,new[]{new HomeArtPart.Polygon{points=Patch(.025f+i*.24f,.35f,.267f+i*.24f,.94f)}});kitchenDoors.Add(door.gameObject);
                HomeHit(counter,"Cupboard handle "+(i+1),new Vector2(-328+i*288,86),new Vector2(65,60),()=>KitchenCommand("door",target:index.ToString()),false);
                HomeHit(counter,"Cook at worktop "+(i+1),new Vector2(Kitchen.X("counter",i)+1780,238),new Vector2(175,55),()=>OpenKitchenItem("cookware-"+index),false);
            }
            HomeHit(counter,"Kitchen tap",new Vector2(-435,250),new Vector2(125,85),()=>KitchenCommand("door",target:"water"),false);
            kitchenWater=Panel(counter,"Running water",new Vector2(-428,248),new Vector2(12,65),new Color(.34f,.77f,.95f,.75f));
            Panel(counter,"Utensil rail",new Vector2(15,350),new Vector2(1010,12),new Color(.58f,.39f,.22f));
            var fridge=KitchenFixture("fridge",-960,350,"fridge",new Vector2(0,210),new Vector2(310,465));kitchenFridge=fridge.Find("fridge").GetComponent<Image>();
            HomeHit(fridge,"Fridge handle",new Vector2(117,265),new Vector2(60,180),()=>KitchenCommand("door",target:"fridge"),false);
            var oven=KitchenFixture("oven",-565,350,"oven",new Vector2(0,125),new Vector2(320,308));kitchenOven=oven.Find("oven").GetComponent<Image>();
            HomeHit(oven,"Oven handle",new Vector2(0,150),new Vector2(190,55),()=>KitchenCommand("door",target:"oven"),false);
            var table=KitchenFixture("dining",-670,130,"dining",new Vector2(0,70),new Vector2(1140,380));
            var front=Rect(Board,"Dining table front",Vector2.zero,Vector2.zero);kitchenDiningFront=front;
            var tablePart=Rect(front,"Table top and legs",new Vector2(0,70),new Vector2(1140,380)).gameObject.AddComponent<HomeArtPart>();tablePart.Configure(KitchenSprite("dining").texture,new[]{new HomeArtPart.Polygon{points=Patch(0,.27f,1,1)}});
            for(var i=0;i<4;i++){var index=i;HomeHit(table,"Dining seat "+(i+1),new Vector2(Kitchen.SeatX(Kitchen.Seat(i))+670,-20),new Vector2(155,75),()=>UseHome(Kitchen.Seat(index)),false);}
            BuildKitchenPanel();
        }
        private void BuildKitchenPanel()
        {
            kitchenPanel=Panel(safe,"Kitchen worktop",Vector2.zero,new Vector2(1060,700),new Color(1,.97f,.87f,.99f)).rectTransform;
            kitchenTitle=Label(kitchenPanel,"Let's cook",31,new Vector2(0,304),new Vector2(750,60));
            Button(kitchenPanel,"Close",new Vector2(454,304),new Vector2(135,60),CloseKitchen,Cream);
            foreach(var name in new[]{"Make","Decorate","Serve"}){
                var index=Array.IndexOf(new[]{"Make","Decorate","Serve"},name);var label=name;
                Button(kitchenPanel,name,new Vector2(-330+index*330,237),new Vector2(270,56),()=>{kitchenMode=label;PresentKitchen();},new Color(.85f,.93f,.89f));
            }
            kitchenRecipes=Rect(kitchenPanel,"Recipe pictures",Vector2.zero,Vector2.zero);
            for(var i=0;i<Kitchen.Recipes.Length;i++){
                var recipe=Kitchen.Recipes[i];var button=Button(kitchenRecipes,"",new Vector2(-410+i%5*205,115-i/5*150),new Vector2(190,140),()=>KitchenCommand(kitchenReadyBase?"readybase":"start",cookingItem,recipe.id),Color.white);
                var parent=button.transform.parent;parent.name="Recipe "+recipe.id;
                HomePicture(parent,recipe.title,new Vector2(0,16),new Vector2(132,90),KitchenSprite("recipes",i,5,3)).preserveAspect=true;
                Label(parent,recipe.title,18,new Vector2(0,-49),new Vector2(175,42));
            }
            kitchenReadyBaseText=Button(kitchenRecipes,"Ready base",new Vector2(0,-282),new Vector2(430,45),()=>{kitchenReadyBase=!kitchenReadyBase;kitchenReadyBaseText.text=kitchenReadyBase?"Ready base: on":"Ready base: off";},Cream);kitchenReadyBaseText.text="Ready base: off";
            kitchenCooking=Rect(kitchenPanel,"Food preparation",Vector2.zero,Vector2.zero);
            kitchenPreview=Panel(kitchenCooking,"Food work surface",new Vector2(-300,15),new Vector2(370,275),new Color(.91f,.79f,.58f),true).rectTransform;
            kitchenPreview.gameObject.AddComponent<Button>().targetGraphic=kitchenPreview.GetComponent<Image>();
            kitchenPreview.gameObject.AddComponent<KitchenGesture>().Screen=this;
            kitchenPreviewBase=Panel(kitchenPreview,"Mixing bowl",Vector2.zero,new Vector2(300,140),new Color(.96f,.9f,.63f),false,true);
            kitchenPreviewFood=HomePicture(kitchenPreview,"Dish",new Vector2(0,15),new Vector2(330,245),KitchenSprite("recipes",0,5,3));kitchenPreviewFood.preserveAspect=true;
            kitchenPreviewToppings=Enumerable.Range(0,24).Select(i=>HomePicture(kitchenPreview,"Added ingredient "+i,Vector2.zero,new Vector2(48,48),IngredientSprite("cheese"))).ToArray();
            kitchenStep=Button(kitchenCooking,"Mix",new Vector2(-300,-178),new Vector2(330,66),KitchenStepGesture,new Color(.76f,.88f,.98f));
            kitchenHeat=Label(kitchenCooking,"",23,new Vector2(-300,-230),new Vector2(360,45));
            Button(kitchenCooking,"Taste",new Vector2(-408,-292),new Vector2(145,60),()=>KitchenCommand("taste",cookingItem),Cream);
            kitchenWash=Button(kitchenCooking,"Wash",new Vector2(-233,-292),new Vector2(145,60),()=>MoveKitchenDish("sink",()=>KitchenCommand("wash",cookingItem)),Cream);
            kitchenIngredients=Rect(kitchenCooking,"Ingredients to add",Vector2.zero,Vector2.zero);
            kitchenServing=Rect(kitchenCooking,"Clean plates to serve",Vector2.zero,Vector2.zero);
            Label(kitchenServing,"One dish makes four portions.\nChoose a clean plate for each friend.",25,new Vector2(210,115),new Vector2(480,110));
            for(var i=0;i<Kitchen.Ingredients.Length;i++){
                var id="ingredient-"+Kitchen.Ingredients[i];var root=Panel(kitchenIngredients,"Add "+Kitchen.Ingredients[i],new Vector2(12+i%5*95,125-i/5*86),new Vector2(86,80),Color.white,true).rectTransform;
                root.gameObject.AddComponent<Button>().targetGraphic=root.GetComponent<Image>();
                var icon=HomePicture(root,"Ingredient",new Vector2(0,5),new Vector2(58,58),IngredientSprite(Kitchen.Ingredients[i]));icon.preserveAspect=true;
                var count=Label(root,"",15,new Vector2(0,-27),new Vector2(82,24));var gesture=root.gameObject.AddComponent<KitchenGesture>();gesture.Screen=this;gesture.Ingredient=id;kitchenIngredientButtons.Add((id,icon,count));
            }
            for(var i=0;i<8;i++){
                var id="plate-"+i;Button(kitchenServing,"Plate "+(i+1),new Vector2(5+i%4*125,-20-i/4*85),new Vector2(117,70),()=>KitchenCommand("serve",cookingItem,id),new Color(.83f,.94f,.93f));
            }
            kitchenHint=Label(kitchenPanel,"",21,new Vector2(0,190),new Vector2(980,40));
            kitchenAppliances=Rect(kitchenPanel,"Appliance controls",Vector2.zero,Vector2.zero);
            Button(kitchenAppliances,"Fridge",new Vector2(-360,-332),new Vector2(160,38),()=>KitchenCommand("door",target:"fridge"),Cream);
            Button(kitchenAppliances,"Cupboards",new Vector2(-160,-332),new Vector2(190,38),()=>ToggleKitchenCupboards(),Cream);
            Button(kitchenAppliances,"Oven door",new Vector2(80,-332),new Vector2(190,38),()=>KitchenCommand("door",target:"oven"),Cream);
            Button(kitchenAppliances,"Tap water",new Vector2(320,-332),new Vector2(190,38),()=>KitchenCommand("door",target:"water"),Cream);
            kitchenPanel.gameObject.SetActive(false);
        }
        private void ToggleKitchenCupboards(int index=0)
        {if(index>=4)return;if(KitchenState.cupboards[index])ToggleKitchenCupboards(index+1);else KitchenCommand("door",target:index.ToString(),complete:r=>{if(r.Accepted)ToggleKitchenCupboards(index+1);});}
        private Vector2 KitchenPoint(string group,int slot)
        {
            if(group=="fridge")return ToBoard(Kitchen.X(group,slot),350)+new Vector2(0,320-slot/3*86)*sceneScale;
            if(group=="cupboard")return ToBoard(Kitchen.X(group,slot),350)+new Vector2(0,113-slot/12*99)*sceneScale;
            if(group=="counter")return ToBoard(Kitchen.X(group,slot),350)+new Vector2(0,235)*sceneScale;
            if(group=="oven")return ToBoard(Kitchen.X(group,slot),350)+new Vector2(0,134-slot/2*56)*sceneScale;
            if(group=="dining")return ToBoard(Kitchen.X(group,slot),130)+new Vector2(0,130)*sceneScale;
            if(group=="tools")return ToBoard(Kitchen.X(group,slot),350)+new Vector2(0,325)*sceneScale;
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
            var additions=Kitchen.Dish(t.kind)?Enumerable.Range(0,24).Select(i=>HomePicture(root,"Topping "+i,Vector2.zero,new Vector2(19,19),IngredientSprite("cheese"))).ToArray():Array.Empty<Image>();
            var count=Label(root,"",16,new Vector2(0,-42),new Vector2(124,30));kitchenItems[t.id]=(baseImage,count,food,additions);
        }
        private void PaintDish(FoodDish dish,Image image,Image baseImage,Image[] additions,float scale=1)
        {
            var visible=dish!=null && dish.portions!=0;image.gameObject.SetActive(visible && dish.heated);baseImage.gameObject.SetActive(true);
            if(visible){image.sprite=KitchenSprite("recipes",Kitchen.Recipe(dish.recipe).art,5,3);image.color=Color.white;var portions=Kitchen.Next(dish)=="serve" && (dish.recipe.StartsWith("PIZ") || dish.recipe.StartsWith("CAK"));image.type=portions?Image.Type.Filled:Image.Type.Simple;image.fillMethod=Image.FillMethod.Radial360;image.fillClockwise=true;image.fillAmount=Enumerable.Range(0,4).Count(i=>(dish.portions&(1<<i))!=0)/4f;image.fillOrigin=(2+Enumerable.Range(0,4).First(i=>(dish.portions&(1<<i))!=0))%4;}
            for(var i=0;i<additions.Length;i++){
                var active=visible && i>0 && i<dish.ingredients.Length;
                if(active && dish.heated){var a=dish.ingredients[i];var quadrant=a.x>=0?(a.y>=0?0:1):(a.y<0?2:3);active=(dish.portions&(1<<quadrant))!=0;
                    if(Kitchen.Recipe(dish.recipe).toppings.Contains(a.ingredient) && Array.FindIndex(dish.ingredients,v=>v.ingredient==a.ingredient)==i)active=false;}
                additions[i].gameObject.SetActive(active);if(!active)continue;
                var add=dish.ingredients[i];additions[i].sprite=KitchenSprite("toppings",Array.IndexOf(Kitchen.Ingredients,add.ingredient),5,5);additions[i].preserveAspect=true;additions[i].rectTransform.anchoredPosition=new Vector2(add.x*43,add.y*24+22)*scale;
            }
        }
        private void PresentKitchen()
        {
            if(kitchenPanel==null)return;var all=AllToys();var state=KitchenState;var visible=CurrentArea=="garden";
            foreach(var pair in kitchenFixtures){var f=pair.Value;f.root.gameObject.SetActive(visible);f.root.anchoredPosition=ToBoard(f.x,f.y);f.root.localScale=Vector3.one*sceneScale;}
            for(var i=0;i<4;i++)kitchenDoors[i].SetActive(visible && !state.cupboards[i]);
            kitchenFridge.sprite=KitchenSprite(state.fridgeOpen?"fridge":"fridge-closed");kitchenOven.sprite=KitchenSprite(state.ovenOpen?"oven":"oven-closed");kitchenWater.gameObject.SetActive(state.waterOn);
            kitchenFridge.rectTransform.sizeDelta=state.fridgeOpen?new Vector2(310,465):new Vector2(260,440);kitchenFridge.rectTransform.anchoredPosition=state.fridgeOpen?new Vector2(0,210):new Vector2(-40,218);
            kitchenDiningFront.gameObject.SetActive(visible);kitchenDiningFront.anchoredPosition=ToBoard(-670,130);kitchenDiningFront.localScale=Vector3.one*sceneScale;
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
            if(!KitchenOpen)return;var current=all.FirstOrDefault(t=>t.id==cookingItem);if(current==null){CloseKitchen();return;}
            var dish=current.kitchen.dish;var choose=dish==null && current.kind==ToyKind.Cookware && !current.kitchen.dirty;
            kitchenRecipes.gameObject.SetActive(choose);kitchenCooking.gameObject.SetActive(!choose);kitchenTitle.text=dish==null?choose?"Choose something to make":"Your plate":Kitchen.Recipe(dish.recipe).title;
            kitchenHint.text=choose?"Open the ingredients, then choose a picture.":kitchenMode=="Serve"?"Serve onto plates, carry to friends, taste, then wash.":kitchenMode=="Decorate"?"Tap ingredients or drag them onto your food.":"Add ingredients and use the next tool. Your friends can help.";
            kitchenIngredients.gameObject.SetActive(kitchenMode!="Serve");kitchenServing.gameObject.SetActive(kitchenMode=="Serve" && current.kind==ToyKind.Cookware);
            kitchenAppliances.gameObject.SetActive(visible);kitchenWash.transform.parent.gameObject.SetActive(visible);
            if(!visible)kitchenHint.text="Tap Taste to enjoy. Bring the dish back to the kitchen to wash.";
            if(choose)return;PaintDish(dish,kitchenPreviewFood,kitchenPreviewBase,kitchenPreviewToppings,3);
            kitchenPreviewBase.color=dish?.heated==true?Cream:new Color(.96f,.9f,.63f);
            var step=Kitchen.Next(dish);kitchenStep.text=step=="heat"?"Put in oven":step=="serve"?"Ready to serve":step=="choose"?"Choose a tray":System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(step.Replace('-',' ').Replace(':',' '));
            kitchenStep.transform.parent.GetComponent<Button>().interactable=dish!=null && step!="serve";
            kitchenHeat.text=dish!=null && step=="heat"?"Gentle bake  "+Mathf.CeilToInt((float)(Kitchen.HeatSeconds-dish.heat))+" seconds":dish?.portions==0?"All enjoyed — time to wash!":"";
            foreach(var b in kitchenIngredientButtons){var ingredient=all.First(t=>t.id==b.id);var open=!Kitchen.Slot(ingredient.container,out var group,out var slot) || Kitchen.Open(state,group,slot);b.image.color=open?Color.white:new Color(.6f,.6f,.6f,.5f);b.count.text=open?ingredient.kitchen.amount.ToString():"Open door";}
        }
        private void AddKitchenDepth(Action<RectTransform,float,int,string> add)
        {
            foreach(var pair in kitchenFixtures){var f=pair.Value;add(f.root,ToBoard(f.x,f.y).y,pair.Key.StartsWith("door-")?2:0,"kitchen-"+pair.Key);}
            if(kitchenDiningFront!=null)add(kitchenDiningFront,ToBoard(-670,130).y,2,"kitchen-dining-front");
        }
        private bool KitchenDepth(SoloToy t,RectTransform rect,Action<RectTransform,float,int,string> add)
        {
            if(!Kitchen.Slot(t.container,out var group,out var slot))return false;
            add(rect,ToBoard(t.x,group=="dining"?130:350).y,group=="counter" || group=="dining" || group=="sink" || group=="tools"?3:1,t.id);return true;
        }
        private void ResetKitchen(){kitchenSprites.Clear();kitchenFixtures.Clear();kitchenItems.Clear();kitchenDoors.Clear();kitchenIngredientButtons.Clear();kitchenPanel=null;kitchenDiningFront=null;cookingItem=null;kitchenPaintRevision=-1;}
    }
}
