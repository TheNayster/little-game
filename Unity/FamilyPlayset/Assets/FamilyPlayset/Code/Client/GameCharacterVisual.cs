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
        private string useKey="";private float useAge;
        public string CharacterId => view == null ? "" : view.characterId;
        public CharacterFrame Frame => view == null ? default : view.Frame;
        public int LayerCount => view == null ? 0 : view.GetComponentsInChildren<Image>(true).Length;

        public void Select(string savedAvatar)
        {
            var id = savedAvatar == "orange-pup" ? "bingo" : "bluey";
            if (CharacterId == id) return;
            var art = Resources.Load<CharacterArt>("CharacterArt/" + id);
            if (art == null) throw new InvalidOperationException("Missing built character artwork: " + id);
            var prior = Frame;
            if (view != null) { view.gameObject.SetActive(false); Destroy(view.gameObject); }
            var visual = Joint(art.displayName, transform, new Vector2(0, -45));
            visual.localScale = Vector3.one * (40 * art.scale);
            view = visual.gameObject.AddComponent<CharacterView>();
            view.characterId = id; view.displayName = art.displayName;
            view.showExampleProp = false;
            view.facing = Joint("Facing", visual, Vector2.zero);
            var joints = new Dictionary<string, Transform>();
            foreach (var layer in art.layers)
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
                if (layer.name == "eyes-open") view.eyesOpenGraphic = graphic;
                if (layer.name == "eyes-closed") view.eyesClosedGraphic = graphic;
            }
            view.body = joints["body"]; view.head = joints["head"]; view.tail = joints["tail"];
            view.armFar = joints["arm-far"]; view.armNear = joints["arm-near"];
            view.footFar = joints["foot-far"]; view.footNear = joints["foot-near"];
            view.heldProp = joints["held-prop"];
            view.shadow=joints["ground-shadow"];view.shadow.SetParent(visual,false);view.shadow.SetAsFirstSibling();
            view.Present(prior, 0);
        }

        public void Present(Vector2 displayedPosition, string continuity, bool held, float dt)
        {
            if (view != null) view.Present(motion.Observe(displayedPosition, continuity, held, false, dt), dt);
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
            view.Present(frame,dt);
        }

        private static RectTransform Joint(string name, Transform parent, Vector2 position)
        {
            var result = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            result.SetParent(parent, false); result.anchoredPosition = position; result.sizeDelta = Vector2.zero;
            return result;
        }
    }
}
