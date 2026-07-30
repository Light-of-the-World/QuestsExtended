using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Quests;
using HarmonyLib;
using Newtonsoft.Json;
using QuestsExtended.Models;
using SPT.Reflection.Utils;
using UnityEngine;

namespace QuestsExtended.Quests;

internal class PhysicalQuestController : AbstractCustomQuestController
{
    public BasePhysicalClass _physical;
    public Vector3 _playerPos;
    public MovementContext _movementContext;
    public PedometerClass _pedometer;

    public bool isEcumbered;
    public bool isEcumberedRunning;
    public bool isOverEncumbered;
    public bool isOverEncumberedRunning;
    
    //Things added by Light
    //bools (no timers)
    public bool isCrouched = false;
    public bool isProne = false;
    public bool isSilent = false;
    public bool isMounted = false;
    public bool isADS = false;
    public bool isBlindFiring = false;
    public bool isRunning = false;

    //floats
    public float lastX;
    public float lastZ;
    public float movementXPTimer;

    //timers
    public bool PositionCheckDelay;
    public bool MovementXPCooldown = false;

    //float storage (I'm sorry CJ)
    public float _moveAllfloat;
    public float _moveRunfloat;
    public float _moveCrouchedfloat;
    public float _moveProneFloat;
    public float _moveSilentFloat;

    
    //Debug things
    /*
    private static bool MovementDebugTimer;
    private static bool PositionDebugTimer;
    */

    public PhysicalQuestController(QuestExtendedController questExtendedController)
        : base(questExtendedController)
    {
        foreach (var person in Singleton<GameWorld>.Instance.AllAlivePlayersList)
        {
            if (person.IsAI) continue;
            if (person.Side == EPlayerSide.Savage)
            {
                Plugin.Log.LogInfo("No need to attatch QE to a scav raid, aborting.");
                _player = null;
                return;
            }
            if (person.Profile.ProfileId == Plugin.PlayerProfileID)
            {
                _physical = person.Physical;
                _pedometer = person.Pedometer;
                _movementContext = person.MovementContext;
                Plugin.Log.LogInfo($"DEBUG: We have just attatched a PhysicalHealthController to {person.Profile.Nickname}");
                break;
            }
        }
        /*
        _physical = Singleton<GameWorld>.Instance.MainPlayer.Physical;
        _pedometer = Singleton<GameWorld>.Instance.MainPlayer.Pedometer;
        _movementContext = Singleton<GameWorld>.Instance.MainPlayer.MovementContext;
        _playerPos = Singleton<GameWorld>.Instance.MainPlayer.PlayerBody.transform.position;
        if (_playerPos == null) Plugin.Log.LogWarning("Playerpos did not get correctly detected");
        */
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
        //_physical = null;
        //_movementContext = null;
    }

    public void Update()
    {
        if (isRaidOver) return;
        if (_physical == null || _movementContext == null || _playerPos == null) return;
        movementXPTimer -= Time.deltaTime;
        if (isEcumbered && !isEcumberedRunning)
            StaticManager.BeginCoroutine(EncumberedTimer());
        
        if (isOverEncumbered && !isOverEncumberedRunning)
            StaticManager.BeginCoroutine(OverEncumberedTimer());

        /*if (_playerPos != null && !MovementXPCooldown)
            BeginMovementIncrement();*/

        if (movementXPTimer <0)
        {
            movementXPTimer = 5;
            BeginMovementIncrement();
        }
        //Debug below this line
        /*
        if (_movementContext != null && !MovementDebugTimer)
            StaticManager.BeginCoroutine(MovementNumbersDebug());

        if (_pedometer != null && !PositionDebugTimer)
            StaticManager.BeginCoroutine(PositionNumbersDebug());
        */
        isMounted = _movementContext.IsInMountedState;
        isADS = _player.HandsController.IsAiming;
        isRunning = _physical.Sprinting;
    }

    private void SetEncumbered(bool encumbered)
    {
        if (isRaidOver) return;
        isEcumbered = encumbered;

        if (!encumbered) return;

        StaticManager.BeginCoroutine(EncumberedTimer());
    }
    
    private void SetOverEncumbered(bool encumbered)
    {
        if (isRaidOver) return;
        isOverEncumbered = encumbered;
        
        if (!encumbered) return;
        
        StaticManager.BeginCoroutine(OverEncumberedTimer());
    }
    public static string LastPose = "Default";
    private void SetPose(int pose)
    {
        if (isRaidOver) return;
        if (_movementContext.IsInPronePose && isCrouched)
        {
            if (LastPose == "Standing") return;
            //Plugin.Log.LogWarning("Player is entering prone from crouch");
            //ProgressMovementQuests(CalculateDistance(), true, isSilent);
            isProne = true;
            isCrouched = false;
            LastPose = "Standing";
        }
        else if (_movementContext.IsInPronePose && !isCrouched)
        {
            if (LastPose == "Prone") return;
            //Plugin.Log.LogInfo("Player is entering prone from a stand");
            //ProgressMovementQuests(CalculateDistance(), false, isSilent);
            isProne = true;
            LastPose = "Prone";
        }
        else if (isProne)
        {
            isProne = false;
            //Plugin.Log.LogInfo("Player is exiting prone");
            //ProgressMovementQuests(CalculateDistance(), true, isSilent);
        }
        else isProne = false;
        if (_movementContext.PoseLevel <= 0.6 && !isProne && !isCrouched)
        {
            if (LastPose == "Crouched") return;
            //Plugin.Log.LogInfo("Player is entering crouch");
            //ProgressMovementQuests(CalculateDistance(), true, isSilent);
            isCrouched = true;
            LastPose = "Crouched";
        }
        else if (_movementContext.PoseLevel >= 0.6 && !isProne && isCrouched)
        {
            if (LastPose == "Standing") return;
            //Plugin.Log.LogInfo("Player is standing from a crouch");
            //ProgressMovementQuests(CalculateDistance(), false, isSilent);
            isCrouched = false;
            LastPose = "Standing";
        }
        else isCrouched= false;
    }

    private void SetClampedSpeed ()
    {
        if (isRaidOver) return;
        //Plugin.Log.LogWarning("OnCharacterControllerSpeedLimitChanged was triggered");
        if (_movementContext.ClampedSpeed <= 0.3f)
        {
            //Plugin.Log.LogWarning("Player is acheiving covert movement");
            isSilent = true;
            //ProgressMovementQuests(CalculateDistance(), CheckForPose(), true);
            return;
        }
        else { isSilent = false; /*ProgressMovementQuests(CalculateDistance(), CheckForPose(), false);*/ /*Plugin.Log.LogWarning("Player is NO LONGER acheiving covert movement");*/ }
    }
    private IEnumerator EncumberedTimer()
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
    private IEnumerator DistanceTracker()
    {
        MovementXPCooldown = true;
        PositionCheckDelay= true;
        yield return new WaitForSeconds(2f);
        /*
        bool AreWeStanding = false;
        if (!isCrouched && !isProne) AreWeStanding = true;
        */
        //ProgressMovementQuests(CalculateDistance(), AreWeStanding, isSilent);
        ProgressMovementQuests();
        PositionCheckDelay= false;
    }

    private void BeginMovementIncrement()
    {
        //Plugin.Log.LogInfo("Progressing movement");
        ProgressMovementQuests();
    }

    public void ProcessMovement (float num, EPlayerState state)
    {
        _moveAllfloat += num;
        switch (state)
        {
            case EPlayerState.Sprint:
                _moveRunfloat += num;
                break;
            case EPlayerState.Run:
                if (isCrouched) _moveCrouchedfloat += num;
                if(isSilent) _moveSilentFloat += num;
                break;
            case EPlayerState.ProneMove:
                _moveProneFloat += num;
                break;
            default:
                break;
        }
    }

    private int SprintDistance(float distance)
    {
        if (isRaidOver) return 0;
        int distanceToInt = (int)Math.Round(distance, 0);
        if (distanceToInt == 0)
        {
            _moveAllfloat += distance;
            _moveRunfloat += distance;
            if (_moveRunfloat > 1f)
            {
                int floatResult = (int)Math.Round(_moveRunfloat, 0);
                _moveRunfloat = 0;
                return floatResult;
            }
            else if (_moveAllfloat > 1f)
            {
                int floatResult = (int)Math.Round(_moveAllfloat, 0);
                _moveAllfloat = 0;
                return floatResult;
            }
        }
        return distanceToInt;
    }
    /*
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
            //Plugin.Log.LogInfo("Incrementing general movement by" + distance);
            _moveAllfloat += distance;
            if (isCrouched)
            {
                Plugin.Log.LogInfo("Incrementing crouched movement by" + distance);
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
                Plugin.Log.LogInfo("Incrementing prone movement by" + distance);
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
                Plugin.Log.LogInfo("Incrementing silent movement by" + distance);
                _moveSilentFloat += distance;
                if (_moveSilentFloat > 1f)
                {
                    int floatResult = (int)Math.Round(_moveSilentFloat, 0);
                    _moveSilentFloat = 0;
                    return floatResult;
                }
            }
            if (isRunning)
            {
                Plugin.Log.LogInfo("Incrementing run movement by" + distance);
                _moveRunfloat += distance;
                if (_moveRunfloat > 1f)
                {
                    int floatResult = (int)Math.Round(_moveRunfloat, 0);
                    _moveRunfloat = 0;
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
    */
    private bool CheckForPose()
    {
        if (!isCrouched && isProne) return true;
        else return false;
    }
    private static void ProgressMovementQuests()
    {
        foreach (QuestExtendedController QEC in Singleton<GameWorld>.Instance.gameObject.GetComponents<QuestExtendedController>())
        {
            if (QEC.LocalPlayerID == Plugin.PlayerProfileID)
            {
                QEC._physicalController.ProgressMoveAll();
                QEC._physicalController.ProgressMoveCrouched();
                QEC._physicalController.ProgressMoveProne();
                QEC._physicalController.ProgressMoveSilent();
                QEC._physicalController.ProgressMoveSprint();
                break;
            }
        }

    }
    private void ProgressMoveAll()
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionGen.MoveDistance);
        foreach (var cond in conditions)
        {
            //Plugin.Log.LogWarning($"Incrementing condition: {cond} by {distance}");
            IncrementCondition(cond, _moveAllfloat);
        }
        _moveAllfloat = 0;
    }
    private void ProgressMoveCrouched()
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionGen.MoveDistanceWhileCrouched);
        foreach (var cond in conditions)
        {
            //Plugin.Log.LogWarning($"Incrementing condition: {cond} by {distance}");
            IncrementCondition(cond, _moveCrouchedfloat);
        }
        _moveCrouchedfloat = 0;
    }
    private void ProgressMoveProne()
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionGen.MoveDistanceWhileProne);
        foreach (var cond in conditions)
        {
            //Plugin.Log.LogWarning($"Incrementing condition: {cond} by {distance}");
            IncrementCondition(cond, _moveProneFloat);
        }
        _moveProneFloat = 0;
    }
    private void ProgressMoveSilent()
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionGen.MoveDistanceWhileSilent);
        foreach (var cond in conditions)
        {
            //Plugin.Log.LogWarning($"Incrementing condition: {cond} by {distance}");
            IncrementCondition(cond, _moveSilentFloat);
        }
        _moveSilentFloat = 0;
    }
    private void ProgressMoveSprint()
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionGen.MoveDistanceWhileRunning);
        foreach (var cond in conditions)
        {
            //Plugin.Log.LogWarning($"Incrementing condition: {cond} by {distance}");
            IncrementCondition(cond, _moveRunfloat);
        }
        _moveRunfloat = 0;
    }
    private void ProgressMovementQuests(int distance, bool Standing, bool Silent)
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
                Plugin.Log.LogInfo("You are prone");
                conditionsToCheck |= EQuestConditionGen.MoveDistanceWhileProne;
            }
            else
            {
                //Plugin.Log.LogWarning($"Unknown pose: {_movementContext.PoseLevel}");
                return;
            }
        }
        if (isRunning) conditionsToCheck |= EQuestConditionGen.MoveDistanceWhileRunning;

        if (Silent) conditionsToCheck |= EQuestConditionGen.MoveDistanceWhileSilent;

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
    }
    private IEnumerator OverEncumberedTimer()
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