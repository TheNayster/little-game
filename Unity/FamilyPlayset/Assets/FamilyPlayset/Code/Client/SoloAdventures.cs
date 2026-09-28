using System;
using System.Linq;
using LittleWeeps.Adapters;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private ContinuationLibrary adventures;
        private ContinuationRecord continuation;
        private Action<string> openAdventure;
        private string soloSavePath;
        private string pendingContinuation;
        public string AdventureId=>continuation?.id??"";
        public void ConfigureAdventures(ContinuationLibrary library,Action<string> open)
        {if(safe!=null)throw new InvalidOperationException("Configure before Start.");adventures=library;openAdventure=open;}
        private bool TryDeselectAdventure()
        {try{adventures?.Select("");return true;}catch(Exception){message.text="Your saved play needs a grown-up's help.";return false;}}
        private void OpenInitialAdventure()
        {
            if(adventures==null || adventures.Selected=="")return;
            TryOpenAdventure(adventures.Selected);
        }
        // Capture once before disconnect callbacks cancel drags and clear motion
        // history. UI coordinates describe the last displayed frame, not a new
        // transaction; they are used only in this private local world.
        private string renderedArea;
        private long renderedVisit;
        public SoloSnapshot CaptureVisibleLocalStart()
        {
            if(!Shared || !Ready)return null;
            var snapshot=SoloWorld.CopySnapshot(shared.View);
            var own=snapshot.players.First(p=>p.id==Actor);
            if(own.stairs==0 && renderedArea==own.zone && renderedVisit==own.visit && Board.rect.width>0 && Board.rect.height>0)
            {
                Vector2 Point(RectTransform rect)=>FromBoard(rect.anchoredPosition);
                var position=Point(avatar);
                if(WorldLayout.Position(own.zone,snapshot.schema,position.x,position.y)){own.x=position.x;own.y=position.y;}
                foreach(var toy in snapshot.toys.Where(t=>t.zone==own.zone && !string.IsNullOrEmpty(t.holder) && snapshot.players.First(p=>p.id==t.holder).stairs==0))
                    if(toys.TryGetValue(toy.id,out var rect) && rect.gameObject.activeSelf)
                    {
                        var point=Point(rect);
                        if(WorldLayout.Position(toy.zone,snapshot.schema,point.x,point.y)){toy.x=point.x;toy.y=point.y;}
                    }
            }
            SoloWorld.Validate(snapshot);return snapshot;
        }
        public bool TryContinue(LocalViewOrigin origin)
        {
            if(adventures==null || !CanChangeSession || (MenuOpen && !RecoveringDisconnected) || (!Shared && continuation!=null))return false;
            try
            {
                if(World!=null && !TrySaveNow())return false;
                if(pendingContinuation==null)pendingContinuation=adventures.CreateVisible(origin).id;
                if(!TryOpenAdventure(pendingContinuation))return false;
                pendingContinuation=null;return true;
            }
            catch(Exception e){Debug.LogWarning("Adventure continuation: "+e.Message);message.text="Your saved play needs a grown-up's help.";return false;}
        }
        public bool TryOpenAdventure(string id)
        {
            if(adventures==null || !CanChangeSession)return false;
            try
            {
                if(World!=null && !TrySaveNow())return false;
                var record=adventures.Load(id);var restored=SoloWorld.WithMealPreparation(SoloWorld.Restore(record.snapshot));
                // Release all old pointer leases and commit before changing the
                // displayed authority. Failed disk writes leave current play intact.
                adventures.Save(record,restored.Snapshot());adventures.Select(id);
                if(World!=null && continuation==null)localWorld=World;
                var keepMenu=MenuOpen;
                ResetPresentation();shared=null;continuation=record;World=restored;Actor=record.actor;SavePath=adventures.PathFor(id);dirty=false;
                BuildScreen();Render();lastLocalAction=Time.realtimeSinceStartup;
                if(keepMenu)SetMenu(true);
                message.text="Keep playing here. This adventure saves separately from family play.";return true;
            }
            catch(Exception e){Debug.LogWarning("Open adventure: "+e.Message);message.text="That adventure needs a grown-up's help. Your other play is safe.";return false;}
        }
        private GameObject adventurePanel;
        private void ShowAdventures(int page)
        {
            if(adventurePanel!=null){adventurePanel.SetActive(false);Destroy(adventurePanel);}
            adventurePanel=Panel(menu.transform,"Saved adventures panel",Vector2.zero,new Vector2(1190,760),Cream,true).gameObject;
            Label(adventurePanel.transform,"Your saved adventures",38,new Vector2(0,290),new Vector2(960,80));
            var branches=adventures.Branches();var pages=Math.Max(1,(branches.Length+2)/3);page=Mathf.Clamp(page,0,pages-1);
            Label(adventurePanel.transform,branches.Length==0?"An adventure will appear here if the connection stops.":"Each adventure keeps its own toys and progress.",22,new Vector2(0,210),new Vector2(980,60));
            for(var i=0;i<3 && page*3+i<branches.Length;i++)
            {
                var id=branches[page*3+i];var number=branches.Length-(page*3+i);
                Button(adventurePanel.transform,"Adventure "+number,new Vector2(0,105-i*100),new Vector2(600,75),()=>{SetMenu(false);openAdventure(id);},new Color(.81f,.92f,.72f));
            }
            if(page>0)Button(adventurePanel.transform,"Newer",new Vector2(-330,-205),new Vector2(220,60),()=>ShowAdventures(page-1),Cream);
            if(page+1<pages)Button(adventurePanel.transform,"Older",new Vector2(330,-205),new Vector2(220,60),()=>ShowAdventures(page+1),Cream);
            Button(adventurePanel.transform,"My solo play",new Vector2(0,-205),new Vector2(300,65),()=>{SetMenu(false);openAdventure("");},new Color(.77f,.88f,.96f));
            Button(adventurePanel.transform,"Back to menu",new Vector2(0,-305),new Vector2(350,65),()=>{adventurePanel.SetActive(false);Destroy(adventurePanel);},Cream);
        }
    }
}
