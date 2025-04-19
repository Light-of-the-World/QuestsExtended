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

internal class ArmourDurabilityPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ArmorComponent), nameof(ArmorComponent.ApplyDamage));
    }
    [PatchPostfix]
    private static void Postfix(ref float __result, ref DamageInfoStruct damageInfo)
    {
        /*
        Plugin.Log.LogInfo($"[StatsPatch] ArmorComponent.ApplyDamage was called in general. Logging some various stats as a start. The durability dealt to the armour was {__result}, let's see who caused it...");
        if (damageInfo.Player.IsAI) Plugin.Log.LogInfo("This call was caused by an AI");
        else if (!damageInfo.Player.IsAI) Plugin.Log.LogInfo("This call was NOT caused by an AI. Presumabely caused by the player?");
        else Plugin.Log.LogInfo("damageInfo.Player.IsAI came back as neither true nor false. That's concerning...");
        */
        if (!damageInfo.Player.IsAI) { StatCounterQuestController.ArmourDamageProcessor(__result, damageInfo); }
        //Do not forget to remove this log before publication!
    }
}