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
    EncumberedTimeInSeconds = 1 << 8,
    OverEncumberedTimeInSeconds = 1 << 9,
    MoveDistance = 1 << 10,
    MoveDistanceWhileCrouched = 1 << 11,
    MoveDistanceWhileProne = 1 << 12,
    MoveDistanceWhileSilent = 1 << 13,
    KillWhileADS = 1 << 14,
    KillWhileProne = 1 << 15,
    SearchContainer = 1 << 16,
    LootItem = 1 << 17,
}