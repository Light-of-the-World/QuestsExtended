﻿using System.Collections;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using QuestsExtended.Quests;
using QuestsExtended.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace QuestsExtended.Patches;

internal class EnemyDamagePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationStatisticsCollectorAbstractClass), nameof(LocationStatisticsCollectorAbstractClass.OnEnemyDamage));
    }
    [PatchPostfix]
    private static void Postfix(LocationStatisticsCollectorAbstractClass __instance, ref DamageInfoStruct damage, ref float distance)
    {
        //Plugin.Log.LogInfo($"[StatsPatch] OnEnemyDamage called. Sending to StatCounterQuestController for processing.");
        StatCounterQuestController.EnemyDamageProcessor(damage, distance);
        //Do not forget to remove this log before publication!
    }
}

internal class EnemyKillPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationStatisticsCollectorAbstractClass), nameof(LocationStatisticsCollectorAbstractClass.OnEnemyKill));
    }

    [PatchPostfix]
    private static void Postfix(LocationStatisticsCollectorAbstractClass __instance, ref DamageInfoStruct damage)
    {
        //Plugin.Log.LogInfo($"[StatsPatch] OnEnemyKill called. Sending to StatCounterQuestController for processing.");
        StatCounterQuestController.EnemyKillProcessor(damage);
        //Do not forget to remove this log before publication!
    }
}

internal class SearchContainerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationStatisticsCollectorAbstractClass), nameof(LocationStatisticsCollectorAbstractClass.OnInteractWithLootContainer));
    }
    [PatchPostfix]
    private static void Postfix(LocationStatisticsCollectorAbstractClass __instance, ref Item item)
    {
        //Plugin.Log.LogInfo($"[StatsPatch] OnEnemyDamage called. Sending to StatCounterQuestController for processing.");
        StatCounterQuestController.SearchingContainer(item);
        //Do not forget to remove this log before publication!
    }
}

internal class ArmourDurabilityPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ArmorComponent), nameof(ArmorComponent.ApplyDamage));
    }
    [PatchPostfix]
    private static void Postfix(ref float __result, ref DamageInfoStruct damageInfo)
    {
        /*
        Plugin.Log.LogInfo($"[StatsPatch] ArmorComponent.ApplyDamage was called in general. Logging some various stats as a start. The durability dealt to the armour was {__result}, let's see who caused it...");
        if (damageInfo.Player.IsAI) Plugin.Log.LogInfo("This call was caused by an AI");
        else if (!damageInfo.Player.IsAI) Plugin.Log.LogInfo("This call was NOT caused by an AI. Presumabely caused by the player?");
        else Plugin.Log.LogInfo("damageInfo.Player.IsAI came back as neither true nor false. That's concerning...");
        */
        if (!damageInfo.Player.IsAI) { StatCounterQuestController.ArmourDamageProcessor(__result, damageInfo); }
        //Do not forget to remove this log before publication!
    }
}

internal class EnterBlindFirePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.ToggleBlindFire));
    }
    [PatchPostfix]
    private static void Postfix(Player __instance, ref float blindFireValue)
    {
        if (!__instance.IsAI && blindFireValue !=0)
        {
            //Plugin.Log.LogInfo($"[StatsPatch] Player is blind firing.");
            PhysicalQuestController.isBlindFiring = true;
        }
    }
}

internal class ExitBlindFirePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.StopBlindFire));
    }
    [PatchPostfix]
    private static void Postfix(Player __instance)
    {
        if (!__instance.IsAI)
        {
            //Plugin.Log.LogInfo($"[StatsPatch] Player is no longer blind firing.");
            PhysicalQuestController.isBlindFiring = false;
        }
    }
}

internal class DestroyLimbsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ActiveHealthController), nameof(ActiveHealthController.ApplyDamage));
    }
    [PatchPrefix]
    private static void Prefix(ActiveHealthController __instance, DamageInfoStruct damageInfo, EBodyPart bodyPart)
    {
        if (__instance == null || damageInfo.Weapon == null) return;
        if (!damageInfo.Player.IsAI)
        {
            if (__instance.Dictionary_0 == null || __instance.dictionary_0.Count <= 0) return;
            GClass2814<ActiveHealthController.GClass2813>.BodyPartState bodyPartState = __instance.Dictionary_0[bodyPart];
            float health = bodyPartState.Health.Current;
            health -= damageInfo.Damage;
            if (!bodyPartState.IsDestroyed && health <= 0)
            {
                StatCounterQuestController.BodyPartDestroyed(damageInfo, bodyPart);
            }
        }
    }
}

internal class FixMalfunctionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.PlayerOwnerInventoryController), nameof(Player.PlayerOwnerInventoryController.CallMalfunctionRepaired));
    }
    [PatchPostfix]
    private static void Postfix(Player.PlayerOwnerInventoryController __instance, ref Weapon weapon)
    {
        if (__instance.Profile.Id == ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.Id)
        {
            StatCounterQuestController.MalfunctionFixed(weapon);
        }
        else Plugin.Log.LogInfo($"Malfunction was fixed, but the id of the instance was {__instance.Profile.Id}");
    }
}