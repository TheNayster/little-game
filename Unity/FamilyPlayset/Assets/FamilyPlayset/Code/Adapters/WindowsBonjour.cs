using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using LittleWeeps.Core;

namespace LittleWeeps.Adapters
{
    // Uses the installed Bonjour service, not our own broadcast/multicast socket.
    // All calls, callbacks and disposal run on the Unity main thread. Poll is
    // nonblocking; DNSServiceProcessResult must never be called on an idle socket.
    public sealed class WindowsBonjour : IDisposable
    {
        public const string ServiceType="_lw-playset._udp";
        public sealed class Endpoint {public string address;public ushort port;public uint networkInterface;}
        private sealed class Operation {public IntPtr handle;public string key;public double deadline;}
        private readonly List<Operation> operations=new List<Operation>();
        private readonly HashSet<string> resolving=new HashSet<string>();
        private readonly Queue<Endpoint> endpoints=new Queue<Endpoint>();
        private readonly Dictionary<IntPtr,(ushort port,uint nic)> addresses=new Dictionary<IntPtr,(ushort,uint)>();
        private readonly FamilyPairing paired;
        private readonly int protocol,content;
        private readonly BrowseReply browseReply;
        private readonly ResolveReply resolveReply;
        private readonly AddressReply addressReply;
        private readonly RegisterReply registerReply;
        private double now;
        public int Error {get;private set;}
        public bool Registered {get;private set;}
        public int HandleCount=>operations.Count;
        public WindowsBonjour(FamilyPairing paired,int protocol,int content)
        {
            this.paired=paired;this.protocol=protocol;this.content=content;
            browseReply=OnBrowse;resolveReply=OnResolve;addressReply=OnAddress;registerReply=OnRegister;
        }
        public void Browse()
        {
            Check(DNSServiceBrowse(out var handle,0,0,ServiceType,"local.",browseReply,IntPtr.Zero));
            operations.Add(new Operation{handle=handle,deadline=double.PositiveInfinity});
        }
        public void Advertise(ushort port)
        {
            var txt=Txt("v=1","family="+paired.familyId,"authority="+paired.authorityId,"world="+paired.worldId,"protocol="+protocol,"content="+content);
            Check(DNSServiceRegister(out var handle,8,0,"LW-"+paired.authorityId,ServiceType,"local.",null,NetworkOrder(port),(ushort)txt.Length,txt,registerReply,IntPtr.Zero));
            operations.Add(new Operation{handle=handle,deadline=double.PositiveInfinity});
        }
        public Endpoint Take()=>endpoints.Count>0?endpoints.Dequeue():null;
        public void Tick(double clock)
        {
            now=clock;
            foreach(var op in operations.ToArray())
            {
                // A preceding browse removal can dispose another operation in
                // this snapshot. Never poll its now-invalid native handle.
                if(!operations.Contains(op))continue;
                if(now>op.deadline){Release(op);continue;}
                var poll=new Poll{fd=DNSServiceRefSockFD(op.handle),events=0x100}; // POLLRDNORM
                var result=WSAPoll(ref poll,1,0);
                if(result<0 || (poll.revents&0x7)!=0){Error=-1;Release(op);continue;}
                if(result>0 && (poll.revents&0x100)!=0)
                {var error=DNSServiceProcessResult(op.handle);if(error!=0){Error=error;Release(op);}}
            }
        }
        private void OnBrowse(IntPtr handle,uint flags,uint nic,int error,string name,string type,string domain,IntPtr context)
        {
            if(error!=0){Error=error;return;}
            // Do not resolve every app on a busy LAN; only the enrolled authority.
            if(name!="LW-"+paired.authorityId || type!=ServiceType+"." || domain!="local.")return;
            var key=nic+":"+name;
            if((flags&2)==0){foreach(var op in operations.Where(o=>o.key==key).ToArray())Release(op);endpoints.Clear();return;}
            if(operations.Count>=16 || !resolving.Add(key))return;
            error=DNSServiceResolve(out var resolved,0,nic,name,type,domain,resolveReply,IntPtr.Zero);
            if(error!=0){resolving.Remove(key);Error=error;return;}
            operations.Add(new Operation{handle=resolved,key=key,deadline=now+8});
        }
        private void OnResolve(IntPtr handle,uint flags,uint nic,int error,string fullName,string host,ushort port,ushort length,IntPtr txt,IntPtr context)
        {
            if(error!=0){Error=error;return;}
            if(length>1024 || string.IsNullOrEmpty(host) || host.Length>255)return;
            var values=ParseTxt(txt,length);
            if(values==null)return;
            values.TryGetValue("family",out var family);values.TryGetValue("authority",out var authority);values.TryGetValue("world",out var world);
            var ad=new FamilyAdvertisement{family=family,authority=authority,world=world,
                schema=Number(values,"v"),protocol=Number(values,"protocol"),content=Number(values,"content")};
            if(!ad.Matches(paired,protocol,content) || NetworkOrder(port)<1024 || operations.Count>=16)return;
            // This first PC qualification intentionally resolves IPv4 only. IPv6
            // and native mobile adapters remain explicit device gates.
            error=DNSServiceGetAddrInfo(out var address,0,nic,1,host,addressReply,IntPtr.Zero);
            if(error!=0){Error=error;return;}
            addresses[address]=(NetworkOrder(port),nic);
            operations.Add(new Operation{handle=address,deadline=now+8});
        }
        private void OnAddress(IntPtr handle,uint flags,uint nic,int error,string hostname,IntPtr socketAddress,uint ttl,IntPtr context)
        {
            if(error!=0){Error=error;return;}
            if((flags&2)==0 || ttl==0 || socketAddress==IntPtr.Zero || !addresses.TryGetValue(handle,out var destination) || endpoints.Count>=16)return;
            if(Marshal.ReadInt16(socketAddress)!=2)return;
            var bytes=new byte[4];Marshal.Copy(IntPtr.Add(socketAddress,4),bytes,0,4);
            if(bytes[0]==0 || bytes[0]>=224)return;
            endpoints.Enqueue(new Endpoint{address=new IPAddress(bytes).ToString(),port=destination.port,networkInterface=nic});
        }
        private void OnRegister(IntPtr handle,uint flags,int error,string name,string type,string domain,IntPtr context)
        {if(error!=0)Error=error;else Registered=true;}
        private static int Number(Dictionary<string,string> values,string key)=>values.TryGetValue(key,out var s) && int.TryParse(s,out var n)?n:-1;
        private static Dictionary<string,string> ParseTxt(IntPtr source,int size)
        {
            var data=new byte[size];Marshal.Copy(source,data,0,size);var result=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            for(var i=0;i<size;)
            {
                var count=data[i++];if(count==0)continue;if(i+count>size)return null;
                var text=Encoding.ASCII.GetString(data,i,count);i+=count;var split=text.IndexOf('=');
                if(split<1 || result.ContainsKey(text.Substring(0,split)))return null;
                result.Add(text.Substring(0,split),text.Substring(split+1));
            }
            return result;
        }
        private static byte[] Txt(params string[] entries)
        {var data=new List<byte>();foreach(var entry in entries){var bytes=Encoding.ASCII.GetBytes(entry);if(bytes.Length>255)throw new ArgumentException("TXT too large.");data.Add((byte)bytes.Length);data.AddRange(bytes);}return data.ToArray();}
        private static ushort NetworkOrder(ushort port)=>(ushort)((port>>8)|(port<<8));
        private static void Check(int error){if(error!=0)throw new InvalidOperationException("Bonjour unavailable ("+error+").");}
        private void Release(Operation op)
        {DNSServiceRefDeallocate(op.handle);operations.Remove(op);addresses.Remove(op.handle);if(op.key!=null)resolving.Remove(op.key);}
        public void Dispose(){foreach(var op in operations.ToArray())Release(op);endpoints.Clear();Registered=false;}
        [StructLayout(LayoutKind.Sequential)] private struct Poll {public UIntPtr fd;public short events,revents;}
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void BrowseReply(IntPtr h,uint f,uint i,int e,[MarshalAs(UnmanagedType.LPStr)]string n,[MarshalAs(UnmanagedType.LPStr)]string t,[MarshalAs(UnmanagedType.LPStr)]string d,IntPtr c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void ResolveReply(IntPtr h,uint f,uint i,int e,[MarshalAs(UnmanagedType.LPStr)]string n,[MarshalAs(UnmanagedType.LPStr)]string host,ushort port,ushort len,IntPtr txt,IntPtr c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void AddressReply(IntPtr h,uint f,uint i,int e,[MarshalAs(UnmanagedType.LPStr)]string host,IntPtr addr,uint ttl,IntPtr c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate void RegisterReply(IntPtr h,uint f,int e,[MarshalAs(UnmanagedType.LPStr)]string n,[MarshalAs(UnmanagedType.LPStr)]string t,[MarshalAs(UnmanagedType.LPStr)]string d,IntPtr c);
        [DllImport("dnssd.dll",CharSet=CharSet.Ansi)] private static extern int DNSServiceBrowse(out IntPtr h,uint flags,uint nic,string type,string domain,BrowseReply reply,IntPtr context);
        [DllImport("dnssd.dll",CharSet=CharSet.Ansi)] private static extern int DNSServiceResolve(out IntPtr h,uint flags,uint nic,string name,string type,string domain,ResolveReply reply,IntPtr context);
        [DllImport("dnssd.dll",CharSet=CharSet.Ansi)] private static extern int DNSServiceGetAddrInfo(out IntPtr h,uint flags,uint nic,uint protocol,string host,AddressReply reply,IntPtr context);
        [DllImport("dnssd.dll",CharSet=CharSet.Ansi)] private static extern int DNSServiceRegister(out IntPtr h,uint flags,uint nic,string name,string type,string domain,string host,ushort port,ushort len,byte[] txt,RegisterReply reply,IntPtr context);
        [DllImport("dnssd.dll")] private static extern UIntPtr DNSServiceRefSockFD(IntPtr h);
        [DllImport("dnssd.dll")] private static extern int DNSServiceProcessResult(IntPtr h);
        [DllImport("dnssd.dll")] private static extern void DNSServiceRefDeallocate(IntPtr h);
        [DllImport("ws2_32.dll")] private static extern int WSAPoll(ref Poll descriptors,uint count,int timeout);
    }
}
