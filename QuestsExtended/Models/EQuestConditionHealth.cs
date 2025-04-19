using System;

namespace QuestsExtended.Models;

[Flags]
public enum EQuestConditionHealth
{
    FixLightBleed = 1 << 0,
    FixHeavyBleed = 1 << 1,
    FixAnyBleed = 1 << 2,
    FixFracture = 1 << 3,
    HealthLoss = 1 << 4,
    HealthGain = 1 << 5,
    DestroyBodyPart = 1 << 6,
    RestoreBodyPart = 1 << 7,
}