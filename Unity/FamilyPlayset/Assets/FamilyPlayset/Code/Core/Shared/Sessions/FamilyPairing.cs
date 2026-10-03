using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LittleWeeps.Core
{
    // Discovery is only a hint. Trust comes from the enrolled CA and a device's
    // independent credential sent inside the certificate-validated DTLS channel.
    [Serializable] public sealed class FamilyPairing
    {
        public int schema=1;
        public string familyId,authorityId,worldId,role,profile,credential;
        public string serverName,caCertificate,certificate,privateKey;
        public FamilyMember[] members;
        public void Validate()
        {
            if(schema!=1 || !Id(familyId) || !Id(authorityId) || !Id(worldId) ||
                serverName!="lw-"+authorityId+".local" || string.IsNullOrEmpty(caCertificate) || caCertificate.Length>8192)
                throw new ArgumentException("Invalid family enrollment.");
            if(role=="server")
            {
                if(string.IsNullOrEmpty(certificate) || certificate.Length>8192 || string.IsNullOrEmpty(privateKey) || privateKey.Length>8192 ||
                    members==null || members.Length!=4 || members.Any(m=>m==null || !Id(m.profile) || !Hex(m.credentialHash)) ||
                    members.Select(m=>m.profile).Distinct().Count()!=4)throw new ArgumentException("Invalid authority enrollment.");
            }
            else if(role!="client" || !Id(profile) || !Hex(credential) || !string.IsNullOrEmpty(privateKey) || members?.Length>0)
                throw new ArgumentException("Invalid player enrollment.");
        }
        public bool Admits(string family,string authority,string world,string player,string secret)
        {
            if(role!="server" || family!=familyId || authority!=authorityId || world!=worldId || !Hex(secret))return false;
            var member=members.FirstOrDefault(m=>m.profile==player);
            if(member==null)return false;
            var hash=Hash(secret);var difference=0;
            for(var i=0;i<hash.Length;i++)difference|=hash[i]^member.credentialHash[i];
            return difference==0;
        }
        public static string Hash(string secret)
        {using var sha=SHA256.Create();return BitConverter.ToString(sha.ComputeHash(Encoding.ASCII.GetBytes(secret))).Replace("-","").ToLowerInvariant();}
        public static bool Id(string value)=>value!=null && value.Length==32 && value.All(c=>c>='0' && c<='9' || c>='a' && c<='f');
        public static bool Hex(string value)=>value!=null && value.Length==64 && value.All(c=>c>='0' && c<='9' || c>='a' && c<='f');
    }
    [Serializable] public sealed class FamilyMember {public string profile,credentialHash;}
    public sealed class FamilyAdvertisement
    {
        public string family,authority,world;
        public int schema,protocol,content;
        public bool Matches(FamilyPairing paired,int wire,int assets)=>paired!=null && schema==1 &&
            family==paired.familyId && authority==paired.authorityId && world==paired.worldId && protocol==wire && content==assets;
    }
}
