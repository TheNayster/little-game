using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        private RectTransform bookOptions;
        private Text bookWordsLabel,bookReplay,bookSound;
        private Button bookPrevious,bookNext;
        private ReaderPictureControl bookPlayPicture,bookHeadingCard;
        private bool bookWordsOn=true;
        public bool BookAutoTurn=>reader.AutoTurn;
        public bool BookWordsVisible=>bookWordsOn;
        public bool BookOptionsOpen=>bookOptions!=null && bookOptions.gameObject.activeSelf;
        public bool BookEffectPending=>bookEffectPending;

        private ReaderPictureControl ReaderDrawing(Transform parent,string name,Vector2 position,Vector2 size,string icon,Color color)
        {var graphic=Rect(parent,name,position,size).gameObject.AddComponent<ReaderPictureControl>();graphic.Icon=icon;graphic.color=color;graphic.raycastTarget=false;return graphic;}
        private Text BookControl(string name,string label,string icon,float x,float y,float width,UnityEngine.Events.UnityAction action,bool tall=false,Transform parent=null)
        {
            var size=new Vector2(width,tall?100:76);var text=Button(parent??readerFrame,label,Vector2.zero,size,action,Color.clear);
            var root=text.transform.parent.GetComponent<RectTransform>();root.name=name;
            var card=ReaderDrawing(root,"Translucent card",Vector2.zero,size,"card",new Color(1,.985f,.94f,.72f));card.transform.SetAsFirstSibling();root.GetComponent<Button>().targetGraphic=card;
            var picture=ReaderDrawing(root,"Control picture",new Vector2(tall?0:-width/2+34,tall?17:0),Vector2.one*(tall?44:42),icon,Color.white);
            text.color=new Color(.17f,.34f,.33f);text.fontSize=tall?25:23;text.fontStyle=FontStyle.Bold;
            text.rectTransform.anchoredPosition=tall?new Vector2(0,-28):new Vector2(23,0);
            text.rectTransform.sizeDelta=tall?new Vector2(width-16,38):new Vector2(width-68,64);
            text.resizeTextForBestFit=true;text.resizeTextMinSize=19;text.resizeTextMaxSize=text.fontSize;
            if(parent==null)bookControlAnchors.Add((root,x,y));else root.anchoredPosition=new Vector2(x,y);
            if(name=="Read to me"){bookPlayPicture=picture;card.color=new Color(.81f,.94f,.83f,.84f);}
            return text;
        }
        private void BuildBookControls()
        {
            BookControl("Our books","Books","books",.085f,347,154,()=>{CloseBook();ShowBookLibrary();});
            BookControl("More reading options","More","more",.78f,347,144,()=>bookOptions.gameObject.SetActive(!bookOptions.gameObject.activeSelf));
            BookControl("Close","Home","home",.93f,347,136,CloseBook);
            bookPrevious=BookControl("<","Back","back",.047f,20,80,()=>TurnBook(reader.Page-1),true).transform.parent.GetComponent<Button>();
            bookNext=BookControl(">","Next","next",.953f,20,80,()=>TurnBook(reader.Page+1),true).transform.parent.GetComponent<Button>();
            bookPlay=BookControl("Read to me","Read to me","play",.28f,-319,216,ToggleBookPlay,true);
            bookReplay=BookControl("Replay","Read again","replay",.5f,-319,216,()=>{PauseBook();reader.Sample=0;ToggleBookPlay();},true);
            bookSound=BookControl("Hear sound","Hear sound","sound",.72f,-319,216,PlayBookEffect,true);
            ReaderDrawing(readerFrame,"Page number card",new Vector2(0,-385),new Vector2(92,28),"card",new Color(1,.985f,.94f,.78f));
            bookCounter=BookText("",20,new Vector2(0,-385),new Vector2(160,28));bookCounter.color=new Color(.17f,.34f,.33f);Destroy(bookCounter.GetComponent<Shadow>());
            bookOptions=Panel(readerFrame,"Reading options",new Vector2(0,72),new Vector2(620,438),new Color(1,.985f,.94f,.98f)).rectTransform;
            bookOptions.GetComponent<Image>().raycastTarget=true;
            Label(bookOptions,"More reading options",30,new Vector2(0,171),new Vector2(580,55));
            bookVoiceLabel=BookControl("Voice on","Voice on","voice",-149,79,278,()=>{PauseBook();ToggleVoice();UpdateBookControls();},parent:bookOptions);
            bookEffectsLabel=BookControl("Sounds on","Sounds on","sound",149,79,278,()=>{var resume=bookEffectPending || bookWasEffect;StopBookEffect();bookEffectsOn=!bookEffectsOn;PlayerPrefs.SetInt(QuietKey("book-effects"),bookEffectsOn?1:0);PlayerPrefs.Save();if(resume && reader.Playing)StartBookSpeech();UpdateBookControls();},parent:bookOptions);
            bookWordsLabel=BookControl("Show words","Words on","books",-149,-14,278,()=>{bookWordsOn=!bookWordsOn;PlayerPrefs.SetInt(QuietKey("book-words"),bookWordsOn?1:0);PlayerPrefs.Save();UpdateBookControls();},parent:bookOptions);
            bookAuto=BookControl("Auto pages on","Auto pages off","books",149,-14,278,()=>{reader.AutoTurn=!reader.AutoTurn;if(!reader.AutoTurn && bookAutoAt>=0)PauseBook();SaveBookmark();UpdateBookControls();},parent:bookOptions);
            BookControl("Reading options done","Done","back",0,-157,204,()=>bookOptions.gameObject.SetActive(false),parent:bookOptions);
            Label(bookOptions,"Open quietly. Tap Read to me to listen.",20,new Vector2(0,-94),new Vector2(580,32));bookOptions.gameObject.SetActive(false);
        }
    }
}
