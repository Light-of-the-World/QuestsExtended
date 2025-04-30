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
        /*
        public static List<string> ThingWeSend { get; set; } = [];

        public static T ServerRoute<T>(string url, T data = default)
        {
            string json = JsonConvert.SerializeObject(data);
            string req = RequestHandler.PostJson(url, json);
            return JsonConvert.DeserializeObject<T>(req);
        }
        */
    }
}
