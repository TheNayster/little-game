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
        private readonly Dictionary<string,RawImage> dinosaurPictures=new Dictionary<string,RawImage>();
        private readonly Dictionary<string,Texture2D> dinosaurTextures=new Dictionary<string,Texture2D>();
        private readonly Dictionary<string,Vector2> dinosaurSeats=new Dictionary<string,Vector2>();
        private readonly Dictionary<string,Vector2> dinosaurPrevious=new Dictionary<string,Vector2>();
        private readonly Dictionary<string,int> dinosaurCalls=new Dictionary<string,int>();
        private readonly Dictionary<string,AudioClip> dinosaurClips=new Dictionary<string,AudioClip>();
        private Texture2D dinosaurRiders;
        private readonly AudioSource[] dinosaurVoices=new AudioSource[2];
        private int dinosaurVoice;
        private GameObject dinosaurControls;
        private DinosaurWorldState Dinosaurs=>HasWorld?(Shared?shared.View.dinosaurWorld:World.ReadDinosaurWorld()):null;
        public DinosaurWorldState DinosaurGame=>Dinosaurs;
        public int DinosaurTextureCount=>dinosaurTextures.Count;
        public bool DinosaurSoundPlaying=>dinosaurVoices.Any(s=>s!=null && s.isPlaying);
        public Vector2 DinosaurSeat=>HasWorld && dinosaurSeats.TryGetValue(DinosaurRides.SpeciesOf(ReadPlayer(Actor).fixture),out var seat)?seat:Vector2.zero;
        private static float DinosaurSize(string id)=>id=="brachiosaurus"?570:550;
        private void BuildDinosaurWorld()
        {
            if(Dinosaurs==null)return;
            foreach(var id in DinosaurRides.Species){
                var root=Rect(Board,"Dinosaur mount "+id,Vector2.zero,Vector2.zero);
                var image=Rect(root,"Animated rideable "+id,Vector2.zero,Vector2.one*DinosaurSize(id)).gameObject.AddComponent<RawImage>();
                image.raycastTarget=false;image.rectTransform.pivot=new Vector2(.5f,0);dinosaurPictures.Add(id,image);
                // Like the existing park rides, one picture tap gets on directly.
                // Only the explicit feet button gets off; repeated taps are safe.
                HomeHit(root,"Ride "+id,new Vector2(0,200),new Vector2(420,400),()=>DinosaurAction("mount",id));
            }
            dinosaurControls=Rect(safe,"Dinosaur riding controls",Vector2.zero,new Vector2(420,160)).gameObject;
            var off=Panel(dinosaurControls.transform,"Dinosaur get off",new Vector2(-105,0),Vector2.one*140,Cream,true,true);
            NavButton(off,()=>DinosaurAction("off",""));
            // Two footprints and a down arrow make the action readable by sight.
            for(var i=0;i<2;i++){
                var foot=Panel(off.transform,"Footprint "+i,new Vector2(-24+i*35,10-i*17),new Vector2(19,40),new Color(.35f,.55f,.48f),false,true);
                foot.rectTransform.localRotation=Quaternion.Euler(0,0,i==0?-15:15);
                Panel(off.transform,"Foot toe "+i,new Vector2(-24+i*35,35-i*17),new Vector2(16,13),new Color(.35f,.55f,.48f),false,true);
            }
            var arrow=Plain(off.transform,"Get down arrow",new Vector2(0,-41),new Vector2(12,28),Ink);
            for(var i=0;i<2;i++){var tip=Plain(off.transform,"Arrow tip "+i,new Vector2(i==0?-8:8,-49),new Vector2(12,22),Ink);tip.rectTransform.localRotation=Quaternion.Euler(0,0,i==0?45:-45);}
            var call=Panel(dinosaurControls.transform,"Dinosaur call",new Vector2(105,0),Vector2.one*140,new Color(.96f,.85f,.43f),true,true);
            NavButton(call,()=>DinosaurAction("call",DinosaurRides.SpeciesOf(ReadPlayer(Actor).fixture)));
            Panel(call.transform,"Voice bubble",new Vector2(-8,6),new Vector2(65,47),Cream,false,true);
            for(var i=0;i<3;i++)Panel(call.transform,"Voice mark "+i,new Vector2(30+i*10,-17+i*15),new Vector2(6,18),Ink,false,true);
            BuildDinosaurCare();TickDinosaurWorld();
        }
        private void DinosaurAction(string value,string id)
        {
            if(ActionPending)return;
            var p=ReadPlayer(Actor);if(value=="mount" && p.fixture==DinosaurRides.Fixture(id))return;
            CancelPointers();destination=null;manualCamera=false;shared?.Walk(WalkMode.Stop);
            void Finish(SoloResult r){if(!r.Accepted){message.text=r.Outcome=="dinosaur-busy" || r.Outcome=="dinosaur-taking-care"?"This dinosaur is busy. Choose another one.":r.Outcome=="put-down-toy"?"Put down your toy first.":r.Outcome=="dinosaur-speaking"?"Listen to the dinosaur.":r.Outcome;}Render();}
            if(Shared)SubmitShared(SoloAction.Dinosaur,"",id,value,0,0,Finish);else Finish(Command(SoloAction.Dinosaur,target:id,value:value));
        }
        private void TickDinosaurWorld()
        {
            if(dinosaurControls==null || Dinosaurs==null)return;
            if(applicationPaused)ResetDinosaurSound();
            var visible=CurrentArea==DinosaurRides.Area;
            var players=Shared?shared.View.players:World.ReadPlayers();
            dinosaurControls.SetActive(visible && DinosaurRides.Usable(ReadPlayer(Actor).fixture) && !MenuOpen);
            dinosaurControls.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-safe.rect.height/2+92);
            foreach(var a in Dinosaurs.animals){
                var image=dinosaurPictures[a.species];image.transform.parent.gameObject.SetActive(visible);
                if(!visible){dinosaurCalls[a.species]=a.calls;continue;}
                if(!dinosaurTextures.TryGetValue(a.species,out var texture)){texture=WorldResources.Load<Texture2D>("Worlds/Dinosaur/Art/"+a.species);dinosaurTextures.Add(a.species,texture);image.texture=texture;}
                var rider=players.FirstOrDefault(p=>p.fixture==DinosaurRides.Fixture(a.species));
                var point=rider==null?new Vector2(a.x,a.y):Shared?shared.VisualPosition(rider.id):new Vector2(rider.x,rider.y);
                var moving=a.wandering;
                if(dinosaurPrevious.TryGetValue(a.species,out var previous))moving|=Vector2.Distance(previous,point)>.02f;
                dinosaurPrevious[a.species]=point;
                var speaking=Dinosaurs.clock-a.lastCall<1.2;
                var care=Dinosaurs.care?.FirstOrDefault(f=>f.species==a.species && DinosaurCareRules.Active(f));
                var frame=care?.phase==DinosaurCarePhase.Pet?7:care?.phase==DinosaurCarePhase.Eat?4:speaking?6:moving?(int)(Time.unscaledTime*5)%4:4;
                var landmark=DinosaurLandmarks.Get(a.species,frame);var size=DinosaurSize(a.species);
                var root=(RectTransform)image.transform.parent;root.anchoredPosition=ToBoard(point.x,point.y);root.localScale=Vector3.one*sceneScale;
                // Register the actual foot and cushion in every pose. Generated
                // frame margins vary; a single guessed lift would visibly slip.
                image.uvRect=new Rect(frame%4*.25f,frame<4?.5f:0,.25f,.5f);
                image.rectTransform.anchoredPosition=new Vector2(0,-size*(1-landmark.z));
                image.rectTransform.localScale=new Vector3(a.left?-1:1,1,1);
                dinosaurSeats[a.species]=new Vector2((landmark.x-.5f)*size*(a.left?-1:1),(landmark.z-landmark.y)*size);
                if(!dinosaurCalls.TryGetValue(a.species,out var heard)){dinosaurCalls[a.species]=a.calls;}
                else if(heard!=a.calls){dinosaurCalls[a.species]=a.calls;if(Dinosaurs.clock-a.lastCall<1.5)PlayDinosaurCall(a.species,point.x);}
            }
            TickDinosaurCare();
            if(!visible){
                foreach(var image in dinosaurPictures.Values)image.texture=null;
                foreach(var texture in dinosaurTextures.Values)if(texture!=null)Resources.UnloadAsset(texture);dinosaurTextures.Clear();dinosaurPrevious.Clear();
                ResetDinosaurSound();if(dinosaurRiders!=null)Resources.UnloadAsset(dinosaurRiders);dinosaurRiders=null;
            }
        }
        private void PresentDinosaurRiders()
        {
            if(CurrentArea!=DinosaurRides.Area || Dinosaurs==null)return;
            void Ride(string id,RectTransform body,GameCharacterVisual visual){
                var p=ReadPlayer(id);var species=DinosaurRides.SpeciesOf(p.fixture);if(species=="" || !dinosaurSeats.ContainsKey(species))return;
                var a=Dinosaurs.animals.Single(v=>v.species==species);var root=(RectTransform)dinosaurPictures[species].transform.parent;
                body.anchoredPosition=root.anchoredPosition+dinosaurSeats[species]*sceneScale;
                visual.PresentSupported(new CharacterFrame(CharacterPose.Sit,0,a.left,1),Time.unscaledDeltaTime);
                // The special rider sheet depicts ordinary clothes. Keep the
                // equipped outfit sheet and its color when sitting on a dinosaur.
                if(string.IsNullOrEmpty(p.outfit) && (p.avatar=="blue-pup" || p.avatar=="orange-pup")){
                    if(dinosaurRiders==null)dinosaurRiders=WorldResources.Load<Texture2D>("Worlds/Dinosaur/Art/rider-poses");
                    var row=p.avatar=="orange-pup"?1:0;var cw=dinosaurRiders.width/4f;var ch=dinosaurRiders.height/2f;
                    var cell=new Rect(0,row*ch,cw,ch);var contact=cell.position+new Vector2(190*cw/446,328*ch/446);
                    visual.ActiveView.PresentRiding(dinosaurRiders,cell,contact,ch*410/446,a.left);
                }
            }
            Ride(Actor,avatar,characterVisual);
            foreach(var friend in friends)if(friend.Value.root.gameObject.activeSelf)Ride(friend.Key,friend.Value.root,friend.Value.view);
            SortDepth();
        }
        private float DinosaurPlayerGround(string fixture)=>((RectTransform)dinosaurPictures[DinosaurRides.SpeciesOf(fixture)].transform.parent).anchoredPosition.y;
        private void AddDinosaurDepth(Action<RectTransform,float,int,string> add)
        {foreach(var pair in dinosaurPictures){var root=(RectTransform)pair.Value.transform.parent;add(root,root.anchoredPosition.y,0,"dinosaur-"+pair.Key);}}
        private void PlayDinosaurCall(string species,float x)
        {
            if(applicationPaused)return;
            if(!dinosaurClips.TryGetValue(species,out var clip)){clip=WorldResources.Load<AudioClip>("Worlds/Dinosaur/Audio/"+species);dinosaurClips.Add(species,clip);}
            var slot=dinosaurVoice++%2;var source=dinosaurVoices[slot];
            if(source==null){source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=false;source.spatialBlend=0;dinosaurVoices[slot]=source;}
            source.Stop();source.clip=clip;source.volume=.24f*ForegroundDucking;source.panStereo=Mathf.Clamp((x-cameraX)/1500,-.55f,.55f);source.Play();
        }
        private void ResetDinosaurSound()
        {
            foreach(var voice in dinosaurVoices)if(voice!=null){voice.Stop();voice.clip=null;}
            foreach(var clip in dinosaurClips.Values)if(clip!=null)Resources.UnloadAsset(clip);dinosaurClips.Clear();
        }
        private void ResetDinosaurWorld()
        {
            ResetDinosaurCare();ResetDinosaurSound();foreach(var voice in dinosaurVoices)if(voice!=null)Destroy(voice);Array.Clear(dinosaurVoices,0,2);
            foreach(var texture in dinosaurTextures.Values)if(texture!=null)Resources.UnloadAsset(texture);dinosaurTextures.Clear();
            if(dinosaurRiders!=null)Resources.UnloadAsset(dinosaurRiders);dinosaurRiders=null;
            dinosaurPictures.Clear();dinosaurSeats.Clear();dinosaurPrevious.Clear();dinosaurCalls.Clear();dinosaurControls=null;
        }
    }
}
