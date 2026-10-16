using LittleWeeps.Core;
namespace LittleWeeps.Client
{
    public sealed partial class GameScreen
    {
        void BuildFunfairFromWorld()=>BuildFunfair(()=>Shared?shared.View.funfair:World.ReadFunfair(),(op,item,target,x,y,done)=>{
            if(Shared)SubmitShared(SoloAction.Funfair,item,target,op,x,y,done);
            else done(Command(SoloAction.Funfair,item:item,target:target,value:op,x:x,y:y));
        });
    }
}
