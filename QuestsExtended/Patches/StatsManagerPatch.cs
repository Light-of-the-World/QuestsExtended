using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using EFT;
using HarmonyLib;
using QuestsExtended.Quests;
using QuestsExtended.Utils;
using SPT.Reflection.Patching;

namespace QuestsExtended.Patches;

internal class StatsManagerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationStatisticsCollectorAbstractClass), nameof(LocationStatisticsCollectorAbstractClass.OnEnemyDamage));
    }
    [PatchPostfix]
    private static void Postfix(LocationStatisticsCollectorAbstractClass __instance, ref DamageInfoStruct damage, ref float distance)
    {
        Plugin.Log.LogInfo($"[StatsPatch] OnEnemyDamage called. Sending to StatCounterQuestController for processing.");
        StatCounterQuestController.EnemyDamageProcessor(damage, distance);
        //Do not forget to remove this log before publication!
    }
}