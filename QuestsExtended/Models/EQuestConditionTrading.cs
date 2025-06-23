using System;

namespace QuestsExtended.Models;

[Flags]
public enum EQuestConditionTrading
{
    CompleteAnyTransaction = 1 << 0,
    SpendMoneyOnTransaction = 1 << 1,
    EarnMoneyOnTransaction = 1 << 2,
    EmptyT = 1 << 31,
}