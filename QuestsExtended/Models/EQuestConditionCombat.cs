using System;

namespace QuestsExtended.Models;

[Flags]
public enum EQuestConditionCombat
{
    DamageWithAny = 1 << 0,
    DamageWithAR = 1 << 1,
    DamageWithDMR = 1 << 2,
    DamageWithGL = 1 << 3,
    DamageWithLMG = 1 << 4,
    DamageWithMelee = 1 << 5,
    DamageWithPistols = 1 << 6,
    DamageWithRevolvers = 1 << 7,
    DamageWithShotguns = 1 << 8,
    DamageWithSMG = 1 << 9,
    DamageWithSnipers = 1 << 10,
    DamageWithThrowables = 1 << 11,
    DamageToArmour = 1 << 12,
    DestroyEnemyBodyParts = 1 << 13,
    KillsWhileADS = 1 << 14,
    KillsWhileCrouched = 1 << 15,
    KillsWhileProne = 1 << 16,
    KillsWhileMounted = 1 << 17,
    KillsWhileSilent = 1 << 18,
    KillsWhileBlindFiring = 1 << 19,
    KillsWithoutADS = 1 << 20,
    MountedKillsWithLMG = 1 << 21,
    DestroyLegsWithSMG = 1 << 22,
    DamageToArmourWithShotguns = 1 << 23,
    TotalShotDistanceWithSnipers = 1 << 24,
    RevolverKillsWithoutADS = 1 << 25,
    Empty = 1 << 31,
}