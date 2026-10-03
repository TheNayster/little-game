using System;
using LittleWeeps.Core;
using LittleWeeps.Client;

static class ScoopFeedbackChecks
{
    static void Need(bool value,string message){if(!value)throw new Exception(message);}
    public static void Run()
    {
        var world=GameWorld.WithDinosaurWorld(GameWorld.Create("a","b","c","d"));
        var state=world.ReadSandpit();var feedback=new SandScoopFeedback();
        feedback.Observe(state,true,0);
        state.moulds[1].scoops=1;feedback.Observe(state,true,.1f);
        Need(feedback.Events[1]==1 && feedback.Remaining(1)==SandScoopFeedback.Duration,"accepted fill starts effect at correct mould");
        feedback.Observe(state,false,0);Need(feedback.Remaining(1)==0,"leaving during active flight clears presentation");
        feedback.Observe(state,true,0);Need(feedback.Events[1]==1 && feedback.Remaining(1)==0,"re-entry does not replay the interrupted flight");
        feedback.Observe(state,true,.1f);Need(feedback.Events[1]==1,"unchanged/rejected state invents no effect");
        state.moulds[1].scoops=3;feedback.Observe(state,true,.1f);Need(feedback.Events[1]==3,"two serialized scoops observed together");
        feedback.Observe(state,true,1);Need(feedback.Remaining(1)==0 && state.moulds[1].scoops==3,"effect expires without overriding authority");
        state.moulds[1].scoops=0;feedback.Observe(state,true,0);Need(feedback.Events[1]==3 && feedback.Remaining(1)==0,"crumble is no accepted scoop");
        state.moulds[0].scoops=1;feedback.Observe(state,true,0);state.round++;feedback.Observe(state,true,0);Need(feedback.Remaining(0)==0,"new lesson clears old effect");
        state.moulds[0].scoops=2;feedback.Observe(state,false,0);feedback.Observe(state,true,0);Need(feedback.Remaining(0)==0 && feedback.Events[0]==1,"leave/re-entry baseline never animates saved fill");
        var late=new SandScoopFeedback();late.Observe(state,true,0);Need(late.Events[0]==0 && late.Remaining(0)==0,"late join/reconnect treats restored fill as baseline");
        Console.WriteLine("PASS: accepted-state-only scoop feedback, merged sibling increments, rejection/no change, expiry, crumble, lesson reset and leave/re-entry/reconnect baselines.");
    }
}
