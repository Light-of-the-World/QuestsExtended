using EFT;
using EFT.HealthSystem;
using QuestsExtended.Models;
using QuestsExtended.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuestsExtended.Quests;

internal class MedicalQuestController 
    : AbstractCustomQuestController
{
    public MedicalQuestController(QuestExtendedController questExtendedController)
        : base(questExtendedController)
    {
        _player.ActiveHealthController.EffectRemovedEvent += HandleRemoveHealthCondition;
        _player.ActiveHealthController.HealthChangedEvent += HandleHealthChange;
        _player.ActiveHealthController.BodyPartDestroyedEvent += HandleBodyPartDestroyed;
        _player.ActiveHealthController.BodyPartRestoredEvent += HandleBodyPartRestored;
    }
    
    public void OnDestroy()
    {
        _player.ActiveHealthController.EffectRemovedEvent -= HandleRemoveHealthCondition;
        _player.ActiveHealthController.HealthChangedEvent -= HandleHealthChange;
        _player.ActiveHealthController.BodyPartDestroyedEvent -= HandleBodyPartDestroyed;
        _player.ActiveHealthController.BodyPartRestoredEvent -= HandleBodyPartRestored;
    }
    
    private void HandleRemoveHealthCondition(IEffect effect)
    {
        if (RE.FractureType.IsInstanceOfType(effect))
        {
            HandleRemoveFracture(effect);
            return;
        }
        
        if (RE.LightBleedType.IsInstanceOfType(effect))
        {
            HandleRemoveLightBleed(effect);
            return;
        }
        
        if (RE.HeavyBleedType.IsInstanceOfType(effect))
        {
            HandleRemoveHeavyBleed(effect);
            return;
        }
    }

    private void HandleHealthChange(EBodyPart bodyPart, float change, DamageInfoStruct damage)
    {
        if (change.Positive())
        {
            HandleHealthGain(bodyPart, change, damage);
        }
        
        if (change.Negative())
        {
            HandleHealthLoss(bodyPart, change, damage);
        }
    }
    
    private void HandleRemoveFracture(IEffect effect)
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionHealth.FixFracture);
        
        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, effect.BodyPart)) 
                IncrementCondition(condition, 1f);
        }
    }
    
    private void HandleRemoveLightBleed(IEffect effect)
    {
        EQuestConditionHealth conditionsToCheck = EQuestConditionHealth.FixLightBleed;
        conditionsToCheck |= EQuestConditionHealth.FixAnyBleed;
        var conditions = _questController.GetActiveConditions(conditionsToCheck);
        
        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, effect.BodyPart)) 
                IncrementCondition(condition, 1f);
        }
    }
    
    private void HandleRemoveHeavyBleed(IEffect effect)
    {
        EQuestConditionHealth conditionsToCheck = EQuestConditionHealth.FixHeavyBleed;
        conditionsToCheck |= EQuestConditionHealth.FixAnyBleed;
        var conditions = _questController.GetActiveConditions(conditionsToCheck);

        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, effect.BodyPart)) 
                IncrementCondition(condition, 1f);
        }
    }

    private void HandleHealthLoss(EBodyPart bodyPart, float change, DamageInfoStruct damage)
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionHealth.HealthLoss);
        int intChange = (int)Math.Round(change, 0);
        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, bodyPart)) 
                IncrementCondition(condition, Math.Abs(intChange));
        }
    }

    private void HandleHealthGain(EBodyPart bodyPart, float change, DamageInfoStruct damage)
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionHealth.HealthGain);
        int intChange = (int)Math.Round(change, 0);

        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, bodyPart)) 
                IncrementCondition(condition, intChange);
        }
    }
    
    private void HandleBodyPartDestroyed(EBodyPart bodyPart, EDamageType damageType)
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionHealth.DestroyBodyPart);
        
        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, bodyPart)) 
                IncrementCondition(condition, 1f);
        }
    }

    private void HandleBodyPartRestored(EBodyPart bodyPart, ValueStruct value)
    {
        var conditions = _questController.GetActiveConditions(EQuestConditionHealth.RestoreBodyPart);
        
        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, bodyPart)) 
                IncrementCondition(condition, 1f);
        }
    }
}