using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Quests;
using HarmonyLib;
using JetBrains.Annotations;
using QuestsExtended.Models;
using UnityEngine;
using EFT.Counters;

namespace QuestsExtended.Quests
{
    internal class StatCounterQuestController : AbstractCustomQuestController
    {
        public StatCounterQuestController(QuestExtendedController questExtendedController)
        : base(questExtendedController)
        {

        }
        private static float ArmourDamageholder;
        public void Awake()
        {
            Plugin.Log.LogInfo("StatCounterQuestController is active.");
        }
        public static void EnemyDamageProcessor(DamageInfoStruct damage, float distance)
        {
            //Plugin.Log.LogInfo($"[StatsCounter] Sucessfully triggered EnemyDamageProcessor with sent information. You may now start writing the code. Body Damage was {damage.DidBodyDamage}, armour damage was {damage.DidArmorDamage} and distance was {distance}");
            EQuestConditionCombat conditionstoAdd = EQuestConditionCombat.DamageWithAny;
            if (damage.DidArmorDamage > 0f)
            { 
                ArmourDamageholder += damage.DidArmorDamage;
                if (ArmourDamageholder > 1f)
                {
                    int floatResult = (int)Math.Round(damage.DidArmorDamage, 0);
                    ArmourDamageholder = 0;
                    var specialcondition1 = _questController.GetActiveConditions(EQuestConditionCombat.DamageToArmour);
                    foreach (var cond in specialcondition1)
                    {
                        //Plugin.Log.LogWarning($"Incrementing condition: {cond} by {distance}");
                        IncrementCondition(cond, floatResult);
                    }
                }
            }
            if (damage.Weapon != null)
            {
                if (damage.Weapon is PistolItemClass)
                {
                    conditionstoAdd |= EQuestConditionCombat.DamageWithPistols;
                }
                else if (damage.Weapon is SmgItemClass)
                {
                    conditionstoAdd |= EQuestConditionCombat.DamageWithSMG;
                }
                else if (damage.Weapon is ShotgunItemClass)
                {
                    conditionstoAdd |= EQuestConditionCombat.DamageWithShotguns;
                }
                else if (damage.Weapon is AssaultRifleItemClass || damage.Weapon is AssaultCarbineItemClass)
                {
                    conditionstoAdd |= EQuestConditionCombat.DamageWithAR;
                }
                else if (damage.Weapon is GrenadeLauncherItemClass)
                {
                    conditionstoAdd |= EQuestConditionCombat.DamageWithGL;
                }
                else if (damage.Weapon is MachineGunItemClass)
                {
                    conditionstoAdd |= EQuestConditionCombat.DamageWithLMG;
                }
                else if (damage.Weapon is MarksmanRifleItemClass)
                {
                    conditionstoAdd |= EQuestConditionCombat.DamageWithDMR;
                }
                else if (damage.Weapon is SniperRifleItemClass)
                {
                    conditionstoAdd |= EQuestConditionCombat.DamageWithSnipers;
                    var specialcondition2 = _questController.GetActiveConditions(EQuestConditionCombat.TotalShotDistanceWithSnipers);
                    foreach (var cond in specialcondition2)
                    {
                        IncrementCondition(cond, (int)Math.Round(distance, 0));
                    }
                }
                else if (damage.Weapon is ThrowWeapItemClass)
                {
                    if (damage.DelayedDamage)
                    {
                        //nothing lmao
                    }
                    else
                    {
                        conditionstoAdd |= EQuestConditionCombat.DamageWithThrowables;
                    }
                }
                var conditions = _questController.GetActiveConditions(conditionstoAdd);
                foreach (var cond in conditions)
                {
                    IncrementCondition(cond, (int)Math.Round(damage.DidBodyDamage, 0));
                }
            }
        }
    }
}
