using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform storyBasket,storyBall,storyInvitation,storyReturn,storyClue,storyPut,storyLeafCover;
        private readonly RectTransform[] storyPlants=new RectTransform[2];
        private Image storyReturnRim;
        private long storyObserved=-1,storySoundSession=-1;
        private bool storyWasReacting;
        public Vector2 StoryBallPoint=>storyBall==null?Vector2.zero:storyBall.anchoredPosition;
        public Vector2 StoryTrunkPoint=>zooAnimals.TryGetValue("elephant",out var image)?((RectTransform)image.transform.parent).anchoredPosition+((ElephantArtView)image).TrunkTip*sceneScale:Vector2.zero;
        private bool StoryInputReady=>Ready && !applicationPaused && !MenuOpen && !ZooMapOpen && !ZooPhotoOpen && !ElephantSnackOpen && !TravelPending && !ActionPending && (!Shared || shared.Connected);
        private void BallPicture(Transform parent,Vector2 at,float size)
        {
            Panel(parent,"Ball blue outline",at,Vector2.one*size,new Color(.22f,.45f,.58f),false,true);
            Panel(parent,"Ball turquoise",at,Vector2.one*(size-6),new Color(.36f,.75f,.78f),false,true);
            Panel(parent,"Ball coral patch",at+new Vector2(-.16f,.08f)*size,Vector2.one*(size*.43f),new Color(.97f,.65f,.44f),false,true);
            Panel(parent,"Ball cream patch",at+new Vector2(.2f,-.16f)*size,Vector2.one*(size*.34f),Cream,false,true);
            Panel(parent,"Ball glint",at+new Vector2(-.13f,.27f)*size,new Vector2(.23f,.09f)*size,new Color(1,1,1,.7f),false,true);
        }
        private void BuildElephantStory()
        {
            // Original editable geometry is retained in elephant-missing-ball.svg.
            storyBasket=ZooObject("Elephant toy basket");
            Panel(storyBasket,"Basket shadow",new Vector2(0,8),new Vector2(135,22),new Color(.29f,.34f,.23f,.18f),false,true);
            Panel(storyBasket,"Toy basket weave",new Vector2(0,30),new Vector2(124,56),new Color(.72f,.51f,.31f),false,true);
            Panel(storyBasket,"Empty basket opening",new Vector2(0,51),new Vector2(120,22),new Color(.4f,.33f,.24f),false,true);
            for(var i=-2;i<=2;i++)Plain(storyBasket,"Woven stripe",new Vector2(i*21,27),new Vector2(5,29),new Color(.86f,.69f,.43f));
            storyInvitation=Panel(storyBasket,"Start Missing Ball",new Vector2(-54,132),new Vector2(120,120),Cream,true,true).rectTransform;
            BallPicture(storyInvitation,Vector2.zero,72);
            NavButton(storyInvitation.GetComponent<Image>(),()=>{if(StoryInputReady)SendZoo("story-start","elephant",_=>{});});
            storyReturn=Panel(storyBasket,"Return elephant ball",new Vector2(0,31),new Vector2(154,90),new Color(1,1,1,0),true,true).rectTransform;
            NavButton(storyReturn.GetComponent<Image>(),()=>{if(StoryInputReady && Zoo.story.carrier==Actor)ZooWalk("story-return",Zoo.story.session.ToString(),new Vector2(ElephantStory.BasketX,100));});
            storyReturnRim=Panel(storyReturn,"Ball return picture",new Vector2(0,50),Vector2.one*71,new Color(.97f,.85f,.52f,.7f),false,true);
            BallPicture(storyReturnRim.transform,Vector2.zero,43);
            var down=Plain(storyReturn,"Return arrow",new Vector2(0,17),new Vector2(10,21),new Color(.28f,.52f,.4f));
            var end=Plain(storyReturn,"Return arrow tip",new Vector2(0,5),new Vector2(18,18),new Color(.28f,.52f,.4f));end.rectTransform.localRotation=Quaternion.Euler(0,0,45);
            for(var i=0;i<2;i++){
                var r=ZooObject("Ball hiding leaves "+i);storyPlants[i]=r;
                Panel(r,"Leaf mat",new Vector2(0,-6),new Vector2(138,22),new Color(.42f,.54f,.29f,.4f),false,true);
                for(var k=0;k<3;k++){
                    var leaf=Panel(r,"Rounded leaf "+k,new Vector2((k-1)*35,26+k%2*19),new Vector2(42,73),new Color(.47f+k*.035f,.65f+k*.025f,.34f),false,true);
                    leaf.rectTransform.localRotation=Quaternion.Euler(0,0,(k-1)*23);
                }
            }
            storyBall=ZooObject("Elephant shared story ball");BallPicture(storyBall,Vector2.zero,48);
            // A retained foreground leaf covers one edge, leaving the colored
            // ball plainly visible instead of making an invisible hotspot.
            storyLeafCover=Panel(storyBall,"Ball hiding leaf edge",new Vector2(-20,0),new Vector2(24,52),new Color(.51f,.68f,.36f),false,true).rectTransform;
            storyLeafCover.localRotation=Quaternion.Euler(0,0,-13);
            HomeHit(storyBall,"Collect elephant ball",Vector2.zero,new Vector2(150,130),()=>{
                if(StoryInputReady){var s=Zoo.story;if(s.phase==ElephantStoryPhase.Searching)ZooWalk("story-pickup",s.session.ToString(),new Vector2(s.x,100));}
            });
            storyClue=Rect(zooAnimals["elephant"].transform.parent,"Elephant ball clue",new Vector2(130,385),Vector2.zero);
            Panel(storyClue,"Picture bubble",Vector2.zero,new Vector2(127,107),Cream,false,true);BallPicture(storyClue,Vector2.zero,68);
            storyPut=Panel(safe,"Put elephant ball down",Vector2.zero,Vector2.one*82,Cream,true,true).rectTransform;
            BallPicture(storyPut,new Vector2(0,10),39);
            var putArrow=Rect(storyPut,"Put down arrow",new Vector2(0,-19),Vector2.zero);ZooArrow(putArrow,Vector2.zero,-1);putArrow.localRotation=Quaternion.Euler(0,0,90);
            NavButton(storyPut.GetComponent<Image>(),()=>{if(StoryInputReady){zooApproach=false;destination=null;shared?.Walk(WalkMode.Stop);SendZoo("story-drop",Zoo.story.session.ToString(),_=>{});}});
        }
        private void TickElephantStory(ZooState z,ZooAnimal a,bool shown)
        {
            if(storyBasket==null)return;var s=z.story;
            storyBasket.gameObject.SetActive(shown);storyBasket.anchoredPosition=ToBoard(ElephantStory.BasketX,ElephantStory.BasketY);storyBasket.localScale=Vector3.one*sceneScale;
            storyInvitation.gameObject.SetActive(s.phase==ElephantStoryPhase.Available || s.phase==ElephantStoryPhase.Complete && s.resetAge>=ElephantStory.ReplayDelay);
            storyReturn.gameObject.SetActive(s.phase==ElephantStoryPhase.Carried || s.phase==ElephantStoryPhase.Searching);
            storyReturnRim.color=s.carrier==Actor?new Color(.97f,.85f,.52f,.85f):new Color(.97f,.85f,.52f,.55f);
            storyClue.gameObject.SetActive(shown && a.phase==ZooPhase.StoryClue);
            storyPut.gameObject.SetActive(s.carrier==Actor && CurrentArea==ZooLayout.Savanna && !MenuOpen && !ZooMapOpen && !ZooPhotoOpen && !ElephantSnackOpen);
            storyPut.anchoredPosition=new Vector2(safe.rect.width/2-70,safe.rect.height/2-181);
            for(var i=0;i<2;i++){
                var spot=ElephantStory.Spot(i);storyPlants[i].gameObject.SetActive(shown);storyPlants[i].anchoredPosition=ToBoard(spot.X,spot.Y);storyPlants[i].localScale=Vector3.one*sceneScale;
            }
            storyBall.gameObject.SetActive(shown && s.phase!=ElephantStoryPhase.Available);
            storyLeafCover.gameObject.SetActive(s.phase==ElephantStoryPhase.Searching);
            storyBall.localScale=Vector3.one*sceneScale;
            var point=ToBoard(ElephantStory.BasketX,ElephantStory.BasketY)+new Vector2(0,33)*sceneScale;
            if(s.phase==ElephantStoryPhase.Searching)point=ToBoard(s.x,s.y)+new Vector2(23,26)*sceneScale;
            else if(s.phase==ElephantStoryPhase.Carried){var p=ReadPlayer(s.carrier);var pose=Shared?shared.VisualPosition(s.carrier):new Vector2(p.x,p.y);point=ToBoard(pose.x,pose.y)+new Vector2(55,55)*sceneScale;}
            else if(s.phase==ElephantStoryPhase.Reacting && a.phase==ZooPhase.StoryReact){
                var age=(float)a.age;
                if(zooSamples.TryGetValue("elephant",out var sample) && Shared)age+=Mathf.Max(0,Time.realtimeSinceStartup-sample.sampled);
                var tip=StoryTrunkPoint+new Vector2(23,0)*sceneScale;
                // The ball touches the deformed original trunk, then rolls on
                // one bounded authored arc and settles back in its basket.
                if(age<.55f)point=Vector2.Lerp(point,tip,Mathf.Clamp01(age/.15f));
                else point+=new Vector2(Mathf.Sin(Mathf.Clamp01((age-.55f)/1.25f)*Mathf.PI)*62,0)*sceneScale;
            }
            storyBall.anchoredPosition=point;
            storyBall.localRotation=Quaternion.Euler(0,0,s.phase==ElephantStoryPhase.Reacting?Mathf.Sin((float)a.age*3)*30:0);
            // Seed observations after reconnect/visit changes; no old sound.
            if(storyObserved==s.session && !storyWasReacting && s.phase==ElephantStoryPhase.Reacting && s.session!=storySoundSession && shown && !applicationPaused){ZooPlay("elephant",true);storySoundSession=s.session;}
            storyObserved=s.session;storyWasReacting=s.phase==ElephantStoryPhase.Reacting;
        }
        private void ResetStoryObservation(){storyObserved=-1;storyWasReacting=false;}
        private void ResetElephantStory(){if(storyPut!=null)Destroy(storyPut.gameObject);storyPut=null;storyBasket=null;storyBall=null;storyClue=null;storySoundSession=-1;ResetStoryObservation();}
    }
}
