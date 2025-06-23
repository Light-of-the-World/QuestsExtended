using EFT;
using EFT.Hideout;
using HarmonyLib;
using QuestsExtended.Quests;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuestsExtended.Patches
{
    internal class CollectCraftedItemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutClass), nameof(HideoutClass.GetProducedItems));
        }

        [PatchPostfix]
        private static void Postfix(HideoutClass __instance, GClass2193 producer)
        {
            EAreaType eArea = producer.AreaType;
            if (eArea == EAreaType.WaterCollector || eArea == EAreaType.BitcoinFarm || eArea == EAreaType.BoozeGenerator)
            {
                HideoutQuestController.CollectCyclicItemFromHideout(eArea);
            }
            else if (eArea == EAreaType.ScavCase || eArea == EAreaType.CircleOfCultists)
            {
                HideoutQuestController.CollectScavOrCultist(eArea);
            }
            else HideoutQuestController.CollectItemFromHideout(eArea);
        }
    }
    internal class WorkoutPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.StartQte));
        }
        [PatchPostfix]
        private static void Postfix(ref HideoutPlayerOwner owner)
        {
            if (owner.Player.ProfileId == ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.Id)
            {
                HideoutQuestController.PlayerDidWorkout();
            }
        }
    }
}
