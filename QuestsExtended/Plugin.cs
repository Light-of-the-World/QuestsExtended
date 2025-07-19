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
using static QuestsExtended.Patches.QEFromTraderScreensGroupPatch;

namespace QuestsExtended;

[BepInPlugin("com.dirtbikercjandlotw.QuestsExtended", "Quests Extended", "3.1.0")]
public class Plugin : BaseUnityPlugin
{
    internal const int TarkovVersion = 35392;
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
        new OnUnregisterPlayerPatch().Enable();
        new EnemyDamagePatch().Enable();
        new SearchContainerPatch().Enable();
        new SwitchPatch().Enable();
        new ArmourDurabilityPatch().Enable();
        new DestroyLimbsPatch().Enable();
        new EnemyKillPatch().Enable();
        new EnterBlindFirePatch().Enable();
        new ExitBlindFirePatch().Enable();
        new CustomConditionChecker().Enable();
        new VanillaConditionChecker().Enable();
        new FixMalfunctionPatch().Enable();
        new QEFromTraderScreensGroupPatch().Enable();
        new IHopeThisWorks().Enable();
        new CheckForQECBeforeHideout().Enable();
        new WorkoutPatch().Enable();
        new CollectCraftedItemPatch().Enable();
        /*
        new QEBuyPatch().Enable();
        new QESellPatch().Enable();
        */
        new QETransactionPatch().Enable();
        new MainMenuControllerGetterPatch().Enable();
        new BSGWHYISYOURCODELIKETHIS().Enable();
        new ResetMainMenuPatch().Enable();
        new BlockMessagePatch().Enable();
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
