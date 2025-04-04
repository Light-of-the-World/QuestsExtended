using System;

namespace QuestsExtended.Models;

[Flags]
public enum EQuestCondition
{
    FixLightBleed = 1 << 0,
    FixHeavyBleed = 1 << 1,
    FixAnyBleed = 1 << 2,
    FixFracture = 1 << 3,
    HealthLoss = 1 << 4,
    HealthGain = 1 << 5,
    DestroyBodyPart = 1 << 6,
    RestoreBodyPart = 1 << 7,
    Die = 1 << 8,
    EncumberedTimeInSeconds = 1 << 9,
    OverEncumberedTimeInSeconds = 1 << 10,
    MoveDistance = 1 << 11,
    MoveDistanceWhileCrouched = 1 << 12,
    MoveDistanceWhileProne = 1 << 13,
    MoveDistanceWhileSilent = 1 << 14,
    KillWhileADS = 1 << 15,
    KillWhileProne = 1 << 16,
    SearchContainer = 1 << 17,
    LootItem = 1 << 18,
}