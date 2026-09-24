using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LittleWeeps.Core;
using LittleWeeps.Adapters;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LittleWeeps.Client
{
    // G2 Windows prototype. Placeholder illustrations, local authority, no network.
    public sealed class SoloScreen : MonoBehaviour
    {
        public SoloWorld World { get; private set; }
        public string Actor { get; private set; }
        public string SavePath { get; private set; }
        public string VerifyMode { get; private set; }
        public string VerifyRun { get; private set; }
        public CheckpointStatus LoadedStatus { get; private set; }
        public RectTransform Board { get; private set; }
        public bool JoystickMode { get; private set; }
        public SoloNarration Narration {get;private set;}
        public bool MenuOpen => menu != null && menu.activeSelf;
        public readonly Dictionary<string, SoloPointerSurface> Surfaces = new Dictionary<string, SoloPointerSurface>();
        private readonly Dictionary<string, RectTransform> toys = new Dictionary<string, RectTransform>();
        private readonly Dictionary<string, Image> fills = new Dictionary<string, Image>();
        private CheckpointStore store;
        private RectTransform safe, avatar, stickKnob, stick;
        private Image head, body;
        private Text message, activity, movementLabel, saveLabel, voiceLabel, listenLabel;
        private GameObject voiceSlash;
        private Font font;
        private Sprite rounded, circle;
        private GameObject menu;
        private Vector2? destination;
        private Vector2 stickDirection;
        private string dragging;
        private bool dirty;
        private float nextSave, nextMovement, lastMovement;
        private Rect lastSafeArea;
        private static readonly Color Ink = new Color(.15f,.25f,.29f), Cream = new Color(.98f,.96f,.88f);
        private void Start()
        {
            Application.targetFrameRate = 30; Application.runInBackground = true;
            if(!ReadVerificationArgs())
            {Debug.LogError("Invalid solo verification arguments; normal saved play was not opened.");Application.Quit(2);return;}
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rounded = Shape(false); circle = Shape(true);
            var canvas = new GameObject("Solo Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scale = canvas.GetComponent<CanvasScaler>(); scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scale.referenceResolution = new Vector2(1280,800); scale.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            safe = Rect(canvas.transform,"Safe Area",Vector2.zero,Vector2.zero); UpdateSafeArea();
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("Solo Events", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            SavePath = Path.Combine(Application.persistentDataPath,"SoloPrototype",VerifyRun ?? "family-local","world.save");
            store = new CheckpointStore(SavePath, ValidPayload);
            try
            {
                var loaded = store.Load(); LoadedStatus = loaded.Status;
                if (loaded.Status == CheckpointStatus.Corrupt || loaded.Status == CheckpointStatus.Unsupported)
                    throw new InvalidDataException("Existing save needs recovery or a compatible version.");
                World = loaded.Status == CheckpointStatus.Missing ? SoloWorld.Create(Guid.NewGuid().ToString("N")) : SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(loaded.Payload));
                Actor = World.Snapshot().players[0].id;
            }
            catch (Exception e)
            {
                Label(safe,"We couldn't open your saved play.\nA grown-up needs to check this save before continuing.",32,Vector2.zero,new Vector2(1100,200));
                Debug.LogError("Solo save load stopped: " + e.Message);
                if (VerifyRun != null) SoloVerification.FinishBlocked(this,e.Message);
                return;
            }
            JoystickMode = PlayerPrefs.GetInt(PreferenceKey("joystick"),0) == 1;
            Narration=gameObject.AddComponent<SoloNarration>();Narration.Initialize(VerifyRun!=null);
            Narration.SetVoiceEnabled(PlayerPrefs.GetInt(PreferenceKey("voice"),1)!=0);
            BuildScreen(); Render();
            if (LoadedStatus == CheckpointStatus.Missing) SaveNow();
            if (LoadedStatus == CheckpointStatus.Recovered) message.text = "Your last safe save is back. Let's play!";
            if (VerifyRun != null)
            {
                if(VerifyMode=="crash-hold" || VerifyMode=="crash-resume")gameObject.AddComponent<SoloCrashVerification>();
                else gameObject.AddComponent<SoloVerification>();
            }
        }
        public static bool ValidPayload(string payload)
        {
            try
            {
                var snapshot = JsonUtility.FromJson<SoloSnapshot>(payload);
                if (snapshot != null && snapshot.schema > 1) throw new NotSupportedException("Newer solo save schema.");
                SoloWorld.Validate(snapshot); return true;
            }
            catch (ArgumentException) { return false; }
            catch (InvalidOperationException) { return false; }
        }
        private bool ReadVerificationArgs()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            var args = Environment.GetCommandLineArgs();
            var requested=false;
            for (var i=0;i<args.Length;i++)
            {
                if(args[i]=="-soloVerify"){requested=true;if(i+1<args.Length)VerifyMode=args[i+1];}
                if(args[i]=="-soloRun"){requested=true;if(i+1<args.Length && Guid.TryParse(args[i+1],out var id))VerifyRun=id.ToString("N");}
            }
            if (VerifyMode != "seed" && VerifyMode != "resume" && VerifyMode != "recover" && VerifyMode != "input" && VerifyMode != "crash-hold" && VerifyMode != "crash-resume" && VerifyMode != "blocked" && VerifyMode != "settings-seed" && VerifyMode != "settings-resume") { VerifyMode=null; VerifyRun=null; }
            if(requested && (VerifyMode==null || VerifyRun==null))return false;
#endif
            return true;
        }
        private void BuildScreen()
        {
            Label(safe,"Little Weeps",36,new Vector2(-407,351),new Vector2(330,58));
            Label(safe,"Garden play lab  /  "+Application.version,17,new Vector2(-404,313),new Vector2(340,34));
            Button(safe,"Blue pup",new Vector2(-95,343),new Vector2(150,64),()=>ChooseAvatar("blue-pup"),new Color(.7f,.87f,.97f));
            Button(safe,"Orange pup",new Vector2(75,343),new Vector2(160,64),()=>ChooseAvatar("orange-pup"),new Color(1,.81f,.61f));
            movementLabel=Button(safe,JoystickMode?"Joystick":"Tap to walk",new Vector2(285,343),new Vector2(210,64),ToggleMovement,Cream);
            Button(safe,"Menu",new Vector2(500,343),new Vector2(160,64),()=>SetMenu(true),Cream);
            var garden=Button(safe,"Grow a flower",new Vector2(-380,265),new Vector2(245,48),()=>StartActivity("garden"),new Color(.86f,.93f,.74f));
            garden.rectTransform.anchoredPosition=new Vector2(22,0);garden.rectTransform.sizeDelta=new Vector2(191,48);
            var blossom=Panel(garden.transform.parent,"Flower picture",new Vector2(-93,0),new Vector2(33,33),new Color(.96f,.52f,.61f),false,true);
            Panel(blossom.transform,"Center",Vector2.zero,new Vector2(12,12),new Color(1,.84f,.35f),false,true);
            var cleanup=Button(safe,"Splash cleanup",new Vector2(-115,265),new Vector2(245,48),()=>StartActivity("cleanup"),new Color(.76f,.9f,1));
            cleanup.rectTransform.anchoredPosition=new Vector2(22,0);cleanup.rectTransform.sizeDelta=new Vector2(191,48);
            Panel(cleanup.transform.parent,"Sponge picture",new Vector2(-94,0),new Vector2(34,23),new Color(1,.83f,.28f));
            Button(safe,"Free play",new Vector2(120,265),new Vector2(190,48),()=>StartActivity(""),Cream);
            Label(safe,"Pick any game. Leave any time.",17,new Vector2(413,265),new Vector2(330,42));
            Board=Panel(safe,"Garden",new Vector2(0,-25),new Vector2(1120,500),new Color(.76f,.89f,.72f),true).rectTransform;
            Surface(Board,"ground");
            Panel(Board,"Sky",new Vector2(0,140),new Vector2(1116,216),new Color(.8f,.92f,.96f));
            for(var i=0;i<7;i++) Panel(Board,"Fence",new Vector2(-465+i*155,80),new Vector2(142,65),new Color(.99f,.95f,.83f));
            Panel(Board,"Path",new Vector2(0,-156),new Vector2(1050,70),new Color(.9f,.81f,.64f));
            foreach(var toy in World.ReadToys()) DrawToy(toy);
            DrawAvatar();
            stick=Panel(safe,"Walk joystick",new Vector2(-477,-193),new Vector2(146,146),new Color(1,1,1,.8f),true,true).rectTransform;
            Surface(stick,"stick");
            stickKnob=Panel(stick,"Thumb",Vector2.zero,new Vector2(65,65),new Color(.34f,.55f,.62f),false,true).rectTransform;
            stick.gameObject.SetActive(JoystickMode);
            activity=Label(safe,"",24,new Vector2(0,-312),new Vector2(1130,44));
            message=Label(safe,"Drag the bucket to the tap, then to the plant.",21,new Vector2(-85,-354),new Vector2(950,40));
            var listen=Button(safe,"Listen",new Vector2(490,-352),new Vector2(150,52),Listen,Cream);
            listenLabel=listen;
            listen.rectTransform.anchoredPosition=new Vector2(15,0);listen.rectTransform.sizeDelta=new Vector2(108,52);
            for(var i=0;i<3;i++)Panel(listen.transform.parent,"Sound",new Vector2(-52+i*9,0),new Vector2(5,12+i*9),Ink);
            saveLabel=Label(safe,"Local solo prototype · placeholder art",15,new Vector2(0,-380),new Vector2(1130,28));
            menu=Panel(safe,"Pause panel",Vector2.zero,new Vector2(1190,760),new Color(.97f,.97f,.91f,.98f),true).gameObject;
            Label(menu.transform,"Take your time",42,new Vector2(0,230),new Vector2(760,100));
            Label(menu.transform,"Your toys stay where you put them.\nYou can always leave an activity.",25,new Vector2(0,140),new Vector2(800,80));
            voiceLabel=Button(menu.transform,"Voice on",new Vector2(0,35),new Vector2(350,75),ToggleVoice,new Color(.77f,.88f,.96f));
            voiceLabel.transform.parent.name="Voice setting";
            voiceLabel.rectTransform.anchoredPosition=new Vector2(20,0);voiceLabel.rectTransform.sizeDelta=new Vector2(250,75);
            for(var i=0;i<3;i++)Panel(voiceLabel.transform.parent,"Sound",new Vector2(-130+i*12,0),new Vector2(7,15+i*12),Ink);
            var slash=Panel(voiceLabel.transform.parent,"Voice off mark",new Vector2(-118,0),new Vector2(55,7),new Color(.73f,.26f,.23f));
            slash.rectTransform.localRotation=Quaternion.Euler(0,0,45);voiceSlash=slash.gameObject;
            Label(menu.transform,"Spoken hints on this device",18,new Vector2(0,-24),new Vector2(650,36));
            Button(menu.transform,"Back to play",new Vector2(0,-90),new Vector2(350,85),()=>SetMenu(false),new Color(.81f,.92f,.72f));
            Button(menu.transform,"Leave activity",new Vector2(0,-200),new Vector2(350,75),()=>{StartActivity("");SetMenu(false);},Cream);
            UpdateVoiceControls();
            menu.SetActive(false);
        }
        public SoloResult Command(SoloAction action,string item="",string target="",string value="",float x=0,float y=0)
        {
            var result=World.Apply(new SoloCommand {requestId=Guid.NewGuid().ToString("N"),actor=Actor,expectedRevision=World.Revision,action=action,item=item,target=target,value=value,x=x,y=y});
            if(result.Accepted) {dirty=true;Render();}
            return result;
        }
        public void ChooseAvatar(string id) { if(World!=null) Command(SoloAction.ChangeAvatar,value:id); }
        public void StartActivity(string id)
        {
            if(World==null)return;
            Command(id==""?SoloAction.LeaveActivity:SoloAction.StartActivity,value:id);
            Narration.Speak(id==""?"freeplay":id);
            message.text=id==""?"All your toys still work. Explore!":id=="garden"?"Fill your bucket at the tap. Give the flower a drink!":"Drag the sponge over the puddle to soak it up.";
        }
        public void ToggleMovement()
        {
            CancelPointers(); JoystickMode=!JoystickMode; movementLabel.text=JoystickMode?"Joystick":"Tap to walk";stick.gameObject.SetActive(JoystickMode);
            Narration.Speak(JoystickMode?"joystick":"tapwalk");
            PlayerPrefs.SetInt(PreferenceKey("joystick"),JoystickMode?1:0);PlayerPrefs.Save();
        }
        // Preferences belong to this device, not the shared world/checkpoint. Test
        // players use a fresh GUID key prefix and never alter the family's keys.
        private string PreferenceKey(string name)=>(VerifyRun==null?"solo.prototype.":"solo.verify."+VerifyRun+".")+name;
        public void ToggleVoice()
        {
            Narration.SetVoiceEnabled(!Narration.VoiceEnabled);
            PlayerPrefs.SetInt(PreferenceKey("voice"),Narration.VoiceEnabled?1:0);PlayerPrefs.Save();
            UpdateVoiceControls();
        }
        private void UpdateVoiceControls()
        {
            voiceLabel.text=Narration.VoiceEnabled?"Voice on":"Voice off";voiceSlash.SetActive(!Narration.VoiceEnabled);
            listenLabel.text=Narration.VoiceEnabled?"Listen":"Voice off";
            listenLabel.transform.parent.GetComponent<Button>().interactable=Narration.VoiceEnabled;
        }
        public void SetMenu(bool open) { if(open){CancelPointers();Narration.Stop();SaveNow();} menu.SetActive(open); }
        public void Listen(){var activityId=World.ReadPlayer(Actor).activity;Narration.Speak(activityId==""?"freeplay":activityId);}
        private Vector2 BoardPoint(Vector2 screen)
        { RectTransformUtility.ScreenPointToLocalPointInRectangle(Board,screen,null,out var local);return new Vector2((local.x/Board.rect.width+.5f)*SoloWorld.Width,(local.y/Board.rect.height+.5f)*SoloWorld.Height); }
        public Vector2 ScreenPoint(float x,float y) => RectTransformUtility.WorldToScreenPoint(null,Board.TransformPoint(new Vector3((x/SoloWorld.Width-.5f)*Board.rect.width,(y/SoloWorld.Height-.5f)*Board.rect.height,0)));
        public bool BeginPointer(string role,Vector2 screen)
        {
            if(World==null || MenuOpen)return false;
            if(role=="ground") {if(JoystickMode)return false;var point=BoardPoint(screen);destination=new Vector2(Mathf.Clamp(point.x,40,960),Mathf.Clamp(point.y,35,455));return true;}
            if(role=="stick") {if(!JoystickMode)return false;MovePointer(role,screen);return true;}
            if(dragging!=null || !Command(SoloAction.Grab,role).Accepted)return false;
            dragging=role;MovePointer(role,screen);return true;
        }
        public void MovePointer(string role,Vector2 screen)
        {
            if(role=="stick") {RectTransformUtility.ScreenPointToLocalPointInRectangle(stick,screen,null,out var point);stickDirection=Vector2.ClampMagnitude(point/55,1);stickKnob.anchoredPosition=stickDirection*40;}
            else if(role==dragging) {var point=BoardPoint(screen);toys[role].anchoredPosition=ToBoard(point.x,point.y);toys[role].SetAsLastSibling();}
        }
        public void EndPointer(string role,Vector2 screen)
        {
            if(role=="stick"){stickDirection=Vector2.zero;stickKnob.anchoredPosition=Vector2.zero;return;}
            if(role!=dragging)return;
            var point=BoardPoint(screen);
            if(!SoloWorld.Position(point.x,point.y)){CancelPointer(role);return;}
            var target=World.ReadToys().Where(t=>t.kind==ToyKind.Tap || t.kind==ToyKind.Plant || t.kind==ToyKind.Puddle)
                .OrderBy(t=>Vector2.Distance(new Vector2(t.x,t.y),point)).First();
            var id=Vector2.Distance(new Vector2(target.x,target.y),point)<=SoloWorld.InteractionRadius?target.id:"";
            var result=Command(SoloAction.Drop,role,id,x:point.x,y:point.y);
            if(!result.Accepted)Command(SoloAction.CancelGrab,role);
            dragging=null;Render();
            message.text=result.Outcome=="bucket-filled"?"Splash! Your bucket is full.":result.Outcome=="plant-watered"?"A drink for the flower!":result.Outcome=="puddle-cleaned"?"Squeeze, soak, tidy!":"What shall we play next?";
        }
        public void CancelPointer(string role)
        {
            if(role=="stick"){stickDirection=Vector2.zero;if(stickKnob!=null)stickKnob.anchoredPosition=Vector2.zero;}
            if(role==dragging){Command(SoloAction.CancelGrab,role);dragging=null;Render();}
        }
        public void CancelPointers(){foreach(var surface in Surfaces.Values)surface.Cancel();destination=null;stickDirection=Vector2.zero;}
        private void Update()
        {
            if(safe!=null && lastSafeArea!=Screen.safeArea)UpdateSafeArea();
            if(World==null || MenuOpen)return;
            // Minimized Windows players may update much faster than presentation.
            // Bound command creation independently of render frequency.
            var now=Time.realtimeSinceStartup;
            if(now<nextMovement)return;
            var delta=Mathf.Clamp(now-lastMovement,0,.1f);lastMovement=now;nextMovement=now+1f/30;
            var p=World.ReadPlayer(Actor);var current=new Vector2(p.x,p.y);var next=current;
            if(JoystickMode)next+=stickDirection*(210*delta);
            else if(destination.HasValue){next=Vector2.MoveTowards(current,destination.Value,210*delta);if(Vector2.Distance(next,destination.Value)<1)destination=null;}
            next.x=Mathf.Clamp(next.x,40,960);next.y=Mathf.Clamp(next.y,35,455);
            if(Vector2.Distance(current,next)>.01f)Command(SoloAction.Move,x:next.x,y:next.y);
            if(dirty && Time.realtimeSinceStartup>=nextSave){SaveNow();nextSave=Time.realtimeSinceStartup+1;}
        }
        public void SaveNow()
        {
            if(World==null || store==null)return;
            try{store.Save(JsonUtility.ToJson(World.Snapshot()));dirty=false;if(saveLabel!=null)saveLabel.text="Saved on this device · solo prototype · "+Application.version;}
            catch(Exception e){if(saveLabel!=null)saveLabel.text="Couldn't save yet. Please ask a grown-up.";Debug.LogError("Solo checkpoint: "+e.Message);}
        }
        private void OnApplicationPause(bool paused){if(paused){CancelPointers();SaveNow();}}
        private void OnApplicationFocus(bool focused){if(!focused && World!=null){CancelPointers();SaveNow();}}
        private void OnApplicationQuit(){if(World!=null){CancelPointers();SaveNow();}}
        private Vector2 ToBoard(float x,float y)=>new Vector2((x/SoloWorld.Width-.5f)*Board.rect.width,(y/SoloWorld.Height-.5f)*Board.rect.height);
        private void Render()
        {
            if(avatar==null)return;
            var toyStates=World.ReadToys();var p=World.ReadPlayer(Actor);avatar.anchoredPosition=ToBoard(p.x,p.y);
            head.color=p.avatar=="blue-pup"?new Color(.35f,.65f,.85f):new Color(.94f,.58f,.31f);body.color=head.color;
            foreach(var t in toyStates){if(t.id!=dragging)toys[t.id].anchoredPosition=ToBoard(t.x,t.y);
                if(t.kind==ToyKind.Bucket)fills[t.id].rectTransform.sizeDelta=new Vector2(58,5+13*t.water);
                if(t.kind==ToyKind.Plant)fills[t.id].gameObject.SetActive(t.water==3);
                if(t.kind==ToyKind.Puddle)fills[t.id].rectTransform.localScale=Vector3.one*(t.water/3f);
            }
            // Larger y is farther back on the illustrated floor plane.
            foreach(var rect in toys.Where(pair=>pair.Key!=dragging).Select(pair=>pair.Value).Concat(new[]{avatar}).OrderByDescending(r=>r.anchoredPosition.y))rect.SetAsLastSibling();
            if(dragging!=null)toys[dragging].SetAsLastSibling();
            activity.text=p.activity==""?"Free play · walk, drag, discover":p.activity=="garden"?(toyStates.First(t=>t.kind==ToyKind.Plant).water==3?"Your flower is happy! Keep exploring.":"Give the flower a drink"):(toyStates.First(t=>t.kind==ToyKind.Puddle).water==0?"All tidy! Keep exploring.":"Soak up the puddle");
        }
        private void DrawToy(SoloToy t)
        {
            var root=Rect(Board,t.id,ToBoard(t.x,t.y),new Vector2(125,115));toys.Add(t.id,root);
            var hit=root.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=t.kind==ToyKind.Bucket || t.kind==ToyKind.Sponge;
            // The graphic must stay in the raycast list after transparent-mesh culling.
            hit.canvasRenderer.cullTransparentMesh=false;
            if(hit.raycastTarget)Surface(root,t.id);
            if(t.kind==ToyKind.Bucket){Panel(root,"Handle",new Vector2(0,27),new Vector2(65,50),Ink,false,true);Panel(root,"Handle hole",new Vector2(0,29),new Vector2(52,40),new Color(.76f,.89f,.72f),false,true);Panel(root,"Bucket",Vector2.zero,new Vector2(80,70),new Color(.98f,.67f,.28f));fills[t.id]=Panel(root,"Water",new Vector2(0,3),new Vector2(58,10),new Color(.32f,.68f,.91f));}
            if(t.kind==ToyKind.Sponge){Panel(root,"Sponge",Vector2.zero,new Vector2(92,53),new Color(1,.87f,.39f));for(var i=0;i<4;i++)Panel(root,"Hole",new Vector2(-27+i*18,(i%2)*15-8),new Vector2(8,8),new Color(.78f,.58f,.25f),false,true);}
            if(t.kind==ToyKind.Tap){Panel(root,"Tap pipe",new Vector2(-14,5),new Vector2(28,100),new Color(.47f,.61f,.68f));Panel(root,"Spout",new Vector2(14,40),new Vector2(74,26),new Color(.59f,.71f,.76f));Panel(root,"Handle",new Vector2(-14,66),new Vector2(67,18),new Color(.29f,.5f,.61f));Panel(root,"Drop",new Vector2(40,6),new Vector2(20,28),new Color(.29f,.65f,.88f),false,true);}
            if(t.kind==ToyKind.Plant){Panel(root,"Stem",new Vector2(0,23),new Vector2(10,79),new Color(.27f,.51f,.29f));Panel(root,"Leaf",new Vector2(-19,32),new Vector2(40,20),new Color(.38f,.66f,.33f),false,true);Panel(root,"Pot",new Vector2(0,-22),new Vector2(76,54),new Color(.8f,.43f,.3f));fills[t.id]=Panel(root,"Bloom",new Vector2(0,64),new Vector2(68,68),new Color(.96f,.52f,.61f),false,true);Panel(fills[t.id].transform,"Pollen",Vector2.zero,new Vector2(26,26),new Color(1,.84f,.35f),false,true);}
            if(t.kind==ToyKind.Puddle)fills[t.id]=Panel(root,"Puddle",Vector2.zero,new Vector2(126,49),new Color(.41f,.73f,.86f),false,true);
            Label(root,t.kind.ToString(),18,new Vector2(0,-70),new Vector2(135,32));
        }
        private void DrawAvatar()
        {
            avatar=Rect(Board,"Pup",Vector2.zero,new Vector2(85,136));
            Panel(avatar,"Shadow",new Vector2(0,-45),new Vector2(92,22),new Color(.27f,.41f,.26f,.3f),false,true);
            body=Panel(avatar,"Body",new Vector2(0,-8),new Vector2(61,75),Color.white);
            head=Panel(avatar,"Head",new Vector2(0,50),new Vector2(87,72),Color.white);
            Panel(head.transform,"Ear",new Vector2(-30,43),new Vector2(24,43),new Color(.23f,.38f,.48f));
            Panel(head.transform,"Ear",new Vector2(30,43),new Vector2(24,43),new Color(.23f,.38f,.48f));
            Panel(head.transform,"Muzzle",new Vector2(0,-10),new Vector2(67,32),Cream);
            foreach(var x in new[]{-20,20}){Panel(head.transform,"Eye",new Vector2(x,10),new Vector2(22,27),Color.white,false,true);Panel(head.transform,"Pupil",new Vector2(x,8),new Vector2(9,13),Ink,false,true);}
            Panel(head.transform,"Nose",new Vector2(0,-9),new Vector2(20,13),Ink,false,true);
        }
        private void Surface(RectTransform rect,string role){var s=rect.gameObject.AddComponent<SoloPointerSurface>();s.Screen=this;s.Role=role;Surfaces.Add(role,s);}
        private RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchoredPosition=position;r.sizeDelta=size;return r;}
        private Image Panel(Transform parent,string name,Vector2 position,Vector2 size,Color color,bool raycast=false,bool oval=false)
        {var img=Rect(parent,name,position,size).gameObject.AddComponent<Image>();img.sprite=oval?circle:rounded;img.type=oval?Image.Type.Simple:Image.Type.Sliced;img.color=color;img.raycastTarget=raycast;return img;}
        private Text Label(Transform parent,string text,int size,Vector2 position,Vector2 dimensions)
        {var label=Rect(parent,text,position,dimensions).gameObject.AddComponent<Text>();label.font=font;label.fontSize=size;label.text=text;label.color=Ink;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;return label;}
        private Text Button(Transform parent,string text,Vector2 position,Vector2 size,UnityEngine.Events.UnityAction action,Color color)
        {var image=Panel(parent,text,position,size,color,true);var button=image.gameObject.AddComponent<Button>();button.onClick.AddListener(action);return Label(image.transform,text,22,Vector2.zero,size);}
        private Sprite Shape(bool oval)
        {
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);texture.filterMode=FilterMode.Bilinear;
            for(var y=0;y<64;y++)for(var x=0;x<64;x++){var dx=Mathf.Abs(x-31.5f);var dy=Mathf.Abs(y-31.5f);var d=oval?Mathf.Sqrt(dx*dx+dy*dy)-31:Mathf.Sqrt(Mathf.Pow(Mathf.Max(0,dx-18),2)+Mathf.Pow(Mathf.Max(0,dy-18),2))-13;texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(1-d)));}
            texture.Apply();return Sprite.Create(texture,new Rect(0,0,64,64),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,oval?Vector4.zero:new Vector4(16,16,16,16));
        }
        private void UpdateSafeArea(){lastSafeArea=Screen.safeArea;safe.anchorMin=new Vector2(lastSafeArea.xMin/Screen.width,lastSafeArea.yMin/Screen.height);safe.anchorMax=new Vector2(lastSafeArea.xMax/Screen.width,lastSafeArea.yMax/Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;}
    }
}
