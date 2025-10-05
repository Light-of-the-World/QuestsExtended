using System.Collections.Generic;
using Comfort.Common;
using EFT;
using QuestsExtended.Models;
using UnityEngine;

namespace QuestsExtended.Quests;

internal abstract class AbstractCustomQuestController
{
    protected static QuestExtendedController _questController;
    public static Player _player;
    public static bool isRaidOver = true;
    public static bool ShowResetMessage = false;
    public static bool ResetMainMenu = false;

    protected AbstractCustomQuestController(QuestExtendedController questExtendedController)
    {
        _questController = questExtendedController;
        if (Singleton<GameWorld>.Instance != null)
        {
            foreach (var person in Singleton<GameWorld>.Instance.AllAlivePlayersList)
            {
                if (person.IsAI) continue;
                _player = person;
                break;
                //We made a change here, watch for breaks.
            }
            //_player = Singleton<GameWorld>.Instance.MainPlayer;
        }
        else
        {
            _player = null;
            Plugin.Log.LogInfo("GameWorld.Instance was null during AbstractCustomQuestController construction (likely main menu).");
        }
    }

    /// <summary>
    /// Check base conditions for medical conditions
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="bodyPart"></param>
    /// <returns>True if all pass</returns>
    protected bool CheckBaseMedicalConditions(ConditionPair condition, EBodyPart bodyPart)
    {
        var pass = IsInZone(condition) 
                   && BodyPartIncludeCheck(condition, bodyPart) 
                   && !BodyPartExcludeCheck(condition, bodyPart);
        
        return pass;
    }
    
    /// <summary>
    /// Loop over and increment all provided condition pairs
    /// </summary>
    /// <param name="conditions"></param>
    /// /// <param name="value"></param>
    protected static void IncrementConditions(List<ConditionPair> conditions, float value = 0f)
    {
        foreach (var condition in conditions)
        {
            _questController.IncrementConditionCounter(condition.Quest, condition.Condition, value);
        }
    }

    /// <summary>
    /// Increment a single provided condition pair
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="value"></param>
    protected static void IncrementCondition(ConditionPair condition, float value = 0f)
    {
        if (condition.CustomCondition.Zones != null)
        {
            if (!IsInZone(condition)) return;
        }
        _questController.IncrementConditionCounter(condition.Quest, condition.Condition, value);
    }

    /// <summary>
    /// Is the player in an applicable zone for the condition
    /// </summary>
    /// <param name="condition"></param>
    /// <returns>True if we are in a zone, or no zones apply</returns>
    /// 
    /*
    private bool IsInZone(ConditionPair condition)
    {
        if (condition.CustomCondition.Zones is null) return true;
        
        var condZones = condition.CustomCondition.Zones;
        var playerZones = _player.TriggerZones;
        
        return condZones.Any(c => playerZones.Any(p => c == p));
    }
    */
    public static bool IsInZone(ConditionPair condition)
    {
        if (condition.CustomCondition.Zones is null) return true;

        var condZones = condition.CustomCondition.Zones;
        var playerZones = _player.TriggerZones;

        foreach (var c in condZones)
        {
            foreach (var p in playerZones)
            {
                if (c == p) return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Checks a provided condition for body parts the condition should be triggered for
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="partToCheckFor"></param>
    /// <returns>True if the condition passes</returns>
    private static bool BodyPartIncludeCheck(ConditionPair condition, EBodyPart partToCheckFor)
    {
        // Condition passes because it doesn't exist
        if (condition.CustomCondition.IncludeBodyParts is null) 
            return true;
        
        return condition.CustomCondition.IncludeBodyParts.Contains(partToCheckFor);
    }

    /// <summary>
    /// Should this body part be ignored for this condition
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="partToCheckFor"></param>
    /// <returns>True if we should ignore</returns>
    private static bool BodyPartExcludeCheck(ConditionPair condition, EBodyPart partToCheckFor)
    {
        // Condition passes because it doesn't exist
        if (condition.CustomCondition.ExcludeBodyParts is null) 
            return false;
        
        return condition.CustomCondition.ExcludeBodyParts.Contains(partToCheckFor);
    }
}