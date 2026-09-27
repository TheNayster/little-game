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
        public SecretRoomState[] Secrets=>HasWorld?(Shared?shared.View.secrets:World.ReadSecrets()):Array.Empty<SecretRoomState>();
        private SecretRoomState LocalSecret=>Secrets.FirstOrDefault(r=>r.id==CurrentArea || r.parent==CurrentArea);
        private readonly Dictionary<string,Sprite> secretSprites=new Dictionary<string,Sprite>();
        private RectTransform secretDoor,secretExit,secretSkyRoot,secretFort,secretFortFront,secretSettings,quietPanel,quietButton,secretManage,secretReturn;
        private CanvasGroup secretDoorGroup;
        private Image secretDoorPicture;
        private SecretSky doorMagic,roomMagic;
        private Text quietMotion,quietLight,quietSound,quietEffects,secretActive;
        private bool secretRevealed,quietStill;
        private int quietBrightness,quietMusicLevel=1,quietEffectsLevel=1;
        private float secretAlpha,secretTime,starBurst;
        private string secretViewArea;
        private AudioSource secretAudio,secretChime;
        private AudioClip secretAmbienceClip,secretChimeClip;
        public bool SecretDoorVisible=>secretDoor!=null && secretDoor.gameObject.activeInHierarchy;
        public bool SecretDoorInteractive=>SecretDoorVisible && secretDoorGroup.blocksRaycasts;
        public bool QuietStill=>quietStill;
        public int QuietBrightness=>quietBrightness;
        public int QuietMusicLevel=>quietMusicLevel;
        public int QuietEffectsLevel=>quietEffectsLevel;
        public float QuietPhase=>secretTime;
        private string quietScope;
        private string QuietKey(string name)
        {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            return "solo.prototype.quiet."+Actor+"."+name;
#else
            // A paired profile keeps the same local preferences when switching
            // between its server and private adventure. Isolated native runs
            // retain their own test namespace, never the family's preferences.
            if(quietScope==null)quietScope=offlineBranch??VerifyRun??shared?.PreferenceScope??"solo.prototype";
            return quietScope+".quiet."+Actor+"."+name;
#endif
        }
        private Sprite SecretSprite(string id)
        {
            if(secretSprites.TryGetValue(id,out var prior))return prior;
            var part=id.StartsWith("plush-")?int.Parse(id.Substring(6)):-1;
            var resource=part>=0?"plush-sheet":id;var sourceKey="texture-"+resource;
            Texture2D texture;
            if(secretSprites.TryGetValue(sourceKey,out var existing))texture=existing.texture;
            else
            {
                texture=Resources.Load<Texture2D>("SecretArt/"+resource);if(texture==null)throw new InvalidOperationException("Missing secret art: "+resource);
                homeTextures.Add(texture);var whole=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));homeSprites.Add(whole);secretSprites.Add(sourceKey,whole);
            }
            var rect=part<0?new Rect(0,0,texture.width,texture.height):new Rect((part%3)*texture.width/3f,(1-part/3)*texture.height/2f,texture.width/3f,texture.height/2f);
            var sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f));homeSprites.Add(sprite);secretSprites.Add(id,sprite);return sprite;
        }
        private void BuildSecrets()
        {
            if(SceneSchema<SecretRooms.Schema)return;
            quietStill=PlayerPrefs.GetInt(QuietKey("still"),0)!=0;quietBrightness=Mathf.Clamp(PlayerPrefs.GetInt(QuietKey("brightness"),0),0,2);
            quietMusicLevel=Mathf.Clamp(PlayerPrefs.GetInt(QuietKey("music"),1),0,2);quietEffectsLevel=Mathf.Clamp(PlayerPrefs.GetInt(QuietKey("effects"),1),0,2);
            secretDoor=Rect(Board,"Magical secret entrance",Vector2.zero,Vector2.zero);secretDoorGroup=secretDoor.gameObject.AddComponent<CanvasGroup>();
            secretDoorPicture=HomePicture(secretDoor,"Little star door",new Vector2(0,110),new Vector2(155,233),SecretSprite("door"));
            doorMagic=Rect(secretDoor,"Door sparkles",new Vector2(0,125),new Vector2(220,290)).gameObject.AddComponent<SecretSky>();doorMagic.door=true;doorMagic.raycastTarget=false;
            HomeHit(secretDoor,"Secret star door",new Vector2(0,110),new Vector2(160,220),RequestSecret);
            secretExit=Rect(Board,"Secret room exit",Vector2.zero,Vector2.zero);
            HomeHit(secretExit,"Leave secret room",new Vector2(0,170),new Vector2(195,350),()=>RequestBedroom(SecretRooms.Parent(CurrentArea)));
            var exitLabel=Label(secretExit,"Bedroom ←",22,new Vector2(0,-25),new Vector2(190,42));exitLabel.color=Cream;
            secretSkyRoot=Rect(Board,"Secret aurora sky",Vector2.zero,new Vector2(2350,300));
            roomMagic=secretSkyRoot.gameObject.AddComponent<SecretSky>();roomMagic.raycastTarget=false;
            for(var i=0;i<3;i++)
            {var n=i;var hit=Plain(secretSkyRoot,"Twinkle star "+(i+1),new Vector2(-620+i*540,0),new Vector2(100,90),Color.clear,true);hit.canvasRenderer.cullTransparentMesh=false;NavButton(hit,()=>Twinkle(n));}
            secretFort=Rect(Board,"Secret blanket fort",Vector2.zero,Vector2.zero);var fort=SecretSprite("fort");
            HomePicture(secretFort,"Blanket fort",new Vector2(0,180),new Vector2(880,440),fort);
            for(var i=0;i<4;i++){var n=i;HomeHit(secretFort,"Sit in fort "+(i+1),new Vector2(-240+i*160,90),new Vector2(145,170),()=>UseHome(SecretRooms.Fort(n)));}
            secretFortFront=Rect(Board,"Fort curtain foreground",Vector2.zero,Vector2.zero);
            var cover=Rect(secretFortFront,"Curtains and hem",new Vector2(0,180),new Vector2(880,440)).gameObject.AddComponent<HomeArtPart>();
            cover.Configure(fort.texture,new[]{new HomeArtPart.Polygon{points=Patch(0,0,1,.29f)},
                new HomeArtPart.Polygon{points=new[]{new Vector2(0,.25f),new Vector2(.26f,.25f),new Vector2(.16f,.95f),new Vector2(0,1)}},
                new HomeArtPart.Polygon{points=new[]{new Vector2(.74f,.25f),new Vector2(1,.25f),new Vector2(1,1),new Vector2(.84f,.95f)}},
                new HomeArtPart.Polygon{points=Patch(0,.9f,1,1)}});
            quietButton=(RectTransform)Button(safe,"Quiet room",new Vector2(-160,315),new Vector2(180,60),()=>{CancelPointers();quietPanel.gameObject.SetActive(!quietPanel.gameObject.activeSelf);},new Color(.85f,.87f,1)).transform.parent;
            quietPanel=Panel(safe,"Quiet preferences",new Vector2(0,225),new Vector2(850,85),new Color(.9f,.93f,1,.98f)).rectTransform;
            quietMotion=Button(quietPanel,"Sky motion",new Vector2(-315,0),new Vector2(195,60),()=>QuietSetting("still"),Cream);
            quietLight=Button(quietPanel,"Sky brightness",new Vector2(-105,0),new Vector2(195,60),()=>QuietSetting("brightness"),Cream);
            quietSound=Button(quietPanel,"Quiet music",new Vector2(105,0),new Vector2(195,60),()=>QuietSetting("music"),Cream);
            quietEffects=Button(quietPanel,"Quiet effects",new Vector2(315,0),new Vector2(195,60),()=>QuietSetting("effects"),Cream);quietPanel.gameObject.SetActive(false);QuietLabels();
            secretManage=(RectTransform)Button(safe,"Secret door",new Vector2(60,315),new Vector2(180,60),()=>{CancelPointers();secretSettings.gameObject.SetActive(!secretSettings.gameObject.activeSelf);},Cream).transform.parent;
            secretSettings=Panel(safe,"Secret entrance settings",new Vector2(0,125),new Vector2(660,85),Cream).rectTransform;
            secretActive=Button(secretSettings,"Entrance visibility",new Vector2(-220,0),new Vector2(200,60),()=>EditSecret("active",LocalSecret.active?0:1),new Color(.85f,.93f,1));
            Button(secretSettings,"Move door",Vector2.zero,new Vector2(200,60),()=>EditSecret("slot",1-LocalSecret.slot),new Color(.85f,.93f,1));
            Button(secretSettings,"Door colour",new Vector2(220,0),new Vector2(200,60),()=>EditSecret("style",1-LocalSecret.style),new Color(.85f,.93f,1));secretSettings.gameObject.SetActive(false);
            secretReturn=(RectTransform)Button(safe,"Bedroom ←",new Vector2(365,315),new Vector2(190,60),()=>{CancelPointers();CancelDoorApproach();Command(SoloAction.ReturnBedroom);},Cream).transform.parent;
            secretAudio=gameObject.AddComponent<AudioSource>();secretAudio.playOnAwake=false;secretAudio.loop=true;secretAudio.spatialBlend=0;secretAudio.volume=0;
            secretChime=gameObject.AddComponent<AudioSource>();secretChime.playOnAwake=false;secretChime.spatialBlend=0;
            secretAmbienceClip=QuietClip(false);secretChimeClip=QuietClip(true);secretAudio.clip=secretAmbienceClip;
            PresentSecrets();
        }
        private static AudioClip QuietClip(bool chime)
        {
            const int rate=22050;var length=chime?2:12;var samples=new float[rate*length];
            for(var i=0;i<samples.Length;i++)
            {var t=i/(float)rate;var envelope=chime?Mathf.Min(t*20,1)*Mathf.Exp(-t*3):.7f+.3f*Mathf.Cos(t*2*Mathf.PI/length);samples[i]=envelope*(Mathf.Sin(t*2*Mathf.PI*(chime?660:110))*.16f+Mathf.Sin(t*2*Mathf.PI*(chime?880:165))*.08f);}
            var clip=AudioClip.Create(chime?"Gentle star chime":"Quiet room pad",samples.Length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        private static string QuietLevel(int value)=>value==0?"off":value==1?"soft":"on";
        private void QuietLabels(){quietMotion.text=quietStill?"Sky: still":"Sky: moving";quietLight.text=quietBrightness==0?"Light: bright":quietBrightness==1?"Light: soft":"Light: dim";quietSound.text="Music: "+QuietLevel(quietMusicLevel);quietEffects.text="Chimes: "+QuietLevel(quietEffectsLevel);}
        private void QuietSetting(string name)
        {
            if(name=="still")quietStill=!quietStill;else if(name=="music")quietMusicLevel=(quietMusicLevel+1)%3;else if(name=="effects")quietEffectsLevel=(quietEffectsLevel+1)%3;else quietBrightness=(quietBrightness+1)%3;
            PlayerPrefs.SetInt(QuietKey(name),name=="still"?(quietStill?1:0):name=="music"?quietMusicLevel:name=="effects"?quietEffectsLevel:quietBrightness);PlayerPrefs.Save();QuietLabels();
        }
        private void Twinkle(int index){starBurst=1;if(quietEffectsLevel>0 && !applicationPaused)secretChime.PlayOneShot(secretChimeClip,quietEffectsLevel==1?.12f:.25f);}
        private void RequestSecret()
        {
            var room=LocalSecret;if(room==null || !secretRevealed || secretAlpha<.8f || !Ready || MenuOpen || WorldLoading || TravelPending)return;
            if(room.created){if(room.active)RequestBedroom(room.id);return;}
            if(room.owner!=Actor)return;
            void Done(SoloResult result){if(result.Accepted){Render();RequestBedroom(room.id);}else{homeFeedback.text="Try your little door again.";homeFeedbackUntil=Time.unscaledTime+3;}}
            if(Shared)SubmitShared(SoloAction.SecretRoom,"","create","0:"+room.entranceRevision,0,0,Done);else Done(Command(SoloAction.SecretRoom,target:"create",value:"0:"+room.entranceRevision));
        }
        private void EditSecret(string kind,int value){var room=LocalSecret;if(room!=null)HomeAction(SoloAction.SecretRoom,kind,value+":"+room.entranceRevision);}
        private void EnsureToyViews(){foreach(var t in AllToys())if(!toys.ContainsKey(t.id))DrawToy(t);}
        private void PresentSecrets()
        {
            if(secretDoor==null || !Ready)return;var room=LocalSecret;var inside=SecretRooms.Index(CurrentArea)>=0;var visible=!WorldLoading;
            if(secretViewArea!=CurrentArea){secretViewArea=CurrentArea;secretRevealed=false;secretAlpha=0;quietPanel.gameObject.SetActive(false);secretSettings.gameObject.SetActive(false);}
            if(!applicationPaused && !quietStill)secretTime+=Time.unscaledDeltaTime;
            starBurst=Mathf.Max(0,starBurst-Time.unscaledDeltaTime*.8f);
            var p=ReadPlayer(Actor);var available=room!=null && !inside && (room.created?room.active:room.owner==Actor);
            secretRevealed=available && SecretRooms.Reveal(secretRevealed,p.x,p.y,room.slot);
            secretAlpha=Mathf.MoveTowards(secretAlpha,secretRevealed?1:0,Time.unscaledDeltaTime*2);
            secretDoor.gameObject.SetActive(available && visible && secretAlpha>0);secretDoorGroup.alpha=secretAlpha;
            secretDoorGroup.blocksRaycasts=secretDoorGroup.interactable=secretRevealed && secretAlpha>=.8f && !MenuOpen;
            if(room!=null){secretDoor.anchoredPosition=ToBoard(SecretRooms.DoorX(room.slot),350);secretDoor.localScale=Vector3.one*sceneScale;secretDoorPicture.color=room.style==0?Color.white:new Color(.78f,1,.87f);}
            doorMagic.Present(quietStill?0:secretTime,0);
            secretExit.gameObject.SetActive(inside && visible);secretExit.anchoredPosition=ToBoard(BedroomLayout.ExitX,BedroomLayout.DoorY);secretExit.localScale=Vector3.one*sceneScale;
            secretSkyRoot.gameObject.SetActive(inside && visible);secretSkyRoot.anchoredPosition=ToBoard(1200,1000)+new Vector2(0,40)*sceneScale;secretSkyRoot.localScale=Vector3.one*sceneScale;roomMagic.Present(quietStill?0:secretTime,quietStill?0:starBurst);
            secretFort.gameObject.SetActive(inside && visible);secretFortFront.gameObject.SetActive(inside && visible);
            if(inside && room!=null){secretFort.anchoredPosition=ToBoard(BedroomFurniture.BedX(room.furniture.layout),350);secretFort.localScale=Vector3.one*sceneScale;secretFortFront.anchoredPosition=secretFort.anchoredPosition;secretFortFront.localScale=secretFort.localScale;}
            quietButton.gameObject.SetActive(room!=null && visible && !MenuOpen);secretReturn.gameObject.SetActive(inside && visible && !MenuOpen);
            secretManage.gameObject.SetActive(room!=null && room.created && room.owner==Actor && visible && !MenuOpen);
            if(room==null || MenuOpen || !visible){quietPanel.gameObject.SetActive(false);secretSettings.gameObject.SetActive(false);}
            if(room!=null)secretActive.text=room.active?"Hide entrance":"Show entrance";
            if(scenicImages.TryGetValue("home-secret",out var background)){var light=quietBrightness==0?1:quietBrightness==1?.84f:.68f;background.color=new Color(light,light,light,1);}
            var audible=inside && visible && !applicationPaused && quietMusicLevel>0 && !musicMuted;
            secretAudio.volume=Mathf.MoveTowards(secretAudio.volume,audible?(quietMusicLevel==1?.08f:.17f):0,Time.unscaledDeltaTime*.3f);
            if(audible && !secretAudio.isPlaying)secretAudio.Play();else if(!audible && secretAudio.volume==0)secretAudio.Stop();
            if(!inside || applicationPaused || quietEffectsLevel==0)secretChime.Stop();if(applicationPaused){secretAudio.Stop();secretAudio.volume=0;}SortDepth();
        }
        private void AddSecretDepth(Action<RectTransform,float,int,string> add)
        {if(secretDoor==null)return;add(secretSkyRoot,99998,0,"sky");add(secretDoor,ToBoard(0,350).y,0,"secret-door");add(secretExit,99997,0,"secret-exit");add(secretFort,secretFort.anchoredPosition.y,0,"fort");add(secretFortFront,secretFortFront.anchoredPosition.y,2,"fort-front");}
        private void ResetSecrets()
        {
            if(secretAudio!=null){secretAudio.Stop();Destroy(secretAudio);}if(secretChime!=null){secretChime.Stop();Destroy(secretChime);}
            if(secretAmbienceClip!=null)Destroy(secretAmbienceClip);if(secretChimeClip!=null)Destroy(secretChimeClip);
            secretSprites.Clear();secretDoor=null;secretViewArea=null;secretAlpha=0;secretRevealed=false;
        }
    }
}
