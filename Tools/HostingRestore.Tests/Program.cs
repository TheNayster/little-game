using System.Text.Json;
using HostingRestore;

try
{
    if (args.Length == 2 && args[0] == "self-test")
        Environment.ExitCode = RestoreChecks.Run(args[1]);
    else if (args.Length == 4 && args[0] == "stage")
    {
        var expected = JsonSerializer.Deserialize<ExpectedCheckpoint>(File.ReadAllText(args[2]), RestoreStage.Json);
        var state = RestoreStage.Publish(args[1], expected, args[3]);
        Console.WriteLine(RestoreStage.Encode(new { staged = true, revision = state.revision, players = state.players.Length }));
    }
    else throw new ArgumentException("Use self-test NEW_EVIDENCE_DIRECTORY or stage REPLICA EXPECTED_JSON NEW_AUTHORITY_DIRECTORY.");
}
catch (Exception error)
{
    Console.Error.WriteLine("Restore experiment refused: " + error.GetType().Name + ". No existing authority was replaced.");
    Environment.ExitCode = 1;
}
