using System;
using System.Collections.Generic;

namespace LittleWeeps.Core
{
    // Legacy IDs are retained so existing Bluey/Bingo saves and selections
    // survive the roster expansion. Add only characters with prepared art.
    public static class PlayableCharacters
    {
        public sealed class Entry
        {
            public readonly string AvatarId, ArtId, Name;
            public readonly float Scale;
            public Entry(string avatarId, string artId, string name, float scale)
            { AvatarId=avatarId; ArtId=artId; Name=name; Scale=scale; }
        }
        public static readonly IReadOnlyList<Entry> All=Array.AsReadOnly(new[] {
            new Entry("blue-pup","bluey","Bluey",1f),
            new Entry("orange-pup","bingo","Bingo",.82f),
            new Entry("muffin","muffin","Muffin",.80f),
            new Entry("socks","socks","Socks",.66f),
            new Entry("bandit","bandit","Bandit",1.35f),
            new Entry("chilli","chilli","Chilli",1.25f),
            new Entry("chloe","chloe","Chloe",.98f),
            new Entry("coco","coco","Coco",.95f),
            new Entry("honey","honey","Honey",.95f),
            new Entry("indy","indy","Indy",.98f),
            new Entry("mackenzie","mackenzie","Mackenzie",1f),
            new Entry("rusty","rusty","Rusty",.98f),
            new Entry("jack","jack","Jack",.98f),
            new Entry("snickers","snickers","Snickers",1f),
            new Entry("winton","winton","Winton",.95f),
            new Entry("terrier-1","terrier-1","Terrier 1",.86f),
            new Entry("terrier-2","terrier-2","Terrier 2",.86f),
            new Entry("terrier-3","terrier-3","Terrier 3",.86f),
            new Entry("lucky","lucky","Lucky",1f),
            new Entry("chucky","chucky","Chucky",.80f),
            new Entry("judo","judo","Judo",1f),
            new Entry("pom-pom","pom-pom","Pom Pom",.58f),
            new Entry("lila","lila","Lila",.80f),
            new Entry("missy","missy","Missy",.75f),
            new Entry("buddy","buddy","Buddy",.72f),
            new Entry("bentley","bentley","Bentley",.72f),
            new Entry("juniper","juniper","Juniper",.72f),
            new Entry("winnie","winnie","Winnie",.95f),
            new Entry("jean-luc","jean-luc","Jean-Luc",.95f),
            new Entry("lulu","lulu","Lulu",.70f),
            new Entry("dusty","dusty","Dusty",.70f),
            new Entry("dougie","dougie","Dougie",.80f),
            new Entry("hercules","hercules","Hercules",1.15f),
            new Entry("pretzel","pretzel","Pretzel",.80f),
            new Entry("digger","digger","Digger",1.15f),
            new Entry("mia","mia","Mia",1.15f),
            new Entry("captain","captain","Captain",1.15f)
        });
        public static Entry Find(string avatarId)
        {
            foreach(var entry in All)if(entry.AvatarId==avatarId)return entry;
            return null;
        }
        public static bool Contains(string avatarId)=>Find(avatarId)!=null;
    }
}
