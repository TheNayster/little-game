using System;
using UnityEngine;
namespace LittleWeeps.Client
{
    public static class WorkshopArt
    {
        private static Sprite[] props,bowl;
        private static Sprite background;
        public static Sprite Background {get{if(background==null){var t=WorldResources.Load<Texture2D>("Worlds/Home/Discovery/workshop-background");background=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f));}return background;}}
        public static Sprite Prop(int index)
        {
            if(props==null){
                var t=WorldResources.Load<Texture2D>("Worlds/Home/Discovery/workshop-props");
                var b=new[]{new Rect(37,151,307,195),new Rect(364,128,372,219),new Rect(769,54,302,311),new Rect(1139,110,269,256),new Rect(47,413,271,274),new Rect(406,411,284,278),new Rect(766,405,281,282),new Rect(1100,425,316,256),new Rect(69,707,259,323),new Rect(429,708,258,323),new Rect(787,708,260,323),new Rect(1195,689,141,342)};
                props=new Sprite[b.Length];for(var i=0;i<b.Length;i++){var r=b[i];props[i]=Sprite.Create(t,new Rect(r.x/1448*t.width,(1086-r.y-r.height)/1086*t.height,r.width/1448*t.width,r.height/1086*t.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);}
            }return props[index];
        }
        public static Sprite Bowl(int index)
        {
            if(bowl==null){var t=WorldResources.Load<Texture2D>("Worlds/Home/Discovery/workshop-bowl");bowl=new Sprite[2];for(var i=0;i<2;i++)bowl[i]=Sprite.Create(t,new Rect(i*t.width/2f,0,t.width/2f,t.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);}
            return bowl[index];
        }
    }
}
