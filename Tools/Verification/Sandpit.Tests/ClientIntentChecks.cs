using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

// Optional managed probe of the compiled, actual client cancellation methods.
// No Unity scene, GameObject, UI or native engine API is created or exercised.
static class ClientIntentChecks
{
    static readonly BindingFlags fields=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    static void Require(bool condition,string text){if(!condition)throw new Exception(text);}
    static void Set(object owner,string name,object value)=>owner.GetType().GetField(name,fields).SetValue(owner,value);
    static object Get(object owner,string name)=>owner.GetType().GetField(name,fields).GetValue(owner);
    public static void Run(string clientAssembly)
    {
        clientAssembly=Path.GetFullPath(clientAssembly);
        // The compiler response file lists the same references used for this DLL.
        var root=Path.GetDirectoryName(clientAssembly);
        var unity=Path.GetFullPath(Path.Combine(root,"../../Unity/FamilyPlayset"));
        var references=File.ReadAllLines(Path.ChangeExtension(clientAssembly,"rsp"))
            .Where(line=>line.StartsWith("-r:")).Select(line=>line.Substring(3).Trim('"'))
            .Select(path=>Path.IsPathRooted(path)?path:Path.GetFullPath(path,unity)).ToArray();
        AssemblyLoadContext.Default.Resolving+=(_,name)=>{
            var path=Path.Combine(root,name.Name+".dll");
            if(!File.Exists(path))path=Path.Combine(unity,"Library/ScriptAssemblies",name.Name+".dll");
            if(!File.Exists(path))path=references.FirstOrDefault(r=>Path.GetFileNameWithoutExtension(r)==name.Name);
            return path!=null && File.Exists(path)?AssemblyLoadContext.Default.LoadFromAssemblyPath(path):null;
        };
        var client=AssemblyLoadContext.Default.LoadFromAssemblyPath(clientAssembly);
        var core=AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(root,"LittleWeeps.Core.dll"));
        var worldType=core.GetType("LittleWeeps.Core.GameWorld");
        var world=worldType.GetMethod("Create").Invoke(null,new object[]{new[]{"one","two","three","four"}});
        world=worldType.GetMethod("WithDinosaurWorld").Invoke(null,new[]{world});
        var snapshot=worldType.GetMethod("Snapshot").Invoke(world,null);
        var sandpit=snapshot.GetType().GetField("sandpit").GetValue(snapshot);
        Set(sandpit,"round",2);Set(sandpit,"phase",1);
        var players=(Array)snapshot.GetType().GetField("players").GetValue(snapshot);Set(players.GetValue(0),"zone","daycare");
        var members=(Array)Get(sandpit,"members");Set(members.GetValue(0),"attending",true);
        world=worldType.GetMethod("Restore").Invoke(null,new[]{snapshot});
        var screenType=client.GetType("LittleWeeps.Client.GameScreen");
        Require(screenType!=null,"Missing actual GameScreen in "+client.Location);
        Require(screenType.GetField("destination",fields)!=null,"Missing destination in "+screenType.Assembly.Location+": "+string.Join(",",screenType.GetFields(fields).Select(f=>f.Name)));
        var vectorType=screenType.GetField("destination",fields).FieldType.GetGenericArguments()[0];
        object Vector(float x,float y)=>Activator.CreateInstance(vectorType,new object[]{x,y});
        object Screen(object destination,int capturedRound){
            // Bypass MonoBehaviour construction: only plain managed fields/methods below.
            var screen=RuntimeHelpers.GetUninitializedObject(screenType);
            Set(screen,"reader",Activator.CreateInstance(core.GetType("LittleWeeps.Core.BookCursor")));
            Set(screen,"<World>k__BackingField",world);Set(screen,"<Actor>k__BackingField","one");
            Set(screen,"sandpitApproach",0);Set(screen,"sandpitApproachRound",capturedRound);
            Set(screen,"sandpitOperation","scoop");Set(screen,"destination",destination);
            Set(screen,"treasureApproach","unrelated-treasure-intent");Set(screen,"sandpitSending",true);
            return screen;
        }
        var own=Screen(Vector(4130,350),1);
        screenType.GetMethod("CheckSandpitInput",fields).Invoke(own,null);
        Require((int)Get(own,"sandpitApproach")==-1 && (int)Get(own,"sandpitApproachRound")==-1 && Get(own,"sandpitOperation")==null,"Old client intent survived observed round change");
        Require(Get(own,"destination")==null,"Old Sandpit automatic walk survived");
        Require((bool)Get(own,"sandpitSending") && (string)Get(own,"treasureApproach")=="unrelated-treasure-intent","Cancellation changed unrelated/in-flight state");
        var otherDestination=Vector(1800,200);var other=Screen(otherDestination,1);
        screenType.GetMethod("CheckSandpitInput",fields).Invoke(other,null);
        Require(Get(other,"destination").Equals(otherDestination) && (int)Get(other,"sandpitApproach")==-1,"Cancellation stopped an unrelated destination");
        var current=Screen(Vector(4130,350),2);
        screenType.GetMethod("CheckSandpitInput",fields).Invoke(current,null);
        Require((int)Get(current,"sandpitApproach")==0 && (int)Get(current,"sandpitApproachRound")==2 && (string)Get(current,"sandpitOperation")=="scoop" && Get(current,"destination")!=null,"Current-round pending intent was canceled");
        Console.WriteLine("PASS: actual compiled client cancels old-round intent before pending-send checks; stops only its own destination; preserves current/unrelated intent and in-flight state (managed probe, no Unity scene).");
    }
}
