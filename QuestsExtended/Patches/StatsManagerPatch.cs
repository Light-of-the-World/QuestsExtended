using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.NextObservedPlayer;
using HarmonyLib;
using QuestsExtended.Quests;
using QuestsExtended.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using SPTarkov.Server.Core.Models.Spt.Bots;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace QuestsExtended.Patches;

internal class EnemyDamagePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LocationStatisticsCollectorAbstractClass), nameof(LocationStatisticsCollectorAbstractClass.OnEnemyDamage));
    }
    [PatchPostfix]
    private static void Postfix(LocationStatisticsCollectorAbstractClass __instance, ref DamageInfoStruct damage, ref float distance, ref string playerProfileId)
    {
        if (AbstractCustomQuestController.isScavRaid) return;
        //Plugin.Log.LogInfo($"[StatsPatch] OnEnemyDamage called. Sending to StatCounterQuestController for processing.");
        QuestExtendedController _questController = null;
        if (__instance.Player_0.Profile.ProfileId != Plugin.PlayerProfileID) return;
        foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
        {
            if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
            {
                _questController = QEC;
                break;
            }
        }
        if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: EnemyDamagePatch)"); return; }
        _questController._statCounterController.EnemyDamageProcessor(damage, distance, playerProfileId);
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
    private static void Postfix(LocationStatisticsCollectorAbstractClass __instance, ref DamageInfoStruct damage, ref string playerProfileId)
    {
        if (AbstractCustomQuestController.isScavRaid) return;
        //Plugin.Log.LogInfo($"[StatsPatch] OnEnemyKill called. Sending to StatCounterQuestController for processing.");
        QuestExtendedController _questController = null;
        foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
        {
            if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
            {
                _questController = QEC;
                break;
            }
        }
        if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: EnemyKillPatch)"); return; }
        _questController._statCounterController.EnemyKillProcessor(__instance.Player_0, damage, playerProfileId);
        //Do not forget to remove this log before publication!
    }
}

internal class KeyUsedOnDoorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(WorldInteractiveObject), nameof(WorldInteractiveObject.UnlockOperation));
    }
    [PatchPostfix]
    private static void Postfix(ref KeyComponent key, ref Player player)
    {
        if (AbstractCustomQuestController.isScavRaid) return;
        //Plugin.Log.LogInfo("Player used a key");
        //Plugin.Log.LogInfo($"Do either of these look correct: {key.Template.KeyId}, {key.Item.Id}");
        if (player.IsAI) return;
        QuestExtendedController _questController = null;
                foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
                {
                    if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
                    {
                        _questController = QEC;
                        break;
                    }
                }
                if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: KeyUsedOnDoorPatch)"); return; }
        _questController._statCounterController.PlayerUsedKeyToUnlockDoor(key.Template.KeyId);
    }
}
internal class PedometerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PedometerClass), nameof(PedometerClass.GetDistanceFromMark));
    }
    [PatchPostfix]
    private static void Postfix (PedometerClass __instance, ref float __result, ref EPlayerState state)
    {
        if (AbstractCustomQuestController.isScavRaid) return;
        //if (__instance != PhysicalQuestController._pedometer) return; This did not work
        if (__instance.Player_0.IsAI) return;
        if (__instance.Player_0.Profile.ProfileId != Plugin.PlayerProfileID) return;
        //if (__instance.player_0.IsAI) return;
        //Plugin.Log.LogInfo(__result + ", " + state.ToString());
        foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
        {
            if (__instance.Player_0 == QEC._player)
            {
                QEC._physicalController.ProcessMovement(__result, state);
            }
        }
        //This seems good.
    }
}
internal class KeyCardUsedOnDoorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(KeycardDoor), nameof(KeycardDoor.UnlockOperation));
    }
    [PatchPostfix]
    private static void Postfix(ref KeyComponent key, ref Player player)
    {
        if (AbstractCustomQuestController.isScavRaid) return;
        QuestExtendedController _questController = null;
        foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
        {
            if (QEC.LocalPlayerID == player.Profile.ProfileId)
            {
                _questController = QEC;
                break;
            }
        }
        if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: KeyCardUsedOnDoorPatch)"); return; }
        _questController._statCounterController.PlayerUsedKeyToUnlockDoor(key.Item.Id);
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
        if (AbstractCustomQuestController.isScavRaid) return;
        QuestExtendedController _questController = null;
        foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
        {
            if (QEC.LocalPlayerID == __instance.Player_0.Profile.ProfileId)
            {
                _questController = QEC;
                break;
            }
        }
        if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: SearchContainerPatch)"); return; }
        _questController._statCounterController.SearchingContainer(item);
        //Do not forget to remove this log before publication!
    }
}

internal class HoldMostRecentlyDamagedPlayer : ModulePatch
{
    public static Player MostRecentPlayer;
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.ProceedDamageThroughArmor));
    }
    [PatchPrefix]
    private static void Prefix(Player __instance)
    {
        MostRecentPlayer = __instance;
    }
}

internal class ArmourDurabilityPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ArmorComponent), nameof(ArmorComponent.ApplyDamage));
    }
    [PatchPostfix]
    private static void Postfix(ArmorComponent __instance, ref float __result, ref DamageInfoStruct damageInfo, ref SkillManager.SkillBuffClass heavyVestsDamageReduction)
    {
        if (AbstractCustomQuestController.isScavRaid) return;
        /*
        Plugin.Log.LogInfo($"[StatsPatch] ArmorComponent.ApplyDamage was called in general. Logging some various stats as a start. The durability dealt to the armour was {__result}, let's see who caused it...");
        if (damageInfo.Player.IsAI) Plugin.Log.LogInfo("This call was caused by an AI");
        else if (!damageInfo.Player.IsAI) Plugin.Log.LogInfo("This call was NOT caused by an AI. Presumabely caused by the player?");
        else Plugin.Log.LogInfo("damageInfo.Player.IsAI came back as neither true nor false. That's concerning...");
        */
        if (HoldMostRecentlyDamagedPlayer.MostRecentPlayer == null) return;
        if (!damageInfo.Player.IsAI && damageInfo.Player.iPlayer.Profile.ProfileId == Plugin.PlayerProfileID)
        {
            QuestExtendedController _questController = null;
            foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
            {
                if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
                {
                    _questController = QEC;
                    break;
                }
            }
            if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: ArmourDurabilityPatch)"); return; }
            _questController._statCounterController.ArmourDamageProcessor(__result, damageInfo, HoldMostRecentlyDamagedPlayer.MostRecentPlayer); 
        }
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
        if (AbstractCustomQuestController.isScavRaid) return;
        if (__instance.Profile.ProfileId != Plugin.PlayerProfileID) return;
        if (!__instance.IsAI && blindFireValue !=0)
        {
            foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
            {
                if (__instance == QEC._player)
                {
                    QEC._physicalController.isBlindFiring = true;
                }
            }
        //Plugin.Log.LogInfo($"[StatsPatch] Player is blind firing.");
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
        if (AbstractCustomQuestController.isScavRaid) return;
        if (__instance.Profile.ProfileId != Plugin.PlayerProfileID) return;
        if (!__instance.IsAI)
        {
            foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
            {
                if (__instance == QEC._player)
                {
                    QEC._physicalController.isBlindFiring = false;
                }
            }
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
        if (AbstractCustomQuestController.isScavRaid) return;
        //Plugin.Log.LogInfo("Debugging DestroyLimbsPatch");
        if (damageInfo.Player == null) {/*Plugin.Log.LogInfo("No player from the damagesource, likely fall or environmental damage. Returning");*/ return; }
        if (damageInfo.Player.IsAI) { /*Plugin.Log.LogInfo("Entity is a bot, returning");*/ return; }
        if (damageInfo.Player.iPlayer == null) { Plugin.Log.LogInfo("iPlayer is null, returning"); return; }
        if (damageInfo.Player.iPlayer.Profile == null) { Plugin.Log.LogInfo("Profile is null, returning"); return; }
        if (damageInfo.Player.iPlayer.Profile.ProfileId != Plugin.PlayerProfileID) { Plugin.Log.LogInfo("Incorrect profile ID, returning"); return; }
        if (__instance == null) { Plugin.Log.LogInfo("__instance was null, returning"); return; }
        if (damageInfo.Weapon == null) { Plugin.Log.LogInfo("Weapon was null returning"); return; }
        if (__instance.Dictionary_0 == null || __instance.Dictionary_0.Count <= 0) { Plugin.Log.LogInfo("Dictionary didn't exist, returning"); return; }
        GClass3009<ActiveHealthController.GClass3008>.BodyPartState bodyPartState = __instance.Dictionary_0[bodyPart];
        if (bodyPartState != null)
        {
            float health = bodyPartState.Health.Current;
            health -= damageInfo.Damage;
            if (!bodyPartState.IsDestroyed && health <= 0)
            {
                QuestExtendedController _questController = null;
                foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
                {
                    if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
                    {
                        _questController = QEC;
                        break;
                    }
                }
                if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: DestroyLimbsPatch)"); return; }
                _questController._statCounterController.BodyPartDestroyed(damageInfo, bodyPart, __instance.Player);
            }
        }
        else { Plugin.Log.LogInfo("bodyPartState is null, returning"); return; }
    }
}
/*
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
*/
/*
internal class FixMalfunctionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController.GClass2013), nameof(Player.FirearmController.GClass2013.method_2));
    }
    [PatchPostfix]
    private static void Postfix(Player.FirearmController.GClass2013 __instance)
    {
        if (__instance.Player_0.Profile.Id == ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.Id)
        {
            StatCounterQuestController.MalfunctionFixed(__instance.Weapon_0);
        }
        else Plugin.Log.LogInfo($"Malfunction was fixed, but the id of the instance was {__instance.Player_0.Profile.Id}");
    }
}
*/
internal class FixMalfunctionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController.FixMalfunctionOperationClass), nameof(Player.FirearmController.FixMalfunctionOperationClass.OnMalfunctionOffEvent));
    }
    [PatchPostfix]
    private static void Postfix(Player.FirearmController.FixMalfunctionOperationClass __instance)
    {
        if (AbstractCustomQuestController.isScavRaid) return;
        if (__instance.Player_0.Profile.Id == Plugin.PlayerProfileID)
        {
            QuestExtendedController _questController = null;
            foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
            {
                if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
                {
                    _questController = QEC;
                    break;
                }
            }
            if (_questController == null) { Plugin.Log.LogError("QEC null, aborting (Patch: FixMalfunctionPatch)"); return; }
            _questController._statCounterController.MalfunctionFixed(__instance.Weapon_0);
        }
        else Plugin.Log.LogInfo($"Malfunction was fixed, but the id of the instance was {__instance.Player_0.Profile.Id}");
    }
}
internal class FixMalfunctionWatcher : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController.FixMalfunctionOperationClass), nameof(Player.FirearmController.FixMalfunctionOperationClass.Start));
    }
    [PatchPostfix]
    private static void Postfix(Player.FirearmController.FixMalfunctionOperationClass __instance)
    {
        if (__instance.Player_0.Profile.Id == ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.Id)
        {
            Plugin.Log.LogInfo("Should use Start instead.");
        }
    }
}