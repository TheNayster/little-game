using System;
using System.Threading.Tasks;

namespace LittleWeeps.Adapters
{
    // Owned by the gameplay thread. At most one detached snapshot is in flight;
    // newer changes stay dirty in the world, rather than forming a disk backlog.
    public sealed class BackgroundSave
    {
        private Task writing;
        public bool Pending => writing != null;
        public void Start(Action commit)
        {
            if(Pending)throw new InvalidOperationException("A save is already in progress.");
            if(commit==null)throw new ArgumentNullException(nameof(commit));
            writing=Task.Run(commit);
        }
        // Only explicit lifecycle/session boundaries wait. Ordinary frames poll.
        // Report failures to the owner so uncommitted progress remains dirty.
        public bool Complete(bool wait,out Exception error)
        {
            error=null;
            if(writing==null)return true;
            if(!wait && !writing.IsCompleted)return false;
            try{writing.GetAwaiter().GetResult();}
            catch(Exception e){error=e;}
            finally{writing=null;}
            return true;
        }
    }
}
