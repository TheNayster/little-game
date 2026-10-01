using System;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    internal sealed class VetAnimalView : MonoBehaviour
    {
        [Serializable] private sealed class Registration {public Foot[] frames;}
        [Serializable] private sealed class Foot {public float foot,center,left,top,width,height;}
        private RawImage image;private int species=-1;private Texture2D pets,walk,dino;private Registration rests,steps;
        public int Drawing {get;private set;}public bool Walking {get;private set;}
        private void Awake(){var drawing=new GameObject("Registered animal drawing",typeof(RectTransform),typeof(RawImage));drawing.transform.SetParent(transform,false);image=drawing.GetComponent<RawImage>();image.raycastTarget=false;}
        public void Present(int patient,double time,bool walking,bool happy,float size,bool left=true)
        {
            if(species!=patient){species=patient;dino=patient<4?null:Resources.Load<Texture2D>("DinosaurWorld/"+DaycareVet.Species(patient));}
            if(patient<4){if(pets==null){pets=Resources.Load<Texture2D>("Vet/pets");walk=Resources.Load<Texture2D>("Vet/pet-walk");rests=JsonUtility.FromJson<Registration>(Resources.Load<TextAsset>("Vet/pet-registration").text);steps=JsonUtility.FromJson<Registration>(Resources.Load<TextAsset>("Vet/walk-registration").text);}}
            Walking=walking;
            var frame=walking?(int)(time*5)%4:happy?2:time%4.6<.2?1:0;Drawing=frame;
            image.texture=patient<4?(walking?walk:pets):dino;
            if(patient>=4){frame=walking?frame:happy?7:time%5.7<.45?5:4;Drawing=frame;image.uvRect=new Rect(frame%4*.25f,frame<4?.5f:0,.25f,.5f);var land=DinosaurLandmarks.Get(DaycareVet.Species(patient),frame);image.rectTransform.pivot=new Vector2(.5f,1-land.z);image.rectTransform.anchoredPosition=Vector2.zero;image.rectTransform.sizeDelta=Vector2.one*size;}
            else{var f=(walking?steps:rests).frames[patient*4+frame];image.uvRect=new Rect(f.left,1-f.top-f.height,f.width,f.height);image.rectTransform.pivot=new Vector2(.5f,0);image.rectTransform.anchoredPosition=Vector2.zero;image.rectTransform.sizeDelta=new Vector2(f.width*4*size,f.height*4*size);}
            // Register at the feet before breathing/stretching; the cushion contact
            // stays fixed even when a reaction changes the selected sprite frame.
            var breath=walking?0:(float)Math.Sin(time*2.3+patient)*.006f;
            image.rectTransform.localScale=new Vector3((left?1:-1)*(1-breath*.4f),1+breath,1);
        }
    }
}
