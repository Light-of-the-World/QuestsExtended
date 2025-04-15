using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using QuestsExtended.Quests;
using QuestsExtended.Utils;
using SPT.Reflection.Patching;

namespace QuestsExtended.Patches;

internal class EnemyDamagePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationStatisticsCollectorAbstractClass), nameof(LocationStatisticsCollectorAbstractClass.OnEnemyDamage));
    }
    [PatchPostfix]
    private static void Postfix(LocationStatisticsCollectorAbstractClass __instance, ref DamageInfoStruct damage, ref float distance)
    {
        //Plugin.Log.LogInfo($"[StatsPatch] OnEnemyDamage called. Sending to StatCounterQuestController for processing.");
        StatCounterQuestController.EnemyDamageProcessor(damage, distance);
        //Do not forget to remove this log before publication!
    }
}

internal class SearchContainerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationStatisticsCollectorAbstractClass), nameof(LocationStatisticsCollectorAbstractClass.OnInteractWithLootContainer));
    }
    [PatchPostfix]
    private static void Postfix(LocationStatisticsCollectorAbstractClass __instance, ref Item item)
    {
        //Plugin.Log.LogInfo($"[StatsPatch] OnEnemyDamage called. Sending to StatCounterQuestController for processing.");
        StatCounterQuestController.SearchingContainer(item);
        //Do not forget to remove this log before publication!
    }
}