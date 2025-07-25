﻿using System;
using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace QuestsExtended.Models;

public struct CustomQuest
{
    // Quest to search for the condition on
    public string QuestId;
    [CanBeNull] public string QuestName;
    [CanBeNull] public bool IsMultipleChoiceStarter;
    [CanBeNull] public bool BlockQuestMessages;
    public List<CustomCondition> Conditions;
}
public struct CustomCondition
{
    public string ConditionId;

    public EQuestConditionGen GenConditionType;
    public EQuestConditionCombat CombatConditionType;
    public EQuestConditionHealth HealthConditionType;
    public EQuestConditionMisc1 Misc1ConditionType;
    public EQuestConditionHideout HideoutConditionType;
    public EQuestConditionTrading TradingConditionType;

    [JsonProperty("ConditionType")] // This maps the JSON "ConditionType" to this property
    public string ConditionTypeRaw
    {
        get => _conditionTypeRaw;
        set
        {
            _conditionTypeRaw = value;
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
            else if (Enum.TryParse(value, out EQuestConditionMisc1 misc))
            {
                Misc1ConditionType = misc;
            }
            else if (Enum.TryParse(value, out EQuestConditionHideout hide))
            {
                HideoutConditionType = hide;
            }
            else if (Enum.TryParse(value, out EQuestConditionTrading trade))
            {
                TradingConditionType = trade;
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
    [CanBeNull] public bool HasMultipleConditionTypes;
    [CanBeNull] public bool IsFail;
    [CanBeNull] public List<string> MultiConditions;
    [CanBeNull] public List<string> Locations;
    [CanBeNull] public List<string> AllowedItems;
    [CanBeNull] public List<string> ForbiddenItems;
    [CanBeNull] public List<string> Zones;
    [CanBeNull] public List<EBodyPart> IncludeBodyParts;
    [CanBeNull] public List<EBodyPart> ExcludeBodyParts;
    [CanBeNull] public List<EDamageType> DamageTypes;
    [CanBeNull] public List<string> QuestsToStart;
    [CanBeNull] public List<string> Workstations;
    [CanBeNull] public List<string> CyclicWorkstations;
    [CanBeNull] public List<string> TraderIds;
    [CanBeNull] public List<string> CurrencyTypes;
    [CanBeNull] public List<string> EnemyTypes;
}