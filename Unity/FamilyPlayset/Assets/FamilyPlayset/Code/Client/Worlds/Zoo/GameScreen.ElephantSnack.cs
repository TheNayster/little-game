using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform snackStation,snackPanel,snackFrame;
        private readonly System.Collections.Generic.Dictionary<string,RectTransform> zooSnackStations=new System.Collections.Generic.Dictionary<string,RectTransform>();
        private readonly RectTransform[,] snackChoices=new RectTransform[3,7];
        private readonly NavigationTap[] snackIngredients=new NavigationTap[2];
        private NavigationTap snackServe;
        private Text snackCue,snackTitle;
        private string snackSpecies="elephant";
        private RawImage snackAnimalPicture;
        private bool snackSending;
        private long snackSeenEdit=-1;
        private int snackSeenCount;
        private float snackLanded;
        public bool ElephantSnackOpen=>snackPanel!=null && snackPanel.gameObject.activeSelf;
        private ZooFood OwnSnack=>Zoo?.food.FirstOrDefault(f=>f.actor==Actor);
        private void SnackBowl(Transform parent,Vector2 at,float scale)
        {
            Panel(parent,"Bowl shadow",at+new Vector2(0,-12)*scale,new Vector2(97,20)*scale,new Color(.3f,.3f,.2f,.18f),false,true);
            Panel(parent,"Blue snack bowl",at,new Vector2(96,39)*scale,new Color(.34f,.64f,.76f),false,true);
            Panel(parent,"Bowl cream interior",at+new Vector2(0,14)*scale,new Vector2(90,25)*scale,Cream,false,true);
        }
        private void BuildElephantSnack()
        {
            foreach(var info in ZooCatalog.All){
                var id=info.id;var station=ZooObject(info.name+" snack preparation");zooSnackStations.Add(id,station);
                if(id=="elephant")snackStation=station;
                for(var i=-1;i<=1;i+=2)Plain(station,"Table leg",new Vector2(i*52,43),new Vector2(14,85),new Color(.62f,.43f,.26f));
                Panel(station,"Table outline",new Vector2(0,92),new Vector2(156,30),Ink,false,true);
                Panel(station,"Snack preparation surface",new Vector2(0,92),new Vector2(150,24),new Color(.85f,.68f,.44f),false,true);
                SnackBowl(station,new Vector2(0,123),.8f);
                var foods=info.SnackFoods;
                for(var k=0;k<foods.Length;k++)ZooFoodPicture(station,foods[k],new Vector2(foods.Length==1?0:k==0?-43:43,159),.65f);
                var portrait=ZooPortraitView(station,"Animal snack picture",new Vector2(-51,64),31);portrait.texture=ZooPortrait(id);
                HomeHit(station,"Prepare "+info.name+" snack",new Vector2(0,105),new Vector2(170,210),()=>OpenZooSnack(id));
            }
            snackPanel=Plain(safe,"Snack preparation input shield",Vector2.zero,Vector2.zero,new Color(.16f,.25f,.22f,.72f),true).rectTransform;Stretch(snackPanel);
            snackFrame=Panel(snackPanel,"Your animal snack",Vector2.zero,new Vector2(760,540),Cream,true).rectTransform;
            snackTitle=Label(snackFrame,"Your animal snack",30,new Vector2(15,222),new Vector2(510,48));
            snackAnimalPicture=ZooPortraitView(snackFrame,"Snack animal portrait",new Vector2(-300,217),70);
            var close=Panel(snackFrame,"Close snack preparation",new Vector2(319,218),Vector2.one*88,new Color(.78f,.88f,.79f),true);NavButton(close,CloseElephantSnack);
            for(var i=0;i<2;i++){
                var bar=Plain(close.transform,"Close stroke",Vector2.zero,new Vector2(43,8),Ink);bar.rectTransform.localRotation=Quaternion.Euler(0,0,i==0?45:-45);
                var kind=i;var ingredient=Panel(snackFrame,i==0?"Add snack leaves":"Add snack hay",new Vector2(i==0?-155:155,120),new Vector2(230,116),new Color(.82f,.9f,.73f),true,true);
                for(var k=0;k<7;k++){var food=Rect(ingredient.transform,"Ingredient "+k,Vector2.zero,Vector2.zero);ZooFoodPicture(food,(ZooFoodKind)k,Vector2.zero,1.45f);}
                snackIngredients[i]=NavButton(ingredient,()=>{var foods=ZooCatalog.Get(snackSpecies).SnackFoods;if(kind<foods.Length)EditElephantSnack("snack-add",(int)foods[kind]);});
            }
            SnackBowl(snackFrame,new Vector2(0,-25),3.6f);
            for(var i=0;i<3;i++){
                var slot=i;var hit=Panel(snackFrame,"Remove snack piece "+i,new Vector2((i-1)*130,-15),new Vector2(112,112),new Color(1,1,1,0),true,true);NavButton(hit,()=>EditElephantSnack("snack-remove",slot));
                for(var k=0;k<7;k++){var r=Rect(hit.transform,"Snack piece picture "+k,Vector2.zero,Vector2.zero);snackChoices[i,k]=r;ZooFoodPicture(r,(ZooFoodKind)k,Vector2.zero,1.6f);}
            }
            snackCue=Label(snackFrame,"Tap a food picture",24,new Vector2(0,-127),new Vector2(660,42));
            var clear=Panel(snackFrame,"Clear snack bowl",new Vector2(-196,-200),new Vector2(185,95),new Color(.88f,.79f,.64f),true,true);NavButton(clear,()=>EditElephantSnack("snack-clear"));
            SnackBowl(clear.transform,new Vector2(-20,0),.85f);ZooArrow(clear.transform,new Vector2(49,7),-1);
            var serve=Panel(snackFrame,"Carry animal snack",new Vector2(141,-200),new Vector2(344,95),new Color(.67f,.84f,.66f),true,true);snackServe=NavButton(serve,()=>EditElephantSnack("snack-serve"));
            SnackBowl(serve.transform,new Vector2(-58,0),.9f);ZooArrow(serve.transform,new Vector2(47,8),1);snackPanel.gameObject.SetActive(false);
        }
        private void OpenElephantSnack()=>OpenZooSnack(ZooInView().id);
        private void OpenZooSnack(string species)
        {
            if(!Ready || MenuOpen || ActionPending || TravelPending)return;
            snackSpecies=species;
            if(OwnSnack?.species!=""){ZooWalk("take",snackSpecies,new Vector2(ZooLayout.BucketX(snackSpecies),100));return;}
            ZooWalk("snack-begin",snackSpecies,new Vector2(ZooCatalog.Get(snackSpecies).SnackX,100));
        }
        private void ShowElephantSnack()
        {CancelPointers();destination=null;shared?.Walk(WalkMode.Stop);snackSeenEdit=-1;snackSeenCount=0;snackPanel.gameObject.SetActive(true);snackPanel.SetAsLastSibling();}
        private void EditElephantSnack(string op,int piece=0)
        {
            var f=OwnSnack;if(!ElephantSnackOpen || !Ready || ActionPending || snackSending || f?.preparing!=true)return;
            if(op=="snack-serve" && f.pieces.Length==0 || op=="snack-add" && f.pieces.Length==3 || op=="snack-remove" && piece>=f.pieces.Length)return;
            snackSending=true;var token=f.prepEpoch+"/"+f.edit+"/"+piece;
            void Done(SoloResult r){
                snackSending=false;
                if(r.Accepted && (op=="snack-serve" || op=="snack-cancel")){
                    snackPanel.gameObject.SetActive(false);
                    if(op=="snack-serve"){var food=OwnSnack;ZooWalk("offer",snackSpecies,new Vector2(ZooLayout.SlotX(snackSpecies,food.slot),100));}
                }
                Render();
            }
            if(Shared)SubmitShared(SoloAction.Zoo,token,snackSpecies,op,0,0,Done);else Done(Command(SoloAction.Zoo,item:token,target:snackSpecies,value:op));
        }
        private void CloseElephantSnack(){if(ElephantSnackOpen)EditElephantSnack("snack-cancel");}
        private void TickElephantSnack(bool visible)
        {
            if(snackStation==null)return;
            foreach(var station in zooSnackStations){
                var info=ZooCatalog.Get(station.Key);var shown=ZooExhibitVisible(info) && !MenuOpen && !ZooMapOpen && !ZooPhotoOpen && !ElephantSnackOpen;
                station.Value.gameObject.SetActive(shown);station.Value.anchoredPosition=ToBoard(info.SnackX,100);station.Value.localScale=Vector3.one*sceneScale;
            }

            if(!ElephantSnackOpen)return;var f=OwnSnack;
            if(CurrentArea!=ZooCatalog.Get(snackSpecies).area || f?.prepSpecies!=snackSpecies || f?.preparing!=true || Shared && !shared.Connected){snackPanel.gameObject.SetActive(false);return;}
            snackFrame.localScale=Vector3.one*Mathf.Min(1,Mathf.Min(safe.rect.width/800,safe.rect.height/580));
            if(snackSeenEdit!=f.edit){if(f.pieces.Length>snackSeenCount)snackLanded=Time.unscaledTime;snackSeenEdit=f.edit;snackSeenCount=f.pieces.Length;}
            for(var i=0;i<3;i++)for(var k=0;k<7;k++){
                var r=snackChoices[i,k];r.gameObject.SetActive(i<f.pieces.Length && f.pieces[i]==k);r.anchoredPosition=new Vector2(0,i==f.pieces.Length-1?Mathf.Max(0,1-(Time.unscaledTime-snackLanded)/.22f)*30:0);
            }
            snackCue.text=f.pieces.Length==0?"Tap a food picture":f.pieces.Length==3?"Bowl ready! Tap a piece to take it out":"Tap a piece to take it out";
            snackServe.interactable=f.pieces.Length>0 && !snackSending && !ActionPending;
            snackTitle.text="Your "+ZooCatalog.Get(snackSpecies).name+" snack";snackAnimalPicture.texture=ZooPortrait(snackSpecies);
            var foods=ZooCatalog.Get(snackSpecies).SnackFoods;
            for(var i=0;i<2;i++){var button=snackIngredients[i];button.gameObject.SetActive(i<foods.Length);button.interactable=f.pieces.Length<3 && !snackSending && !ActionPending;button.GetComponent<RectTransform>().anchoredPosition=new Vector2(foods.Length==1?0:i==0?-155:155,120);
                foreach(Transform child in button.transform)child.gameObject.SetActive(i<foods.Length && child.name=="Ingredient "+(int)foods[i]);}

            if(Keyboard.current?.escapeKey.wasPressedThisFrame==true)CloseElephantSnack();
        }
        private void BuildCarriedSnack(RectTransform parent)
        {
            var bowl=Rect(parent,"Prepared snack",Vector2.zero,Vector2.zero);
            for(var i=0;i<3;i++)for(var k=0;k<7;k++){var piece=Rect(bowl,"Piece "+i+"/"+k,new Vector2((i-1)*24,22),Vector2.zero);ZooFoodPicture(piece,(ZooFoodKind)k,Vector2.zero,.48f);}
        }
        private void TickCarriedSnack(RectTransform parent,ZooFood f)
        {
            foreach(Transform child in parent)child.gameObject.SetActive(f.pieces.Length>0?child.name=="Prepared snack":child.name=="Food "+(int)ZooCatalog.Get(f.species).food);
            if(f.pieces.Length==0)return;
            foreach(Transform child in parent.Find("Prepared snack")){
                if(!child.name.StartsWith("Piece ",StringComparison.Ordinal))continue;
                var bits=child.name.Substring(6).Split('/');var i=int.Parse(bits[0]);var k=int.Parse(bits[1]);child.gameObject.SetActive(i<f.pieces.Length && f.pieces[i]==k);
            }
        }
        private void ResetElephantSnack(){if(snackPanel!=null)Destroy(snackPanel.gameObject);snackPanel=snackFrame=snackStation=null;zooSnackStations.Clear();snackSending=false;}
    }
}
