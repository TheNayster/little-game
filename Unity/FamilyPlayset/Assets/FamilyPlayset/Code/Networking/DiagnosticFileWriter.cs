using System.IO;
using System.Text;

namespace LittleWeeps.NetworkProbe
{
    // Disposable observations only. Durable CheckpointStore does not use this.
    internal static class DiagnosticFileWriter
    {
        internal static bool TryWrite(string path,string payload,Encoding encoding)
        {
            try
            {
                var temp=path+".pending";
                File.WriteAllText(temp,payload,encoding);
                if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
                return true;
            }
            catch(IOException error) when(IsReplacementConflict(error.HResult))
            {
                // Skip this observation; the next normal update retries. Never
                // block the game loop, delete the old target or touch its save.
                return false;
            }
        }

        internal static bool IsReplacementConflict(int hresult)
        {
            var code=hresult&0xffff;
            // ReplaceFile can wrap sharing failure as 1175, rather than 32/33.
            // 1176/1177 are its other incomplete replacement outcomes.
            return code==32 || code==33 || code==1175 || code==1176 || code==1177;
        }
    }
}
