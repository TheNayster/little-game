# /// script
# dependencies = ["dnfile>=0.18,<0.19", "capstone==5.0.6"]
# ///
"""Map Cpp2IL address attributes independently through ELF64 PT_LOAD segments.

Reads metadata-only dummy assemblies without loading them into a CLR. The
publisher binary is never executed. Native ranges are metadata-derived bounds,
not recovered original C# or proof of exhaustive control flow.
"""
import argparse,hashlib,json,struct
from pathlib import Path
import dnfile
from capstone import Cs,CS_ARCH_ARM64,CS_MODE_LITTLE_ENDIAN

def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def strings(blob):
    assert blob[:2]==b'\x01\x00';count=struct.unpack_from('<H',blob,2)[0];at=4;result={}
    def read():
        nonlocal at
        length=blob[at];at+=1
        if length&0xc0==0x80:length=((length&0x3f)<<8)|blob[at];at+=1
        elif length&0xe0==0xc0:length=((length&0x1f)<<24)|int.from_bytes(blob[at:at+3],'big');at+=3
        value=blob[at:at+length].decode();at+=length;return value
    for _ in range(count):
        assert blob[at] in (0x53,0x54) and blob[at+1]==0x0e;at+=2;name=read();result[name]=read()
    return result

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--dummy',type=Path,required=True);p.add_argument('--binary',type=Path,required=True);p.add_argument('--output',type=Path,required=True);p.add_argument('--type',action='append',required=True);p.add_argument('--address-source',choices=['rva','file-offset'],default='rva',help='Select only after checking the producer address convention against known methods/fields.');a=p.parse_args();assert not a.output.exists(),'Use a fresh evidence directory';a.output.mkdir(parents=True)
    binary=a.binary.read_bytes();assert binary[:6]==b'\x7fELF\x02\x01';phoff=struct.unpack_from('<Q',binary,32)[0];size,count=struct.unpack_from('<HH',binary,54);segments=[]
    for i in range(count):
        kind,flags,offset,va,_,length,_,_=struct.unpack_from('<IIQQQQQQ',binary,phoff+i*size)
        if kind==1:segments.append((va,va+length,offset,flags))
    pe=dnfile.dnPE(str(a.dummy),clr_lazy_load=True);owners={m.row_index:str(t.TypeNamespace)+'.'+str(t.TypeName) for t in pe.net.mdtables.TypeDef.rows for m in t.MethodList};ctorNames={}
    wanted={i for i,typ in owners.items() if any(v in typ for v in a.type)};methods=[];decoder=Cs(CS_ARCH_ARM64,CS_MODE_LITTLE_ENDIAN)
    for attr in pe.net.mdtables.CustomAttribute.rows:
        if attr.Parent.table.name!='MethodDef' or attr.Parent.row_index not in wanted:continue
        key=(attr.Type.table.name,attr.Type.row_index)
        if key not in ctorNames:
            row=attr.Type.row;ctorNames[key]=str(row.Class.row.TypeName) if hasattr(row,'Class') and hasattr(row.Class.row,'TypeName') else owners.get(attr.Type.row_index,'')
        if not ctorNames[key].endswith('AddressAttribute'):continue
        values=strings(attr.Value.value);va=int(values['RVA'],16);length=int(values['Length'],16)
        if not va or not length:continue
        if a.address_source=='file-offset':
            raw=int(values['Offset'],16);candidates=[v for v in segments if v[2]<=raw and raw+length<=v[2]+v[1]-v[0] and v[3]&1];assert len(candidates)==1,(raw,length);start,_,offset,_=candidates[0];va=start+raw-offset
        else:
            candidates=[v for v in segments if v[0]<=va and va+length<=v[1] and v[3]&1];assert len(candidates)==1,(va,length);start,_,offset,_=candidates[0];raw=offset+va-start
        data=binary[raw:raw+length];assert len(data)==length;name=str(attr.Parent.row.Name);typ=owners[attr.Parent.row_index];filename=f'method-{len(methods):04d}.bin';(a.output/filename).write_bytes(data);ins=list(decoder.disasm(data,va))
        (a.output/(filename+'.arm64.txt')).write_text('\n'.join([typ+' :: '+name,f'VA 0x{va:X}; independently mapped file offset 0x{raw:X}','Native instructions; not original C#.']+[f'0x{i.address:08X}  {i.mnemonic} {i.op_str}' for i in ins]),encoding='utf-8')
        methods.append(dict(type=typ,method=name,address=va,fileOffset=raw,bytes=length,decodedBytes=sum(i.size for i in ins),mappingVerified=True,binaryFile=filename,reportedAttributes=values))
    report=dict(dummySha256=sha(a.dummy),binarySha256=sha(a.binary),addressSource=a.address_source,methods=methods,selectedTypes=a.type,limits=['ELF containment and decoding do not establish correct method identity. Confirm the producer address convention separately.','Address/length attributes are Cpp2IL-derived.','Decoded native instructions are static evidence; dynamic dispatch and original source semantics need separate tracing.']);(a.output/'method-map.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(dict(methods=len(methods),allMappingsVerified=True,fullyDecoded=sum(v['bytes']==v['decodedBytes'] for v in methods))))

if __name__=='__main__':main()
