using System;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform marketsPreparation;
        private readonly List<RawImage> marketsPreparationPieces=new List<RawImage>();
        private Text marketsPreparationLabel;
        private void BuildMarketsPreparation()
        {
            marketsPreparation=Rect(Board,"Markets shared work",new Vector2(-180,40),new Vector2(280,220));
            var card=Panel(marketsPreparation,"Shared work card",Vector2.zero,new Vector2(280,200),new Color(1,.99f,.92f,.10f),false,false);
            marketsPreparationLabel=Label(card.transform,"",18,new Vector2(0,80),new Vector2(270,32));
            for(int i=0;i<8;i++)marketsPreparationPieces.Add(MarketsPicture(card.transform,"Shared piece "+i,"",new Vector2((i%4-1.5f)*61,36-i/4*67),Vector2.one*60));
        }
        private void ResetMarketsPreparation(){marketsPreparation=null;marketsPreparationPieces.Clear();marketsPreparationLabel=null;}
        private void TickMarketsPreparation(MarketsState h,string current,bool visible)
        {
            if(!marketsPreparation)return;
            bool shown=visible&&h!=null&&(current=="smoothie"||current=="flowers"||current=="icecream");
            var station=shown?h.stations.Single(t=>t.id==current):null;
            shown&=station!=null&&station.parts.Length>0;
            marketsPreparation.gameObject.SetActive(shown);if(!shown)return;
            marketsPreparation.anchoredPosition=new Vector2(current=="icecream"?170:-65,current=="icecream"?-25:30);
            marketsPreparationLabel.text=current=="icecream"?Upper(MarketsRules.IceCreamNames[station.choice])+" • "+station.parts.Length+"/"+MarketsRules.Recipes[station.choice].Length:current=="flowers"?"Our flowers":"Our smoothie";
            for(int i=0;i<marketsPreparationPieces.Count;i++){
                var image=marketsPreparationPieces[i];bool has=i<station.parts.Length;image.gameObject.SetActive(has);if(!has)continue;
                string part=station.parts[i];string key=current=="smoothie"?"pfb_mkd_FoodItem_"+Upper(part):current=="flowers"?"pfb_mkd_Flower_"+Upper(part):"pfb_mkd_byo_"+part;
                image.texture=MarketsTexture(key);image.rectTransform.sizeDelta=Vector2.one*(current=="icecream"?64:55);image.rectTransform.anchoredPosition=current=="icecream"?new Vector2(0,-115+i*37):new Vector2((i%4-1.5f)*61,36-i/4*67);image.rectTransform.localRotation=Quaternion.Euler(0,0,station.busyUntil>h.clock?Mathf.Sin((float)h.clock*9+i)*5:0);
            }
        }
    }
}
