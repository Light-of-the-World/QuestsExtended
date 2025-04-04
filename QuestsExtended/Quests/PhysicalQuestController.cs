using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    private static Vector3 _pedometer;
    private static MovementContext _movementContext;

    private static bool isEcumbered;
    private static bool isEcumberedRunning;
    
    private static bool isOverEncumbered;
    private static bool isOverEncumberedRunning;
    
    //Things added by Light
    //bools (no timers)
    private static bool isCrouched;
    private static bool isProne;
    private static bool isSilent;

    //floats
    private static float lastx;
    private static float lastz;
    private static float currentx;
    private static float currentz;

    //timers
    private static bool MovementTimer;

    //Debug things
    /*
    private static bool MovementDebugTimer;
    private static bool PositionDebugTimer;
    */

    public PhysicalQuestController(QuestExtendedController questExtendedController)
        : base(questExtendedController)
    {
        _physical = Singleton<GameWorld>.Instance.MainPlayer.Physical;
        //_pedometer = Singleton<GameWorld>.Instance.MainPlayer.Pedometer;
        _movementContext = Singleton<GameWorld>.Instance.MainPlayer.MovementContext;
        _pedometer = Singleton<GameWorld>.Instance.MainPlayer.PlayerBody.transform.position;

        _physical.EncumberedChanged += SetEncumbered;
        _physical.OverEncumberedChanged += SetOverEncumbered;
        _movementContext.OnPoseChanged += SetPose;
        _movementContext.OnCharacterControllerSpeedLimitChanged += SetClampedSpeed;

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
        _movementContext.OnPoseChanged -= SetPose;
        _movementContext.OnCharacterControllerSpeedLimitChanged -= SetClampedSpeed;
    }

    public void Update()
    {
        if (isEcumbered && !isEcumberedRunning)
            StaticManager.BeginCoroutine(EncumberedTimer());
        
        if (isOverEncumbered && !isOverEncumberedRunning)
            StaticManager.BeginCoroutine(OverEncumberedTimer());

        if (_pedometer != null && !MovementTimer)
            StaticManager.BeginCoroutine(DistanceTracker());

        //Debug below this line
        /*
        if (_movementContext != null && !MovementDebugTimer)
            StaticManager.BeginCoroutine(MovementNumbersDebug());

        if (_pedometer != null && !PositionDebugTimer)
            StaticManager.BeginCoroutine(PositionNumbersDebug());
        */
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

    private static void SetPose(int pose)
    {
        if (_movementContext.IsInPronePose)
        {
            Plugin.Log.LogWarning("Player is prone");
            isProne = true;
        }
        else isProne = false;
        if (_movementContext.PoseLevel <= 0.6 && !isProne)
        {
            Plugin.Log.LogWarning("Player is crouched");
            isCrouched = true;
        }
        else isCrouched= false;
    }

    private static void SetClampedSpeed ()
    {
        Plugin.Log.LogWarning("OnCharacterControllerSpeedLimitChanged was triggered");
        if (_movementContext.ClampedSpeed <= 0.3f)
        {
            Plugin.Log.LogWarning("Player is acheiving covert movement");
            isSilent = true;
        }
        else isSilent = false;
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
    /*
    private static IEnumerator MovementNumbersDebug()
    {
        MovementDebugTimer = true;
            yield return new WaitForSeconds(5f);
            Plugin.Log.LogWarning("Clamped speed is " + _movementContext.ClampedSpeed + ". IsInPronePose = " + _movementContext.IsInPronePose + ". PoseLevel is " + _movementContext.PoseLevel + ".");
        MovementDebugTimer = false;
    }
    */
    /*
    private static IEnumerator PositionNumbersDebug()
    {
        PositionDebugTimer = true;
        yield return new WaitForSeconds(5f);
        lastx = _pedometer.x;
        lastz = _pedometer.z;
        _pedometer = Singleton<GameWorld>.Instance.MainPlayer.PlayerBody.transform.position;
        Plugin.Log.LogWarning("Current position is " + _pedometer.ToString());
        float num1 = Math.Abs(Math.Abs(_pedometer.x) - Math.Abs(lastx));
        float num2 = Math.Abs(Math.Abs(_pedometer.z) - Math.Abs(lastz));
        float num3 = num1 + num2;
        Plugin.Log.LogWarning("Distance between last marked pos and current pos is " + num3  + ".");
        PositionDebugTimer = false;
    }
    */
    private static IEnumerator DistanceTracker()
    {
        MovementTimer = true;
        yield return new WaitForSeconds(5f);
        lastx = _pedometer.x;
        lastz = _pedometer.z;
        _pedometer = Singleton<GameWorld>.Instance.MainPlayer.PlayerBody.transform.position;
        Plugin.Log.LogWarning("Current position is " + _pedometer.ToString());
        float num1 = Math.Abs(Math.Abs(_pedometer.x) - Math.Abs(lastx));
        float num2 = Math.Abs(Math.Abs(_pedometer.z) - Math.Abs(lastz));
        float num3 = num1 + num2;
        Plugin.Log.LogWarning("Distance between last marked pos and current pos is " + num3 + ".");
        MovementTimer = false;
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