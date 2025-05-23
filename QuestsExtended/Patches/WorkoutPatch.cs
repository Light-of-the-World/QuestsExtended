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
    internal class WorkoutPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutPlayerOwner), nameof(HideoutPlayerOwner.StopWorkout));
        }
        [PatchPostfix]
        private static void Postfix(HideoutPlayerOwner __instance)
        {
            PhysicalQuestController.PlayerDidWorkout();
        }
    }
}
