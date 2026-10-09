using System;
using System.Linq;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private const float BrachiosaurusSize=380;
        private readonly Image[] brachioPads=new Image[4],brachioBadges=new Image[4];
        private readonly RawImage[] brachioPortraits=new RawImage[4];
        private readonly RectTransform[] brachioPointers=new RectTransform[4],brachioDone=new RectTransform[4],brachioWaiting=new RectTransform[4];
        private readonly string[] brachioAvatars=new string[4];
        public string BrachiosaurusCue {get;private set;}="idle";
        public float BrachiosaurusMouthGap {get;private set;}
        private void BuildBrachiosaurusSpot(RectTransform rail,int i)
        {
            brachioPads[i]=Panel(rail,"Your standing picture",Vector2.zero,new Vector2(80,18),new Color(.76f,.69f,.53f,.24f),false,true);
            brachioBadges[i]=Panel(rail,"Brachiosaurus feeder portrait",new Vector2(0,380),Vector2.one*58,Cream,false,true);
            brachioPortraits[i]=Rect(brachioBadges[i].transform,"Child picture",Vector2.zero,Vector2.one*48).gameObject.AddComponent<RawImage>();brachioPortraits[i].raycastTarget=false;
            brachioPointers[i]=Rect(rail,"Your browse pointer",new Vector2(0,337),Vector2.zero);ZooArrow(brachioPointers[i],Vector2.zero,1);
            brachioDone[i]=Rect(brachioBadges[i].transform,"Food consumed picture",new Vector2(17,-19),Vector2.zero);FossilCheck(brachioDone[i],true);brachioDone[i].localScale=Vector3.one*.45f;
            brachioWaiting[i]=Rect(brachioBadges[i].transform,"Waiting for dinosaur picture",new Vector2(18,-18),Vector2.zero);Panel(brachioWaiting[i],"Turn clock",Vector2.zero,new Vector2(24,24),Cream,false,true);Plain(brachioWaiting[i],"Clock hand",new Vector2(0,3),new Vector2(3,9),Ink);Plain(brachioWaiting[i],"Clock hand",new Vector2(3,0),new Vector2(9,3),Ink);
        }
        private static float BrachiosaurusBend(ZooAnimal a,double age)
        {
            if(a.phase==ZooPhase.Eat)return 60+(a.consumed?0:Mathf.Sin((float)age*6)*1.2f);
            if(a.phase==ZooPhase.Approach)return 60*Mathf.SmoothStep(0,1,Mathf.Clamp01((float)(age/a.duration)-.65f)/.35f);
            return 0;
        }
        private Vector2 BrachiosaurusMouth()
        {
            var image=(BrachiosaurusArtView)zooAnimals["brachiosaurus"];var root=(RectTransform)image.transform.parent;
            return Board.InverseTransformPoint(root.TransformPoint(image.MouthTip));
        }
        private void TickBrachiosaurusSpots(ZooState z,bool shown)
        {
            var a=z.animals.First(v=>v.species=="brachiosaurus");var own=z.food.FirstOrDefault(v=>v.actor==Actor);BrachiosaurusCue=own?.species!="brachiosaurus"?"idle":!own.offered?"carrying":a.owner!=Actor?"waiting":a.consumed?"finished":a.phase==ZooPhase.Eat?"eating":"approaching";BrachiosaurusMouthGap=0;
            for(var i=0;i<4;i++){
                var f=z.food.FirstOrDefault(v=>v.species=="brachiosaurus" && v.slot==i);var local=f?.actor==Actor;var done=f!=null && a.owner==f.actor && a.consumed;
                brachioPads[i].color=local?new Color(.98f,.8f,.33f,.85f):f!=null?new Color(.63f,.75f,.55f,.5f):new Color(.76f,.69f,.53f,.24f);
                brachioBadges[i].gameObject.SetActive(f!=null);brachioPointers[i].gameObject.SetActive(local && !done);brachioDone[i].gameObject.SetActive(done);brachioWaiting[i].gameObject.SetActive(f!=null && f.offered && a.owner!=f.actor);
                if(f!=null && a.owner==f.actor && a.phase==ZooPhase.Eat && !a.consumed)BrachiosaurusMouthGap=Vector2.Distance(ZooHandFoodPoint(f.actor),BrachiosaurusMouth())/sceneScale;
                if(f==null)continue;var avatar=ReadPlayer(f.actor).avatar;
                if(brachioAvatars[i]!=avatar){brachioPortraits[i].texture=WorldResources.Load<CharacterMenuArt>("Shared/Characters/Menu/"+PlayableCharacters.Find(avatar).ArtId)?.texture;brachioAvatars[i]=avatar;}
                brachioBadges[i].color=done?new Color(.7f,.87f,.58f):local?new Color(1,.87f,.48f):Cream;
            }
        }
        private void ResetBrachiosaurus(){Array.Clear(brachioAvatars,0,brachioAvatars.Length);BrachiosaurusCue="idle";BrachiosaurusMouthGap=0;}
    }
}
