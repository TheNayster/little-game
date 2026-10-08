using System.IO;
using LittleWeeps.Core;
using UnityEditor;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public static class ZooNavigationArt
    {
        // Small stills reuse the retained editable atlas sources. Navigation
        // must not keep all sixteen full animation atlases resident on a phone.
        public static void Prepare()
        {
            const string folder="Assets/FamilyPlayset/Resources/Worlds/Zoo/Navigation";
            Directory.CreateDirectory(folder);
            foreach(var species in ZooCatalog.All){
                var path=folder+"/"+species.id+".png";if(File.Exists(path))continue;
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/FamilyPlayset/Resources/Worlds/Zoo/Art/"+species.id+".png");
                if(texture==null)throw new System.InvalidOperationException("Missing Zoo atlas "+species.id);
                var target=RenderTexture.GetTemporary(160,160,0,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;
                var portrait=new Texture2D(160,160,TextureFormat.RGBA32,false);
                try{
                    Graphics.Blit(texture,target,new Vector2(.245f,.5f),new Vector2(.0025f,0));RenderTexture.active=target;
                    portrait.ReadPixels(new Rect(0,0,160,160),0,0);portrait.Apply();File.WriteAllBytes(path,portrait.EncodeToPNG());
                }finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Object.DestroyImmediate(portrait);}
            }
            AssetDatabase.Refresh();
        }
    }
}
