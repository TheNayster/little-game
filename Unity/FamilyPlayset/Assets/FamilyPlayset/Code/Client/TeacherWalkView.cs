using System;
using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    // The teacher's original four action pictures have no walk cycle. Use a
    // separate eight-frame walking atlas, with explicit crops and floor pivots.
    public sealed class TeacherWalkView : MonoBehaviour
    {
        [Serializable] private sealed class Data
        {
            public float referenceHeight,walkReferenceHeight;
            public CharacterArt.SheetFrame[] poses,walk;
        }
        private RawImage picture;
        private Texture2D poses,walking;
        private Data data;
        private float phase,face=1;
        public int Drawing { get; private set; }
        public bool Walking { get; private set; }
        public Vector2 Floor => picture.rectTransform.anchoredPosition;
        public void Configure(RawImage target,Texture2D actionAtlas)
        {
            picture=target;poses=actionAtlas;
            walking=Resources.Load<Texture2D>("Daycare/calypso-walk");
            var metadata=Resources.Load<TextAsset>("Daycare/calypso-animation");
            if(walking==null || metadata==null)throw new InvalidOperationException("Missing Calypso walking artwork.");
            data=JsonUtility.FromJson<Data>(metadata.text);
            if(data.walk.Length!=8 || data.poses.Length!=4)throw new InvalidOperationException("Invalid Calypso animation frames.");
        }
        public void Present(CharacterFrame frame,int restingPose,float dt,float worldScale)
        {
            Walking=frame.Pose==CharacterPose.Walk && frame.Speed>.6f;
            if(frame.ResetMotion)phase=0;
            if(Walking)
            {
                face=frame.FaceLeft?1:-1;
                phase=Mathf.Repeat(phase+Mathf.Min(frame.Travel.magnitude/(160*worldScale),Mathf.Clamp(dt,0,.1f)*1.8f),1);
            }
            Drawing=Walking?4+Mathf.Min(7,Mathf.FloorToInt(phase*8)):Mathf.Clamp(restingPose,0,3);
            var texture=Walking?walking:poses;
            var drawing=Walking?data.walk[Drawing-4]:data.poses[Drawing];var crop=drawing.pixels.Value;
            picture.texture=texture;picture.uvRect=new Rect(crop.x/texture.width,1-crop.yMax/texture.height,crop.width/texture.width,crop.height/texture.height);
            var rect=picture.rectTransform;
            rect.pivot=new Vector2((drawing.ground.x-crop.x)/crop.width,(crop.yMax-drawing.ground.y)/crop.height);
            rect.sizeDelta=crop.size*(270/(Walking?data.walkReferenceHeight:data.referenceHeight));
            rect.anchoredPosition=Vector2.zero;rect.localScale=new Vector3(face,1,1);
        }
    }
}
