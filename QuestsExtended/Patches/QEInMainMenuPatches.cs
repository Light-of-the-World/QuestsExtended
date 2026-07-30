using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Quests;
using EFT.UI;
using GPUInstancer;
using HarmonyLib;
using QuestsExtended.Quests;
using QuestsExtended.SaveLoadRelatedClasses;
using QuestsExtended.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Xml;
using TMPro;
using UnityEngine;

namespace QuestsExtended.Patches
{
    internal class QEFromTraderScreensGroupPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderScreensGroup), nameof(TraderScreensGroup.method_6));
        }

        [PatchPostfix]
        private static void Postfix(TraderScreensGroup __instance)
        {
            if (Singleton<GameWorld>.Instance != null)
            {
                if (Singleton<GameWorld>.Instance.LocationId.ToLower() != "hideout") return;
            }
            Plugin.Log.LogInfo($"(QE) Checking for QEC (trader screen).");
            MenuUI menuUI = MenuUI.Instance;
            if (menuUI.gameObject.GetComponents<QuestExtendedController>().Length != 0)
            {
                Plugin.Log.LogInfo($"(QE) A Controller already exists. Double checking if our player has one. Current controllers: {menuUI.gameObject.GetComponents<QuestExtendedController>().Length}");
                QuestExtendedController _questController = null;
                foreach (QuestExtendedController QEC in menuUI.gameObject.GetComponents<QuestExtendedController>())
                {
                    if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
                    {
                        _questController = QEC;
                        break;
                    }
                }
                if (_questController == null)
                {
                    Plugin.Log.LogInfo("Our player does not have a QEC. Making a new one for them.");
                    _questController = menuUI.gameObject.AddComponent<QuestExtendedController>();
                    _questController.hasCompletedInitMM = true;
                    _questController.isInMainMenu = true;
                    AbstractQuestControllerClass sendingController = __instance.AbstractQuestControllerClass;
                    //Plugin.Log.LogInfo("Running InitForMainMenu. Remove this logger before publishing.");
                    _questController.InitFromMainMenu(sendingController);
                    CompletedSaveData completedSaveData = menuUI.gameObject.AddComponent<CompletedSaveData>();
                    completedSaveData.init(false);
                    _questController._optionalController.saveData = completedSaveData;
                    Plugin.Log.LogInfo($"(QE) Quest Controller created by TradingScreen.");
                }
                else Plugin.Log.LogInfo("Controller exists for our player, all good. Checking save data.");
                //if (_questController._optionalController.saveData != menuUI.GetComponent<CompletedSaveData>())
                if (!menuUI.gameObject.GetComponents<CompletedSaveData>().Contains<CompletedSaveData>(_questController._optionalController.saveData))
                {
                    Plugin.Log.LogInfo("SaveData missing or incorrect, creating now");
                    _questController._optionalController.saveData = menuUI.GetOrAddComponent<CompletedSaveData>();
                    if (!_questController._optionalController.saveData.hasDoneInit) _questController._optionalController.saveData.init(false);
                }
                else Plugin.Log.LogInfo("All good.");
            }
            else
            {
                Plugin.Log.LogInfo("No controllers exist. Creating one now.");
                QuestExtendedController controller = menuUI.gameObject.AddComponent<QuestExtendedController>();
                controller.hasCompletedInitMM = true;
                controller.isInMainMenu = true;
                AbstractQuestControllerClass sendingController = __instance.AbstractQuestControllerClass;
                //Plugin.Log.LogInfo("Running InitForMainMenu. Remove this logger before publishing.");
                controller.InitFromMainMenu(sendingController);
                Plugin.Log.LogInfo("Making save data...");
                CompletedSaveData completedSaveData = menuUI.gameObject.AddComponent<CompletedSaveData>();
                completedSaveData.init(false);
                controller._optionalController.saveData = completedSaveData;
                Plugin.Log.LogInfo($"(QE) Quest Controller created by TradingScreen.");
            }
        }

        internal class ResetMainMenuPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(DefaultUIButton), nameof(DefaultUIButton.OnPointerClick));
            }

            [PatchPostfix]
            private static void Postfix(DefaultUIButton __instance)
            {
                if (AbstractCustomQuestController.ResetMainMenu)
                {
                    //Plugin.Log.LogInfo($"Header text is {__instance.HeaderText}");
                    if (__instance.HeaderText.ToLower() == "ok")
                    OptionalConditionController.ResetMainMenuForQE();
                }
            }
        }
    }
    /*
    internal class QEBuyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderAssortmentControllerClass), nameof(TraderAssortmentControllerClass.Purchase));
        }

        [PatchPostfix]
        private static void Postfix(TraderAssortmentControllerClass __instance)
        {
            Plugin.Log.LogInfo($"Logging some things. Amount sold was {__instance.PreparedSum.Amount}, trader id is {__instance.traderClass.Id}. This should be a PURCHASE");
        }
    }
    */
    //Having trouble with BuyPatch, needs a lot more work. Skip for now.
    /*
    internal class QESellPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderAssortmentControllerClass), nameof(TraderAssortmentControllerClass.Sell));
        }

        [PatchPostfix]
        private static void Postfix(TraderAssortmentControllerClass __instance)
        {
            Plugin.Log.LogInfo($"Logging some things. Amount sold was {__instance.PreparedSum.Amount}, trader id is {__instance.traderClass.Id}. This should be a SALE");
            TradingQuestController.SaleMade(__instance.PreparedSum.Amount, __instance.traderClass.Id);
        }
    }
    */
    
    internal class QETransactionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderDealScreen), nameof(TraderDealScreen.method_0));
        }

        [PatchPrefix]
        private static void Prefix(TraderDealScreen __instance)
        {
            try
            {
                MenuUI menuUI = MenuUI.Instance;
                TraderClass traderClass = (TraderClass)AccessTools.Field(__instance.GetType(), "traderClass_1").GetValue(__instance);
                if (traderClass.Id == "6617beeaa9cfa777ca915b7c") { Plugin.Log.LogInfo("No transaction support for Ref at this time."); return; }
                TMP_Text[] money = (TMP_Text[])AccessTools.Field(__instance.GetType(), "_equivalentSumValue").GetValue(__instance);
                int currency = int.Parse(Regex.Replace(money[0].text, @"[^\d]", ""));
                string currencyType = "RUB";
                if (traderClass != null)
                {
                    //Plugin.Log.LogInfo($"Is the price of the transaction somewhere around {money[0].text}?");
                    if (money[0].text.Contains("₽"))
                    {
                        //Plugin.Log.LogInfo($"Roubles");
                        currencyType = "RUB";
                    }
                    else if (money[0].text.Contains("€"))
                    {
                        //Plugin.Log.LogInfo($"Euros");
                        currencyType = "EUR";
                    }
                    else if (money[0].text.Contains("$"))
                    {
                        //Plugin.Log.LogInfo($"Dollars");
                        currencyType = "USD";
                    }
                    //This works perfectly. We can create what we need to now.
                    QuestExtendedController _questController = null;
                    foreach (QuestExtendedController QEC in menuUI.gameObject.GetComponents<QuestExtendedController>())
                    {
                        if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
                        {
                            _questController = QEC;
                            break;
                        }
                    }
                    if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: QETransactionPatch)"); return; }
                    if (__instance.ETradeMode_0 == ETradeMode.Purchase) _questController._tradingQuestController.PurchaseMade(currency, currencyType, traderClass.Id);
                    else if (__instance.ETradeMode_0 == ETradeMode.Sale) _questController._tradingQuestController.SaleMade(currency, currencyType, traderClass.Id);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error during Transaction: {ex}");
            }

        }
    }

    internal class ResetAFSOnQuestAccept : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(QuestView), nameof(QuestView.StartQuest));
        }

        [PatchPostfix]
        private static void Postfix(QuestView __instance)
        {
            if (!AbstractCustomQuestController.QuestsToResetAFS.Contains(__instance.QuestId)) { /*Plugin.Log.LogInfo("wrong quest");*/ return; }
            if (AbstractCustomQuestController.QuestsToResetAFS.Count != 0 && AbstractCustomQuestController.hasResetWithoutQuestAccept)
            {
                Plugin.Log.LogInfo("Resetting some quest AFSs.");
                MenuUI ui = MenuUI.Instance;
                QuestExtendedController QEC = ui.gameObject.GetComponent<QuestExtendedController>();
                QEC._optionalController.RestoreAFSForQuests();
            }
            //else Plugin.Log.LogInfo("Right method, wrong time");
            AbstractCustomQuestController.hasResetWithoutQuestAccept = false;
        }
    }

    internal class MainMenuControllerGetterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MainMenuControllerClass), nameof(MainMenuControllerClass.method_5));
        }

        [PatchPostfix]
        private static void Postfix(MainMenuControllerClass __instance)
        {
            Plugin.Log.LogInfo("MMCC.method_5 ran");
            Plugin.TransitioningFromRaid = false;
            if (OptionalConditionController.mainMenuControllerClass != __instance) OptionalConditionController.mainMenuControllerClass = __instance;
            if (AbstractCustomQuestController.wipeData)
            {
                CompletedSaveData.WipeQEProfileData();
                AbstractCustomQuestController.wipeData = false;
            }
        }
    }

    internal class WipeQEDataOnNewCharacterPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass2305), nameof(GClass2305.method_5));
        }

        [PatchPostfix]
        private static void Postfix(GClass2305 __instance)
        {
            Plugin.Log.LogInfo("GClass2305.method_5 has triggered. Player is either creating a new profile, wiping, or prestiging. Deleting QE profile data when menu loads");
            AbstractCustomQuestController.wipeData = true;
        }
    }

    /*
    internal class WillThisWork : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderAssortmentControllerClass), nameof(TraderAssortmentControllerClass.Purchase));
        }

        [PatchPrefix]
        private static void Prefix(TraderAssortmentControllerClass __instance)
        {
            double netWorth = 0;
            foreach (KeyValuePair<MongoID, ItemTemplate> keyValuePair in Singleton<ItemFactoryClass>.Instance.ItemTemplates)
            {
                if (keyValuePair.Value == __instance.SelectedItem.Template)
                {
                    MongoID mongoID;
                    ItemTemplate itemTemplate;
                    keyValuePair.Deconstruct(out mongoID, out itemTemplate);
                    ItemTemplate itemTemplate2 = itemTemplate;
                    netWorth = (double)itemTemplate2.CreditsPrice;
                    Plugin.Log.LogInfo("Found the thing");
                    break;
                }
            }
            Plugin.Log.LogInfo($"Purchased {__instance.CurrentQuantity} {__instance.SelectedItem} from {__instance.traderClass.LocalizedName}. Does this come to around {netWorth * __instance.CurrentQuantity}?");
        }
    }
    */
}
