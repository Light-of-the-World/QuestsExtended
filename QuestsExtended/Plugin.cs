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

[BepInPlugin("com.dirtbikercjandlotw.QuestsExtended", "Quests Extended", "3.2.4")]
public class Plugin : BaseUnityPlugin
{
    internal const int TarkovVersion = 35392;
    internal static ManualLogSource Log;
    public static List<String> BannedConditionIds;
    public static List<String> BannedQuestIds;

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
        new HoldMostRecentlyDamagedPlayer().Enable();
        new KeyUsedOnDoorPatch().Enable();
        new KeyCardUsedOnDoorPatch().Enable();
        
    }
    private void FillBannedConditions()
    {
        BannedConditionIds = new List<string>();
        BannedConditionIds.AddRange(new string[]
        {
            "660fe523f4b8adbe11925c5e",
            "6513ef97971d04543779d03c",
            "65141df0e69594cf853a40b9",
            "6513ed7795e79afdeaa767c6",
            "651414b741f4ad07ba7d55f9",
            "651414eb3ec86f33dd54d978",
            "651415067c262d47d685c6d9",
            "6514151b2e8590fc2ac1d859",
            "6514151da13f174e3f52bc6e",
            "65141520143ef6349ad5f071",
            "6514156e21e1d85a7d029f8a",
            "65141570b18e12f60ba2e450",
            "65141571dbcb26761524e977",
            "651415cfe97ba875119ef01c",
            "651415d13c02ff4aa9e9a426",
            "657b21a3564a9197c2778f5a",
            "66c34bbbd5d174a3c9cd1382",
            "651418d2d2b2875692087490",
            "6527d3c461b75610b0857223",
            "651432072a2ef048f4e277fc",
            "6513eeb54f0adb43ec14e0f9",
            "6514124f2898c656ba65b1d6",
            "651411e599c1dc821414894a",
            "65140aa7877c54335a17c843",
            "66742cc94c02b62d30e18379",
            "6513f0735bafc372682987b2",
            "65141a397f14aa0040cf561a",
            "66c3290e6ddb3232ac35f475",
            "655b4a60b530cde7167d842f",
            "668bf4fbe40131ddf9bffdc7",
            "664f23ed786c7eba19a3e98f",
            "6527d345af3f7ee9f537a2dc",
            "65141b74fee1b6288d92068e",
            "6513f1676e95dadb20aa5843",
            "65142bdef6e6b9a6e9081739",
            "65142bf9d453b894d6dc429a",
            "670feb56b91cd521b33d16ad",
            "65140b330329e9a52eb9bfeb",
            "6512ea68d94f62c7d905a1ba",
            "651412ef0afef6dad1a21477",
            "65140ba9e91ae7a2bdcaa332",
            "66c328ce17df4e6ce92d1120",
            "66b49441b14491e93b51599c",
            "66b49a1662133b59e3e9a92c",
            "66b49a3e5665abc69d87b5e6",
            "66b4993717f669ba37f271a9",
            "66b4997047402dedee9d1c55",
            "66b499a1257913079a6e3645",
            "66b499c730acccbfc0436665",
            "66b499d7eb712b1555360355",
            "66b499eac980c9c597d23296",
            "6513ee015cbb9120ce42ac02",
            "6514142b50c1d03f34439529",
            "6512eb68f6c95fe8862e384d",
            "6512eb9a12da627da04880b3",
            "6512efeca198eb75ff9ca1c7",
            "6512f0166a9637a1cb352507",
            "6512f09316440cb67572c0fa",
            "6512f4a0df345dd5029b586a",
            "6512f4fb1ea20e8cd761de2a",
            "6512f819fddeee167c2518e3",
            "6512f83596d92e790ada99b0",
            "657b1feef3231fc23e3ccdf7",
            "65141cdcb9d5ff895410e1e4",
            "652909ac342bdd14e0bcb1bb",
            "652909cd3f9e480e9d1a3489",
            "652909ef8cb2a699ccbc2cf0",
            "65290de6e76953256668112c",
            "65290e22f16e69470b5d6145",
            "65290e51fee42b19970ccbfd",
            "65290e8d6193b1a4e12a7967",
            "65290ed47ef294bc6eb7ee85",
            "65290f1579363c7810e7233d",
            "65290f3fd7c6005f6d78f453",
            "65290f50897943fb9bf8955d",
            "657b1e91958145eb193f9a40",
            "66c34ab2c3eee7ac0c41d160",
            "65141bd40495cdcd5a295617",
            "670febc8d838bb4a9e934b61",
            "6513ed34e15d5337298168de",
            "65140bed0189951cd6816e28",
            "66e2a74cbe26c77a31e40e21",
            "675709e6d99b59e15bcd69f5",
            "6512f1835ccfd6cf6105c5f9",
            "6514103cf0ead1139b0691ba",
            "66c329f6f07551de7372f589",
            "655b4995528d47f68c1ffed6",
            "664f1f22aa7c03fbe75abc3c",
            "675998e314f914c3859afd4f",
        });
    }
    private void FillBannedQuests()
    {
        BannedQuestIds = new List<string>();
        BannedQuestIds.AddRange(new string[]
        {
            "68a020e49ff04a4a50037f3e",
        });
    }

    private void Start()
    {
        LoadAllQuestConditions();
        FillBannedConditions();
        FillBannedQuests();
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
