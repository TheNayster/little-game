#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.IO;
using LittleWeeps.Adapters;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.NetworkProbe
{
    internal static class AndroidFamilyContext
    {
        internal static AndroidJavaObject Activity()
        {using var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer");return unity.GetStatic<AndroidJavaObject>("currentActivity");}
    }
    internal sealed class AndroidBonjour : IFamilyDiscovery
    {
        private AndroidJavaObject native;
        private readonly FamilyPairing paired;
        private readonly int protocol,content;
        public int Error {get;private set;}
        [Serializable] private sealed class Advertisement {public string family,authority,world;public int schema,protocol,content;}
        [Serializable] private sealed class Answer {public string address;public int port;public uint networkInterface;public Advertisement ad;}
        internal AndroidBonjour(FamilyPairing paired,int protocol,int content){this.paired=paired;this.protocol=protocol;this.content=content;}
        public void Browse()
        {
            if(native!=null)throw new InvalidOperationException("Discovery already active.");
            using var context=AndroidFamilyContext.Activity();
            native=new AndroidJavaObject("com.littleweeps.family.FamilyDiscovery",context,paired.familyId,paired.authorityId,paired.worldId,protocol,content);
            native.Call("start");
        }
        public void Tick(double clock){if(native!=null)Error=native.Call<int>("error");}
        public FamilyEndpoint Take()
        {
            if(native==null)return null;
            for(var i=0;i<16;i++)
            {
                var json=native.Call<string>("take");if(json==null)return null;if(json.Length>2048)continue;
                var answer=JsonUtility.FromJson<Answer>(json);var a=answer?.ad;
                if(a==null || answer.port<1024 || answer.port>65535 || !System.Net.IPAddress.TryParse(answer.address,out var ip) || ip.AddressFamily!=System.Net.Sockets.AddressFamily.InterNetwork)continue;
                var ad=new FamilyAdvertisement{family=a.family,authority=a.authority,world=a.world,schema=a.schema,protocol=a.protocol,content=a.content};
                if(ad.Matches(paired,protocol,content))return new FamilyEndpoint{address=answer.address,port=(ushort)answer.port,networkInterface=answer.networkInterface};
            }
            return null;
        }
        public void Dispose(){if(native==null)return;try{native.Call("close");}finally{native.Dispose();native=null;}}
    }
    internal static class AndroidEnrollment
    {
        internal static FamilyPairing Load(out string status)
        {
            status="unpaired";
            try
            {
                using var context=AndroidFamilyContext.Activity();
                using var native=new AndroidJavaObject("com.littleweeps.family.FamilyEnrollment",context);
                var json=native.Call<string>("load");status=native.Call<string>("status");
                if(json==null)return null;
                if(json.Length>32768)throw new InvalidDataException();
                var pair=JsonUtility.FromJson<FamilyPairing>(json);if(pair==null || pair.role!="client")throw new InvalidDataException();
                pair.Validate();return pair;
            }
            catch(Exception){status="enrollment-needs-parent";return null;}
        }
    }
}
#endif
