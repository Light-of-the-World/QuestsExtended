using Comfort.Common;
using EFT;
using EFT.Quests;
using HarmonyLib;
using JetBrains.Annotations;
using QuestsExtended.Models;
using SPT.Reflection.Utils;
using SPTarkov.Server.Core.Models.Spt.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QuestsExtended.Quests;

internal class QuestExtendedController : MonoBehaviour
{
    private static string UnderlyingQuestControllerClassName;
    public Player _player;
    public string LocalPlayerID;
    public AbstractQuestControllerClass _questAbstractController;
    private Dictionary<string, CustomQuest> CustomQuests => Plugin.Quests;

    private readonly List<string> _questsWithCustomConditions = [];

    public List<string> conditionTypes = new List<string>();

    public MedicalQuestController _medController;
    public PhysicalQuestController _physicalController;
    public StatCounterQuestController _statCounterController;
    public OptionalConditionController _optionalController;
    public HideoutQuestController _hideoutQuestController;
    public TradingQuestController _tradingQuestController;
    public bool isRaidOver = false;
    public bool isInMainMenu = false;
    public bool hasCompletedInitMM = false;
    public bool hasCompletedHideoutInit = false;
    //public string PlayerProfile = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId;
    void Awake()
    {
        Plugin.PlayerProfileID = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId;
    }
    public void InitForRaid()
    {
        isInMainMenu = false;
        LocalPlayerID = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId;
        if (LocalPlayerID == null) Plugin.Log.LogError("Failed to get LocalPlayerID! Expect errors...");
        if (Plugin.PlayerProfileID == null) Plugin.PlayerProfileID = LocalPlayerID;
        //_player = Singleton<GameWorld>.Instance.MainPlayer;
        foreach (var person in Singleton<GameWorld>.Instance.AllAlivePlayersList)
        {
            if (person.IsAI) continue;
            if (person.Profile.ProfileId == Plugin.PlayerProfileID)
                if (person.Side == EPlayerSide.Savage)
                {
                    Plugin.Log.LogInfo("(QE) No need to attatch QE to a scav raid, aborting.");
                    _player = null;
                    AbstractCustomQuestController.isScavRaid = true;
                    return;
                }
            { _player = person; break; }
            //We made a change here, watch for breaks.
        }
        AbstractCustomQuestController.isScavRaid = false;
        _questAbstractController = _player?.AbstractQuestControllerClass;

        hasCompletedInitMM = false;

        _medController = new MedicalQuestController(this);
        _physicalController = new PhysicalQuestController(this);
        _statCounterController = new StatCounterQuestController(this);
        _optionalController = new OptionalConditionController(this);
        _statCounterController.Awake();
        _optionalController.Awake();


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
            Plugin.Log.LogInfo($"Resolved {nameof(UnderlyingQuestControllerClassName)} to be {UnderlyingQuestControllerClassName} (put this back to LogDebug!)");
        }

        foreach (var condition in CustomQuests)
        {
            _questsWithCustomConditions.Add(condition.Key);
        }
        Plugin.Log.LogInfo("InitForRaid ran successfully. Happy playing!");
    }

    public void InitFromMainMenu(AbstractQuestControllerClass questControllerClass)
    {
        _questAbstractController = questControllerClass;
        _optionalController = new OptionalConditionController(this);
        _optionalController.Awake();
        _hideoutQuestController = new HideoutQuestController(this);
        _hideoutQuestController.Init();
        _tradingQuestController = new TradingQuestController(this);
        _tradingQuestController.Init();
        LocalPlayerID = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.ProfileId;
        isInMainMenu = true;

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
            //Plugin.Log.LogInfo($"Resolved {nameof(UnderlyingQuestControllerClassName)} to be {UnderlyingQuestControllerClassName} (put this back to LogDebug!)");
        }
        foreach (var condition in CustomQuests)
        {
            _questsWithCustomConditions.Add(condition.Key);
        }
        Plugin.Log.LogInfo($"(QE) InitfromMainMenu ran successfully.");
    }


    public void OnDestroy()
    {
        if (_medController != null) _medController.OnDestroy();
        if (_physicalController != null) _physicalController.OnDestroy();
        _player = null;
        isRaidOver = true;
        isInMainMenu = true;
    }

    private void Update()
    {
        if (isInMainMenu || _player == null) return;
        _physicalController.Update();
        _optionalController.Update();
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
        else if (conditionType.GetType() == typeof(EQuestConditionMisc1))
        {
            enumType = typeof(EQuestConditionMisc1);
            enumValues = Enum.GetValues(typeof(EQuestConditionMisc1));
        }
        else if (conditionType.GetType() == typeof(EQuestConditionHideout))
        {
            enumType = typeof(EQuestConditionHideout);
            enumValues = Enum.GetValues(typeof(EQuestConditionHideout));
        }
        else if (conditionType.GetType() == typeof(EQuestConditionTrading))
        {
            enumType = typeof(EQuestConditionTrading);
            enumValues = Enum.GetValues(typeof(EQuestConditionTrading));
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
                        if (isInMainMenu)
                        {
                            activeOnLocation.Add(cond);
                            break;
                        }
                        if (cond.Locations != null)
                        {
                            foreach (var loc in cond.Locations)
                            {
                                if (_player != null)
                                {
                                    if (loc == _player.Location || loc == "any")
                                    {
                                        activeOnLocation.Add(cond);
                                        break;
                                    }
                                }
                            }
                        }
                        else if (cond.Locations == null)
                        {
                            activeOnLocation.Add(cond);
                            break;
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
    public void IncrementConditionCounter(QuestClass quest, EFT.Quests.Condition condition, float value)
    {
        //Plugin.Log.LogInfo("0");
        var currentVal = quest.ProgressCheckers[condition].CurrentValue;
        //Plugin.Log.LogInfo("1");
        quest.ProgressCheckers[condition].SetCurrentValueGetter(_ => currentVal + value);
        //Plugin.Log.LogInfo("2");

        // We call 'SetConditionCurrentValue' to trigger all the code needed to make the condition completion appear visually in-game 693e7ffec0460862b6e074c4
        /*
        var conditionController = AccessTools.Field(
                _questAbstractController.GetType(), 
                $"{UnderlyingQuestControllerClassName.ToLowerInvariant()}_0")
            .GetValue(_questAbstractController);
        /*
        //TaskConditionCounterClass conditionController = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.GetTaskConditionCounter(quest, condition.id);

        Plugin.Log.LogInfo("3");
        
        AccessTools.DeclaredMethod(_questAbstractController.GetType().BaseType, "SetConditionCurrentValue")
            .Invoke(conditionController, new object[] { quest, /*EQuestStatus.AvailableForFinish < WRONG*/ // EQuestStatus.Started, condition, currentVal + value, true });
        TaskConditionCounterClass correctClass = null;
        foreach (TaskConditionCounterClass taskCompletionClass in quest.ConditionCountersManager.Counters)
        {
            if (taskCompletionClass.Id == condition.id) correctClass = taskCompletionClass;// Plugin.Log.LogInfo("We got the correct TaskConditionCounterClass");
        }
        if (correctClass == null)
        {
            Plugin.Log.LogInfo("Could not find the correct class. Cancelling.");
            return;
        }
        else
        {
            ConditionCounterCreator conditionCounterCreator = correctClass.Template as ConditionCounterCreator;
            if (conditionCounterCreator == null)
            {
                return;
            }

            correctClass.Value += (int)Math.Round(value);
            //Plugin.Log.LogInfo("If you're seeing this, it's possible that things are working.");
        }
    }
    /// <summary>
    /// Gets all active quests that are started,
    /// and we have custom conditions for
    /// </summary>
    /// <returns></returns>
    public IEnumerable<QuestClass> GetActiveQuests()
    {
        List<QuestClass> activeQuests = [];

        foreach (var quest in _questAbstractController.Quests)
        {
            if (quest.QuestStatus == EQuestStatus.Started && _questsWithCustomConditions.Contains(quest.Id))
            {
                activeQuests.Add(quest);
                _optionalController.AddQuestIDToActiveList(quest.Id);
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
    private EFT.Quests.Condition GetBsgConditionById(string questId, string conditionId)
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
                if (condition.ChildConditions.Count > 0)
                {
                    foreach (var childCondition in condition.ChildConditions)
                    {
                        if (childCondition.id == conditionId)
                        {
                            return childCondition;
                        }
                    }
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
        if (quest.Conditions == null) return null;
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
            case EQuestConditionHideout _:
                // Check if the conditionType matches the passed enum using bitwise operations
                foreach (var cond in quest.Conditions)
                {
                    if ((Convert.ToInt32(conditionType) & Convert.ToInt32(cond.HideoutConditionType)) != 0)
                    {
                        customConditions.Add(cond);
                    }
                }
                break;
            case EQuestConditionMisc1 _:
                // Check if the conditionType matches the passed enum using bitwise operations
                foreach (var cond in quest.Conditions)
                {
                    if ((Convert.ToInt32(conditionType) & Convert.ToInt32(cond.Misc1ConditionType)) != 0)
                    {
                        customConditions.Add(cond);
                    }
                }
                break;
            case EQuestConditionTrading _:
                // Check if the conditionType matches the passed enum using bitwise operations
                foreach (var cond in quest.Conditions)
                {
                    if ((Convert.ToInt32(conditionType) & Convert.ToInt32(cond.TradingConditionType)) != 0)
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
        if (_questAbstractController?.Quests == null) return null;

        foreach (var quest in _questAbstractController.Quests)
        {
            if (quest != null && quest.Id == questId)
            {
                return quest;
            }
        }

        return null;
    }
}