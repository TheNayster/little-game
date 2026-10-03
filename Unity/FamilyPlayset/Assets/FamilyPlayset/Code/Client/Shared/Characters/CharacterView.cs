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
        public bool sourceFacesLeft;
        public bool profileArtwork;
        public SortingGroup sorting;
        public float floorUnitsPerArtUnit=40;
        private Vector3 bodyRest, headRest, farRest, nearRest, footFarRest, footNearRest;
        private Transform torso;
        private CharacterLimbBend farLegBend,nearLegBend,farArmBend,nearArmBend;
        private Vector2 travelDirection=Vector2.right;
        private bool initialized;
        private float time, cycle, walkWeight;
        public float WalkPhase=>cycle;
        public float WalkWeight=>walkWeight;
        public Vector3 FarContact=>footFar.TransformPoint(new Vector3(0,-1.02f,0)+(Vector3)(farLegBend==null?Vector2.zero:farLegBend.EndOffset));
        public Vector3 NearContact=>footNear.TransformPoint(new Vector3(0,-1.02f,0)+(Vector3)(nearLegBend==null?Vector2.zero:nearLegBend.EndOffset));
        public CharacterFrame Frame { get; private set; }

        private void Initialize()
        {
            if (initialized) return;
            // Body and facial layers share a pelvis pivot: sway never opens a
            // seam between Bluey's head patches, muzzle and rectangular torso.
            torso=new GameObject("Connected torso",facing is RectTransform?typeof(RectTransform):typeof(Transform)).transform;
            torso.SetParent(facing,false);torso.localPosition=(footFar.localPosition+footNear.localPosition)*.5f;
            foreach(var joint in new[]{tail,armFar,body,head,armNear})joint.SetParent(torso,true);
            bodyRest = body.localPosition; headRest = head.localPosition;
            farRest = armFar.localPosition; nearRest = armNear.localPosition;
            footFarRest=footFar.localPosition;footNearRest=footNear.localPosition;
            farLegBend=footFar.GetComponentInChildren<CharacterLimbBend>();nearLegBend=footNear.GetComponentInChildren<CharacterLimbBend>();
            farArmBend=armFar.GetComponentInChildren<CharacterLimbBend>();nearArmBend=armNear.GetComponentInChildren<CharacterLimbBend>();
            initialized = true;
        }

        public void Present(CharacterFrame frame, float deltaTime)
        {
            Initialize(); Frame = frame;
            var dt = Mathf.Clamp(deltaTime, 0, .1f);
            time += dt;
            var locomotion=frame.Pose==CharacterPose.Idle || frame.Pose==CharacterPose.Walk || frame.Pose==CharacterPose.Carry;
            if(frame.ResetMotion){walkWeight=0;cycle=0;travelDirection=frame.FaceLeft?Vector2.left:Vector2.right;}
            var strideLength=CharacterWalk.Stride;
            if(frame.Travel.sqrMagnitude>.00001f)travelDirection=frame.Travel.normalized;
            var distance=frame.Travel.magnitude;
            // The standalone SpriteRenderer workshop also supplies explicit
            // speed frames. Game frames always include measured floor travel.
            if(distance==0 && frame.Speed>1 && !frame.ResetMotion)distance=frame.Speed*dt;
            if(locomotion)cycle=Mathf.Repeat(cycle+distance/strideLength,1);
            walkWeight=Mathf.MoveTowards(walkWeight,locomotion && frame.Speed>1?1:0,dt/.14f);
            if(!locomotion)walkWeight=0;
            var angle=cycle*Mathf.PI*2;
            var swing=Mathf.Cos(angle-.16f)*walkWeight;
            var bob=-Mathf.Sin(time*2)*.012f*(1-walkWeight);
            // The SVG's nose points left and tail sits to the right. Mirroring
            // it as if it faced right made the old character walk backwards.
            var mirror=(frame.FaceLeft?-1:1)*(sourceFacesLeft?-1:1);
            facing.localScale = new Vector3(mirror, 1, 1);
            facing.localPosition=Vector3.zero;facing.localRotation=Quaternion.identity;
            footFar.localPosition=footFarRest;footNear.localPosition=footNearRest;
            footFar.localScale=footNear.localScale=Vector3.one;
            torso.localPosition=(footFarRest+footNearRest)*.5f;torso.localRotation=Quaternion.identity;
            farLegBend?.Pose(Vector2.zero,0);nearLegBend?.Pose(Vector2.zero,0);
            farArmBend?.Pose(Vector2.zero,0);nearArmBend?.Pose(Vector2.zero,0);
            if(shadow!=null){shadow.localPosition=Vector3.zero;shadow.localScale=Vector3.one;}
            body.localPosition = bodyRest + Vector3.up * bob;
            head.localPosition = headRest + Vector3.up * bob;
            // The face markings belong to the continuous box-shaped torso.
            // Do not rotate them as a detached round head over the body outline.
            head.localRotation = Quaternion.identity;
            tail.localRotation = Rotation(-Mathf.Sin(time * (frame.Pose == CharacterPose.Wave ? 9 : 4)) * 5);
            footFar.localRotation = Quaternion.identity;
            footNear.localRotation = Quaternion.identity;
            armFar.localPosition = farRest + Vector3.up * bob;
            armNear.localPosition = nearRest + Vector3.up * bob;
            var forward=travelDirection.x*mirror;
            armFar.localRotation = Rotation(6*walkWeight-forward*swing*8);
            var armAngle = frame.Pose == CharacterPose.Wave ? 132 - Mathf.Sin(time * 7) * 14 :
                frame.Pose == CharacterPose.Carry ? 12 : -6*walkWeight+forward*Mathf.Cos(angle-.24f)*8*walkWeight;
            armNear.localRotation = Rotation(armAngle);
            heldProp.gameObject.SetActive(showExampleProp && frame.Pose == CharacterPose.Carry);
            // Counter-rotate inside the mirrored hierarchy; the grip stays on
            // the hand and the bucket stays upright in both facing directions.
            heldProp.localRotation = Rotation(-armAngle);
            if(locomotion && walkWeight>0)
            {
                var hip=new Vector2(Mathf.Sin(angle)*.012f,CharacterWalk.HipHeight(cycle))*walkWeight;
                torso.localPosition+=(Vector3)hip;
                torso.localRotation=Rotation((-forward*1.4f+Mathf.Sin(angle-.15f)*.4f)*walkWeight);
                var projected=new Vector2(forward,travelDirection.y*.45f);
                // The standing drawing has widely separated frontal hips.
                // Bring them onto the side-view walking lane for horizontal
                // travel; otherwise the long stride draws an X through both legs.
                var lane=profileArtwork?0:Mathf.Abs(travelDirection.x)*.30f*walkWeight;
                PoseFoot(footFar,farLegBend,footFarRest+new Vector3(lane,.025f*walkWeight,0),CharacterWalk.Sample(cycle),projected,hip,strideLength);
                PoseFoot(footNear,nearLegBend,footNearRest+new Vector3(-lane,-.025f*walkWeight,0),CharacterWalk.Sample(cycle+.5f),projected,hip,strideLength);
                farArmBend?.Pose(new Vector2(0,.025f)*walkWeight,-.02f*walkWeight);
                if(frame.Pose!=CharacterPose.Carry)nearArmBend?.Pose(new Vector2(0,.025f)*walkWeight,.02f*walkWeight);
                tail.localRotation=Rotation(Mathf.Sin(angle-.55f)*3*walkWeight);
            }
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
                facing.localScale=new Vector3(mirror*(1+.08f*squash),1-.12f*squash,1);
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
            heldProp.localRotation=Rotation(-armAngle-torso.localEulerAngles.z);
            var blink = time % 4.3f > 4.14f;
            if (eyesOpen != null) eyesOpen.enabled = !blink;
            if (eyesClosed != null) eyesClosed.enabled = blink;
            if (eyesOpenGraphic != null) eyesOpenGraphic.enabled = !blink;
            if (eyesClosedGraphic != null) eyesClosedGraphic.enabled = blink;
        }

        private void PoseFoot(Transform foot,CharacterLimbBend bend,Vector3 rest,CharacterWalk.Foot pose,Vector2 direction,Vector2 hip,float stride)
        {
            var target=(direction*(pose.Along*stride/floorUnitsPerArtUnit)+Vector2.up*pose.Lift)*walkWeight;
            foot.localPosition=rest+(Vector3)hip;
            if(bend!=null)bend.Pose(target-hip,pose.Bend*walkWeight*Mathf.Sign(direction.x));
            else foot.localPosition=rest+(Vector3)target;
        }

        public void SetFloorDepth(float worldY)
        {
            sorting.sortingOrder = Mathf.Clamp(-Mathf.RoundToInt(worldY * 100), -30000, 30000);
        }

        private static Quaternion Rotation(float degrees) => Quaternion.Euler(0, 0, degrees);
    }
}
