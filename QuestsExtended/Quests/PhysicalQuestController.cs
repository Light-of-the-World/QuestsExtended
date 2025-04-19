using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Quests;
using HarmonyLib;
using QuestsExtended.Models;
using UnityEngine;

namespace QuestsExtended.Quests;

internal class PhysicalQuestController
    : AbstractCustomQuestController
{
    private static BasePhysicalClass _physical;
    private static Vector3 _playerPos;
    public static MovementContext _movementContext;

    private static bool isEcumbered;
    private static bool isEcumberedRunning;
    
    private static bool isOverEncumbered;
    private static bool isOverEncumberedRunning;
    
    //Things added by Light
    //bools (no timers)
    public static bool isCrouched;
    public static bool isProne;
    public static bool isSilent;
    public static bool isMounted;
    public static bool isRaidOver = false;

    //floats
    private static float lastX;
    private static float lastZ;

    //timers
    private static bool PositionCheckDelay;
    private static bool MovementXPCooldown;

    //float storage (I'm sorry CJ)
    private static float _moveAllfloat;
    private static float _moveCrouchedfloat;
    private static float _moveProneFloat;
    private static float _moveSilentFloat;
    
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
        _playerPos = Singleton<GameWorld>.Instance.MainPlayer.PlayerBody.transform.position;

        _physical.EncumberedChanged += SetEncumbered;
        _physical.OverEncumberedChanged += SetOverEncumbered;
        _movementContext.OnPoseChanged += SetPose;
        _movementContext.OnCharacterControllerSpeedLimitChanged += SetClampedSpeed;
        _player.OnPlayerDeadOrUnspawn += RaidOver;

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
        _player.OnPlayerDeadOrUnspawn -= RaidOver;
    }

    public void Update()
    {
        if (isRaidOver) return;
        if (isEcumbered && !isEcumberedRunning)
            StaticManager.BeginCoroutine(EncumberedTimer());
        
        if (isOverEncumbered && !isOverEncumberedRunning)
            StaticManager.BeginCoroutine(OverEncumberedTimer());

        if (_playerPos != null && !PositionCheckDelay)
            StaticManager.BeginCoroutine(DistanceTracker());

        //Debug below this line
        /*
        if (_movementContext != null && !MovementDebugTimer)
            StaticManager.BeginCoroutine(MovementNumbersDebug());

        if (_pedometer != null && !PositionDebugTimer)
            StaticManager.BeginCoroutine(PositionNumbersDebug());
        */
        isMounted = _movementContext.IsInMountedState;
    }

    private static void SetEncumbered(bool encumbered)
    {
        if (isRaidOver) return;
        isEcumbered = encumbered;

        if (!encumbered) return;

        StaticManager.BeginCoroutine( EncumberedTimer());
    }
    
    private static void SetOverEncumbered(bool encumbered)
    {
        if (isRaidOver) return;
        isOverEncumbered = encumbered;
        
        if (!encumbered) return;
        
        StaticManager.BeginCoroutine( OverEncumberedTimer());
    }
    public static string LastPose = "Default";
    private static void SetPose(int pose)
    {
        if (isRaidOver) return;
        if (_movementContext.IsInPronePose && isCrouched)
        {
            if (LastPose == "Standing") return;
            //Plugin.Log.LogWarning("Player is entering prone from crouch");
            ProgressMovementQuests(CalculateDistance(), true, isSilent);
            isProne = true;
            isCrouched = false;
            LastPose = "Standing";
        }
        else if (_movementContext.IsInPronePose && !isCrouched)
        {
            if (LastPose == "Prone") return;
            //Plugin.Log.LogInfo("Player is entering prone from a stand");
            ProgressMovementQuests(CalculateDistance(), false, isSilent);
            isProne = true;
            LastPose = "Prone";
        }
        else if (isProne)
        {
            isProne = false;
            //Plugin.Log.LogInfo("Player is exiting prone");
            ProgressMovementQuests(CalculateDistance(), true, isSilent);
        }
        else isProne = false;
        if (_movementContext.PoseLevel <= 0.6 && !isProne && !isCrouched)
        {
            if (LastPose == "Crouched") return;
            //Plugin.Log.LogInfo("Player is entering crouch");
            ProgressMovementQuests(CalculateDistance(), true, isSilent);
            isCrouched = true;
            LastPose = "Crouched";
        }
        else if (_movementContext.PoseLevel >= 0.6 && !isProne && isCrouched)
        {
            if (LastPose == "Standing") return;
            //Plugin.Log.LogInfo("Player is standing from a crouch");
            ProgressMovementQuests(CalculateDistance(), false, isSilent);
            isCrouched = false;
            LastPose = "Standing";
        }
        else isCrouched= false;
    }

    private static void SetClampedSpeed ()
    {
        if (isRaidOver) return;
        //Plugin.Log.LogWarning("OnCharacterControllerSpeedLimitChanged was triggered");
        if (_movementContext.ClampedSpeed <= 0.3f)
        {
            //Plugin.Log.LogWarning("Player is acheiving covert movement");
            isSilent = true;
            ProgressMovementQuests(CalculateDistance(), CheckForPose(), true);
            return;
        }
        else { isSilent = false; ProgressMovementQuests(CalculateDistance(), CheckForPose(), false); /*Plugin.Log.LogWarning("Player is NO LONGER acheiving covert movement");*/ }
    }
    private static IEnumerator EncumberedTimer()
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionGen.EncumberedTimeInSeconds);
        
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
    private static IEnumerator DistanceTracker()
    {
        PositionCheckDelay= true;
        yield return new WaitForSeconds(2f);
        bool AreWeStanding = false;
        if (!isCrouched && !isProne) AreWeStanding = true;
        ProgressMovementQuests(CalculateDistance(), AreWeStanding, isSilent);
        PositionCheckDelay= false;
    }

    private static int CalculateDistance()
    {
        if (isRaidOver) return 0;
        lastX = _playerPos.x;
        lastZ = _playerPos.z;
        _playerPos = Singleton<GameWorld>.Instance.MainPlayer.PlayerBody.transform.position;
        //Plugin.Log.LogWarning("Current position is " + _pedometer.ToString());
        float newX = Math.Abs(Math.Abs(_playerPos.x) - Math.Abs(lastX));
        float newZ = Math.Abs(Math.Abs(_playerPos.z) - Math.Abs(lastZ));
        float distance = newX + newZ;
        int distanceToInt = (int)Math.Round(distance, 0);
        if (distanceToInt == 0)
        {
            _moveAllfloat += distance;
            if (isCrouched)
            {
                _moveCrouchedfloat += distance;
                if (_moveCrouchedfloat > 1f)
                {
                    int floatResult = (int)Math.Round(_moveCrouchedfloat, 0);
                    _moveCrouchedfloat = 0;
                    return floatResult;
                }
            }
            else if (isProne)
            {
                _moveProneFloat += distance;
                if (_moveProneFloat > 1f)
                {
                    int floatResult = (int)Math.Round(_moveProneFloat, 0);
                    _moveProneFloat = 0;
                    return floatResult;
                }
            }
            if (isSilent)
            {
                _moveSilentFloat += distance;
                if (_moveSilentFloat > 1f)
                {
                    int floatResult = (int)Math.Round(_moveSilentFloat, 0);
                    return floatResult;
                }
            }
            if (_moveAllfloat > 1f)
            {
                int floatResult = (int)Math.Round(_moveAllfloat, 0);
                _moveAllfloat = 0;
                return floatResult;
            }
        }
        //Plugin.Log.LogWarning("Rounded distance between last marked pos and current pos is " + num4 + ".");
        return distanceToInt;
    }

    private static bool CheckForPose()
    {
        if (!isCrouched && isProne) return true;
        else return false;
    }
    private static void ProgressMovementQuests(int distance, bool Standing, bool Silent)
    {
        if (isRaidOver) return;
        if (MovementXPCooldown) return;
        // Always include MoveDistance
        EQuestConditionGen conditionsToCheck = EQuestConditionGen.MoveDistance;

        if (!Standing)
        {
            if (isCrouched)
            {
                conditionsToCheck |= EQuestConditionGen.MoveDistanceWhileCrouched;
            }
            else if (isProne)
            {
                conditionsToCheck |= EQuestConditionGen.MoveDistanceWhileProne;
            }
            else
            {
                //Plugin.Log.LogWarning($"Unknown pose: {_movementContext.PoseLevel}");
                return;
            }
        }

        if (Silent)
        {
            conditionsToCheck |= EQuestConditionGen.MoveDistanceWhileSilent;
        }

        // Retrieve active conditions just once
        var conditions = _questController.GetActiveConditions(conditionsToCheck);
        /*
        Plugin.Log.LogWarning($"Conditions: {conditionsToCheck} ({(EQuestCondition)conditionsToCheck}).");
        Plugin.Log.LogWarning($"Matching conditions found: {string.Join(", ", conditions)}");
        */
        foreach (var cond in conditions)
        {
            //Plugin.Log.LogWarning($"Incrementing condition: {cond} by {distance}");
            IncrementCondition(cond, distance);
        }
        StaticManager.BeginCoroutine(MovementCooldown());
    }
    private static IEnumerator OverEncumberedTimer()
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionGen.OverEncumberedTimeInSeconds);

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

    private static IEnumerator MovementCooldown()
    {
        MovementXPCooldown = true;
        yield return new WaitForSeconds(0.15f);
        MovementXPCooldown = false;
    }
    private static void RaidOver(Player player)
    {
        isRaidOver = true;
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
}