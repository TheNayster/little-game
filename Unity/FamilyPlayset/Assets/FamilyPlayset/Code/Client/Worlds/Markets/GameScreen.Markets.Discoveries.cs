using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform marketsDiscoveryCard;
        private RawImage marketsDiscoverySign,marketsDiscoveryDog,marketsDiscoveryChoice;
        private MarketsParasolPicture marketsDiscoveryParasol;
        private Text marketsDiscoveryTitle,marketsDiscoveryCaption;
        private string marketsDiscoveryKind="";
        private int marketsDiscoverySignSeen=-1,marketsDiscoveryUmbrellaSeen=-1,marketsDiscoveryDogSeen=-1;
        private float marketsDiscoveryAt;
        public string MarketsDiscoveryKind=>marketsDiscoveryCard&&marketsDiscoveryCard.gameObject.activeSelf?marketsDiscoveryKind:"";
        private void BuildMarketsDiscoveries()
        {
            var card=Panel(safe,"Markets pictured discoveries",new Vector2(360,45),new Vector2(270,230),new Color(1,.99f,.91f,.98f),false,false);
            marketsDiscoveryCard=card.rectTransform;
            marketsDiscoveryTitle=Label(card.transform,"",21,new Vector2(0,86),new Vector2(248,32));
            marketsDiscoveryCaption=Label(card.transform,"",17,new Vector2(0,-82),new Vector2(248,52));
            marketsDiscoverySign=MarketsPicture(card.transform,"Recovered Markets sign","Discoveries/sign",new Vector2(0,0),new Vector2(145,145),true);
            marketsDiscoveryChoice=MarketsPicture(marketsDiscoverySign.transform,"Shared sign destination","cup",new Vector2(0,14),Vector2.one*70);
            NavButton(marketsDiscoverySign,()=>{if(Markets!=null)MarketsGo(new[]{"smoothie","flowers","bubbles"}[Markets.sign]);});
            marketsDiscoveryDog=MarketsPicture(card.transform,"Recovered hidden Long dog","Discoveries/tex_emb_long_dog",Vector2.zero,new Vector2(250,160));
            marketsDiscoveryParasol=Rect(card.transform,"Recovered yellow parasol",Vector2.zero,new Vector2(240,156)).gameObject.AddComponent<MarketsParasolPicture>();
            marketsDiscoveryParasol.Source=MarketsTexture("flowers");marketsDiscoveryParasol.raycastTarget=false;
            var close=Button(card.transform,"×",new Vector2(112,91),new Vector2(35,32),()=>marketsDiscoveryCard.gameObject.SetActive(false),new Color(.93f,.95f,1));
            close.transform.parent.name="Markets close discovery";
            marketsDiscoveryCard.gameObject.SetActive(false);
        }
        private void ResetMarketsDiscoveries()
        {
            marketsDiscoveryCard=null;marketsDiscoverySign=null;marketsDiscoveryDog=null;marketsDiscoveryChoice=null;marketsDiscoveryParasol=null;marketsDiscoveryTitle=null;marketsDiscoveryCaption=null;
            marketsDiscoveryKind="";marketsDiscoverySignSeen=marketsDiscoveryUmbrellaSeen=marketsDiscoveryDogSeen=-1;marketsDiscoveryAt=0;
        }
        private void MarketsShowDiscovery(string kind)
        {
            marketsDiscoveryKind=kind;marketsDiscoveryAt=Time.realtimeSinceStartup;marketsDiscoveryCard.gameObject.SetActive(true);
            // This is local presentation of an authoritative shared change. It
            // does not invent original-app animation or independent progress.
            string sound=kind=="sign"?"sfx_mkd_market_tappable_sign_update":kind=="umbrella"?"sfx_mkd_parasol_ontap_01":"sfx_mkd_treasurehunt_flag_spin_01";
            var clip=Resources.Load<AudioClip>("Worlds/Markets/Audio/"+sound);if(clip&&marketsEffect)marketsEffect.PlayOneShot(clip,.55f);
        }
        private void TickMarketsDiscoveries(MarketsState h,bool visible)
        {
            if(!marketsDiscoveryCard)return;
            if(!visible||h==null){marketsDiscoveryCard.gameObject.SetActive(false);return;}
            // First arrival establishes a baseline, avoiding replay of every
            // old discovery after reconnect. Subsequent shared changes reveal.
            if(marketsDiscoverySignSeen<0){marketsDiscoverySignSeen=h.sign;marketsDiscoveryUmbrellaSeen=h.umbrella;marketsDiscoveryDogSeen=h.longDog;return;}
            if(marketsDiscoverySignSeen!=h.sign){marketsDiscoverySignSeen=h.sign;MarketsShowDiscovery("sign");}
            if(marketsDiscoveryUmbrellaSeen!=h.umbrella){marketsDiscoveryUmbrellaSeen=h.umbrella;MarketsShowDiscovery("umbrella");}
            if(marketsDiscoveryDogSeen!=h.longDog){marketsDiscoveryDogSeen=h.longDog;MarketsShowDiscovery("longdog");}
            if(!marketsDiscoveryCard.gameObject.activeSelf)return;
            float age=Time.realtimeSinceStartup-marketsDiscoveryAt;
            if(age>8){marketsDiscoveryCard.gameObject.SetActive(false);return;}
            marketsDiscoveryCard.localScale=Vector3.one*Mathf.Lerp(.85f,1,Mathf.Clamp01(age*5));
            marketsDiscoverySign.gameObject.SetActive(marketsDiscoveryKind=="sign");marketsDiscoveryDog.gameObject.SetActive(marketsDiscoveryKind=="longdog");marketsDiscoveryParasol.gameObject.SetActive(marketsDiscoveryKind=="umbrella");
            if(marketsDiscoveryKind=="sign"){
                string[] places={"Smoothies","Flowers","Bubbles"};string[] icons={"cup","pfb_mkd_Flower_Pink","pfb_mkd_BubbleWand"};
                marketsDiscoveryTitle.text="The market sign";marketsDiscoveryChoice.texture=MarketsTexture(icons[h.sign]);
                marketsDiscoverySign.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(age*9)*8*Mathf.Exp(-age*2));
                marketsDiscoveryCaption.text=places[h.sign]+" — tap the sign to visit together";
            }else if(marketsDiscoveryKind=="umbrella"){
                marketsDiscoveryTitle.text="A sunny parasol";marketsDiscoveryCaption.text="Tap Umbrella again to sway the shade";
                marketsDiscoveryParasol.rectTransform.localRotation=Quaternion.Euler(0,0,(h.umbrella==1?7:h.umbrella==2?-7:0)+Mathf.Sin(age*8)*13*Mathf.Exp(-age*1.2f));
            }else{
                marketsDiscoveryTitle.text="You found Long dog!";marketsDiscoveryCaption.text="Found together: "+h.longDog;
                string[] colors={"","_blue","_brown","_green","_purple","_yellow"};marketsDiscoveryDog.texture=MarketsTexture("Discoveries/tex_emb_long_dog"+colors[(h.longDog-1)%colors.Length]);
                marketsDiscoveryDog.rectTransform.anchoredPosition=new Vector2(Mathf.Sin(age*2)*10,Mathf.Sin(age*8)*5*Mathf.Exp(-age*.8f));
                marketsDiscoveryDog.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(age*5)*3);
            }
        }
    }

    // Code-native silhouette uses the existing recovered flower-stall pixels.
    // No rectangular background patch, fabricated replacement parasol or new
    // raster generation: both canopy and pole retain their actual source art.
    public sealed class MarketsParasolPicture : MaskableGraphic
    {
        private Texture2D source;
        public Texture2D Source{get=>source;set{source=value;SetMaterialDirty();SetVerticesDirty();}}
        public override Texture mainTexture=>source?source:Texture2D.whiteTexture;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(!source)return;
            Vector2[] edge={new Vector2(422,280),new Vector2(535,246),new Vector2(535,231),new Vector2(553,231),new Vector2(553,222),new Vector2(595,222),new Vector2(595,246),new Vector2(617,246),new Vector2(617,222),new Vector2(658,222),new Vector2(658,245),new Vector2(791,279),new Vector2(791,300),new Vector2(780,319),new Vector2(749,335),new Vector2(713,339),new Vector2(695,326),new Vector2(691,340),new Vector2(540,340),new Vector2(525,324),new Vector2(518,337),new Vector2(483,336),new Vector2(451,326),new Vector2(434,314),new Vector2(423,300)};
            void Vertex(Vector2 p){var rect=rectTransform.rect;vh.AddVert(new Vector3((p.x-607)/400*rect.width,(340-p.y)/260*rect.height,0),color,new Vector2(p.x/1600,1-p.y/1000));}
            Vertex(new Vector2(607,290));foreach(var p in edge)Vertex(p);
            for(int i=0;i<edge.Length;i++)vh.AddTriangle(0,i+1,(i+1)%edge.Length+1);
            int start=vh.currentVertCount;Vertex(new Vector2(588,337));Vertex(new Vector2(632,337));Vertex(new Vector2(632,453));Vertex(new Vector2(588,453));vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
        }
    }
}
