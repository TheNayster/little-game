using System;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LittleWeeps.Client
{
 public sealed partial class GameScreen
 {
  // Original editable drawings, with named parts for animation. No texture
  // copies, player placement, carried trays or per-frame object allocation.
  private Image ZooOval(Transform p,string name,Vector2 at,Vector2 size,Color color)
  {
   var edge=Panel(p,name+" outline",at,size+Vector2.one*5,Ink,false,true);
   Panel(edge.transform,name,Vector2.zero,size,color,false,true);return edge;
  }
  private RectTransform ZooTwig(Transform p,string name,Vector2 from,Vector2 to,float width,Color color)
  {
   var delta=to-from;var twig=Plain(p,name,(from+to)/2,new Vector2(width,delta.magnitude),color).rectTransform;twig.localRotation=Quaternion.Euler(0,0,-Mathf.Atan2(delta.x,delta.y)*Mathf.Rad2Deg);return twig;
  }
  private RectTransform ZooLeafPivot(Transform parent,Vector2 at,float scale,float angle)
  {
   var root=Rect(parent,"Moving leaf",at,Vector2.zero);ActivityLeaf(root,Vector2.zero,scale);root.localRotation=Quaternion.Euler(0,0,angle);return root;
  }
  private void DrawActivitySymbol(Transform p,ZooSpecies info)
  {
   if(info.id=="zebra"){DrawElephantBrush(p,Vector2.zero,.6f);}
   else if(info.id=="gecko"){ActivityLeaf(p,Vector2.zero,.8f);ZooOval(p,"Warm sun",new Vector2(14,14),Vector2.one*17,new Color(.97f,.73f,.32f));}
   else if(info.PlayKind=="browse")ActivityLeaf(p,Vector2.zero,.75f);
   else if(info.PlayKind=="toy"){ZooOval(p,"Roll picture",Vector2.zero,new Vector2(35,25),new Color(.8f,.61f,.35f));ZooArrow(p,new Vector2(0,22),1);p.GetChild(p.childCount-1).localScale=Vector3.one*.5f;}
   else{ZooOval(p,"Water drop",Vector2.zero,new Vector2(23,32),new Color(.28f,.65f,.81f));var tip=Plain(p,"Drop tip",new Vector2(0,12),Vector2.one*16,new Color(.28f,.65f,.81f));tip.rectTransform.localRotation=Quaternion.Euler(0,0,45);}
  }
  private void DrawHabitatActivity(ZooActivityView v,ZooSpecies info)
  {
   var p=v.playMotion;var wood=new Color(.65f,.46f,.28f);var green=new Color(.4f,.63f,.31f);var id=info.id;
   Panel(p,"Habitat shadow",new Vector2(52,-6),new Vector2(186,20),new Color(.23f,.35f,.22f,.18f),false,true);
   switch(id){
    case "giraffe":case "brachiosaurus":
     ZooOval(p,"Rooted acacia base",new Vector2(65,12),new Vector2(140,34),new Color(.72f,.66f,.45f));
     ZooTwig(p,"Rooted browse branch",new Vector2(58,16),new Vector2(80,id=="giraffe"?246:296),15,wood);
     for(var i=0;i<3;i++)ZooTwig(p,"Branch fork",new Vector2(75,150+i*35),new Vector2(i%2==0?135:30,(id=="giraffe"?194:244)+i*30),10,wood);
     for(var i=0;i<9;i++)v.foliage.Add(ZooLeafPivot(p,new Vector2(37+i%3*43,(id=="giraffe"?179:229)+i/3*30),id=="giraffe"?1.1f:1.3f,(i%2==0?-1:1)*18));
     break;
    case "zebra":
     ZooOval(p,"Scratch post foot",new Vector2(55,12),new Vector2(146,34),new Color(.69f,.6f,.43f));
     ZooOval(p,"Smooth scratch log",new Vector2(55,69),new Vector2(135,91),wood);
     ZooOval(p,"Log end grain",new Vector2(117,70),new Vector2(30,82),new Color(.88f,.7f,.46f));
     for(var i=0;i<5;i++)ZooTwig(p,"Bark groove",new Vector2(2+i*23,34),new Vector2(7+i*23,106),3,new Color(.42f,.32f,.23f));
     for(var i=0;i<3;i++)ZooOval(p,"Growth ring",new Vector2(117,70),new Vector2(23-i*6,66-i*18),i%2==0?wood:new Color(.88f,.7f,.46f));break;
    case "triceratops":case "stegosaurus":
     ZooOval(p,"Soft earth",new Vector2(65,12),new Vector2(177,35),new Color(.72f,.64f,.43f));
     for(var i=0;i<7;i++){
      var stem=Rect(p,"Rooted fern frond",new Vector2(5+i*20,25),Vector2.zero);v.foliage.Add(stem);
      ZooTwig(stem,"Fern stalk",Vector2.zero,new Vector2((i-3)*9,75+i%3*16),5,green);
      for(var j=0;j<5;j++)ActivityLeaf(stem,new Vector2((j%2==0?-1:1)*14+(i-3)*j*1.5f,19+j*13),.5f);
     }break;
    case "lion":case "tyrannosaurus":
     ZooOval(p,id=="lion"?"Bound rope roller":"Scented wooden log",new Vector2(65,55),new Vector2(143,74),new Color(.8f,.63f,.39f));
     ZooOval(p,"Roll end",new Vector2(127,55),new Vector2(27,66),new Color(.94f,.8f,.54f));
     for(var i=0;i<5;i++)ZooTwig(p,id=="lion"?"Rope winding":"Scent groove",new Vector2(12+i*22,26),new Vector2(4+i*22,83),4,id=="lion"?Cream:wood);
     if(id=="tyrannosaurus")for(var i=0;i<3;i++)ActivityLeaf(p,new Vector2(28+i*27,86),.6f);
     break;
    case "clownfish":
     ZooOval(p,"Anemone rock",new Vector2(60,20),new Vector2(147,45),new Color(.57f,.68f,.72f));
     for(var i=0;i<9;i++){
      var tentacle=Rect(p,"Anemone waving arm",new Vector2(12+i*12,30),Vector2.zero);v.foliage.Add(tentacle);
      ZooOval(tentacle,"Anemone arm",new Vector2(0,26+i%3*8),new Vector2(16,70+i%3*16),new Color(.88f,.64f,.75f));ZooOval(tentacle,"Anemone tip",new Vector2(0,64+i%3*16),Vector2.one*19,new Color(.98f,.8f,.83f));
     }break;
    case "blue-tang":
     ZooOval(p,"Reef rock",new Vector2(58,20),new Vector2(155,51),new Color(.62f,.7f,.74f));
     for(var i=0;i<5;i++){
      var fan=Rect(p,"Soft coral fan",new Vector2(12+i*23,40),Vector2.zero);v.foliage.Add(fan);
      ZooTwig(fan,"Coral stem",Vector2.zero,new Vector2(0,67+i%2*25),6,new Color(.87f,.59f,.67f));
      for(var k=0;k<3;k++)ZooTwig(fan,"Fan branch",new Vector2(0,20+k*14),new Vector2((i%2==0?-1:1)*(22+k*5),36+k*19),5,new Color(.94f,.7f,.72f));
     }break;
    case "zebra-shark":
     ZooOval(p,"Sand ripple bank",new Vector2(58,15),new Vector2(189,43),new Color(.9f,.82f,.62f));
     for(var i=0;i<4;i++)ZooTwig(p,"Sand ripple",new Vector2(-13+i*19,13+i*3),new Vector2(82+i*11,15+i*3),3,new Color(.72f,.66f,.5f));
     DrawShell(p,new Vector2(61,44),.95f,false);for(var i=0;i<3;i++)v.foliage.Add(ZooLeafPivot(p,new Vector2(116+i*13,35+i*14),.7f,50));break;
    case "penguin":
     ZooOval(p,"Pool stone rim",new Vector2(63,28),new Vector2(188,70),new Color(.65f,.69f,.65f));
     ZooOval(p,"Pool water",new Vector2(63,34),new Vector2(160,43),new Color(.38f,.72f,.84f));
     for(var i=0;i<3;i++)ZooOval(p,"Pool ripple",new Vector2(27+i*32,35),new Vector2(26,5),new Color(.8f,.94f,.96f));break;
    case "gecko":
     ZooOval(p,"Warm layered rock",new Vector2(60,29),new Vector2(166,63),new Color(.75f,.66f,.46f));ZooOval(p,"Warm rock face",new Vector2(47,53),new Vector2(119,38),new Color(.88f,.78f,.55f));
     ZooTwig(p,"Shade twig",new Vector2(113,20),new Vector2(125,112),7,wood);v.foliage.Add(ZooLeafPivot(p,new Vector2(65,114),2.2f,-25));break;
    case "tortoise":
     ZooOval(p,"Garden soil",new Vector2(62,12),new Vector2(163,35),new Color(.73f,.66f,.45f));for(var i=0;i<6;i++)v.foliage.Add(ZooLeafPivot(p,new Vector2(11+i*21,27+i%2*15),1,35));DrawMistNozzle(p);break;
    case "iguana":
     ZooOval(p,"Basking ledge",new Vector2(63,27),new Vector2(172,56),new Color(.73f,.69f,.5f));ZooTwig(p,"Rooted climbing vine",new Vector2(0,33),new Vector2(117,115),10,wood);
     for(var i=0;i<6;i++)v.foliage.Add(ZooLeafPivot(p,new Vector2(10+i*20,44+i*14),.9f,i%2*40));DrawMistNozzle(p);break;
    case "crocodile":
     ZooOval(p,"Bank stone",new Vector2(55,29),new Vector2(182,57),new Color(.66f,.67f,.46f));ZooOval(p,"Bank water edge",new Vector2(77,8),new Vector2(180,24),new Color(.43f,.72f,.76f));
     for(var i=0;i<4;i++){var reed=Rect(p,"Bank reed",new Vector2(105+i*10,29),Vector2.zero);v.foliage.Add(reed);ZooTwig(reed,"Reed stalk",Vector2.zero,new Vector2(4,57+i%2*19),5,green);ZooOval(reed,"Reed seed head",new Vector2(4,65+i%2*19),new Vector2(11,25),wood);}DrawMistNozzle(p);break;
   }
   for(var i=0;i<v.effects.Length;i++){
    var effect=v.effects[i]=Rect(v.effectsRoot,"Play effect "+i,Vector2.zero,Vector2.zero);
    if(info.PlayKind=="browse" && id!="zebra")ActivityLeaf(effect,Vector2.zero,.45f);
    else if(info.PlayKind=="current"){Panel(effect,"Bubble outer",Vector2.zero,Vector2.one*(12+i%3*5),new Color(.85f,.97f,1,.9f),false,true);Panel(effect,"Bubble centre",Vector2.zero,Vector2.one*(7+i%3*5),new Color(.53f,.8f,.89f,.5f),false,true);Panel(effect,"Bubble glint",new Vector2(-3,3),Vector2.one*3,Cream,false,true);}
    else if(info.PlayKind=="toy" || id=="zebra" || id=="gecko"){ZooTwig(effect,"Play sparkle",new Vector2(-6,0),new Vector2(6,0),3,new Color(.99f,.86f,.53f));ZooTwig(effect,"Play sparkle upright",new Vector2(0,-7),new Vector2(0,7),3,Cream);}
    else Panel(effect,"Gentle droplet",Vector2.zero,new Vector2(10,16),new Color(.77f,.94f,.99f,.9f),false,true);
   }
  }
  private void DrawMistNozzle(Transform p)
  {
   ZooTwig(p,"Fixed mist pipe",new Vector2(148,11),new Vector2(148,82),9,new Color(.36f,.61f,.61f));ZooOval(p,"Mist rose",new Vector2(135,83),new Vector2(31,21),new Color(.58f,.77f,.72f));for(var i=0;i<3;i++)Panel(p,"Rose hole",new Vector2(125+i*8,84),Vector2.one*3,Ink,false,true);
  }
  private void AnimateHabitat(ZooActivityView v,ZooSpecies info,float age,bool active,ZooAnimal animal,double animalAge)
  {
   var pulse=active?Mathf.Sin(Mathf.Clamp01(age/1.8f)*Mathf.PI):0;var kind=info.PlayKind;
   v.playMotion.anchoredPosition=new Vector2(kind=="toy"?(info.id=="lion"?110*pulse:Mathf.Sin(age*4)*23*pulse):0,info.id=="lion"?-25*pulse:0);v.playMotion.localRotation=Quaternion.Euler(0,0,kind=="toy"?Mathf.Sin(age*4)*14*pulse:0);
   for(var i=0;i<v.foliage.Count;i++)v.foliage[i].localRotation=Quaternion.Euler(0,0,Mathf.Sin((float)animalAge*1.7f+i)*2+(active?(info.id=="gecko"?-29:Mathf.Sin(age*7+i)*13)*pulse:0));
   for(var i=0;i<v.effects.Length;i++){
    var effect=v.effects[i];effect.gameObject.SetActive(active);if(!active)continue;var t=Mathf.Repeat(age*.85f+i*.071f,1);
    if(kind=="browse")effect.anchoredPosition=new Vector2(25+i%4*25+Mathf.Sin(t*5+i)*16,145-t*88);
    else if(kind=="current")effect.anchoredPosition=new Vector2(32+i%4*22+Mathf.Sin(t*5+i)*18,48+t*139);
    else if(kind=="toy" || info.id=="zebra" || info.id=="gecko")effect.anchoredPosition=new Vector2(4+i%4*35,37+Mathf.Sin(t*Mathf.PI)*63);
    else if(kind=="water")effect.anchoredPosition=new Vector2(63+(i-5.5f)*12*t,36+Mathf.Sin(t*Mathf.PI)*80);
    else effect.anchoredPosition=Vector2.Lerp(new Vector2(134,83),new Vector2(4+i*10,30),t)+new Vector2(0,Mathf.Sin(t*Mathf.PI)*31);
    effect.localScale=Vector3.one*Mathf.Clamp01(Mathf.Sin(t*Mathf.PI)*1.3f);effect.localRotation=Quaternion.Euler(0,0,kind=="browse"?t*90:0);
   }
  }
  private Vector2 CarePatchPoint(ZooSpecies info,float size,int patch)
  {
   if(info.HabitatCare)return new Vector2((patch-1)*65,85);
   // Each set stays on the body/shell, clear of eyes and feeding sockets.
   switch(info.id){
    case "giraffe":return new Vector2((patch-1)*size*.11f,info.footOffset+size*(patch==1?.32f:.24f));
    case "zebra":return new Vector2((patch-1)*size*.17f,info.footOffset+size*.31f);
    case "brachiosaurus":return new Vector2((patch-1)*size*.15f,info.footOffset+size*.27f);
    case "triceratops":case "stegosaurus":return new Vector2((patch-1)*size*.16f,info.footOffset+size*.31f);
    case "crocodile":return new Vector2((patch-1)*size*.16f,info.footOffset+size*(patch==1?.51f:.48f));
    case "penguin":return new Vector2((patch-1)*size*.08f,info.footOffset+size*(patch==1?.48f:.37f));
    case "tortoise":return new Vector2((patch-1)*size*.13f,info.footOffset+size*(patch==1?.41f:.34f));
    default:return new Vector2((patch-1)*size*.16f,info.footOffset+size*.31f);
   }
  }
  private void DrawCareHabitat(Transform p,ZooSpecies info)
  {
   if(info.habitat==ZooHabitat.Tank){
    Panel(p,"Glass edge",new Vector2(0,86),new Vector2(236,143),new Color(.34f,.58f,.65f),false);Panel(p,"Glass face",new Vector2(0,86),new Vector2(226,133),new Color(.67f,.87f,.92f,.28f),false);
    ZooTwig(p,"Glass reflection",new Vector2(-88,61),new Vector2(-48,118),5,new Color(.85f,.97f,1,.75f));ZooTwig(p,"Glass reflection short",new Vector2(58,47),new Vector2(93,96),3,new Color(.85f,.97f,1,.65f));
    Panel(p,"Glass lower rim",new Vector2(0,14),new Vector2(246,15),new Color(.49f,.65f,.62f),false,true);
   }else{
    ZooOval(p,"Habitat stone base",new Vector2(0,31),new Vector2(237,60),new Color(.58f,.61f,.43f));ZooOval(p,"Flat habitat stone",new Vector2(0,76),new Vector2(220,107),new Color(.8f,.74f,.55f));
    ZooTwig(p,"Stone grain",new Vector2(-72,102),new Vector2(-20,112),4,new Color(.65f,.59f,.45f));ZooTwig(p,"Stone grain lower",new Vector2(30,58),new Vector2(81,64),3,new Color(.65f,.59f,.45f));ActivityLeaf(p,new Vector2(-99,22),.6f);
   }
  }
  private RectTransform CareSpark(Transform p,int i,bool rinse)
  {
   var r=Rect(p,"Cleaning effect "+i,Vector2.zero,Vector2.zero);
   if(rinse)Panel(r,"Rinse droplet",Vector2.zero,new Vector2(5,9),new Color(.66f,.91f,.96f),false,true);
   else{Plain(r,"Shine across",Vector2.zero,new Vector2(13,3),Cream);Plain(r,"Shine upright",Vector2.zero,new Vector2(3,13),Cream);}return r;
  }
  private void DrawCareFinish(Transform p)
  {
   for(var i=0;i<5;i++){var star=Rect(p,"Clean sparkle",new Vector2((i-2)*31,Mathf.Abs(i-2)*-8),Vector2.zero);Plain(star,"Shine across",Vector2.zero,new Vector2(18,4),new Color(.99f,.86f,.47f));Plain(star,"Shine upright",Vector2.zero,new Vector2(4,23),Cream);}
  }
  private void DrawDiscoveryHiding(ZooActivityView v,ZooSpecies info,int slot)
  {
   var p=v.discoveries[slot];Panel(p,"Discovery ground shade",new Vector2(0,-3),new Vector2(152,18),new Color(.26f,.35f,.24f,.18f),false,true);
   if(info.habitat==ZooHabitat.Tank){ZooOval(p,"Reef hiding rock",new Vector2(0,16),new Vector2(128,39),new Color(.61f,.69f,.73f));for(var i=0;i<5;i++){var weed=Rect(p,"Opening reef grass",new Vector2((i-2)*21,28),Vector2.zero);v.hiding[slot].Add(weed);ZooOval(weed,"Reef blade",new Vector2(0,23),new Vector2(13,61+i%2*15),new Color(.4f,.68f,.59f));}}
   else if(info.id=="giraffe" && slot==0){ZooTwig(p,"Nest branch",new Vector2(-58,25),new Vector2(63,45),10,new Color(.64f,.46f,.29f));for(var i=0;i<4;i++)v.hiding[slot].Add(ZooLeafPivot(p,new Vector2((i-1.5f)*30,49+i%2*13),1.15f,0));}
   else{
    ZooOval(p,"Discovery mossy stone",new Vector2(-25,14),new Vector2(88,33),new Color(.65f,.66f,.47f));ZooOval(p,"Small hiding stone",new Vector2(35,12),new Vector2(58,28),new Color(.76f,.73f,.53f));
    for(var i=0;i<5;i++){var cover=Rect(p,"Opening hiding foliage",new Vector2((i-2)*21,25),Vector2.zero);v.hiding[slot].Add(cover);ActivityLeaf(cover,new Vector2(0,17+i%2*8),1.2f);}
   }
  }
  private void DrawEye(Transform p,Vector2 at,float size=7){Panel(p,"Eye",at,Vector2.one*size,Ink,false,true);Panel(p,"Eye glint",at+new Vector2(-1,1),Vector2.one*(size*.28f),Cream,false,true);}
  private void DrawShell(Transform p,Vector2 at,float scale,bool pearl)
  {
   var r=Rect(p,"Illustrated shell",at,Vector2.zero);r.localScale=Vector3.one*scale;ZooOval(r,"Scallop shell",Vector2.zero,new Vector2(72,48),new Color(.94f,.79f,.61f));
   for(var i=0;i<5;i++)ZooTwig(r,"Shell rib",new Vector2(0,-20),new Vector2((i-2)*14,15+i%2*6),3,new Color(.72f,.54f,.43f));
   if(pearl){var lid=Rect(r,"Opening shell lid",Vector2.zero,Vector2.zero);ZooOval(lid,"Pearl shell lid",new Vector2(0,12),new Vector2(66,24),new Color(.86f,.65f,.64f));ZooOval(r,"Pearl",new Vector2(0,-4),Vector2.one*24,Cream);Panel(r,"Pearl glint",new Vector2(-5,2),Vector2.one*7,Color.white,false,true);}
  }
  private void DrawSnail(Transform p,bool water)
  {
   var green=water?new Color(.62f,.74f,.58f):new Color(.7f,.75f,.45f);ZooOval(p,"Snail foot",new Vector2(4,-18),new Vector2(79,18),green);ZooOval(p,"Snail head",new Vector2(34,-6),new Vector2(25,29),green);
   ZooOval(p,"Spiral shell",new Vector2(-7,3),Vector2.one*50,new Color(.9f,.74f,.49f));for(var i=0;i<4;i++)ZooOval(p,"Spiral coil",new Vector2(-7+i,3),Vector2.one*(39-i*9),i%2==0?new Color(.64f,.47f,.32f):new Color(.9f,.74f,.49f));
   for(var i=0;i<2;i++){ZooTwig(p,"Eye stalk",new Vector2(27+i*13,4),new Vector2(26+i*15,19),3,green);DrawEye(p,new Vector2(26+i*15,20),5);}
  }
  private void DrawBeetle(Transform p,Color color,bool amber=false)
  {
   if(amber)ZooOval(p,"Amber window",Vector2.zero,new Vector2(79,69),new Color(.96f,.75f,.34f,.75f));
   for(var i=0;i<6;i++)ZooTwig(p,"Beetle leg",new Vector2((i<3?-1:1)*13,(i%3-1)*10),new Vector2((i<3?-1:1)*29,(i%3-1)*19),4,Ink);
   ZooOval(p,"Beetle wing case",Vector2.zero,new Vector2(37,46),color);Plain(p,"Wing seam",new Vector2(0,-3),new Vector2(3,34),Ink);ZooOval(p,"Beetle head",new Vector2(0,23),new Vector2(23,20),color);DrawEye(p,new Vector2(-6,26),5);DrawEye(p,new Vector2(6,26),5);
   ZooTwig(p,"Antenna left",new Vector2(-6,30),new Vector2(-17,40),3,Ink);ZooTwig(p,"Antenna right",new Vector2(6,30),new Vector2(17,40),3,Ink);
  }
  private void DrawFern(Transform p,bool cone)
  {
   ZooTwig(p,"Fern stem",new Vector2(0,-24),new Vector2(0,51),4,new Color(.35f,.55f,.27f));
   for(var i=0;i<7;i++)ActivityLeaf(p,new Vector2((i%2==0?-1:1)*15,-13+i*9),.55f);
   if(cone){ZooOval(p,"Seed cone",new Vector2(0,35),new Vector2(32,45),new Color(.74f,.5f,.29f));for(var i=0;i<4;i++)ZooTwig(p,"Cone scale",new Vector2(-12,22+i*9),new Vector2(12,25+i*9),3,new Color(.93f,.72f,.44f));}
   else{ZooOval(p,"Fern curl",new Vector2(7,49),Vector2.one*20,new Color(.47f,.68f,.32f));ZooOval(p,"Fern curl centre",new Vector2(7,49),Vector2.one*9,new Color(.73f,.84f,.43f));}
  }
  private void DrawShrimp(Transform p,Color color)
  {
   for(var i=0;i<5;i++)ZooOval(p,"Shrimp segment",new Vector2((i-2)*11,Mathf.Sin(i*.6f)*10),new Vector2(17,24-i),color);
   ZooOval(p,"Shrimp tail",new Vector2(-33,-3),new Vector2(24,16),color);for(var i=0;i<6;i++)ZooTwig(p,"Shrimp leg",new Vector2(-24+i*8,-4),new Vector2(-28+i*8,-19),3,color);
   for(var i=0;i<2;i++)ZooTwig(p,"Long antenna",new Vector2(24,9+i*6),new Vector2(63,17+i*20),2,Ink);DrawEye(p,new Vector2(26,15),6);
  }
  private void DrawStarfish(Transform p,Color color)
  {
   for(var i=0;i<5;i++){var arm=Rect(p,"Star arm",Vector2.zero,Vector2.zero);arm.localRotation=Quaternion.Euler(0,0,i*72);ZooOval(arm,"Sea star arm",new Vector2(0,21),new Vector2(16,47),color);for(var j=0;j<3;j++)Panel(arm,"Star spot",new Vector2(0,9+j*12),Vector2.one*4,Cream,false,true);}ZooOval(p,"Sea star centre",Vector2.zero,Vector2.one*27,color);
  }
  private void DrawZooDiscovery(RectTransform p,string id,int slot)
  {
   // Explicit species/slot selection: an insect cannot accidentally become a
   // plant, nor can a lizard be rendered by a generic six-leg fallback.
   switch(id){
    case "giraffe":
     if(slot==0){
      ZooOval(p,"Woven weaver nest",new Vector2(-6,-13),new Vector2(77,51),new Color(.81f,.63f,.35f));for(var i=0;i<5;i++)ZooTwig(p,"Nest weave",new Vector2(-37,-24+i*7),new Vector2(25,-20+i*7),3,new Color(.53f,.41f,.25f));ZooOval(p,"Nest opening",new Vector2(12,-11),Vector2.one*23,new Color(.4f,.35f,.23f));
      var bird=Rect(p,"Weaver bird",new Vector2(-8,18),Vector2.zero);ZooOval(bird,"Golden bird body",Vector2.zero,new Vector2(41,38),new Color(.97f,.79f,.32f));var wing=Rect(bird,"Bird wing",new Vector2(-10,-1),Vector2.zero);ZooOval(wing,"Wing",Vector2.zero,new Vector2(23,25),new Color(.64f,.5f,.27f));ZooOval(bird,"Bird head",new Vector2(13,16),Vector2.one*25,new Color(.97f,.81f,.38f));DrawEye(bird,new Vector2(18,20),6);ZooTwig(bird,"Bird beak",new Vector2(24,13),new Vector2(35,11),7,new Color(.77f,.48f,.22f));
     }else{
      ZooTwig(p,"Seed pod stalk",new Vector2(-30,-19),new Vector2(29,22),5,new Color(.57f,.42f,.25f));
      for(var i=0;i<2;i++){var pod=Rect(p,"Opening seed pod "+i,new Vector2(i==0?-22:23,i==0?0:13),Vector2.zero);ZooOval(pod,"Pod shell left",new Vector2(-10,0),new Vector2(24,53),new Color(.82f,.57f,.28f));ZooOval(pod,"Pod shell right",new Vector2(10,0),new Vector2(24,53),new Color(.93f,.72f,.39f));for(var j=0;j<3;j++)ZooOval(pod,"Round seed",new Vector2(0,(j-1)*13),Vector2.one*10,new Color(.49f,.36f,.25f));}ActivityLeaf(p,new Vector2(25,43),.7f);
     }break;
    case "zebra":
     if(slot==0){
      ZooOval(p,"Grasshopper body",Vector2.zero,new Vector2(55,22),new Color(.56f,.74f,.31f));ZooOval(p,"Grasshopper head",new Vector2(27,7),Vector2.one*22,new Color(.66f,.8f,.36f));DrawEye(p,new Vector2(32,12));
      for(var i=0;i<2;i++){ZooTwig(p,"Jumping thigh",new Vector2(-8,0),new Vector2(-30,i==0?27:-20),6,new Color(.4f,.59f,.26f));ZooTwig(p,"Jumping foot",new Vector2(-30,i==0?27:-20),new Vector2(-47,-23+i*6),4,new Color(.4f,.59f,.26f));ZooTwig(p,"Grasshopper antenna",new Vector2(26,16),new Vector2(45,34+i*10),2,Ink);}
     }else for(var i=0;i<3;i++){var feather=Rect(p,"Striped feather",new Vector2((i-1)*25,3+i%2*9),Vector2.zero);feather.localRotation=Quaternion.Euler(0,0,(i-1)*24);ZooOval(feather,"Cream feather",Vector2.zero,new Vector2(18,65),Cream);ZooTwig(feather,"Feather shaft",new Vector2(0,-39),new Vector2(0,27),3,Ink);for(var j=0;j<5;j++)Plain(feather,"Feather stripe",new Vector2(0,-22+j*11),new Vector2(17,4),new Color(.38f,.4f,.35f));}break;
    case "lion":
     if(slot==0){
      ZooOval(p,"Little lizard body",Vector2.zero,new Vector2(48,24),new Color(.69f,.78f,.36f));ZooOval(p,"Lizard head",new Vector2(28,7),new Vector2(28,23),new Color(.78f,.84f,.46f));
      ZooTwig(p,"Lizard long tail",new Vector2(-21,0),new Vector2(-54,-11),8,new Color(.69f,.78f,.36f));for(var i=0;i<4;i++)ZooTwig(p,"Four lizard feet",new Vector2((i<2?-1:1)*13,0),new Vector2((i<2?-1:1)*24,(i%2==0?-1:1)*22),5,new Color(.59f,.68f,.32f));DrawEye(p,new Vector2(33,12));
     }else DrawBeetle(p,new Color(.91f,.72f,.29f));break;
    case "brachiosaurus":if(slot==0)DrawFern(p,false);else{for(var i=0;i<3;i++){var crystal=Rect(p,"Amber crystal",new Vector2((i-1)*26,i==1?8:0),Vector2.zero);ZooOval(crystal,"Honey amber",Vector2.zero,new Vector2(30,48),new Color(.97f,.72f,.3f));ZooTwig(crystal,"Amber glint",new Vector2(-5,-9),new Vector2(-2,14),4,Cream);}}break;
    case "triceratops":if(slot==0)DrawFern(p,true);else DrawSnail(p,false);break;
    case "stegosaurus":
     if(slot==0){for(var i=0;i<4;i++){var wing=Rect(p,"Dragonfly wing "+i,new Vector2((i<2?-1:1)*21,(i%2==0?-1:1)*11),Vector2.zero);ZooOval(wing,"Translucent wing",Vector2.zero,new Vector2(49,17),new Color(.75f,.91f,.93f));ZooTwig(wing,"Wing vein",new Vector2(-17,0),new Vector2(17,0),2,new Color(.47f,.69f,.7f));}ZooOval(p,"Dragonfly body",Vector2.zero,new Vector2(12,61),new Color(.41f,.67f,.66f));DrawEye(p,new Vector2(-5,28),6);DrawEye(p,new Vector2(5,28),6);}
     else{ActivityLeaf(p,Vector2.zero,2);for(var i=0;i<4;i++)ZooTwig(p,"Leaf pattern vein",new Vector2(-15+i*13,0),new Vector2(-28+i*13,15),2,new Color(.81f,.9f,.5f));}break;
    case "tyrannosaurus":
     if(slot==0){ZooOval(p,"Footprint pebble",Vector2.zero,new Vector2(89,62),new Color(.81f,.73f,.56f));ZooOval(p,"Footprint pad",new Vector2(0,-5),new Vector2(30,26),new Color(.48f,.46f,.36f));for(var i=0;i<3;i++)ZooOval(p,"Three fossil toes",new Vector2((i-1)*16,14),new Vector2(12,28),new Color(.48f,.46f,.36f));}
     else DrawBeetle(p,new Color(.65f,.45f,.24f),true);break;
    case "clownfish":if(slot==0)DrawShrimp(p,new Color(.96f,.72f,.62f));else DrawShell(p,Vector2.zero,1,true);break;
    case "blue-tang":if(slot==0)DrawStarfish(p,new Color(.91f,.66f,.48f));else DrawShrimp(p,new Color(.87f,.51f,.55f));break;
    case "zebra-shark":if(slot==0)DrawShell(p,Vector2.zero,1.15f,false);else DrawStarfish(p,new Color(.94f,.79f,.55f));break;
    case "penguin":
     if(slot==0){for(var i=0;i<6;i++)ZooTwig(p,"Crab leg",new Vector2((i<3?-1:1)*20,(i%3-1)*5),new Vector2((i<3?-1:1)*37,(i%3-1)*16),4,new Color(.77f,.46f,.31f));ZooOval(p,"Rock crab body",Vector2.zero,new Vector2(54,33),new Color(.92f,.64f,.42f));for(var i=0;i<2;i++){ZooTwig(p,"Crab claw arm",new Vector2((i==0?-1:1)*20,5),new Vector2((i==0?-1:1)*37,27),5,new Color(.77f,.46f,.31f));ZooOval(p,"Crab claw",new Vector2((i==0?-1:1)*39,31),new Vector2(22,25),new Color(.94f,.7f,.48f));DrawEye(p,new Vector2((i==0?-1:1)*12,20));}}
     else DrawShell(p,Vector2.zero,1,false);break;
    case "tortoise":if(slot==0)DrawSnail(p,false);else{ZooOval(p,"Seedling earth",new Vector2(0,-21),new Vector2(53,20),new Color(.66f,.48f,.3f));ZooTwig(p,"Seedling stalk",new Vector2(0,-17),new Vector2(0,28),5,new Color(.4f,.61f,.3f));ActivityLeaf(p,new Vector2(-16,14),.8f);ActivityLeaf(p,new Vector2(18,26),.8f);}break;
    case "gecko":if(slot==0)DrawBeetle(p,new Color(.62f,.66f,.48f));else{for(var i=0;i<3;i++){var r=Rect(p,"Rock crystal",new Vector2((i-1)*22,i==1?8:0),Vector2.zero);r.localRotation=Quaternion.Euler(0,0,(i-1)*17);ZooOval(r,"Violet crystal",Vector2.zero,new Vector2(25,57),new Color(.72f,.75f,.89f));ZooTwig(r,"Crystal facet",new Vector2(-5,-18),new Vector2(-5,19),3,Cream);}}break;
    case "iguana":
     if(slot==0){for(var i=0;i<6;i++)ZooTwig(p,"Six leaf insect legs",new Vector2((i<3?-1:1)*9,(i%3-1)*10),new Vector2((i<3?-1:1)*27,(i%3-1)*22),4,new Color(.36f,.55f,.27f));ZooOval(p,"Leaf insect body",Vector2.zero,new Vector2(34,53),new Color(.61f,.78f,.35f));ZooTwig(p,"Leaf insect vein",new Vector2(0,-21),new Vector2(0,22),3,new Color(.4f,.57f,.25f));ZooOval(p,"Leaf insect head",new Vector2(0,29),new Vector2(20,19),new Color(.65f,.8f,.4f));DrawEye(p,new Vector2(-6,32),5);DrawEye(p,new Vector2(6,32),5);}
     else{ZooOval(p,"Seed pod",Vector2.zero,new Vector2(34,62),new Color(.78f,.57f,.32f));for(var i=0;i<4;i++)ZooOval(p,"Pod seed",new Vector2(0,-20+i*13),Vector2.one*9,new Color(.96f,.81f,.5f));ActivityLeaf(p,new Vector2(17,31),.65f);}break;
    case "crocodile":
     if(slot==0){ZooOval(p,"Reed frog body",Vector2.zero,new Vector2(48,35),new Color(.54f,.73f,.36f));for(var i=0;i<4;i++){var side=i<2?-1:1;ZooOval(p,"Four frog feet",new Vector2(side*(i%2==0?29:22),i%2==0?-13:11),new Vector2(23,13),new Color(.63f,.8f,.4f));}for(var i=0;i<2;i++){ZooOval(p,"Raised frog eye",new Vector2(i==0?-14:14,18),Vector2.one*17,new Color(.66f,.82f,.44f));DrawEye(p,new Vector2(i==0?-14:14,20));}ZooTwig(p,"Frog smile",new Vector2(-10,-2),new Vector2(10,-2),2,Ink);}
     else DrawSnail(p,true);break;
   }
  }
  private void AnimateZooDiscovery(ZooActivityView v,ZooSpecies info,int slot,float age,bool active)
  {
   var reveal=v.reveals[slot];reveal.gameObject.SetActive(active);var opening=active?Mathf.Sin(Mathf.Clamp01(age/.6f)*Mathf.PI/2)*Mathf.Clamp01((6-age)/.8f):0;
   reveal.anchoredPosition=new Vector2(Mathf.Sin(age*2+slot)*7*opening,35+opening*54);reveal.localRotation=Quaternion.Euler(0,0,active?Mathf.Sin(age*3)*4:0);reveal.localScale=Vector3.one*(info.id=="brachiosaurus" && slot==0?Mathf.Lerp(.5f,1,opening):1);
   for(var i=0;i<v.hiding[slot].Count;i++)v.hiding[slot][i].localRotation=Quaternion.Euler(0,0,(i-2)*opening*16+(active?Mathf.Sin(age*8+i)*3:Mathf.Repeat(Time.realtimeSinceStartup,16)<1?Mathf.Sin(Time.realtimeSinceStartup*5+i)*2:0));
   if(!active)return;
   foreach(var part in v.revealParts[slot]){
    if(part.name=="Bird wing")part.localRotation=Quaternion.Euler(0,0,Mathf.Sin(age*12)*18);
    else if(part.name.StartsWith("Dragonfly wing ",StringComparison.Ordinal))part.localScale=new Vector3(.7f+.3f*Mathf.Sin(age*16),1,1);
    else if(part.name=="Pod shell left outline")part.anchoredPosition=new Vector2(-10-opening*9,0);
    else if(part.name=="Pod shell right outline")part.anchoredPosition=new Vector2(10+opening*9,0);
    else if(part.name=="Opening shell lid")part.anchoredPosition=new Vector2(0,opening*16);
   }
   if(info.id=="zebra" && slot==0)reveal.anchoredPosition+=new Vector2(Mathf.Sin(age*2)*20,Mathf.Abs(Mathf.Sin(age*3))*19);
   if(info.habitat==ZooHabitat.Tank)reveal.anchoredPosition+=new Vector2(Mathf.Sin(age*2)*16,Mathf.Sin(age*3)*7);
  }
 }
}
