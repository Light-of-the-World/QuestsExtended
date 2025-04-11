﻿using System;

namespace QuestsExtended.API;

/// <summary>
/// This is where all custom events used for quests are stored.
/// </summary>
public sealed class QuestEvents
{
    private static QuestEvents _questEvents;
    
    public static QuestEvents Instance
    {
        get
        {
            if (_questEvents is null)
            {
                _questEvents = new QuestEvents();
                return _questEvents;
            }

            return _questEvents;
        }
    }
    /*
    #region  LOCKPICKING

    public event PickLockEventHandler OnLockPicked;
    public delegate void PickLockEventHandler(object sender, EventArgs e);

    public void OnLockPickedEvent(object sender, EventArgs e)
    {
        if (OnLockPicked is null) return;
        
        OnLockPicked(sender, e);
    }
    
    
    public event PickLockFailedEventHandler OnLockPickFailed;
    public delegate void PickLockFailedEventHandler(object sender, EventArgs e);
    
    public void OnLockPickedFailedEvent(object sender, EventArgs e)
    {
        if (OnLockPickFailed is null) return;
        
        OnLockPickFailed(sender, e);
    }
    
    public event InspectEventHandler OnLockInspected;
    public delegate void InspectEventHandler(object sender, EventArgs e); 
    
    public void OnLockInspectedEvent(object sender, EventArgs e)
    {
        if (OnLockInspected is null) return;
        
        OnLockInspected(sender, e);
    }
    
    public event BreakLockEventHandler OnBreakLock;
    public delegate void BreakLockEventHandler(object sender, EventArgs e); 
    
    public void OnBreakLockEvent(object sender, EventArgs e)
    {
        if (OnBreakLock is null) return;
        
        OnBreakLock(sender, e);
    }
    
    public event RepairLockHandler OnRepairLock;
    public delegate void RepairLockHandler(object sender, EventArgs e); 
    
    public void OnRepairLockEvent(object sender, EventArgs e)
    {
        if (OnRepairLock is null) return;
        
        OnRepairLock(sender, e);
    }
    
    public event RepairLockFailedHandler OnRepairLockFailed;
    public delegate void RepairLockFailedHandler(object sender, EventArgs e); 
    
    public void OnRepairLockFailedEvent(object sender, EventArgs e)
    {
        if (OnRepairLock is null) return;
        
        OnRepairLock(sender, e);
    }
    
    public event HackDoorEventHandler OnHackDoor;
    public delegate void HackDoorEventHandler(object sender, EventArgs e); 
    
    public void OnHackDoorEvent(object sender, EventArgs e)
    {
        if (OnHackDoor is null) return;
        
        OnHackDoor(sender, e);
    }
    
    public event HackDoorFailedEventHandler OnHackDoorFailed;
    public delegate void HackDoorFailedEventHandler(object sender, EventArgs e); 
    
    public void OnHackDoorFailedEvent(object sender, EventArgs e)
    {
        if (OnHackDoorFailed is null) return;
        
        OnHackDoorFailed(sender, e);
    }
    
    #endregion
    */
}