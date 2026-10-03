using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client.Worlds.Beach
{
    // Owns the Beach flock sprites, animation clock and tracks; receives shared widget factories.
    internal sealed class SeagullFlockView
    {
        private readonly RectTransform[] gullRoots=new RectTransform[BeachSeagulls.Count];
        private readonly Image[] gullPictures=new Image[BeachSeagulls.Count];
        private readonly RectTransform[] gullTracks=new RectTransform[16];
        private readonly Sprite[] gullSprites=new Sprite[4];
        private Texture2D gullTexture;
        private double gullDisplayClock,gullSampleClock=-1;
        private string gullWorld="";
        private static readonly Rect[] GullBounds={new Rect(153,103,452,395),new Rect(937,39,497,449),new Rect(142,624,567,357),new Rect(810,582,693,314)};
        private static readonly Vector2[] GullBodies={new Vector2(420,300),new Vector2(1210,390),new Vector2(440,720),new Vector2(1190,790)};
        internal int VisibleCount=>gullRoots.Count(r=>r!=null && r.gameObject.activeInHierarchy);
        internal int VisibleTracks=>gullTracks.Count(r=>r!=null && r.gameObject.activeInHierarchy);
        internal void Build(Transform board,Func<Transform,string,Vector2,Vector2,RectTransform> rect,
            Func<Transform,string,Vector2,Vector2,Sprite,Image> picture,
            Action<Transform,string,Vector2,Vector2,Action> hit,
            Func<Transform,string,Vector2,Vector2,Color,Image> panel,Action sayHello)
        {
            gullTexture=WorldResources.Load<Texture2D>("Worlds/Beach/Art/seagull-poses");
            if(gullTexture==null)throw new InvalidOperationException("Missing seagull poses.");
            for(var i=0;i<4;i++){
                var b=GullBounds[i];var sx=gullTexture.width/1536f;var sy=gullTexture.height/1024f;
                gullSprites[i]=Sprite.Create(gullTexture,new Rect(b.x*sx,(1024-b.y-b.height)*sy,b.width*sx,b.height*sy),new Vector2(.5f,.5f));
            }
            for(var i=0;i<gullRoots.Length;i++){
                var root=rect(board,"Beach gull "+i,Vector2.zero,Vector2.zero);gullRoots[i]=root;
                gullPictures[i]=picture(root,"Silver gull",Vector2.zero,Vector2.one,gullSprites[0]);
                hit(root,"Say hello to seagull "+(i+1),new Vector2(0,38),new Vector2(110,100),sayHello);
            }
            for(var i=0;i<gullTracks.Length;i++){
                var root=rect(board,"Beach bird tracks "+i,Vector2.zero,Vector2.zero);gullTracks[i]=root;
                for(var toe=0;toe<3;toe++){
                    var mark=panel(root,"Toe "+toe,new Vector2((toe-1)*3,0),new Vector2(2,10),new Color(.62f,.43f,.22f,.5f));
                    mark.raycastTarget=false;mark.rectTransform.localRotation=Quaternion.Euler(0,0,(toe-1)*35);
                }
            }
        }
        internal void Tick(SeagullState g,string worldId,bool visible,float sceneScale,
            Func<float,float,Vector2> toBoard,float unscaledDeltaTime)
        {
            if(g==null || gullRoots[0]==null)return;
            if(gullWorld!=worldId || g.clock<gullSampleClock){gullWorld=worldId;gullDisplayClock=g.clock;}
            gullSampleClock=g.clock;gullDisplayClock=Math.Max(g.clock,Math.Min(g.clock+.1,gullDisplayClock+Math.Min(.1,unscaledDeltaTime)));
            var ahead=g.phase==GullPhase.Resting?0:Math.Max(0,gullDisplayClock-g.clock);
            for(var i=0;i<gullRoots.Length;i++){
                var root=gullRoots[i];root.gameObject.SetActive(visible);if(!visible)continue;
                var p=BeachSeagulls.Point(g,i,ahead,out var height);var flying=g.phase>=GullPhase.TakeOff;
                var pose=flying?new[]{1,2,1,3}[(int)((gullDisplayClock+i*.045)*6)%4]:0;
                var left=flying?g.to<g.from:i%3==0;var b=GullBounds[pose];var body=GullBodies[pose];
                root.anchoredPosition=toBoard(p.X,p.Y)+Vector2.up*(height*sceneScale);root.localScale=Vector3.one*sceneScale;
                var pic=gullPictures[i];pic.sprite=gullSprites[pose];pic.rectTransform.sizeDelta=new Vector2(b.width,b.height)*.19f;
                var offset=new Vector2((b.center.x-body.x)*.19f,38+(body.y-b.center.y)*.19f);
                pic.rectTransform.anchoredPosition=new Vector2(left?-offset.x:offset.x,offset.y);
                pic.rectTransform.localScale=new Vector3(left?-1:1,1,1);
                root.Find("Say hello to seagull "+(i+1)).gameObject.SetActive(g.phase==GullPhase.Resting);
            }
            for(var i=0;i<gullTracks.Length;i++){
                var root=gullTracks[i];root.gameObject.SetActive(visible && g.flights>0);
                var start=BeachSeagulls.Patches[g.trailFrom];
                // Marks are a short readable trail toward the flock, not a new
                // quest, reward or persistent player-footprint system.
                var end=BeachSeagulls.Patches[g.to];var t=(i+1)/17f;
                root.anchoredPosition=toBoard(Mathf.Lerp(start,end,t),BeachSeagulls.GroundY+(i%2==0?-28:-8));root.localScale=Vector3.one*sceneScale;
            }
        }
        internal void AddDepth(SeagullState g,Func<float,float,Vector2> toBoard,Action<RectTransform,float,int,string> add)
        {
            if(g==null)return;
            for(var i=0;i<gullRoots.Length;i++){var p=BeachSeagulls.Point(g,i,0,out _);add(gullRoots[i],toBoard(p.X,p.Y).y,g.phase>=GullPhase.TakeOff?8:2,"gull-"+i);}
            foreach(var track in gullTracks)add(track,track==null?0:track.anchoredPosition.y,0,"bird-track");
        }
        internal void Reset()
        {
            foreach(var sprite in gullSprites)if(sprite!=null)UnityEngine.Object.Destroy(sprite);
            Array.Clear(gullSprites,0,4);Array.Clear(gullRoots,0,gullRoots.Length);Array.Clear(gullPictures,0,gullPictures.Length);Array.Clear(gullTracks,0,gullTracks.Length);
            if(gullTexture!=null)Resources.UnloadAsset(gullTexture);gullTexture=null;gullSampleClock=-1;gullWorld="";
        }
    }
}
