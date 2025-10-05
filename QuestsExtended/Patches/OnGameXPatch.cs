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
using EFT.UI;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using EFT.Communications;
using Comfort.Common;

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
        //if (AbstractCustomQuestController.isRaidOver == false) return;
        Plugin.Log.LogInfo("[QE] Raid starting");
        QuestExtendedController controller = __instance.GetOrAddComponent<QuestExtendedController>();
        controller.InitForRaid();
        CompletedSaveData saveDataClass = __instance.GetOrAddComponent<CompletedSaveData>();
        saveDataClass.init();
        PhysicalQuestController.LastPose = "Default";
        AbstractCustomQuestController.isRaidOver = false;
        DumpTriggerZones();
        //next line is a fika specific test
        //Plugin.Log.LogWarning("MainPlayer is listed as"+__instance.MainPlayer.Profile.Nickname);
        //if (PhysicalQuestController._pedometer != null) Plugin.Log.LogWarning("Pedometer is set");
        //PhysicalQuestController._pedometer = __instance.MainPlayer.Pedometer;
            /*
        if (ConfigManager.DumpQuestZones.Value)
        {
            DumpTriggerZones();
        }
            */
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
    private static void Postfix(GameWorld __instance, IPlayer iPlayer)
    {
        if (__instance.LocationId.ToLower() == "hideout") return;
        if (__instance is HideoutGameWorld) return;
        if (iPlayer.ProfileId == ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId)
        {
            if (!AbstractCustomQuestController.isRaidOver)
            {
                AbstractCustomQuestController.isRaidOver = true;
                Plugin.Log.LogInfo("[QE] Raid over.");
                CompletedSaveData call = __instance.GetComponent<CompletedSaveData>();
                call.SaveCompletedMultipleChoice();
                QuestExtendedController controller = __instance.GetComponent<QuestExtendedController>();
                if (controller != null) Plugin.Log.LogInfo("We successfully got the QE controller, attempting to remove it");
                else return;
                controller.OnDestroy();
                call.SaveCompletedOptionals();
            }
        }
    }
}

internal class IHopeThisWorks : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(InventoryScreen), nameof(InventoryScreen.Show), [typeof(IHealthController), typeof(InventoryController), typeof(AbstractQuestControllerClass), typeof(AbstractAchievementControllerClass), typeof(GClass3695), typeof(CompoundItem), typeof(EInventoryTab), typeof(ISession), typeof(ItemContextAbstractClass), typeof(bool)]);
    }

    [PatchPostfix]
    private static void Postfix(InventoryScreen __instance, AbstractQuestControllerClass questController)
    {
        if (Singleton<GameWorld>.Instance != null)
        {
            if (Singleton<GameWorld>.Instance.LocationId.ToLower() != "hideout") return;
        }
        Plugin.Log.LogInfo($"(QE) Checking for QEC (inventory screen).");
        MenuUI menuUI = MenuUI.Instance;
        if (menuUI.GetComponent<QuestExtendedController>() != null)
        {
            Plugin.Log.LogInfo("(QE) Controller already exists, all good");
        }
        QuestExtendedController controller = menuUI.GetOrAddComponent<QuestExtendedController>();
        if (controller.hasCompletedInitMM == false)
        {
            controller.hasCompletedInitMM = true;
            QuestExtendedController.isInMainMenu = true;
            AbstractQuestControllerClass sendingController = questController;
            Plugin.Log.LogInfo("Running InitForMainMenu. Remove this logger before publishing.");
            controller.InitFromMainMenu(sendingController);
            Plugin.Log.LogInfo($"(QE) Quest Controller created by TradingScreen.");
        }
    }
}

internal class CheckForQECBeforeHideout : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MenuScreen), nameof(MenuScreen.method_14));
    }

    [PatchPrefix]
    private static bool Prefix(MenuScreen __instance)
    {
        MenuUI menuUI = MenuUI.Instance;
        if (menuUI.GetComponent<QuestExtendedController>() == null)
        {
            Plugin.Log.LogWarning("QEC not detected, blocking invoke. Make sure you write the rest of the code before publishing!");

            NotificationManagerClass.DisplayMessageNotification(
                "You must click on Trader or Character before entering the Hideout! This is to ensure that Quests Extended works properly. Apologies for the inconvenience!",
                ENotificationDurationType.Default,
                ENotificationIconType.Alert
                );
            return false;
        }
        else return true;
    }
}
//Consider force saving quest data when the player clicks exit? Seems like it doesn't always update... might be weird dev profile things, though.