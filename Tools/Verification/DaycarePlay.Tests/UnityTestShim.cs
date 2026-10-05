// Portable route tests execute the actual build-gate class. Serialization
// checks must run in Unity; this shim cannot masquerade as JsonUtility.
namespace UnityEngine {
 public static class Debug {public static void Log(object value)=>System.Console.WriteLine(value);}
 public static class JsonUtility {
  public static string ToJson(object value)=>throw new System.NotSupportedException("Run Unity JSON checks in Unity.");
  public static T FromJson<T>(string value)=>throw new System.NotSupportedException("Run Unity JSON checks in Unity.");
 }
}
