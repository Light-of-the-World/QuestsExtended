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
using Comfort.Common;
using System.Collections.Generic;
using TMPro;
using System.Text.RegularExpressions;
using System.Xml;
using System;
using System.Globalization;

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
            if (menuUI.GetComponent<QuestExtendedController>() != null)
            {
                Plugin.Log.LogInfo("(QE) Controller already exists");
            }
            QuestExtendedController controller = menuUI.GetOrAddComponent<QuestExtendedController>();
            if (controller.hasCompletedInitMM == false)
            {
                controller.hasCompletedInitMM = true;
                QuestExtendedController.isInMainMenu = true;
                AbstractQuestControllerClass sendingController = __instance.AbstractQuestControllerClass;
                //Plugin.Log.LogInfo("Running InitForMainMenu. Remove this logger before publishing.");
                controller.InitFromMainMenu(sendingController);
                CompletedSaveData completedSaveData = menuUI.GetOrAddComponent<CompletedSaveData>();
                completedSaveData.init();
                OptionalConditionController.saveData = completedSaveData;
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
                    Plugin.Log.LogInfo($"Header text is {__instance.HeaderText}");
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
                    if (__instance.ETradeMode_0 == ETradeMode.Purchase) TradingQuestController.PurchaseMade(currency, currencyType, traderClass.Id);
                    else if (__instance.ETradeMode_0 == ETradeMode.Sale) TradingQuestController.SaleMade(currency, currencyType, traderClass.Id);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error during Transaction: {ex}");
            }

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
            if (OptionalConditionController.mainMenuControllerClass != __instance) OptionalConditionController.mainMenuControllerClass = __instance;
            if (!CompletedSaveData.hasScrubbedAFS)
            {
                CompletedSaveData.hasScrubbedAFS = true;
                CompletedSaveData.LoadQuestsThatWereStarted();
                OptionalConditionController.RemoveAFSOnGameLaunch(CompletedSaveData.QuestsStartedByQE);
            }
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
