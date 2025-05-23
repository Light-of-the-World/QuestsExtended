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
using UnityEngine;
using Comfort.Common;
using SPT.Reflection.Utils;
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
        public static float questCheckCooldown = 5f;
        public static List<ConditionPair> conditions = new List<ConditionPair>();

        public void Awake()
        {
            conditions.Clear();
            conditions = _questController.GetActiveConditions(EQuestConditionGen.CompleteOptionals);
            //Plugin.Log.LogInfo($"There are {conditions.Count} condition pairs ready for Optional Tasks");
        }

        public void Update()
        {
            if (isRaidOver) return;
            questCheckCooldown -= Time.deltaTime;
            if (questCheckCooldown < 0)
            {
                questCheckCooldown = 5f;
                //ForceCheckServerValues();
            }
        }

        public static void AddQuestIDToActiveList(string id)
        {
            if (alreadyLoaded) return;
            alreadyLoaded = true;
            _activeQuestIds.Add(id);
          //  Plugin.Log.LogInfo($"Quest id {id} made it to QuestStarter");
        }

        public static void HandleOptionalConditionCompletion(Condition condition)
        {
            bool foundCondition = false;
            ConditionPair correctCond = null;
            foreach (var cond in conditions)
            {
                if (cond.Condition.ChildConditions.Contains(condition))
                {
                    foundCondition = true; correctCond = cond; break;
                }
            }
            if (!foundCondition)
            {
                Plugin.Log.LogWarning("Condition not found, updating list and trying again");
                conditions = _questController.GetActiveConditions(EQuestConditionGen.CompleteOptionals);
                foreach (var cond in conditions)
                {
                    if (cond.Condition.ChildConditions.Contains(condition))
                    {
                        foundCondition = true; correctCond = cond; break;
                    }
                }
            }
            if (!foundCondition)
            {
                Plugin.Log.LogError("Condition is still not found. We have an issue.");
                return;
            }
            if (CompletedSaveData.CompletedOptionals.Contains(condition.id))
            {
                //Plugin.Log.LogInfo("Condition was already within CompletedOptionals list");
                return;
            }
            CompletedSaveData.CompletedOptionals.Add(condition.id);
            //Plugin.Log.LogInfo($"We are adding the id {condition.id} to CompletedOptionals and increasing the CompleteOptionals condition by 1.");
            IncrementCondition(correctCond, 1);

            if (!Plugin.Quests.TryGetValue(correctCond.Quest.Id, out CustomQuest customQuest))
            {
                Plugin.Log.LogWarning($"StartMultiChoiceQuest: Could not find CustomQuest data for quest ID {correctCond.Quest.Id}");
                return;
            }

            CustomCondition completedCondition = default;
            bool conditionFound = false;

            foreach (CustomCondition cond2 in customQuest.Conditions)
            {
                if (cond2.ConditionId == condition.id)
                {
                    completedCondition = cond2;
                    conditionFound = true;
                    //Plugin.Log.LogInfo($"Located the completed condition within CustomConditions: {cond2.ConditionId}");
                    break;
                }
            }

            if (!conditionFound || !customQuest.IsMultipleChoiceStarter)
            {
                return;
            }

            if (completedCondition.QuestsToStart != null && completedCondition.QuestsToStart.Count > 0)
            {
                if (CompletedSaveData.CompletedMultipleChoice.Contains(correctCond.Quest.Id)) return;
                CompletedSaveData.CompletedMultipleChoice.Add(correctCond.Quest.Id);
                //Plugin.Log.LogInfo($"We are adding the id {correctCond.Quest.Id} to CompletedMultipleChoice and attempting to load the correct quests");
                //Plugin.Log.LogInfo($"StartMultiChoiceQuest: Sending {completedCondition.QuestsToStart.Count} quest(s) to the server.");
                foreach (string questToStart in completedCondition.QuestsToStart)
                {
                    //Plugin.Log.LogInfo($"StartMultiChoiceQuest: Queued quest ID to start -> {questToStart}");
                }
                //Plugin.Log.LogWarning("Running a method to try and set the quest to completed, expect crashes"); //NOT CRASHING ANYMORE CAUSE IT WORKS!
                SendQuestIdsForEditing<List<RawQuestClass>>(completedCondition.QuestsToStart);

            }
            else
            {
                Plugin.Log.LogError($"StartMultiChoiceQuest: No QuestsToStart defined for condition {correctCond.Condition.id}.");
            }
        }   

        private class ServerResponse<T>
        {
            public T data { get; set; }
        }
        public static void SendQuestIdsForEditing<T>(List<string> questIds, T data = default)
        {
            try
            {
                var templates = RequestHandler.PostJson("/QE/StartMultiChoice", JsonConvert.SerializeObject(questIds));
                JsonConvert.DeserializeObject<ServerResponse<List<RawQuestClass>>>(templates, new JsonConverter[] { new GClass1637<ECompareMethod>(true), new GClass1643<GClass1642, Condition, string>() });
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("Did not send the quest list properly: " + ex);
            }
        }

        public static void HandleVanillaChildConditionChanged(string conditionId, int currentValue)
        {
            var conditions = _questController.GetActiveConditions(EQuestConditionGen.CompleteOptionals);
            if (conditions.Count == 0) return; //No active quests with CompleteOptionals, no need to run this.
            var activeQuests = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.QuestsData;
            foreach (var quest in activeQuests)
            {
                foreach (var cond in quest.Template.conditionsDict_0)
                {
                    foreach (var condition in cond.Value.list_0)
                    {
                        if (condition.id == conditionId)
                        {
                            if (currentValue >= condition.value)
                            {
                                //Plugin.Log.LogInfo("Condition acquired and completed, let's run it through CompleteOptionals!");
                                HandleOptionalConditionCompletion(condition);
                                return;
                            }
                            else
                            {
                                //Plugin.Log.LogInfo("Condition acquired, but is not completed.");
                            }
                        }
                    }
                }
            }
        }
    }
}