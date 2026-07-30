using Comfort.Common;
using EFT;
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
            return AccessTools.Method(typeof(Switch), nameof(Switch.method_5));
        }
        [PatchPostfix]
        private static void Postfix(Switch __instance, ref Turnable.EState state)
        {
            //Plugin.Log.LogInfo($"[SwitchPatch] Switch.method_7 called, logging some relavent information: state to string: {state.ToString()}. Switch instance's TypeKey: {__instance.TypeKey}.");
            if (__instance.Lamps != null) return;
            QuestExtendedController _questController = null;
            foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
            {
                if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
                {
                    _questController = QEC;
                    break;
                }
            }
            if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: SwitchPatch)"); return; }
            _questController._statCounterController.PowerSwitchInteractedWith();
        }
    }
}