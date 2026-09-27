using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Visual-only adapter. Legacy avatar IDs remain the save/network contract.
    // The existing toy/pointer system continues to draw and own actual props.
    public sealed class GameCharacterVisual : MonoBehaviour
    {
        private readonly CharacterMotion motion = new CharacterMotion();
        private CharacterSheetView view;
        private string useKey="";private float useAge;
        public string CharacterId => view == null ? "" : view.CharacterId;
        public CharacterFrame Frame => view == null ? default : view.Frame;
        public int LayerCount => view == null ? 0 : view.GetComponentsInChildren<Graphic>(true).Length;
        public CharacterSheetView ActiveView => view;

        public void Select(string savedAvatar)
        {
            var id = savedAvatar == "orange-pup" ? "bingo" : "bluey";
            if (CharacterId == id) return;
            var art = Resources.Load<CharacterArt>("CharacterArt/" + id);
            if (art == null) throw new InvalidOperationException("Missing built character artwork: " + id);
            var prior = Frame;
            var old = view;
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

        public void Present(Vector2 displayedPosition, string continuity, bool held, float dt)
        {
            if (view != null) PresentFrame(motion.Observe(displayedPosition, continuity, held, false, dt), dt);
        }

        public void PresentHome(Vector2 point,string continuity,bool held,float dt,Core.SoloPlayer player,Core.HomeState home,Core.KeepyState balloon=null)
        {
            if(view==null)return;
            var frame=motion.Observe(point,continuity,held,false,dt);
            if(Core.BedroomFurniture.Seat(player.fixture))
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
