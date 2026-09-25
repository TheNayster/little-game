using System;
using System.Linq;
using System.Text.Json;
using LittleWeeps.Core;

// Use the game's validator, not a second implementation of its item/receipt rules.
// Input contains a world and public profile IDs only; no enrollment secrets.
try
{
    var input = JsonDocument.Parse(Console.In.ReadToEnd());
    var world = JsonSerializer.Deserialize<SoloSnapshot>(input.RootElement.GetProperty("world").GetRawText(),
        new JsonSerializerOptions { IncludeFields = true });
    SoloWorld.Validate(world);
    var profiles = input.RootElement.GetProperty("profiles").EnumerateArray().Select(p => p.GetString()).OrderBy(p => p);
    if (world.schema != 2 || !world.players.Select(p => p.id).OrderBy(p => p).SequenceEqual(profiles))
        throw new Exception("Unsupported shared world or mismatched players.");
    Console.WriteLine("VALID");
}
catch { Console.Error.WriteLine("Checkpoint failed the game's recovery validation."); Environment.ExitCode = 1; }
