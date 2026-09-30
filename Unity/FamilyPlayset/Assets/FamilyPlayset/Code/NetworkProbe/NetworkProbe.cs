using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LittleWeeps.Core;
using LittleWeeps.Adapters;
using LittleWeeps.Client;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace LittleWeeps.NetworkProbe
{
    // Loopback qualification remains the default. An explicit protected family
    // enrollment enables the separate Windows LAN proof; never widen the lab bind.
    public sealed partial class NetworkProbe : MonoBehaviour
    {
        // Unity includes empty inline food records. Bound the full four-room,
        // twelve-dish reliable view separately from the 1200-byte motion stream.
        private const int Protocol=3, Content=WorldLayout.Content, MaxWireBytes=131072;
        private const string WalkMessage="littleweeps.walk.v1", MotionMessage="littleweeps.motion.v1";
        private const string CommandMessage="littleweeps.probe.command.v1", StateMessage="littleweeps.probe.state.v1", PoseMessage="littleweeps.probe.pose.v1";
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false,true);
        private Config config;
        private string root,output,epoch;
        private NetworkManager network;
        private UnityTransport transport;
        private FamilyPairing pairing;
        private IFamilyDiscovery discovery;
        private double discoveryDeadline,admissionDeadline;
        private bool guestStarted,presentationStarted,localOnly,localRequested;
        private SoloScreen familyScreen;
        private NetworkGardenSession familyGarden;
        private bool clientReady,retryPending,applicationPaused,reconnectBlocked;
        private double nextRetry;
        private int reconnectAttempts;
        public bool Reconnecting=>pairing!=null && presentationStarted && !localOnly && !ConnectedToServer && !reconnectBlocked && !stopping && !failed;
        public bool FamilyLan=>pairing!=null;
        private double previousFrame,nextConnectionEvidence,lastTransportData;
        private double maxFrameGap;
        private long receivedDataEvents,receivedDataBytes;
        private readonly List<ConnectionTrace> connectionTrace=new List<ConnectionTrace>();
        [Serializable] private sealed class ConnectionTrace {public string utc,phase,detail;public double seconds;public ulong peer;}
        [Serializable] private sealed class ConnectionEvidence
        {
            public bool listening,connected,driverCreated,driverBound,driverListening;
            public int receiveError;public double seconds,maxFrameGap,lastDataAge;
            public long receivedDataEvents,receivedDataBytes;public string[] profiles;public ConnectionTrace[] events;
        }
        private FamilySession session;
        private MovementAuthority movement;
        private double motionClock,accumulator,nextMotionSend,lastPositionSave,nextMotionEvidence;
        private long motionSequence,seenMotionSequence;
        private bool positionDirty;
        private bool maintenanceDirty;
        private double lastMaintenanceSave;
        private int checkpointWrites,motionPackets,diagnosticWriteConflicts;
        private readonly Dictionary<string,double> positionTimes=new Dictionary<string,double>();
        private readonly Dictionary<string,long> inputAcks=new Dictionary<string,long>();
        private readonly List<(double due,MotionFrame frame)> delayedFrames=new List<(double,MotionFrame)>();
        private int receivedMotionPackets;
        public double ServerClock=>network==null?0:network.ServerTime.Time;
        public double PositionTime(string actor)=>positionTimes.TryGetValue(actor,out var t)?t:0;
        public long InputAck(string actor)=>inputAcks.TryGetValue(actor,out var ack)?ack:0;
        public event Action MotionReceived;
        [Serializable] public sealed class MovingPlayer {public string actor,zone;public long visit,input;public float x,y;public double stairs;}
        [Serializable] public sealed class MotionFrame {public string epoch;public long sequence;public double time;public MovingPlayer[] players;public KeepyState keepy;public ParkState park;public DinosaurMotion dinosaurs;}
        [Serializable] public sealed class DinosaurMotion {public double clock;public int[] points;public int left,wandering,riders;}
        [Serializable] private sealed class MotionMetrics {public int checkpointWrites,motionPackets,diagnosticWriteConflicts;public double seconds;}
        private CheckpointStore store;
        private FileStream authorityLock;
        private long sequence,seenSequence;
        private int controlSerial;
        private double nextControl,started;
        private bool failed,stopping;
        private string lastParentRequest;
        [Serializable] private sealed class ParentRequest {public string requestId,instanceId,kind;}
        [Serializable] private sealed class ParentResponse {public string requestId,instanceId,result;public int players;}
        private readonly Dictionary<string,DragPose> poses=new Dictionary<string,DragPose>();
        public State Latest {get;private set;}
        public Config Settings=>config;
        public string Output=>output;
        public bool ConnectedToServer=>network!=null && network.IsConnectedClient && clientReady && !failed && !stopping;
        public string ConnectionStatus {get;private set;}="Connecting…";
        public event Action<State> Received;
        public event Action LostConnection;
        [Serializable] public sealed class DragPose {public string actor,item,lease;public float x,y;public long tick;}
        [Serializable] public sealed class Slot {public string profile,token;}
        [Serializable] public sealed class Config
        {
            public string runId,instanceId,role,profile,token,pairingPath;
            public int port,protocol=Protocol,content=Content;
            public Slot[] slots;
            public bool presentation,verifyGarden,interactive,persistentServer;
            public double testLifetimeOffsetSeconds;
            public int testMotionDelayMs,testMotionJitterMs,testMotionDropEvery;
        }
        [Serializable] private sealed class Hello {public string runId,profile,token,family,authority;public int protocol,content,recovery,activity;}
        [Serializable] public sealed class Control {public int serial;public string kind;public Request request;}
        [Serializable] public sealed class Request {public string requestId;public int protocol=Protocol;public SoloCommand command;}
        [Serializable] public sealed class State
        {
            public double time;
            public int protocol=Protocol,content=Content;
            public int recovery;
            public string runId,epoch,requestId,outcome;
            public long sequence;
            public bool accepted,duplicate,durable;
            public SoloSnapshot view;
            public string[] connected;
            public DragPose[] poses;
        }
        [Serializable] private sealed class Status {public string role,runId,instanceId,build,status,reason;public int pid;public bool persistentServer;}

        private void Start()
        {
            Application.runInBackground=true;Application.targetFrameRate=60;started=Time.realtimeSinceStartupAsDouble;
            try
            {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                #if UNITY_IOS
                pairing=AppleEnrollment.Load(out var enrollmentStatus);
#else
                pairing=AndroidEnrollment.Load(out var enrollmentStatus);
#endif
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath,"FamilyLAN"));
                // Only a coarse reason is recorded; credential/certificate data
                // never enters status files. An unpaired update keeps solo usable.
                File.WriteAllText(Path.Combine(Application.persistentDataPath,"FamilyLAN","enrollment-status.txt"),enrollmentStatus);
                if(pairing==null){EnsureLocalPresentation();gameObject.AddComponent<LittleWeeps.Client.SoloScreen>();enabled=false;return;}
                config=new Config{runId=pairing.worldId,instanceId=Guid.NewGuid().ToString("N"),role="client",profile=pairing.profile,token=pairing.credential,port=1025,presentation=true,interactive=true};
                root=Path.Combine(Application.persistentDataPath,"FamilyLAN",config.runId);
#else
                var args=Environment.GetCommandLineArgs();var index=Array.IndexOf(args,"-familyNetworkConfig");
                if(index<0 || index+1>=args.Length)throw new ArgumentException("Explicit isolated network config required.");
                var path=Path.GetFullPath(args[index+1]);root=Path.GetDirectoryName(path);
                if(new FileInfo(path).Length>8192)throw new InvalidDataException("Configuration too large.");
                config=JsonUtility.FromJson<Config>(File.ReadAllText(path,Utf8));
                if(config==null || !Guid.TryParseExact(config.runId,"N",out _) || !Guid.TryParseExact(config.instanceId,"N",out _) ||
                    new DirectoryInfo(root).Name!=config.runId || config.port<1024 || config.port>65535 ||
                    (config.role!="server" && config.role!="client"))throw new InvalidDataException("Invalid isolated configuration.");
                if(!string.IsNullOrEmpty(config.pairingPath))
                {
                    pairing=JsonUtility.FromJson<FamilyPairing>(WindowsPairingVault.Read(config.pairingPath));
                    if(pairing==null)throw new InvalidDataException("Missing enrollment.");pairing.Validate();
                    if(pairing.role!=config.role || pairing.worldId!=config.runId)throw new InvalidDataException("Enrollment does not match this world/role.");
                    if(config.role=="server")config.slots=pairing.members.Select(m=>new Slot{profile=m.profile,token=m.credentialHash}).ToArray();
                    else {config.profile=pairing.profile;config.token=pairing.credential;}
                }
#endif
                output=Path.Combine(root,config.instanceId);Directory.CreateDirectory(output);
                if(config.persistentServer && (pairing==null || config.role!="server" || !config.interactive))
                    throw new InvalidDataException("Persistent hosting requires an enrolled interactive authority.");
                if(double.IsNaN(config.testLifetimeOffsetSeconds) || double.IsInfinity(config.testLifetimeOffsetSeconds) ||
                    config.testLifetimeOffsetSeconds<0 || config.testLifetimeOffsetSeconds>86400 ||
                    (config.testLifetimeOffsetSeconds!=0 && !config.verifyGarden))
                    throw new InvalidDataException("Lifetime offset requires an isolated verification config.");
                File.WriteAllText(Path.Combine(root,"latest-instance.txt"),config.instanceId);
                var go=new GameObject("Loopback Network",typeof(NetworkManager),typeof(UnityTransport));
                network=go.GetComponent<NetworkManager>();transport=go.GetComponent<UnityTransport>();
                transport.OnTransportEvent+=TraceTransport;
                network.OnServerStopped+=host=>TraceConnection("server-stopped",0,host.ToString());
                network.OnClientStopped+=host=>TraceConnection("client-stopped",0,host.ToString());
                network.OnTransportFailure+=()=>TraceConnection("transport-failure",0,"");
                // Explicit loopback bind prevents this development protocol from
                // becoming a LAN service. Tokens below are test credentials only.
                transport.SetConnectionData(true,"127.0.0.1",(ushort)config.port,"127.0.0.1");
                if(pairing!=null)
                {
                    transport.UseEncryption=true;
                    if(config.role=="server")
                    {
                        transport.SetServerSecrets(pairing.certificate,pairing.privateKey);
                        transport.SetConnectionData(true,"127.0.0.1",(ushort)config.port,"0.0.0.0");
                    }
                    else transport.SetClientSecrets(pairing.serverName,pairing.caCertificate);
                }
                transport.MaxPayloadSize=MaxWireBytes;transport.DisconnectTimeoutMS=2500;transport.HeartbeatTimeoutMS=400;
                // Four reliable windows plus motion/recovery need headroom in
                // the per-frame packet queue. This does not enlarge saved data.
                transport.MaxPacketQueueSize=512;
                // Runtime-created managers have no inspector-serialized config.
                network.NetworkConfig=new NetworkConfig{NetworkTransport=transport};
                network.NetworkConfig.EnableSceneManagement=false;network.NetworkConfig.ConnectionApproval=true;
                network.NetworkConfig.ForceSamePrefabs=false;network.NetworkConfig.TickRate=30;
                network.OnClientConnectedCallback+=Connected;network.OnClientDisconnectCallback+=Disconnected;
                if(config.role=="server")StartAuthority();
                else if(pairing==null)StartGuest();
                else
                {
                    ShowLocal();
                    try
                    {
                        discovery=CreateDiscovery();discovery.Browse();
                        discoveryDeadline=Time.realtimeSinceStartupAsDouble+10;WriteStatus("discovering","");
                    }
                    catch(Exception){BeginReconnect("discovery-unavailable");}
                }
            }
            catch(Exception e){Fail(e);}
        }
        private IFamilyDiscovery CreateDiscovery()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return new AppleBonjour(pairing,Protocol,Content);
#elif UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidBonjour(pairing,Protocol,Content);
#else
            return new WindowsBonjour(pairing,Protocol,Content);
#endif
        }
        private void EnsureLocalPresentation()
        {
            if(FindAnyObjectByType<AudioListener>()==null)gameObject.AddComponent<AudioListener>();
            if(FindAnyObjectByType<Camera>()==null)
            {
                var camera=new GameObject("Offline Garden Camera",typeof(Camera)).GetComponent<Camera>();
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.95f,.94f,.86f);camera.transform.position=new Vector3(0,0,-10);
            }
        }
        private void RegisterMessages()
        {
            network.CustomMessagingManager.RegisterNamedMessageHandler(CommandMessage,ReceiveCommand);
            network.CustomMessagingManager.RegisterNamedMessageHandler(StateMessage,ReceiveState);
            network.CustomMessagingManager.RegisterNamedMessageHandler(PoseMessage,ReceivePose);
            network.CustomMessagingManager.RegisterNamedMessageHandler(WalkMessage,ReceiveWalk);
            network.CustomMessagingManager.RegisterNamedMessageHandler(MotionMessage,ReceiveMotion);
            network.CustomMessagingManager.RegisterNamedMessageHandler(ActivityMessage,ReceiveActivity);
            RegisterRecoveryMessages();
        }
        private void ShowShared()
        {
            if(pairing!=null)return; // Paired play switches only after a valid snapshot and safe local checkpoint.
            if(presentationStarted || !config.presentation)return;presentationStarted=true;
            var garden=gameObject.AddComponent<NetworkGardenSession>();garden.Initialize(this);
            var screen=gameObject.AddComponent<LittleWeeps.Client.SoloScreen>();screen.Configure(garden);
            if(config.verifyGarden)gameObject.AddComponent<NetworkGardenVerification>();
        }
        private void ShowLocal()
        {
            if(!config.presentation || familyScreen!=null)return;
            EnsureLocalPresentation();
            familyGarden=gameObject.AddComponent<NetworkGardenSession>();familyGarden.Initialize(this);
            familyScreen=gameObject.AddComponent<SoloScreen>();
            familyScreen.ConfigureOfflineBranch(config.runId,config.profile);
            familyScreen.ConfigureFamilyMode(RequestFamilyMode,config.verifyGarden);
            ConfigureContinuation();
            if(config.verifyGarden)gameObject.AddComponent<NetworkGardenVerification>();
        }
        private void RequestFamilyMode()
        {
            familyScreen.SetMenu(false);
            if(familyScreen.Shared){localRequested=true;return;}
            if(reconnectBlocked)return; // Explicit revocation needs new parent enrollment, not button retries.
            localOnly=false;
            if(retryPending)nextRetry=Time.realtimeSinceStartupAsDouble+.25;
        }
        private void TickPresentation()
        {
            if(familyScreen==null)return;
            var localReady=outageClock.Tick(Time.realtimeSinceStartupAsDouble,!applicationPaused,!ConnectedToServer && !localOnly && !reconnectBlocked);
            if(applicationPaused)return;
            if(requestedAdventure!=null)
            {
                if(!familyScreen.CanChangeSession)return;
                var selected=requestedAdventure;requestedAdventure=null;
                if(selected==""?familyScreen.TryReturnToLocal():familyScreen.TryOpenAdventure(selected))
                {localOnly=true;localRequested=false;BeginReconnect("adventure-selected");WriteStatus("solo-selected","");}
                return;
            }
            if(localRequested)
            {
                if(!familyScreen.TryReturnToLocal())return;
                localOnly=true;localRequested=false;BeginReconnect("solo-selected");
                WriteStatus("solo-selected","");TraceConnection("local-restored",0,"");
            }
            else if(!localOnly && ConnectedToServer && !familyScreen.Shared && familyScreen.TryJoinFamily(familyGarden))
            {localStart=null;presentationStarted=true;TraceConnection("shared-presented",0,"local-checkpoint-preserved");}
            else if(localReady && familyScreen.RecoveringDisconnected)TryLocalContinuation();
        }
        private void TickDiscovery()
        {
            if(pairing==null)return;
            if(config.role=="client" && (applicationPaused || reconnectBlocked || localOnly))return;
            if(retryPending)
            {
                // NGO disposes its driver asynchronously. Never start a new
                // client from inside the old driver's disconnect callback.
                if(network.ShutdownInProgress || network.IsListening || Time.realtimeSinceStartupAsDouble<nextRetry)return;
                retryPending=false;guestStarted=false;
                epoch=null;seenSequence=0;seenMotionSequence=0;
                positionTimes.Clear();inputAcks.Clear();delayedFrames.Clear();
                discovery=CreateDiscovery();discovery.Browse();
                discoveryDeadline=Time.realtimeSinceStartupAsDouble+10;
                WriteStatus("rediscovering","");
            }
            discovery?.Tick(Time.realtimeSinceStartupAsDouble);
            if(config.role=="server")return;
            if(!guestStarted)
            {
                var endpoint=discovery?.Take();
                if(endpoint!=null)
                {
                    transport.SetConnectionData(true,endpoint.address,endpoint.port);
                    TraceConnection("discovered",0,"native-bonjour-ipv4");StartGuest();
                }
                else if(discovery?.Error!=0 || Time.realtimeSinceStartupAsDouble>discoveryDeadline)Unavailable("discovery-unavailable");
            }
            else if(!ConnectedToServer && Time.realtimeSinceStartupAsDouble>admissionDeadline)Unavailable("admission-unavailable");
        }
        private void Unavailable(string reason)
        {
            if(pairing!=null)BeginReconnect(reason);
        }
        private void BeginReconnect(string reason)
        {
            if(config.role!="client" || pairing==null || stopping || failed || retryPending || reconnectBlocked)return;
            RememberVisibleLocalStart();
            clientReady=false;retryPending=true;ResetRecoveryTransfer();
            // Empty reason is ordinary transport loss. Explicit authentication
            // or compatibility rejection needs parent action, not an attack loop.
            reconnectBlocked=reason=="unpaired-profile" || reason=="incompatible-version" || reason=="unknown-profile" || reason=="invalid-admission" || reason=="invalid-message" || reason=="invalid-walk";
            nextRetry=Time.realtimeSinceStartupAsDouble+Math.Min(15,Math.Pow(2,Math.Min(4,++reconnectAttempts)));
            discovery?.Dispose();discovery=null;delayedFrames.Clear();
            LostConnection?.Invoke();network.Shutdown();
            WriteStatus(reconnectBlocked?"needs-parent":familyScreen!=null && !familyScreen.Shared?"solo-available":"reconnecting",reason);
            TraceConnection("reconnect-scheduled",0,reconnectBlocked?"blocked":reconnectAttempts.ToString());
        }
        private void StartAuthority()
        {
            if(config.slots==null || config.slots.Length!=4 || config.slots.Any(s=>s==null || string.IsNullOrWhiteSpace(s.profile) || s.profile.Length>64 || s.token==null || s.token.Length!=64) || config.slots.Select(s=>s.profile).Distinct().Count()!=4)
                throw new InvalidDataException("Exactly four distinct test profiles required.");
            var folder=Path.Combine(root,"server-world");Directory.CreateDirectory(folder);
            authorityLock=File.Open(Path.Combine(folder,"authority.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            // An interrupted parent restore must be resolved before any writer
            // loads a possibly mixed checkpoint/backup set, even via direct launch.
            if(File.Exists(Path.Combine(root,"recovery.pending.json")))throw new InvalidDataException("Parent recovery is incomplete; server start is blocked.");
            store=new CheckpointStore(Path.Combine(folder,"world.save"),ValidSave);
            var saved=store.Load();
            if(saved.Status==CheckpointStatus.Corrupt || saved.Status==CheckpointStatus.Unsupported)throw new InvalidDataException("Server checkpoint is blocked.");
            var world=saved.Status==CheckpointStatus.Missing?SoloWorld.Create(config.slots.Select(s=>s.profile).ToArray()):SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(saved.Payload));
            world=SoloWorld.WithDinosaurWorld(world);
            if(saved.Status==CheckpointStatus.Missing && config.presentation)
                for(var i=0;i<config.slots.Length;i++)world.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=config.slots[i].profile,expectedRevision=world.Revision,action=SoloAction.Move,x=280+i*180,y=100});
            if(!world.Snapshot().players.Select(p=>p.id).OrderBy(s=>s).SequenceEqual(config.slots.Select(s=>s.profile).OrderBy(s=>s)))throw new InvalidDataException("Roster does not match checkpoint.");
            session=new FamilySession(world);movement=new MovementAuthority(world,session);epoch=Guid.NewGuid().ToString("N");SaveAuthority();
            motionClock=Time.realtimeSinceStartupAsDouble;nextMotionSend=motionClock;
            network.ConnectionApprovalCallback=Approve;
            if(!network.StartServer())throw new InvalidOperationException("Loopback server did not start.");
            RegisterMessages();
            if(pairing!=null)
            {
                var advertiser=new WindowsBonjour(pairing,Protocol,Content);discovery=advertiser;advertiser.Advertise((ushort)config.port);
            }
            WriteStatus("listening","");Publish();
        }
        private void StartGuest()
        {
            clientReady=false;
            var hello=new Hello{runId=config.runId,profile=config.profile,token=config.token,protocol=config.protocol,content=config.content,family=pairing?.familyId,authority=pairing?.authorityId,recovery=1,activity=1};
            network.NetworkConfig.ConnectionData=Utf8.GetBytes(JsonUtility.ToJson(hello));
            if(!network.StartClient())throw new InvalidOperationException("Loopback client did not start.");
            guestStarted=true;admissionDeadline=Time.realtimeSinceStartupAsDouble+10;RegisterMessages();
            if(pairing==null)ShowShared();
            WriteStatus("connecting","");
        }
        private void Approve(NetworkManager.ConnectionApprovalRequest request,NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved=false;response.CreatePlayerObject=false;response.Pending=false;response.Reason="invalid-admission";
            try
            {
                if(stopping || failed){response.Reason="server-stopping";return;}
                if(request.Payload==null || request.Payload.Length>1024)return;
                var hello=JsonUtility.FromJson<Hello>(Utf8.GetString(request.Payload));
                if(hello==null || hello.runId!=config.runId)return;
                if(hello.protocol!=Protocol || hello.content!=Content){response.Reason="incompatible-version";return;}
                var slot=config.slots.FirstOrDefault(s=>s.profile==hello.profile);
                var admitted=pairing==null?slot!=null && slot.token==hello.token:
                    transport.UseEncryption && pairing.Admits(hello.family,hello.authority,hello.runId,hello.profile,hello.token);
                if(!admitted){response.Reason="unpaired-profile";return;}
                response.Approved=session.Attach(request.ClientNetworkId,hello.profile,out var reason);response.Reason=reason;
                if(response.Approved && hello.recovery==1)recoveryPeers[request.ClientNetworkId]=new RecoveryPeer();
                if(response.Approved && hello.activity==1)activityPeers.Add(request.ClientNetworkId);
            }
            catch(ArgumentException){response.Reason="invalid-admission";}
            finally{TraceConnection(response.Approved?"approved":"rejected",request.ClientNetworkId,response.Reason);}
        }
        private void Connected(ulong client)
        {
            TraceConnection("connected",client,"");
            if(config.role=="server")Publish();
            else if(client==network.LocalClientId)
            {discovery?.Dispose();discovery=null;WriteStatus("synchronizing","");ShowShared();}
        }
        private void Disconnected(ulong client)
        {
            TraceConnection("disconnected",client,network.DisconnectReason??"");
            if(stopping || failed)return;
            try
            {
                if(config.role=="server")
                {recoveryPeers.Remove(client);activityPeers.Remove(client);if(session.TryPlayer(client,out var actor))movement.Forget(actor);if(session.Detach(client)){SaveAuthority();Publish();}}
                else if(client==network.LocalClientId)
                {
                    clientReady=false;ResetRecoveryTransfer();
                    if(pairing!=null)BeginReconnect(network.DisconnectReason??"");
                    else {WriteStatus("disconnected",network.DisconnectReason??"");LostConnection?.Invoke();}
                }
            }
            catch(Exception e){Fail(e);}
        }
        private State Current(string requestId="",SoloResult? result=null)
        {
            return new State{runId=config.runId,epoch=epoch,time=ServerClock,sequence=++sequence,requestId=requestId,recovery=1,
                accepted=result?.Accepted??false,duplicate=result?.Duplicate??false,outcome=result?.Outcome??"snapshot",
                durable=!positionDirty,view=session.View(),connected=session.ConnectedPlayers,poses=poses.Values.ToArray()};
        }
        private void Publish(ulong? acknowledgedPeer=null)
        {
            foreach(var item in poses.Keys.ToArray())if(!session.Checkpoint().toys.Any(t=>t.id==item && t.holder==poses[item].actor))poses.Remove(item);
            var state=Current();WriteJson(Path.Combine(output,"view.json"),state);
            // The command acknowledgement already carries the same complete
            // state to its actor. Do not enqueue a second fragmented copy.
            foreach(var peer in network.ConnectedClientsIds)if(peer!=acknowledgedPeer)Send(StateMessage,peer,state);
        }
        private void ReceiveCommand(ulong sender,FastBufferReader reader)
        {
            if(config.role!="server" || stopping || failed)return;
            try
            {
                var request=JsonUtility.FromJson<Request>(Read(reader));
                if(request==null || !Guid.TryParseExact(request.requestId,"N",out _))throw new InvalidDataException("Bad request identity.");
                var result=request.protocol!=Protocol?new SoloResult(false,"incompatible-version",session.Checkpoint().revision):
                    request.command==null || request.command.requestId!=request.requestId?new SoloResult(false,"invalid-command",session.Checkpoint().revision):session.Submit(sender,request.command);
                if(result.Accepted && !result.Duplicate)
                {
                    if(request.command.action==SoloAction.Travel || request.command.action==SoloAction.Move || request.command.action==SoloAction.UseFixture || request.command.action==SoloAction.Dinosaur && request.command.value!="call")movement.Forget(request.command.actor);
                    SaveAuthority();
                    if(request.command.action==SoloAction.Grab)
                    {
                        var toy=session.Checkpoint().toys.First(t=>t.id==request.command.item);
                        poses[toy.id]=new DragPose{actor=toy.holder,item=toy.id,lease=request.requestId,x=toy.x,y=toy.y};
                    }
                    else if(request.command.action==SoloAction.Drop || request.command.action==SoloAction.CancelGrab)poses.Remove(request.command.item);
                    else if(request.command.action==SoloAction.Travel)
                        foreach(var item in poses.Keys.Where(id=>poses[id].actor==request.command.actor).ToArray())poses.Remove(item);
                }
                Send(StateMessage,sender,Current(request.requestId,result));
                if(result.Accepted && !result.Duplicate)Publish(sender);
            }
            catch(Exception e) when(e is ArgumentException || e is InvalidDataException || e is OverflowException)
            {network.DisconnectClient(sender,"invalid-message");}
            catch(Exception e){Fail(e);}
        }
        private double keepyTime,parkTime,dinosaurTime;
        private void ReceiveState(ulong sender,FastBufferReader reader)
        {
            if(config.role!="client" || sender!=NetworkManager.ServerClientId || failed || applicationPaused || retryPending || localOnly || !network.IsConnectedClient)return;
            try
            {
                var state=JsonUtility.FromJson<State>(Read(reader));
                if(state==null || state.protocol!=Protocol || state.content!=Content || state.runId!=config.runId || !Guid.TryParseExact(state.epoch,"N",out _))throw new InvalidDataException("Wrong server state.");
                SoloWorld.Validate(state.view);
                if(!state.view.players.Any(p=>p.id==config.profile))throw new InvalidDataException("Snapshot lacks the admitted player.");
                if(!string.IsNullOrEmpty(epoch) && state.epoch!=epoch)throw new InvalidDataException("Unexpected authority epoch.");
                epoch=state.epoch;
                if(!string.IsNullOrEmpty(state.requestId))
                {if(!Guid.TryParseExact(state.requestId,"N",out _))throw new InvalidDataException("Bad response identity.");WriteJson(Path.Combine(output,"reply-"+state.requestId+".json"),state);}
                if(state.sequence>seenSequence)
                {
                    PreserveActivityProgress(state);
                    foreach(var p in state.view.players)
                    {
                        var prior=Latest?.view.players.FirstOrDefault(v=>v.id==p.id);
                        if(prior!=null && prior.zone==p.zone && prior.visit==p.visit && PositionTime(p.id)>state.time){p.x=prior.x;p.y=prior.y;p.stairs=prior.stairs;}
                        else{positionTimes[p.id]=state.time;if(prior==null || prior.visit!=p.visit)inputAcks[p.id]=0;}
                    }
                    if(Latest?.view.dinosaurWorld!=null && state.view.dinosaurWorld!=null && dinosaurTime>state.time){
                        state.view.dinosaurWorld.clock=Latest.view.dinosaurWorld.clock;
                        for(var i=0;i<4;i++){var from=Latest.view.dinosaurWorld.animals[i];var to=state.view.dinosaurWorld.animals[i];to.x=from.x;to.y=from.y;to.left=from.left;to.wandering=from.wandering;}
                    }else dinosaurTime=state.time;
                    foreach(var mounted in state.view.players.Where(p=>DinosaurRides.Usable(p.fixture))){var mount=state.view.dinosaurWorld.animals.Single(a=>DinosaurRides.Fixture(a.species)==mounted.fixture);mount.x=mounted.x;mount.y=mounted.y;}
                    if(!clientReady)
                    {
                        TraceConnection("first-snapshot",sender,state.sequence.ToString());
                        clientReady=true;retryPending=false;reconnectAttempts=0;WriteStatus("connected","");
                    }
                    if(Latest!=null && Latest.epoch==state.epoch && keepyTime>state.time)state.view.keepy=Latest.view.keepy?.Copy();
                    else keepyTime=state.time;
                    if(Latest!=null && Latest.epoch==state.epoch && parkTime>state.time)state.view.park=Latest.view.park?.Copy();else parkTime=state.time;
                    seenSequence=state.sequence;Latest=state;WriteJson(Path.Combine(output,"view.json"),state);
                }
                Received?.Invoke(state);
            }
            catch(Exception e){Fail(e);}
        }
        public void Submit(SoloCommand command)
        {
            if(!ConnectedToServer)throw new InvalidOperationException("Client is not connected.");
            Send(CommandMessage,NetworkManager.ServerClientId,new Request{requestId=command.requestId,command=command});
        }
        public void DisconnectGuest()
        {
            if(config.role!="client")return;
            if(pairing!=null){BeginReconnect("response-timeout");return;}
            stopping=true;network.Shutdown();WriteStatus("disconnected","response-timeout");LostConnection?.Invoke();
        }
        public void Preview(DragPose pose)
        {if(ConnectedToServer)Send(PoseMessage,NetworkManager.ServerClientId,pose);}
        public void SendWalk(WalkInput input,bool stop)
        {if(ConnectedToServer)Send(WalkMessage,NetworkManager.ServerClientId,input,stop?NetworkDelivery.ReliableSequenced:NetworkDelivery.UnreliableSequenced);}
        private void ReceiveWalk(ulong sender,FastBufferReader reader)
        {
            if(config.role!="server" || stopping || failed)return;
            try{movement.Accept(sender,JsonUtility.FromJson<WalkInput>(Read(reader)),Time.realtimeSinceStartupAsDouble);}
            catch(ArgumentException){network.DisconnectClient(sender,"invalid-walk");}
            catch(InvalidDataException){network.DisconnectClient(sender,"invalid-walk");}
        }
        private void ReceiveMotion(ulong sender,FastBufferReader reader)
        {
            if(config.role!="client" || sender!=NetworkManager.ServerClientId || failed)return;
            try
            {
                var frame=JsonUtility.FromJson<MotionFrame>(Read(reader));
                // Explicit test-only receive impairment. Transactions/admission
                // are unchanged; this is not a simulation of an entire Wi-Fi link.
                receivedMotionPackets++;
                if(config.verifyGarden && config.testMotionDropEvery>0 && receivedMotionPackets%config.testMotionDropEvery==0)return;
                var jitter=config.verifyGarden?((receivedMotionPackets*37)%3-1)*config.testMotionJitterMs:0;
                var delay=config.verifyGarden?Math.Max(0,config.testMotionDelayMs+jitter):0;
                if(delay>0){if(delayedFrames.Count<128)delayedFrames.Add((Time.realtimeSinceStartupAsDouble+delay/1000.0,frame));}
                else ApplyMotion(frame);
            }
            catch(Exception e){Fail(e);}
        }
        private void ApplyMotion(MotionFrame frame)
        {
            if(Latest==null || frame==null || frame.epoch!=epoch || frame.sequence<=seenMotionSequence)return;
            if(frame.players==null || frame.players.Length>4 || double.IsNaN(frame.time) || double.IsInfinity(frame.time))throw new InvalidDataException("Invalid motion frame.");
            seenMotionSequence=frame.sequence;
            foreach(var sample in frame.players)
            {
                if(sample==null || !WorldLayout.Position(sample.zone,Latest.view.schema,sample.x,sample.y))throw new InvalidDataException("Invalid motion point.");
                var p=Latest.view.players.FirstOrDefault(v=>v.id==sample.actor);
                if(p==null || p.zone!=sample.zone || p.visit!=sample.visit || frame.time<=PositionTime(p.id))continue;
                // Leaving an authored slot is also conveyed by the motion lane.
                // A position frame can arrive before the reliable state update.
                if(!string.IsNullOrEmpty(p.fixture) && !DinosaurRides.Usable(p.fixture))
                {
                    var bedroom=BedroomFurniture.Seat(p.fixture)?SecretRooms.Furnishings(Latest.view).FirstOrDefault(r=>r.id==p.zone):null;
                    var supportX=bedroom!=null?BedroomFurniture.SeatX(p.fixture,bedroom.layout):ParkPlay.Usable(p.fixture)?ParkPlay.X(p.fixture):HomeLayout.X(p.fixture);
                    var supportY=bedroom!=null?BedroomFurniture.SeatY(p.fixture):ParkPlay.Usable(p.fixture)?ParkPlay.Y(p.fixture):HomeLayout.Y(p.fixture);
                    if(sample.x!=supportX || sample.y!=supportY){p.fixture="";p.useSeconds=0;p.rideStarted=0;}
                }
                if(!KeepyRules.Finite(sample.stairs) || sample.stairs<0 || sample.stairs>=HomeRooms.StairDuration)throw new InvalidDataException("Invalid stair sample.");
                p.x=sample.x;p.y=sample.y;p.stairs=sample.stairs;positionTimes[p.id]=frame.time;inputAcks[p.id]=sample.input;
            }
            if(frame.time>keepyTime && frame.keepy!=null)
            {
                SoloWorld.ValidateKeepy(new SoloSnapshot{schema=WorldLayout.Schema,players=Latest.view.players,keepy=frame.keepy});
                Latest.view.keepy=frame.keepy;keepyTime=frame.time;
            }
            if(frame.time>parkTime && frame.park!=null){Latest.view.park=frame.park;parkTime=frame.time;}
            if(frame.dinosaurs!=null && frame.dinosaurs.clock==0 && (frame.dinosaurs.points==null || frame.dinosaurs.points.Length==0) && frame.dinosaurs.left==0 && frame.dinosaurs.wandering==0 && frame.dinosaurs.riders==0)frame.dinosaurs=null; // Unity expands null inline objects.
            if(frame.time>dinosaurTime && frame.dinosaurs!=null && Latest.view.dinosaurWorld!=null){
                var d=frame.dinosaurs;
                if(!HideAndSeek.Finite(d.clock) || d.clock<0 || d.points==null || d.points.Length!=8 || d.left<0 || d.left>15 || d.wandering<0 || d.wandering>15 || d.riders<0 || d.riders>4095)throw new InvalidDataException("Invalid dinosaur motion.");
                for(var i=0;i<4;i++)if(!DinosaurRides.Point(d.points[i*2]/4f,d.points[i*2+1]/4f) || ((d.riders>>(i*3))&7)>Latest.view.players.Length)throw new InvalidDataException("Invalid dinosaur point or rider.");
                foreach(var p in Latest.view.players.Where(p=>DinosaurRides.Usable(p.fixture))){p.fixture="";p.useSeconds=0;}
                Latest.view.dinosaurWorld.clock=d.clock;
                for(var i=0;i<4;i++){
                    var a=Latest.view.dinosaurWorld.animals[i];a.x=d.points[i*2]/4f;a.y=d.points[i*2+1]/4f;a.left=(d.left&(1<<i))!=0;a.wandering=(d.wandering&(1<<i))!=0;
                    var seat=(d.riders>>(i*3))&7;
                    if(seat>0){var p=Latest.view.players[seat-1];if(p.zone==DinosaurRides.Area){p.fixture=DinosaurRides.Fixture(a.species);a.x=p.x;a.y=p.y;}}
                }
                dinosaurTime=frame.time;
            }
            MotionReceived?.Invoke();
            WriteJson(Path.Combine(output,"view.json"),Latest);
        }
        private void TickMovement(double now)
        {
            if(config.role=="client")
            {
                foreach(var pending in delayedFrames.Where(v=>v.due<=now).OrderBy(v=>v.due).ToArray()){delayedFrames.Remove(pending);ApplyMotion(pending.frame);}
                return;
            }
            var elapsed=Math.Min(.1,Math.Max(0,now-motionClock));
            accumulator+=elapsed;motionClock=now;var moved=false;var stepped=false;
            maintenanceDirty|=session.AdvanceIdle(elapsed,out var maintenanceVisible);
            if(maintenanceVisible){SaveAuthority();Publish();}
            else if(maintenanceDirty && now-lastMaintenanceSave>=5)SaveAuthority();
            var beforeWalkingRevision=session.Revision;
            while(accumulator>=1.0/30){stepped=true;moved|=movement.Tick(now,1f/30);accumulator-=1.0/30;}
            if(session.Revision!=beforeWalkingRevision){SaveAuthority();Publish();}
            positionDirty|=moved;
            if(positionDirty && ((stepped && !moved) || now-lastPositionSave>=1))SaveAuthority();
            if(now<nextMotionSend)return;nextMotionSend=Math.Max(nextMotionSend+.05,now);
            var view=session.View();
            // Positions describe the completed simulation step, not the later
            // packet-send instant. Otherwise 30 Hz simulation sampled at 20 Hz
            // creates an artificial alternating fast/slow interpolation speed.
            DinosaurMotion dinosaurMotion=null;
            if(view.dinosaurWorld!=null && view.players.Any(p=>p.zone==DinosaurRides.Area)){dinosaurMotion=new DinosaurMotion{clock=view.dinosaurWorld.clock,points=view.dinosaurWorld.animals.SelectMany(a=>new[]{(int)Math.Round(a.x*4),(int)Math.Round(a.y*4)}).ToArray()};
                for(var i=0;i<4;i++){var a=view.dinosaurWorld.animals[i];if(a.left)dinosaurMotion.left|=1<<i;if(a.wandering)dinosaurMotion.wandering|=1<<i;var rider=Array.FindIndex(view.players,p=>p.fixture==DinosaurRides.Fixture(a.species));dinosaurMotion.riders|=(rider+1)<<(i*3);}}
            var frame=new MotionFrame{dinosaurs=dinosaurMotion,epoch=epoch,sequence=++motionSequence,time=ServerClock-accumulator,keepy=view.keepy,park=view.park,players=view.players.Select(p=>new MovingPlayer{actor=p.id,zone=p.zone,visit=p.visit,x=p.x,y=p.y,stairs=p.stairs,input=movement.Acknowledged(p.id)}).ToArray()};
            foreach(var peer in network.ConnectedClientsIds){Send(MotionMessage,peer,frame,NetworkDelivery.UnreliableSequenced);motionPackets++;}
            // Diagnostics are deliberately not durable checkpoints.
            WriteJson(Path.Combine(output,"view.json"),Current());
            if(now>=nextMotionEvidence){nextMotionEvidence=now+1;WriteJson(Path.Combine(output,"motion-stats.json"),new MotionMetrics{checkpointWrites=checkpointWrites,motionPackets=motionPackets,diagnosticWriteConflicts=diagnosticWriteConflicts,seconds=now-started});}
        }
        private void ReceivePose(ulong sender,FastBufferReader reader)
        {
            if(config.role!="server" || stopping || failed)return;
            try
            {
                var pose=JsonUtility.FromJson<DragPose>(Read(reader));
                var view=session.View();
                if(pose==null || pose.item==null || !WorldLayout.Position(view.toys.FirstOrDefault(t=>t.id==pose.item)?.zone,view.schema,pose.x,pose.y) || !session.TryPlayer(sender,out var actor) ||
                    actor!=pose.actor || !poses.TryGetValue(pose.item,out var current) || current.actor!=actor ||
                    current.lease!=pose.lease || pose.tick<=current.tick)return;
                current.x=pose.x;current.y=pose.y;current.tick=pose.tick;activityDirty=true;
            }
            catch(ArgumentException){network.DisconnectClient(sender,"invalid-preview");}
            catch(InvalidDataException){network.DisconnectClient(sender,"invalid-preview");}
        }
        private void Send<T>(string name,ulong peer,T message,NetworkDelivery delivery=NetworkDelivery.ReliableFragmentedSequenced)
        {
            var bytes=Utf8.GetBytes(JsonUtility.ToJson(message));
            if(bytes.Length>MaxWireBytes-4)throw new InvalidDataException("Network message exceeds probe limit.");
            using var writer=new FastBufferWriter(bytes.Length+4,Allocator.Temp);
            writer.WriteValueSafe(bytes.Length);writer.WriteBytesSafe(bytes);
            if(delivery==NetworkDelivery.UnreliableSequenced && bytes.Length>1200)throw new InvalidDataException("Motion packet exceeds datagram budget.");
            network.CustomMessagingManager.SendNamedMessage(name,peer,writer,delivery);
        }
        private static string Read(FastBufferReader reader)
        {
            if(reader.Length<4 || reader.Length>MaxWireBytes)throw new InvalidDataException("Invalid message size.");
            reader.ReadValueSafe(out int count);
            if(count<1 || count!=reader.Length-reader.Position)throw new InvalidDataException("Invalid payload length.");
            var bytes=new byte[count];reader.ReadBytesSafe(ref bytes,count);return Utf8.GetString(bytes);
        }
        private void Update()
        {
            var started=System.Diagnostics.Stopwatch.GetTimestamp();
            try{UpdateNetwork();}
            finally{if(familyScreen!=null)familyScreen.RecordNetworkWork(started);}
        }
        private void UpdateNetwork()
        {
            if(config==null || output==null || failed || stopping)return;
            TickInterruptedArchives();
            try{TickDiscovery();}catch(Exception) when(pairing!=null && config.role=="client"){Unavailable("discovery-unavailable");}
            try{TickPresentation();}catch(Exception e){Fail(e);return;}
            UpdateConnectionEvidence();
            try{TickMovement(Time.realtimeSinceStartupAsDouble);}catch(Exception e){Fail(e);return;}
            try{TickActivity(Time.realtimeSinceStartupAsDouble);}catch(Exception e){Fail(e);return;}
            TickRecovery(Time.realtimeSinceStartupAsDouble);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            // Only explicit home hosting opts out; automated probes keep their
            // deadlines. The offset exercises this exact branch without a two-hour test.
            if(SessionLifetime.Expired(Time.realtimeSinceStartupAsDouble-started+config.testLifetimeOffsetSeconds,config.interactive,config.persistentServer))
            {Fail(new TimeoutException("Isolated probe lifetime exceeded."));return;}
#endif
            if(Time.realtimeSinceStartupAsDouble<nextControl)return;nextControl=Time.realtimeSinceStartupAsDouble+.04;
            try
            {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                return; // Desktop qualification control files are not a mobile command surface.
#else
                if(config.role=="server" && config.persistentServer && TickParentControl())return;
                var path=Path.Combine(output,"control.json");if(!File.Exists(path))return;
                if(new FileInfo(path).Length>4096)throw new InvalidDataException("Control too large.");
                string json;
                try
                {using var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);using var reader=new StreamReader(file,Utf8);json=reader.ReadToEnd();}
                catch(IOException){return;}
                var control=JsonUtility.FromJson<Control>(json);
                if(control==null || control.serial<=controlSerial)return;controlSerial=control.serial;
                if(control.kind=="quit"){Stop();return;}
                if(control.kind!="command" || config.role!="client" || !network.IsConnectedClient)throw new InvalidOperationException("Command needs a connected test client.");
                Send(CommandMessage,NetworkManager.ServerClientId,control.request);
#endif
            }
            catch(Exception e){Fail(e);}
        }
        // Only this authority can decide that stopping is safe: a desktop roster
        // check followed by an unconditional quit races with admission. Both this
        // decision and Approve run on Unity's main thread; Stop closes admission.
        private bool TickParentControl()
        {
            var path=Path.Combine(output,"parent-control.json");
            if(!File.Exists(path))return false;
            ParentRequest request;
            try
            {
                if(new FileInfo(path).Length>4096)return false;
                using var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);
                using var reader=new StreamReader(file,Utf8);
                request=JsonUtility.FromJson<ParentRequest>(reader.ReadToEnd());
            }
            catch(IOException){return false;}
            catch(ArgumentException){return false;}
            if(request==null || !Guid.TryParseExact(request.requestId,"N",out _) || request.requestId==lastParentRequest)return false;
            lastParentRequest=request.requestId;
            var count=session.ConnectedPlayers.Length;
            var result=request.instanceId!=config.instanceId || request.kind!="stop-if-empty"?"invalid-request":
                count>0 || network.ConnectedClientsIds.Count>0?"players-connected":"stopping";
            if(result=="stopping")SaveAuthority();
            WriteJson(Path.Combine(output,"parent-response.json"),new ParentResponse{requestId=request.requestId,instanceId=config.instanceId,result=result,players=count});
            if(result!="stopping")return false;
            Stop();return true;
        }
        private void SaveAuthority()
        {
            var checkpoint=session.Checkpoint();store.Save(JsonUtility.ToJson(checkpoint));
            CaptureRecovery(checkpoint);
            checkpointWrites++;positionDirty=false;maintenanceDirty=false;lastPositionSave=lastMaintenanceSave=Time.realtimeSinceStartupAsDouble;
        }
        private void TraceConnection(string phase,ulong peer,string detail)
        {
            if(connectionTrace.Count>=256)connectionTrace.RemoveAt(0);
            connectionTrace.Add(new ConnectionTrace{utc=DateTime.UtcNow.ToString("O"),seconds=Time.realtimeSinceStartupAsDouble,phase=phase,peer=peer,detail=detail});
            nextConnectionEvidence=0;
        }
        private void TraceTransport(NetworkEvent kind,ulong peer,ArraySegment<byte> payload,float receivedAt)
        {
            if(kind==NetworkEvent.Data){receivedDataEvents++;receivedDataBytes+=payload.Count;lastTransportData=Time.realtimeSinceStartupAsDouble;}
            else TraceConnection("transport-"+kind,peer,kind==NetworkEvent.Disconnect?transport.DisconnectEvent.ToString():"");
        }
        private void UpdateConnectionEvidence()
        {
            var now=Time.realtimeSinceStartupAsDouble;
            if(previousFrame>0)maxFrameGap=Math.Max(maxFrameGap,now-previousFrame);previousFrame=now;
            if(now<nextConnectionEvidence || transport==null)return;nextConnectionEvidence=now+1;
            ref var driver=ref transport.GetNetworkDriver();var created=driver.IsCreated;
            WriteJson(Path.Combine(output,"connection-evidence.json"),new ConnectionEvidence{seconds=now,listening=network.IsListening,connected=network.IsConnectedClient,
                driverCreated=created,driverBound=created && driver.Bound,driverListening=created && driver.Listening,receiveError=created?driver.ReceiveErrorCode:0,
                maxFrameGap=maxFrameGap,lastDataAge=now-lastTransportData,receivedDataEvents=receivedDataEvents,receivedDataBytes=receivedDataBytes,
                profiles=session?.ConnectedPlayers??Latest?.connected??Array.Empty<string>(),events=connectionTrace.ToArray()});
        }
        private static bool ValidSave(string payload)
        {
            try{var saved=JsonUtility.FromJson<SoloSnapshot>(payload);if(saved!=null && saved.schema>WorldLayout.Schema)throw new NotSupportedException("Newer schema.");SoloWorld.Validate(saved);return true;}
            catch(ArgumentException){return false;}catch(InvalidOperationException){return false;}
        }
        private void WriteJson<T>(string path,T value)
        {
            if(!DiagnosticFileWriter.TryWrite(path,JsonUtility.ToJson(value,true),Utf8))diagnosticWriteConflicts++;
        }
        private void WriteStatus(string status,string reason)
        {ConnectionStatus=status;if(output!=null)WriteJson(Path.Combine(output,"status.json"),new Status{role=config.role,runId=config.runId,instanceId=config.instanceId,build=Application.version,status=status,reason=reason,pid=System.Diagnostics.Process.GetCurrentProcess().Id,persistentServer=config.persistentServer});}
        private void Fail(Exception error)
        {failed=true;Debug.LogException(error);WriteStatus("failed",error.Message);Application.Quit(1);}
        private void Stop(){if(config.role=="server" && (positionDirty || maintenanceDirty))SaveAuthority();stopping=true;network.Shutdown();WriteStatus("stopped","");Application.Quit(0);}
        private void OnApplicationPause(bool paused)
        {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            SetFamilyForeground(!paused);
#endif
        }
        private void SetFamilyForeground(bool foreground)
        {
            applicationPaused=!foreground;
            if(pairing==null || config?.role!="client")return;
            if(!foreground)
            {
                BeginReconnect("foreground-required");
            }
            else if(retryPending)nextRetry=Time.realtimeSinceStartupAsDouble+.25;
        }
        public void VerifyFamilyForeground(bool foreground)
        {
            if(config==null || !config.verifyGarden)throw new InvalidOperationException("Verification config required.");
            SetFamilyForeground(foreground);
        }
        private void OnApplicationQuit(){if(!failed && config?.role=="server" && (positionDirty || maintenanceDirty))SaveAuthority();stopping=true;discovery?.Dispose();if(network!=null)network.Shutdown();authorityLock?.Dispose();}
    }
}
