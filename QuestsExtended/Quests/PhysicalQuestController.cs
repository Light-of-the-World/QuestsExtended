using System;
using System.Collections;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using HarmonyLib;
using QuestsExtended.Models;
using UnityEngine;

namespace QuestsExtended.Quests;

internal class PhysicalQuestController
    : AbstractCustomQuestController
{
    private static BasePhysicalClass _physical;
    private static PedometerClass _pedometer;

    private static bool isEcumbered;
    private static bool isEcumberedRunning;
    
    private static bool isOverEncumbered;
    private static bool isOverEncumberedRunning;

    public PhysicalQuestController(QuestExtendedController questExtendedController)
        : base(questExtendedController)
    {
        _physical = Singleton<GameWorld>.Instance.MainPlayer.Physical;
        _pedometer = Singleton<GameWorld>.Instance.MainPlayer.Pedometer;

        _physical.EncumberedChanged += SetEncumbered;
        _physical.OverEncumberedChanged += SetOverEncumbered;

        // Flag for encumbered
        if (_physical.Boolean_0)
        {
            Plugin.Log.LogWarning("Starting Encumbered!");
            isEcumbered = true;
            StaticManager.BeginCoroutine(EncumberedTimer());
        }

        // Flag for over encumbered
        if (_physical.Overweight >= 1f)
        {
            Plugin.Log.LogWarning("Starting Over Encumbered!");
            isOverEncumbered = true;
            StaticManager.BeginCoroutine(OverEncumberedTimer());
        }
    }

    public void OnDestroy()
    {
        _physical.EncumberedChanged -= SetEncumbered;
        _physical.OverEncumberedChanged -= SetOverEncumbered;
    }

    public void Update()
    {
        if (isEcumbered && !isEcumberedRunning)
            StaticManager.BeginCoroutine(EncumberedTimer());
        
        if (isOverEncumbered && !isOverEncumberedRunning)
            StaticManager.BeginCoroutine(OverEncumberedTimer());
    }

    private static void SetEncumbered(bool encumbered)
    {
        isEcumbered = encumbered;

        if (!encumbered) return;

        StaticManager.BeginCoroutine( EncumberedTimer());
    }
    
    private static void SetOverEncumbered(bool encumbered)
    {
        isOverEncumbered = encumbered;
        
        if (!encumbered) return;
        
        StaticManager.BeginCoroutine( OverEncumberedTimer());
    }

    private static IEnumerator EncumberedTimer()
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.EncumberedTimeInSeconds);
        
        if (conditions.Count == 0) yield return null;
        
        while (isEcumbered)
        {
            isEcumberedRunning = true;
            
            yield return new WaitForSeconds(1f);

            foreach (var cond in conditions)
            {
                IncrementCondition(cond, 1f);
            }
        }
        
        isEcumberedRunning = false;
    }
    
    private static IEnumerator OverEncumberedTimer()
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.OverEncumberedTimeInSeconds);

        if (conditions.Count == 0) yield return null;
        
        while (isOverEncumbered)
        {
            isOverEncumberedRunning = true;
            
            yield return new WaitForSeconds(1f);

            foreach (var cond in conditions)
            {
                IncrementCondition(cond, 1f);
            }
        }
        
        isOverEncumberedRunning = false;
    }
}