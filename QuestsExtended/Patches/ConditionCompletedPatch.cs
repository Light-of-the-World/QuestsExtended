using EFT.Quests;
using EFT;
using HarmonyLib;
using QuestsExtended.Quests;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.UI;
using System.Net.Http;
using Newtonsoft.Json;
using SPT.Common.Http;
using System.Collections;
using UnityEngine;
using static LocationScene;
using static BackendDummyClass;
using System.Diagnostics;
using static EFT.UI.InteractionButtonsContainer;
using static LocalQuestControllerClass;
using QuestsExtended.Models;
using QuestsExtended.SaveLoadRelatedClasses;
using Comfort.Common;

namespace QuestsExtended.Patches
{
    internal class CustomConditionChecker : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ConditionProgressChecker), nameof(ConditionProgressChecker.CallConditionChanged));
        }
        [PatchPostfix]
        private static void Postfix(ConditionProgressChecker  __instance)
        {
            //Plugin.Log.LogInfo($"Let's just log some things. Condition id being called is {__instance.Condition.id}. The current value is {__instance.CurrentValue}. The max value might be {__instance.Condition.value}.");
            if (__instance.Condition.ParentId != null)
            {
                //Plugin.Log.LogInfo($"This is a child condition. The parent id is {__instance.Condition.ParentId}.");
                if (__instance.CurrentValue >= __instance.Condition.value)
                {
                    //Plugin.Log.LogInfo("This child condition is completed... let's try a new HandleOptionalConditionCompletion.");
                    OptionalConditionController.HandleQuestStartingConditionCompletion(__instance.Condition);
                }

            }
        }
    }
    internal class VanillaConditionChecker : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ConditionCounterManager), nameof(ConditionCounterManager.smethod_0));
        }
        [PatchPrefix]
        private static void Prefix(ConditionCounterManager __instance, int valueToAdd, TaskConditionCounterClass counter, out int __state)
        {
            string questId = counter.Conditional.Id;
            string counterId = counter.Id;
            //Plugin.Log.LogInfo($"(PREFIX)Quest {questId} is about to change {counterId}'s value by {valueToAdd}, it was {counter.Value}.");
            __state = counter.Value;
        }
        [PatchPostfix]
        private static void Postfix(ConditionCounterManager __instance, ref int valueToAdd, ref TaskConditionCounterClass counter, int __state)
        {
            if (counter == null) return;
            string questId = counter.Conditional.Id;
            if (Plugin.BannedQuestIds.Contains(questId)) return;
            string counterId = counter.Id;
            if (Plugin.BannedConditionIds.Contains(counterId)) return;
            //Plugin.Log.LogInfo($"(POSTFIX)Quest {questId} just changed {counterId}'s value by {valueToAdd}, making it {counter.Value}.");
            if (__state != counter.Value)
            {
                Plugin.Log.LogWarning($"We got the vanilla condition that just changed: {counterId}. Send it to OCC for processing");
                OptionalConditionController.HandleVanillaConditionChanged(counterId, counter.Value);
            }
        }
    }

    internal class BSGWHYISYOURCODELIKETHIS : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(QuestClass), nameof(QuestClass.CheckForStatusChange), [typeof(EQuestStatus), typeof(bool), typeof(bool), typeof(bool), typeof(Action<QuestClass>), typeof(bool)]);
        }
        [PatchPostfix]
        private static void Postfix (QuestClass __instance, EQuestStatus status)
        {
            //Plugin.Log.LogInfo($"CFSC ran on quest {__instance.Id}. The status in the method is {status}, while the __instance status shows as {__instance.QuestStatus}");
            //This seems to work. We can try using this,
            if (status == EQuestStatus.AvailableForFinish && __instance.QuestStatus == status)
            {
                //Plugin.Log.LogInfo("Did a quest just get completed?");
                foreach (var condDict in __instance.Conditions)
                {
                    foreach (var cond in condDict.Value)
                    {
                        if (CompletedSaveData.CompletedMultipleChoice.Contains(__instance.Id)) continue;
                        foreach (var quest in Plugin.Quests)
                        {
                            if (quest.Value.IsMultipleChoiceStarter)
                            {
                                foreach (var overrideCond in quest.Value.Conditions)
                                {
                                    if (overrideCond.ConditionId == cond.id)
                                    {
                                        if (overrideCond.QuestsToStart != null && !overrideCond.IsFail)
                                        {
                                            Plugin.Log.LogInfo("Probably got a condition that's meant to start multiple quests, running through the OCC.");
                                            OptionalConditionController.DirectHandleQuestStartingConditionCompletion(quest.Value.QuestId, overrideCond);
                                            return;
                                        } 
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (status == EQuestStatus.Started && __instance.QuestStatus == status && __instance.CompletedConditions != null)
            {
                Plugin.Log.LogInfo("This quest is not completed, but has a comleted condition. Let's check those conditions to see if they are Quest Starters.");
                foreach (var condDict in __instance.Conditions)
                {
                    foreach (var cond in condDict.Value)
                    {
                        if (!__instance.CompletedConditions.Contains(cond.id)) continue;
                        if (CompletedSaveData.CompletedMultipleChoice.Contains(__instance.Id)) continue;
                        foreach (var quest in Plugin.Quests)
                        {
                            if (quest.Value.IsMultipleChoiceStarter)
                            {
                                foreach (var overrideCond in quest.Value.Conditions)
                                {
                                    if (overrideCond.ConditionId == cond.id)
                                    {
                                        if (overrideCond.QuestsToStart != null && !overrideCond.IsFail)
                                        {
                                            Plugin.Log.LogInfo("Probably got a condition that's meant to start multiple quests, running through the OCC.");
                                            OptionalConditionController.DirectHandleQuestStartingConditionCompletion(quest.Value.QuestId, overrideCond);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if ((status == EQuestStatus.Fail || status == EQuestStatus.MarkedAsFailed) && __instance.QuestStatus == status)
            {
                Plugin.Log.LogInfo($"{status.ToString()} is status. {__instance.Id} is the quest id. Did a quest just fail?");
                foreach (var condDict in __instance.Conditions)
                {
                    foreach (var cond in condDict.Value)
                    {
                        if (CompletedSaveData.CompletedMultipleChoice.Contains(__instance.Id)) continue;
                        foreach (var quest in Plugin.Quests)
                        {
                            if (quest.Value.IsMultipleChoiceStarter)
                            {
                                foreach (var overrideCond in quest.Value.Conditions)
                                {
                                    if (overrideCond.ConditionId == cond.id && overrideCond.QuestsToStart != null && overrideCond.IsFail)
                                    {
                                        Plugin.Log.LogInfo("Ooooo, a fail condition that starts a quest! How exciting. Running through the OCC.");
                                        OptionalConditionController.DirectHandleQuestStartingConditionCompletion(quest.Value.QuestId, overrideCond);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    internal class BlockMessagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(QuestView), nameof(QuestView.ShowQuestMessage));
        }
        [PatchPrefix]
        private static bool Prefix(QuestView __instance, ref RawQuestClass questTemplate)
        {
            foreach (var quest in Plugin.Quests.Values)
            {
                if (questTemplate.Id == quest.QuestId)
                {
                    Plugin.Log.LogInfo($"Found quest {quest.QuestId} in BlockMessagePatch.");
                    QuestClass questClass = (QuestClass)AccessTools.Field(__instance.GetType(), "_quest").GetValue(__instance);
                    Plugin.Log.LogInfo($"Quest's status during BlockMessagePatch was {questClass.QuestStatus}");
                    //Let's try to get only the "new quest" messages to be blocked, and allow the "complete quest" messages to still show.
                    if (quest.BlockQuestMessages==true)
                    {
                        Plugin.Log.LogInfo($"Skipping quest {quest.QuestId}'s messages.");
                        return false;
                    }
                    else break;
                }
            }
            return true;
        }
    }
}
