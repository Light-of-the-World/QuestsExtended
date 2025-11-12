using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Text.Json.Serialization;

public record AFSData : IRequestData
{
    [JsonPropertyName("Data")]
    public List<string> Data { get; set; }
}

[Injectable]
public class ServerSender(JsonUtil jsonUtil, AFSScrubber scrubber) : StaticRouter(jsonUtil,
    [new RouteAction<AFSData>(
        "/QE/QEScrubAFS", (_, rawIDs, _, _) =>
        {
            List<string> ids = rawIDs.Data;
            scrubber.AdjustAFSInQuests(ids);
            string yes = "yep";
            return ValueTask.FromResult(jsonUtil.Serialize(yes) ?? string.Empty);
        }
    )
{
    },]
    );

[Injectable]
public class AFSScrubber(DatabaseService databaseService, ISptLogger<AFSScrubber> logger)
{
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