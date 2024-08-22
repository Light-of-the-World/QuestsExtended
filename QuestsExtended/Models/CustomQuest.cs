using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;

namespace QuestsExtended.Models;

public struct CustomQuest
{
    // Quest to search for the condition on
    public string QuestId;

    public List<CustomCondition> Conditions;
}

public struct CustomCondition
{
    // Quest to search for the condition on
    public string ConditionId;
    public EQuestCondition ConditionType;
    [CanBeNull] public List<string> Locations;
    [CanBeNull] public List<string> AllowedItems;
    [CanBeNull] public List<string> ForbiddenItems;
    [CanBeNull] public List<string> Zones;
    [CanBeNull] public List<EBodyPart> IncludeBodyParts;
    [CanBeNull] public List<EBodyPart> ExcludeBodyParts;
    [CanBeNull] public List<EDamageType> DamageTypes;
}
