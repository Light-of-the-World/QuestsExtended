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
        private static bool SniperCooldown = false;
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
        public static void EnemyDamageProcessor(DamageInfoStruct damage, float distance, string enemyID)
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
                    if (!SniperCooldown)
                    {
                        SniperCooldown = true;
                        var specialcondition2 = _questController.GetActiveConditions(EQuestConditionCombat.TotalShotDistanceWithSnipers);
                        foreach (var cond in specialcondition2)
                        {
                            if (cond.CustomCondition.EnemyTypes != null)
                            {
                                Player enemy = Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(enemyID);
                                bool test = CheckForCorrectEnemyType(enemy, cond);
                                if (!test) continue;
                            }
                            IncrementCondition(cond, (int)Math.Round(distance, 0));
                        }
                        StaticManager.BeginCoroutine(StartSniperCooldown());
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
                    if (cond.CustomCondition.EnemyTypes != null)
                    {
                        Player enemy = Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(enemyID);
                        bool test = CheckForCorrectEnemyType(enemy, cond);
                        if (!test) continue;
                    }
                    IncrementCondition(cond, (int)Math.Round(damage.DidBodyDamage, 0));
                }
            }
        }
        public static void ArmourDamageProcessor(float damage, DamageInfoStruct damageInfo, Player enemy)
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
                        if (cond.CustomCondition.EnemyTypes != null)
                        {
                            bool test = CheckForCorrectEnemyType(enemy, cond);
                            if (!test) continue;
                        }
                        IncrementCondition(cond, floatResult);
                    }
                }
            }
        }
        public static void BodyPartDestroyed(DamageInfoStruct damageInfo, EBodyPart bodyPart, Player enemy)
        {
            EQuestConditionCombat conditionsToAdd = EQuestConditionCombat.DestroyEnemyBodyParts;
            if (damageInfo.Weapon is SmgItemClass && (bodyPart.Equals(EBodyPart.LeftLeg) || bodyPart.Equals(EBodyPart.RightLeg)))
            {
                //Plugin.Log.LogInfo("Player destoryed a leg with an SMG. Remove this logger before publishing.");
                conditionsToAdd |= EQuestConditionCombat.DestroyLegsWithSMG;
            }
            var conditions = _questController.GetActiveConditions(conditionsToAdd);
            foreach (var cond in conditions)
            {
                if (cond.CustomCondition.EnemyTypes != null)
                {
                    bool test = CheckForCorrectEnemyType(enemy, cond);
                    if (!test) continue;
                }
                IncrementCondition(cond, 1);
            }
        }
        public static void EnemyKillProcessor(DamageInfoStruct damageInfo, string enemyID)
        {
            Player enemy3 = Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(enemyID);
            Plugin.Log.LogInfo("You killed a " + enemy3.Side);
            EQuestConditionCombat conditionsToAdd = EQuestConditionCombat.EmptyC;
            if (PhysicalQuestController.isCrouched)
            {
                //Plugin.Log.LogInfo("Player scored a kill while crouched. Remove this logger before publishing.");
                conditionsToAdd |= EQuestConditionCombat.KillsWhileCrouched;
            }
            else if (PhysicalQuestController.isProne)
            {
                //Plugin.Log.LogInfo("Player scored a kill while prone. Remove this logger before publishing.");
                conditionsToAdd |= EQuestConditionCombat.KillsWhileProne;
            }
            if (PhysicalQuestController._movementContext.IsInMountedState)
            {
                //Plugin.Log.LogInfo("Player scored a kill while mounted. Remove this logger before publishing.");
                conditionsToAdd |= EQuestConditionCombat.KillsWhileMounted;
                //Plugin.Log.LogInfo("You killed him while mounted");
                if (damageInfo.Weapon is MachineGunItemClass)
                {
                    //Plugin.Log.LogInfo("Player scored a kill with an LMG while mounted. Remove this logger before publishing.");
                    conditionsToAdd |= EQuestConditionCombat.MountedKillsWithLMG;
                    //Plugin.Log.LogInfo("You also used an LMG");
                }
            }
            if (PhysicalQuestController.isSilent)
            {
                //Plugin.Log.LogInfo("Player scored a kill while silent. Remove this logger before publishing.");
                conditionsToAdd |= EQuestConditionCombat.KillsWhileSilent;
            }
            if (PhysicalQuestController.isADS)
            {
                //Plugin.Log.LogInfo("Player scored a kill while aiming down sight. Remove this logger before publishing.");
                conditionsToAdd |= EQuestConditionCombat.KillsWhileADS;
            }
            if (!PhysicalQuestController.isADS)
            {
                conditionsToAdd |= EQuestConditionCombat.KillsWithoutADS;
                if (damageInfo.Weapon is RevolverItemClass)
                {
                    //Plugin.Log.LogInfo("Player scored a kill with a revolver while hip firing. Remove this logger before publishing.");
                    conditionsToAdd |= EQuestConditionCombat.RevolverKillsWithoutADS;
                }
            }
            if (PhysicalQuestController.isBlindFiring)
            {
                //Plugin.Log.LogInfo("Player got a kill while blind firing. Remove this logger before publishing.");
                conditionsToAdd |= EQuestConditionCombat.KillsWhileBlindFiring;
            }
            var conditions = _questController.GetActiveConditions(conditionsToAdd);
            foreach (var cond in conditions)
            {
                if (cond.CustomCondition.EnemyTypes != null)
                {
                    Player enemy = Singleton<GameWorld>.Instance.GetEverExistedPlayerByID(enemyID);
                    if (enemy == null) break;
                    bool test = CheckForCorrectEnemyType(enemy, cond);
                    if (!test) continue;
                }
                IncrementCondition(cond, 1);
            }
        }
        //Troubleshooting / malfunctions
        public static void MalfunctionFixed(Weapon weapon)
        {
            EQuestConditionMisc1 conditionsToAdd = EQuestConditionMisc1.FixAnyMalfunction;
            if (weapon is AssaultCarbineItemClass || weapon is AssaultRifleItemClass)
            {
                conditionsToAdd |= EQuestConditionMisc1.FixARMalfunction;
            }
            else if (weapon is MarksmanRifleItemClass)
            {
                conditionsToAdd |= EQuestConditionMisc1.FixDMRMalfunction;
            }
            else if (weapon is MachineGunItemClass)
            {
                conditionsToAdd |= EQuestConditionMisc1.FixLMGMalfunction;
            }
            else if (weapon is PistolItemClass)
            {
                conditionsToAdd |= EQuestConditionMisc1.FixPistolMalfunction;
            }
            else if (weapon is ShotgunItemClass)
            {
                conditionsToAdd |= EQuestConditionMisc1.FixShotgunMalfunction;
            }
            else if (weapon is SmgItemClass)
            {
                conditionsToAdd |= EQuestConditionMisc1.FixSMGMalfunction;
            }
            else if (weapon is SniperRifleItemClass)
            {
                conditionsToAdd |= EQuestConditionMisc1.FixSniperMalfunction;
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
        private static IEnumerator StartSniperCooldown()
        {
            yield return new WaitForSeconds(0.2f);
            SniperCooldown = false;
        }

        private static bool CheckForCorrectEnemyType(Player enemy, ConditionPair cond)
        {
            string faction = enemy.Side.ToString();
            faction.ToLower();
            List<string> types = cond.CustomCondition.EnemyTypes.ToList();
            foreach (var type in types) 
            {
                type.ToLower();
                if (type == faction) return true;
                else if (faction == "savage" && type == "scav") return true;
            }
            return false;
        }
    }
}