using System;

namespace QuestsExtended.Models;

[Flags]
public enum EQuestConditionCombat
{
    DamageWithAR = 1 << 0,
    DamageWithDMR = 1 << 1,
    DamageWithLMG = 1 << 2,
    DamageWithMelee = 1 << 3,
    DamageWithPistols = 1 << 4,
    DamageWithRevolvers = 1 << 5,
    DamageWithShotguns = 1 << 6,
    DamageWithSMG = 1 << 7,
    DamageWithSnipers = 1 << 8,
    DamageWithThrowables = 1 << 9,
    TotalShotDistanceWithSnipers = 1 << 10
}
