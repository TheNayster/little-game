using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class SoloScreen
    {
        [Serializable] private sealed class BookPage {public string title,caption,speech;public int species;}
        [Serializable] private sealed class BookArtRect {public float x,y,width,height;}
        [Serializable] private sealed class BookContent {public string id,title,locale,kind;public int revision;public BookPage[] pages;public string[] names;public BookArtRect[] artRects;}
        // Pending requests own references: stale completions cannot unload a
        // resource that a newer request for the same page is still using.
        private sealed class BookLease {public string key;public int users;public ResourceRequest request;public UnityEngine.Object asset;}
        private readonly Dictionary<string,BookLease> bookMedia=new Dictionary<string,BookLease>();
        private readonly BookCursor reader=new BookCursor();
        private BookContent bookContent;
        private int bookArtGeneration,bookPageGeneration,bookNameGeneration,bookEffectGeneration,bookName=-1,bookSelected;
        private RectTransform readerOverlay,readerFrame,bookLibrary,bookLibraryFrame,bookRack,bookRackFront;
        private Text bookTitle,bookCaption,bookCounter,bookPlay,bookAuto,bookStatus,bookVoiceLabel,bookEffectsLabel;
        private Image bookFocus,bookWordsPanel;
        private readonly List<(RectTransform rect,float x,float y)> bookControlAnchors=new List<(RectTransform,float,float)>();
        private readonly List<Image> bookChoices=new List<Image>();
        private readonly List<Text> bookChoiceLabels=new List<Text>();
        private readonly List<Sprite> bookPageSprites=new List<Sprite>(),bookCovers=new List<Sprite>();
        private readonly List<BookLease> bookArt=new List<BookLease>();
        private Sprite bookRackSprite;
        private AudioSource readerVoice,readerEffects;
        private BookLease bookPageLease,bookNameLease,bookEffectLease;
        private AudioClip pageAudio,nameAudio,effectAudio;
        private bool bookPagePending,bookNamePending,bookWasSpeaking,bookAwaitingSpeech,bookEffectsOn=true;
        private float bookSpeechDeadline;
        private float bookAutoAt=-1,bookNextSave,bookMotionUntil;
        private string readAfterDrop;
        public bool BookOpen=>reader.Open;
        public bool BookLibraryOpen=>bookLibrary!=null && bookLibrary.gameObject.activeSelf;
        public string BookTitleId=>bookContent?.id??"";
        public int BookPageNumber=>reader.Page;
        public int BookSample=>reader.Sample;
        public bool BookPlaying=>reader.Playing;
        public bool BookSpeaking=>BookOpen && readerVoice!=null && readerVoice.isPlaying;
        public bool BookEffectPlaying=>BookOpen && readerEffects!=null && readerEffects.isPlaying;
        public bool BookNaming=>bookName>=0 || bookNamePending;
        public int BookResidentTextures=>bookArt.Count(l=>l.asset!=null);
        public int BookResidentAudio=>(pageAudio==null?0:1)+(nameAudio==null?0:1)+(effectAudio==null?0:1);
        public bool BookPageReady=>BookOpen && bookPageSprites.Count==(DinosaurBook?12:8) && pageAudio!=null && !bookPagePending;
        public bool IsBook(string role)=>HomeBooks.Index(role)>=0 && HasWorld && SceneSchema>=HomeBooks.Schema;
        private bool DinosaurBook=>bookContent?.kind=="dinosaurs";
        private float BookDucking=>(BookSpeaking || BookEffectPlaying) ? .22f : 1f;
        private string BookKey=>QuietKey("bookmarks")+"."+bookContent.id+".";
        private string BookPath=>"Books/"+bookContent.id+"/";
        private static Sprite BookCell(Texture2D t,int i,int cols,int rows)=>Sprite.Create(t,new Rect(i%cols*t.width/(float)cols,(rows-1-i/cols)*t.height/(float)rows,t.width/(float)cols,t.height/(float)rows),new Vector2(.5f,.5f));
        private Sprite BookPropSprite(string name)
        {
            if(name=="rack" && bookRackSprite!=null)return bookRackSprite;
            var t=Resources.Load<Texture2D>("Books/"+name);if(t==null)throw new InvalidOperationException("Missing book prop: "+name);homeTextures.Add(t);
            if(name=="covers"){for(var i=0;i<6;i++){var s=BookCell(t,i,3,2);homeSprites.Add(s);bookCovers.Add(s);}return bookCovers[0];}
            bookRackSprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f));homeSprites.Add(bookRackSprite);return bookRackSprite;
        }
        private Sprite BookCoverSprite(int i){if(bookCovers.Count==0)BookPropSprite("covers");return bookCovers[Mathf.Clamp(i,0,HomeBooks.Titles.Length-1)];}
        private static HomeArtPart.Polygon BookRail(float left,float top,float right,float bottom)=>new HomeArtPart.Polygon{points=new[]{new Vector2(left,top),new Vector2(right,top),new Vector2(right,bottom),new Vector2(left,bottom)}};
        private void ReadBookContent(int index)
        {
            var data=Resources.Load<TextAsset>("Books/"+HomeBooks.Titles[index]+"/content");
            if(data==null)throw new InvalidOperationException("Missing book text");
            bookContent=JsonUtility.FromJson<BookContent>(data.text);Resources.UnloadAsset(data);
        }
        private void BuildBooks()
        {
            if(SceneSchema<HomeBooks.Schema)return;if(bookContent==null)ReadBookContent(0);
            bookRack=Rect(Board,"Shared living-room book rack",Vector2.zero,Vector2.zero);
            HomePicture(bookRack,"Empty book rack",new Vector2(0,155),new Vector2(500,333),BookPropSprite("rack"));
            bookRackFront=Rect(Board,"Book rack lips",Vector2.zero,Vector2.zero);
            var lip=Rect(bookRackFront,"Real book support front",new Vector2(0,155),new Vector2(500,333)).gameObject.AddComponent<HomeArtPart>();
            lip.Configure(bookRackSprite.texture,new[]{BookRail(.055f,.615f,.945f,.83f),BookRail(.07f,.353f,.93f,.418f)});
            HomeHit(bookRack,"Read shared books",new Vector2(0,70),new Vector2(400,55),ShowBookLibrary);
            Label(bookRack,"Our books",26,new Vector2(0,70),new Vector2(370,40)).color=Ink;
            bookLibrary=Plain(safe,"Our book collection",Vector2.zero,Vector2.zero,new Color(.95f,.97f,.91f),true).rectTransform;Stretch(bookLibrary);
            bookLibraryFrame=Rect(bookLibrary,"Book collection",Vector2.zero,new Vector2(1200,800));
            Label(bookLibraryFrame,"Our story corner",42,new Vector2(0,337),new Vector2(850,75));
            Label(bookLibraryFrame,"Choose a book. Everyone can read together.",24,new Vector2(0,279),new Vector2(950,46));
            for(var i=0;i<HomeBooks.Titles.Length;i++)
            {
                var index=i;var pos=new Vector2((i%3-1)*350,i<3?126:-160);
                var cover=HomePicture(bookLibraryFrame,"Choose "+HomeBooks.Titles[i],pos,new Vector2(320,280),BookCoverSprite(i));cover.preserveAspect=true;cover.raycastTarget=true;
                NavButton(cover,()=>{bookLibrary.gameObject.SetActive(false);OpenBook(HomeBooks.Copy(index));});
            }
            Button(bookLibraryFrame,"Close library",new Vector2(0,-349),new Vector2(215,62),()=>{bookLibrary.gameObject.SetActive(false);stick.gameObject.SetActive(JoystickMode && !MenuOpen);},Cream);
            bookLibrary.gameObject.SetActive(false);
            readerOverlay=Plain(safe,"Book reader",Vector2.zero,Vector2.zero,new Color(.96f,.96f,.90f),true).rectTransform;Stretch(readerOverlay);
            readerFrame=Rect(readerOverlay,"Landscape picture book",Vector2.zero,new Vector2(1200,800));
            bookFocus=HomePicture(readerFrame,"Full page illustration",Vector2.zero,new Vector2(1200,800),null);bookFocus.preserveAspect=true;bookFocus.raycastTarget=true;
            NavButton(bookFocus,()=>{if(DinosaurBook)BookName(bookContent.pages[reader.Page].species);else PlayBookEffect();});
            for(var i=0;i<12;i++)
            {
                var index=i;
                var choice=HomePicture(readerFrame,"Animal choice "+i,Vector2.zero,new Vector2(250,170),null);choice.preserveAspect=true;choice.raycastTarget=true;NavButton(choice,()=>BookName(index));bookChoices.Add(choice);
                bookChoiceLabels.Add(BookText("",24,Vector2.zero,new Vector2(260,36)));
            }
            bookTitle=BookText("",36,new Vector2(0,303),new Vector2(1000,64));
            bookWordsPanel=Panel(readerFrame,"Story words on page",new Vector2(0,-224),new Vector2(1040,172),new Color(.02f,.04f,.06f,.27f));
            bookCaption=BookText("",28,new Vector2(0,-224),new Vector2(1008,156));
            bookCaption.resizeTextForBestFit=true;bookCaption.resizeTextMinSize=24;bookCaption.resizeTextMaxSize=28;
            bookCaption.horizontalOverflow=HorizontalWrapMode.Wrap;bookCaption.verticalOverflow=VerticalWrapMode.Truncate;
            bookStatus=BookText("",18,new Vector2(0,-310),new Vector2(1000,28));
            BookControl("Close",.94f,352,110,CloseBook);
            BookControl("Our books",.78f,352,150,()=>{CloseBook();ShowBookLibrary();});
            bookVoiceLabel=BookControl("Voice on",.075f,352,140,()=>{PauseBook();ToggleVoice();UpdateBookControls();});
            bookEffectsLabel=BookControl("Sounds on",.23f,352,150,()=>{bookEffectsOn=!bookEffectsOn;StopBookEffect();PlayerPrefs.SetInt(QuietKey("book-effects"),bookEffectsOn?1:0);PlayerPrefs.Save();UpdateBookControls();});
            bookCounter=BookText("",22,new Vector2(0,352),new Vector2(140,42));
            BookControl("<",.045f,0,66,()=>TurnBook(reader.Page-1));
            BookControl(">",.955f,0,66,()=>TurnBook(reader.Page+1));
            bookPlay=BookControl("Read to me",.26f,-354,170,ToggleBookPlay);
            BookControl("Replay",.42f,-354,150,()=>{PauseBook();reader.Sample=0;ToggleBookPlay();});
            BookControl("Hear sound",.58f,-354,150,PlayBookEffect);
            bookAuto=BookControl("Auto pages on",.74f,-354,185,()=>{reader.AutoTurn=!reader.AutoTurn;if(!reader.AutoTurn)bookAutoAt=-1;SaveBookmark();UpdateBookControls();});
            readerVoice=gameObject.AddComponent<AudioSource>();readerEffects=gameObject.AddComponent<AudioSource>();
            foreach(var source in new[]{readerVoice,readerEffects}){source.playOnAwake=false;source.spatialBlend=0;}
            var muted=VerifyRun!=null || familyTestMuted || shared?.MutedTest==true;readerVoice.volume=muted?0:.65f;readerEffects.volume=muted?0:.45f;
            bookEffectsOn=PlayerPrefs.GetInt(QuietKey("book-effects"),1)!=0;readerOverlay.gameObject.SetActive(reader.Open);
            if(reader.Open){reader.Pause();ShowBookPage();StartBookArt();}Application.lowMemory+=BookLowMemory;
        }
        private Text BookText(string text,int size,Vector2 position,Vector2 dimensions)
        {
            var label=Label(readerFrame,text,size,position,dimensions);label.color=Color.white;
            var shadow=label.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.9f);shadow.effectDistance=new Vector2(1.5f,-1.5f);return label;
        }
        private Text BookControl(string text,float x,float y,float width,UnityEngine.Events.UnityAction action)
        {
            var label=Button(readerFrame,text,Vector2.zero,new Vector2(width,64),action,new Color(.04f,.07f,.10f,.30f));label.color=Color.white;
            var shadow=label.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.7f);shadow.effectDistance=new Vector2(1,-1);
            bookControlAnchors.Add((label.transform.parent.GetComponent<RectTransform>(),x,y));return label;
        }
        private void LayoutBookReader()
        {
            // Fit the entire page to the actual screen, with controls floating
            // over it. Never crop a picture to fill a different device aspect.
            var scale=safe.rect.height/800;var width=safe.rect.width/scale;
            readerFrame.localScale=Vector3.one*scale;readerFrame.sizeDelta=new Vector2(width,800);
            bookFocus.rectTransform.sizeDelta=DinosaurBook?new Vector2(width-180,450):new Vector2(width,800);
            foreach(var control in bookControlAnchors)control.rect.anchoredPosition=new Vector2((control.x-.5f)*width,control.y);
            var group=DinosaurBook && bookContent.pages[reader.Page].species<0;
            bookWordsPanel.rectTransform.sizeDelta=new Vector2(width-180,group?128:172);bookCaption.rectTransform.sizeDelta=new Vector2(width-216,group?112:156);
            bookWordsPanel.rectTransform.anchoredPosition=bookCaption.rectTransform.anchoredPosition=new Vector2(0,group?-245:-224);bookTitle.rectTransform.sizeDelta=new Vector2(width-240,64);
            var cell=(width-200)/4;
            for(var i=0;i<12;i++)
            {
                var pos=new Vector2((i%4-1.5f)*cell,220-i/4*140);
                bookChoices[i].rectTransform.anchoredPosition=pos;bookChoices[i].rectTransform.sizeDelta=new Vector2(cell-24,110);
                bookChoiceLabels[i].rectTransform.anchoredPosition=pos+new Vector2(0,-65);bookChoiceLabels[i].rectTransform.sizeDelta=new Vector2(cell,40);
            }
        }
        private void ShowBookLibrary()
        {
            if(!Ready || MenuOpen || WorldLoading || TravelPending)return;
            CancelPointers();CancelStairApproach();CancelDoorApproach();CloseNavigation();Narration.Stop();
            bookLibrary.gameObject.SetActive(true);bookLibrary.SetAsLastSibling();stick.gameObject.SetActive(false);
        }
        public void OpenBook(string copyId)
        {
            if(!Ready || !IsBook(copyId) || MenuOpen || WorldLoading || TravelPending)return;
            CancelPointers();CancelStairApproach();CancelDoorApproach();CloseNavigation();Narration.Stop();ReadBookContent(HomeBooks.Index(copyId));
            var key=BookKey;reader.Begin(PlayerPrefs.GetInt(key+"page",0),PlayerPrefs.GetInt(key+"sample",0),PlayerPrefs.GetInt(key+"revision",bookContent.revision),bookContent.pages.Length,bookContent.revision);reader.AutoTurn=PlayerPrefs.GetInt(key+"auto",1)!=0;
            readerOverlay.gameObject.SetActive(true);readerOverlay.SetAsLastSibling();stick.gameObject.SetActive(false);
            ShowBookPage();StartBookArt();SaveBookmark();
        }
        private BookLease AcquireBook(string key)
        {
            if(!bookMedia.TryGetValue(key,out var lease)){lease=new BookLease{key=key,request=Resources.LoadAsync<UnityEngine.Object>(key)};bookMedia[key]=lease;}
            lease.users++;return lease;
        }
        private void CompleteBookLoad(BookLease lease){if(lease.asset==null)lease.asset=lease.request.asset;if(lease.users==0)PurgeBookLoad(lease);}
        private void PurgeBookLoad(BookLease lease)
        {
            if(!bookMedia.TryGetValue(lease.key,out var active) || active!=lease)return;
            if(lease.asset!=null)Resources.UnloadAsset(lease.asset);lease.asset=null;bookMedia.Remove(lease.key);
        }
        private void ReleaseBook(ref BookLease lease)
        {if(lease==null)return;lease.users--;if(lease.users==0 && lease.request.isDone){lease.asset=lease.request.asset;PurgeBookLoad(lease);}lease=null;}
        private void StartBookArt()
        {
            var generation=++bookArtGeneration;
            for(var i=0;i<(DinosaurBook?2:1);i++){var lease=AcquireBook(BookPath+(DinosaurBook?(i==0?"dinosaurs":"dinosaurs-extra"):"pages"));bookArt.Add(lease);}
            StartCoroutine(LoadBookArt(generation,bookArt.ToArray(),DinosaurBook));
        }
        private IEnumerator LoadBookArt(int generation,BookLease[] leases,bool dinosaurs)
        {
            foreach(var lease in leases){yield return lease.request;CompleteBookLoad(lease);}
            if(!reader.Open || generation!=bookArtGeneration)yield break;
            foreach(var lease in leases)
            {
                var texture=lease.asset as Texture2D;if(texture==null){bookStatus.text="Pictures are unavailable. The words and controls still work.";yield break;}
                // Rectangles are authored from the actual sheet, whose story
                // panel boundaries need not lie on a perfectly uniform grid.
                for(var i=0;i<(dinosaurs?6:8);i++)
                {
                    var r=bookContent.artRects[bookPageSprites.Count];
                    bookPageSprites.Add(Sprite.Create(texture,new Rect(r.x*texture.width,(1-r.y-r.height)*texture.height,r.width*texture.width,r.height*texture.height),new Vector2(.5f,.5f)));
                }
            }
            ShowBookPictures();
        }
        private void CaptureBookSample()
        {if(bookName<0 && pageAudio!=null && readerVoice!=null && readerVoice.isPlaying && readerVoice.clip==pageAudio)reader.Sample=readerVoice.timeSamples;}
        private void SaveBookmark()
        {
            if(!reader.Open)return;CaptureBookSample();var key=BookKey;
            PlayerPrefs.SetInt(key+"revision",reader.ContentRevision);PlayerPrefs.SetInt(key+"page",reader.Page);PlayerPrefs.SetInt(key+"sample",Math.Max(0,reader.Sample));PlayerPrefs.SetInt(key+"auto",reader.AutoTurn?1:0);PlayerPrefs.Save();bookNextSave=Time.unscaledTime+5;
        }
        private void StopBookName()
        {bookNameGeneration++;bookNamePending=false;bookName=-1;if(readerVoice!=null){readerVoice.Stop();readerVoice.clip=null;}nameAudio=null;ReleaseBook(ref bookNameLease);bookWasSpeaking=bookAwaitingSpeech=false;}
        private void StopBookEffect()
        {bookEffectGeneration++;if(readerEffects!=null){readerEffects.Stop();readerEffects.clip=null;}effectAudio=null;ReleaseBook(ref bookEffectLease);}
        private void PauseBook()
        {if(!reader.Open)return;CaptureBookSample();reader.Pause();StopBookName();StopBookEffect();bookAutoAt=-1;SaveBookmark();UpdateBookControls();}
        private void CloseBook()
        {if(!reader.Open)return;PauseBook();reader.Close();ReleaseBookMedia();readerOverlay.gameObject.SetActive(false);stick.gameObject.SetActive(JoystickMode && !MenuOpen);CancelPointers();}
        private void TurnBook(int page)
        {if(!reader.Open || page<0 || page>=reader.Pages)return;PauseBook();reader.Turn(page);ShowBookPage();SaveBookmark();}
        private void ShowBookPage()
        {
            StopBookName();StopBookEffect();bookAutoAt=-1;pageAudio=null;ReleaseBook(ref bookPageLease);bookPagePending=true;
            var page=bookContent.pages[reader.Page];bookSelected=Math.Max(0,page.species);bookTitle.text=page.title;bookCaption.text=page.speech;bookCounter.text=(reader.Page+1)+" / "+reader.Pages;
            ShowBookPictures();UpdateBookControls();bookPageLease=AcquireBook(BookPath+"audio/page-"+reader.Page);StartCoroutine(LoadBookPage(++bookPageGeneration,bookPageLease));
        }
        private void ShowBookPictures()
        {
            if(bookFocus==null)return;var species=bookContent.pages[reader.Page].species;var group=DinosaurBook && species<0;
            var index=DinosaurBook?species:reader.Page;bookFocus.gameObject.SetActive(!group);bookFocus.sprite=index>=0 && index<bookPageSprites.Count?bookPageSprites[index]:null;
            readerOverlay.GetComponent<Image>().color=DinosaurBook?new Color(.23f,.30f,.25f):new Color(.07f,.09f,.10f);
            LayoutBookReader();
            for(var i=0;i<12;i++){bookChoices[i].gameObject.SetActive(group);bookChoiceLabels[i].gameObject.SetActive(group);bookChoices[i].sprite=bookPageSprites.Count==12?bookPageSprites[i]:null;bookChoiceLabels[i].text=DinosaurBook?bookContent.names[i]:"";}
        }
        private IEnumerator LoadBookPage(int generation,BookLease lease)
        {
            yield return LoadBookAudio(lease);if(!reader.Open || generation!=bookPageGeneration)yield break;
            pageAudio=lease.asset as AudioClip;bookPagePending=false;
            if(pageAudio==null || pageAudio.loadState!=AudioDataLoadState.Loaded){reader.Pause();bookStatus.text="Page audio is unavailable. You can still read and turn pages.";RecordBookAudio("page-unavailable");yield break;}
            reader.ClampSamples(pageAudio.samples);if(reader.Playing && !BookNaming)StartBookSpeech();UpdateBookControls();
        }
        private IEnumerator LoadBookAudio(BookLease lease)
        {
            yield return lease.request;CompleteBookLoad(lease);
            var clip=lease.asset as AudioClip;if(lease.users==0 || clip==null)yield break;
            // ResourceRequest completion only loads the clip object. These
            // files deliberately disable preloading to bound reader memory.
            if(clip.loadState==AudioDataLoadState.Unloaded && !clip.LoadAudioData())yield break;
            var until=Time.unscaledTime+10;
            while(lease.users>0 && clip!=null && clip.loadState==AudioDataLoadState.Loading && Time.unscaledTime<until)yield return null;
        }
        private void ToggleBookPlay()
        {
            if(!reader.Open)return;if(reader.Playing){PauseBook();return;}if(!Narration.VoiceEnabled)return;
            StopBookName();StopBookEffect();if(pageAudio!=null && reader.Sample>=pageAudio.samples-1)reader.Sample=0;
            reader.Play();bookAutoAt=-1;StartBookSpeech();UpdateBookControls();
        }
        private void StartBookSpeech()
        {
            if(!reader.Playing || bookPagePending || pageAudio==null || pageAudio.loadState!=AudioDataLoadState.Loaded || applicationPaused || !Narration.VoiceEnabled)return;
            Narration.Stop();reader.ClampSamples(pageAudio.samples);readerVoice.clip=pageAudio;readerVoice.timeSamples=reader.Sample;StartReaderVoice();
        }
        private void StartReaderVoice()
        {readerVoice.Play();bookWasSpeaking=false;bookAwaitingSpeech=true;bookSpeechDeadline=Time.unscaledTime+3;RecordBookAudio("play-requested");}
        private void BookName(int index)
        {
            if(!reader.Open || !DinosaurBook || index<0 || index>=bookContent.names.Length)return;bookSelected=index;bookMotionUntil=Time.unscaledTime+1.6f;
            if(!Narration.VoiceEnabled)return;CaptureBookSample();StopBookName();StopBookEffect();bookName=index;bookNamePending=true;bookAutoAt=-1;
            bookNameLease=AcquireBook(BookPath+"audio/name-"+index);StartCoroutine(LoadBookName(bookNameGeneration,reader.Generation,bookNameLease));UpdateBookControls();
        }
        private IEnumerator LoadBookName(int generation,int intent,BookLease lease)
        {
            yield return LoadBookAudio(lease);if(!reader.Current(intent) || generation!=bookNameGeneration)yield break;
            nameAudio=lease.asset as AudioClip;bookNamePending=false;
            if(nameAudio!=null && nameAudio.loadState==AudioDataLoadState.Loaded){Narration.Stop();readerVoice.clip=nameAudio;StartReaderVoice();}
            else{StopBookName();if(reader.CanResume(intent))StartBookSpeech();}
        }
        private void PlayBookEffect()
        {
            if(!reader.Open || !bookEffectsOn || applicationPaused)return;StopBookEffect();bookMotionUntil=Time.unscaledTime+1.6f;
            bookEffectLease=AcquireBook(BookPath+"audio/effect-"+(DinosaurBook?bookSelected:0));StartCoroutine(LoadBookEffect(bookEffectGeneration,bookEffectLease));
        }
        private IEnumerator LoadBookEffect(int generation,BookLease lease)
        {
            yield return LoadBookAudio(lease);if(!reader.Open || generation!=bookEffectGeneration || !bookEffectsOn || applicationPaused)yield break;
            effectAudio=lease.asset as AudioClip;if(effectAudio!=null && effectAudio.loadState==AudioDataLoadState.Loaded){readerEffects.clip=effectAudio;readerEffects.Play();RecordBookAudio("effect-requested");}
        }
        private void UpdateBookControls()
        {
            if(bookPlay==null)return;bookPlay.text=reader.Playing?"Pause":reader.Sample>0?"Continue":"Read to me";bookPlay.transform.parent.GetComponent<Button>().interactable=Narration.VoiceEnabled;
            bookAuto.text=reader.AutoTurn?"Auto pages on":"Auto pages off";bookVoiceLabel.text=Narration.VoiceEnabled?"Voice on":"Voice off";bookEffectsLabel.text=bookEffectsOn?"Sounds on":"Sounds off";
            bookStatus.text=bookPagePending?"Getting this page ready…":BookNaming?"Listen to the animal's name":"";
        }
        private void ReleaseBookMedia()
        {
            bookArtGeneration++;bookPageGeneration++;StopBookName();StopBookEffect();bookPagePending=false;pageAudio=null;ReleaseBook(ref bookPageLease);
            if(bookFocus!=null)bookFocus.sprite=null;foreach(var picture in bookChoices)picture.sprite=null;
            foreach(var sprite in bookPageSprites)Destroy(sprite);bookPageSprites.Clear();foreach(var item in bookArt){var lease=item;ReleaseBook(ref lease);}bookArt.Clear();
        }
        private void BookLowMemory(){if(!reader.Open)ReleaseBookMedia();}
        private void ResetBooks()
        {
            if(reader.Open)PauseBook();ReleaseBookMedia();Application.lowMemory-=BookLowMemory;
            if(readerVoice!=null)Destroy(readerVoice);if(readerEffects!=null)Destroy(readerEffects);readerVoice=readerEffects=null;
            readerOverlay=readerFrame=bookRack=bookRackFront=bookLibrary=bookLibraryFrame=null;bookFocus=bookWordsPanel=null;bookChoices.Clear();bookChoiceLabels.Clear();bookCovers.Clear();bookRackSprite=null;bookControlAnchors.Clear();
        }
        private Vector2 BookSupportPicture(int slot)=>ToBoard(HomeBooks.RackX,HomeBooks.RackY)+new Vector2((slot%3-1)*135,slot<3?251:157)*sceneScale;
        private Vector2 BookDropPoint(Vector2 raw)
        {
            if(CurrentArea!="garden" || !IsBook(dragging))return raw;var point=ToBoard(raw.x,raw.y);
            for(var i=0;i<6;i++)if(Vector2.Distance(point,BookSupportPicture(i))/sceneScale<60){bedroomDragTarget=HomeBooks.Support(i);return new Vector2(HomeBooks.X(i),HomeBooks.Y(i));}return raw;
        }
        private bool BookDropReads(string id,Vector2 point)
        {
            if(!IsBook(id) || bedroomDragTarget!="")return false;var p=ReadPlayer(Actor);
            if(Vector2.Distance(point,new Vector2(p.x,p.y))<105)return true;
            var room=FurnishedRoom;return room!=null && Mathf.Abs(point.x-(room.layout==0?1120:1460))<450 && point.y<200;
        }
        private void PresentBooks()
        {
            if(bookRack==null)return;var visible=CurrentArea=="garden" && !WorldLoading;bookRack.gameObject.SetActive(visible);bookRackFront.gameObject.SetActive(visible);
            bookRack.anchoredPosition=bookRackFront.anchoredPosition=ToBoard(HomeBooks.RackX,HomeBooks.RackY);bookRack.localScale=bookRackFront.localScale=Vector3.one*sceneScale;
            foreach(var t in ReadToys())if(t.id!=dragging && t.zone==CurrentArea && HomeBooks.Slot(t.container)>=0)toys[t.id].anchoredPosition=BookSupportPicture(HomeBooks.Slot(t.container));
            var scale=Vector3.one*Mathf.Min(safe.rect.width/1200,safe.rect.height/800);if(BookLibraryOpen){bookLibrary.SetAsLastSibling();bookLibraryFrame.localScale=scale;}
            if(!reader.Open)return;readerOverlay.SetAsLastSibling();LayoutBookReader();
            if(Time.unscaledTime>=bookNextSave)SaveBookmark();
            ObserveBookAudio();
            if(bookAwaitingSpeech && !applicationPaused)
            {
                if(readerVoice!=null && readerVoice.isPlaying){bookAwaitingSpeech=false;bookWasSpeaking=true;RecordBookAudio("play-started");}
                else if(Time.unscaledTime>=bookSpeechDeadline){RecordBookAudio("play-did-not-start");PauseBook();bookStatus.text="Sound did not start. Tap Read to me to try again.";}
            }
            if(bookWasSpeaking && readerVoice!=null && !readerVoice.isPlaying && !applicationPaused)
            {
                bookWasSpeaking=false;
                if(bookName>=0){StopBookName();if(reader.Playing)StartBookSpeech();}
                else if(reader.Playing && pageAudio!=null){reader.Sample=pageAudio.samples-1;readerVoice.clip=null;bookAutoAt=reader.AutoTurn && reader.Page<reader.Pages-1?Time.unscaledTime+1:-1;if(bookAutoAt<0)reader.Pause();SaveBookmark();}
                UpdateBookControls();
            }
            if(bookAutoAt>=0 && Time.unscaledTime>=bookAutoAt){TurnBook(reader.Page+1);reader.Play();StartBookSpeech();UpdateBookControls();}
            if(readerEffects!=null)readerEffects.volume=(VerifyRun!=null || familyTestMuted || shared?.MutedTest==true)?0:(BookSpeaking ? .13f : .45f);
            var phase=Mathf.Clamp01((bookMotionUntil-Time.unscaledTime)/1.6f);var moving=DinosaurBook && !quietStill && phase>0;
            bookFocus.rectTransform.anchoredPosition=new Vector2(moving?Mathf.Sin((1-phase)*Mathf.PI*4)*8:0,(DinosaurBook?80:0)+(moving?Mathf.Abs(Mathf.Sin((1-phase)*Mathf.PI*4))*5:0));
        }
        private void AddBookDepth(Action<RectTransform,float,int,string> add)
        {if(bookRack!=null){add(bookRack,bookRack.anchoredPosition.y,0,"book-rack");add(bookRackFront,bookRackFront.anchoredPosition.y,2,"book-lips");}}
    }
}
