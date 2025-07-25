﻿using EFT.Quests;
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
using EFT;
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
        public static MainMenuControllerClass mainMenuControllerClass = null;
        public static CompletedSaveData saveData;
        public void Awake()
        {
            conditions.Clear();
            if (_questController != null) conditions = _questController.GetActiveConditions(EQuestConditionGen.CompleteOptionals & EQuestConditionGen.EmptyWithQuestStarter);
            else Plugin.Log.LogWarning("_questController was null on OptionalConditionController.Awake().");
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

        public static void HandleQuestStartingConditionCompletion(Condition condition)
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
                Plugin.Log.LogWarning($"Condition not found, Checking some things. Condition id we're looking for is {condition.id}.");
                if (_questController == null)
                {
                    Plugin.Log.LogInfo($"_questController is null. Likely in the main menu trying to start a new quest. Conditionid we got is {condition.id}. Just to make sure things are loaded, we currently have {Plugin.Quests.Count} quests loaded. Overriding...");
                    foreach (var quest in Plugin.Quests)
                    {
                        if (quest.Value.IsMultipleChoiceStarter)
                        {
                            foreach (var overrideCond in quest.Value.Conditions)
                            {
                                if (overrideCond.ConditionId == condition.id)
                                {
                                    if (CompletedSaveData.CompletedMultipleChoice.Contains(quest.Value.QuestId)) { Plugin.Log.LogInfo("Already did this, returning."); return; }
                                    CompletedSaveData.CompletedMultipleChoice.Add(quest.Value.QuestId);
                                    Plugin.Log.LogInfo("Got the condition, updating quests.");
                                    SendQuestIdsForEditing<List<RawQuestClass>>(overrideCond.QuestsToStart);
                                    saveData.SaveCompletedMultipleChoice();
                                    if (Singleton<GameWorld>.Instance != null) return;
                                    Plugin.Log.LogInfo("Resetting main menu for QE.");
                                    ShowResetMessage = true;
                                    //mainMenuControllerClass.method_5();
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    var conditions2 = _questController.GetActiveConditions(EQuestConditionGen.CompleteOptionals);
                    foreach (var cond3 in conditions2)
                    {
                        if (cond3.Condition.ChildConditions.Contains(condition))
                        {
                            foundCondition = true; correctCond = cond3; break;
                        }
                    }
                    if (!foundCondition)
                    {
                        Plugin.Log.LogInfo("_questController was not null, but we still don't have the condition. Most likely a quest that starts a new quest the QE way. Overriding...");
                        foreach (var quest in Plugin.Quests)
                        {
                            if (quest.Value.IsMultipleChoiceStarter)
                            {
                                foreach (var overrideCond in quest.Value.Conditions)
                                {
                                    if (overrideCond.ConditionId == condition.id)
                                    {
                                        if (CompletedSaveData.CompletedMultipleChoice.Contains(quest.Value.QuestId)) { Plugin.Log.LogInfo("Already did this, returning."); return; }
                                        CompletedSaveData.CompletedMultipleChoice.Add(quest.Value.QuestId);
                                        Plugin.Log.LogInfo("Got the condition, updating quests.");
                                        SendQuestIdsForEditing<List<RawQuestClass>>(overrideCond.QuestsToStart);
                                        saveData.SaveCompletedMultipleChoice();
                                        if (Singleton<GameWorld>.Instance != null) return;
                                        Plugin.Log.LogInfo("Resetting main menu for QE.");
                                        ShowResetMessage = true;
                                        AdvisePlayerOfReset();
                                        //mainMenuControllerClass.method_5();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (!foundCondition)
            {
                Plugin.Log.LogError("Condition not found anywhere, we have an issue");
                return;
            }
            if (CompletedSaveData.CompletedOptionals.Contains(condition.id))
            {
                //Plugin.Log.LogInfo("Condition was already within CompletedOptionals list");
                return;
            }
            if (foundCondition)
            {
                CompletedSaveData.CompletedOptionals.Add(condition.id);
                foreach (var quest in Plugin.Quests)
                {
                    if (quest.Value.IsMultipleChoiceStarter)
                    {
                        foreach (var overrideCond in quest.Value.Conditions)
                        {
                            if (overrideCond.ConditionId == condition.id)
                            {
                                if (overrideCond.QuestsToStart != null && !overrideCond.IsFail)
                                {
                                    Plugin.Log.LogInfo("Probably got a condition that's meant to start multiple quests, and it's acting weird because a custom condition bumped it. Running it throughOCC.");
                                    OptionalConditionController.DirectHandleQuestStartingConditionCompletion(quest.Value.QuestId, overrideCond);
                                }
                            }
                        }
                    }
                }
                Plugin.Log.LogInfo($"We are adding the id {condition.id} to CompletedOptionals and increasing the CompleteOptionals condition by 1.");
                IncrementCondition(correctCond, 1);
                saveData.SaveCompletedOptionals();
            }
            if (!foundCondition)
            {
                var newConditions = conditions = _questController.GetActiveConditions(EQuestConditionGen.EmptyWithQuestStarter);
                foreach (var cond in conditions)
                {
                    if (cond.Condition == condition)
                    {
                        foundCondition = true; correctCond = cond; break;
                    }
                    else
                    {
                        Plugin.Log.LogError("Condition is not found anywhere! This probably shouldn't happen.");
                        return;
                    }
                }
                if (conditions.Count == 0)
                {
                    Plugin.Log.LogWarning("Condition for starting a new quest wasn't found. We might be in the main menu.");
                }
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
                    Plugin.Log.LogInfo($"We are adding the id {correctCond.Quest.Id} to CompletedMultipleChoice and attempting to load the correct quests");
                    //Plugin.Log.LogInfo($"StartMultiChoiceQuest: Sending {completedCondition.QuestsToStart.Count} quest(s) to the server.");
                    //Plugin.Log.LogWarning("Running a method to try and set the quest to completed, expect crashes"); //NOT CRASHING ANYMORE CAUSE IT WORKS!
                    saveData.SaveCompletedMultipleChoice();
                    SendQuestIdsForEditing<List<RawQuestClass>>(completedCondition.QuestsToStart);

                }
                else
                {
                    Plugin.Log.LogError($"StartMultiChoiceQuest: No QuestsToStart defined for condition {correctCond.Condition.id}.");
                }
            }
            else
            {
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
                        Plugin.Log.LogInfo($"Located the completed condition within CustomConditions: {cond2.ConditionId}");
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
                    saveData.SaveCompletedMultipleChoice();
                    if (Singleton<GameWorld>.Instance != null) return;
                    Plugin.Log.LogInfo("Resetting main menu for QE.");
                    ShowResetMessage = true;
                    AdvisePlayerOfReset();
                    //mainMenuControllerClass.method_5();
                }
                else
                {
                    Plugin.Log.LogError($"StartMultiChoiceQuest: No QuestsToStart defined for condition {correctCond.Condition.id}.");
                }
            }
        }

        public static void DirectHandleQuestStartingConditionCompletion(string questId, CustomCondition cond)
        {
            Plugin.Log.LogInfo("If you are here it's because you are direct calling a quest. Oh joy! Let's see how this goes. Let's make sure we're not running a double...");
            if (saveData == null)
            {
                Plugin.Log.LogInfo("QE hasn't loaded yet, delaying the call.");
                StaticManager.BeginCoroutine(DelayedHandleQuestStartingConditionCompletion(questId, cond));
                return;
            }
            if (CompletedSaveData.CompletedMultipleChoice.Contains(questId)) return;
            Plugin.Log.LogInfo("All good! Continuing on");
            CompletedSaveData.CompletedMultipleChoice.Add(questId);
            SendQuestIdsForEditing<List<RawQuestClass>>(cond.QuestsToStart);
            saveData.SaveCompletedMultipleChoice();
            if (Singleton<GameWorld>.Instance != null) return;
            Plugin.Log.LogInfo("Resetting main menu for QE.");
            ShowResetMessage = true;
            AdvisePlayerOfReset();
            //mainMenuControllerClass.method_5();
            return;
        }

        private static IEnumerator DelayedHandleQuestStartingConditionCompletion (string questId, CustomCondition cond)
        {
            while (saveData == null)
            {
                Plugin.Log.LogInfo("Not loaded yet, waiting...");
                yield return new WaitForSeconds(1f);
            }
            if (CompletedSaveData.CompletedMultipleChoice.Contains(questId)) yield break;
            Plugin.Log.LogInfo("All good! Continuing on");
            CompletedSaveData.CompletedMultipleChoice.Add(questId);
            SendQuestIdsForEditing<List<RawQuestClass>>(cond.QuestsToStart);
            saveData.SaveCompletedMultipleChoice();
            if (Singleton<GameWorld>.Instance != null) yield break;
            Plugin.Log.LogInfo("Resetting main menu for QE.");
            ShowResetMessage = true;
            AdvisePlayerOfReset();
            //mainMenuControllerClass.method_5();
            yield break;

        }

        private static void AdvisePlayerOfReset()
        {
            if (AbstractCustomQuestController.ShowResetMessage == true)
            {
                Plugin.Log.LogInfo("Trying to create a reset message");
                AbstractCustomQuestController.ShowResetMessage = false;
                AbstractCustomQuestController.ResetMainMenu = true;
                ErrorScreen errorScreen = new ErrorScreen();
                PreloaderUI preloaderUi = Singleton<PreloaderUI>.Instance;
                string title = "Quests Extended Notice";
                string description = "Quests Extended has detected a new quest that is ready to be loaded. We need to briefly restart the main menu. Thank you for understanding.";
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen(title, description, ErrorScreen.EButtonType.OkButton, 180);
            }
        }

        public static void ResetMainMenuForQE()
        {
            AbstractCustomQuestController.ShowResetMessage = false;
            AbstractCustomQuestController.ResetMainMenu = false;
            QuestExtendedController.isInMainMenu = true;
            mainMenuControllerClass.method_5();
            MenuUI menuUI = MenuUI.Instance;
            QuestExtendedController cont = menuUI.GetComponent<QuestExtendedController>();
            UnityEngine.Object.Destroy(cont);
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

        public static void HandleVanillaConditionChanged(string conditionId, int currentValue)
        {
            /*
            var conditions = _questController.GetActiveConditions(EQuestConditionGen.CompleteOptionals);
            if (conditions.Count == 0) return; //No active quests with CompleteOptionals, no need to run this.
            */
            var activeQuests = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.QuestsData;
            if (activeQuests == null) return; //Shouldn't be possible but since this class keeps breaking, screw it
            foreach (var quest in activeQuests)
            {
                if (quest.Template.conditionsDict_0 == null) continue; //Null guard
                foreach (var cond in quest.Template.conditionsDict_0)
                {
                    if (cond.Value.list_0 == null) continue; //Null guard
                    foreach (var condition in cond.Value.list_0)
                    {
                        if (condition.id == conditionId)
                        {
                            if (currentValue >= condition.value)
                            {
                                //We need to adjust this so that it can handle non-optional conditions!
                                //Plugin.Log.LogInfo("Condition acquired and completed, let's run it through CompleteOptionals!");
                                HandleQuestStartingConditionCompletion(condition);
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