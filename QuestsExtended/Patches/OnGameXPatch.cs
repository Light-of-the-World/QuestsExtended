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
using QuestsExtended.SaveLoadRelatedClasses;

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
        CompletedSaveData saveDataClass = __instance.GetOrAddComponent<CompletedSaveData>();
        saveDataClass.init();
        PhysicalQuestController.isRaidOver = false;
        PhysicalQuestController.LastPose = "Default";

        if (ConfigManager.DumpQuestZones.Value)
        {
            DumpTriggerZones();
        }
    }
    private static void DumpTriggerZones()
    {
        var zones = Object.FindObjectsOfType<TriggerWithId>();

        foreach (var zone in zones)
        {
            if( zone is QuestTrigger || zone is PlaceItemTrigger || zone is ExperienceTrigger)
            {
                Plugin.Log.LogInfo($"ZoneId: {zone.Id} Position: {zone.transform.position.ToString()} Type: {zone.GetType()}");
            }
        }
    }
}
internal class OnGameEndedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.UnregisterPlayer));
    }

    [PatchPostfix]
    private static void Postfix(GameWorld __instance)
    {
        CompletedSaveData call = __instance.GetComponent<CompletedSaveData>();
        call.SaveCompletedMultipleChoice();
        call.SaveCompletedOptionals();
    }
}