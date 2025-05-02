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
using System.Security.Policy;
namespace QuestsExtended.Quests
{
    internal class OptionalConditionController : AbstractCustomQuestController
    {
        public OptionalConditionController(QuestExtendedController questExtendedController) : base(questExtendedController)
        {

        }
        public static List<string> _activeQuestIds = new List<string>();
        private static bool alreadyLoaded = false;
        public static GClass2098 questClass = null;
        public static LocalQuestControllerClass localQuestControllerClass;

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
                if (CompletedSaveData.CompletedOptionals.Contains(condition.id))
                {
                  //  Plugin.Log.LogInfo("Condition was already within CompletedOptionals list");
                    continue;
                }
                if (cond.Quest.Id != conditional.Id)
                {
                  //  Plugin.Log.LogInfo("Tried to increment a CompleteOptionals task on a quest that doesn't contain the completed optional, skipping.");
                    continue;
                }
                CompletedSaveData.CompletedOptionals.Add(condition.id);
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
                if (CompletedSaveData.CompletedMultipleChoice.Contains(questId)) return;
                CompletedSaveData.CompletedMultipleChoice.Add(questId);
                //Plugin.Log.LogInfo($"StartMultiChoiceQuest: Sending {completedCondition.QuestsToStart.Count} quest(s) to the server.");
                foreach (string questToStart in completedCondition.QuestsToStart)
                {
                    //Plugin.Log.LogInfo($"StartMultiChoiceQuest: Queued quest ID to start -> {questToStart}");
                }
                Plugin.Log.LogWarning("Running a method to try and set the quest to completed, expect crashes");
                var templates = SendQuestIds<List<RawQuestClass>>(completedCondition.QuestsToStart);
                Plugin.Log.LogWarning($"Possibly received the correct data. Exit raid, let's see if this worked...");
                //localQuestControllerClass.Quests.AddTemplates(templates);
            }
            else
            {
                Plugin.Log.LogError($"StartMultiChoiceQuest: No QuestsToStart defined for condition {conditionId}.");
            }
        }

        private class ServerResponse<T>
        {
            public T data { get; set; }
        }
        public static List<RawQuestClass> SendQuestIds<T>(List<string> questIds, T data = default)
        {
            try
            {
                var templates = RequestHandler.PostJson("/QE/ConditionCompleted", JsonConvert.SerializeObject(questIds));
                var result = JsonConvert.DeserializeObject<ServerResponse<List<RawQuestClass>>>(templates, new JsonConverter[] { new GClass1637<ECompareMethod>(true), new GClass1643<GClass1642, Condition, string>() });
                // Now loop through and override the level requirements
                foreach (var quest in result.data)
                {
                    if (quest.AppearStatus == EQuestStatus.Locked)
                    {
                        quest.AppearStatus = EQuestStatus.AvailableForStart;
                    }
                }
                return result.data;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Did not send the quest list properly: " + ex);
                return default;
            }
        }
    }
}