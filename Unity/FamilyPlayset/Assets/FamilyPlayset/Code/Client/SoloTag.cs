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
        private RectTransform tagHud,tagNpc;
        private Text tagHint,tagJoinText,tagCount;
        private GameCharacterVisual tagNpcVisual;
        private readonly Dictionary<string,Text> tagMarkers=new Dictionary<string,Text>();
        private string tagVisualWorld="";
        private Vector2 tagNpcPoint,tagNpcVelocity;
        private bool tagSending;
        private TagState tagFrame;private int tagFrameNumber=-1;private long tagFrameRevision=-1;
        public bool TagNpcVisible=>tagNpc!=null && tagNpc.gameObject.activeInHierarchy;
        public TagState TagGame {get{if(!HasWorld)return null;if(Shared)return shared.View.park?.tag;
            if(tagFrameNumber!=Time.frameCount || tagFrameRevision!=World.Revision){tagFrame=World.ReadTag();tagFrameNumber=Time.frameCount;tagFrameRevision=World.Revision;}return tagFrame;}}
        private void SendTag(bool leave=false)
        {
            if(tagSending || !Ready || TravelPending)return;
            CloseMiniGames();CancelPointers();Narration.Stop();destination=null;tagSending=true;
            void Done(SoloResult result){tagSending=false;Render();}
            if(Shared){if(!SubmitShared(SoloAction.Park,"","",leave?"tag-leave":"tag-join",0,0,Done))tagSending=false;}
            else Done(Command(SoloAction.Park,value:leave?"tag-leave":"tag-join"));
        }
        private void BuildTag()
        {
            tagNpc=Rect(Board,"Tag Bandit playmate",Vector2.zero,Vector2.zero);tagNpcVisual=tagNpc.gameObject.AddComponent<GameCharacterVisual>();tagNpcVisual.Select("bandit");
            tagHud=Rect(safe,"Tag controls",Vector2.zero,new Vector2(780,90));tagHud.anchorMin=tagHud.anchorMax=new Vector2(.5f,0);tagHud.anchoredPosition=new Vector2(0,130);
            tagHint=Label(tagHud,"",25,new Vector2(-150,0),new Vector2(430,80));
            tagJoinText=Button(tagHud,"Join tag",new Vector2(225,0),new Vector2(255,80),()=>SendTag(ParkTag.Member(TagGame,Actor)),Cream);
            tagCount=Label(safe,"",95,new Vector2(0,100),new Vector2(350,145));tagCount.fontStyle=FontStyle.Bold;tagCount.color=new Color(.85f,.35f,.07f);
            foreach(var id in AllPlayersForTag().Select(p=>p.id).Concat(new[]{ParkTag.Npc})){
                var root=Rect(Board,"Tag marker "+id,Vector2.zero,new Vector2(125,65));
                var mark=Label(root,"★",56,Vector2.zero,new Vector2(125,65));mark.color=new Color(1,.57f,.08f);tagMarkers[id]=mark;
            }
        }
        private SoloPlayer[] AllPlayersForTag()=>Shared?shared.View.players:World.ReadPlayers();
        private void AddTagChoice()
        {
            var label=MiniGameChoice("park","Tag",new Vector2(0,-105),new Vector2(700,190),()=>SendTag(),new Color(.93f,.86f,.54f));
            var root=(RectTransform)label.transform.parent;root.anchorMin=root.anchorMax=new Vector2(.5f,1);
            label.rectTransform.anchoredPosition=new Vector2(120,28);label.rectTransform.sizeDelta=new Vector2(350,60);label.fontSize=40;
            HomePicture(root,"Tag picture",new Vector2(-215,0),new Vector2(150,150),banditFrames[1]);
            Label(root,"Chase and swap turns!",25,new Vector2(120,-35),new Vector2(400,75));
        }
        private void PresentTag()
        {
            if(tagHud==null)return;var t=TagGame;var visible=CurrentArea=="park" && !WorldLoading && t!=null && t.phase!=TagPhase.Idle;
            var own=visible && ParkTag.Member(t,Actor);tagHud.gameObject.SetActive(visible && !MenuOpen);tagCount.gameObject.SetActive(own && !MenuOpen && t.phase==TagPhase.Counting);
            tagNpc.gameObject.SetActive(visible && t.members.Length==1);
            if(visible){tagHint.text=!own?"Friends are playing tag!":t.phase==TagPhase.Counting?"Get ready together!":t.it==Actor?"You have the star. Chase a friend!":"Run! Follow the orange star.";
                tagJoinText.text=own?"All done":"Join tag";tagCount.text=Math.Max(1,Math.Ceiling(t.starts-t.clock)).ToString();
                if(tagNpc.gameObject.activeSelf){var world=Shared?shared.View.worldId:World.WorldId;
                    var target=new Vector2(t.npcX,t.npcY);
                    if(tagVisualWorld!=world){tagVisualWorld=world;tagNpcPoint=target;tagNpcVelocity=Vector2.zero;}
                    tagNpcPoint=Vector2.SmoothDamp(tagNpcPoint,target,ref tagNpcVelocity,.1f,1000,Time.unscaledDeltaTime);
                    tagNpc.anchoredPosition=ToBoard(tagNpcPoint.x,tagNpcPoint.y);tagNpc.localScale=Vector3.one*sceneScale;
                    tagNpcVisual.Present(tagNpcPoint,world+"/tag",false,Time.unscaledDeltaTime);
                }
            }
            foreach(var pair in tagMarkers){var id=pair.Key;var show=visible && (id==ParkTag.Npc?t.members.Length==1:ParkTag.Member(t,id));
                pair.Value.transform.parent.gameObject.SetActive(show);if(!show)continue;
                RectTransform body=id==ParkTag.Npc?tagNpc:id==Actor?avatar:friends.TryGetValue(id,out var friend)?friend.root:null;
                if(body==null){pair.Value.transform.parent.gameObject.SetActive(false);continue;}
                var root=(RectTransform)pair.Value.transform.parent;root.anchoredPosition=body.anchoredPosition+new Vector2(0,245)*sceneScale;root.localScale=Vector3.one*sceneScale;
                pair.Value.text=t.it==id?"★":"●";pair.Value.color=t.it==id?new Color(1,.57f,.08f):new Color(.17f,.69f,.76f);
            }
            SortDepth();
        }
        private void AddTagDepth(Action<RectTransform,float,int,string> add)
        {if(tagNpc!=null)add(tagNpc,tagNpc.anchoredPosition.y,1,ParkTag.Npc);foreach(var p in tagMarkers)if(p.Value!=null)add((RectTransform)p.Value.transform.parent,-10000,2,p.Key+"-tag-marker");}
        private void ResetTag()
        {tagHud=null;tagNpc=null;tagNpcVisual=null;tagHint=null;tagJoinText=null;tagCount=null;tagMarkers.Clear();tagVisualWorld="";tagNpcVelocity=Vector2.zero;tagFrame=null;tagFrameNumber=-1;tagFrameRevision=-1;tagSending=false;}
    }
}
