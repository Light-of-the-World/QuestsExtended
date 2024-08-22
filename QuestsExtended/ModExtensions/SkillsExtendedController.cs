using System;
using QuestsExtended.API;
using QuestsExtended.Models;
using QuestsExtended.Quests;

namespace QuestsExtended.ModExtensions;

internal class SkillsExtendedController : AbstractCustomQuestController
{
    public SkillsExtendedController(QuestExtendedController questController)
        : base(questController)
    {
        QuestEvents.Instance.OnLockInspected += InspectLockHandler;
        QuestEvents.Instance.OnLockPicked += PickLockHandler;
        QuestEvents.Instance.OnLockPickFailed += PickLockFailedHandler;
        QuestEvents.Instance.OnBreakLock += BreakLockHandler;
        QuestEvents.Instance.OnRepairLock += RepairLockHandler;
        QuestEvents.Instance.OnRepairLockFailed += RepairLockFailedHandler;
        QuestEvents.Instance.OnHackDoor += HackDoorHandler;
        QuestEvents.Instance.OnHackDoorFailed += HackDoorFailedHandler;
    }

    public void OnDestroy()
    {
        QuestEvents.Instance.OnLockInspected -= InspectLockHandler;
        QuestEvents.Instance.OnLockPicked -= PickLockHandler;
        QuestEvents.Instance.OnLockPickFailed -= PickLockFailedHandler;
        QuestEvents.Instance.OnRepairLock -= RepairLockHandler;
        QuestEvents.Instance.OnRepairLockFailed -= RepairLockFailedHandler;
        QuestEvents.Instance.OnBreakLock -= BreakLockHandler;
        QuestEvents.Instance.OnHackDoor -= HackDoorHandler;
        QuestEvents.Instance.OnHackDoorFailed -= HackDoorFailedHandler;
    }
    
    private void InspectLockHandler(object sender, EventArgs e)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.InspectLock);
        
        IncrementConditions(conditions, 1f);
    }
    
    private void PickLockHandler(object sender, EventArgs e)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.PickLock);
        
        IncrementConditions(conditions, 1f);
    }
    
    private void PickLockFailedHandler(object sender, EventArgs e)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.PickLockFailed);
        
        IncrementConditions(conditions, 1f);
    }
    
    private void BreakLockHandler(object sender, EventArgs e)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.BreakLock);
        
        IncrementConditions(conditions, 1f);
    }
    
    private void HackDoorHandler(object sender, EventArgs e)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.HackDoor);
        
        IncrementConditions(conditions, 1f);
    }
    
    private void RepairLockHandler(object sender, EventArgs e)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.RepairLock);
        
        IncrementConditions(conditions, 1f);
    }
    
    private void RepairLockFailedHandler(object sender, EventArgs e)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.RepairLockFailed);
        
        IncrementConditions(conditions, 1f);
    }
    
    private void HackDoorFailedHandler(object sender, EventArgs e)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.HackDoorFailed);
        
        IncrementConditions(conditions, 1f);
    }
}