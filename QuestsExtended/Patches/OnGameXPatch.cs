using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Quests;
using EFT.UI;
using EFT.UI.Screens;
using HarmonyLib;
using QuestsExtended.Quests;
using QuestsExtended.SaveLoadRelatedClasses;
using QuestsExtended.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Reflection;
using System.Security.Policy;
using UnityEngine;
using static EFT.UI.MenuScreen;

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
        Plugin.PlayerInRaid = true;
        if (__instance.MainPlayer.Side == EPlayerSide.Savage)
        {
            Plugin.Log.LogInfo($"No need to attatch QE to a scav raid, aborting.");
            AbstractCustomQuestController.isScavRaid = true;
            AbstractCustomQuestController.isRaidOver = false;
            return;
        }
        QuestExtendedController controller = Singleton<GameWorld>.Instance.gameObject.AddComponent<QuestExtendedController>();
        controller.InitForRaid();
        CompletedSaveData saveDataClass = Singleton<GameWorld>.Instance.gameObject.AddComponent<CompletedSaveData>();
        saveDataClass.init(true);
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
        var zones = UnityEngine.Object.FindObjectsOfType<TriggerWithId>();

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
        if (iPlayer.IsAI) return;
        if (iPlayer.Profile.ProfileId != Plugin.PlayerProfileID) return;
        Plugin.TransitioningFromRaid = true;
        if (AbstractCustomQuestController.isScavRaid)
        {
            Plugin.Log.LogInfo("Ending scav raid, no need to remove a non-existent QEC"); Plugin.PlayerInRaid = false; return;
        }
        if (iPlayer.ProfileId == Plugin.PlayerProfileID)
        {
            if (!AbstractCustomQuestController.isRaidOver)
            {
                AbstractCustomQuestController.isRaidOver = true;
                Plugin.Log.LogInfo("[QE] Raid over.");
                Plugin.PlayerInRaid = false;
                CompletedSaveData call = null;
                foreach (CompletedSaveData dats in __instance.gameObject.GetComponents<CompletedSaveData>())
                {
                    if (dats.SaveProfileID == Plugin.PlayerProfileID)
                    {
                        dats.SaveCompletedMultipleChoice();
                        dats.SaveCompletedOptionals();
                        call = dats;
                        Plugin.Log.LogInfo("Data saved.");
                        break;
                    }
                }
                //Plugin.Log.LogInfo("1");
                QuestExtendedController[] controllers = __instance.gameObject.GetComponents<QuestExtendedController>();
                //Plugin.Log.LogInfo("2");
                QuestExtendedController controller = null;
                if (controllers == null)
                {
                    Plugin.Log.LogError("No controllers detected. Aborting (1)."); return;
                }
                if (controllers.Length == 0)
                {
                    Plugin.Log.LogError("No controllers detected. Aborting (2)."); return;
                }
                //Plugin.Log.LogInfo("3");
                foreach (QuestExtendedController controllerTest in controllers)
                {
                    if (controllerTest.LocalPlayerID == null)
                    {
                        Plugin.Log.LogError("Why is the LocalPlayerID null??? Something did not initiate properly."); return;
                    }
                    if (controllerTest.LocalPlayerID == Plugin.PlayerProfileID)
                    {
                        controller = controllerTest; break;
                    }
                }
                //Plugin.Log.LogInfo("4");
                if (controller != null) Plugin.Log.LogInfo("We successfully got the QE controller, attempting to remove it");
                else return;
                controller.OnDestroy();
                //Plugin.Log.LogInfo("5");
                if (call != null) { call.SaveCompletedOptionals(); }
                else Plugin.Log.LogInfo("SaveData was null");
            }
        }
        else Plugin.Log.LogError($"Incorrect id on Unregister Player. ProfileID we got was: {iPlayer.ProfileId}. We should have: {Plugin.PlayerProfileID}");
    }
}

internal class CreateQEInMainMenuOnInventoryScreenClick : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(InventoryScreen), nameof(InventoryScreen.Show), [typeof(IHealthController), typeof(InventoryController), typeof(AbstractQuestControllerClass), typeof(AbstractAchievementControllerClass), typeof(AbstractPrestigeControllerClass), typeof(CompoundItem), typeof(EInventoryTab), typeof(ISession), typeof(ItemContextAbstractClass), typeof(bool)]);
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
        if (menuUI.gameObject.GetComponents<QuestExtendedController>().Length != 0)
        {
            Plugin.Log.LogInfo("(QE) Controller already exists, checking if our player has one");
            QuestExtendedController[] controllers = menuUI.gameObject.GetComponents<QuestExtendedController>();
            foreach (QuestExtendedController controllerTest in controllers)
            {
                if (controllerTest.LocalPlayerID == Plugin.PlayerProfileID)
                {
                    Plugin.Log.LogInfo("(QEC) Player has a controller, all good."); return;
                }
            }
        }
        Plugin.Log.LogInfo("We don't have a controller for this player, making one now");
        QuestExtendedController controller = menuUI.gameObject.AddComponent<QuestExtendedController>();
        controller.hasCompletedInitMM = true;
        controller.isInMainMenu = true;
        AbstractQuestControllerClass sendingController = questController;
        Plugin.Log.LogInfo("Running InitForMainMenu. Remove this logger before publishing.");
        controller.InitFromMainMenu(sendingController);
        Plugin.Log.LogInfo($"(QE) Quest Controller created by InventoryScreen.");
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
        //Plugin.Log.LogInfo("Sanity check: Prefix");
        MenuUI menuUI = MenuUI.Instance;
        if (menuUI.gameObject.GetComponents<QuestExtendedController>().Length == 0)
        {
            Plugin.Log.LogWarning("QEC not detected, blocking hideout travel (Method: CheckForQECBeforeHideout).");

            NotificationManagerClass.DisplayMessageNotification(
                "You must click on Trader or Character before entering the Hideout! This is to ensure that Quests Extended works properly. Apologies for the inconvenience!",
                ENotificationDurationType.Default,
                ENotificationIconType.Alert
                );
            return false;
        }
        else
        {
            /*Plugin.Log.LogInfo("QEC exists, proceeding to hideout");*/
            QuestExtendedController[] controllers = menuUI.gameObject.GetComponents<QuestExtendedController>();
            foreach (QuestExtendedController controllerTest in controllers)
            {
                if (controllerTest.LocalPlayerID == Plugin.PlayerProfileID)
                {
                    Plugin.Log.LogInfo("(QEC) PLayer has a controller, all good."); return true;
                }
            }
            Plugin.Log.LogWarning("Our local player doesn't have a QEC, blocking.");
            NotificationManagerClass.DisplayMessageNotification(
                "You must click on Trader or Character before entering the Hideout! This is to ensure that Quests Extended works properly. Apologies for the inconvenience!",
                ENotificationDurationType.Default,
                ENotificationIconType.Alert
                );
            return false; 
        }
    }
    /*
    [PatchPostfix]
    private static void Postfix(MenuScreen __instance)
    {
        Plugin.Log.LogInfo("Sanity check: Postfix");
        MenuUI menuUI = MenuUI.Instance;
        if (menuUI.GetComponent<QuestExtendedController>() == null)
        {
            Plugin.Log.LogWarning("QEC not detected, warning player.");

            NotificationManagerClass.DisplayMessageNotification(
                "You must click on Trader or Character before entering the Hideout! This is to ensure that Quests Extended works properly. Please go back to the main menu and select a different option.",
                ENotificationDurationType.Default,
                ENotificationIconType.Alert
                );
        } 
    }
    */
}
/*
internal class TaskHideoutButtonPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MenuTaskBar), nameof(MenuTaskBar.OnScreenChanged));
    }
    [PatchPrefix]
    private static bool Prefix(MenuTaskBar __instance, ref EEftScreenType eftScreenType)
    {
        Plugin.Log.LogInfo("Sanity check: Prefix of MenuTaskBar");
        if (eftScreenType != EEftScreenType.Hideout && eftScreenType != EEftScreenType.HideoutCircleOfCultists && eftScreenType != EEftScreenType.HideoutAreaItemsTransfer && eftScreenType != EEftScreenType.HideoutAreaMannequinEquipment)
        {
            Plugin.Log.LogInfo("Not tranferring to hideout, skipping");
            return true;
        }
        MenuUI menuUI = MenuUI.Instance;
        if (menuUI.GetComponent<QuestExtendedController>() == null)
        {
            Plugin.Log.LogWarning("QEC not detected, blocking hideout travel (5).");

            NotificationManagerClass.DisplayMessageNotification(
                "You must click on Trader or Character before entering the Hideout! This is to ensure that Quests Extended works properly. Apologies for the inconvenience!",
                ENotificationDurationType.Default,
                ENotificationIconType.Alert
                );
            return false;
        }
        else { Plugin.Log.LogInfo("QEC exists, proceeding to hideout"); return true; }
    }
}
*/
internal class HideoutSelectedHandlerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TarkovApplication), nameof(TarkovApplication.method_57));
    }
    [PatchPrefix]
    private static bool Prefix(TarkovApplication __instance, ref EMenuType menuType)
    {
        /*
        Plugin.Log.LogInfo("Sanity check: Prefix of HideoutSelectedHandler");
        if (menuType != EMenuType.Hideout)
        {
            Plugin.Log.LogInfo("Not going to hideout, skipping"); return true;
        }
        */
        if (!AbstractCustomQuestController.isRaidOver) { return true; } //Do not use this method in raid!
        MenuUI menuUI = MenuUI.Instance;
        if (menuUI.gameObject.GetComponents<QuestExtendedController>().Length == 0)
        {
            Plugin.Log.LogWarning("QEC not detected, blocking hideout travel (Method: HideoutSelectedHandlerPatch).");

            NotificationManagerClass.DisplayMessageNotification(
                "You must click on Trader or Character before entering the Hideout! This is to ensure that Quests Extended works properly. Apologies for the inconvenience!",
                ENotificationDurationType.Default,
                ENotificationIconType.Alert
                );
            return false;
        }
        else
        {
            /*Plugin.Log.LogInfo("QEC exists, proceeding to hideout");*/
            QuestExtendedController[] controllers = menuUI.gameObject.GetComponents<QuestExtendedController>();
            foreach (QuestExtendedController controllerTest in controllers)
            {
                if (controllerTest.LocalPlayerID == Plugin.PlayerProfileID)
                {
                    Plugin.Log.LogInfo("(QEC) PLayer has a controller, all good."); return true;
                }
            }
            Plugin.Log.LogWarning("Our local player doesn't have a QEC, blocking.");
            NotificationManagerClass.DisplayMessageNotification(
                "You must click on Trader or Character before entering the Hideout! This is to ensure that Quests Extended works properly. Apologies for the inconvenience!",
                ENotificationDurationType.Default,
                ENotificationIconType.Alert
                );
            return false;
        }
        //else { /*Plugin.Log.LogInfo("QEC exists, proceeding to hideout");*/ return true; }
    }
}
internal class ProducedItemsButtonPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MenuTaskBar), nameof(MenuTaskBar.method_6));
    }
    [PatchPrefix]
    private static bool Prefix(MenuTaskBar __instance)
    {
        //Plugin.Log.LogInfo("Sanity check: Prefix of MenuTaskBar");
        if (!AbstractCustomQuestController.isRaidOver) { return true; } //Do not use this method in raid!
        MenuUI menuUI = MenuUI.Instance;
        if (menuUI.GetComponent<QuestExtendedController>() == null)
        {
            Plugin.Log.LogWarning("QEC not detected, blocking hideout travel (Method: ProcudedItemsButtonPatch).");

            NotificationManagerClass.DisplayMessageNotification(
                "You must click on Trader or Character before entering the Hideout! This is to ensure that Quests Extended works properly. Apologies for the inconvenience!",
                ENotificationDurationType.Default,
                ENotificationIconType.Alert
                );
            return false;
        }
        else { /*Plugin.Log.LogInfo("QEC exists, proceeding to hideout");*/ return true; }
    }
}
//Consider force saving quest data when the player clicks exit? Seems like it doesn't always update... might be weird dev profile things, though.