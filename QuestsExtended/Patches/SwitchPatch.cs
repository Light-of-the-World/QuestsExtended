using EFT.Interactive;
using HarmonyLib;
using QuestsExtended.Quests;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuestsExtended.Patches
{
    internal class SwitchPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Switch), nameof(Switch.method_7));
        }
        [PatchPostfix]
        private static void Postfix(Switch __instance, ref Turnable.EState state)
        {
            //Plugin.Log.LogInfo($"[SwitchPatch] Switch.method_7 called, logging some relavent information: state to string: {state.ToString()}. Switch instance's TypeKey: {__instance.TypeKey}.");
            if (__instance.ExtractionZoneTip != "EXFIL_BUNKER_POWER")
            {
                StatCounterQuestController.PowerSwitchInteractedWith();
            }
        }
    }
}