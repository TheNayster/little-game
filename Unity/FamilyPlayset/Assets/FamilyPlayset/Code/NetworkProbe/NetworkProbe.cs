using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LittleWeeps.Core;
using LittleWeeps.Adapters;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace LittleWeeps.NetworkProbe
{
    // Isolated Windows loopback qualification. No LAN discovery, family pairing,
    // mobile authority selection or normal-game save access is implemented here.
    public sealed class NetworkProbe : MonoBehaviour
    {
        private const int Protocol=1, Content=1, MaxWireBytes=16384;
        private const string CommandMessage="littleweeps.probe.command.v1", StateMessage="littleweeps.probe.state.v1", PoseMessage="littleweeps.probe.pose.v1";
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false,true);
        private Config config;
        private string root,output,epoch;
        private NetworkManager network;
        private FamilySession session;
        private CheckpointStore store;
        private FileStream authorityLock;
        private long sequence,seenSequence;
        private int controlSerial;
        private float nextControl,started;
        private bool failed,stopping;
        private readonly Dictionary<string,DragPose> poses=new Dictionary<string,DragPose>();
        public State Latest {get;private set;}
        public Config Settings=>config;
        public string Output=>output;
        public bool ConnectedToServer=>network!=null && network.IsConnectedClient && !failed && !stopping;
        public string ConnectionStatus {get;private set;}="Connecting…";
        public event Action<State> Received;
        public event Action LostConnection;
        [Serializable] public sealed class DragPose {public string actor,item,lease;public float x,y;public long tick;}
        [Serializable] public sealed class Slot {public string profile,token;}
        [Serializable] public sealed class Config
        {
            public string runId,instanceId,role,profile,token;
            public int port,protocol=Protocol,content=Content;
            public Slot[] slots;
            public bool presentation,verifyGarden,interactive;
        }
        [Serializable] private sealed class Hello {public string runId,profile,token;public int protocol,content;}
        [Serializable] public sealed class Control {public int serial;public string kind;public Request request;}
        [Serializable] public sealed class Request {public string requestId;public int protocol=Protocol;public SoloCommand command;}
        [Serializable] public sealed class State
        {
            public int protocol=Protocol,content=Content;
            public string runId,epoch,requestId,outcome;
            public long sequence;
            public bool accepted,duplicate,durable;
            public SoloSnapshot view;
            public string[] connected;
            public DragPose[] poses;
        }
        [Serializable] private sealed class Status {public string role,runId,instanceId,build,status,reason;public int pid;}

        private void Start()
        {
            Application.runInBackground=true;Application.targetFrameRate=60;started=Time.realtimeSinceStartup;
            try
            {
                var args=Environment.GetCommandLineArgs();var index=Array.IndexOf(args,"-familyNetworkConfig");
                if(index<0 || index+1>=args.Length)throw new ArgumentException("Explicit isolated network config required.");
                var path=Path.GetFullPath(args[index+1]);root=Path.GetDirectoryName(path);
                if(new FileInfo(path).Length>8192)throw new InvalidDataException("Configuration too large.");
                config=JsonUtility.FromJson<Config>(File.ReadAllText(path,Utf8));
                if(config==null || !Guid.TryParseExact(config.runId,"N",out _) || !Guid.TryParseExact(config.instanceId,"N",out _) ||
                    new DirectoryInfo(root).Name!=config.runId || config.port<1024 || config.port>65535 ||
                    (config.role!="server" && config.role!="client"))throw new InvalidDataException("Invalid isolated configuration.");
                output=Path.Combine(root,config.instanceId);Directory.CreateDirectory(output);
                var go=new GameObject("Loopback Network",typeof(NetworkManager),typeof(UnityTransport));
                network=go.GetComponent<NetworkManager>();var transport=go.GetComponent<UnityTransport>();
                // Explicit loopback bind prevents this development protocol from
                // becoming a LAN service. Tokens below are test credentials only.
                transport.SetConnectionData(true,"127.0.0.1",(ushort)config.port,"127.0.0.1");
                transport.MaxPayloadSize=MaxWireBytes;transport.DisconnectTimeoutMS=2500;transport.HeartbeatTimeoutMS=400;
                // Runtime-created managers have no inspector-serialized config.
                network.NetworkConfig=new NetworkConfig{NetworkTransport=transport};
                network.NetworkConfig.EnableSceneManagement=false;network.NetworkConfig.ConnectionApproval=true;
                network.NetworkConfig.ForceSamePrefabs=false;network.NetworkConfig.TickRate=30;
                network.OnClientConnectedCallback+=Connected;network.OnClientDisconnectCallback+=Disconnected;
                if(config.role=="server")StartAuthority();else StartGuest();
                network.CustomMessagingManager.RegisterNamedMessageHandler(CommandMessage,ReceiveCommand);
                network.CustomMessagingManager.RegisterNamedMessageHandler(StateMessage,ReceiveState);
                network.CustomMessagingManager.RegisterNamedMessageHandler(PoseMessage,ReceivePose);
                if(config.role=="client" && config.presentation)
                {
                    var garden=gameObject.AddComponent<NetworkGardenSession>();garden.Initialize(this);
                    var screen=gameObject.AddComponent<LittleWeeps.Client.SoloScreen>();screen.Configure(garden);
                    if(config.verifyGarden)gameObject.AddComponent<NetworkGardenVerification>();
                }
            }
            catch(Exception e){Fail(e);}
        }
        private void StartAuthority()
        {
            if(config.slots==null || config.slots.Length!=4 || config.slots.Any(s=>s==null || string.IsNullOrWhiteSpace(s.profile) || s.profile.Length>64 || s.token==null || s.token.Length!=64) || config.slots.Select(s=>s.profile).Distinct().Count()!=4)
                throw new InvalidDataException("Exactly four distinct test profiles required.");
            var folder=Path.Combine(root,"server-world");Directory.CreateDirectory(folder);
            authorityLock=File.Open(Path.Combine(folder,"authority.lock"),FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            store=new CheckpointStore(Path.Combine(folder,"world.save"),ValidSave);
            var saved=store.Load();
            if(saved.Status==CheckpointStatus.Corrupt || saved.Status==CheckpointStatus.Unsupported)throw new InvalidDataException("Server checkpoint is blocked.");
            var world=saved.Status==CheckpointStatus.Missing?SoloWorld.Create(config.slots.Select(s=>s.profile).ToArray()):SoloWorld.Restore(JsonUtility.FromJson<SoloSnapshot>(saved.Payload));
            if(saved.Status==CheckpointStatus.Missing && config.presentation)
                for(var i=0;i<config.slots.Length;i++)world.Apply(new SoloCommand{requestId=Guid.NewGuid().ToString("N"),actor=config.slots[i].profile,expectedRevision=world.Revision,action=SoloAction.Move,x=280+i*180,y=100});
            if(!world.Snapshot().players.Select(p=>p.id).OrderBy(s=>s).SequenceEqual(config.slots.Select(s=>s.profile).OrderBy(s=>s)))throw new InvalidDataException("Roster does not match checkpoint.");
            session=new FamilySession(world);epoch=Guid.NewGuid().ToString("N");SaveAuthority();
            network.ConnectionApprovalCallback=Approve;
            if(!network.StartServer())throw new InvalidOperationException("Loopback server did not start.");
            WriteStatus("listening","");Publish();
        }
        private void StartGuest()
        {
            var hello=new Hello{runId=config.runId,profile=config.profile,token=config.token,protocol=config.protocol,content=config.content};
            network.NetworkConfig.ConnectionData=Utf8.GetBytes(JsonUtility.ToJson(hello));
            if(!network.StartClient())throw new InvalidOperationException("Loopback client did not start.");
            WriteStatus("connecting","");
        }
        private void Approve(NetworkManager.ConnectionApprovalRequest request,NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved=false;response.CreatePlayerObject=false;response.Pending=false;response.Reason="invalid-admission";
            try
            {
                if(request.Payload==null || request.Payload.Length>1024)return;
                var hello=JsonUtility.FromJson<Hello>(Utf8.GetString(request.Payload));
                if(hello==null || hello.runId!=config.runId)return;
                if(hello.protocol!=Protocol || hello.content!=Content){response.Reason="incompatible-version";return;}
                var slot=config.slots.FirstOrDefault(s=>s.profile==hello.profile);
                if(slot==null || slot.token!=hello.token){response.Reason="unpaired-profile";return;}
                response.Approved=session.Attach(request.ClientNetworkId,hello.profile,out var reason);response.Reason=reason;
            }
            catch(ArgumentException){response.Reason="invalid-admission";}
        }
        private void Connected(ulong client)
        {
            if(config.role=="server")Publish();
            else if(client==network.LocalClientId)WriteStatus("connected","");
        }
        private void Disconnected(ulong client)
        {
            if(stopping || failed)return;
            try
            {
                if(config.role=="server")
                {if(session.Detach(client)){SaveAuthority();Publish();}}
                else if(client==network.LocalClientId){WriteStatus("disconnected",network.DisconnectReason??"");LostConnection?.Invoke();}
            }
            catch(Exception e){Fail(e);}
        }
        private State Current(string requestId="",SoloResult? result=null)
        {
            return new State{runId=config.runId,epoch=epoch,sequence=++sequence,requestId=requestId,
                accepted=result?.Accepted??false,duplicate=result?.Duplicate??false,outcome=result?.Outcome??"snapshot",
                durable=true,view=session.View(),connected=session.ConnectedPlayers,poses=poses.Values.ToArray()};
        }
        private void Publish()
        {
            foreach(var item in poses.Keys.ToArray())if(!session.Checkpoint().toys.Any(t=>t.id==item && t.holder==poses[item].actor))poses.Remove(item);
            var state=Current();WriteJson(Path.Combine(output,"view.json"),state);
            foreach(var peer in network.ConnectedClientsIds)Send(StateMessage,peer,state);
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
                    SaveAuthority();
                    if(request.command.action==SoloAction.Grab)
                    {
                        var toy=session.Checkpoint().toys.First(t=>t.id==request.command.item);
                        poses[toy.id]=new DragPose{actor=toy.holder,item=toy.id,lease=request.requestId,x=toy.x,y=toy.y};
                    }
                    else if(request.command.action==SoloAction.Drop || request.command.action==SoloAction.CancelGrab)poses.Remove(request.command.item);
                }
                Send(StateMessage,sender,Current(request.requestId,result));
                if(result.Accepted && !result.Duplicate)Publish();
            }
            catch(Exception e) when(e is ArgumentException || e is InvalidDataException || e is OverflowException)
            {network.DisconnectClient(sender,"invalid-message");}
            catch(Exception e){Fail(e);}
        }
        private void ReceiveState(ulong sender,FastBufferReader reader)
        {
            if(config.role!="client" || sender!=NetworkManager.ServerClientId || failed)return;
            try
            {
                var state=JsonUtility.FromJson<State>(Read(reader));
                if(state==null || state.protocol!=Protocol || state.content!=Content || state.runId!=config.runId || !Guid.TryParseExact(state.epoch,"N",out _))throw new InvalidDataException("Wrong server state.");
                SoloWorld.Validate(state.view);
                if(!string.IsNullOrEmpty(epoch) && state.epoch!=epoch)throw new InvalidDataException("Unexpected authority epoch.");
                epoch=state.epoch;
                if(!string.IsNullOrEmpty(state.requestId))
                {if(!Guid.TryParseExact(state.requestId,"N",out _))throw new InvalidDataException("Bad response identity.");WriteJson(Path.Combine(output,"reply-"+state.requestId+".json"),state);}
                if(state.sequence>seenSequence){seenSequence=state.sequence;Latest=state;WriteJson(Path.Combine(output,"view.json"),state);}
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
        {if(config.role!="client")return;stopping=true;network.Shutdown();WriteStatus("disconnected","response-timeout");LostConnection?.Invoke();}
        public void Preview(DragPose pose)
        {if(ConnectedToServer)Send(PoseMessage,NetworkManager.ServerClientId,pose);}
        private void ReceivePose(ulong sender,FastBufferReader reader)
        {
            if(config.role!="server" || stopping || failed)return;
            try
            {
                var pose=JsonUtility.FromJson<DragPose>(Read(reader));
                if(pose==null || pose.item==null || !SoloWorld.Position(pose.x,pose.y) || !session.TryPlayer(sender,out var actor) ||
                    actor!=pose.actor || !poses.TryGetValue(pose.item,out var current) || current.actor!=actor ||
                    current.lease!=pose.lease || pose.tick<=current.tick)return;
                current.x=pose.x;current.y=pose.y;current.tick=pose.tick;Publish();
            }
            catch(ArgumentException){network.DisconnectClient(sender,"invalid-preview");}
            catch(InvalidDataException){network.DisconnectClient(sender,"invalid-preview");}
        }
        private void Send<T>(string name,ulong peer,T message)
        {
            var bytes=Utf8.GetBytes(JsonUtility.ToJson(message));
            if(bytes.Length>MaxWireBytes-4)throw new InvalidDataException("Network message exceeds probe limit.");
            using var writer=new FastBufferWriter(bytes.Length+4,Allocator.Temp);
            writer.WriteValueSafe(bytes.Length);writer.WriteBytesSafe(bytes);
            network.CustomMessagingManager.SendNamedMessage(name,peer,writer,NetworkDelivery.ReliableFragmentedSequenced);
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
            if(config==null || output==null || failed || stopping)return;
            if(Time.realtimeSinceStartup-started>(config.interactive?7200:240)){Fail(new TimeoutException("Isolated probe lifetime exceeded."));return;}
            if(Time.realtimeSinceStartup<nextControl)return;nextControl=Time.realtimeSinceStartup+.04f;
            try
            {
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
            }
            catch(Exception e){Fail(e);}
        }
        private void SaveAuthority()=>store.Save(JsonUtility.ToJson(session.Checkpoint()));
        private static bool ValidSave(string payload)
        {
            try{var saved=JsonUtility.FromJson<SoloSnapshot>(payload);if(saved!=null && saved.schema>1)throw new NotSupportedException("Newer schema.");SoloWorld.Validate(saved);return true;}
            catch(ArgumentException){return false;}catch(InvalidOperationException){return false;}
        }
        private static void WriteJson<T>(string path,T value)
        {
            var temp=path+".pending";File.WriteAllText(temp,JsonUtility.ToJson(value,true),Utf8);
            if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
        }
        private void WriteStatus(string status,string reason)
        {ConnectionStatus=status;if(output!=null)WriteJson(Path.Combine(output,"status.json"),new Status{role=config.role,runId=config.runId,instanceId=config.instanceId,build=Application.version,status=status,reason=reason,pid=System.Diagnostics.Process.GetCurrentProcess().Id});}
        private void Fail(Exception error)
        {failed=true;Debug.LogException(error);WriteStatus("failed",error.Message);Application.Quit(1);}
        private void Stop(){stopping=true;network.Shutdown();WriteStatus("stopped","");Application.Quit(0);}
        private void OnApplicationQuit(){stopping=true;if(network!=null)network.Shutdown();authorityLock?.Dispose();}
    }
}
