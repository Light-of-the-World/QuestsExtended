using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using QuestsExtended.Models;
using QuestsExtended.Patches;
using QuestsExtended.Utils;

namespace QuestsExtended;

[BepInPlugin("com.dirtbikercj.QuestsExtended", "Quests Extended", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal const int TarkovVersion = 30626;
    internal static ManualLogSource Log;

    internal static Dictionary<string, CustomQuest> Quests { get; private set; } = [];
    
    private void Awake()
    {
        if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
        {
            throw new Exception("Invalid EFT Version");
        }

        Log = Logger;
        
        RE.CacheTypes();
        ConfigManager.InitConfig(Config);
        
        new OnGameStartedPatch().Enable();
        new SetConditionCurrentValuePatch().Enable();
    }

    private void Start()
    {
        LoadAllQuestConditions();
    }

    private void LoadAllQuestConditions()
    {
        var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        directory = Path.Combine(directory, "Quests");

        var files = Directory.GetFiles(directory);
        
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            var quests = JsonConvert.DeserializeObject<Dictionary<string, CustomQuest>>(text);
            
            Quests.AddRange(quests);
        }
        
        Log.LogInfo($"Loaded {Quests.Count} custom quests");
    }
}
