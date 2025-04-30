using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using Newtonsoft.Json;
using QuestsExtended.Models;
using SPT.Common.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using SPT.Common.Utils;
using QuestsExtended.SaveLoadRelatedClasses;
namespace QuestsExtended.Quests
{
    internal class OptionalConditionController : AbstractCustomQuestController
    {
        public OptionalConditionController(QuestExtendedController questExtendedController) : base(questExtendedController)
        {

        }
        public static List<string> _activeQuestIds = new List<string>();
        private static bool alreadyLoaded = false;

        public static void AddQuestIDToActiveList(string id)
        {
            if (alreadyLoaded) return;
            alreadyLoaded = true;
            _activeQuestIds.Add(id);
          //  Plugin.Log.LogInfo($"Quest id {id} made it to QuestStarter");
        }
        public static void HandleOptionalConditionCompletion(GClass3719<IConditional> controller, IConditional conditional, Condition condition)
        {
            var conditions = _questController.GetActiveConditions(EQuestConditionGen.CompleteOptionals);
            foreach (var cond in conditions)
            {
                if (CompletedChildConditions.CompletedOptionals.Contains(condition.id))
                {
                  //  Plugin.Log.LogInfo("Condition was already within CompletedOptionals list");
                    continue;
                }
                if (cond.Quest.Id != conditional.Id)
                {
                  //  Plugin.Log.LogInfo("Tried to increment a CompleteOptionals task on a quest that doesn't contain the completed optional, skipping.");
                    continue;
                }
                CompletedChildConditions.CompletedOptionals.Add(condition.id);
                //Plugin.Log.LogInfo($"We are adding the id {condition.id} to CompletedOptionals and increasing the CompleteOptionals condition by 1.");
                IncrementCondition(cond, 1);
            }
            string questId = conditional.Id;
            string conditionId = condition.id;

            if (!Plugin.Quests.TryGetValue(questId, out CustomQuest customQuest))
            {
                Plugin.Log.LogWarning($"StartMultiChoiceQuest: Could not find CustomQuest data for quest ID {questId}");
                return;
            }

            CustomCondition completedCondition = default;
            bool conditionFound = false;

            foreach (CustomCondition cond in customQuest.Conditions)
            {
                if (cond.ConditionId == conditionId)
                {
                    completedCondition = cond;
                    conditionFound = true;
                    break;
                }
            }

            if (!conditionFound || !customQuest.IsMultipleChoiceStarter)
            {
                return;
            }

            if (completedCondition.QuestsToStart != null && completedCondition.QuestsToStart.Count > 0)
            {
                //Plugin.Log.LogInfo($"StartMultiChoiceQuest: Sending {completedCondition.QuestsToStart.Count} quest(s) to the server.");
                foreach (string questToStart in completedCondition.QuestsToStart)
                {
                    //Plugin.Log.LogInfo($"StartMultiChoiceQuest: Queued quest ID to start -> {questToStart}");
                }
                /*
                SendQuestIds(completedCondition.QuestsToStart);
                Plugin.Log.LogWarning("Running a method to try and set the quest to completed, expect crashes");
                */

            }
            else
            {
                Plugin.Log.LogError($"StartMultiChoiceQuest: No QuestsToStart defined for condition {conditionId}.");
            }
        }
        /*
        public static void SendQuestIds(List<string> questIds)
        {
            try
            {
                RequestHandler.PutJson("/QE/ConditionCompleted", JsonConvert.SerializeObject(questIds));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Did not send the quest list properly: " + ex);
            }
        }
        */
    }
}