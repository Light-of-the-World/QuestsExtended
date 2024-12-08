using System.Linq;
using System.Reflection;
using System.Security.Policy;
using EFT;
using EFT.Interactive;
using EFT.Quests;
using HarmonyLib;
using QuestsExtended.Quests;
using QuestsExtended.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace QuestsExtended.Patches;

internal class OnGameStartedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }
    
    [PatchPostfix]
    private static void Postfix(GameWorld __instance)
    {
        __instance.GetOrAddComponent<QuestExtendedController>();

        if (ConfigManager.DumpQuestZones.Value)
        {
            DumpTriggerZones();
        }
    }

    private static void DumpTriggerZones()
    {
        var zones = Object.FindObjectsOfType<TriggerWithId>();

        foreach (var zone in zones.Where(z => z is QuestTrigger || z is PlaceItemTrigger || z is ExperienceTrigger))
        {
            Plugin.Log.LogInfo($"ZoneId: {zone.Id} Position: {zone.transform.position.ToString()} Type: {zone.GetType()}");
        }
    }
}