using EFT;
using QuestsExtended.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QuestsExtended.Quests
{
    internal class TradingQuestController : AbstractCustomQuestController
    {
        public TradingQuestController(QuestExtendedController questExtendedController) : base(questExtendedController)
        {

        }

        public void Init()
        {
            Plugin.Log.LogInfo("Created a TradingQuestController.");
        }

        public static void PurchaseMade(int amount, string currencyType, string traderId)
        {
            StaticManager.BeginCoroutine(PurchaseMadeDelayed(amount, currencyType, traderId));
        }

        private static IEnumerator PurchaseMadeDelayed(int price, string currencyType, string traderId)
        {
            yield return new WaitForSeconds(0.2f); // wait 0.2s

            try
            {
                //0 is rub, 1 is eur, 2 is dol
                //a dollar is 130 rub, a euro is 150 rub
                int unadjustedPrice = price;
                if (currencyType == "EUR") price *= 153;
                else if (currencyType == "USD") price *= 139;
                EQuestConditionTrading transactionMade = EQuestConditionTrading.CompleteAnyTransaction;
                var trade = _questController.GetActiveConditions(transactionMade);
                EQuestConditionTrading conditionsToAdd = EQuestConditionTrading.SpendMoneyOnTransaction;
                var conditions = _questController.GetActiveConditions(conditionsToAdd);
                Plugin.Log.LogInfo($"We have {conditions.Count} conditions(purchase)");
                foreach (var cond in conditions)
                {
                    CustomCondition cCond = cond.CustomCondition;
                    if (cCond.TraderIds == null && cCond.CurrencyTypes == null)
                    {
                        Plugin.Log.LogInfo($"Checking {cond.Condition.id} (all null)");
                        IncrementCondition(cond, price);
                        continue;
                    }
                    else if (cCond.TraderIds == null && cCond.CurrencyTypes != null)
                    {
                        Plugin.Log.LogInfo($"Checking {cond.Condition.id} (TraderIds null)");
                        if (!cCond.CurrencyTypes.Contains(currencyType)) continue;
                        IncrementCondition(cond, unadjustedPrice);
                        //Check if traderid is in the list, then convert euros and dollars to roubles, then increment condition by that amount.
                    }
                    else if (cCond.TraderIds != null && cCond.CurrencyTypes == null)
                    {
                        Plugin.Log.LogInfo($"Checking {cond.Condition.id} (CurrencyTypes null)");
                        if (!cCond.TraderIds.Contains(traderId)) continue;
                        IncrementCondition(cond, price);
                        //Check if currency type is in the list, then increment condition by that amount.
                    }
                    else if (cCond.TraderIds != null && cCond.CurrencyTypes != null)
                    {
                        Plugin.Log.LogInfo($"Checking {cond.Condition.id} (none null)");
                        if (!cCond.TraderIds.Contains(traderId)) continue;
                        if (!cCond.CurrencyTypes.Contains(currencyType)) continue;
                        IncrementCondition(cond, unadjustedPrice);
                        //Check if the trader id is in the list and if the currency type is in the list, then increment condition by that amount.
                    }
                    else Plugin.Log.LogError("Idk how but you managed to get an impossible combination. Well done.");
                    //Also copy this entire foreach to SaleMade AFTER YOU WRITE IT ALL!
                }
                foreach (var cond2 in trade)
                {
                    CustomCondition cCond = cond2.CustomCondition;
                    if (cCond.TraderIds == null && cCond.CurrencyTypes == null)
                    {
                        Plugin.Log.LogInfo($"Checking {cond2.Condition.id} (all null)");
                        IncrementCondition(cond2, 1);
                        continue;
                    }
                    else if (cCond.TraderIds == null && cCond.CurrencyTypes != null)
                    {
                        Plugin.Log.LogInfo($"Checking {cond2.Condition.id} (TraderIds null)");
                        if (!cCond.CurrencyTypes.Contains(currencyType)) continue;
                        IncrementCondition(cond2, 1);
                        //Check if traderid is in the list, then convert euros and dollars to roubles, then increment condition by that amount.
                    }
                    else if (cCond.TraderIds != null && cCond.CurrencyTypes == null)
                    {
                        Plugin.Log.LogInfo($"Checking {cond2.Condition.id} (CurrencyTypes null)");
                        if (!cCond.TraderIds.Contains(traderId)) continue;
                        IncrementCondition(cond2, 1);
                        //Check if currency type is in the list, then increment condition by that amount.
                    }
                    else if (cCond.TraderIds != null && cCond.CurrencyTypes != null)
                    {
                        Plugin.Log.LogInfo($"Checking {cond2.Condition.id} (none null)");
                        if (!cCond.TraderIds.Contains(traderId)) continue;
                        if (!cCond.CurrencyTypes.Contains(currencyType)) continue;
                        IncrementCondition(cond2, 1);
                        //Check if the trader id is in the list and if the currency type is in the list, then increment condition by that amount.
                    }
                }
                if (currencyType == "RUB")Plugin.Log.LogInfo($"Made it to the end of PurchaseMade! Player spent {unadjustedPrice} {currencyType}.");
                else Plugin.Log.LogInfo($"Made it to the end of PurchaseMade! Player spent {unadjustedPrice} {currencyType}, which converts to {price} RUB.");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Error during PurchaseMade: {ex}");
            }

        }
        
        public static void SaleMade(int price, string currencyType, string traderId)
        {
            //0 is rub, 1 is eur, 2 is dol
            //a dollar is 130 rub, a euro is 150 rub
            int unadjustedPrice = price;
            if (currencyType == "EUR") price *= 153;
            else if (currencyType == "USD") price *= 139;
            EQuestConditionTrading transactionMade = EQuestConditionTrading.CompleteAnyTransaction;
            var trade = _questController.GetActiveConditions(transactionMade);
            EQuestConditionTrading conditionsToAdd = EQuestConditionTrading.EarnMoneyOnTransaction;
            var conditions = _questController.GetActiveConditions(conditionsToAdd);
            Plugin.Log.LogInfo($"We have {conditions.Count} conditions(purchase)");
            foreach (var cond in conditions)
            {
                CustomCondition cCond = cond.CustomCondition;
                if (cCond.TraderIds == null && cCond.CurrencyTypes == null)
                {
                    Plugin.Log.LogInfo($"Checking {cond.Condition.id} (all null)");
                    IncrementCondition(cond, price);
                    continue;
                }
                else if (cCond.TraderIds == null && cCond.CurrencyTypes != null)
                {
                    Plugin.Log.LogInfo($"Checking {cond.Condition.id} (TraderIds null)");
                    if (!cCond.CurrencyTypes.Contains(currencyType)) continue;
                    IncrementCondition(cond, unadjustedPrice);
                    //Check if traderid is in the list, then convert euros and dollars to roubles, then increment condition by that amount.
                }
                else if (cCond.TraderIds != null && cCond.CurrencyTypes == null)
                {
                    Plugin.Log.LogInfo($"Checking {cond.Condition.id} (CurrencyTypes null)");
                    if (!cCond.TraderIds.Contains(traderId)) continue;
                    IncrementCondition(cond, price);
                    //Check if currency type is in the list, then increment condition by that amount.
                }
                else if (cCond.TraderIds != null && cCond.CurrencyTypes != null)
                {
                    Plugin.Log.LogInfo($"Checking {cond.Condition.id} (none null)");
                    if (!cCond.TraderIds.Contains(traderId)) continue;
                    if (!cCond.CurrencyTypes.Contains(currencyType)) continue;
                    IncrementCondition(cond, unadjustedPrice);
                    //Check if the trader id is in the list and if the currency type is in the list, then increment condition by that amount.
                }
                else Plugin.Log.LogError("Idk how but you managed to get an impossible combination. Well done.");
                //Also copy this entire foreach to SaleMade AFTER YOU WRITE IT ALL!
            }
            foreach (var cond2 in trade)
            {
                CustomCondition cCond = cond2.CustomCondition;
                if (cCond.TraderIds == null && cCond.CurrencyTypes == null)
                {
                    Plugin.Log.LogInfo($"Checking {cond2.Condition.id} (all null)");
                    IncrementCondition(cond2, 1);
                    continue;
                }
                else if (cCond.TraderIds == null && cCond.CurrencyTypes != null)
                {
                    Plugin.Log.LogInfo($"Checking {cond2.Condition.id} (TraderIds null)");
                    if (!cCond.CurrencyTypes.Contains(currencyType)) continue;
                    IncrementCondition(cond2, 1);
                    //Check if traderid is in the list, then convert euros and dollars to roubles, then increment condition by that amount.
                }
                else if (cCond.TraderIds != null && cCond.CurrencyTypes == null)
                {
                    Plugin.Log.LogInfo($"Checking {cond2.Condition.id} (CurrencyTypes null)");
                    if (!cCond.TraderIds.Contains(traderId)) continue;
                    IncrementCondition(cond2, 1);
                    //Check if currency type is in the list, then increment condition by that amount.
                }
                else if (cCond.TraderIds != null && cCond.CurrencyTypes != null)
                {
                    Plugin.Log.LogInfo($"Checking {cond2.Condition.id} (none null)");
                    if (!cCond.TraderIds.Contains(traderId)) continue;
                    if (!cCond.CurrencyTypes.Contains(currencyType)) continue;
                    IncrementCondition(cond2, 1);
                    //Check if the trader id is in the list and if the currency type is in the list, then increment condition by that amount.
                }
            }
            if (currencyType == "RUB") Plugin.Log.LogInfo($"Made it to the end of PurchaseMade! Player spent {unadjustedPrice} {currencyType}.");
            else Plugin.Log.LogInfo($"Made it to the end of PurchaseMade! Player spent {unadjustedPrice} {currencyType}, which converts to {price} RUB.");
        }

    }
}
