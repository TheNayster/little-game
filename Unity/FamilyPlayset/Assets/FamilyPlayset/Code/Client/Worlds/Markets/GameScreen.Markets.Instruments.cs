using UnityEngine;
namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        // Small readable control pictures; recovered music remains the soundtrack.
        private void MarketsInstrumentPicture(Transform parent,string instrument)
        {
            var root=Rect(parent,"Picture "+instrument,new Vector2(0,12),new Vector2(62,56));
            if(instrument=="keys"){
                for(int i=0;i<5;i++)Panel(root,"White key",new Vector2(-24+i*12,0),new Vector2(11,40),Color.white,false,false);
                for(int i=0;i<4;i++)if(i!=2)Panel(root,"Black key",new Vector2(-18+i*12,9),new Vector2(6,22),new Color(.16f,.2f,.3f),false,false);
            }else if(instrument=="drum"){
                Panel(root,"Drum shell",Vector2.zero,new Vector2(46,30),new Color(.87f,.38f,.27f),false,false);
                var top=Panel(root,"Drum skin",new Vector2(0,15),new Vector2(50,13),new Color(1,.94f,.76f),false,false);top.sprite=circle;
                for(int i=0;i<2;i++){var stick=Panel(root,"Drum stick",new Vector2(i==0?-12:12,22),new Vector2(3,35),new Color(.53f,.33f,.16f),false,false);stick.rectTransform.localRotation=Quaternion.Euler(0,0,i==0?40:-40);}
            }else{
                var body=Panel(root,"Guitar body",new Vector2(-5,-8),new Vector2(34,35),new Color(.94f,.61f,.18f),false,false);body.sprite=circle;
                var neck=Panel(root,"Guitar neck",new Vector2(8,15),new Vector2(9,42),new Color(.55f,.32f,.15f),false,false);neck.rectTransform.localRotation=Quaternion.Euler(0,0,-28);
                var hole=Panel(root,"Sound hole",new Vector2(-2,-3),Vector2.one*11,new Color(.3f,.2f,.13f),false,false);hole.sprite=circle;
            }
        }
    }
}
