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
using SPT.Reflection.Utils;

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
        if (__instance.LocationId.ToLower() == "hideout") return;
        if (__instance is HideoutGameWorld) return;
        Plugin.Log.LogInfo("[QE] Raid starting");
        __instance.GetOrAddComponent<QuestExtendedController>();
        CompletedSaveData saveDataClass = __instance.GetOrAddComponent<CompletedSaveData>();
        saveDataClass.init();
        PhysicalQuestController.LastPose = "Default";
        AbstractCustomQuestController.isRaidOver = false;

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
internal class OnUnregisterPlayerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.UnregisterPlayer));
    }

    [PatchPostfix]
    private static void Postfix(GameWorld __instance, ref IPlayer iPlayer)
    {
        if (__instance.LocationId.ToLower() == "hideout") return;
        if (__instance is HideoutGameWorld) return;
        if (iPlayer.ProfileId == ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId && !AbstractCustomQuestController.isRaidOver)
        {
            Plugin.Log.LogInfo("[QE] Raid over.");
            AbstractCustomQuestController.isRaidOver = true;
            CompletedSaveData call = __instance.GetComponent<CompletedSaveData>();
            call.SaveCompletedMultipleChoice();
            call.SaveCompletedOptionals();
        }
    }
}