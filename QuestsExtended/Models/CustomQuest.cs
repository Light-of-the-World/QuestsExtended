using System;
using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace QuestsExtended.Models;

public struct CustomQuest
{
    // Quest to search for the condition on
    public string QuestId;

    public List<CustomCondition> Conditions;
}

/*
public struct CustomCondition
{
    // Quest to search for the condition on
    public string ConditionId;
    public EQuestConditionGen GenConditionType;
    public EQuestConditionCombat CombatConditionType;
    public EQuestConditionHealth HealthConditionType;
    [CanBeNull] public bool RequireMoving;
    [CanBeNull] public List<string> Locations;
    [CanBeNull] public List<string> AllowedItems;
    [CanBeNull] public List<string> ForbiddenItems;
    [CanBeNull] public List<string> Zones;
    [CanBeNull] public List<EBodyPart> IncludeBodyParts;
    [CanBeNull] public List<EBodyPart> ExcludeBodyParts;
    [CanBeNull] public List<EDamageType> DamageTypes;
}
*/
public struct CustomCondition
{
    public string ConditionId;

    public EQuestConditionGen GenConditionType;
    public EQuestConditionCombat CombatConditionType;
    public EQuestConditionHealth HealthConditionType;

    // This is only used during JSON parsing
    [JsonProperty("ConditionType")] // This maps the JSON "ConditionType" to this property
    public string ConditionTypeRaw
    {
        get => _conditionTypeRaw;
        set
        {
            _conditionTypeRaw = value;
            Plugin.Log.LogInfo($"[DEBUG] Parsed ConditionTypeRaw = {value}");
            // Try each enum type until one succeeds
            if (Enum.TryParse(value, out EQuestConditionGen gen))
            {
                GenConditionType = gen;
            }
            else if (Enum.TryParse(value, out EQuestConditionCombat combat))
            {
                CombatConditionType = combat;
            }
            else if (Enum.TryParse(value, out EQuestConditionHealth health))
            {
                HealthConditionType = health;
            }
            else
            {
                // Optional: log unknown condition type
                Plugin.Log.LogWarning($"Unknown condition type: {value}");
            }
        }
    }

    private string _conditionTypeRaw;

    [CanBeNull] public bool RequireMoving;
    [CanBeNull] public List<string> Locations;
    [CanBeNull] public List<string> AllowedItems;
    [CanBeNull] public List<string> ForbiddenItems;
    [CanBeNull] public List<string> Zones;
    [CanBeNull] public List<EBodyPart> IncludeBodyParts;
    [CanBeNull] public List<EBodyPart> ExcludeBodyParts;
    [CanBeNull] public List<EDamageType> DamageTypes;
}