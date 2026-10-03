using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LittleWeeps.Adapters;

static partial class Program
{
    static void BackgroundSaveTests()
    {
        Test("background save leaves gameplay caller free and permits only one writer",()=>{
            var writer=new BackgroundSave();using var entered=new ManualResetEventSlim();using var release=new ManualResetEventSlim();
            var caller=Environment.CurrentManagedThreadId;var worker=caller;
            try
            {
                writer.Start(()=>{worker=Environment.CurrentManagedThreadId;entered.Set();if(!release.Wait(5000))throw new TimeoutException();});
                Check(entered.Wait(5000) && worker!=caller && writer.Pending);
                Check(!writer.Complete(false,out var error) && error==null);
                Throws(()=>writer.Start(()=>{}));
            }
            finally{release.Set();}
            Check(writer.Complete(true,out var result) && result==null && !writer.Pending);
        });
        Test("failed background commit is observable and retry retains atomic previous good save",()=>{
            var path=Path.Combine(root,"background-atomic.save");var store=new CheckpointStore(path,s=>s.StartsWith("valid"));
            store.Save("valid-before");var writer=new BackgroundSave();
            writer.Start(()=>store.Save("invalid"));Check(writer.Complete(true,out var error) && error!=null);
            Check(store.Load().Payload=="valid-before" && !writer.Pending);
            writer.Start(()=>store.Save("valid-after"));Check(writer.Complete(true,out error) && error==null);
            Check(store.Load().Payload=="valid-after");
            Check(new CheckpointStore(path+".bak",s=>true).Load().Payload=="valid-before");
        });
        Test("lifecycle barrier waits for the in-flight write before a later commit",()=>{
            var writer=new BackgroundSave();using var entered=new ManualResetEventSlim();using var release=new ManualResetEventSlim();
            var path=Path.Combine(root,"background-order.txt");
            writer.Start(()=>{entered.Set();if(!release.Wait(5000))throw new TimeoutException();File.WriteAllText(path,"old");});
            Check(entered.Wait(5000));
            var barrier=Task.Run(()=>{Check(writer.Complete(true,out var error) && error==null);File.WriteAllText(path,"new");});
            try{Check(!barrier.Wait(50));}finally{release.Set();}
            Check(barrier.Wait(5000) && File.ReadAllText(path)=="new");
        });
    }
}
