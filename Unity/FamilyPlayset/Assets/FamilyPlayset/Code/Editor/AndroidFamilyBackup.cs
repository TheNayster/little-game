#if UNITY_ANDROID
using System.IO;
using System.Xml;
using UnityEditor.Android;

namespace LittleWeeps.EditorTools
{
    // A development USB inbox briefly contains a player secret before Keystore
    // import. Exclude it from both legacy backup and Android 12+ device transfer.
    public sealed class AndroidFamilyBackup : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder=>100;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var resources=Path.Combine(path,"src/main/res/xml");Directory.CreateDirectory(resources);
            const string exclusions="<exclude domain=\"external\" path=\"FamilyLAN/enrollment.json\"/><exclude domain=\"external\" path=\"FamilyLAN/enrollment.pending\"/>";
            File.WriteAllText(Path.Combine(resources,"lw_family_backup.xml"),"<full-backup-content>"+exclusions+"</full-backup-content>");
            File.WriteAllText(Path.Combine(resources,"lw_family_transfer.xml"),"<data-extraction-rules><cloud-backup>"+exclusions+"</cloud-backup><device-transfer>"+exclusions+"</device-transfer></data-extraction-rules>");
            var file=Path.Combine(path,"src/main/AndroidManifest.xml");var xml=new XmlDocument();xml.Load(file);
            var app=(XmlElement)xml.SelectSingleNode("/manifest/application");
            app.SetAttribute("fullBackupContent","http://schemas.android.com/apk/res/android","@xml/lw_family_backup");
            app.SetAttribute("dataExtractionRules","http://schemas.android.com/apk/res/android","@xml/lw_family_transfer");
            xml.Save(file);
        }
    }
}
#endif
