using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Visual-only adapter. The roster maps stable saved IDs to prepared art.
    // The existing toy/pointer system continues to draw and own actual props.
    public sealed class GameCharacterVisual : MonoBehaviour
    {
        private readonly CharacterMotion motion = new CharacterMotion();
        private CharacterSheetView view;
        private string selectedAvatar,leasedArt;
        private sealed class ArtLease {public CharacterArt art;public int users;}
        private static readonly Dictionary<string,ArtLease> liveArt=new Dictionary<string,ArtLease>();
        public static int LoadedArtCount=>liveArt.Count;
        public static long LoadedArtTextureBytes {get {long n=0;foreach(var lease in liveArt.Values){n+=(long)lease.art.sheet.width*lease.art.sheet.height*4;n+=(long)lease.art.walkSheet.width*lease.art.walkSheet.height*4;}return n;}}
        private static CharacterArt AcquireArt(string id)
        {
            if(!liveArt.TryGetValue(id,out var lease)){
                var art=Resources.Load<CharacterArt>("CharacterArt/"+id);
                if(art==null)throw new InvalidOperationException("Missing built character artwork: "+id);
                liveArt[id]=lease=new ArtLease{art=art};
            }
            lease.users++;return lease.art;
        }
        private void ReleaseArt()
        {
            var old=view;view=null;
            if(old!=null){old.ClearArtwork();old.gameObject.SetActive(false);Destroy(old.gameObject);}
            if(leasedArt==null)return;
            var id=leasedArt;leasedArt=null;var lease=liveArt[id];
            if(--lease.users!=0)return;
            liveArt.Remove(id);
            // All live views of this character have released their references.
            Resources.UnloadAsset(lease.art.sheet);Resources.UnloadAsset(lease.art.walkSheet);Resources.UnloadAsset(lease.art);
        }
        private string useKey="";private float useAge;
        private int lastRoar=-1;
        private float roarUntil;
        private AudioSource roarAudio;
        public bool RoarPlaying=>roarAudio!=null && roarAudio.isPlaying;
        public string Outfit=>view==null?"":view.Outfit;
        public string CharacterId => view == null ? "" : view.CharacterId;
        public CharacterFrame Frame => view == null ? default : view.Frame;
        public int LayerCount => view == null ? 0 : view.GetComponentsInChildren<Graphic>(true).Length;
        public CharacterSheetView ActiveView => view;
        public void Wear(string id,string color){view?.Wear(id,color);if(id!="dinosaur")SilenceRoar();}
        public void ObserveRoar(int sequence,bool sounds)
        {
            if(lastRoar<0){lastRoar=sequence;return;}
            if(lastRoar==sequence)return;
            lastRoar=sequence;
            if(Core.CharacterOutfits.Find(Outfit)?.CanRoar!=true)return;
            roarUntil=Time.unscaledTime+1.1f;
            if(!sounds)return;
            if(roarAudio==null){roarAudio=gameObject.AddComponent<AudioSource>();roarAudio.playOnAwake=false;roarAudio.spatialBlend=0;roarAudio.volume=.55f;}
            roarAudio.clip=Resources.Load<AudioClip>("Books/"+Core.HomeBooks.Title+"/audio/effect-0");
            if(roarAudio.clip!=null){roarAudio.Stop();roarAudio.Play();}
        }
        public void SilenceRoar(){if(roarAudio!=null)roarAudio.Stop();roarUntil=0;}
        private void OnEnable(){if(selectedAvatar!=null)Select(selectedAvatar);}
        private void OnDisable(){SilenceRoar();lastRoar=-1;ReleaseArt();}
        private void OnDestroy(){ReleaseArt();}
        private void OnApplicationPause(bool paused){if(paused)SilenceRoar();}
        private void OnApplicationFocus(bool focused){if(!focused)SilenceRoar();}

        public void Select(string savedAvatar)
        {
            var entry = Core.PlayableCharacters.Find(savedAvatar);
            if (entry == null) throw new InvalidOperationException("Unknown character: " + savedAvatar);
            selectedAvatar=savedAvatar;
            if(!gameObject.activeInHierarchy)return;
            var id = entry.ArtId;
            if(view!=null && CharacterId==id)return;
            var prior=Frame;
            ReleaseArt();
            var art=AcquireArt(id);leasedArt=id;
            CharacterSheetView old=null;
            var visual = Joint(art.displayName + " selected sheet", transform, new Vector2(0, -45));
            view = visual.gameObject.AddComponent<CharacterSheetView>();
            view.Configure(art, old);
            if (old != null) { old.gameObject.SetActive(false); Destroy(old.gameObject); }
            view.Present(prior, 0);
        }

        public void PresentFrame(CharacterFrame frame, float dt)
        {
            if (view != null) view.Present(frame, dt);
        }

        public void PresentSupported(CharacterFrame frame,float dt,float angle=0)
        {PresentFrame(frame,dt);view?.AttachToSupport(angle);}

        public void PresentBeachRide(CharacterFrame frame,float dt,float angle)
        {if(view!=null)((RectTransform)view.transform).anchoredPosition=new Vector2(0,-45);PresentSupported(frame,dt,angle);}

        public void Present(Vector2 displayedPosition, string continuity, bool held, float dt)
        {
            if (view != null) PresentFrame(motion.Observe(displayedPosition, continuity, held, false, dt), dt);
        }

        public void PresentHome(Vector2 point,string continuity,bool held,float dt,Core.SoloPlayer player,Core.HomeState home,Core.KeepyState balloon=null)
        {
            if(view==null)return;
            // The new shoreline/prints use the authoritative floor directly.
            // Remove the legacy lab art offset only on this illustrated beach.
            ((RectTransform)view.transform).anchoredPosition=new Vector2(0,player.zone=="beach"?0:-45);
            Wear(player.outfit,player.outfitColor);
            var frame=motion.Observe(point,continuity,held,false,dt);
            if(Core.BathroomLayout.Usable(player.fixture))
                frame=new CharacterFrame(CharacterPose.Idle,0,false,(float)player.useSeconds);
            else if(Core.BedroomFurniture.Seat(player.fixture))
                frame=new CharacterFrame(player.fixture==Core.BedroomFurniture.Bed?CharacterPose.Rest:CharacterPose.Sit,0,false,(float)player.useSeconds);
            else if(!held && Core.HomeLayout.Usable(player.fixture))
            {
                var key=continuity+"/"+player.fixture;
                if(useKey!=key){useKey=key;useAge=(float)player.useSeconds;}
                else useAge=Mathf.Max(useAge+Mathf.Clamp(dt,0,.1f),(float)player.useSeconds);
                frame=new CharacterFrame(Core.HomeLayout.Seat(player.fixture)?CharacterPose.Sit:CharacterPose.Bounce,0,frame.FaceLeft,useAge);
            }
            else if(frame.Pose==CharacterPose.Idle && player.activity=="" && Core.HomeLayout.RadioNear(home,player))
                frame=new CharacterFrame(CharacterPose.Dance,0,frame.FaceLeft);
            if(!held && string.IsNullOrEmpty(player.fixture) && player.zone=="garden" && balloon!=null && balloon.phase==1)
            {
                var struck=balloon.lastHitter==player.id && balloon.hitAge<.38;
                var reaching=Core.KeepyRules.Under(player,balloon) && balloon.vz<0 &&
                    balloon.height<Core.KeepyRules.HandHeight(player.avatar)+Core.KeepyRules.Radius+55;
                if(struck || reaching)frame=new CharacterFrame(CharacterPose.BalloonTap,frame.Speed,
                    struck?balloon.hitLeft:balloon.x<player.x,struck?(float)balloon.hitAge:0,frame.Travel,frame.ResetMotion);
            }
            if(player.stairs>0)frame=new CharacterFrame(held?CharacterPose.Carry:CharacterPose.Walk,Core.Walking.Speed,true,travel:frame.Travel,resetMotion:frame.ResetMotion);
            if(!Core.HomeLayout.Usable(player.fixture))useKey="";
            if(Time.unscaledTime<roarUntil && frame.Pose==CharacterPose.Idle && player.activity=="" && string.IsNullOrEmpty(player.fixture))
                frame=new CharacterFrame(CharacterPose.Roar,0,frame.FaceLeft);
            PresentFrame(frame,dt);
        }

        private static RectTransform Joint(string name, Transform parent, Vector2 position)
        {
            var result = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            result.SetParent(parent, false); result.anchoredPosition = position; result.sizeDelta = Vector2.zero;
            return result;
        }
    }
}
