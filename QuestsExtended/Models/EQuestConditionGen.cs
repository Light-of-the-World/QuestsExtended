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
    SearchContainer = 1 << 6,
    LootItem = 1 << 7,
    ActivatePowerSwitch = 1 << 8,
    Empty = 1 <<31,
}