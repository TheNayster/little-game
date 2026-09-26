using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // Replaceable visual child of a gameplay root. Owns neither position,
    // inventory nor saves; a supplied frame only poses the imported joints.
    public sealed class CharacterView : MonoBehaviour
    {
        public string characterId, displayName;
        public Transform shadow;
        public Transform facing, body, head, tail, armFar, armNear, footFar, footNear, handAnchor, heldProp;
        public SpriteRenderer eyesOpen, eyesClosed;
        public Graphic eyesOpenGraphic, eyesClosedGraphic;
        public bool showExampleProp = true;
        public SortingGroup sorting;
        private Vector3 bodyRest, headRest, farRest, nearRest, footFarRest, footNearRest;
        private bool initialized;
        private float time, cycle;
        public CharacterFrame Frame { get; private set; }

        private void Initialize()
        {
            if (initialized) return;
            bodyRest = body.localPosition; headRest = head.localPosition;
            farRest = armFar.localPosition; nearRest = armNear.localPosition;
            footFarRest=footFar.localPosition;footNearRest=footNear.localPosition;
            initialized = true;
        }

        public void Present(CharacterFrame frame, float deltaTime)
        {
            Initialize(); Frame = frame;
            var dt = Mathf.Clamp(deltaTime, 0, .1f);
            time += dt;
            var stride = Mathf.Clamp01(frame.Speed / Core.Walking.Speed);
            cycle = Mathf.Repeat(cycle + dt * Mathf.PI * 2 * 1.35f * stride, Mathf.PI * 2);
            var walking = stride > .005f;
            var swing = walking ? Mathf.Sin(cycle) * 19 * stride : 0;
            var bob = walking ? Mathf.Abs(Mathf.Sin(cycle)) * .03f * stride : -Mathf.Sin(time * 2) * .012f;
            facing.localScale = new Vector3(frame.FaceLeft ? -1 : 1, 1, 1);
            facing.localPosition=Vector3.zero;facing.localRotation=Quaternion.identity;
            footFar.localPosition=footFarRest;footNear.localPosition=footNearRest;
            footFar.localScale=footNear.localScale=Vector3.one;
            if(shadow!=null){shadow.localPosition=Vector3.zero;shadow.localScale=Vector3.one;}
            body.localPosition = bodyRest + Vector3.up * bob;
            head.localPosition = headRest + Vector3.up * bob;
            // The face markings belong to the continuous box-shaped torso.
            // Do not rotate them as a detached round head over the body outline.
            head.localRotation = Quaternion.identity;
            tail.localRotation = Rotation(-Mathf.Sin(time * (frame.Pose == CharacterPose.Wave ? 9 : 4)) * 5);
            footFar.localRotation = Rotation(-swing);
            footNear.localRotation = Rotation(swing);
            armFar.localPosition = farRest + Vector3.up * bob;
            armNear.localPosition = nearRest + Vector3.up * bob;
            armFar.localRotation = Rotation(swing * .65f);
            var armAngle = frame.Pose == CharacterPose.Wave ? 132 - Mathf.Sin(time * 7) * 14 :
                frame.Pose == CharacterPose.Carry ? 12 : -swing * .7f;
            armNear.localRotation = Rotation(armAngle);
            heldProp.gameObject.SetActive(showExampleProp && frame.Pose == CharacterPose.Carry);
            // Counter-rotate inside the mirrored hierarchy; the grip stays on
            // the hand and the bucket stays upright in both facing directions.
            heldProp.localRotation = Rotation(-armAngle);
            if(frame.Pose==CharacterPose.Sit)
            {
                var enter=Mathf.SmoothStep(0,1,frame.UseSeconds/.32f);
                facing.localPosition=Vector3.up*1.5f*enter;
                body.localPosition=bodyRest+Vector3.down*.18f*enter;
                head.localPosition=headRest+Vector3.down*.18f*enter;
                footFar.localPosition=footFarRest+new Vector3(-.12f,-.24f,0)*enter;
                footNear.localPosition=footNearRest+new Vector3(.12f,-.24f,0)*enter;
                footFar.localRotation=Rotation(-35*enter);footNear.localRotation=Rotation(35*enter);
                footFar.localScale=footNear.localScale=new Vector3(1,.7f,1);
                if(shadow!=null)shadow.localPosition=Vector3.up*1.3f;
                armFar.localRotation=Rotation(-24*enter);armNear.localRotation=Rotation(24*enter);
            }
            else if(frame.Pose==CharacterPose.Bounce)
            {
                var age=Mathf.Max(0,frame.UseSeconds-.3f);
                var phase=Mathf.Repeat(age/1.05f,1);
                var height=Mathf.Sin(Mathf.PI*phase);
                var squash=1-Mathf.Abs(Mathf.Sin(Mathf.PI*phase));
                facing.localPosition=Vector3.up*(2.1f+height*2.7f);
                if(shadow!=null){shadow.localPosition=Vector3.up*2.1f;shadow.localScale=Vector3.one*(1-.4f*height);}
                facing.localScale=new Vector3((frame.FaceLeft?-1:1)*(1+.08f*squash),1-.12f*squash,1);
                footFar.localRotation=Rotation(-30*height);footNear.localRotation=Rotation(30*height);
                footFar.localPosition=footFarRest+Vector3.up*.25f*height;
                footNear.localPosition=footNearRest+Vector3.up*.25f*height;
                armFar.localRotation=Rotation(-45*height);armNear.localRotation=Rotation(45*height);
            }
            else if(frame.Pose==CharacterPose.Dance)
            {
                var beat=time*Mathf.PI*2*1.6f;var wave=Mathf.Sin(beat);var phrase=(int)(time/2.5f)%3;
                facing.localPosition=new Vector3(phrase==0?wave*.16f:0,Mathf.Abs(wave)*.16f,0);
                facing.localRotation=Rotation(wave*(phrase==2?9:3));
                armFar.localRotation=Rotation(phrase==1?-95-wave*22:-25-wave*20);
                armNear.localRotation=Rotation(phrase==1?95+wave*22:25-wave*20);
                footFar.localRotation=Rotation(-wave*22);footNear.localRotation=Rotation(wave*22);
                tail.localRotation=Rotation(wave*14);
            }
            var blink = time % 4.3f > 4.14f;
            if (eyesOpen != null) eyesOpen.enabled = !blink;
            if (eyesClosed != null) eyesClosed.enabled = blink;
            if (eyesOpenGraphic != null) eyesOpenGraphic.enabled = !blink;
            if (eyesClosedGraphic != null) eyesClosedGraphic.enabled = blink;
        }

        public void SetFloorDepth(float worldY)
        {
            sorting.sortingOrder = Mathf.Clamp(-Mathf.RoundToInt(worldY * 100), -30000, 30000);
        }

        private static Quaternion Rotation(float degrees) => Quaternion.Euler(0, 0, degrees);
    }
}
