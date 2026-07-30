using EFT;
using QuestsExtended.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestsExtended.Quests
{
    internal class HideoutQuestController : AbstractCustomQuestController
    {
        public HideoutQuestController(QuestExtendedController questExtendedController) : base(questExtendedController)
        {

        }
        public void Init()
        {
            Plugin.Log.LogInfo("Created a HideoutQuestController. We are ready to add code here.");
        }

        public void CollectItemFromHideout(EAreaType eArea)
        {
            Plugin.Log.LogInfo("We are in CollectItemFromHideout, running loggers...");
            if (_questController == null)
            {
                Plugin.Log.LogError("We are missing a quest controller! Stopping method."); return;
            }
            EQuestConditionHideout conditionsToAdd = EQuestConditionHideout.CraftAnyItem;
            var conditions = _questController.GetActiveConditions(conditionsToAdd);
            foreach (var cond in conditions)
            {
                if (cond.CustomCondition.Workstations != null)
                {
                    foreach (string name in cond.CustomCondition.Workstations)
                    {
                        if (name == eArea.ToString())
                        {
                            IncrementCondition(cond, 1);
                            break;
                        }
                        else if ((name.ToLower() == "lavatory" || name.ToLower() == "watercloset") && eArea == EAreaType.WaterCloset)
                        {
                            IncrementCondition(cond, 1);
                            break;
                        }
                        else if ((name.ToLower() == "nutritionunit" || name.ToLower() == "kitchen") && eArea == EAreaType.Kitchen)
                        {
                            IncrementCondition(cond, 1);
                            break;
                        }
                        else if (name.ToLower() == "medstation" && eArea == EAreaType.MedStation)
                        {
                            IncrementCondition(cond, 1);
                            break;
                        }
                        else if (name.ToLower() == "intelligencecenter" && eArea == EAreaType.IntelligenceCenter)
                        {
                            IncrementCondition(cond, 1);
                            break;
                        }
                        else if (name.ToLower() == "watercollector" && eArea == EAreaType.WaterCollector)
                        {
                            IncrementCondition(cond, 1);
                            break;
                        }
                    }
                }
                else
                IncrementCondition(cond, 1);
            }
            //Plugin.Log.LogInfo($"Test run. We just collected an item from {eArea}, and reached the end of CollectItemFromHideout.");
        }

        public void CollectCyclicItemFromHideout (EAreaType eArea)
        {
            EQuestConditionHideout conditionsToAdd = EQuestConditionHideout.CraftCyclicItem;
            var conditions = _questController.GetActiveConditions(conditionsToAdd);
            foreach (var cond in conditions)
            {
                if (cond.CustomCondition.Workstations != null)
                {
                    foreach (string name in cond.CustomCondition.Workstations)
                    {
                        if (name.ToLower() == eArea.ToString().ToLower())
                        {
                            IncrementCondition(cond, 1);
                            break;
                        }
                        else if (name.ToLower() == "bitcoinfarm" && eArea == EAreaType.BitcoinFarm)
                        {
                            IncrementCondition(cond, 1);
                            break;
                        }
                        else if (name.ToLower() == "boozegenerator" && eArea == EAreaType.BoozeGenerator)
                        {
                            IncrementCondition(cond, 1);
                            break;
                        }
                        else if (name.ToLower() == "watercollector" && eArea == EAreaType.WaterCollector)
                        {
                            IncrementCondition(cond, 1);
                            break;
                        }
                    }
                }
                else
                IncrementCondition(cond, 1);
            }
            //Plugin.Log.LogInfo($"Test run. We just collected an item from {eArea}, and reached the end of CollectCyclicItemFromHideout.");
        }

        public void CollectScavOrCultist (EAreaType eArea)
        {
            EQuestConditionHideout conditionsToAdd = EQuestConditionHideout.EmptyHI;
            if (eArea == EAreaType.ScavCase) conditionsToAdd = EQuestConditionHideout.CollectScavCase;
            else if (eArea == EAreaType.CircleOfCultists) conditionsToAdd = EQuestConditionHideout.CollectCultistOffering;
            else { Plugin.Log.LogError("Called CollectScavOrCultist when the EAreaType is neither!"); return; }
            var conditions = _questController.GetActiveConditions(conditionsToAdd);
            foreach (var cond in conditions)
            {
                IncrementCondition(cond, 1);
            }
            Plugin.Log.LogInfo($"Test run. We just collected an item from {eArea}, and reached the end of CollectScavOrCultist.");
        }

        public void PlayerDidWorkout()
        {
            EQuestConditionHideout conditionsToAdd = EQuestConditionHideout.CompleteWorkout;
            var conditions = _questController.GetActiveConditions(conditionsToAdd);
            foreach (var cond in conditions)
            {
                IncrementCondition(cond, 1);
            }
            //Plugin.Log.LogInfo("Test run. Reached the end of CompleteWorkout without errors");
        }
    }
}
