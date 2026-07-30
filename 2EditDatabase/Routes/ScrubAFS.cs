using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System.Reflection;
using System.Text.Json.Serialization;

namespace QEServerPart.Routes;
/*
public record AFSData : IRequestData
{
    [JsonPropertyName("Data")]
    public List<string> Data { get; set; }
}

[Injectable]
public class ServerSender(JsonUtil jsonUtil, AFSScrubber scrubber) : StaticRouter(jsonUtil,
    [new RouteAction<AFSData>(
        "/QE/QEScrubAFS", (, rawIDs, _, _) =>
        {
            List<string> ids = new List<string>();
            int i = 0;
            foreach (var id in rawIDs.Data)
            {
                ids.Add((string)JsonConvert.DeserializeObject(rawIDs.Data[i]));
                i++;
            }
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
        logger.Info("Adjusting some quests...");
        var quests = databaseService.GetQuests();
        foreach (string id in questIDs)
        {
            quests[id].Conditions.AvailableForStart.Clear();
            if (quests[id].QuestName != null) logger.Info($"Removed the AFS for the quest {quests[id].QuestName}");
            else logger.Info($"Removed the AFS for the quest {quests[id].Id}");
        }
    }
}
*/
[Injectable]
public class AFSScrubber : StaticRouter
{
    private static JsonUtil? _jsonUtil;
    private static HttpResponseUtil? _httpResponseUtil;
    private static DatabaseService? _databaseService;
    private static string? _modPath;
    private static string? _savesPath;
    private static ISptLogger<AFSScrubber>? _logger;
    private static Dictionary<int, string> AFSIDs = new Dictionary<int, string>();

    public static List<string>? RecievedIds = null;
    public AFSScrubber(
          JsonUtil jsonUtil,
          HttpResponseUtil httpResponseUtil,
          DatabaseService databaseService,
          ModHelper modHelper,
          ISptLogger<AFSScrubber> logger
      ) : base(
          jsonUtil,
          GetCustomRoutes()
      )
    {
        _jsonUtil = jsonUtil;
        _httpResponseUtil = httpResponseUtil;
        _databaseService = databaseService;
        _modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()); ;
        _savesPath = System.IO.Path.Join(_modPath, "Data");
        _logger = logger;
    }
    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction<AFSData>("/QE/QEScrubAFS",
                async (
                    url,
                    data,
                    sessionID,
                    output
                ) => await AdjustAFSInQuests(data)
            ),
            new RouteAction<AFSData>("/QE/QERestoreAFS",
            async (
                    url,
                    data,
                    sessionID,
                    output
                ) => await RestoreAFSInQuests(data))
        ];
    }
    public static ValueTask<string> AdjustAFSInQuests(AFSData data)
    {
        var quests = _databaseService.GetQuests();
        _logger.Info("Adjusting some quests...");
        if (data == null) _logger.Info($"data was null! error message inbound.");
        if (data.AfsQuestIds == null) _logger.Info($"afsQI was null! error message inbound.");
        foreach (var id in data.AfsQuestIds)
        {
            if (quests[id] == null) _logger.Info("0");
            if (quests[id].Conditions == null) _logger.Info("1");
            if (quests[id].Conditions.AvailableForStart == null) _logger.Info("2");
            AFSIDs.Add(AFSIDs.Count + 1, quests[id].Conditions.AvailableForStart[0].Id);
            quests[id].Conditions.AvailableForStart.Clear();
            if (quests[id].QuestName != null) _logger.Info($"Removed the AFS for the quest {quests[id].QuestName}");
            else _logger.Info($"Removed the AFS for the quest {quests[id].Id}");
        }
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }
    public static ValueTask<string> RestoreAFSInQuests(AFSData data)
    {
        var quests = _databaseService.GetQuests();
        _logger.Info("Attempting to restore the AFS for quests some quests...");
        foreach (var id in data.AfsQuestIds)
        {
            //_logger.Info("Log0");
            quests[id].Conditions.AvailableForStart.Add(GenerateCondition());
            _logger.Info($"Restored the AFS for the quest {quests[id].Id}");
        }
        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }
    public static QuestCondition GenerateCondition()
    {
        //_logger.Info("Log1");
        QuestCondition newAFSCondition = new QuestCondition()
        {
            CompareMethod = ">=",
            ConditionType = "Level",
            DynamicLocale = false,
            Id = GetAFSID(),
            Value = 99,
        };
        return newAFSCondition;
    }
    public static string GetAFSID()
    {
        //_logger.Info("Log2");
        string id = "";
        if (AFSIDs.Count == 0)
        {
            _logger.Error("AFSIDs is null, that's no good. Using a backup.");
            id = "6a28166e155422a7c133b115";
        }
        else
        {
            //_logger.Info("Log3");
            id = AFSIDs[AFSIDs.Count];
            AFSIDs.Remove(AFSIDs.Count);
        }
            return id;
    }
}
public record AFSData : IRequestData
{
    [JsonPropertyName("templateIds")]
    //public List<string> afsQuestIds = new List<string>();
    public MongoId[] AfsQuestIds { get; set; }
}

/*
public static ValueTask<string> AdjustAFSInQuests(List<string> ids)
{
    var quests = _databaseService.GetQuests();
    _logger.Info("Adjusting some quests...");
    if (ids == null) _logger.Info($"ids was null! error message inbound.");
    foreach (var id in ids)
    {
        if (quests[id] == null) _logger.Info("0");
        if (quests[id].Conditions == null) _logger.Info("1");
        if (quests[id].Conditions.AvailableForStart == null) _logger.Info("2");
        quests[id].Conditions.AvailableForStart.Clear();
        if (quests[id].QuestName != null) _logger.Info($"Removed the AFS for the quest {quests[id].QuestName}");
        else _logger.Info($"Removed the AFS for the quest {quests[id].Id}");
    }
    return new ValueTask<string>(_httpResponseUtil.NullResponse());
}
*/