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
using System.Collections;
using EFT.InventoryLogic;
using EFT.HealthSystem;

namespace QuestsExtended.Quests
{
    internal class StatCounterQuestController : AbstractCustomQuestController
    {
        public StatCounterQuestController(QuestExtendedController questExtendedController)
        : base(questExtendedController)
        {
            _player.StatisticsManager.OnUniqueLoot += UniqueItemLooted;
        }
        private static float ArmourDamageholder;
        public void Awake()
        {
            Plugin.Log.LogInfo("StatCounterQuestController is active.");
        }

        //Damage
        public static void EnemyDamageProcessor(DamageInfoStruct damage, float distance)
        {
            //Plugin.Log.LogInfo($"[StatsCounter] Sucessfully triggered EnemyDamageProcessor with sent information. You may now start writing the code. Body Damage was {damage.DidBodyDamage}, armour damage was {damage.DidArmorDamage} and distance was {distance}");
            EQuestConditionCombat conditionstoAdd = EQuestConditionCombat.DamageWithAny;
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
        public static void ArmourDamageProcessor(float damage, DamageInfoStruct damageInfo)
        {
            if (damage > 0f)
            {
                ArmourDamageholder += damage;
                if (ArmourDamageholder > 1f)
                {
                    int floatResult = (int)Math.Round(damage, 0);
                    ArmourDamageholder = 0;
                    EQuestConditionCombat conditionsToAdd = EQuestConditionCombat.DamageToArmour;
                    if (damageInfo.Weapon is ShotgunItemClass) conditionsToAdd |= EQuestConditionCombat.DamageToArmourWithShotguns;
                    var conditions = _questController.GetActiveConditions(conditionsToAdd);
                    foreach (var cond in conditions)
                    {
                        IncrementCondition(cond, floatResult);
                    }
                }
            }
        }
        public static void BodyPartDestroyed (DamageInfoStruct damageInfo, EBodyPart bodyPart)
        {
            EQuestConditionCombat conditionsToAdd = EQuestConditionCombat.DestroyBodyParts;
            if (damageInfo.Weapon is SmgItemClass && (bodyPart.Equals(EBodyPart.LeftLeg) || bodyPart.Equals(EBodyPart.RightLeg)))
            {
                //Plugin.Log.LogInfo("Player destoryed a leg with an SMG. Remove this logger before publishing.");
                conditionsToAdd |= EQuestConditionCombat.DestroyLegsWithSMG;
            }
            var conditions = _questController.GetActiveConditions(conditionsToAdd);
            foreach (var cond in conditions)
            {
                IncrementCondition(cond, 1);
            }
        }
        //Looting
        private static bool LootingXPCooldown;
        public static void UniqueItemLooted()
        {
            if (LootingXPCooldown) return;
            var conditions = _questController.GetActiveConditions(EQuestConditionGen.LootItem);
            foreach (var cond in conditions)
            {
                IncrementCondition(cond, 1);
            }
            StaticManager.BeginCoroutine(LootingCooldown());
        }

        private static IEnumerator LootingCooldown()
        {
            LootingXPCooldown = true;
            yield return new WaitForSeconds(0.05f);
            LootingXPCooldown = false;
        }
        //Searching containers
        private static HashSet<string> SearchedContainers = new HashSet<string>();

        public static void SearchingContainer (Item item)
        {
            if (SearchedContainers.Contains(item.Id))
            {
                // Already searched this container
                return;
            }

            // Mark it as searched
            SearchedContainers.Add(item.Id);

            // Proceed with the rest of your logic
            var conditions = _questController.GetActiveConditions(EQuestConditionGen.SearchContainer);
            foreach (var cond in conditions)
            {
                IncrementCondition(cond, 1);
            }
        }
        //Interacting with power switches
        private static bool SwitchCooldown; //The switch method that BSG uses likes to call itself quite a few times... let's ensure it's only called once.
        public static void PowerSwitchInteractedWith()
        {
            if (SwitchCooldown) return;
            var conditions = _questController.GetActiveConditions(EQuestConditionGen.ActivatePowerSwitch);
            foreach (var cond in conditions)
            {
                IncrementCondition(cond, 1);
            }
            StaticManager.BeginCoroutine(SwitchCooldownTimer());
        }

        private static IEnumerator SwitchCooldownTimer()
        {
            SwitchCooldown = true;
            yield return new WaitForSeconds(5f); //Players will not be hitting multiple power levers in 5 seconds, and as mentioned earlier, the switch method likes to call multiple times.
            SwitchCooldown = false;
        }
    }
}