using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private int kitchenFamily,kitchenPage;
        private bool kitchenChoosing;
        private bool kitchenExperiment;
        private Text kitchenExperimentText;
        private Text kitchenMore,kitchenPrevious,kitchenUndo,kitchenTaste,kitchenBack,kitchenStage;
        private Image kitchenActionIcon;
        private RectTransform kitchenOccupied;
        private readonly System.Collections.Generic.List<RectTransform> recipeCards=new System.Collections.Generic.List<RectTransform>();
        private readonly System.Collections.Generic.List<Text> plateCards=new System.Collections.Generic.List<Text>();
        private readonly System.Collections.Generic.List<Text> occupiedCards=new System.Collections.Generic.List<Text>();
        private Sprite CookingLayer(int index)=>KitchenSprite("cooking-layers",index,3,2);

        private void StartEasyRecipe(string recipe)
        {
            // The authority chooses a free tray when the recipe is committed,
            // rather than letting four open pickers race for the same preview.
            var previous=ReadToys().Where(t=>t.kitchen?.dish!=null).Select(t=>t.kitchen.dish.id).ToArray();var id="";
            KitchenCommand(kitchenExperiment?(kitchenReadyBase?"easy:readyexperiment":"easy:experiment"):kitchenReadyBase?"easy:readybase":"easy:start",id,recipe,complete:r=>{
                if(!r.Accepted)return;
                var tray=ReadToys().FirstOrDefault(t=>t.kind==ToyKind.Cookware && t.kitchen.cook==Actor && t.kitchen.dish?.recipe==recipe && !previous.Contains(t.kitchen.dish.id) && t.zone==CurrentArea);
                if(tray!=null){cookingItem=tray.id;kitchenChoosing=false;kitchenMode="Make";kitchenPage=0;}
            });
        }

        private void OpenEasyCook()
        {
            if(CurrentArea!="garden")return;
            var all=ReadToys();
            var mine=all.FirstOrDefault(t=>t.kind==ToyKind.Cookware && t.zone==CurrentArea && (t.holder=="" || t.holder==Actor) && t.kitchen.cook==Actor && t.kitchen.dish!=null);
            var empty=all.FirstOrDefault(t=>t.kind==ToyKind.Cookware && t.zone==CurrentArea && (t.holder=="" || t.holder==Actor) && t.kitchen.dish==null && !t.kitchen.dirty && (string.IsNullOrEmpty(t.kitchen.cook) || t.kitchen.cook==Actor));
            CancelPointers();cookingItem=mine?.id;kitchenChoosing=mine==null && empty!=null;kitchenArea=CurrentArea;kitchenMode="Make";kitchenPage=0;
            kitchenPanel.gameObject.SetActive(true);kitchenPanel.SetAsLastSibling();PresentKitchen();
        }

        private void BuildEasyKitchenPanel()
        {
            kitchenPanel=Panel(safe,"Kitchen worktop",Vector2.zero,Vector2.zero,new Color(1,.97f,.88f,1),true).rectTransform;
            kitchenPanel.anchorMin=Vector2.zero;kitchenPanel.anchorMax=Vector2.one;kitchenPanel.offsetMin=new Vector2(12,12);kitchenPanel.offsetMax=new Vector2(-12,-12);
            kitchenBack=Button(kitchenPanel,"Back",new Vector2(-525,334),new Vector2(150,72),CloseKitchen,new Color(.83f,.93f,.92f));
            kitchenTitle=Label(kitchenPanel,"Let's cook!",32,new Vector2(0,338),new Vector2(770,60));
            Button(kitchenPanel,"Help",new Vector2(525,334),new Vector2(150,72),()=>{kitchenHint.text=KitchenHelp();},new Color(.83f,.93f,.92f));
            kitchenStage=Label(kitchenPanel,"",22,new Vector2(0,280),new Vector2(760,40));
            kitchenHint=Label(kitchenPanel,"",23,new Vector2(0,-355),new Vector2(1050,42));
            kitchenRecipes=Rect(kitchenPanel,"Recipe pictures",Vector2.zero,Vector2.zero);
            for(var family=0;family<3;family++){
                var pick=family;var text=new[]{"Pizza","Cakes","Meals"}[family];
                var category=Button(kitchenRecipes,text,new Vector2(-330+family*330,205),new Vector2(280,86),()=>{kitchenFamily=pick;PresentKitchen();},new Color(.85f,.94f,.9f));
                HomePicture(category.transform.parent,text+" picture",new Vector2(-90,0),new Vector2(72,65),KitchenSprite("recipes",family*5,5,3)).preserveAspect=true;
            }
            for(var i=0;i<Kitchen.Recipes.Length;i++){
                var recipe=Kitchen.Recipes[i];var button=Button(kitchenRecipes,"",new Vector2(-444+i%5*222,0),new Vector2(208,245),()=>StartEasyRecipe(recipe.id),Color.white);
                var card=(RectTransform)button.transform.parent;card.name="Recipe "+recipe.id;recipeCards.Add(card);
                HomePicture(card,recipe.title,new Vector2(0,28),new Vector2(180,160),KitchenSprite("recipes",i,5,3)).preserveAspect=true;
                Label(card,recipe.title,23,new Vector2(0,-85),new Vector2(192,65));
            }
            kitchenReadyBaseText=Button(kitchenRecipes,"Make from the beginning",new Vector2(-255,-220),new Vector2(470,80),()=>{kitchenReadyBase=!kitchenReadyBase;PresentKitchen();},new Color(.91f,.87f,.98f));
            kitchenExperimentText=Button(kitchenRecipes,"Recipe ingredients",new Vector2(255,-220),new Vector2(470,80),()=>{kitchenExperiment=!kitchenExperiment;PresentKitchen();},new Color(.91f,.87f,.98f));
            kitchenOccupied=Rect(kitchenPanel,"Saved creations",Vector2.zero,Vector2.zero);
            Label(kitchenOccupied,"Your creations are safe. Tap one to help or continue.",25,new Vector2(0,195),new Vector2(950,70));
            for(var i=0;i<4;i++){
                var id="cookware-"+i;occupiedCards.Add(Button(kitchenOccupied,"",new Vector2(-420+i*280,0),new Vector2(252,190),()=>OpenKitchenItem(id),Color.white));
            }
            kitchenCooking=Rect(kitchenPanel,"Food preparation",Vector2.zero,Vector2.zero);
            kitchenPreview=Panel(kitchenCooking,"Food work surface",new Vector2(0,30),new Vector2(510,480),Color.white,true).rectTransform;
            kitchenPreview.GetComponent<Image>().sprite=CookingLayer(4);kitchenPreview.GetComponent<Image>().type=Image.Type.Simple;
            kitchenPreview.gameObject.AddComponent<Button>().targetGraphic=kitchenPreview.GetComponent<Image>();
            kitchenPreview.gameObject.AddComponent<KitchenGesture>().Screen=this;
            kitchenPreviewBase=HomePicture(kitchenPreview,"Serving dish",Vector2.zero,new Vector2(425,425),CookingLayer(5));
            kitchenPreviewFood=HomePicture(kitchenPreview,"Dish",Vector2.zero,new Vector2(430,430),CookingLayer(0));kitchenPreviewFood.preserveAspect=true;
            kitchenPreviewToppings=Enumerable.Range(0,36).Select(i=>HomePicture(kitchenPreview,"Added ingredient "+i,Vector2.zero,new Vector2(85,85),IngredientSprite("cheese"))).ToArray();
            BuildCakeSurface();
            kitchenIngredients=Rect(kitchenCooking,"Ingredients to add",Vector2.zero,Vector2.zero);
            for(var i=0;i<Kitchen.Ingredients.Length;i++){
                var name=Kitchen.Ingredients[i];var id="ingredient-"+name;
                var root=Panel(kitchenIngredients,"Add "+name,Vector2.zero,new Vector2(178,153),Color.white,true).rectTransform;
                root.GetComponent<Image>().sprite=CookingLayer(5);root.GetComponent<Image>().type=Image.Type.Simple;
                root.gameObject.AddComponent<Button>().targetGraphic=root.GetComponent<Image>();
                var icon=HomePicture(root,"Ingredient",new Vector2(0,13),new Vector2(115,104),KitchenSprite("toppings",i,5,5));icon.preserveAspect=true;
                var label=Label(root,name,22,new Vector2(0,-57),new Vector2(175,35));
                var gesture=root.gameObject.AddComponent<KitchenGesture>();gesture.Screen=this;gesture.Ingredient=id;kitchenIngredientButtons.Add((id,icon,label));
            }
            kitchenStep=Button(kitchenCooking,"Cooking action",new Vector2(0,-283),new Vector2(320,92),KitchenStepGesture,new Color(.73f,.88f,.78f));
            kitchenActionIcon=HomePicture(kitchenStep.transform.parent,"Next action picture",new Vector2(-105,0),new Vector2(70,70),KitchenSprite("utensils",0,2,2));kitchenActionIcon.preserveAspect=true;
            kitchenStep.rectTransform.anchoredPosition=new Vector2(36,0);kitchenStep.rectTransform.sizeDelta=new Vector2(230,85);kitchenStep.fontSize=26;
            kitchenHeat=Label(kitchenCooking,"",23,new Vector2(0,-221),new Vector2(480,46));
            kitchenUndo=Button(kitchenCooking,"Undo",new Vector2(-325,-282),new Vector2(150,74),()=>KitchenCommand("easy:undo",cookingItem),new Color(.89f,.88f,.96f));
            kitchenMore=Button(kitchenCooking,"More bowls  >",new Vector2(448,-282),new Vector2(210,74),()=>{kitchenPage++;PresentKitchen();},new Color(.83f,.92f,.96f));
            kitchenPrevious=Button(kitchenCooking,"<",new Vector2(294,-282),new Vector2(80,80),()=>{kitchenPage=Math.Max(0,kitchenPage-1);PresentKitchen();},new Color(.83f,.92f,.96f));
            kitchenTaste=Button(kitchenCooking,"Taste",new Vector2(-448,-282),new Vector2(210,90),()=>KitchenCommand(CurrentArea=="garden"?"easy:taste":"taste",cookingItem),new Color(.95f,.84f,.8f));
            kitchenWash=Button(kitchenCooking,"Wash & reuse",new Vector2(0,-282),new Vector2(320,92),()=>KitchenCommand("easy:wash",cookingItem,complete:r=>{if(r.Accepted)OpenEasyCook();}),new Color(.8f,.91f,.98f));
            kitchenServing=Rect(kitchenCooking,"Clean plates to serve",Vector2.zero,Vector2.zero);
            for(var i=0;i<8;i++){
                var id="plate-"+i;var button=Button(kitchenServing,"Plate "+(i+1),Vector2.zero,new Vector2(162,105),()=>KitchenCommand("easy:serve",cookingItem,id),new Color(.88f,.95f,.95f));
                HomePicture(button.transform.parent,"Plate picture",new Vector2(0,14),new Vector2(70,56),CookingLayer(5));button.rectTransform.anchoredPosition=new Vector2(0,-30);plateCards.Add(button);
            }
            kitchenPanel.gameObject.SetActive(false);
        }

        private string KitchenHelp()
        {
            var t=ReadToys().FirstOrDefault(v=>v.id==cookingItem);var d=t?.kitchen?.dish;
            if(d==null)return "Tap a food picture to start cooking.";
            if(CakeFlow.Active(d)){cakeDemoUntil=Time.unscaledTime+3;return CakeInstruction(d)+". Tap the pictured tool for help.";}
            var next=Kitchen.Next(d);return next.StartsWith("add:")?"Tap the "+next.Substring(4)+" bowl.":next=="serve"?"Tap a plate to share your food.":next=="heat"?"Tap Bake. Your food cooks safely.":"Tap the big tool, or play with the food.";
        }

        private void PresentEasyKitchen(SoloToy[] all,bool inKitchen)
        {
            if(!KitchenOpen)return;
            var current=all.FirstOrDefault(t=>t.id==cookingItem);var dish=current?.kitchen?.dish;
            var choose=kitchenChoosing;
            kitchenOccupied.gameObject.SetActive(current==null && !choose);kitchenRecipes.gameObject.SetActive(choose);kitchenCooking.gameObject.SetActive(current!=null && !choose);
            kitchenTitle.text=choose?"What shall we make?":current==null?"The kitchen is full of creations":dish==null?"Your plate":Kitchen.Recipe(dish.recipe).title;
            kitchenStage.text=choose?"Choose a picture":dish==null?"":dish.heated?"Ready for decorating and sharing":"Make it your way";
            kitchenReadyBaseText.text=kitchenReadyBase?"Start with a ready-made base":"Make from the beginning";
            kitchenExperimentText.text=kitchenExperiment?"Experiment: mix anything":"Recipe ingredients";
            for(var i=0;i<recipeCards.Count;i++)recipeCards[i].gameObject.SetActive(i/5==kitchenFamily);
            if(choose)return;
            if(current==null){for(var i=0;i<4;i++){var t=all.First(v=>v.id=="cookware-"+i);occupiedCards[i].text=t.kitchen.dish==null?"Wash this tray":Kitchen.Recipe(t.kitchen.dish.recipe).title;occupiedCards[i].transform.parent.GetComponent<Button>().interactable=t.zone==CurrentArea && (t.holder=="" || t.holder==Actor);}return;}
            if(choose)return;
            PaintDish(dish,kitchenPreviewFood,kitchenPreviewBase,kitchenPreviewToppings,3.4f);
            var next=Kitchen.Next(dish);var empty=dish==null || dish.portions==0;
            var serving=!empty && next=="serve";var preparing=!empty && !serving && inKitchen;
            kitchenIngredients.gameObject.SetActive(preparing || !empty && dish.portions==15 && current.kind==ToyKind.Cookware && inKitchen && kitchenMode=="Decorate");
            kitchenServing.gameObject.SetActive(serving && current.kind==ToyKind.Cookware && inKitchen && kitchenMode!="Decorate");
            kitchenMore.transform.parent.gameObject.SetActive(kitchenIngredients.gameObject.activeSelf);
            kitchenPrevious.transform.parent.gameObject.SetActive(kitchenIngredients.gameObject.activeSelf && kitchenPage>0);
            kitchenUndo.transform.parent.gameObject.SetActive(preparing && !dish.heated && dish.heat==0 && dish.ingredients.Length>1 && dish.ingredients.Last().by==Actor);
            kitchenTaste.transform.parent.gameObject.SetActive(serving);kitchenWash.transform.parent.gameObject.SetActive(empty && inKitchen && (dish!=null || current.kitchen.dirty));
            kitchenStep.transform.parent.gameObject.SetActive(!empty || dish==null && !current.kitchen.dirty);
            kitchenStep.transform.parent.GetComponent<Button>().interactable=empty || !(next=="heat" && Kitchen.Slot(current.container,out var place,out _) && place=="oven");
            kitchenStep.text=empty?"Make food":next.StartsWith("add:")?"Add "+next.Substring(4):next=="heat"?"Bake":next=="serve"?kitchenMode=="Decorate"?"Serve":"Decorate":System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(next.Replace('-',' '));
            kitchenActionIcon.sprite=empty?KitchenSprite("recipes",0,5,3):next.StartsWith("add:")?IngredientSprite(next.Substring(4)):next=="heat"?KitchenSprite("oven-closed"):next=="serve"?CookingLayer(5):KitchenSprite("utensils",Array.IndexOf(Kitchen.Tools,Kitchen.Tool(next)),2,2);
            if(current.kind==ToyKind.Plate && serving){kitchenStep.text="Make more food";kitchenStep.transform.parent.gameObject.SetActive(inKitchen);}
            kitchenHeat.text=!empty && next=="heat" && Kitchen.Slot(current.container,out var group,out _) && group=="oven"?"Baking… "+Mathf.CeilToInt((float)(Kitchen.HeatSeconds-dish.heat)):empty && dish!=null?"All shared! Your tray is ready to wash.":"";
            if(!inKitchen)kitchenHint.text="Enjoy your food. Wash it back in the kitchen.";
            var order=dish==null?Kitchen.Ingredients:Kitchen.Palette(dish).Distinct().ToArray();
            var pages=Math.Max(1,(order.Length+5)/6);kitchenPage%=pages;
            var shown=order.Skip(kitchenPage*6).Take(6).ToArray();
            foreach(var b in kitchenIngredientButtons){var name=b.id.Substring(11);var index=Array.IndexOf(shown,name);var root=(RectTransform)b.image.transform.parent;root.gameObject.SetActive(index>=0);if(index<0)continue;
                root.anchoredPosition=new Vector2(index<3?-448:448,190-index%3*166);
                var stock=all.First(t=>t.id==b.id);var available=stock.zone==CurrentArea && (stock.holder=="" || stock.holder==Actor);
                b.image.color=available?Color.white:new Color(.7f,.7f,.7f,.6f);b.count.text=stock.kitchen.amount==0?"Refill":name;
            }
            kitchenMore.text="More bowls  "+(kitchenPage+1)+"/"+pages+"  >";
            kitchenMore.transform.parent.gameObject.SetActive(pages>1 && kitchenIngredients.gameObject.activeSelf);
            for(var i=0;i<plateCards.Count;i++){
                var plate=all.First(t=>t.id=="plate-"+i);var button=plateCards[i];var root=(RectTransform)button.transform.parent;root.anchoredPosition=new Vector2(i<4?-448:448,192-i%4*118);
                var available=plate.zone==CurrentArea && (plate.holder=="" || plate.holder==Actor) && plate.kitchen.dish==null && !plate.kitchen.dirty;
                root.GetComponent<Button>().interactable=available;button.text=available?"Plate "+(i+1):"In use";
            }
            PaintCakeControls(dish,current);
        }
    }
}
