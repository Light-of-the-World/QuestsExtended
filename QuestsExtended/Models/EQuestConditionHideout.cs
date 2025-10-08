using System;

namespace QuestsExtended.Models;

[Flags]
public enum EQuestConditionHideout
{
    CraftAnyItem = 1 << 0,
    CraftCyclicItem = 1 << 1,
    CollectScavCase = 1 << 2,
    CollectCultistOffering = 1 << 3,
    CompleteWorkout = 1 << 4,
    EmptyHI = 1 << 31,
}