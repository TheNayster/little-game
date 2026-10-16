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
        private readonly Dictionary<string,Text> marketsBadges=new Dictionary<string,Text>();
        private readonly Dictionary<string,string> marketsHeardActions=new Dictionary<string,string>();
        private GameCharacterVisual marketsBusker;
        private RectTransform marketsBuskerRoot;
        private RawImage marketsBuskerGuitar;
        private GameCharacterVisual marketsIndy;
        private RectTransform marketsIndyRoot;
        private int marketsSoundEvents,marketsHeardGuitar,marketsHeardSerial;
        private bool marketsSoundSeeded;
        public int MarketsSoundEvents=>marketsSoundEvents;
        public string[] MarketsMusicClips=>marketsMusic.Where(s=>s&&s.isPlaying&&s.clip).Select(s=>s.clip.name).ToArray();
        public void PrepareMarketsCapture(){if(CurrentArea==MarketsRules.Zone)TickMarkets();}
        private void BuildMarketsPresentation()
        {
            int index=0;Color[] colors={new Color(.68f,.9f,1),new Color(1,.79f,.53f),new Color(.74f,.94f,.69f),new Color(.89f,.77f,1)};
            foreach(var member in Markets.members){var badge=Panel(Board,"Markets player badge "+member.actor,Vector2.zero,new Vector2(92,28),colors[index++%4]);marketsBadges[member.actor]=Label(badge.transform,member.actor.Replace("player-","P")+(member.actor==Actor?" • You":""),15,Vector2.zero,new Vector2(90,26));}
            // Scene4 binds family dancers and a playable guitar UI, not a
            // separate performer. Bandit's busker role is a family adaptation.
            marketsBuskerRoot=Rect(Board,"Markets family busker",new Vector2(-270,10),Vector2.zero);
            marketsBusker=marketsBuskerRoot.gameObject.AddComponent<GameCharacterVisual>();marketsBusker.Select("bandit");
            marketsBuskerGuitar=MarketsPicture(Board,"Markets busker guitar","Discoveries/guitar",new Vector2(270,75),new Vector2(135,67));marketsBuskerGuitar.color=Color.white;

            marketsIndyRoot=Rect(Board,"Markets Indy support",Vector2.zero,Vector2.zero);marketsIndy=marketsIndyRoot.gameObject.AddComponent<GameCharacterVisual>();marketsIndy.Select("indy");

        }
        private void ResetMarketsPresentation()
        {marketsBadges.Clear();marketsHeardActions.Clear();marketsBusker=null;marketsBuskerRoot=null;marketsBuskerGuitar=null;marketsIndy=null;marketsIndyRoot=null;marketsSoundEvents=marketsHeardGuitar=marketsHeardSerial=0;marketsSoundSeeded=false;}
        private GameCharacterVisual MarketsPerson(string actor)=>actor==Actor?characterVisual:friends.TryGetValue(actor,out var f)?f.view:null;
        private Vector2 MarketsHandPoint(GameCharacterVisual visual)
        {
            var picture=visual.ActiveView.GetComponentsInChildren<RawImage>().First(i=>i.name=="Selected character drawing").rectTransform;
            // Measured palms in the authored wave crops: Bluey frame3 has
            // palm(1184,143) within crop(994,6,215,299); Chilli's matching
            // crop places its palm at approximately(.91,.55). Transform the
            // drawing so facing, crop pivot and cast scale remain respected.
            var hand=visual.Frame.Pose==CharacterPose.BalloonTap?new Vector2(.89f,.55f):new Vector2(.62f,.4f);
            var rect=picture.rect;return Board.InverseTransformPoint(picture.TransformPoint(new Vector3(rect.xMin+rect.width*hand.x,rect.yMin+rect.height*hand.y,0)));
        }
        private Vector2 MarketsGroundPoint(GameCharacterVisual visual)
        {
            // CharacterSheetView sets this image pivot from each frame's authored
            // ground pixel. Its origin therefore stays valid across cast/pose crops.
            var picture=visual.ActiveView.GetComponentsInChildren<RawImage>().First(i=>i.name=="Selected character drawing").rectTransform;
            return Board.InverseTransformPoint(picture.TransformPoint(Vector3.zero));
        }
        private Vector2 MarketsPonyContact(string station,string pose)
        {
            if(!marketsPonies.TryGetValue(station,out var image))return Vector2.zero;
            // Source512px alpha bbox(115,77)-(402,434); measured mouth-edge
            // pixel(132,222), body-edge(285,245). Transform through the actual
            // image rect instead of targeting transparent frame padding.
            Vector2 pixel=pose=="clean"?new Vector2(300,426):pose=="brush"||pose=="wash"?new Vector2(285,245):new Vector2(132,222);
            var rect=image.rectTransform.rect;var local=new Vector3(rect.xMin+pixel.x/512*rect.width,rect.yMin+(1-pixel.y/512)*rect.height,0);
            return Board.InverseTransformPoint(image.rectTransform.TransformPoint(local));
        }
        private void TickMarketsPresentation(MarketsState h,bool visible)
        {
            foreach(var pair in marketsBadges){var p=ReadPlayer(pair.Key);var person=MarketsPerson(pair.Key);bool show=visible&&p.zone==MarketsRules.Zone&&person&&person.gameObject.activeInHierarchy;pair.Value.transform.parent.gameObject.SetActive(show);if(show)((RectTransform)pair.Value.transform.parent).anchoredPosition=(marketsLastView=="icecream"&&Math.Abs(p.x-MarketsRules.X("icecream"))<=300?(Vector3)((RectTransform)person.transform).anchoredPosition+new Vector3(0,-28,0):Board.InverseTransformPoint(person.ActiveView.OverheadPoint)+new Vector3(0,18,0));}
            bool music=visible&&marketsLastView=="music";marketsBuskerRoot.gameObject.SetActive(music);marketsBuskerGuitar.gameObject.SetActive(music);
            if(music){float age=(float)Math.Max(0,h.clock-h.musicAt);marketsBusker.PresentNpcFrame(new CharacterFrame(h.playing?CharacterPose.Carry:CharacterPose.Idle,0,false,age),Time.unscaledDeltaTime,.8f);marketsBuskerRoot.anchoredPosition=new Vector2(230,20+(h.playing?Mathf.Abs(Mathf.Sin(age*Mathf.PI*4))*3:0));marketsBuskerGuitar.rectTransform.localRotation=Quaternion.Euler(0,0,h.playing?Mathf.Sin(age*Mathf.PI*4)*3:0);}
            bool indy=visible&&(marketsLastView=="flowers"||marketsLastView=="ponies"||music)&&!h.members.Any(m=>(!Shared||shared.Players.Contains(m.actor))&&ReadPlayer(m.actor).avatar=="indy");
            marketsIndyRoot.gameObject.SetActive(indy);
            if(indy){marketsIndyRoot.anchoredPosition=marketsLastView=="ponies"?new Vector2(15,-140):music?new Vector2(-15,-140):new Vector2(185,-140);bool dancing=music&&h.playing;var pose=dancing?CharacterPose.Dance:h.clock%9<1.8?CharacterPose.Wave:CharacterPose.Idle;marketsIndy.PresentNpcFrame(new CharacterFrame(pose,0,false,(float)h.clock),Time.unscaledDeltaTime,.36f);}
        }
        private void MarketsPlaySound(string key)
        {var clip=Resources.Load<AudioClip>("Worlds/Markets/Audio/"+key);if(clip&&marketsEffect){marketsEffect.PlayOneShot(clip);marketsSoundEvents++;}}
        private void MarketsActionSounds(MarketsState h)
        {
            if(!marketsSoundSeeded){marketsSoundSeeded=true;marketsSeenPop=h.popped;marketsSeenHits=h.musicHits;marketsSeenPony=h.ponies.Sum(p=>p.rounds);marketsHeardGuitar=h.guitar;marketsHeardSerial=h.serial;foreach(var m in h.members)marketsHeardActions[m.actor]=m.at+"/"+m.pose;return;}
            if(h.popped!=marketsSeenPop)MarketsPlaySound("sfx_mkd_bubble_pop_01");
            if(h.musicHits!=marketsSeenHits)MarketsPlaySound("sfx_mkd_busker_correct_button_press");
            if(h.guitar!=marketsHeardGuitar)MarketsPlaySound("sfx_mkd_busker_string_pluck_01");
            if(h.ponies.Sum(p=>p.rounds)!=marketsSeenPony)MarketsPlaySound("sfx_mkd_ponies_pony_reaction_happy_01");
            if(h.serial!=marketsHeardSerial&&h.items.Any(i=>i.id=="market-item-"+h.serial&&i.kind=="smoothie"))MarketsPlaySound("sfx_mkd_market_blender_finish_flourish");
            foreach(var m in h.members){string key=m.at+"/"+m.pose;if(marketsHeardActions.TryGetValue(m.actor,out var old)&&old!=key&&ReadPlayer(m.actor).zone==MarketsRules.Zone){
                string sound=m.pose.StartsWith("feed-")?"sfx_mkd_ponies_pony_eating_01":m.pose=="water"?"sfx_mkd_ponies_pony_drinking_01":m.pose=="wash"?"sfx_mkd_ponies_bucket_bubbles_loop":m.pose=="mix"?"sfx_mkd_market_blender_ontap_01":m.pose=="eat"?"sfx_mkd_market_eating_cupcake_01":m.pose=="drink"?"sfx_mkd_drinking_water_01":m.pose=="reach"&&m.station=="bubbles"?"sfx_mkd_market_bubble_drag_spawn_01":m.pose=="reach"&&m.station=="smoothie"?"sfx_mkd_market_drop_fruit_into_blender_01":m.pose=="reach"&&m.station=="cakes"?"sfx_mkd_market_cupcake_ontap_01":"";
                if(sound!="")MarketsPlaySound(sound);
            }marketsHeardActions[m.actor]=key;}
            marketsSeenPop=h.popped;marketsSeenHits=h.musicHits;marketsSeenPony=h.ponies.Sum(p=>p.rounds);marketsHeardGuitar=h.guitar;marketsHeardSerial=h.serial;
        }
    }
}
