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
                    OptionalConditionController.HandleOptionalConditionCompletion(__instance.Condition);
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
            string questId = counter.Conditional.Id;
            string counterId = counter.Id;
            //Plugin.Log.LogInfo($"(POSTFIX)Quest {questId} just changed {counterId}'s value by {valueToAdd}, making it {counter.Value}.");
            if (__state != counter.Value)
            {
                //Plugin.Log.LogWarning($"We got the vanilla condition that just changed: {counterId}. Send it to OCC for processing");
                OptionalConditionController.HandleVanillaChildConditionChanged(counterId, counter.Value);
            }
        }
    }
}
