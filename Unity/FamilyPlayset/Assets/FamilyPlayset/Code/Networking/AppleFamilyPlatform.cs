#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.IO;
using System.Runtime.InteropServices;
using LittleWeeps.Adapters;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.NetworkProbe
{
    internal static class AppleFamilyNative
    {
        [DllImport("__Internal")] internal static extern IntPtr LWFamilyBrowse(string authority);
        [DllImport("__Internal")] internal static extern int LWFamilyPump(IntPtr handle);
        [DllImport("__Internal")] internal static extern IntPtr LWFamilyTake(IntPtr handle);
        [DllImport("__Internal")] internal static extern void LWFamilyStop(IntPtr handle);
        [DllImport("__Internal")] internal static extern void LWFamilyFree(IntPtr text);
        [DllImport("__Internal")] internal static extern IntPtr LWPairingRead(out int status);
        [DllImport("__Internal")] internal static extern int LWPairingAdd(string json);
        [DllImport("__Internal")] internal static extern void LWExcludeEnrollmentBackup(string path);
        internal static string TakeString(IntPtr value)
        {if(value==IntPtr.Zero)return null;try{return Marshal.PtrToStringUTF8(value);}finally{LWFamilyFree(value);}}
    }
    internal sealed class AppleBonjour : IFamilyDiscovery
    {
        private IntPtr handle;
        private readonly FamilyPairing paired;
        private readonly int protocol,content;
        public int Error {get;private set;}
        [Serializable] private sealed class Advertisement {public string family,authority,world;public int schema,protocol,content;}
        [Serializable] private sealed class Answer {public string address;public int port;public uint networkInterface;public Advertisement ad;}
        internal AppleBonjour(FamilyPairing paired,int protocol,int content){this.paired=paired;this.protocol=protocol;this.content=content;}
        public void Browse(){if(handle!=IntPtr.Zero)throw new InvalidOperationException("Discovery already active.");handle=AppleFamilyNative.LWFamilyBrowse(paired.authorityId);if(handle==IntPtr.Zero)Error=-1;}
        public void Tick(double clock){if(handle!=IntPtr.Zero)Error=AppleFamilyNative.LWFamilyPump(handle);}
        public FamilyEndpoint Take()
        {
            if(handle==IntPtr.Zero)return null;
            for(var i=0;i<16;i++)
            {
                var json=AppleFamilyNative.TakeString(AppleFamilyNative.LWFamilyTake(handle));if(json==null)return null;
                if(json.Length>2048)continue;
                var answer=JsonUtility.FromJson<Answer>(json);var a=answer?.ad;
                if(a==null || answer.port<1024 || answer.port>65535 || !System.Net.IPAddress.TryParse(answer.address,out var ip) || ip.AddressFamily!=System.Net.Sockets.AddressFamily.InterNetwork)continue;
                var ad=new FamilyAdvertisement{family=a.family,authority=a.authority,world=a.world,schema=a.schema,protocol=a.protocol,content=a.content};
                if(ad.Matches(paired,protocol,content))return new FamilyEndpoint{address=answer.address,port=(ushort)answer.port,networkInterface=answer.networkInterface};
            }
            return null;
        }
        public void Dispose(){if(handle!=IntPtr.Zero){AppleFamilyNative.LWFamilyStop(handle);handle=IntPtr.Zero;}}
    }
    internal static class AppleEnrollment
    {
        // Development parent provisioning uses the paired Mac's USB app-data
        // channel. No credential is built into the IPA, advertised or logged.
        // The inbox is consumed only after validation and a durable Keychain add.
        internal static FamilyPairing Load(out string status)
        {
            status="unpaired";
            try
            {
                var inbox=Path.Combine(Application.persistentDataPath,"FamilyLAN","enrollment.json");
                if(File.Exists(inbox))
                {
                    AppleFamilyNative.LWExcludeEnrollmentBackup(inbox);
                    if(new FileInfo(inbox).Length>32768)throw new InvalidDataException();
                    var incoming=File.ReadAllText(inbox);Validate(incoming);
                    var added=AppleFamilyNative.LWPairingAdd(incoming);
                    if(added!=0){status="enrollment-storage-"+added;return null;}
                    File.Delete(inbox);
                }
                var json=AppleFamilyNative.TakeString(AppleFamilyNative.LWPairingRead(out var code));
                if(code!=0){status=code==-25300?"unpaired":"enrollment-storage-"+code;return null;}
                var pairing=Validate(json);status="paired";return pairing;
            }
            catch(Exception){status="enrollment-needs-parent";return null;}
        }
        private static FamilyPairing Validate(string json)
        {
            if(string.IsNullOrEmpty(json) || json.Length>32768)throw new InvalidDataException();
            var pair=JsonUtility.FromJson<FamilyPairing>(json);if(pair==null || pair.role!="client")throw new InvalidDataException();
            pair.Validate();return pair;
        }
    }
}
#endif
