using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace QEServerPart;

/// <summary>
/// This is the replacement for the former package.json data. This is required for all mods.
///
/// This is where we define all the metadata associated with this mod.
/// You don't have to do anything with it, other than fill it out.
/// All properties must be overriden, properties you don't use may be left null.
/// It is read by the mod loader when this mod is loaded.
/// </summary>
public record ModMetadata : AbstractModMetadata
{
    /// <summary>
    /// Any string can be used for a modId, but it should ideally be unique and not easily duplicated
    /// a 'bad' ID would be: "mymod", "mod1", "questmod"
    /// It is recommended (but not mandatory) to use the reverse domain name notation,
    /// see: https://docs.oracle.com/javase/tutorial/java/package/namingpkgs.html
    /// </summary>
    public override string ModGuid { get; init; } = "com.lightoftheworld.questsextended";
    public override string Name { get; init; } = "QuestsExtended";
    public override string Author { get; init; } = "LightOfTheWorld";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new(typeof(ModMetadata).Assembly.GetName().Version?.ToString(3));
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string? License { get; init; } = "MIT";
}

// We want to load after PostDBModLoader is complete, so we set our type priority to that, plus 1.
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class QEServer(
    ISptLogger<QEServer> logger, // We are injecting a logger similar to example 1, but notice the class inside <> is different
    DatabaseService databaseService)
    : IOnLoad // Implement the `IOnLoad` interface so that this mod can do something
{
    // Our constructor

    /// <summary>
    /// This is called when this class is loaded, the order in which its loaded is set according to the type priority
    /// on the [Injectable] attribute on this class. Each class can then be used as an entry point to do
    /// things at varying times according to type priority
    /// </summary>
    public Task OnLoad()
    {
        // When SPT starts, it stores all the data found in (SPT_Data\Server\database) in memory
        // We can use the 'databaseService' we injected to access this data, this includes files from EFT and SPT

        // lets write a nice log message to the server console so players know our mod has made changes
        logger.Success("Finished Editing Database!");
        
        // Inform server we have finished
        return Task.CompletedTask;
    }

    public void AdjustAFSInQuests(List<string> questIDs)
    {
        var quests = databaseService.GetQuests();
        foreach (string id in questIDs)
        {
            quests[id].Conditions.AvailableForStart.Clear();
            if (quests[id].QuestName != null) logger.Info($"Removed the AFS for the quest {quests[id].QuestName}");
            else logger.Info($"Removed the AFS for the quest {quests[id].Id}");
        }
    }
}
