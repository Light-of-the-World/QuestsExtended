using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Quests;
using HarmonyLib;
using JetBrains.Annotations;
using QuestsExtended.Models;
using UnityEngine;

namespace QuestsExtended.Quests;

internal class QuestExtendedController : MonoBehaviour
{
    private static string UnderlyingQuestControllerClassName;
    private Player _player;
    private AbstractQuestControllerClass _questController;
    private Dictionary<string, CustomQuest> CustomQuests => Plugin.Quests;

    private readonly List<string> _questsWithCustomConditions = [];

    public static List<string> conditionTypes = new List<string>();

    private static MedicalQuestController _medController;
    private static PhysicalQuestController _physicalController;
    private static StatCounterQuestController _statCounterController;

    void Awake()
    {
        _player = Singleton<GameWorld>.Instance.MainPlayer;
        _questController = _player?.AbstractQuestControllerClass;

        _medController = new MedicalQuestController(this);
        _physicalController = new PhysicalQuestController(this);
        _statCounterController = new StatCounterQuestController(this);

        _statCounterController.Awake();


        if (UnderlyingQuestControllerClassName == null)
        {
            Type foundType = null;

            foreach (var type in AccessTools.GetTypesFromAssembly(typeof(AbstractGame).Assembly))
            {
                if (type.GetEvent("OnConditionQuestTimeExpired", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) != null)
                {
                    foundType = type;
                    break;
                }
            }

            if (foundType == null)
            {
                Plugin.Log.LogError("Failed to locate a specific quest controller type");
                return;
            }

            UnderlyingQuestControllerClassName = foundType.Name.Split('`')[0];
            Plugin.Log.LogDebug($"Resolved {nameof(UnderlyingQuestControllerClassName)} to be {UnderlyingQuestControllerClassName}");
        }

        foreach (var condition in CustomQuests)
        {
            _questsWithCustomConditions.Add(condition.Key);
        }
    }


    private void OnDestroy()
    {
        _medController.OnDestroy();
        _physicalController.OnDestroy();
    }

    private void Update()
    {
        _physicalController.Update();
    }

    /// <summary>
    /// Get active conditions for a specific type
    /// </summary>
    /// <param name="conditionType"></param>
    /*
    public List<ConditionPair> GetActiveConditions(EQuestConditionGen conditionType)
    {
        var quests = GetActiveQuests();

        // No quests, return empty
        using var enumerator = quests.GetEnumerator();
        if (!enumerator.MoveNext()) return [];

        List<ConditionPair> pairs = [];
        
        foreach (var quest in quests)
        {
            var questRespCond = GetCustomConditionsByCondition(quest.Id, conditionType);
            
            if (questRespCond is null)
            {
                Plugin.Log.LogWarning($"Skipping quest {quest.Id} : No {conditionType} condition");
                continue;
            }

            List<CustomCondition> activeOnLocation = [];

            foreach (var cond in questRespCond)
            {
                if ((conditionType & cond.ConditionType) == 0) continue;
                if (cond.Locations != null)
                {
                    foreach (var loc in cond.Locations)
                    {
                        if (loc == _player.Location || loc == "any")
                        {
                            activeOnLocation.Add(cond);
                            break; // No need to check more locations once a match is found
                        }
                    }
                }
            }
            // Make sure if there are conditions both specific to this map and across any map that we increment all of them
            foreach (var condition in activeOnLocation)
            {
                var bsgCondition = GetBsgConditionById(quest.Id, condition.ConditionId);
                
                if (bsgCondition is null) continue;

                ConditionPair pair = new()
                {
                    Quest = quest,
                    Condition = bsgCondition,
                    CustomCondition = condition
                };
                
                pairs.Add(pair);
            }
        }

        return pairs;
    }
    */
    public List<ConditionPair> GetActiveConditions(params string[] conditionNames)
    {
        var quests = GetActiveQuests();

        // No quests, return empty
        using var enumerator = quests.GetEnumerator();
        if (!enumerator.MoveNext()) return new List<ConditionPair>();

        List<ConditionPair> pairs = new List<ConditionPair>();

        // List of all Enum types (EQuestCondition* enums)
        var enumTypes = new List<Type>
        {
            typeof(EQuestConditionGen),
            typeof(EQuestConditionHealth),
            typeof(EQuestConditionCombat)
            // Add other enums if needed
        };

        foreach (var quest in quests)
        {
            // Get the matching custom conditions for the quest
            var questRespCond = GetCustomConditionsByCondition(quest.Id, conditionNames);

            if (questRespCond is null)
            {
                Plugin.Log.LogWarning($"Skipping quest {quest.Id} : No conditions for provided names");
                continue;
            }

            List<CustomCondition> activeOnLocation = new List<CustomCondition>();
            // Loop through all enums
            // List of known enum-type properties on CustomCondition
            //var enumProperties = new[] { "GenConditionType", "HealthConditionType", "CombatConditionType" };
            foreach (var cond in questRespCond)
            {
                foreach (var enumType in enumTypes)
                {
                    if (enumType == typeof(EQuestConditionGen))
                    {
                        if (Enum.TryParse<EQuestConditionGen>(name, out var parsedEnum))
                        {
                            if ((cond.GenConditionType & parsedEnum) != 0) activeOnLocation.Add(cond);
                            else Plugin.Log.LogInfo($"Parse failed. Logging values. parsed enum = {parsedEnum.ToString()}. cond type = {cond.GenConditionType.ToString()}.");
                        }
                    }
                    else if (enumType == typeof(EQuestConditionCombat))
                    {
                        if (Enum.TryParse<EQuestConditionCombat>(name, out var parsedEnum))
                        {
                            if ((cond.CombatConditionType & parsedEnum) != 0) activeOnLocation.Add(cond);
                        }
                    }
                    else if (enumType == typeof(EQuestConditionHealth))
                    {
                        if (Enum.TryParse<EQuestConditionHealth>(name, out var parsedEnum))
                        {
                            if ((cond.HealthConditionType & parsedEnum) != 0) activeOnLocation.Add(cond);
                        }
                    }
                    else Plugin.Log.LogWarning("enumType is none of the enums!");
                }
            }

            // Now, add the conditions that match
            foreach (var condition in activeOnLocation)
            {
                Plugin.Log.LogInfo("Trying GetBsgConditionById");
                var bsgCondition = GetBsgConditionById(quest.Id, condition.ConditionId);

                if (bsgCondition is null) continue;

                ConditionPair pair = new()
                {
                    Quest = quest,
                    Condition = bsgCondition,
                    CustomCondition = condition
                };
                Plugin.Log.LogInfo($"[QE]Adding pair: {quest}, {bsgCondition}, {condition}");
                pairs.Add(pair);
            }
        }

        return pairs;
    }
    /// <summary>
    /// Increments a provided condition counter
    ///
    /// Credits: Terkoiz for this code
    /// </summary>
    /// <param name="quest"></param>
    /// <param name="condition"></param>
    /// /// <param name="value"></param>
    public void IncrementConditionCounter(QuestClass quest, Condition condition, float value)
    {
        // This line will increment the condition counter by 1
        var currentVal = quest.ProgressCheckers[condition].CurrentValue;
        quest.ProgressCheckers[condition].SetCurrentValueGetter(_ => currentVal + value);
                    
        // We call 'SetConditionCurrentValue' to trigger all the code needed to make the condition completion appear visually in-game
        var conditionController = AccessTools.Field(
                _questController.GetType(), 
                $"{UnderlyingQuestControllerClassName.ToLowerInvariant()}_0")
            .GetValue(_questController);
                    
        AccessTools.DeclaredMethod(conditionController.GetType().BaseType, "SetConditionCurrentValue")
            .Invoke(conditionController, new object[] { quest, EQuestStatus.AvailableForFinish, condition, currentVal + value, true });
    }
    
    /// <summary>
    /// Gets all active quests that are started,
    /// and we have custom conditions for
    /// </summary>
    /// <returns></returns>
    private IEnumerable<QuestClass> GetActiveQuests()
    {
        List<QuestClass> activeQuests = [];

        foreach (var quest in _questController.Quests)
        {
            if (quest.QuestStatus == EQuestStatus.Started && _questsWithCustomConditions.Contains(quest.Id))
            {
                activeQuests.Add(quest);
            }
        }

        Plugin.Log.LogDebug($"Custom conditions active: {activeQuests.Count > 0}");

        return activeQuests;
    }
    
    /// <summary>
    /// Gets BSG conditions by questId and conditionId
    /// </summary>
    /// <param name="questId"></param>
    /// <param name="conditionId"></param>
    /// <returns></returns>
    [CanBeNull]
    private Condition GetBsgConditionById(string questId, string conditionId)
    {
        var quest = GetQuestById(questId);

        if (quest is null) return null;
        if (quest.QuestStatus != EQuestStatus.Started) return null;

        foreach (var gclass in quest.Conditions)
        {
            var conditions = gclass.Value.IEnumerable_0;

            foreach (var condition in conditions)
            {
                if (condition.id == conditionId)
                {
                    return condition;
                }
            }
        }

        Plugin.Log.LogWarning($"Could not find condition `{conditionId}` on quest `{questId}`");
        return null;
    }

    /// <summary>
    /// Gets custom conditions by quest id and condition type
    /// </summary>
    /// <param name="questId"></param>
    /// <param name="conditionType"></param>
    /// <returns></returns>
    [CanBeNull]
    private IEnumerable<CustomCondition> GetCustomConditionsByCondition(string questId, params string[] conditionNames)
    {
        if (!CustomQuests.TryGetValue(questId, out var quest))
        {
            return null;
        }

        List<CustomCondition> customConditions = new List<CustomCondition>();
        foreach (var cond in quest.Conditions)
        {
            if ((conditionNames != null) && (conditionNames.Length != 0) /*&& ((cond.GenConditionType !=0 ) || (cond.HealthConditionType !=0 ) || (cond.CombatConditionType != 0 ))*/)
            {
                customConditions.Add(cond);
            }
            else
            {
                if (conditionNames == null)
                {
                    Plugin.Log.LogWarning("[QE] conditionNames was null");
                }
                if (conditionNames.Length == 0)
                {
                    Plugin.Log.LogWarning("[QE] conditionNames's length is equal to 0.");
                }
                if (cond.GenConditionType == 0)
                {
                    Plugin.Log.LogWarning("[QE] cond.GenConditionType is empty.");
                }
                if (cond.HealthConditionType == 0)
                {
                    Plugin.Log.LogWarning("[QE] cond.HealthConditionType is empty.");
                }
                if (cond.CombatConditionType == 0)
                {
                    Plugin.Log.LogWarning("[QE] cond.CombatConditionType is empty.");
                }
            }
        }

        return customConditions;
    }

    [CanBeNull]
    private QuestClass GetQuestById(string questId)
    {
        if (_questController?.Quests == null) return null;

        foreach (var quest in _questController.Quests)
        {
            if (quest != null && quest.Id == questId)
            {
                return quest;
            }
        }

        return null;
    }
}