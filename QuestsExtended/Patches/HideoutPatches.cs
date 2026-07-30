using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.Hideout;
using EFT.UI;
using HarmonyLib;
using QuestsExtended.Quests;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuestsExtended.Patches
{
    internal class CollectCraftedItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutClass), nameof(HideoutClass.GetProducedItems));
        }
        /*
        [PatchPrefix]
        private static bool Prefix(HideoutClass __instance)
        {
            MenuUI menuUI = MenuUI.Instance;
            if (menuUI.GetComponent<QuestExtendedController>() != null)
            {
                Plugin.Log.LogWarning("QEC not detected, blocking item collection.");

                NotificationManagerClass.DisplayMessageNotification(
                    "You must click on Trader or Character before doing anything in the Hideout! This is to ensure that Quests Extended works properly. Apologies for the inconvenience!",
                    ENotificationDurationType.Default,
                    ENotificationIconType.Alert
                    );
                return false;
            }
            else
            {
                Plugin.Log.LogInfo("Sanity check: We are in the prefix of HideoutClass.GetProducedItems.");
                Plugin.Log.LogInfo($"We have a QEC registered to {menuUI.GetComponent<QuestExtendedController>()._player.Profile.Nickname}");
            }
            return true;
        }
        */
        [PatchPostfix]
        private static void Postfix(HideoutClass __instance, GClass2431 producer)
        {
            MenuUI menuUI = MenuUI.Instance;
            EAreaType eArea = producer.AreaType;
            QuestExtendedController _questController = null;
            foreach (QuestExtendedController QEC in menuUI.gameObject.GetComponents<QuestExtendedController>())
            {
                if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
                {
                    _questController = QEC;
                    break;
                }
            }
            if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: CollectCraftedItemPatch)"); return; }
            if (eArea == EAreaType.WaterCollector || eArea == EAreaType.BitcoinFarm || eArea == EAreaType.BoozeGenerator)
            {
                _questController._hideoutQuestController.CollectCyclicItemFromHideout(eArea);
            }
            else if (eArea == EAreaType.ScavCase || eArea == EAreaType.CircleOfCultists)
            {
                _questController._hideoutQuestController.CollectScavOrCultist(eArea);
            }
            else _questController._hideoutQuestController.CollectItemFromHideout(eArea);
        }
    }
    internal class WorkoutPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.StartQte));
        }
        /*
        [PatchPrefix]
        private static bool Prefix(WorkoutBehaviour __instance)
        {
            MenuUI menuUI = MenuUI.Instance;
            if (menuUI.GetComponent<QuestExtendedController>() != null)
            {
                Plugin.Log.LogWarning("QEC not detected, blocking workout.");

                NotificationManagerClass.DisplayMessageNotification(
                    "You must click on Trader or Character before doing anything in the Hideout! This is to ensure that Quests Extended works properly. Apologies for the inconvenience!",
                    ENotificationDurationType.Default,
                    ENotificationIconType.Alert
                    );
                return false;
            }
            else return true;
        }
        */
        [PatchPostfix]
        private static void Postfix(ref HideoutPlayerOwner owner)
        {
            if (owner.Player.Profile == ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile)
            {
                MenuUI menuUI = MenuUI.Instance;
                QuestExtendedController _questController = null;
                foreach (QuestExtendedController QEC in menuUI.gameObject.GetComponents<QuestExtendedController>())
                {
                    if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
                    {
                        _questController = QEC;
                        break;
                    }
                }
                if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: WorkoutPatch)"); return; }
                _questController._hideoutQuestController.PlayerDidWorkout();
            }
            else Plugin.Log.LogError($"Tried to credit a workout, but HideoutOwner was listed as {owner.Player.Profile.Id}");
            if (owner.Player.Profile == null) Plugin.Log.LogError("Hideout owner was null!");
        }
    }
    /*
    internal class CultistCircleActivatedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(CircleOfCultistsBehaviour), nameof(CircleOfCultistsBehaviour.method_11));
        }

        [PatchPostfix]
        private static void Postfix(CircleOfCultistsBehaviour __instance)
        {
            HideoutQuestController.CollectScavOrCultist(EAreaType.CircleOfCultists);
        }
    }
    */
    /*
    internal class CultistCircleWatchOne : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(CircleOfCultistsPanel), nameof(CircleOfCultistsPanel.method_0));
        }

        [PatchPostfix]
        private static void Postfix(CircleOfCultistsPanel __instance)
        {
            Plugin.Log.LogInfo("Cultist Watch: method_0 is correct.");
        }
    }

    internal class CultistCircleWatchTwo : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(CircleOfCultistsPanel), nameof(CircleOfCultistsPanel.method_0));
        }

        [PatchPostfix]
        private static void Postfix(CircleOfCultistsPanel __instance)
        {
            Plugin.Log.LogInfo("Cultist Watch: method_1 is correct.");
        }
    }
    */
    //Neither of these are correct.
}
