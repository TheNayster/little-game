using System;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.EditorTools
{
    // Measure every actual paragraph with the same Unity font at the narrowest
    // supported landscape reader width, before shipping a clipped text panel.
    public sealed class BookContentBuildCheck : IPreprocessBuildWithReport
    {
        [Serializable] private sealed class Page {public string speech;public int species;}
        [Serializable] private sealed class Book {public string kind;public Page[] pages;}
        public int callbackOrder=>0;
        public void OnPreprocessBuild(BuildReport report)
        {
            var host=new GameObject("Book text qualification",typeof(RectTransform));
            try
            {
                var label=host.AddComponent<Text>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=24;
                label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Overflow;
                var count=0;var maximum=0f;
                foreach(var title in Core.HomeBooks.Titles)
                {
                    var path="Assets/FamilyPlayset/Resources/Worlds/Home/Books/"+title+"/content.json";
                    var book=JsonUtility.FromJson<Book>(File.ReadAllText(path));
                    foreach(var page in book.pages)
                    {
                        var settings=label.GetGenerationSettings(new Vector2(800f*4/3-216,1000));
                        var height=label.cachedTextGeneratorForLayout.GetPreferredHeight(page.speech,settings)/label.pixelsPerUnit;
                        var limit=book.kind=="dinosaurs" && page.species<0?112:156;
                        if(height>limit)throw new BuildFailedException(title+" has overflowing story words: "+height+" > "+limit);
                        maximum=Mathf.Max(maximum,height);count++;
                    }
                }
                Debug.Log("BOOK TEXT CHECK: "+count+" pages fit a 4:3 landscape reader at 24pt; maximum paragraph height "+maximum);
            }
            finally {UnityEngine.Object.DestroyImmediate(host);}
        }
    }
}
