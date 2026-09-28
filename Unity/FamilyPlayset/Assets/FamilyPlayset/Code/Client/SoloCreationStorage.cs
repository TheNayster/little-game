using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform collectionPanel,collectionPaper,collectionFoodRoot;
        private Text collectionTitle,collectionInfo,collectionBack,collectionPrevious,collectionNext,collectionUse,collectionRemove,collectionUndo;
        private Text coloringKeep,coloringFolder,kitchenStore,kitchenStored;
        private ColoringSheet collectionSheet,roomCollectionSheet;
        private DiscoverySurface collectionLegacy,roomCollectionLegacy;
        private Image collectionFood,collectionPlate;
        private Image[] collectionToppings;
        private bool collectionFoodMode,collectionRoomMode,collectionPending;
        private int collectionIndex;
        private string collectionOwner,collectionArea,collectionMessage="",collectionCachedText;
        private CreationCollection collectionCached;
        private RectTransform roomCollectionRoot;
        private readonly System.Collections.Generic.Dictionary<DiscoverySurface,Tuple<int,DiscoveryWorkspace>> storedDrawingViews=new System.Collections.Generic.Dictionary<DiscoverySurface,Tuple<int,DiscoveryWorkspace>>();
        private bool CollectionOpen=>collectionPanel!=null && collectionPanel.gameObject.activeSelf;
        private CreationCollection Collections
        {
            get{
                var data=Shared?shared.View.homeCreations:World.ReadCreationData();
                if(collectionCached==null || collectionCachedText!=data){collectionCached=HomeCreations.Decode(data);collectionCachedText=data;}
                return collectionCached;
            }
        }
        private string CollectionError(string reason)=>reason=="food-storage-full"?"Your two food places are full. Bring a creation out first.":reason=="picture-folder-full"?"Your four picture places are full. Choose one to put away; Undo can bring it back.":reason=="picture-empty"?"Add some colors first.":reason=="food-still-cooking"?"Let your food finish cooking first.":reason=="need-empty-clean-tray"?"Free a clean tray before bringing this food out.":reason=="owner-only"?"This belongs to another player.":reason=="creation-storage-full"?"Your collection is full. Your creation is still safe.":"That changed while you were playing. Your creation is safe; try again.";
        private void CollectionCommand(SoloAction action,string op,string item,string target,Action<SoloResult> after=null)
        {
            if(!Ready || ActionPending || collectionPending || applicationPaused)return;
            collectionPending=true;collectionMessage="Saving…";
            void Done(SoloResult result){collectionPending=false;collectionCachedText=null;collectionMessage=result.Accepted?"Saved":CollectionError(result.Outcome);after?.Invoke(result);if(HasWorld)Render();}
            if(Shared)SubmitShared(action,item,target,op,0,0,Done);else Done(Command(action,item,target,op,0,0));
        }
        private void KeepColoringPicture()
        {
            if(discoveryPending)return;
            CollectionCommand(SoloAction.Discovery,"save-picture",Actor,Discovery.PageToken(discoveryPage,OwnDiscovery.pages[discoveryPage]),r=>{
                discoveryFeedback=r.Accepted?"Saved in My pictures. Your page stays here to keep coloring.":CollectionError(r.Outcome);discoveryFeedbackUntil=Time.unscaledTime+5;
            });
        }
        private void PutFoodAway()
        {
            var item=ReadToys().FirstOrDefault(t=>t.id==cookingItem);var dish=item?.kitchen?.dish;if(dish==null)return;
            if(cakeHelping || cakeSending)return;
            CollectionCommand(SoloAction.Kitchen,"store-food",item.id,HomeCreations.FoodToken(dish),r=>{if(r.Accepted)OpenEasyCook();else kitchenHint.text=CollectionError(r.Outcome);});
        }
        private void OpenCollection(bool food,string owner=null,bool fromRoom=false)
        {
            if(!Ready || SceneSchema<HomeCreations.Schema || ActionPending || discoveryPending)return;
            BuildCollectionPanel();CancelPointers();CloseDiscovery();CloseKitchen();PauseBook();
            collectionFoodMode=food;collectionRoomMode=fromRoom;collectionOwner=owner??Actor;collectionArea=CurrentArea;collectionIndex=0;collectionMessage="";
            collectionPanel.gameObject.SetActive(true);collectionPanel.SetAsLastSibling();stick.gameObject.SetActive(false);PresentCollections();
            var icon=collectionUse.transform.parent.Find("Coloring icon").GetComponent<ReaderPictureControl>();icon.Icon=food?"dish":"pictures";icon.SetVerticesDirty();
        }
        private void CloseCollection(bool returnToActivity=true)
        {
            if(collectionPanel==null)return;var open=CollectionOpen;collectionPanel.gameObject.SetActive(false);
            if(stick!=null)stick.gameObject.SetActive(JoystickMode && !MenuOpen);
            if(!open || !returnToActivity || !Ready || applicationPaused || CurrentArea!=collectionArea || MenuOpen)return;
            if(!collectionRoomMode){if(collectionFoodMode)OpenEasyCook();else OpenDiscovery(3);}
        }
        private Text CollectionButton(string name,string icon,Action action)
        {var text=Button(collectionPanel,name,Vector2.zero,new Vector2(210,78),()=>action(),Cream);ColorControl(text,icon);return text;}
        private void BuildCollectionPanel()
        {
            if(collectionPanel!=null)return;
            collectionPanel=Panel(safe,"Personal creations",Vector2.zero,Vector2.zero,new Color(.9f,.97f,.95f),true).rectTransform;Stretch(collectionPanel);
            collectionTitle=Label(collectionPanel,"",30,Vector2.zero,new Vector2(780,60));collectionInfo=Label(collectionPanel,"",22,Vector2.zero,new Vector2(900,70));
            collectionBack=CollectionButton("Close creations","back",()=>CloseCollection());
            collectionPrevious=CollectionButton("Previous creation","back",()=>{collectionIndex=Math.Max(0,collectionIndex-1);collectionMessage="";PresentCollections();});
            collectionNext=CollectionButton("Next creation","next",()=>{collectionIndex++;collectionMessage="";PresentCollections();});
            collectionUse=CollectionButton("Use saved creation","pictures",UseCollection);
            collectionRemove=CollectionButton("Put picture away","more",()=>{
                var p=SelectedPicture;if(p!=null)CollectionCommand(SoloAction.Discovery,"remove-picture",Actor,p.key.ToString());
            });
            collectionUndo=CollectionButton("Undo picture removal","undo",()=>{
                var p=Collections.removed.FirstOrDefault(q=>q.owner==Actor);if(p!=null)CollectionCommand(SoloAction.Discovery,"undo-picture-remove",Actor,p.key.ToString());
            });
            collectionPaper=Panel(collectionPanel,"Saved picture paper",Vector2.zero,Vector2.zero,Color.white,true).rectTransform;
            collectionSheet=Rect(collectionPaper,"Saved coloring",Vector2.zero,Vector2.zero).gameObject.AddComponent<ColoringSheet>();Stretch(collectionSheet.rectTransform);collectionSheet.raycastTarget=false;collectionSheet.CanInteract=()=>false;
            collectionLegacy=Rect(collectionPaper,"Saved drawing",Vector2.zero,Vector2.zero).gameObject.AddComponent<DiscoverySurface>();Stretch(collectionLegacy.rectTransform);collectionLegacy.raycastTarget=false;
            collectionFoodRoot=Rect(collectionPanel,"Stored food preview",Vector2.zero,new Vector2(450,450));
            collectionPlate=HomePicture(collectionFoodRoot,"Food dish",Vector2.zero,new Vector2(420,420),CookingLayer(5));
            collectionFood=HomePicture(collectionFoodRoot,"Stored food",Vector2.zero,new Vector2(420,420),CookingLayer(0));collectionFood.preserveAspect=true;
            collectionToppings=Enumerable.Range(0,36).Select(i=>HomePicture(collectionFoodRoot,"Preserved topping "+i,Vector2.zero,new Vector2(80,80),IngredientSprite("cheese"))).ToArray();
            collectionPanel.gameObject.SetActive(false);
        }
        private StoredPicture SelectedPicture=>Collections.pictures.Where(p=>p.owner==collectionOwner).OrderBy(p=>p.key).ElementAtOrDefault(collectionIndex);
        private void UseCollection()
        {
            if(collectionFoodMode){
                var f=Collections.foods.Where(p=>p.owner==Actor).OrderBy(p=>p.key).ElementAtOrDefault(collectionIndex);if(f==null)return;
                CollectionCommand(SoloAction.Kitchen,"restore-food","",f.key.ToString(),r=>{
                    if(!r.Accepted)return;var tray=ReadToys().FirstOrDefault(t=>t.kitchen?.dish?.id==f.dish.id && t.kitchen.dish.portions==f.dish.portions && t.kind==ToyKind.Cookware);
                    CloseCollection(false);if(tray!=null)OpenKitchenItem(tray.id);
                });
            }else {var p=SelectedPicture;if(p!=null)CollectionCommand(SoloAction.Discovery,"display-picture",Actor,p.key.ToString());}
        }
        private void PresentStoredPicture(StoredPicture picture,RectTransform paper,ColoringSheet official,DiscoverySurface legacy,Vector2 bounds)
        {
            var isOfficial=picture.page>=Discovery.LegacyPages;official.gameObject.SetActive(isOfficial);legacy.gameObject.SetActive(!isOfficial);
            var aspect=800f/460;
            if(isOfficial){official.Present(picture.page,picture.drawing,picture.key);aspect=official.Aspect;}
            else {
                if(!storedDrawingViews.TryGetValue(legacy,out var view) || view.Item1!=picture.key){
                    var w=new DiscoveryWorkspace{pages=Enumerable.Range(0,Discovery.Pages.Length).Select(i=>i==picture.page?picture.drawing:new ColoringPage{colors=new int[Discovery.Regions[i]]}).ToArray()};
                    view=Tuple.Create(picture.key,w);storedDrawingViews[legacy]=view;
                }
                legacy.Present(view.Item2,3,picture.page);
            }
            var height=Mathf.Min(bounds.y,bounds.x/aspect);paper.sizeDelta=new Vector2(height*aspect,height);
        }
        private void PresentCollections()
        {
            if(!HasWorld || SceneSchema<HomeCreations.Schema)return;
            PresentRoomCreation();
            if(!CollectionOpen)return;
            if(applicationPaused || CurrentArea!=collectionArea || TravelPending || menu.activeSelf || BookOpen || CharactersOpen){CloseCollection(false);return;}
            var size=collectionPanel.rect.size;var width=size.x;var height=size.y;
            var foods=Collections.foods.Where(f=>f.owner==collectionOwner).OrderBy(f=>f.key).ToArray();var pictures=Collections.pictures.Where(f=>f.owner==collectionOwner).OrderBy(f=>f.key).ToArray();
            var count=collectionFoodMode?foods.Length:pictures.Length;collectionIndex=Math.Max(0,Math.Min(collectionIndex,count-1));var editable=collectionOwner==Actor && !collectionRoomMode;
            collectionTitle.text=(collectionFoodMode?"My saved food":collectionRoomMode?"Bedroom pictures":"My pictures")+"   "+count+" / "+(collectionFoodMode?HomeCreations.FoodPlaces:HomeCreations.PicturePlaces);
            collectionTitle.rectTransform.anchoredPosition=new Vector2(-90,height/2-46);collectionTitle.resizeTextForBestFit=true;collectionTitle.resizeTextMinSize=20;collectionTitle.resizeTextMaxSize=30;collectionTitle.rectTransform.sizeDelta=new Vector2(width-350,60);
            LayoutColorControl(collectionBack,"Back",new Vector2(width/2-116,height/2-46),new Vector2(208,74),!collectionPending);
            LayoutColorControl(collectionPrevious,"Back",new Vector2(-width/2+112,-height/2+53),new Vector2(190,76),!collectionPending && collectionIndex>0);
            LayoutColorControl(collectionNext,"Next",new Vector2(width/2-112,-height/2+53),new Vector2(190,76),!collectionPending && collectionIndex<count-1);
            collectionInfo.rectTransform.anchoredPosition=new Vector2(0,-height/2+52);collectionInfo.rectTransform.sizeDelta=new Vector2(width-470,76);collectionInfo.resizeTextForBestFit=true;collectionInfo.resizeTextMinSize=16;collectionInfo.resizeTextMaxSize=23;
            collectionInfo.text=collectionPending?"Saving…":collectionMessage!=""?collectionMessage:count==0?collectionFoodMode?"Use Put away on a dish to free its tray.":"Color a picture, then tap Keep picture.":collectionFoodMode?"Bring it out to keep cooking or share it.":"Your saved picture stays safe while you keep coloring.";
            collectionUse.transform.parent.gameObject.SetActive(editable && count>0);collectionRemove.transform.parent.gameObject.SetActive(editable && !collectionFoodMode && count>0);collectionUndo.transform.parent.gameObject.SetActive(editable && !collectionFoodMode);
            LayoutColorControl(collectionUse,collectionFoodMode?"Bring out":count>0 && pictures[collectionIndex].displayed?"Take down":"Hang in room",new Vector2(-width/2+126,100),new Vector2(230,80),!collectionPending && count>0);
            LayoutColorControl(collectionRemove,"Put away",new Vector2(-width/2+126,0),new Vector2(230,80),!collectionPending);
            LayoutColorControl(collectionUndo,"Undo",new Vector2(-width/2+126,-100),new Vector2(230,80),!collectionPending && Collections.removed.Any(p=>p.owner==Actor) && pictures.Length<HomeCreations.PicturePlaces);
            collectionPaper.gameObject.SetActive(!collectionFoodMode && count>0);collectionPaper.anchoredPosition=new Vector2(95,0);
            if(!collectionFoodMode && count>0)PresentStoredPicture(pictures[collectionIndex],collectionPaper,collectionSheet,collectionLegacy,new Vector2(width-380,height-195));
            collectionFoodRoot.gameObject.SetActive(collectionFoodMode && count>0);collectionFoodRoot.anchoredPosition=new Vector2(95,5);collectionFoodRoot.localScale=Vector3.one*Mathf.Min((width-380)/450,(height-210)/450);
            if(collectionFoodMode && count>0)PaintDish(foods[collectionIndex].dish,collectionFood,collectionPlate,collectionToppings,3.4f);
        }
        private void PresentRoomCreation()
        {
            if(roomPrint==null)return;var room=FurnishedRoom;
            var stored=room==null || SecretRooms.Index(CurrentArea)>=0?null:Collections.pictures.FirstOrDefault(p=>p.owner==room.owner && p.displayed);
            if(stored!=null && roomCollectionRoot==null){
                roomCollectionRoot=Rect(roomPrint.transform.parent,"Saved personal artwork",Vector2.zero,new Vector2(170,120));
                roomCollectionSheet=Rect(roomCollectionRoot,"Framed coloring",Vector2.zero,Vector2.zero).gameObject.AddComponent<ColoringSheet>();Stretch(roomCollectionSheet.rectTransform);roomCollectionSheet.raycastTarget=false;roomCollectionSheet.CanInteract=()=>false;
                roomCollectionLegacy=Rect(roomCollectionRoot,"Framed drawing",Vector2.zero,Vector2.zero).gameObject.AddComponent<DiscoverySurface>();Stretch(roomCollectionLegacy.rectTransform);roomCollectionLegacy.raycastTarget=false;
            }
            if(roomCollectionRoot!=null)roomCollectionRoot.gameObject.SetActive(stored!=null);
            roomPrint.gameObject.SetActive(stored==null);
            if(stored!=null)PresentStoredPicture(stored,roomCollectionRoot,roomCollectionSheet,roomCollectionLegacy,new Vector2(170,120));
        }
        private void ResetCollections()
        {if(collectionPanel!=null)Destroy(collectionPanel.gameObject);collectionPanel=null;collectionCached=null;collectionCachedText=null;roomCollectionRoot=null;collectionPending=false;storedDrawingViews.Clear();}
        private void PresentCreationEntrances()
        {
            if(SceneSchema<HomeCreations.Schema)return;
            if(coloringKeep!=null){
                var art=DiscoveryOpen && discoveryStation==3 && !discoveryGallery.gameObject.activeSelf;
                coloringKeep.transform.parent.gameObject.SetActive(art);coloringFolder.transform.parent.gameObject.SetActive(art);
                if(art){var size=discoveryPanel.rect.size;var left=-size.x/2+112;
                    LayoutColorControl(coloringKeep,"Keep picture",new Vector2(left,size.y/2-226),new Vector2(190,76),!collectionPending && !discoveryPending && !ActionPending);
                    LayoutColorControl(coloringFolder,"My pictures",new Vector2(left,size.y/2-318),new Vector2(190,76),!collectionPending && !discoveryPending && !ActionPending);
                    LayoutColorControl(discoveryUndo,"Undo",new Vector2(left,size.y/2-410),new Vector2(190,76),!collectionPending && !discoveryPending && OwnDiscovery.pages[discoveryPage].undo.Length>0);
                    LayoutColorControl(discoveryRedo,"Redo",new Vector2(left,size.y/2-502),new Vector2(190,76),!collectionPending && !discoveryPending && OwnDiscovery.pages[discoveryPage].redo.Length>0);
                }
            }
            if(kitchenStored!=null && KitchenOpen){
                var item=ReadToys().FirstOrDefault(t=>t.id==cookingItem);var dish=item?.kitchen?.dish;
                kitchenStored.transform.parent.gameObject.SetActive(CurrentArea=="garden");kitchenStore.transform.parent.gameObject.SetActive(CurrentArea=="garden" && dish!=null && dish.portions>0 && !kitchenChoosing);
                LayoutColorControl(kitchenStored,"Saved food",new Vector2(-325,334),new Vector2(208,62),!collectionPending && !ActionPending);
                LayoutColorControl(kitchenStore,"Put away",new Vector2(325,334),new Vector2(208,62),!collectionPending && !ActionPending && !cakeHelping && !cakeSending && (dish==null || dish.heated || dish.heat==0));
                kitchenStage.rectTransform.sizeDelta=new Vector2(680,40);
            }
        }
    }
}
