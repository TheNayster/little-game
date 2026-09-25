using System;
using System.IO;
using System.Linq;
using LittleWeeps.Core;

namespace LittleWeeps.Adapters
{
    [Serializable] public sealed class ContinuationRecord
    {
        public int version=1;
        public string id, actor;
        public long createdUtcTicks;
        public RecoveryRecord basis;
        public SoloSnapshot snapshot;
    }
    [Serializable] public sealed class ContinuationSelection
    { public int version=1; public string branch=""; }

    // Each outage gets its own file. The immutable common base is retained for
    // later reconciliation; this store never writes back to the shared world.
    public sealed class ContinuationLibrary
    {
        private readonly string root, family, authority, world, actor;
        private readonly Func<string,ContinuationRecord> decode;
        private readonly Func<ContinuationRecord,string> encode;
        private readonly Func<string,ContinuationSelection> decodeSelection;
        private readonly Func<ContinuationSelection,string> encodeSelection;
        private readonly CheckpointStore selection;
        public string Selected { get; private set; }
        public ContinuationLibrary(string root,string family,string authority,string world,string actor,
            Func<string,ContinuationRecord> decode,Func<ContinuationRecord,string> encode,
            Func<string,ContinuationSelection> decodeSelection,Func<ContinuationSelection,string> encodeSelection)
        {
            this.root=root;this.family=family;this.authority=authority;this.world=world;this.actor=actor;
            this.decode=decode;this.encode=encode;this.decodeSelection=decodeSelection;this.encodeSelection=encodeSelection;
            selection=new CheckpointStore(Path.Combine(root,"selection.save"),ValidSelection);
            var saved=selection.Load();RequireReadable(saved.Status);
            Selected=saved.Payload==null?"":decodeSelection(saved.Payload).branch;
            if(Selected!="")Load(Selected); // Refuse a broken pointer, never open a blank replacement.
        }
        private static void RequireReadable(CheckpointStatus status)
        {if(status==CheckpointStatus.Corrupt || status==CheckpointStatus.Unsupported)throw new InvalidDataException("Saved adventure needs recovery.");}
        private bool ValidSelection(string text)
        {
            try
            {
                var value=decodeSelection(text);
                if(value?.version>1)throw new NotSupportedException("Newer adventure selection.");
                return value!=null && value.version==1 && (value.branch=="" || FamilyPairing.Id(value.branch));
            }
            catch(ArgumentException){return false;}
            catch(InvalidDataException){return false;}
        }
        private void Validate(ContinuationRecord record)
        {
            if(record?.version>1 || record?.snapshot?.schema>2)throw new NotSupportedException("Newer adventure format.");
            if(record==null || record.version!=1 || !FamilyPairing.Id(record.id) || record.actor!=actor || record.createdUtcTicks<=0 || record.basis==null)
                throw new InvalidDataException("Invalid adventure identity.");
            record.basis.Validate(family,authority,world);SoloWorld.Validate(record.snapshot);
            if(record.snapshot.schema!=2 || record.snapshot.worldId!=record.id || record.snapshot.revision<record.basis.snapshot.revision ||
                !record.snapshot.players.Any(p=>p.id==actor) ||
                !record.snapshot.players.Select(p=>p.id).SequenceEqual(record.basis.snapshot.players.Select(p=>p.id)) ||
                !record.snapshot.toys.Select(t=>t.id).SequenceEqual(record.basis.snapshot.toys.Select(t=>t.id)))
                throw new InvalidDataException("Adventure lineage mismatch.");
        }
        private bool Valid(string text)
        {
            try{Validate(decode(text));return true;}
            catch(ArgumentException){return false;}
            catch(InvalidOperationException){return false;}
            catch(InvalidDataException){return false;}
        }
        public string PathFor(string id)
        {if(!FamilyPairing.Id(id))throw new ArgumentException("Invalid adventure ID.");return Path.Combine(root,id,"world.save");}
        private CheckpointStore Store(string id)=>new CheckpointStore(PathFor(id),Valid);
        public ContinuationRecord Load(string id)
        {
            var saved=Store(id).Load();RequireReadable(saved.Status);
            if(saved.Payload==null)throw new InvalidDataException("Missing saved adventure.");
            var record=decode(saved.Payload);Validate(record);
            if(record.id!=id)throw new InvalidDataException("Adventure path mismatch.");return record;
        }
        public string[] Branches()=>!Directory.Exists(root)?Array.Empty<string>():Directory.GetDirectories(root)
            .Select(Path.GetFileName).Where(FamilyPairing.Id).OrderByDescending(id=>Directory.GetCreationTimeUtc(Path.Combine(root,id))).ThenBy(id=>id).ToArray();
        public void Select(string id)
        {
            if(id!="")Load(id);
            var payload=encodeSelection(new ContinuationSelection{branch=id});selection.Save(payload);
            if(selection.Load().Payload!=payload)throw new IOException("Adventure selection not verified.");Selected=id;
        }
        public ContinuationRecord Create(RecoveryRecord basis)
        {
            basis.Validate(family,authority,world);
            // Clone through the serializer so subsequent replica commits and
            // local play cannot mutate the preserved common base.
            var record=decode(encode(new ContinuationRecord{id=Guid.NewGuid().ToString("N"),actor=actor,
                createdUtcTicks=DateTime.UtcNow.Ticks,basis=basis,snapshot=basis.snapshot}));
            var state=SoloWorld.Restore(record.snapshot).Snapshot();state.worldId=record.id;
            foreach(var player in state.players.Where(p=>p.id!=actor))player.activity="";
            record.snapshot=state;Validate(record);Save(record,state);return record;
        }
        public void Save(ContinuationRecord record,SoloSnapshot snapshot)
        {
            // No serialization round-trip of a discarded previous snapshot.
            // Callers own the detached snapshot; the immutable basis is shared.
            var next=new ContinuationRecord{version=record.version,id=record.id,actor=record.actor,
                createdUtcTicks=record.createdUtcTicks,basis=record.basis,snapshot=snapshot};Validate(next);
            var store=Store(record.id);var old=store.Load();RequireReadable(old.Status);
            if(old.Payload!=null)
            {
                var previous=decode(old.Payload);
                if(previous.id!=next.id || previous.createdUtcTicks!=next.createdUtcTicks || next.snapshot.revision<previous.snapshot.revision ||
                    encode(new ContinuationRecord{basis=previous.basis})!=encode(new ContinuationRecord{basis=next.basis}))
                    throw new InvalidDataException("Cannot replace an adventure base or roll it back.");
            }
            var payload=encode(next);store.Save(payload);
            if(store.Load().Payload!=payload)throw new IOException("Adventure write not verified.");record.snapshot=snapshot;
        }
    }
}
