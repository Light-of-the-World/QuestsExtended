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
    public List<ConditionPair> GetActiveConditions(Enum conditionType)
    {
        var quests = GetActiveQuests();

        // No quests, return empty
        using var enumerator = quests.GetEnumerator();
        if (!enumerator.MoveNext()) return new List<ConditionPair>();

        List<ConditionPair> pairs = new List<ConditionPair>();

        // Determine the enum type based on the conditionType
        Type enumType = null;
        Array enumValues = null;

        if (conditionType.GetType() == typeof(EQuestConditionGen))
        {
            enumType = typeof(EQuestConditionGen);
            enumValues = Enum.GetValues(typeof(EQuestConditionGen));
        }
        else if (conditionType.GetType() == typeof(EQuestConditionCombat))
        {
            enumType = typeof(EQuestConditionCombat);
            enumValues = Enum.GetValues(typeof(EQuestConditionCombat));
        }
        else if (conditionType.GetType() == typeof(EQuestConditionHealth))
        {
            enumType = typeof(EQuestConditionHealth);
            enumValues = Enum.GetValues(typeof(EQuestConditionHealth));
        }
        else
        {
            Plugin.Log.LogWarning($"Unsupported condition type: {conditionType.GetType().Name}. Defaulting to EQuestConditionGen.");
            enumType = typeof(EQuestConditionGen);
            enumValues = Enum.GetValues(typeof(EQuestConditionGen));
        }

        foreach (var quest in quests)
        {
            var questRespCond = GetCustomConditionsByCondition(quest.Id, conditionType);

            if (questRespCond is null)
            {
                Plugin.Log.LogWarning($"Skipping quest {quest.Id} : No {conditionType} condition");
                continue;
            }

            List<CustomCondition> activeOnLocation = new List<CustomCondition>();

            foreach (var cond in questRespCond)
            {
                // Check if the conditionType belongs to the selected enum type
                if (conditionType.GetType() != enumType) continue;

                // Check if the condition matches one of the selected enum values
                foreach (var enumValue in enumValues)
                {
                    if ((Convert.ToInt32(conditionType) & Convert.ToInt32(enumValue)) != 0)
                    {
                        if (cond.Locations != null)
                        {
                            foreach (var loc in cond.Locations)
                            {
                                if (loc == _player.Location || loc == "any")
                                {
                                    activeOnLocation.Add(cond);
                                    break;
                                }
                            }
                        }
                        break;  // Exit once we find a match
                    }
                }
            }

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
    private IEnumerable<CustomCondition> GetCustomConditionsByCondition(string questId, Enum conditionType)
    {
        if (!CustomQuests.TryGetValue(questId, out var quest))
        {
            return null;
        }

        List<CustomCondition> customConditions = new List<CustomCondition>();

        // Use a switch case based on the type of the enum
        switch (conditionType)
        {
            case EQuestConditionGen _:
                // Check if the conditionType matches the passed enum using bitwise operations
                foreach (var cond in quest.Conditions)
                {
                    if ((Convert.ToInt32(conditionType) & Convert.ToInt32(cond.GenConditionType)) != 0)
                    {
                        customConditions.Add(cond);
                    }
                }
                break;

            case EQuestConditionCombat _:
                // Check if the conditionType matches the passed enum using bitwise operations
                foreach (var cond in quest.Conditions)
                {
                    if ((Convert.ToInt32(conditionType) & Convert.ToInt32(cond.CombatConditionType)) != 0)
                    {
                        customConditions.Add(cond);
                    }
                }
                break;

            case EQuestConditionHealth _:
                // Check if the conditionType matches the passed enum using bitwise operations
                foreach (var cond in quest.Conditions)
                {
                    if ((Convert.ToInt32(conditionType) & Convert.ToInt32(cond.HealthConditionType)) != 0)
                    {
                        customConditions.Add(cond);
                    }
                }
                break;

            default:
                return null;
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