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
        private readonly Dictionary<string,(RectTransform root,float x,float y)> homeObjects=new Dictionary<string,(RectTransform,float,float)>();
        private readonly List<Sprite> homeSprites=new List<Sprite>();
        private readonly List<Texture2D> homeTextures=new List<Texture2D>();
        private readonly List<Image> storageHints=new List<Image>();
        private Image shedPicture;
        private readonly Dictionary<string,RectTransform> homeFronts=new Dictionary<string,RectTransform>();
        private HomeArtPart.Layout homeLayerLayout;
        private HomeArtPart shedFront;
        private Sprite shedOpenSprite,shedClosedSprite;
        private AudioSource homeMusic;
        private Text musicSetting,homeFeedback;
        private float homeFeedbackUntil;
        private bool musicMuted;
        public HomeState Home=>HasWorld?(Shared?shared.View.home:World.ReadHome()):null;
        public string HomePose=>characterVisual==null?"":characterVisual.Frame.Pose.ToString();
        public float HomePoseAge=>characterVisual==null?0:characterVisual.Frame.UseSeconds;
        public bool HomeMusicPlaying=>homeMusic!=null && homeMusic.isPlaying && homeMusic.volume>0;
        public bool MusicMuted=>musicMuted;

        private Sprite HomeSprite(string id)
        {
            var texture=Resources.Load<Texture2D>("HomeArt/"+id);
            if(texture==null)throw new InvalidOperationException("Missing home art: "+id);
            homeTextures.Add(texture);
            var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));
            homeSprites.Add(sprite);return sprite;
        }
        private RectTransform HomeObject(string id,float x,float y)
        {
            var root=Rect(Board,id,ToBoard(x,y),Vector2.zero);homeObjects.Add(id,(root,x,y));return root;
        }
        private Image HomePicture(Transform parent,string id,Vector2 pos,Vector2 size,Sprite sprite)
        {
            var pic=Rect(parent,id,pos,size).gameObject.AddComponent<Image>();pic.sprite=sprite;pic.raycastTarget=false;return pic;
        }
        private HomeArtPart HomeFront(string fixture,string partId,Vector2 pos,Vector2 size,Sprite sprite)
        {
            var obj=homeObjects[fixture];
            var root=Rect(Board,fixture+" front",ToBoard(obj.x,obj.y),Vector2.zero);
            homeFronts.Add(fixture,root);
            var part=Rect(root,partId,pos,size).gameObject.AddComponent<HomeArtPart>();
            part.Configure(sprite.texture,homeLayerLayout.parts.Single(p=>p.id==partId).polygons);
            return part;
        }
        private void HomeHit(Transform parent,string id,Vector2 pos,Vector2 size,Action action)
        {
            var hit=Plain(parent,id,pos,size,Color.clear,true);hit.canvasRenderer.cullTransparentMesh=false;
            NavButton(hit,()=>{if(MenuOpen || TravelPending || !Ready)return;CancelPointers();action();});
        }
        private void HomeAction(SoloAction action,string target,string value="")
        {
            void Done(SoloResult result)
            {
                if(!result.Accepted)
                {
                    homeFeedback.text=result.Outcome=="fixture-busy"?"That spot is busy. Try the other one!":"Try that again.";
                    homeFeedbackUntil=Time.unscaledTime+2.5f;
                }
                Render();
            }
            if(Shared)SubmitShared(action,"",target,value,0,0,Done);
            else Done(Command(action,target:target,value:value));
        }
        private void UseHome(string slot)
        {HomeAction(ReadPlayer(Actor).fixture==slot?SoloAction.LeaveFixture:SoloAction.UseFixture,slot);}
        private void BuildHome()
        {
            if(Home==null)return;
            homeLayerLayout=JsonUtility.FromJson<HomeArtPart.Layout>(Resources.Load<TextAsset>("HomeArt/layer-layout").text);
            var sofa=HomeObject("Home sofa",HomeLayout.SofaX,HomeLayout.SofaY);
            var sofaSprite=HomeSprite("home-seat");
            HomePicture(sofa,"Sofa rear and seats",new Vector2(0,30),new Vector2(570,285),sofaSprite);
            HomeFront("Home sofa","sofa-front",new Vector2(0,30),new Vector2(570,285),sofaSprite);
            HomeHit(sofa,"Sit left",new Vector2(-100,45),new Vector2(185,220),()=>UseHome("sofa-left"));
            HomeHit(sofa,"Sit right",new Vector2(100,45),new Vector2(185,220),()=>UseHome("sofa-right"));
            var trampoline=HomeObject("Home trampoline",HomeLayout.TrampolineX,HomeLayout.TrampolineY);
            var trampolineSprite=HomeSprite("home-trampoline");
            HomePicture(trampoline,"Trampoline rear and mat",new Vector2(0,25),new Vector2(560,280),trampolineSprite);
            HomeFront("Home trampoline","trampoline-front",new Vector2(0,25),new Vector2(560,280),trampolineSprite);
            HomeHit(trampoline,"Bounce left",new Vector2(-110,60),new Vector2(200,195),()=>UseHome("trampoline-left"));
            HomeHit(trampoline,"Bounce right",new Vector2(110,60),new Vector2(200,195),()=>UseHome("trampoline-right"));
            var radioSprite=HomeSprite("home-radio");
            foreach(var name in new[]{"living","garden"})
            {
                var place=name;var root=HomeObject("Radio "+place,place=="living"?-3480:1230,170);
                HomePicture(root,"Radio",Vector2.zero,new Vector2(145,97),radioSprite);
                HomeHit(root,"Radio "+place+" power",Vector2.zero,new Vector2(160,120),()=>HomeAction(SoloAction.SetFixture,"radio-"+place,
                    (place=="living"?Home.livingRadio:Home.gardenRadio)?"off":"on"));
                var notes=Label(root,"♪  ♫",36,new Vector2(0,88),new Vector2(180,70));notes.name="Music notes";notes.color=new Color(.25f,.6f,.35f);
            }
            var shed=HomeObject("Home shed",HomeLayout.ShedX,HomeLayout.ShedY);
            shedOpenSprite=HomeSprite("home-shed");shedClosedSprite=HomeSprite("home-shed-closed");
            shedPicture=HomePicture(shed,"Shed",new Vector2(0,25),new Vector2(560,373),shedClosedSprite);
            shedFront=HomeFront("Home shed","shed-front",new Vector2(0,25),new Vector2(560,373),shedOpenSprite);
            HomeHit(shed,"Shed doors",new Vector2(0,130),new Vector2(330,90),()=>HomeAction(SoloAction.SetFixture,"shed",Home.shedOpen?"off":"on"));
            // The handle remains reachable when the interior has stored toys.
            var latch=Panel(shed,"Door handle",new Vector2(0,130),new Vector2(58,22),new Color(.69f,.8f,.82f));
            for(var i=0;i<4;i++)
            {
                var hint=Panel(shed,"Storage place "+i,new Vector2(HomeLayout.StorageX(i)-HomeLayout.ShedX,(HomeLayout.StorageY(i)-HomeLayout.ShedY)*.45f),new Vector2(130,90),new Color(1,.89f,.45f,.7f));
                hint.sprite=hintRing;hint.type=Image.Type.Simple;storageHints.Add(hint);
            }
            homeFeedback=Label(safe,"",25,new Vector2(0,230),new Vector2(680,70));
            musicMuted=PlayerPrefs.GetInt(PreferenceKey("music-muted"),0)!=0;
            musicSetting=Button(menu.transform,musicMuted?"Music off":"Music on",new Vector2(410,35),new Vector2(250,75),ToggleMusic,new Color(.77f,.88f,.96f));
            musicSetting.transform.parent.name="Music setting";
            homeMusic=gameObject.AddComponent<AudioSource>();homeMusic.playOnAwake=false;homeMusic.loop=true;homeMusic.spatialBlend=0;
            homeMusic.clip=Resources.Load<AudioClip>("HomeArt/home-music");
            TickHome();
        }
        public void ToggleMusic()
        {
            musicMuted=!musicMuted;PlayerPrefs.SetInt(PreferenceKey("music-muted"),musicMuted?1:0);PlayerPrefs.Save();
            if(musicSetting!=null)musicSetting.text=musicMuted?"Music off":"Music on";
        }
        private void ResetHome()
        {
            if(homeMusic!=null){homeMusic.Stop();Destroy(homeMusic);homeMusic=null;}
            foreach(var sprite in homeSprites)Destroy(sprite);
            foreach(var texture in homeTextures)Resources.UnloadAsset(texture);
            homeSprites.Clear();homeTextures.Clear();homeObjects.Clear();homeFronts.Clear();storageHints.Clear();shedPicture=null;shedFront=null;homeLayerLayout=null;
        }
        private void TickHome()
        {
            var home=Home;if(home==null || homeObjects.Count==0)return;
            var visible=CurrentArea=="garden";
            foreach(var pair in homeObjects)
            {var obj=pair.Value;obj.root.gameObject.SetActive(visible);obj.root.anchoredPosition=ToBoard(obj.x,obj.y);obj.root.localScale=Vector3.one*sceneScale;}
            foreach(var pair in homeFronts)
            {
                var obj=homeObjects[pair.Key];pair.Value.gameObject.SetActive(visible);
                pair.Value.anchoredPosition=obj.root.anchoredPosition;pair.Value.localScale=obj.root.localScale;
            }
            shedPicture.sprite=home.shedOpen?shedOpenSprite:shedClosedSprite;
            shedFront.gameObject.SetActive(home.shedOpen);
            // Keep rigid frames and their front/rear registration fixed. Bounce
            // motion belongs to the character; mat-only deformation is future art.
            var items=ReadToys();
            for(var i=0;i<storageHints.Count;i++)
                storageHints[i].gameObject.SetActive(home.shedOpen && dragging!=null && !items.Any(t=>t.container=="shed-"+i));
            foreach(var place in new[]{"living","garden"})
            {
                var notes=homeObjects["Radio "+place].root.Find("Music notes");
                notes.gameObject.SetActive(place=="living"?home.livingRadio:home.gardenRadio);
                ((RectTransform)notes).anchoredPosition=new Vector2(0,84+Mathf.Sin(Time.unscaledTime*3)*8);
            }
            foreach(var t in AllToys())toys[t.id].gameObject.SetActive(t.zone==CurrentArea && (string.IsNullOrEmpty(t.container) || home.shedOpen));
            var own=ReadPlayer(Actor);
            var audible=!applicationPaused && !musicMuted && (!Shared || shared.Connected) && HomeLayout.RadioNear(home,own);
            if(homeMusic!=null)
            {
                var distance=Mathf.Min(home.livingRadio?Mathf.Abs(own.x+3480):99999,home.gardenRadio?Mathf.Abs(own.x-1230):99999);
                homeMusic.volume=audible?Mathf.Lerp(.04f,.22f,1-distance/640):0;
                if(audible && !homeMusic.isPlaying)homeMusic.Play();else if(!audible && homeMusic.isPlaying)homeMusic.Stop();
            }
            homeFeedback.gameObject.SetActive(Time.unscaledTime<homeFeedbackUntil);
            SortDepth();
        }
        private string HomeDropTarget(Vector2 point)
        {
            if(Home==null || CurrentArea!="garden")return "";
            // Return even while closed/full: the authority rejects safely instead
            // of silently placing a toy behind an opaque door.
            for(var i=0;i<4;i++)if(Mathf.Abs(point.x-HomeLayout.StorageX(i))<85 && Mathf.Abs(point.y-HomeLayout.StorageY(i))<115)return "shed-"+i;
            return "";
        }
        private void SettleHomeUse()
        {
            if(Ready && !string.IsNullOrEmpty(ReadPlayer(Actor).fixture))Command(SoloAction.LeaveFixture);
            if(homeMusic!=null)homeMusic.Stop();
        }
    }
}
