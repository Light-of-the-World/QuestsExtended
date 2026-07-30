using EFT;
using EFT.HealthSystem;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.InventoryLogic;

namespace QuestsExtended.Patches
{
    public class HideLockedTradersPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() //THIS CODE WAS GRABBED FROM VAGABOND AND IS NOT MY OWN.
        {
            return AccessTools.Constructor(
                typeof(TraderScreensGroup.GClass3888),
                new[]
                {
                typeof(TraderClass),
                typeof(IEnumerable<TraderClass>),
                typeof(Profile),
                typeof(InventoryController),
                typeof(IHealthController),
                typeof(AbstractQuestControllerClass),
                typeof(AbstractAchievementControllerClass),
                typeof(ISession)
                });
        }

        [PatchPrefix] //Probably need to edit this?
        public static void Prefix(ref TraderClass trader, ref IEnumerable<TraderClass> tradersList)
        {
            if (tradersList == null)
            {
                return;
            }

            var filtered = tradersList.Where(x => x != null && x.Info != null && x.Info.Available).ToArray();

            if (filtered.Length == 0)
            {
                return;
            }

            tradersList = filtered;

            if (trader == null || trader.Info == null || !trader.Info.Available || !filtered.Contains(trader))
            {
                trader = filtered[0];
            }
        }
    }
}
