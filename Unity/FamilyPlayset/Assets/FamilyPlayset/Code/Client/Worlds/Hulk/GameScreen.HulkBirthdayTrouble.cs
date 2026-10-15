using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LittleWeeps.Client
{
    public sealed class BirthdayVillainTap:Button
    {
        public Action pressed;
        public override void OnPointerDown(PointerEventData e){if(!IsActive() || !IsInteractable())return;base.OnPointerDown(e);pressed?.Invoke();}
    }
    public sealed class BirthdayWebs:MaskableGraphic
    {
        public Vector2[] endpoints=Array.Empty<Vector2>();
        protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();for(int i=0;i+1<endpoints.Length;i+=2){var a=endpoints[i];var b=endpoints[i+1];var n=new Vector2(-(b-a).y,(b-a).x).normalized*2;int start=vh.currentVertCount;foreach(var p in new[]{a-n,a+n,b-n,b+n})vh.AddVert(p,color,Vector2.zero);vh.AddTriangle(start,start+2,start+1);vh.AddTriangle(start+1,start+2,start+3);}}
    }
    public sealed partial class GameScreen
    {
        private string birthdayVillainApproach="";
        private long birthdayVillainVisit=-1;
        private readonly Dictionary<string,Text> birthdayPeek=new Dictionary<string,Text>();
        private BirthdayWebs birthdayWebs;
        private BirthdayBubble birthdaySmashEffect;
        private Text birthdayTroubleHint,birthdayLeaveWeb;
        private bool birthdayWebLeave;
        private readonly Dictionary<string,Vector2> birthdayPetPositions=new Dictionary<string,Vector2>();
        private readonly Dictionary<string,bool> birthdayPetMirrors=new Dictionary<string,bool>();
        private readonly Dictionary<string,string> birthdayPetPoses=new Dictionary<string,string>();
        public string[] BirthdayPetPoses=>birthdayPetPoses.Select(p=>p.Key+"="+p.Value).ToArray();
        private void BeginBirthdayVillain(string id){CancelBirthdayIntent();birthdayVillainApproach=id;birthdayVillainVisit=ReadPlayer(Actor).visit;}
        private void BuildBirthdayTroubleUi()
        {
            foreach(var v in Birthday.villains){
                birthdayPeople.Add(v.id,BirthdayImage("Birthday artwork "+v.id));var face=Plain(Board,"Birthday villain "+v.id,Vector2.zero,new Vector2(80,90),Color.clear,true);birthdayFaces.Add(v.id,face);face.gameObject.AddComponent<BirthdayVillainTap>().pressed=()=>BeginBirthdayVillain(v.id);
                birthdayPeek.Add(v.id,BirthdayButton(Board,"Birthday hiding spot "+v.id,Vector2.zero,new Vector2(135,60),()=>BeginBirthdayVillain(v.id)));
            }
            birthdayWebs=Rect(Board,"Birthday shared webs",Vector2.zero,Vector2.zero).gameObject.AddComponent<BirthdayWebs>();birthdayWebs.color=new Color(.95f,.98f,1,.95f);birthdayWebs.raycastTarget=false;
            birthdaySmashEffect=Rect(Board,"Birthday smash effect",Vector2.zero,new Vector2(100,100)).gameObject.AddComponent<BirthdayBubble>();birthdaySmashEffect.raycastTarget=false;birthdaySmashEffect.color=Color.white;
            birthdayTroubleHint=Label(safe,"",22,Vector2.zero,new Vector2(700,45));
            birthdayLeaveWeb=BirthdayButton(birthdayPartyActions,"Let go of web",new Vector2(0,0),new Vector2(230,60),()=>birthdayWebLeave=true);
        }
        private IEnumerable<BirthdayNpc> BirthdayVillainViews(BirthdayState b)
        {
            foreach(var v in b.villains)yield return new BirthdayNpc{id=v.id,role=v.role,active=v.phase!="hidden" && v.phase!="cooldown",x=v.x,y=v.y,
                targetX=v.targetX,targetY=v.targetY,phase=v.phase=="approach" || v.phase=="fake-approach" || v.phase=="run"?"enter":"idle",station=v.phase=="trapped"?"shake":v.phase=="launched"?"freak":"",actionAt=v.phaseAt};
        }
        private Vector2 BirthdayHeroPoint(string id,Vector2 point)
        {
            var b=Birthday;
            double clock=BirthdayVisualTime;
            var v=b.villains.FirstOrDefault(v=>v.id==id);
            if(v?.phase=="launched"){float age=(float)(clock-v.phaseAt);point.y+=Mathf.Sin(Mathf.Clamp01(age/2.5f)*Mathf.PI)*700;}
            if(b.heroActor==id && b.heroAt>=0){var target=b.villains.Single(v=>v.id==b.heroTarget);float phase=Mathf.Clamp01((float)(clock-b.heroAt)/1.35f);point.x=Mathf.Lerp(point.x,target.x-60,phase);point.y+=Mathf.Sin(phase*Mathf.PI)*2.5f*BirthdayArt.PixelsPerUnit/.45f;}
            return point;
        }
        private void TickBirthdayTroubleUi(bool visible)
        {
            if(birthdayWebs==null)return;
            var b=Birthday;var own=ReadPlayer(Actor);var member=b.members.Single(m=>m.actor==Actor);
            birthdayTroubleHint.gameObject.SetActive(visible && b.villain>=0);birthdayTroubleHint.rectTransform.anchoredPosition=new Vector2(0,safe.rect.height/2-125);
            birthdayTroubleHint.text=b.villain>=0?(b.villains[b.villain].phase=="trapped"?"Hulk, finish the rescue!":"Find the villain and help together!"):"";
            birthdayLeaveWeb.transform.parent.gameObject.SetActive(visible && member.station=="web");
            birthdayWebs.gameObject.SetActive(visible);birthdaySmashEffect.gameObject.SetActive(false);
            foreach(var pair in birthdayPeek){var v=b.villains.Single(v=>v.id==pair.Key);pair.Value.transform.parent.gameObject.SetActive(visible && v.phase=="hidden");pair.Value.text="Peek!";var rect=(RectTransform)pair.Value.transform.parent;rect.anchoredPosition=ToBoard(v.x,v.y)+new Vector2(0,50)*sceneScale;rect.localScale=Vector3.one*sceneScale;}
            if(!visible){birthdayVillainApproach="";birthdayWebLeave=false;return;}
            if(birthdayWebLeave && !ActionPending){birthdayWebLeave=false;BirthdaySend("station",target:"");}
            if(birthdayVillainApproach!=""){
                var v=b.villains.Single(v=>v.id==birthdayVillainApproach);float range=member.role==HulkBirthday.Roles[0] && v.phase=="trapped"?240:550;
                if(own.visit!=birthdayVillainVisit || v.phase=="cooldown" || v.phase=="launched")birthdayVillainApproach="";
                else if(Mathf.Abs(own.x-v.x)<range && Mathf.Abs(own.y-v.y)<250 && !ActionPending){
                    string action=v.phase=="hidden"?"peek":member.role==HulkBirthday.Roles[0]?(v.phase=="trapped"?"smash":"help"):"web";
                    // A spider finding the chosen hiding spot follows through
                    // with its web, rather than requiring another moving tap.
                    if(action!="peek" || member.role==HulkBirthday.Roles[0])birthdayVillainApproach="";
                    destination=null;BirthdaySend(action,target:v.id);
                }
                else destination=new Vector2(Mathf.Clamp(v.x+(own.x<v.x?-range*.6f:range*.6f),0,4800),v.y);
            }
            var points=new List<Vector2>();
            foreach(var v in b.villains.Where(v=>v.phase=="trapped" && birthdayPositions.ContainsKey(v.id))){
                var end=ToBoard(v.x,v.y)+new Vector2(0,50)*sceneScale;
                foreach(var id in v.webbers)if(birthdayFrames.ContainsKey(id)){points.Add(BirthdayHandPoint(id,false));points.Add(end);}
            }
            birthdayWebs.endpoints=points.ToArray();birthdayWebs.SetVerticesDirty();
            if(b.reaction=="rescue-happy" && b.clock-b.reactionAt<.85 && b.villain>=0){var v=b.villains[b.villain];float t=(float)((b.clock-b.reactionAt)/.85);birthdaySmashEffect.gameObject.SetActive(true);birthdaySmashEffect.rectTransform.anchoredPosition=ToBoard(v.x,v.y);birthdaySmashEffect.rectTransform.sizeDelta=Vector2.one*(90+t*210)*sceneScale;birthdaySmashEffect.color=new Color(.8f,1,.8f,1-t);}
        }
        private void DrawBirthdayPet(BirthdayProp prop,RawImage image)
        {
            if(prop.kind!="gift" || !prop.opened || prop.content<6 || prop.holder!="" || prop.location!="ground"){birthdayPetPoses.Remove(prop.id);return;}
            var point=new Vector2(prop.x,prop.y);var delta=birthdayPetPositions.TryGetValue(prop.id,out var old)?point-old:Vector2.zero;birthdayPetPositions[prop.id]=point;
            if(Mathf.Abs(delta.x)>.01f)birthdayPetMirrors[prop.id]=delta.x<0;
            bool mirror=birthdayPetMirrors.TryGetValue(prop.id,out var left) && left;
            string view=prop.petMoving?"side":"front",pose=prop.petMoving?"walk":"idle";var action=BirthdayArt.Action(HulkBirthday.Contents[prop.content],pose+(view=="front"?"":"-"+view));
            if(action==null)throw new InvalidOperationException("Recovered pet motion is missing.");int frame=Mathf.Min(action.frames-1,(int)(Time.unscaledTimeAsDouble%Math.Max(.001,action.duration)*BirthdayArt.Data.fps));
            image.texture=BirthdayArt.Texture(action.texture);var uv=BirthdayArt.Uv(action,frame);image.uvRect=mirror?new Rect(uv.x+uv.width,uv.y,-uv.width,uv.height):uv;
            image.rectTransform.pivot=new Vector2(mirror?1-action.pivot.x/256:action.pivot.x/256,action.pivot.y/256);image.rectTransform.sizeDelta=Vector2.one*256*action.unitsPerPixel*BirthdayArt.PixelsPerUnit*sceneScale;image.rectTransform.anchoredPosition=ToBoard(point.x,point.y);
            birthdayPetPoses[prop.id]=pose+"-"+view+":"+frame+(mirror?":mirrored":"");
        }
        private void AddBirthdayTroubleDepth(Action<RectTransform,float,int,string> add)
        {
            if(birthdayWebs==null)return;
            foreach(var v in Birthday.villains){add(birthdayPeople[v.id].rectTransform,ToBoard(0,v.y).y,1,"birthday-"+v.id);add(birthdayFaces[v.id].rectTransform,ToBoard(0,v.y).y,4,"birthday-face-"+v.id);add((RectTransform)birthdayPeek[v.id].transform.parent,-1500,4,"birthday-peek-"+v.id);}
            add(birthdayWebs.rectTransform,-1000,3,"birthday-webs");add(birthdaySmashEffect.rectTransform,-1000,3,"birthday-smash-effect");
        }
        private void ResetBirthdayTroubleUi(){birthdayVillainApproach="";birthdayVillainVisit=-1;birthdayPeek.Clear();birthdayWebs=null;birthdaySmashEffect=null;birthdayTroubleHint=null;birthdayLeaveWeb=null;birthdayPetPositions.Clear();birthdayPetMirrors.Clear();birthdayPetPoses.Clear();birthdayWebLeave=false;}
    }
}
