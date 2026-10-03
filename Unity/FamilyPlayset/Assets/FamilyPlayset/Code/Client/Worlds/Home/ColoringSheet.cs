using System;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    // One active full-resolution page; colors travel as bounded region IDs.
    // Original publisher line art remains unchanged beneath the shader.
    public sealed class ColoringSheet:RawImage,IPointerClickHandler
    {
        [Serializable] public sealed class Entry {public string id,name;public int width,height,regions;}
        [Serializable] public sealed class Catalog {public Entry[] pages;}
        private static Catalog catalog;
        private sealed class SharedArt {public Texture2D texture;public int users;}
        private static readonly Dictionary<int,SharedArt> activeArt=new Dictionary<int,SharedArt>();
        public static Catalog Pages=>catalog??(catalog=JsonUtility.FromJson<Catalog>(WorldResources.Load<TextAsset>("Worlds/Home/Discovery/Coloring/catalog").text));
        public Action<int> Fill;public Func<bool> CanInteract;
        private Texture2D lineArt,mask,palette;
        private byte[] regions;
        private int width,height,selected=-1;
        private long revision=-1;
        private Material ink;
        public float Aspect=>width/(float)Math.Max(1,height);
        public void Present(int page,ColoringPage state,long? generation=null)
        {
            if(selected!=page){
                Release();selected=page;var entry=Pages.pages[page-Discovery.LegacyPages];
                var path="Worlds/Home/Discovery/Coloring/"+entry.id;
                // A bedroom frame and its open folder may show the same page.
                // Unload only after the last active view releases this asset.
                if(!activeArt.TryGetValue(page,out var art)){art=new SharedArt{texture=WorldResources.Load<Texture2D>(path)};activeArt.Add(page,art);}
                art.users++;lineArt=art.texture;
                var file=WorldResources.Load<TextAsset>(path);regions=file.bytes;Resources.UnloadAsset(file);
                width=BitConverter.ToUInt16(regions,0);height=BitConverter.ToUInt16(regions,2);
                if(width!=lineArt.width || height!=lineArt.height || regions.Length!=4+width*height || state.colors.Length!=entry.regions)throw new InvalidOperationException("Coloring resource contract mismatch.");
                mask=new Texture2D(width,height,TextureFormat.Alpha8,false,true){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
                var pixels=new byte[width*height];for(var y=0;y<height;y++)Array.Copy(regions,4+y*width,pixels,(height-1-y)*width,width);
                mask.LoadRawTextureData(pixels);mask.Apply(false,true);
                palette=new Texture2D(256,1,TextureFormat.RGBA32,false,true){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
                ink=new Material(WorldResources.Load<Shader>("Worlds/Home/Discovery/ColoringInk"));ink.SetTexture("_Regions",mask);ink.SetTexture("_Palette",palette);material=ink;texture=lineArt;
            }
            var next=generation??state.revision;if(revision==next)return;revision=next;
            var colors=new Color32[256];for(var i=0;i<colors.Length;i++)colors[i]=Color.white;
            for(var i=0;i<state.colors.Length;i++)colors[i+1]=DiscoverySurface.Palette[state.colors[i]];
            palette.SetPixels32(colors);palette.Apply(false,false);SetMaterialDirty();
        }
        public void OnPointerClick(PointerEventData e)
        {
            if(regions==null || !(CanInteract?.Invoke()??true))return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var p);var r=rectTransform.rect;
            var x=Mathf.Clamp((int)((p.x-r.xMin)/r.width*width),0,width-1);var y=Mathf.Clamp((int)((r.yMax-p.y)/r.height*height),0,height-1);
            var id=regions[4+y*width+x];if(id>0)Fill?.Invoke(id-1);
        }
        private void Release()
        {
            texture=null;material=null;regions=null;
            if(lineArt!=null && activeArt.TryGetValue(selected,out var art) && --art.users==0){Resources.UnloadAsset(lineArt);activeArt.Remove(selected);}
            if(mask!=null)Destroy(mask);if(palette!=null)Destroy(palette);if(ink!=null)Destroy(ink);
            lineArt=null;mask=null;palette=null;ink=null;revision=-1;selected=-1;
        }
        protected override void OnDisable(){Release();base.OnDisable();}
        protected override void OnDestroy(){Release();base.OnDestroy();}
    }
}
