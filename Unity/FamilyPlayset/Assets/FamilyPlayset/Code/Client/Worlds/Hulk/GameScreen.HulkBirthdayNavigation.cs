using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform birthdayNavigation,birthdaySeatActions;
        private Text birthdayRoofText;
        private RectTransform birthdayThrowButton,birthdayNapButton;
        private readonly Dictionary<string,(RectTransform bar,RectTransform cord)> birthdayZipHandles=new Dictionary<string,(RectTransform,RectTransform)>();
        private readonly List<RectTransform> birthdaySeatTargets=new List<RectTransform>(),birthdayZipTargets=new List<RectTransform>(),birthdayWires=new List<RectTransform>();
        private bool birthdayRoof;
        private bool birthdayRightRoof;
        private float birthdayCameraY;
        private float BirthdayCameraOffset=>CurrentArea==HulkBirthday.Zone?birthdayCameraY:0;
        private double BirthdayVisualTime
        {
            get {
                var b=Birthday;double now=Time.unscaledTimeAsDouble;
                double value=birthdayClock.Sample(Shared?shared.View.worldId:World.WorldId,b.clock,now,Shared);
                // This world's authority clock pauses when nobody is here. A
                // resumed client must not keep extrapolating its old wall clock.
                if(Math.Abs(value-b.clock)>1){birthdayClock.Reset();value=birthdayClock.Sample(Shared?shared.View.worldId:World.WorldId,b.clock,now,Shared);}
                return value;
            }
        }
        private BirthdayMember BirthdayOwn=>Birthday?.members.FirstOrDefault(m=>m.actor==Actor);
        private bool BirthdayCameraFollowing=>CurrentArea==HulkBirthday.Zone && BirthdayOwn!=null && (birthdayRoof || BirthdayOwn.seat>=0 || BirthdayOwn.station=="zipline");
        private Vector2 BirthdayNavigationPoint(BirthdayMember m)
        {
            BirthdayNavigation.Point(m,BirthdayVisualTime,out float x,out float y);
            // The original roof pegs stack in 3D depth. Spread four shared
            // silhouettes across that same roof in the flat family view.
            if(m.seat>=0 && BirthdayNavigation.SeatY[m.seat]>3000){
                int place=Array.IndexOf(Birthday.members,m);if(place<0)place=Array.FindIndex(Birthday.members,n=>n.actor==m.actor);
                bool right=BirthdayNavigation.SeatX[m.seat]>2400;x=(right?4910:0)+(place-1.5f)*100;
                y=right?3210:3390;
            }
            return new Vector2(x,y);
        }
        private float BirthdayCameraTarget
        {
            get {
                var m=BirthdayOwn;
                if(m.seat>=0 || m.station=="zipline")return BirthdayNavigationPoint(m).x;
                return birthdayRightRoof?4900:100;
            }
        }
        private void UpdateBirthdayCamera()
        {
            if(CurrentArea!=HulkBirthday.Zone){birthdayCameraY=0;birthdayRoof=false;return;}
            var m=BirthdayOwn;float target=birthdayRoof?1400:0;
            if(m!=null && (m.seat>=0 || m.station=="zipline"))target=Mathf.Clamp(BirthdayNavigationPoint(m).y*.45f-250,0,1600);
            birthdayCameraY=Mathf.Lerp(birthdayCameraY,target,1-Mathf.Exp(-8*Time.unscaledDeltaTime));
        }
        private void BuildBirthdayNavigationUi()
        {
            birthdayNavigation=Rect(safe,"Birthday rooftop navigation",Vector2.zero,new Vector2(390,60));
            birthdayRoofText=BirthdayButton(birthdayNavigation,"Rooftops",new Vector2(-100,0),new Vector2(170,52),()=>{birthdayRoof=!birthdayRoof;manualCamera=false;CancelBirthdayIntent();});
            BirthdayButton(birthdayNavigation,"Other roof",new Vector2(90,0),new Vector2(170,52),()=>{birthdayRoof=true;birthdayRightRoof=!birthdayRightRoof;manualCamera=false;});
            for(int i=0;i<BirthdayNavigation.Groups.Length;i++){
                int group=i;var label=BirthdayButton(Board,"Birthday seats "+i,Vector2.zero,new Vector2(130,52),()=>{CancelBirthdayIntent();BirthdaySend("seat",target:group.ToString());});
                label.text=i==2?"Birthday chair":"Sit together";birthdaySeatTargets.Add((RectTransform)label.transform.parent);
            }
            for(int i=0;i<2;i++){
                int route=i;var label=BirthdayButton(Board,"Birthday zipline "+i,Vector2.zero,new Vector2(145,54),()=>{CancelBirthdayIntent();BirthdaySend("zipline",target:route.ToString());});label.text="Zipline!";birthdayZipTargets.Add((RectTransform)label.transform.parent);
                var wire=Plain(Board,"Birthday zipline cable "+i,Vector2.zero,Vector2.one,new Color(.25f,.3f,.38f),false);birthdayWires.Add(wire.rectTransform);
            }
            var throwText=BirthdayButton(safe,"Birthday throw",Vector2.zero,new Vector2(220,56),()=>BirthdaySend("throw"));birthdayThrowButton=(RectTransform)throwText.transform.parent;
            birthdaySeatActions=Rect(safe,"Birthday seat actions",Vector2.zero,new Vector2(480,60));
            var nap=BirthdayButton(birthdaySeatActions,"Nap or wake",new Vector2(-120,0),new Vector2(220,56),()=>BirthdaySend("nap"));birthdayNapButton=(RectTransform)nap.transform.parent;
            BirthdayButton(birthdaySeatActions,"Leave seat or zipline",new Vector2(120,0),new Vector2(220,56),()=>{birthdayRoof=false;BirthdaySend("station",target:"");});
        }
        private void TickBirthdayNavigationUi(bool visible)
        {
            if(birthdayNavigation==null)return;
            foreach(var pair in birthdayZipHandles.Values){pair.bar.gameObject.SetActive(false);pair.cord.gameObject.SetActive(false);}
            birthdayNapButton.gameObject.SetActive(visible && BirthdayOwn.seat>=0);
            birthdayNavigation.gameObject.SetActive(visible);birthdaySeatActions.gameObject.SetActive(visible && (BirthdayOwn.seat>=0 || BirthdayOwn.station=="zipline"));
            birthdayNavigation.anchoredPosition=new Vector2(safe.rect.width/2-235,safe.rect.height/2-190);
            birthdaySeatActions.anchoredPosition=new Vector2(0,-safe.rect.height/2+180);
            birthdayThrowButton.anchoredPosition=new Vector2(safe.rect.width/2-145,-safe.rect.height/2+180);
            birthdayThrowButton.gameObject.SetActive(visible && BirthdayOwn.station=="" && Birthday.props.Any(p=>p.holder==Actor && (BirthdayOwn.role!=HulkBirthday.Roles[0] || p.kind=="battery")));
            birthdayRoofText.text=birthdayRoof?"Party floor":"Rooftops";
            for(int i=0;i<birthdaySeatTargets.Count;i++){
                var group=BirthdayNavigation.Groups[i];float x=group.Average(n=>BirthdayNavigation.SeatX[n]),y=group.Average(n=>BirthdayNavigation.SeatY[n]);
                var p=ToBoard(x,y)+new Vector2(0,180*sceneScale);var rect=birthdaySeatTargets[i];rect.anchoredPosition=p;rect.localScale=Vector3.one*sceneScale;
                rect.gameObject.SetActive(visible && Math.Abs(p.y)<Board.rect.height/2+50);
            }
            for(int i=0;i<2;i++){
                var start=ToBoard(BirthdayNavigation.StartX[i],BirthdayNavigation.StartY[i]);var end=ToBoard(BirthdayNavigation.EndX[i],BirthdayNavigation.EndY[i]);
                var button=birthdayZipTargets[i];button.anchoredPosition=start+new Vector2(0,85*sceneScale);button.localScale=Vector3.one*sceneScale;button.gameObject.SetActive(visible);
                var wire=birthdayWires[i];wire.gameObject.SetActive(visible);wire.anchoredPosition=(start+end)/2+new Vector2(0,95*sceneScale);wire.sizeDelta=new Vector2(Vector2.Distance(start,end),3*sceneScale);wire.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(end.y-start.y,end.x-start.x)*Mathf.Rad2Deg);wire.SetAsFirstSibling();
            }
        }
        private void BirthdayDrawZipHandle(string actor,Vector2 wire,Vector2 left,Vector2 right)
        {
            if(!birthdayZipHandles.TryGetValue(actor,out var parts)){
                parts=(Plain(Board,"Birthday zipline handle "+actor,Vector2.zero,Vector2.one,new Color(.55f,.58f,.62f),false).rectTransform,Plain(Board,"Birthday zipline hook "+actor,Vector2.zero,Vector2.one,new Color(.4f,.43f,.48f),false).rectTransform);birthdayZipHandles.Add(actor,parts);
            }
            var centre=(left+right)/2;parts.bar.gameObject.SetActive(true);parts.cord.gameObject.SetActive(true);
            parts.bar.anchoredPosition=centre;parts.bar.sizeDelta=new Vector2(Vector2.Distance(left,right)+12*sceneScale,9*sceneScale);parts.bar.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(right.y-left.y,right.x-left.x)*Mathf.Rad2Deg);
            parts.cord.anchoredPosition=(wire+centre)/2;parts.cord.sizeDelta=new Vector2(Vector2.Distance(wire,centre),5*sceneScale);parts.cord.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(centre.y-wire.y,centre.x-wire.x)*Mathf.Rad2Deg);
        }
        private void AddBirthdayNavigationDepth(Action<RectTransform,float,int,string> add)
        {
            foreach(var pair in birthdayZipHandles){add(pair.Value.bar,float.MinValue,4,pair.Key+"-handle");add(pair.Value.cord,float.MinValue,3,pair.Key+"-hook");}
            foreach(var rect in birthdaySeatTargets.Concat(birthdayZipTargets))add(rect,float.MinValue,5,rect.name);
        }
        private void ResetBirthdayNavigationUi(){birthdayNavigation=birthdaySeatActions=birthdayThrowButton=birthdayNapButton=null;birthdayZipHandles.Clear();birthdayRoofText=null;birthdaySeatTargets.Clear();birthdayZipTargets.Clear();birthdayWires.Clear();birthdayCameraY=0;birthdayRoof=false;}
    }
}
