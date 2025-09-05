using System;

namespace QuestsExtended.Models;

[Flags]
public enum EQuestConditionGen
{
    EncumberedTimeInSeconds = 1 << 0,
    OverEncumberedTimeInSeconds = 1 << 1,
    MoveDistance = 1 << 2,
    MoveDistanceWhileRunning = 1 << 3,
    MoveDistanceWhileCrouched = 1 << 4,
    MoveDistanceWhileProne = 1 << 5,
    MoveDistanceWhileSilent = 1 << 6,
    SearchContainer = 1 << 7,
    LootItem = 1 << 8,
    ActivatePowerSwitch = 1 << 9,
    UseKey = 1 << 10,
    CompleteOptionals = 1 << 11,
    EmptyWithQuestStarter = 1 << 30,
    Empty = 1 << 31,
}