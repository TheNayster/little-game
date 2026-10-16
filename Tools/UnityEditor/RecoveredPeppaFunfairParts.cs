using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static partial class RecoveredPeppaFunfairBake
{
    [Serializable] public sealed class WheelFrames {public Rect pixels;public string texture="wheel.png";public int frames=72,columns=8,size=512;public Vector2[] seats;}
    static void BakeSceneParts(int index,Camera camera,Dictionary<long,Transform> go,Dictionary<long,Transform> transforms)
    {
        var all=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);var enabled=all.ToDictionary(r=>r,r=>r.enabled);
        void Only(Renderer[] keep){foreach(var r in all)r.enabled=keep.Contains(r) && enabled[r] && !(r is ParticleSystemRenderer);}
        void Restore(){foreach(var pair in enabled)if(pair.Key)pair.Key.enabled=pair.Value;}
        void Prop(long id,string key){
            var clone=UnityEngine.Object.Instantiate(go[id].gameObject);clone.SetActive(true);clone.transform.position=new Vector3(10000,0,0);foreach(var r in clone.GetComponentsInChildren<Renderer>(true)){if(r is ParticleSystemRenderer){r.enabled=false;continue;}r.enabled=true;Recover(r);}
            var renderers=clone.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();if(renderers.Length==0){UnityEngine.Object.DestroyImmediate(clone);return;}var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            var cam=new GameObject("PEP-01 prop capture").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=Math.Max(bounds.size.x,bounds.size.y)*.65f;cam.transform.rotation=camera.transform.rotation;cam.transform.position=bounds.center-cam.transform.forward*12;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;
            var pivot=Pixel(cam,clone.transform.position);propFrames.Add(new RecoveredBirthdayBake.PropFrame{name=key,texture="Props/"+key+".png",view="front",unitsPerPixel=cam.orthographicSize*2/256,pivot=pivot,holdPivot=pivot,sourceBaseId=id,sourceHoldId=id});
            var texture=Capture(cam,256,256);Directory.CreateDirectory(Path.Combine(Output,"Props"));File.WriteAllBytes(Path.Combine(Output,"Props",key+".png"),texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(cam.gameObject);UnityEngine.Object.DestroyImmediate(clone);
        }
        if(index==0){Prop(233,"drink");Prop(go.First(p=>p.Value==transforms[1334]).Key,"teacup");}if(index==1){Prop(366,"flower");Prop(770,"rod");go[770].gameObject.SetActive(false);}if(index==2)Prop(2449,"balloon");
        if(index==0){
            var wheel=go[242];var hinges=new[]{transforms[1049],transforms[1202],transforms[1213]};var chairs=new[]{transforms[1334],transforms[1294],transforms[1000]};var parts=wheel.GetComponentsInChildren<Renderer>(true).Concat(chairs.SelectMany(h=>h.GetComponentsInChildren<Renderer>(true))).Distinct().ToArray();var rotations=chairs.Select(t=>t.rotation).ToArray();var positions=chairs.Select(t=>t.position).ToArray();var rotation=wheel.localRotation;
            // Scene component IDs 1950/1991/2098 own these seat transforms.
            var seats=new[]{go[618],go[299],go[466]};
            Only(parts);camera.backgroundColor=Color.clear;var first=Capture(camera,1200,900);var pixels=first.GetPixels32();int x0=1200,x1=0,y0=900,y1=0;
            for(int y=0;y<900;y++)for(int x=0;x<1200;x++)if(pixels[y*1200+x].a>0){x0=Math.Min(x0,x);x1=Math.Max(x1,x+1);y0=Math.Min(y0,y);y1=Math.Max(y1,y+1);}
            UnityEngine.Object.DestroyImmediate(first);var area=new Rect(Math.Max(0,x0-15),Math.Max(0,y0-15),Math.Min(1200,x1+15)-Math.Max(0,x0-15),Math.Min(900,y1+15)-Math.Max(0,y0-15));
            // A square capture region encloses all rotations of the original
            // wheel while retaining upright cabins through their source hinges.
            float extent=Math.Max(area.width,area.height);area=new Rect(area.center-Vector2.one*extent*.5f,Vector2.one*extent);
            var data=new WheelFrames{pixels=area,seats=new Vector2[72*3]};var atlas=new Texture2D(4096,4608,TextureFormat.RGBA32,false);atlas.SetPixels32(new Color32[4096*4608]);
            var fullRect=camera.rect;
            for(int i=0;i<72;i++){
                // The source cabins are separate rigid-body objects. Their
                // anchors coincide with the wheel hinges, not its descendants.
                wheel.localRotation=rotation*Quaternion.Euler(0,0,i*5);for(int j=0;j<3;j++){chairs[j].position=hinges[j].position;chairs[j].rotation=rotations[j];}
                for(int j=0;j<3;j++){var p=camera.WorldToViewportPoint(seats[j].position);data.seats[i*3+j]=new Vector2(p.x*1200,p.y*900);}
                var full=Capture(camera,1200,900);var frame=new Texture2D(512,512,TextureFormat.RGBA32,false);var colours=new Color[512*512];
                for(int y=0;y<512;y++)for(int x=0;x<512;x++){float px=area.x+(x+.5f)/512*area.width,py=area.y+(y+.5f)/512*area.height;colours[y*512+x]=px<0||px>=1200||py<0||py>=900?Color.clear:full.GetPixelBilinear(px/1200,py/900);}
                atlas.SetPixels(i%8*512,(8-i/8)*512,512,512,colours);UnityEngine.Object.DestroyImmediate(frame);UnityEngine.Object.DestroyImmediate(full);
            }
            atlas.Apply();File.WriteAllBytes(Path.Combine(Output,"wheel.png"),atlas.EncodeToPNG());File.WriteAllText(Path.Combine(Output,"wheel.json"),JsonUtility.ToJson(data,true));UnityEngine.Object.DestroyImmediate(atlas);
            wheel.localRotation=rotation;for(int j=0;j<3;j++){chairs[j].position=positions[j];chairs[j].rotation=rotations[j];}Restore();foreach(var r in parts)r.enabled=false;
            foreach(var id in new long[]{233,234,235,295})if(go.ContainsKey(id))go[id].gameObject.SetActive(false);
            camera.backgroundColor=new Color(.56f,.76f,.92f,1);
        }
    }
}
