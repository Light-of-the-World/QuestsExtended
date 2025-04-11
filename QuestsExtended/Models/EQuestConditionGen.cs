using System;

namespace QuestsExtended.Models;

[Flags]
public enum EQuestConditionGen
{
    EncumberedTimeInSeconds = 1 << 0,
    OverEncumberedTimeInSeconds = 1 << 1,
    MoveDistance = 1 << 2,
    MoveDistanceWhileCrouched = 1 << 3,
    MoveDistanceWhileProne = 1 << 4,
    MoveDistanceWhileSilent = 1 << 5,
    KillWhileADS = 1 << 6,
    KillWhileProne = 1 << 7,
    SearchContainer = 1 << 8,
    LootItem = 1 << 9,
}