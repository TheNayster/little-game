using System;
using System.IO;
using System.Text;
using LittleWeeps.NetworkProbe;

internal static class DiagnosticFileWriterTests
{
    static int Main(string[] args)
    {
        var path=Path.Combine(args[0],"locked-status.json");
        var utf8=new UTF8Encoding(false,true);
        foreach(var code in new[]{32,33,1175,1176,1177})
            if(!DiagnosticFileWriter.IsReplacementConflict(unchecked((int)0x80070000)|code))throw new Exception("Replacement code not handled: "+code);
        File.WriteAllText(path,"before",utf8);
        using(var held=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read))
        {
            File.WriteAllText(path+".pending","probe",utf8);
            try{File.Replace(path+".pending",path,null);throw new Exception("Expected real lock failure");}
            catch(IOException error)
            {
                Console.WriteLine("Reproduced native error "+(error.HResult&0xffff)+": "+error.Message);
                if(!DiagnosticFileWriter.IsReplacementConflict(error.HResult))throw;
            }
            for(var i=0;i<100;i++)
                if(DiagnosticFileWriter.TryWrite(path,"after",utf8))throw new Exception("Locked observation unexpectedly replaced");
            if(File.ReadAllText(path)!="before")throw new Exception("Old observation lost");
        }
        if(!DiagnosticFileWriter.TryWrite(path,"after",utf8) || File.ReadAllText(path)!="after")throw new Exception("Did not recover after unlock");
        if(DiagnosticFileWriter.IsReplacementConflict(unchecked((int)0x80070070)))throw new Exception("Disk-full error swallowed");
        try{DiagnosticFileWriter.TryWrite(Path.Combine(args[0],"missing","status.json"),"x",utf8);throw new Exception("Missing directory swallowed");}
        catch(DirectoryNotFoundException){}
        Console.WriteLine("PASS: locked observations are skipped; unlock publishes again; permanent I/O faults propagate.");
        return 0;
    }
}
