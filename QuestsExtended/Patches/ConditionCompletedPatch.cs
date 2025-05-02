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

namespace QuestsExtended.Patches
{
    internal class ConditionCompletedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass3719<IConditional>), nameof(GClass3719<IConditional>.SetConditionCurrentValue));
        }
        [PatchPostfix]
        private static void Postfix(GClass3719<IConditional> __instance, ref IConditional conditional, ref EQuestStatus status, ref Condition condition)
        {
            if (__instance.gclass3873_0.dictionary_0.TryGetValue(condition.id, out var value))
            {
                Plugin.Log.LogInfo($"Found the condition we called. It's value might be {value.Value}, or maybe {value.int_0}, or {value.string_0} perhaps? It's the first and the second, wonderful!");
                //THIS WORKS LFGGGGGGGGGGGG
                foreach (var item in value.Conditional.Conditions)
                {
                    foreach (var item2 in item.Value.list_0)
                    {
                        foreach(var ohGoshTheNestings in item2.ChildConditions)
                        {
                            if (ohGoshTheNestings.ParentId == condition.ParentId && ohGoshTheNestings.id == condition.id)
                            {
                                //Plugin.Log.LogInfo($"Called a child condition. It's current value should be {value.Value}, and the max value should be {ohGoshTheNestings.value}. If these values equal (or the left is higher than the right, since it doesn't stop counting), let's increment optional conditions.");
                                if (value.Value >= ohGoshTheNestings.value)
                                {
                                    OptionalConditionController.HandleOptionalConditionCompletion(__instance, conditional, condition);
                                }
                            }
                            /*
                            else
                            {
                                Plugin.Log.LogInfo($"Called a child condition but something went wrong. isNecessary is set to {ohGoshTheNestings._isNecessary}. Parent id searched for was {ohGoshTheNestings.ParentId} It's current value should be {value.Value}, and the max value should be {ohGoshTheNestings.value}. ");
                            }
                            */
                        }
                    }
                }
            }
        }
    }
    /*
    internal class GClass3873Getter : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass3873), nameof(GClass3873.method_1));
        }
        [PatchPostfix]
        private static void Postfix(GClass3873 __instance)
        {
            if (OptionalConditionController.questClass == null)
            {
                Plugin.Log.LogInfo("Got GClass3873 from method1");
                OptionalConditionController.questClass = __instance;
            }
        }
    }
    */
    internal class GClass2098Getter : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass2098), nameof(GClass2098.UpdateProfile));
        }
        [PatchPostfix]
        private static void Postfix(GClass2098 __instance)
        {
            if (OptionalConditionController.questClass != __instance)
            {
                Plugin.Log.LogInfo("Got GClass2098 from UpdateProfile");
                OptionalConditionController.questClass = __instance;
            }
        }
    }
    internal class GClass2098Getter2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass2098), nameof(GClass2098.Bind));
        }
        [PatchPostfix]
        private static void Postfix(GClass2098 __instance)
        {
            if (OptionalConditionController.questClass != __instance)
            {
                Plugin.Log.LogInfo("Got GClass2098 from Bind");
                OptionalConditionController.questClass = __instance;
            }
        }
    }

    internal class GClass2098Getter3 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass2098), nameof(GClass2098.Class1480.method_0));
        }
        [PatchPostfix]
        private static void Postfix(GClass2098 __instance)
        {
            if (OptionalConditionController.questClass != __instance)
            {
                Plugin.Log.LogInfo("Got GClass2098 from Class1480.method_0");
                OptionalConditionController.questClass = __instance;
            }
        }
    }
    internal class GClass2098Getter4 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass2098), nameof(GClass2098.Class1480.method_2));
        }
        [PatchPostfix]
        private static void Postfix(GClass2098 __instance)
        {
            if (OptionalConditionController.questClass != __instance)
            {
                Plugin.Log.LogInfo("Got GClass2098 from Class1480.method_2");
                OptionalConditionController.questClass = __instance;
            }
        }
    }
    internal class LocalQuestControllerClassGetter : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LocalQuestControllerClass), nameof(LocalQuestControllerClass.Init));
        }
        [PatchPostfix]
        private static void Postfix(LocalQuestControllerClass __instance)
        {
            if (OptionalConditionController.localQuestControllerClass != __instance)
            {
                Plugin.Log.LogInfo("Got LocalQuestControllerClass from Init");
                OptionalConditionController.localQuestControllerClass = __instance;
            }
        }
    }
}
