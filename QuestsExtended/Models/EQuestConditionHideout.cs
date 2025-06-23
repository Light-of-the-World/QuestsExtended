using System;

namespace QuestsExtended.Models;

[Flags]
public enum EQuestConditionHideout
{
    CraftItem = 1 << 0,
    CraftCyclicItem = 1 << 1,
    CompleteWorkout = 1 << 2,
    CollectScavCase = 1 << 3,
    CollectCultistOffering = 1 << 4,
    EmptyHI = 1 << 31,
}