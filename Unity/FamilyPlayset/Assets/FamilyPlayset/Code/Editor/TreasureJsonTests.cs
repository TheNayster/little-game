using System;
using LittleWeeps.Core;
using UnityEngine;
namespace LittleWeeps.EditorTools { public static class TreasureJsonTests { public static void Run(){
 TreasureRuleTests.Run();foreach(var id in new[]{"step-1","step-2","step-3","step-4","site-0","site-1","site-2","step-6","step-7","mystery-2","mystery-3","mystery-4","surprise","retry","note-0","note-1","note-2"})if(Resources.Load<AudioClip>("Treasure/"+id)==null)throw new InvalidOperationException("Missing treasure audio "+id);
 foreach(var id in new[]{"treasure-cove","treasure-grove"})if(Resources.Load<Texture2D>("Scenery/"+id)==null)throw new InvalidOperationException("Missing treasure scenery "+id);Debug.Log("TREASURE_JSON_PASS: shared authority scenarios, Unity partial save retention, additive46 migration, two island panels and seventeen audio assets.");
} } }
