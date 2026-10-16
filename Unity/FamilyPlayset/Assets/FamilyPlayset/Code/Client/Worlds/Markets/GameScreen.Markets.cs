using System;
using System.Linq;
using System.Collections.Generic;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform marketsControls,marketsTabs,marketsTools;
        private RawImage marketsBackground;
        private Text marketsHint,marketsProgress;
        private readonly Dictionary<string,Texture2D> marketsTextures=new Dictionary<string,Texture2D>();
        private readonly Dictionary<string,RectTransform> marketsProducts=new Dictionary<string,RectTransform>();
        private readonly Dictionary<int,Image> marketsBubbles=new Dictionary<int,Image>();
        private readonly List<AudioSource> marketsMusic=new List<AudioSource>();
        private AudioSource marketsEffect;
        private string marketsProductStation="";
        private string marketsPending="",marketsControlKey="",marketsOutcome="",marketsLastView="",marketsAudioKey="";
        private bool marketsVisible;
        private int marketsProductPage,marketsSeenPop,marketsSeenHits,marketsSeenPony;
        private string marketsSelectedBrick="Base_LightPurple",marketsSelectedPlaced="";
        private RectTransform marketsBuildBoard;
        private readonly Dictionary<int,RawImage> marketsBuildSlots=new Dictionary<int,RawImage>();
        private readonly Dictionary<string,RawImage> marketsPonies=new Dictionary<string,RawImage>();
        private readonly Dictionary<string,RawImage> marketsActionProps=new Dictionary<string,RawImage>();
        private MarketsState Markets=>Shared?shared.View?.markets:HasWorld?World.ReadMarkets():null;
        public MarketsState MarketsViewState=>Markets;
        private Sprite marketsMenuSprite;
        private Sprite MarketsMenuPicture(){if(!marketsMenuSprite){var t=MarketsTexture("smoothie");if(t)marketsMenuSprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f);}return marketsMenuSprite;}
        public string MarketsOutcome=>marketsOutcome;
        public string MarketsView=>marketsLastView;
        private bool MarketsWalkingApproach=>CurrentArea==MarketsRules.Zone&&marketsPending!="";
        private void CancelMarketsIntent(){marketsPending="";marketsQueuedAction=null;marketsQueuedStation="";}
        private Texture2D MarketsTexture(string key)
        {
            switch(key){case "pfb_mkd_FoodItem_Apples":key="pfb_mkd_Apples_Scene1";break;case "pfb_mkd_FoodItem_Bananas":key="pfb_mkd_Banana";break;case "pfb_mkd_FoodItem_Carrots":key="pfb_mkd_Carrots_Scene1";break;case "pfb_mkd_FoodItem_Grapes":key="pfb_mkd_Grapes";break;case "apple":key="pfb_mkd_Apples_Scene1";break;case "water":key="pfb_mkd_Bucket";break;case "brush":key="pony-brush";break;case "pitchfork":key="pony-pitchfork";break;case "cup":key="pfb_mkd_SmoothieCup";break;case "pony-buttermilk":key="pfb_emb_Pony_Brown";break;case "pony-biscuit":key="pfb_emb_Pony_Grey";break;case "ice-chocolate":key="pfb_mkd_byo_ChocolateIceCream_Stack1";break;case "ice-sundae":key="pfb_mkd_byo_SundaeIceCream_Stack1";break;case "ice-mango":key="pfb_mkd_byo_MangoIceCream_Stack1";break;}
            if(key=="pfb_mkd_FoodItem_Apple")key="pfb_mkd_Apples_Scene1";if(key=="pfb_mkd_FoodItem_Banana")key="pfb_mkd_Banana";if(key=="pfb_mkd_FoodItem_Carrot")key="pfb_mkd_Carrots_Scene1";
            key=key.Replace("pfb_mkd_Cupcake_","pfb_mkd_Cupcake").Replace("pfb_mkd_byo_pfb_mkd_byo_","pfb_mkd_byo_").Replace("_coop_LightPurple","_Scoop_LightPurple");
            if(!marketsTextures.TryGetValue(key,out var texture)){texture=Resources.Load<Texture2D>("Worlds/Markets/Art/"+key);marketsTextures[key]=texture;}return texture;
        }
        private RawImage MarketsPicture(Transform parent,string name,string key,Vector2 point,Vector2 size,bool input=false)
        {var image=Rect(parent,name,point,size).gameObject.AddComponent<RawImage>();image.texture=MarketsTexture(key);image.raycastTarget=input;return image;}
        private void ResetMarkets()
        {
            ResetMarketsLayout();ResetMarketsPreparation();ResetMarketsDiscoveries();ResetMarketsPresentation();
            foreach(var s in marketsMusic)if(s)Destroy(s);marketsMusic.Clear();if(marketsEffect)Destroy(marketsEffect);
            marketsControls=null;marketsTabs=null;marketsTools=null;marketsBackground=null;marketsHint=null;marketsProgress=null;marketsEffect=null;marketsBuildBoard=null;marketsBuildSlots.Clear();marketsPonies.Clear();
            marketsProducts.Clear();marketsBubbles.Clear();marketsActionProps.Clear();marketsPending="";marketsControlKey="";marketsLastView="";marketsAudioKey="";marketsVisible=false;marketsProductStation="";marketsProductPage=0;
            if(marketsMenuSprite)Destroy(marketsMenuSprite);marketsMenuSprite=null;foreach(var t in marketsTextures.Values.Distinct())if(t)Resources.UnloadAsset(t);marketsTextures.Clear();
        }
        private void BuildMarkets()
        {
            if(Markets==null)return;
            marketsBackground=MarketsPicture(Board,"Markets recovered setting","smoothie",Vector2.zero,Board.rect.size);marketsBackground.transform.SetAsFirstSibling();
            BuildMarketsLayout();
            foreach(var id in new[]{"buttermilk","biscuit"})marketsPonies[id]=MarketsPicture(Board,"Markets pony "+id,"pony-"+id,Vector2.zero,Vector2.one*270);
            foreach(var m in Markets.members)marketsActionProps[m.actor]=MarketsPicture(Board,"Markets action prop "+m.actor,"",Vector2.zero,Vector2.one*64);
            for(int i=0;i<5;i++){var s=gameObject.AddComponent<AudioSource>();s.playOnAwake=false;s.loop=true;s.volume=.12f;marketsMusic.Add(s);}
            marketsEffect=gameObject.AddComponent<AudioSource>();marketsEffect.playOnAwake=false;marketsEffect.volume=.4f;
            BuildMarketsPreparation();BuildMarketsDiscoveries();BuildMarketsPresentation();
        }
        private void MarketsTool(string tool)
        {
            var h=Markets;var item=h.items.FirstOrDefault(i=>i.holder==Actor);
            if(tool=="Put down"&&item!=null)MarketsSend("drop",item.id,value:item.revision.ToString());
            else if(tool=="Give"&&item!=null){var p=ReadPlayer(Actor);var friend=h.members.Where(m=>m.actor!=Actor).Select(m=>ReadPlayer(m.actor)).Where(a=>a.zone==p.zone).OrderBy(a=>Math.Abs(a.x-p.x)).FirstOrDefault();if(friend!=null)MarketsSend("give",item.id,friend.id,item.revision.ToString());}
            else if(tool=="Leave")MarketsSend("leave");else if(tool=="Sign")MarketsSend("sign");else if(tool=="Umbrella")MarketsSend("umbrella");else if(tool=="Long dog")MarketsSend("longdog");else if(tool=="More items")marketsProductPage=(marketsProductPage+1)%Math.Max(1,(h.items.Count(i=>i.station==marketsProductStation&&i.holder=="")+7)/8);
        }
        private void MarketsBuildSlot(int slot)
        {
            var h=Markets;var brick=h.bricks.FirstOrDefault(b=>b.slot==slot);
            if(brick!=null){marketsSelectedPlaced=brick.id;MarketsSend("build-turn",brick.id,((brick.turn+1)%4).ToString(),brick.revision.ToString());}
            else if(marketsSelectedPlaced!=""&&h.bricks.Any(b=>b.id==marketsSelectedPlaced&&b.creator==Actor)){brick=h.bricks.Single(b=>b.id==marketsSelectedPlaced);MarketsSend("build-move",brick.id,slot.ToString(),brick.revision.ToString());marketsSelectedPlaced="";}
            else MarketsSend("build-add",marketsSelectedBrick,slot.ToString(),h.stations.Single(t=>t.id=="icecream").revision.ToString());
        }
        private void MarketsGo(string station)
        {
            if(CurrentArea!=MarketsRules.Zone||ActionPending)return;
            CancelMarketsIntent();marketsPending=station;destination=new Vector2(MarketsRules.X(station),300);manualCamera=false;
            marketsControlKey="";
        }
        private void MarketsSend(string operation,string item="",string target="",string value="")
        {
            if(CurrentArea!=MarketsRules.Zone||ActionPending)return;
            destination=null;if(Shared)shared.Walk(WalkMode.Stop);
            void Done(SoloResult result){marketsOutcome=result.Outcome;if(!result.Accepted)marketsHint.text=result.Outcome=="wait-your-turn"?"Your friend's turn — yours is next":Friendly(result.Outcome);Render();}
            var command=operation+":"+value;
            if(Shared)SubmitShared(SoloAction.Markets,item,target,command,0,0,Done);else Done(Command(SoloAction.Markets,item:item,target:target,value:command));
        }
        private void MarketsBuildControls(string station)
        {
            foreach(Transform child in marketsControls){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            MarketsBuildWorldControls(station);
        }
        private static string Upper(string value)=>string.IsNullOrEmpty(value)?value:char.ToUpperInvariant(value[0])+value.Substring(1);
        private string MarketsNearest(float x)=>MarketsRules.StationIds.OrderBy(id=>Math.Abs(MarketsRules.X(id)-x)).First();
        private void TickMarkets()
        {
            if(!marketsBackground)return;bool visible=CurrentArea==MarketsRules.Zone;
            marketsBackground.gameObject.SetActive(visible);marketsTabs.gameObject.SetActive(visible);marketsTools.gameObject.SetActive(visible);marketsControls.gameObject.SetActive(visible);marketsHint.transform.parent.gameObject.SetActive(visible);marketsProgress.transform.parent.gameObject.SetActive(visible);marketsBuildBoard.gameObject.SetActive(visible&&marketsLastView=="icecream");foreach(var pony in marketsPonies.Values)pony.gameObject.SetActive(visible&&marketsLastView=="ponies");
            foreach(var pair in marketsProducts)pair.Value.gameObject.SetActive(false);foreach(var pair in marketsBubbles)pair.Value.gameObject.SetActive(false);
            foreach(var image in marketsActionProps.Values)image.gameObject.SetActive(false);
            if(!visible){TickMarketsLayout(null,"",false);TickMarketsPreparation(null,"",false);TickMarketsDiscoveries(null,false);TickMarketsPresentation(null,false);if(marketsVisible){CancelMarketsIntent();if(marketsEffect)marketsEffect.Stop();foreach(var s in marketsMusic)s.Stop();marketsAudioKey="";}marketsVisible=false;return;}
            marketsVisible=true;var h=Markets;if(h==null)return;var p=ReadPlayer(Actor);var member=h.members.Single(a=>a.actor==Actor);
            if(marketsPending!=""&&Math.Abs(p.x-MarketsRules.X(marketsPending))<24&&Math.Abs(p.y-300)<25&&!ActionPending){string station=marketsPending;marketsPending="";MarketsSend("join",target:station);}
            string current=member.station!=""?member.station:MarketsNearest(p.x);
            if(marketsProductStation!=current){marketsProductStation=current;marketsProductPage=0;}
            marketsHint.transform.parent.gameObject.SetActive(current!="icecream");
            string view=current=="flowers"||current=="cakes"?"flowers":current=="buttermilk"||current=="biscuit"?"ponies":current;
            if(marketsLastView!=view){marketsLastView=view;marketsBackground.texture=MarketsTexture(view);}
            marketsBackground.rectTransform.sizeDelta=Board.rect.size;marketsBackground.rectTransform.anchoredPosition=Vector2.zero;
            string key=current+"/"+h.playing+"/"+h.stations.Single(a=>a.id=="icecream").choice+"/"+h.stations.Single(a=>a.id=="icecream").parts.Length+"/"+string.Join(",",h.ponies.Select(a=>a.body+"-"+a.mane+"-"+a.saddle+"-"+a.tail));
            if(marketsControlKey!=key){marketsControlKey=key;MarketsBuildControls(current);}
            var stationState=h.stations.Single(a=>a.id==current);string next=stationState.queue.FirstOrDefault();
            marketsProgress.text=Upper(current)+"  •  "+(next==null?"Join in":next==Actor?"Your turn":next.Replace("player-","Player ")+"'s turn")+"  •  "+stationState.completed+" finished";
            if(member.station=="")marketsHint.text=marketsPending!=""?"On our way…":"Tap a stall, pony or pictured object";
            else if(stationState.busyUntil>h.clock)marketsHint.text=next==Actor?"Keep watching your action":"Your friend is helping";
            else if(stationState.parts.Length>0)marketsHint.text=string.Join(" + ",stationState.parts.Select(Upper));
            else if(current=="buttermilk"||current=="biscuit"){var pony=h.ponies.Single(a=>a.id==current);marketsHint.text=Upper(current)+"  "+((pony.needs&1)!=0?"♥ ":"")+((pony.needs&2)!=0?"Water ✓  ":"")+((pony.needs&4)!=0?"Brushed ✓  ":"")+((pony.needs&8)!=0?"Tidy ✓  ":"")+"  Visits: "+pony.rounds;}
            else marketsHint.text="Tap or drag an object to play";
            TickMarketsPreparation(h,current,true);TickMarketsDiscoveries(h,true);MarketsPresentPeople(h);MarketsPresentProducts(h,current);MarketsPresentBubbles(h);MarketsTickSound(h);MarketsPresentBuild(h);MarketsPresentPonies(h);MarketsPresentActionProps(h);TickMarketsPresentation(h,true);TickMarketsLayout(h,current,true);
            if(miniGamesButton)miniGamesButton.gameObject.SetActive(false);
        }
        private void MarketsPresentBuild(MarketsState h)
        {
            if(marketsLastView!="icecream")return;
            foreach(var pair in marketsBuildSlots){var brick=h.bricks.FirstOrDefault(b=>b.slot==pair.Key);pair.Value.texture=brick==null?null:MarketsTexture("pfb_mkd_byo_"+brick.variant);pair.Value.color=brick==null?Color.clear:Color.white;pair.Value.rectTransform.localRotation=Quaternion.Euler(0,0,brick==null?0:-90*brick.turn);}
        }
        private void MarketsPresentPonies(MarketsState h)
        {
            if(marketsLastView!="ponies")return;
            foreach(var pony in h.ponies){var image=marketsPonies[pony.id];image.rectTransform.anchoredPosition=new Vector2(pony.id=="buttermilk"?-160:175,-18);image.rectTransform.sizeDelta=Vector2.one*360;
                var station=h.stations.Single(t=>t.id==pony.id);var member=h.members.FirstOrDefault(m=>m.actor==station.workingActor);
                bool feeding=member!=null&&(member.pose.StartsWith("feed-")||member.pose=="water")&&station.busyUntil>h.clock;
                string variant=(feeding?"pony-eat-body-":"pony-body-")+pony.body+"-mane-"+pony.mane+"-saddle-"+pony.saddle;var chosen=MarketsTexture(variant);image.texture=chosen?chosen:MarketsTexture("pony-"+pony.id);image.uvRect=new Rect(0,0,1,1);
                image.rectTransform.localRotation=Quaternion.Euler(0,0,member!=null&&station.busyUntil>h.clock?Mathf.Sin((float)(h.clock-member.at)*3)*1.2f:Mathf.Sin((float)h.clock*.4f)*.35f);
            }
        }
        private void MarketsPresentActionProps(MarketsState h)
        {
            foreach(var m in h.members){
                var p=ReadPlayer(m.actor);if(p.zone!=MarketsRules.Zone||m.pose=="idle"||m.pose=="dance"||m.pose=="celebrate"||m.station=="")continue;
                var image=marketsActionProps[m.actor];string key=m.pose=="water"?"water":m.pose=="feed-carrot"?"pfb_mkd_Carrots_Scene1":m.pose=="wash"?"water":m.pose=="brush"?"brush":m.pose=="clean"?"pitchfork":m.station=="smoothie"?"cup":m.station=="flowers"?"pfb_mkd_Flower_Pink":m.station=="cakes"?"pfb_mkd_CupcakePink":m.station=="bubbles"?"pfb_mkd_BubbleWand":m.station=="buttermilk"||m.station=="biscuit"?"pony-apple":"";
                if(key=="")continue;image.texture=MarketsTexture(key);if(!image.texture)continue;image.gameObject.SetActive(true);
                float age=(float)Math.Max(0,h.clock-m.at);var person=MarketsPerson(m.actor);var hand=person&&person.ActiveView?MarketsHandPoint(person):ToBoard(p.x,p.y)+new Vector2(32,74)*sceneScale;
                float size=(m.pose=="clean"?110:m.pose=="brush"?80:64)*sceneScale;
                var center=hand;var dimensions=Vector2.one*size;
                if(m.station=="buttermilk"||m.station=="biscuit"){
                    var contact=MarketsPonyContact(m.station,m.pose);
                    if(m.pose=="brush")contact.y+=Mathf.Sin(age*9)*8;
                    // The source prop visibly bridges the grounded hand and pony contact.
                    // Bound enlargement so the artwork remains a recognizable tool.
                    center=(hand+contact)*.5f;
                    float limit=(m.pose=="clean"?110:100)*sceneScale;
                    dimensions=new Vector2(Mathf.Clamp(Mathf.Abs(contact.x-hand.x)+size*.65f,size,limit),Mathf.Clamp(Mathf.Abs(contact.y-hand.y)+size*.65f,size,limit));
                }
                image.transform.SetAsLastSibling();image.rectTransform.anchoredPosition=center;image.rectTransform.localRotation=Quaternion.Euler(0,0,m.pose=="brush"?Mathf.Sin(age*9)*15:0);image.rectTransform.sizeDelta=dimensions;
            }
        }
        private void MarketsPresentPeople(MarketsState h)
        {
            foreach(var member in h.members){
                var p=ReadPlayer(member.actor);if(p.zone!=MarketsRules.Zone)continue;
                GameCharacterVisual visual=member.actor==Actor?characterVisual:friends.TryGetValue(member.actor,out var f)?f.view:null;
                if(!visual||!visual.gameObject.activeInHierarchy)continue;
                var position=Shared&&shared.Connected?shared.VisualPosition(member.actor):new Vector2(p.x,p.y);
                var personRoot=(RectTransform)visual.transform;personRoot.localScale=Vector3.one*sceneScale;personRoot.anchoredPosition=ToBoard(position.x,position.y);
                if(marketsLastView=="icecream"&&Math.Abs(p.x-MarketsRules.X("icecream"))<=300){
                    int helper=Array.FindIndex(h.members,m=>m.actor==member.actor);personRoot.localScale=Vector3.one*sceneScale*.30f;personRoot.anchoredPosition=new Vector2(-420+helper*140,-240);
                }
                if(member.pose=="idle"){
                    if(h.items.Any(i=>i.holder==member.actor))visual.PresentFrame(new CharacterFrame(CharacterPose.Carry,visual.Frame.Speed,visual.Frame.FaceLeft,0),Time.unscaledDeltaTime);
                    continue;
                }
                bool care=member.station=="buttermilk"||member.station=="biscuit";
                var pose=care&&member.pose!="celebrate"&&member.pose!="wave"?(member.pose=="clean"?CharacterPose.Carry:CharacterPose.BalloonTap):member.pose=="dance"||member.pose=="guitar"?CharacterPose.Dance:member.pose=="celebrate"?CharacterPose.Wave:member.pose=="eat"||member.pose=="drink"?CharacterPose.Carry:CharacterPose.BalloonTap;
                visual.PresentFrame(new CharacterFrame(pose,0,p.x>MarketsRules.X(member.station),Mathf.Max(0,(float)(h.clock-member.at))),Time.unscaledDeltaTime);
                if(care&&member.pose!="celebrate"&&member.pose!="wave"){
                    // Keep the normal grounded character height. Only approach
                    // horizontally; a tall pony must never pull a child's feet up.
                    personRoot.localScale=Vector3.one*sceneScale;
                    var target=MarketsPonyContact(member.station,member.pose);
                    float side=visual.Frame.FaceLeft?1:-1;
                    float reach=target.x+side*48*sceneScale-MarketsHandPoint(visual).x;
                    personRoot.anchoredPosition+=new Vector2(reach*Mathf.Clamp01((float)(h.clock-member.at)*4),0);
                }
            }
        }
        private void MarketsPresentProducts(MarketsState h,string current)
        {
            var items=h.items.Where(i=>i.station==current&&i.holder==""||i.holder!="").ToArray();
            var table=items.Where(i=>i.holder=="").ToArray();marketsProductPage=Mathf.Clamp(marketsProductPage,0,Math.Max(0,(table.Length-1)/8));
            foreach(var item in items){
                if(!marketsProducts.TryGetValue(item.id,out var root)){
                    var panel=Panel(Board,"Markets product "+item.id,Vector2.zero,new Vector2(62,64),new Color(1,1,1,.9f),true,false);root=panel.rectTransform;marketsProducts[item.id]=root;
                    string id=item.id;NavButton(panel,()=>{var now=Markets.items.FirstOrDefault(a=>a.id==id);if(now!=null)MarketsSend(now.holder==Actor?"consume":"claim",id,value:now.revision.ToString());});
                    MarketsPicture(root,"Product picture",item.kind=="cake"?"pfb_mkd_Cupcake_"+Upper(item.choice):item.kind=="flowers"?"pfb_mkd_Flower_Pink":item.kind=="icecream"?"ice-"+item.choice:"cup",Vector2.zero,new Vector2(60,60));
                }
                bool held=item.holder!="";var holder=held?ReadPlayer(item.holder):null;
                int index=Array.IndexOf(table,item);bool shown=held?holder.zone==MarketsRules.Zone:index>=marketsProductPage*8&&index<marketsProductPage*8+8;
                root.gameObject.SetActive(shown);if(!shown)continue;
                var person=held?MarketsPerson(item.holder):null;
                root.anchoredPosition=held?(person&&person.ActiveView?MarketsHandPoint(person):ToBoard(holder.x,holder.y)+new Vector2(43,82)*sceneScale):new Vector2(Board.rect.width*.5f-48-(index%2)*66,marketsLastView=="icecream"?-35-index/2%4*58:75-index/2%4*62);root.localScale=Vector3.one*(held?sceneScale:1);
            }
        }
        private void MarketsPresentBubbles(MarketsState h)
        {
            if(marketsLastView!="bubbles")return;
            foreach(var bubble in h.bubbles){
                if(!marketsBubbles.TryGetValue(bubble.id,out var image)){int id=bubble.id;image=Panel(Board,"Markets pop "+id,Vector2.zero,Vector2.one*80,new Color(.77f,.9f,1,.67f),true,false);image.sprite=circle;marketsBubbles[id]=image;NavButton(image,()=>MarketsSend("pop",id.ToString()));if(bubble.shape>0)Label(image.transform,"BLUEY"[bubble.shape-1].ToString(),28,Vector2.zero,Vector2.one*70);}
                image.gameObject.SetActive(true);float age=(float)(h.clock-bubble.born);image.rectTransform.anchoredPosition=new Vector2((bubble.id%7-3)*125+Mathf.Sin(age*.9f+bubble.id)*25,-80+age*38);image.rectTransform.localScale=Vector3.one*(.65f+.12f*(bubble.id%3));
            }
        }
        private void MarketsTickSound(MarketsState h)
        {
            foreach(var s in marketsMusic){s.mute=musicMuted||applicationPaused;s.volume=.12f*ForegroundDucking;}
            marketsEffect.mute=musicMuted||applicationPaused;
            string key=h.playing?h.song+"/"+h.layers:"";
            if(key!=marketsAudioKey){marketsAudioKey=key;string[] songs={"one","two","three"};for(int i=0;i<marketsMusic.Count;i++){var s=marketsMusic[i];s.Stop();if(h.playing&&i<h.layers){s.clip=Resources.Load<AudioClip>("Worlds/Markets/Audio/mus_mkd_busker_song_"+songs[h.song]+"_layer_"+new[]{"one","two","three","four","five"}[i]+(h.song==2&&i==4?"_sax":""));if(s.clip){s.time=(float)((h.clock-h.musicAt)%s.clip.length);s.PlayScheduled(AudioSettings.dspTime+.1);}}}}
            MarketsActionSounds(h);
        }
    }
}
