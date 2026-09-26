using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Visual-only adapter. Legacy avatar IDs remain the save/network contract.
    // The existing toy/pointer system continues to draw and own actual props.
    public sealed class GameCharacterVisual : MonoBehaviour
    {
        private readonly CharacterMotion motion = new CharacterMotion();
        private CharacterView view;
        private CharacterView frontView,profileView;
        private bool sideWalking;
        private string useKey="";private float useAge;
        public string CharacterId => view == null ? "" : view.characterId;
        public CharacterFrame Frame => view == null ? default : view.Frame;
        public int LayerCount => view == null ? 0 : view.GetComponentsInChildren<Image>(true).Length;
        public CharacterView ActiveView=>view;
        public bool ProfileVisible=>view!=null && view.profileArtwork;

        public void Select(string savedAvatar)
        {
            var id = savedAvatar == "orange-pup" ? "bingo" : "bluey";
            if (CharacterId == id) return;
            var art = Resources.Load<CharacterArt>("CharacterArt/" + id);
            if (art == null) throw new InvalidOperationException("Missing built character artwork: " + id);
            var prior = Frame;
            if(frontView!=null){frontView.gameObject.SetActive(false);Destroy(frontView.gameObject);}
            if(profileView!=null){profileView.gameObject.SetActive(false);Destroy(profileView.gameObject);}
            frontView=BuildView(art,false);profileView=BuildView(art,true);
            sideWalking=false;PresentFrame(prior,0);
        }

        private CharacterView BuildView(CharacterArt art,bool profile)
        {
            var layers=profile?art.profileLayers:art.layers;
            if(layers==null || layers.Length==0)throw new InvalidOperationException("Missing directional character artwork: "+art.characterId);
            var visual = Joint(art.displayName+(profile?" profile":" front"), transform, new Vector2(0, -45));
            visual.localScale = Vector3.one * (40 * art.scale);
            var view = visual.gameObject.AddComponent<CharacterView>();
            view.characterId = art.characterId; view.displayName = art.displayName;
            view.profileArtwork=profile;
            view.floorUnitsPerArtUnit=40*art.scale;
            view.sourceFacesLeft=true;
            view.showExampleProp = false;
            view.facing = Joint("Facing", visual, Vector2.zero);
            var joints = new Dictionary<string, Transform>();
            foreach (var layer in layers)
            {
                var parent = view.facing;
                var position = new Vector2((layer.pivotX - art.groundX) / 100, (art.groundY - layer.pivotY) / 100);
                if (layer.name.StartsWith("eyes-") || layer.name == "muzzle") { parent = joints["head"]; position = Vector2.zero; }
                if (layer.name == "held-prop")
                {
                    view.handAnchor = Joint("Hand anchor", joints["arm-near"], (Vector3)position - joints["arm-near"].localPosition);
                    parent = view.handAnchor; position = Vector2.zero;
                }
                var joint = Joint(layer.name, parent, position);
                joints.Add(layer.name, joint);
                // The workshop bucket is never duplicated in the real world.
                if (layer.name == "held-prop") continue;
                var picture = Joint("Sprite", joint, new Vector2((layer.left + layer.width / 2 - layer.pivotX) / 100,
                    (layer.pivotY - layer.top - layer.height / 2) / 100));
                picture.sizeDelta = new Vector2(layer.width / 100, layer.height / 100);
                var graphic = picture.gameObject.AddComponent<Image>();
                graphic.sprite = layer.sprite; graphic.raycastTarget = false;
                if(layer.name.StartsWith("foot-") || layer.name.StartsWith("arm-"))
                    picture.gameObject.AddComponent<CharacterLimbBend>().Configure(layer.name.StartsWith("foot-")?.67f:.73f);
                if (layer.name == "eyes-open") view.eyesOpenGraphic = graphic;
                if (layer.name == "eyes-closed") view.eyesClosedGraphic = graphic;
            }
            view.body = joints["body"]; view.head = joints["head"]; view.tail = joints["tail"];
            view.armFar = joints["arm-far"]; view.armNear = joints["arm-near"];
            view.footFar = joints["foot-far"]; view.footNear = joints["foot-near"];
            view.heldProp = joints["held-prop"];
            view.shadow=joints["ground-shadow"];view.shadow.SetParent(visual,false);view.shadow.SetAsFirstSibling();
            view.Present(default, 0);
            return view;
        }

        public void PresentFrame(CharacterFrame frame,float dt)
        {
            if(frontView==null)return;
            // Both lightweight rigs keep the same phase and blink clock. Only
            // the selected one renders; changing drawings never restarts a step.
            frontView.Present(frame,dt);profileView.Present(frame,dt);
            var locomotion=frame.Pose==CharacterPose.Walk || frame.Pose==CharacterPose.Carry || frame.Pose==CharacterPose.Idle;
            if(!locomotion || frame.ResetMotion || profileView.WalkWeight==0)sideWalking=false;
            else if(frame.Speed>1)
                sideWalking=frame.Travel.sqrMagnitude<.00001f || Mathf.Abs(frame.Travel.normalized.x)>.25f;
            view=sideWalking?profileView:frontView;
            frontView.gameObject.SetActive(!sideWalking);profileView.gameObject.SetActive(sideWalking);
        }

        public void Present(Vector2 displayedPosition, string continuity, bool held, float dt)
        {
            if (view != null) PresentFrame(motion.Observe(displayedPosition, continuity, held, false, dt), dt);
        }

        public void PresentHome(Vector2 point,string continuity,bool held,float dt,Core.SoloPlayer player,Core.HomeState home)
        {
            if(view==null)return;
            var frame=motion.Observe(point,continuity,held,false,dt);
            if(!held && Core.HomeLayout.Usable(player.fixture))
            {
                var key=continuity+"/"+player.fixture;
                if(useKey!=key){useKey=key;useAge=(float)player.useSeconds;}
                else useAge=Mathf.Max(useAge+Mathf.Clamp(dt,0,.1f),(float)player.useSeconds);
                frame=new CharacterFrame(Core.HomeLayout.Seat(player.fixture)?CharacterPose.Sit:CharacterPose.Bounce,0,frame.FaceLeft,useAge);
            }
            else if(frame.Pose==CharacterPose.Idle && player.activity=="" && Core.HomeLayout.RadioNear(home,player))
                frame=new CharacterFrame(CharacterPose.Dance,0,frame.FaceLeft);
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
