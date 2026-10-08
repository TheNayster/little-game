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
        private readonly RectTransform[,] snackChoices=new RectTransform[3,2];
        private readonly NavigationTap[] snackIngredients=new NavigationTap[2];
        private NavigationTap snackServe;
        private Text snackCue;
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
            snackStation=ZooObject("Elephant snack station");
            for(var i=-1;i<=1;i+=2)Plain(snackStation,"Table leg",new Vector2(i*52,43),new Vector2(14,85),new Color(.62f,.43f,.26f));
            Panel(snackStation,"Snack preparation surface",new Vector2(0,92),new Vector2(150,24),new Color(.85f,.68f,.44f),false,true);
            SnackBowl(snackStation,new Vector2(0,123),.8f);
            ZooFoodPicture(snackStation,ZooFoodKind.Leaves,new Vector2(-45,160),.55f);
            ZooFoodPicture(snackStation,ZooFoodKind.Hay,new Vector2(45,160),.55f);
            HomeHit(snackStation,"Prepare elephant snack",new Vector2(0,105),new Vector2(170,210),OpenElephantSnack);
            snackPanel=Plain(safe,"Snack preparation input shield",Vector2.zero,Vector2.zero,new Color(.16f,.25f,.22f,.72f),true).rectTransform;Stretch(snackPanel);
            snackFrame=Panel(snackPanel,"Your elephant snack",Vector2.zero,new Vector2(760,540),Cream,true).rectTransform;
            Label(snackFrame,"Your elephant snack",30,new Vector2(-30,222),new Vector2(590,48));
            var close=Panel(snackFrame,"Close snack preparation",new Vector2(319,218),Vector2.one*88,new Color(.78f,.88f,.79f),true);NavButton(close,CloseElephantSnack);
            for(var i=0;i<2;i++){
                var bar=Plain(close.transform,"Close stroke",Vector2.zero,new Vector2(43,8),Ink);bar.rectTransform.localRotation=Quaternion.Euler(0,0,i==0?45:-45);
                var kind=i;var ingredient=Panel(snackFrame,i==0?"Add snack leaves":"Add snack hay",new Vector2(i==0?-155:155,120),new Vector2(230,116),new Color(.82f,.9f,.73f),true,true);
                ZooFoodPicture(ingredient.transform,(ZooFoodKind)i,Vector2.zero,1.45f);snackIngredients[i]=NavButton(ingredient,()=>EditElephantSnack("snack-add",kind));
            }
            SnackBowl(snackFrame,new Vector2(0,-25),3.6f);
            for(var i=0;i<3;i++){
                var slot=i;var hit=Panel(snackFrame,"Remove snack piece "+i,new Vector2((i-1)*130,-15),new Vector2(112,112),new Color(1,1,1,0),true,true);NavButton(hit,()=>EditElephantSnack("snack-remove",slot));
                for(var k=0;k<2;k++){var r=Rect(hit.transform,"Snack piece picture "+k,Vector2.zero,Vector2.zero);snackChoices[i,k]=r;ZooFoodPicture(r,(ZooFoodKind)k,Vector2.zero,1.6f);}
            }
            snackCue=Label(snackFrame,"Tap a food picture",24,new Vector2(0,-127),new Vector2(660,42));
            var clear=Panel(snackFrame,"Clear snack bowl",new Vector2(-196,-200),new Vector2(185,95),new Color(.88f,.79f,.64f),true,true);NavButton(clear,()=>EditElephantSnack("snack-clear"));
            SnackBowl(clear.transform,new Vector2(-20,0),.85f);ZooArrow(clear.transform,new Vector2(49,7),-1);
            var serve=Panel(snackFrame,"Carry elephant snack",new Vector2(141,-200),new Vector2(344,95),new Color(.67f,.84f,.66f),true,true);snackServe=NavButton(serve,()=>EditElephantSnack("snack-serve"));
            SnackBowl(serve.transform,new Vector2(-58,0),.9f);ZooArrow(serve.transform,new Vector2(47,8),1);snackPanel.gameObject.SetActive(false);
        }
        private void OpenElephantSnack()
        {
            if(!Ready || MenuOpen || ActionPending || TravelPending)return;
            if(OwnSnack?.species!=""){ZooWalk("take","elephant",new Vector2(ZooLayout.BucketX("elephant"),100));return;}
            ZooWalk("snack-begin","elephant",new Vector2(ZooLayout.SnackX,100));
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
                    if(op=="snack-serve"){var food=OwnSnack;ZooWalk("offer","elephant",new Vector2(ZooLayout.SlotX("elephant",food.slot),100));}
                }
                Render();
            }
            if(Shared)SubmitShared(SoloAction.Zoo,token,"elephant",op,0,0,Done);else Done(Command(SoloAction.Zoo,item:token,target:"elephant",value:op));
        }
        private void CloseElephantSnack(){if(ElephantSnackOpen)EditElephantSnack("snack-cancel");}
        private void TickElephantSnack(bool visible)
        {
            if(snackStation==null)return;
            snackStation.gameObject.SetActive(visible);snackStation.anchoredPosition=ToBoard(ZooLayout.SnackX,100);snackStation.localScale=Vector3.one*sceneScale;
            if(!ElephantSnackOpen)return;var f=OwnSnack;
            if(CurrentArea!=ZooLayout.Savanna || f?.preparing!=true || Shared && !shared.Connected){snackPanel.gameObject.SetActive(false);return;}
            snackFrame.localScale=Vector3.one*Mathf.Min(1,Mathf.Min(safe.rect.width/800,safe.rect.height/580));
            if(snackSeenEdit!=f.edit){if(f.pieces.Length>snackSeenCount)snackLanded=Time.unscaledTime;snackSeenEdit=f.edit;snackSeenCount=f.pieces.Length;}
            for(var i=0;i<3;i++)for(var k=0;k<2;k++){
                var r=snackChoices[i,k];r.gameObject.SetActive(i<f.pieces.Length && f.pieces[i]==k);r.anchoredPosition=new Vector2(0,i==f.pieces.Length-1?Mathf.Max(0,1-(Time.unscaledTime-snackLanded)/.22f)*30:0);
            }
            snackCue.text=f.pieces.Length==0?"Tap a food picture":f.pieces.Length==3?"Bowl ready! Tap a piece to take it out":"Tap a piece to take it out";
            snackServe.interactable=f.pieces.Length>0 && !snackSending && !ActionPending;
            foreach(var button in snackIngredients)button.interactable=f.pieces.Length<3 && !snackSending && !ActionPending;
            if(Keyboard.current?.escapeKey.wasPressedThisFrame==true)CloseElephantSnack();
        }
        private void BuildCarriedSnack(RectTransform parent)
        {
            var bowl=Rect(parent,"Prepared snack",Vector2.zero,Vector2.zero);SnackBowl(bowl,Vector2.zero,.9f);
            for(var i=0;i<3;i++)for(var k=0;k<2;k++){var piece=Rect(bowl,"Piece "+i+"/"+k,new Vector2((i-1)*24,22),Vector2.zero);ZooFoodPicture(piece,(ZooFoodKind)k,Vector2.zero,.48f);}
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
        private void ResetElephantSnack(){if(snackPanel!=null)Destroy(snackPanel.gameObject);snackPanel=snackFrame=snackStation=null;snackSending=false;}
    }
}
