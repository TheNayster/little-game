using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LittleWeeps.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        private RectTransform zooPhotoButtons,zooPhotoOverlay,zooAlbumFrame,zooCameraFrame;
        private RawImage zooPhotoView,zooPhotoConfirmation;
        private Text zooPhotoMessage;
        private ZooPhotoAlbum zooAlbum;
        private readonly List<Texture2D> zooPhotoLoaded=new List<Texture2D>();
        private readonly List<GameObject> zooPhotoContent=new List<GameObject>();
        private Texture2D zooConfirmationTexture;
        private bool zooPhotoCamera,zooPhotoSaving,zooPhotoDelete;
        private int zooAlbumPage,zooStickerSelected=-1;
        private string zooPhotoSelected,zooPhotoArea;
        private long zooPhotoVisit;
        private readonly List<Graphic> zooPhotoHidden=new List<Graphic>();
        private readonly List<RectTransform> zooFrameCorners=new List<RectTransform>();
        private RectTransform zooFullPicture;
        public double ZooPhotoCaptureMilliseconds { get; private set; }
        public double ZooPhotoSaveMilliseconds { get; private set; }
        public bool ZooPhotoOpen=>zooPhotoOverlay!=null && zooPhotoOverlay.gameObject.activeSelf;
        public bool ZooPhotoSaving=>zooPhotoSaving;
        public string ZooAlbumPath=>zooAlbum?.DirectoryPath??"";
        public ZooAlbumManifest ZooAlbumData=>zooAlbum?.Manifest;
        public int ZooPhotoLoadedTextures=>zooPhotoLoaded.Count+(zooConfirmationTexture!=null?1:0);
        public string ZooPhotoFeedback=>zooPhotoMessage?.text??"";
        private void EnsureZooAlbum()
        {
            if(zooAlbum!=null)return;
            // Enrolled profiles are stable across process restarts. Private
            // branches are separate from shared family visits on this device.
            var scope=Shared?"shared/"+shared.View.worldId+"/"+Actor:
                "private/"+(offlineBranch??VerifyRun??"device")+"/"+Actor;
            zooAlbum=new ZooPhotoAlbum(Application.persistentDataPath,scope);
        }
        private RectTransform PhotoControl(Transform parent,string name,Vector2 at,Vector2 size,Action action)
        {
            var p=Panel(parent,name,at,size,Cream,true).rectTransform;NavButton(p.GetComponent<Image>(),action);return p;
        }
        private void CameraDrawing(Transform parent,Vector2 at,float scale=1)
        {
            var r=Rect(parent,"Toy camera drawing",at,Vector2.zero);r.localScale=Vector3.one*scale;
            Panel(r,"Camera body",Vector2.zero,new Vector2(70,47),new Color(.45f,.7f,.71f));
            Panel(r,"Camera top",new Vector2(-18,27),new Vector2(25,12),new Color(.45f,.7f,.71f));
            Panel(r,"Lens rim",new Vector2(5,0),Vector2.one*36,Cream,false,true);
            Panel(r,"Lens",new Vector2(5,0),Vector2.one*25,Ink,false,true);
            Panel(r,"Lens gleam",new Vector2(9,5),Vector2.one*8,Color.white,false,true);
        }
        private void AlbumDrawing(Transform parent)
        {
            var p=Panel(parent,"Album cover",Vector2.zero,new Vector2(60,61),new Color(.76f,.61f,.8f));
            Plain(p.transform,"Album spine",new Vector2(-22,0),new Vector2(5,56),Cream);
            var page=Panel(p.transform,"Album picture",new Vector2(5,1),new Vector2(38,33),Cream);
            StickerDrawing(page.transform,0,Vector2.zero,.55f);
        }
        private void StickerDrawing(Transform parent,int kind,Vector2 at,float scale=1)
        {
            var r=Rect(parent,"Sticker drawing",at,Vector2.zero);r.localScale=Vector3.one*scale;
            var color=kind==0?new Color(.35f,.62f,.32f):kind==1?new Color(.91f,.46f,.57f):kind==2?new Color(.95f,.74f,.24f):new Color(.65f,.51f,.78f);
            if(kind==0){var leaf=Panel(r,"Leaf",Vector2.zero,new Vector2(46,27),color,false,true);leaf.rectTransform.localRotation=Quaternion.Euler(0,0,35);var stem=Plain(r,"Leaf vein",Vector2.zero,new Vector2(37,4),Cream);stem.rectTransform.localRotation=Quaternion.Euler(0,0,35);}
            else if(kind==1){var tip=Plain(r,"Heart tip",new Vector2(0,-5),Vector2.one*27,color);tip.rectTransform.localRotation=Quaternion.Euler(0,0,45);for(var i=-1;i<=1;i+=2)Panel(r,"Heart lobe",new Vector2(i*10,8),Vector2.one*27,color,false,true);}
            else if(kind==2){for(var i=0;i<5;i++){var ray=Plain(r,"Star point",new Vector2(Mathf.Sin(i*Mathf.PI*.4f)*10,Mathf.Cos(i*Mathf.PI*.4f)*10),new Vector2(12,30),color);ray.rectTransform.localRotation=Quaternion.Euler(0,0,-i*72);}Panel(r,"Star middle",Vector2.zero,Vector2.one*24,color,false,true);}
            else {Panel(r,"Paw pad",new Vector2(0,-7),new Vector2(31,27),color,false,true);for(var i=0;i<3;i++)Panel(r,"Paw toe",new Vector2((i-1)*18,17+(i==1?5:0)),Vector2.one*17,color,false,true);}
        }
        private void PhotoCross(Transform parent)
        {for(var i=-1;i<=1;i+=2){var p=Plain(parent,"Close cross",Vector2.zero,new Vector2(37,7),Ink);p.rectTransform.localRotation=Quaternion.Euler(0,0,i*45);}}
        private void BuildZooPhotos()
        {
            zooPhotoButtons=Rect(safe,"Zoo keepsake buttons",Vector2.zero,new Vector2(220,100));
            CameraDrawing(PhotoControl(zooPhotoButtons,"Zoo toy camera",new Vector2(-55,0),new Vector2(100,88),()=>OpenZooPhotos(true)),Vector2.zero);
            AlbumDrawing(PhotoControl(zooPhotoButtons,"Zoo photo album",new Vector2(55,0),new Vector2(100,88),()=>OpenZooPhotos(false)));
            zooPhotoOverlay=Plain(safe,"Zoo photo input shield",Vector2.zero,Vector2.zero,new Color(0,0,0,0),true).rectTransform;Stretch(zooPhotoOverlay);
            zooCameraFrame=Rect(zooPhotoOverlay,"Camera framing",Vector2.zero,Vector2.zero);Stretch(zooCameraFrame);
            zooAlbumFrame=Panel(zooPhotoOverlay,"Zoo album page",Vector2.zero,new Vector2(1100,690),new Color(.93f,.92f,.83f),true).rectTransform;
            zooPhotoMessage=Label(zooPhotoOverlay,"",21,Vector2.zero,new Vector2(1050,42));
            zooPhotoOverlay.gameObject.SetActive(false);
        }
        private void OpenZooPhotos(bool camera)
        {
            if(MenuOpen || TravelPending || zooPhotoSaving || !ZooLayout.Area(CurrentArea))return;
            EnsureZooAlbum();
            // Keep an in-progress food walk/lease, but release held pointer and
            // joystick input. Only this client's locomotion waits for closing.
            var approach=zooApproach;var walk=destination;CancelPointers();zooApproach=approach;destination=walk;
            zooPhotoCamera=camera;zooPhotoSelected=null;zooStickerSelected=-1;zooPhotoDelete=false;
            zooPhotoArea=CurrentArea;zooPhotoVisit=ReadPlayer(Actor).visit;
            zooPhotoOverlay.gameObject.SetActive(true);zooPhotoOverlay.SetAsLastSibling();
            stick.gameObject.SetActive(false);RebuildZooPhotoUI();
        }
        private void ClearPhotoContent()
        {
            foreach(var g in zooPhotoHidden)if(g!=null)g.enabled=true;zooPhotoHidden.Clear();zooFrameCorners.Clear();zooFullPicture=null;
            foreach(var go in zooPhotoContent)if(go!=null){go.SetActive(false);Destroy(go);}zooPhotoContent.Clear();
            foreach(var t in zooPhotoLoaded)if(t!=null)Destroy(t);zooPhotoLoaded.Clear();zooPhotoView=null;zooPhotoConfirmation=null;
        }
        private RectTransform PhotoItem(Transform parent,string name,Vector2 at,Vector2 size,Action action)
        {var r=PhotoControl(parent,name,at,size,action);zooPhotoContent.Add(r.gameObject);return r;}
        private void PhotoText(Transform parent,string text,int size,Vector2 at,Vector2 bounds)
        {zooPhotoContent.Add(Label(parent,text,size,at,bounds).gameObject);}
        private RawImage PhotoImage(Transform parent,ZooPhoto p,bool thumbnail,Vector2 at,Vector2 bounds)
        {
            var t=zooAlbum.Load(p.id,thumbnail);
            if(t==null){PhotoText(parent,"Picture needs help",21,at,bounds);return null;}
            zooPhotoLoaded.Add(t);
            var r=Rect(parent,"Saved Zoo picture",at,FitPhoto(p,bounds));zooPhotoContent.Add(r.gameObject);
            var view=r.gameObject.AddComponent<RawImage>();view.texture=t;view.raycastTarget=false;return view;
        }
        private static Vector2 FitPhoto(ZooPhoto p,Vector2 box)=>new Vector2(p.width,p.height)*Mathf.Min(box.x/p.width,box.y/p.height);
        private void RebuildZooPhotoUI()
        {
            ClearPhotoContent();zooCameraFrame.gameObject.SetActive(zooPhotoCamera);zooAlbumFrame.gameObject.SetActive(!zooPhotoCamera);
            zooPhotoOverlay.GetComponent<Image>().color=zooPhotoCamera?Color.clear:new Color(.13f,.22f,.2f,.85f);
            zooPhotoMessage.text=zooAlbum.Warning;
            if(zooPhotoCamera){
                var close=PhotoItem(zooCameraFrame,"Close Zoo camera",new Vector2(480,245),Vector2.one*88,CloseZooPhotos);PhotoCross(close);
                var shutter=PhotoItem(zooCameraFrame,"Take Zoo photo",new Vector2(0,-260),new Vector2(142,96),TakeZooPhoto);CameraDrawing(shutter,Vector2.zero,1.3f);
                var album=PhotoItem(zooCameraFrame,"Camera to album",new Vector2(200,-260),new Vector2(100,96),()=>{if(zooPhotoSaving)return;zooPhotoCamera=false;RebuildZooPhotoUI();});AlbumDrawing(album);
                var confirm=Rect(zooCameraFrame,"Latest photo thumbnail",new Vector2(-230,-260),new Vector2(128,80));zooPhotoContent.Add(confirm.gameObject);zooPhotoConfirmation=confirm.gameObject.AddComponent<RawImage>();zooPhotoConfirmation.texture=zooConfirmationTexture;zooPhotoConfirmation.color=zooConfirmationTexture==null?Color.clear:Color.white;zooPhotoConfirmation.raycastTarget=false;
                PhotoText(zooCameraFrame,"Frame your Zoo moment",24,new Vector2(0,260),new Vector2(740,45));
                for(var x=-1;x<=1;x+=2)for(var y=-1;y<=1;y+=2){
                    var h=Plain(zooCameraFrame,"Frame corner",new Vector2(x,y),new Vector2(44,5),Cream);zooPhotoContent.Add(h.gameObject);zooFrameCorners.Add(h.rectTransform);
                    var v=Plain(zooCameraFrame,"Frame corner",new Vector2(x,y),new Vector2(5,44),Cream);zooPhotoContent.Add(v.gameObject);zooFrameCorners.Add(v.rectTransform);
                }
                zooFullPicture=Rect(zooCameraFrame,"Full album picture",Vector2.zero,new Vector2(84,70));zooPhotoContent.Add(zooFullPicture.gameObject);AlbumDrawing(zooFullPicture);
                for(var i=0;i<3;i++)for(var j=0;j<2;j++)Panel(zooFullPicture,"Full page tile",new Vector2((i-1)*14,(j==0?-1:1)*8),new Vector2(11,12),new Color(.42f,.65f,.43f));
                foreach(var g in ownedCanvas.GetComponentsInChildren<Graphic>(false))if(g.enabled && !g.transform.IsChildOf(zooPhotoOverlay) && !ZooPictureGraphic(g)){zooPhotoHidden.Add(g);g.enabled=false;}
                if(zooAlbum.Manifest.photos.Count>=ZooPhotoAlbum.PhotoLimit)zooPhotoMessage.text="Album full — open the album to make room.";
                return;
            }
            var back=PhotoItem(zooAlbumFrame,"Return to Zoo",new Vector2(480,283),Vector2.one*88,CloseZooPhotos);PhotoCross(back);
            PhotoText(zooAlbumFrame,"My Zoo pictures",30,new Vector2(0,283),new Vector2(660,44));
            PhotoText(zooAlbumFrame,"Saved on this device only",18,new Vector2(0,-320),new Vector2(950,28));
            if(zooPhotoSelected==null){
                var m=zooAlbum.Manifest;zooAlbumPage=Mathf.Clamp(zooAlbumPage,0,Mathf.Max(0,(m.photos.Count-1)/6));
                for(var i=0;i<6;i++){
                    var index=zooAlbumPage*6+i;if(index>=m.photos.Count)break;var p=m.photos[index];var at=new Vector2((i%3-1)*310,135-i/3*205);
                    var card=PhotoItem(zooAlbumFrame,"Open Zoo photo "+i,at,new Vector2(286,185),()=>{zooPhotoSelected=p.id;zooStickerSelected=-1;RebuildZooPhotoUI();});
                    var img=PhotoImage(card,p,true,Vector2.zero,new Vector2(270,171));if(img!=null)DrawPhotoStickers(p,img.rectTransform,false);
                }
                ZooArrow(PhotoItem(zooAlbumFrame,"Previous album page",new Vector2(-365,-267),new Vector2(130,88),()=>{zooAlbumPage--;RebuildZooPhotoUI();}),Vector2.zero,-1);
                ZooArrow(PhotoItem(zooAlbumFrame,"Next album page",new Vector2(365,-267),new Vector2(130,88),()=>{zooAlbumPage++;RebuildZooPhotoUI();}),Vector2.zero,1);
                PhotoText(zooAlbumFrame,(m.photos.Count==0?"Take a picture with your toy camera":(zooAlbumPage+1)+" / "+Mathf.Max(1,(m.photos.Count+5)/6))+"   ·   "+m.photos.Count+" / 24",20,new Vector2(0,-206),new Vector2(760,38));
                if(m.Removed!=null)PhotoText(PhotoItem(zooAlbumFrame,"Undo removed photo",new Vector2(0,-267),new Vector2(360,88),()=>{if(zooAlbum.Undo(out var error))RebuildZooPhotoUI();else zooPhotoMessage.text=error;}),"Undo remove",22,Vector2.zero,new Vector2(340,70));
                return;
            }
            var photo=zooAlbum.Manifest.photos.FirstOrDefault(p=>p.id==zooPhotoSelected);
            if(photo==null){zooPhotoSelected=null;RebuildZooPhotoUI();return;}
            if(zooPhotoDelete){
                PhotoImage(zooAlbumFrame,photo,true,new Vector2(0,135),new Vector2(350,180));
                PhotoText(zooAlbumFrame,"Remove this picture?\nYou can undo in the album.",25,new Vector2(0,-10),new Vector2(830,90));
                PhotoText(PhotoItem(zooAlbumFrame,"Keep Zoo photo",new Vector2(-230,-100),new Vector2(360,95),()=>{zooPhotoDelete=false;RebuildZooPhotoUI();}),"Keep picture",24,Vector2.zero,new Vector2(340,80));
                PhotoText(PhotoItem(zooAlbumFrame,"Confirm remove Zoo photo",new Vector2(230,-100),new Vector2(360,95),()=>{if(zooAlbum.Remove(photo.id,out var error)){zooPhotoSelected=null;zooPhotoDelete=false;RebuildZooPhotoUI();}else zooPhotoMessage.text=error;}),"Remove picture",24,Vector2.zero,new Vector2(340,80));return;
            }
            zooPhotoView=PhotoImage(zooAlbumFrame,photo,false,new Vector2(-82,58),new Vector2(795,366));
            if(zooPhotoView!=null)DrawPhotoStickers(photo,zooPhotoView.rectTransform,true);
            for(var i=0;i<4;i++){var kind=i;var choice=PhotoItem(zooAlbumFrame,"Add Zoo sticker "+i,new Vector2(-420+i*110,-182),new Vector2(100,88),()=>EditPhoto(p=>{if(p.stickers.Count>=ZooPhotoAlbum.StickerLimit)return;p.stickers.Add(new ZooSticker{kind=kind,cell=4});zooStickerSelected=p.stickers.Count-1;}));StickerDrawing(choice,kind,Vector2.zero,1.05f);}
            var moveNames=new[]{"Move sticker left","Move sticker right","Move sticker up","Move sticker down"};
            var moveAt=new[]{new Vector2(325,-160),new Vector2(465,-160),new Vector2(395,-66),new Vector2(395,-254)};
            for(var i=0;i<4;i++){var direction=i;var r=PhotoItem(zooAlbumFrame,moveNames[i],moveAt[i],new Vector2(96,88),()=>EditPhoto(p=>{if(zooStickerSelected<0 || zooStickerSelected>=p.stickers.Count)return;var s=p.stickers[zooStickerSelected];var x=s.cell%3;var y=s.cell/3;if(direction<2)x=Mathf.Clamp(x+(direction==0?-1:1),0,2);else y=Mathf.Clamp(y+(direction==2?-1:1),0,2);s.cell=y*3+x;}));var drawing=Rect(r,"Direction picture",Vector2.zero,Vector2.zero);ZooArrow(drawing,Vector2.zero,i==0?-1:1);if(i>=2)drawing.localRotation=Quaternion.Euler(0,0,i==2?90:-90);}
            PhotoText(PhotoItem(zooAlbumFrame,"Remove selected sticker",new Vector2(135,-182),new Vector2(196,88),()=>EditPhoto(p=>{if(zooStickerSelected>=0 && zooStickerSelected<p.stickers.Count)p.stickers.RemoveAt(zooStickerSelected);zooStickerSelected=-1;})),"Remove sticker",18,Vector2.zero,new Vector2(190,72));
            PhotoText(PhotoItem(zooAlbumFrame,"Back to album grid",new Vector2(-420,-270),new Vector2(190,88),()=>{zooPhotoSelected=null;zooStickerSelected=-1;RebuildZooPhotoUI();}),"Album",22,Vector2.zero,new Vector2(180,68));
            ZooArrow(PhotoItem(zooAlbumFrame,"Move photo left",new Vector2(-247,-270),new Vector2(110,88),()=>ReorderPhoto(-1)),Vector2.zero,-1);
            ZooArrow(PhotoItem(zooAlbumFrame,"Move photo right",new Vector2(-125,-270),new Vector2(110,88),()=>ReorderPhoto(1)),Vector2.zero,1);
            PhotoText(PhotoItem(zooAlbumFrame,"Remove Zoo photo",new Vector2(50,-270),new Vector2(214,88),()=>{zooPhotoDelete=true;RebuildZooPhotoUI();}),"Remove picture",20,Vector2.zero,new Vector2(205,68));
            PhotoText(zooAlbumFrame,"Tap a sticker\nthen the arrows\n"+photo.stickers.Count+" / 6",18,new Vector2(410,128),new Vector2(170,110));
        }
        private void DrawPhotoStickers(ZooPhoto p,RectTransform parent,bool editable)
        {
            for(var i=0;i<p.stickers.Count;i++){
                var index=i;var s=p.stickers[i];var box=parent.sizeDelta;var at=new Vector2((s.cell%3-1)*box.x*.30f,(1-s.cell/3)*box.y*.29f);
                var size=editable?88:32;RectTransform r;
                if(editable){r=PhotoItem(parent,"Select Zoo sticker "+i,at,Vector2.one*size,()=>{zooStickerSelected=index;RebuildZooPhotoUI();});r.GetComponent<Image>().color=index==zooStickerSelected?new Color(1,1,1,.65f):Color.clear;}
                else {r=Rect(parent,"Thumbnail sticker",at,Vector2.zero);zooPhotoContent.Add(r.gameObject);}
                StickerDrawing(r,s.kind,Vector2.zero,editable?1:.45f);
            }
        }
        private void EditPhoto(Action<ZooPhoto> edit)
        {
            if(zooPhotoSaving || zooPhotoSelected==null)return;
            if(zooAlbum.Change(m=>edit(m.photos.First(p=>p.id==zooPhotoSelected)),out var error))RebuildZooPhotoUI();else zooPhotoMessage.text=error;
        }
        private void ReorderPhoto(int direction)
        {
            if(zooAlbum.Change(m=>{var i=m.photos.FindIndex(p=>p.id==zooPhotoSelected);var j=Mathf.Clamp(i+direction,0,m.photos.Count-1);var p=m.photos[i];m.photos.RemoveAt(i);m.photos.Insert(j,p);zooAlbumPage=j/6;},out var error))RebuildZooPhotoUI();else zooPhotoMessage.text=error;
        }
        private void CloseZooPhotos()
        {
            if(!ZooPhotoOpen)return;
            zooPhotoOverlay.gameObject.SetActive(false);ClearPhotoContent();zooPhotoSelected=null;zooPhotoDelete=false;
            stick.gameObject.SetActive(JoystickMode && !MenuOpen);
        }
        private void TickZooPhotos(bool visible)
        {
            if(zooPhotoButtons==null)return;
            zooPhotoButtons.gameObject.SetActive(visible && !MenuOpen && !CharactersOpen);
            var scale=1f;zooPhotoButtons.localScale=Vector3.one*scale;
            zooPhotoButtons.anchoredPosition=new Vector2(-safe.rect.width/2+132*scale,-safe.rect.height/2+90*scale);
            if(!ZooPhotoOpen)return;
            if(!visible || applicationPaused || CurrentArea!=zooPhotoArea || ReadPlayer(Actor).visit!=zooPhotoVisit || TravelPending){CloseZooPhotos();return;}
            zooPhotoOverlay.SetAsLastSibling();zooAlbumFrame.localScale=Vector3.one*Mathf.Min(1,Mathf.Min(safe.rect.width/1140,safe.rect.height/730));
            zooCameraFrame.offsetMin=zooCameraFrame.offsetMax=Vector2.zero;
            for(var i=0;i<zooFrameCorners.Count;i++){
                var r=zooFrameCorners[i];var x=i/4==0?-1:1;var y=i%4<2?-1:1;
                r.anchoredPosition=new Vector2(x*(safe.rect.width/2-(i%2==0?32:12)),y*(safe.rect.height/2-(i%2==0?12:32)));
            }
            // Label objects are named by Label's generic helper, so locate the
            // framing text by its component rather than depending on that name.
            foreach(var t in zooCameraFrame.GetComponentsInChildren<Text>())if(t.text=="Frame your Zoo moment")t.rectTransform.anchoredPosition=new Vector2(0,safe.rect.height/2-46);
            if(zooFullPicture!=null){zooFullPicture.anchoredPosition=new Vector2(0,-safe.rect.height/2+205);zooFullPicture.gameObject.SetActive(zooAlbum.Manifest.photos.Count>=ZooPhotoAlbum.PhotoLimit);}
            // Camera controls occupy safe corners; the fixed frame is the whole
            // Board, including habitat and children, without any interface.
            foreach(var name in new[]{"Close Zoo camera","Take Zoo photo","Camera to album","Latest photo thumbnail"}){
                var r=zooCameraFrame.Find(name) as RectTransform;if(r==null)continue;
                r.anchoredPosition=name=="Close Zoo camera"?new Vector2(safe.rect.width/2-65,safe.rect.height/2-65):new Vector2(name=="Take Zoo photo"?0:name=="Camera to album"?165:-195,-safe.rect.height/2+78);
            }
            zooPhotoMessage.rectTransform.anchoredPosition=new Vector2(0,-safe.rect.height/2+155);zooPhotoMessage.rectTransform.sizeDelta=new Vector2(safe.rect.width-60,42);
            if(Keyboard.current?.escapeKey.wasPressedThisFrame==true){if(zooPhotoDelete){zooPhotoDelete=false;RebuildZooPhotoUI();}else if(!zooPhotoCamera && zooPhotoSelected!=null){zooPhotoSelected=null;RebuildZooPhotoUI();}else CloseZooPhotos();}
        }
        private async void TakeZooPhoto()
        {
            if(!ZooPhotoOpen || !zooPhotoCamera || zooPhotoSaving || !Ready || applicationPaused)return;
            if(zooAlbum.Manifest.photos.Count>=ZooPhotoAlbum.PhotoLimit){zooPhotoMessage.text="Album full — open the album to make room.";return;}
            var album=zooAlbum;zooPhotoSaving=true;zooPhotoMessage.text="Saving your picture…";
            var clock=System.Diagnostics.Stopwatch.StartNew();
            Texture2D photo=null,thumb=null;
            try{
                photo=CaptureZooPicture();thumb=ReducePhoto(photo,ZooPhotoAlbum.ThumbnailDimension);
                var info=new ZooPhoto{id=Guid.NewGuid().ToString("N"),exhibit=ZooCurrentExhibit,width=photo.width,height=photo.height};
                var image=photo.EncodeToJPG(82);var thumbnail=thumb.EncodeToJPG(75);
                ZooPhotoCaptureMilliseconds=clock.Elapsed.TotalMilliseconds;
                // File I/O runs outside the frame loop; Unity's encoder and
                // texture APIs stay on the main thread. No shared pause/command.
                var result=await Task.Run(()=>{var ok=album.Add(info,image,thumbnail,out var error);return (ok,error);});
                if(this==null || zooAlbum!=album)return;
                if(result.ok){
                    if(zooConfirmationTexture!=null)Destroy(zooConfirmationTexture);zooConfirmationTexture=thumb;thumb=null;
                    if(ZooPhotoOpen){zooPhotoMessage.text="Picture saved ✓";if(zooPhotoConfirmation!=null){zooPhotoConfirmation.texture=zooConfirmationTexture;zooPhotoConfirmation.color=Color.white;}}
                    PhotoShutterSound();
                }else if(ZooPhotoOpen)zooPhotoMessage.text=result.error;
            }catch(Exception e){if(this!=null && ZooPhotoOpen)zooPhotoMessage.text="Could not take this picture. Try again.";Debug.LogWarning("Zoo photo capture: "+e.GetType().Name);}
            finally{ZooPhotoSaveMilliseconds=clock.Elapsed.TotalMilliseconds;if(photo!=null)Destroy(photo);if(thumb!=null)Destroy(thumb);zooPhotoSaving=false;}
        }
        private bool ZooPictureGraphic(Graphic g)
        {
            if(!g.transform.IsChildOf(Board) || g is Text)return false;
            for(var p=g.transform;p!=null && p!=Board;p=p.parent)
                if(p.GetComponent<Button>()!=null || p.name=="Feeder portrait" || p.name=="Your standing spot" || p.name=="Stand here" || p.name.Contains("pointer"))return false;
            return true;
        }
        private void PhotoShutterSound()
        {
            if(ZooEffectGain<=0 || applicationPaused)return;
            // Short original soft click. It uses its own source so a shutter
            // never interrupts an animal call, feeding or water sound.
            var go=new GameObject("Quiet toy shutter");var source=go.AddComponent<AudioSource>();source.playOnAwake=false;
            var clip=AudioClip.Create("Toy camera click",2205,1,22050,false);var data=new float[2205];
            for(var i=0;i<data.Length;i++)data[i]=Mathf.Sin(i*.71f)*Mathf.Exp(-i/180f)*.13f;
            clip.SetData(data,0);source.clip=clip;source.volume=.18f*ZooEffectGain;source.Play();Destroy(go,.2f);Destroy(clip,.25f);
        }
        private Texture2D CaptureZooPicture()
        {
            var canvas=ownedCanvas.GetComponent<Canvas>();var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;var distance=canvas.planeDistance;
            var active=RenderTexture.active;var hidden=new List<Graphic>();var layers=new List<(GameObject go,int layer)>();
            GameObject go=null;RenderTexture target=null;
            try{
                // Built-in rendering (project GraphicsSettings) renders the
                // existing live UI geometry, textures, masks and depth order.
                // No cloned behaviours, desktop grab or substitute animal pose.
                foreach(var g in canvas.GetComponentsInChildren<Graphic>(false)){
                    var keep=ZooPictureGraphic(g);
                    if(g.enabled && !keep){hidden.Add(g);g.enabled=false;}
                }
                foreach(var t in canvas.GetComponentsInChildren<Transform>(true)){layers.Add((t.gameObject,t.gameObject.layer));t.gameObject.layer=31;}
                go=new GameObject("Temporary Zoo photo camera");var camera=go.AddComponent<Camera>();camera.enabled=false;camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.66f,.88f,.97f);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=2000;camera.transform.position=new Vector3(0,0,-1000);
                target=RenderTexture.GetTemporary(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=100;
                Canvas.ForceUpdateCanvases();camera.Render();
                return ReadReduced(target,ZooPhotoAlbum.MaxDimension);
            }finally{
                canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;
                foreach(var item in layers)if(item.go!=null)item.go.layer=item.layer;
                foreach(var g in hidden)if(g!=null)g.enabled=true;
                Canvas.ForceUpdateCanvases();RenderTexture.active=active;
                if(go!=null){go.SetActive(false);Destroy(go);}if(target!=null)RenderTexture.ReleaseTemporary(target);
            }
        }
        private static Texture2D ReducePhoto(Texture2D source,int dimension)=>ReadReduced(source,dimension);
        private static Texture2D ReadReduced(Texture source,int dimension)
        {
            var scale=Mathf.Min(1,(float)dimension/Mathf.Max(source.width,source.height));var w=Mathf.Max(1,Mathf.RoundToInt(source.width*scale));var h=Mathf.Max(1,Mathf.RoundToInt(source.height*scale));
            var active=RenderTexture.active;var rt=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGB32);Texture2D result=null;
            try{Graphics.Blit(source,rt);RenderTexture.active=rt;result=new Texture2D(w,h,TextureFormat.RGB24,false);result.ReadPixels(new Rect(0,0,w,h),0,0);result.Apply();return result;}
            catch{if(result!=null)Destroy(result);throw;}
            finally{RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
        }
        private void ResetZooPhotos()
        {
            ClearPhotoContent();if(zooConfirmationTexture!=null)Destroy(zooConfirmationTexture);zooConfirmationTexture=null;
            if(zooPhotoButtons!=null)Destroy(zooPhotoButtons.gameObject);if(zooPhotoOverlay!=null)Destroy(zooPhotoOverlay.gameObject);
            zooPhotoButtons=zooPhotoOverlay=zooAlbumFrame=zooCameraFrame=null;zooPhotoMessage=null;zooAlbum=null;zooPhotoSelected=null;
        }
    }
}
