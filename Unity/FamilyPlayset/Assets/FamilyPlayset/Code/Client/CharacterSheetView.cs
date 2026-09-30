using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Plays the selected raster drawings without deforming their face or body.
    // Movement and saves remain owned by the existing player controller.
    public sealed class CharacterSheetView : MonoBehaviour
    {
        private CharacterArt art;
        private RectTransform facing, picture, shadow;
        private RawImage image;
        private CharacterArt outfitArt;
        private Material clothMaterial;
        private string outfit="",outfitColor="green";
        public string Outfit => outfit;
        private float time, cycle;
        // Match the requested 2x travel increase while preserving the
        // quieter step rhythm the user accepted in build 123.
        public const float WalkStride = 320;
        public CharacterFrame Frame { get; private set; }
        public int FrameIndex { get; private set; }
        public float WalkPhase => cycle;
        public float WalkWeight => Frame.Speed > 1 && (Frame.Pose == CharacterPose.Walk || Frame.Pose == CharacterPose.Carry) ? 1 : 0;
        public string CharacterId => art.characterId;
        public float SupportScale => art.scale;
        public string SourceHash => art.sheetSha256;
        public string WalkSourceHash => art.walkSheetSha256;
        public bool IsWalkDrawing => image.texture == art.walkSheet;
        public float FacingSign => facing.localScale.x;
        public int FrameCount => art.frames.Length;
        public Vector3 Ground => transform.position;

        public void Configure(CharacterArt source, CharacterSheetView prior)
        {
            art = source;
            if (art.sheet == null || art.frames == null || art.frames.Length != 16 || art.referenceHeight <= 0)
                throw new InvalidOperationException("Missing prepared character sheet: " + art.characterId);
            if (art.walkSheet == null || art.walkFrames == null || art.walkFrames.Length != 8 || art.walkReferenceHeight <= 0)
                throw new InvalidOperationException("Missing relaxed walking sheet: " + art.characterId);
            if (prior != null) { time = prior.time; cycle = prior.cycle; }
            transform.localScale = Vector3.one * art.scale;
            shadow = Rect("Ground shadow", transform);
            shadow.sizeDelta = new Vector2(76, 11);
            var shade = shadow.gameObject.AddComponent<Image>();
            shade.sprite = art.shadowSprite; shade.raycastTarget = false;
            facing = Rect("Facing", transform);
            picture = Rect("Selected character drawing", facing);
            image = picture.gameObject.AddComponent<RawImage>();
            image.texture = art.sheet; image.raycastTarget = false;
        }

        public void Wear(string id,string colorId)
        {
            if(outfit==id && outfitColor==colorId)return;
            outfit=id??"";outfitColor=colorId??"green";
            outfitArt=outfit==""?null:Resources.Load<CharacterArt>("CharacterOutfitArt/"+outfit+"/"+art.characterId);
            if(outfit!="" && outfitArt==null)throw new InvalidOperationException("Missing prepared outfit sheet: "+outfit+"/"+art.characterId);
            if(outfit=="dinosaur" && clothMaterial==null)
                clothMaterial=new Material(Resources.Load<Shader>("CharacterOutfits/DinosaurCloth"));
            image.material=outfit=="dinosaur" && outfitColor!="green"?clothMaterial:null;
            if(clothMaterial!=null)clothMaterial.SetColor("_Cloth",CharacterOutfitPalette.Cloth(outfitColor));
            Present(Frame,0);
        }
        private void OnDestroy(){if(clothMaterial!=null)Destroy(clothMaterial);}

        public void Present(CharacterFrame frame, float dt)
        {
            Frame = frame; dt = Mathf.Clamp(dt, 0, .1f); time += dt;
            var moving = frame.Speed > 1 && (frame.Pose == CharacterPose.Walk || frame.Pose == CharacterPose.Carry);
            if (frame.ResetMotion) cycle = 0;
            else if (moving)
            {
                var distance = frame.Travel.magnitude;
                if (distance == 0) distance = frame.Speed * dt;
                cycle = Mathf.Repeat(cycle + distance / WalkStride, 1);
            }
            var blink = time % 4.3f > 4.14f;
            var index = moving ? 4 + Mathf.Min(7, Mathf.FloorToInt(cycle * 8)) : blink ? 1 : 0;
            var offset = Vector2.zero;
            var scale = Vector2.one;
            if (frame.Pose == CharacterPose.Sit)
            {
                index = 12;
                offset.y = 60 * Mathf.SmoothStep(0, 1, frame.UseSeconds / .32f);
            }
            else if (frame.Pose == CharacterPose.Bounce)
            {
                var phase = Mathf.Repeat(Mathf.Max(0, frame.UseSeconds - .3f) / 1.05f, 1);
                var height = Mathf.Sin(Mathf.PI * phase);
                index = height > .45f ? 2 : 0;
                offset.y = 84 + height * 108;
                scale = new Vector2(1 + .045f * (1 - height), 1 - .06f * (1 - height));
            }
            else if (frame.Pose == CharacterPose.Dance)
            {
                var beat = time * 1.6f;
                index = (int)(beat * 2) % 2 == 0 ? 14 : 15;
                offset.y = Mathf.Abs(Mathf.Sin(beat * Mathf.PI * 2)) * 3;
            }
            else if (frame.Pose == CharacterPose.BalloonTap)
            {index=frame.UseSeconds<.22f?2:3;offset.y=Mathf.Sin(Mathf.Clamp01(frame.UseSeconds/.38f)*Mathf.PI)*2;}
            else if (frame.Pose == CharacterPose.Wave) index = 2 + (int)(time * 4) % 2;
            else if (frame.Pose == CharacterPose.Roar) index = 14 + (int)(time * 4) % 2;
            else if (frame.Pose == CharacterPose.Carry && !moving) index = 13;
            FrameIndex = index;
            // Only walking changes atlas. Keep the already accepted appearance
            // for idle and home actions rather than regenerating those poses.
            var walking = index >= 4 && index < 12;
            var drawingArt=outfitArt??art;
            var texture = walking ? drawingArt.walkSheet : drawingArt.sheet;
            image.texture = texture;
            var drawing = walking ? drawingArt.walkFrames[index - 4] : drawingArt.frames[index];
            var crop = drawing.pixels.Value;
            image.uvRect = new Rect(crop.x / texture.width, 1 - crop.yMax / texture.height,
                crop.width / texture.width, crop.height / texture.height);
            picture.pivot = new Vector2((drawing.ground.x - crop.x) / crop.width,
                (crop.yMax - drawing.ground.y) / crop.height);
            picture.sizeDelta = crop.size * (180 / (walking ? drawingArt.walkReferenceHeight : drawingArt.referenceHeight));
            var resting=frame.Pose==CharacterPose.Rest;
            facing.localRotation=Quaternion.Euler(0,0,resting?90:0);
            facing.anchoredPosition = resting?new Vector2(-65,210):offset;
            shadow.gameObject.SetActive(!resting);
            facing.localScale = new Vector3((frame.FaceLeft ? -1 : 1) * scale.x, scale.y, 1);
            shadow.anchoredPosition = new Vector2(0, frame.Pose == CharacterPose.Bounce ? 84 : frame.Pose == CharacterPose.Sit ? 52 : 0);
            shadow.localScale = frame.Pose == CharacterPose.Bounce ? Vector3.one * Mathf.Lerp(1, .6f, Mathf.Clamp01((offset.y - 84) / 108)) : Vector3.one;
        }

        public void PresentRiding(Texture2D texture,Rect crop,Vector2 contact,float height,bool left)
        {
            // Authored seat/deck pixels become the drawing's pivot. Equipment
            // and rider then share an explicit contact, not a guessed body lift.
            image.texture=texture;image.material=null;
            image.uvRect=new Rect(crop.x/texture.width,1-crop.yMax/texture.height,crop.width/texture.width,crop.height/texture.height);
            picture.pivot=new Vector2((contact.x-crop.x)/crop.width,(crop.yMax-contact.y)/crop.height);
            picture.sizeDelta=crop.size*(180/height);
            facing.anchoredPosition=new Vector2(0,45);facing.localRotation=Quaternion.identity;
            facing.localScale=new Vector3(left?-1:1,1,1);shadow.gameObject.SetActive(false);
        }

        public void AttachToSupport(float angle)
        {
            // Park anchors describe the actual contact point in the equipment
            // art. Remove the Home sofa's 60-unit lift and ground-only shadow.
            // The selected-sheet joint is 45 below the avatar root.
            facing.anchoredPosition=new Vector2(0,45);
            facing.localRotation=Quaternion.Euler(0,0,angle);
            shadow.gameObject.SetActive(false);
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.sizeDelta = Vector2.zero;
            return rect;
        }
    }
}
