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
            new Entry("chilli","chilli","Chilli",1.25f)
        });
        public static Entry Find(string avatarId)
        {
            foreach(var entry in All)if(entry.AvatarId==avatarId)return entry;
            return null;
        }
        public static bool Contains(string avatarId)=>Find(avatarId)!=null;
    }
}
