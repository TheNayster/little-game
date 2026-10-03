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
        public LocalViewOrigin origin;
        public SoloSnapshot snapshot;
    }
    // A private local fork is not a complete server recovery checkpoint. It may
    // contain visible anticipation, but can never be used to elect a shared host.
    [Serializable] public sealed class LocalViewOrigin
    {
        public string family,authority,world,epoch;
        public SoloSnapshot snapshot;
    }
    [Serializable] public sealed class ContinuationSelection
    { public int version=1; public string branch=""; }

    // Each outage gets its own file. Retain its immutable origin for provenance;
    // this store never writes back to the shared world or the recovery replica.
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
            if(record?.version>2 || record?.snapshot?.schema>WorldLayout.Schema)throw new NotSupportedException("Newer adventure format.");
            if(record==null || (record.version!=1 && record.version!=2) || !FamilyPairing.Id(record.id) || record.actor!=actor || record.createdUtcTicks<=0)
                throw new InvalidDataException("Invalid adventure identity.");
            SoloSnapshot source;
            if(record.version==1)
            {
                // Unity inline class serialization expands null fields to empty
                // objects. Version, not reference-nullness, selects the format.
                if(record.basis==null || record.origin!=null &&
                    (!string.IsNullOrEmpty(record.origin.family) || !string.IsNullOrEmpty(record.origin.authority) ||
                     !string.IsNullOrEmpty(record.origin.world) || !string.IsNullOrEmpty(record.origin.epoch)))
                    throw new InvalidDataException("Invalid checkpoint adventure.");
                record.basis.Validate(family,authority,world);source=record.basis.snapshot;
            }
            else
            {
                if(record.basis!=null && (record.basis.version!=0 || record.basis.protocol!=0 || record.basis.content!=0 || record.basis.checkpoint!=0 ||
                    !string.IsNullOrEmpty(record.basis.family) || !string.IsNullOrEmpty(record.basis.authority) ||
                    !string.IsNullOrEmpty(record.basis.world) || !string.IsNullOrEmpty(record.basis.epoch)))
                    throw new InvalidDataException("Local view cannot claim a recovery checkpoint.");
                ValidateOrigin(record.origin);source=record.origin.snapshot;
            }
            GameWorld.Validate(record.snapshot);
            if(record.snapshot.schema<2 || record.snapshot.worldId!=record.id || record.snapshot.revision<source.revision ||
                !record.snapshot.players.Any(p=>p.id==actor) ||
                !record.snapshot.players.Select(p=>p.id).SequenceEqual(source.players.Select(p=>p.id)) ||
                !record.snapshot.toys.Select(t=>t.id).SequenceEqual(source.toys.Select(t=>t.id)))
                throw new InvalidDataException("Adventure lineage mismatch.");
        }
        private void ValidateOrigin(LocalViewOrigin origin)
        {
            if(origin==null || origin.family!=family || origin.authority!=authority || origin.world!=world || !FamilyPairing.Id(origin.epoch))
                throw new InvalidDataException("Local view identity mismatch.");
            GameWorld.Validate(origin.snapshot);
            if(origin.snapshot.schema<2 || !origin.snapshot.players.Any(p=>p.id==actor))throw new InvalidDataException("Incomplete local view.");
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
            var state=GameWorld.Restore(record.snapshot).Snapshot();state.worldId=record.id;
            foreach(var player in state.players.Where(p=>p.id!=actor))player.activity="";
            record.snapshot=state;Validate(record);Save(record,state);return record;
        }
        public ContinuationRecord CreateVisible(LocalViewOrigin origin)
        {
            ValidateOrigin(origin);
            var record=decode(encode(new ContinuationRecord{version=2,id=Guid.NewGuid().ToString("N"),actor=actor,
                createdUtcTicks=DateTime.UtcNow.Ticks,origin=origin,snapshot=origin.snapshot}));
            var state=GameWorld.Restore(record.snapshot).Snapshot();state.worldId=record.id;
            foreach(var player in state.players.Where(p=>p.id!=actor))player.activity="";
            record.snapshot=state;Validate(record);Save(record,state);return record;
        }
        public void Save(ContinuationRecord record,SoloSnapshot snapshot)
        {
            // No serialization round-trip of a discarded previous snapshot.
            // Callers own the detached snapshot; the immutable basis is shared.
            var next=new ContinuationRecord{version=record.version,id=record.id,actor=record.actor,
                createdUtcTicks=record.createdUtcTicks,basis=record.basis,origin=record.origin,snapshot=snapshot};Validate(next);
            var store=Store(record.id);var old=store.Load();RequireReadable(old.Status);
            if(old.Payload!=null)
            {
                var previous=decode(old.Payload);
                if(previous.version!=next.version || previous.id!=next.id || previous.createdUtcTicks!=next.createdUtcTicks || next.snapshot.revision<previous.snapshot.revision ||
                    OriginIdentity(previous)!=OriginIdentity(next))
                    throw new InvalidDataException("Cannot replace an adventure base or roll it back.");
            }
            var payload=encode(next);store.Save(payload);
            if(store.Load().Payload!=payload)throw new IOException("Adventure write not verified.");record.snapshot=snapshot;
        }
        private string OriginIdentity(ContinuationRecord record)=>record.version==1?
            encode(new ContinuationRecord{basis=record.basis}):encode(new ContinuationRecord{version=2,origin=record.origin});
    }
}
